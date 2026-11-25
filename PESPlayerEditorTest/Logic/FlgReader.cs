using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Representa el color de una entrada de paleta (256 colores RGB de 3 bytes cada
    /// uno) usada por los archivos .flg para decodificar sus imágenes indexadas.
    /// </summary>
    public readonly struct FlgColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public FlgColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    /// <summary>
    /// Representa una entrada (un emblema/escudo) dentro de una de las secciones de
    /// datos de un archivo .flg, ya con su índice original y bytes descomprimidos
    /// (valores de índice de paleta, uno por píxel).
    /// </summary>
    public class FlgImageEntry
    {
        public int Index;
        public int CompressedSize;
        public uint RelativeOffset;
        public uint AbsoluteOffset;
        public byte[] DecompressedPixelIndices = Array.Empty<byte>();
    }

    /// <summary>
    /// Lector del formato .flg usado por flag_160f.flg (emblemas 32x32) y
    /// flag_4030.flg (emblemas 64x64) dentro de FLAG.afs. Contiene los emblemas de
    /// los equipos, en el mismo orden en que aparecen en la lista de equipos.
    ///
    /// ESTRUCTURA (deducida empíricamente a partir de un volcado hexadecimal de
    /// flag_160f.flg; PENDIENTE DE VALIDACIÓN completa contra el archivo real -
    /// usar <see cref="GetRawFileInfo"/> para confirmar los números antes de
    /// confiar en la extracción final):
    ///
    /// - 0x00: u32 Field00 (sin identificar; en el dump de muestra = 1280)
    /// - 0x04: u32 Field04 (sin identificar; en el dump de muestra = 50800,
    ///          cercano al final de los datos comprimidos: posible tamaño total)
    /// - 0x08: u32 reservado/0
    /// - 0x0C: 4 bytes sueltos (posible versión; en el dump = 01 14 04 04)
    /// - 0x10..0x1F: 16 bytes en 0 (padding)
    /// - 0x20: u32 EntryCountTotal (en el dump = 330 = suma aproximada de las
    ///          3 secciones; pendiente de confirmar fórmula exacta)
    /// - 0x24: u16 + u16 sin identificar (en el dump = 0x0F16, 0x1020 - estos
    ///          valores recuerdan al propio nombre "flag_160f"/"4030"; posible
    ///          "firma"/ID del archivo en vez de datos de tabla)
    /// - 0x28: u32 = 12 (sin identificar)
    /// - 0x2C: inicio de 3 descriptores de sección de 12 bytes cada uno:
    ///         u32 EntryCount, u32 SectionDataOffset (absoluto), u32 Unknown
    ///         (en el dump de muestra: sección 1 => count=239, offset=47728;
    ///          sección 2 => count=103, offset=48752 (47728+1024);
    ///          sección 3 => count=12,  offset=49776 (48752+1024))
    /// - Justo después del tercer descriptor (offset 0x50 en el dump de
    ///   muestra) empieza la TABLA de entradas de la primera sección: una
    ///   entrada de 8 bytes por emblema:
    ///         u16 Index, u16 CompressedSize, u32 RelativeOffset
    ///   El offset de cada entrada es RELATIVO al inicio de la sección de
    ///   datos correspondiente (ej. SectionDataOffset de arriba), no absoluto
    ///   desde el inicio del archivo.
    /// - Tras agotar las entradas de la sección 1 (EntryCount1 registros),
    ///   sigue inmediatamente la tabla de la sección 2, y luego la de la
    ///   sección 3 (sin padding detectado entre tablas en el dump analizado).
    /// - Cada entrada, una vez localizada su posición real
    ///   (SectionDataOffset + RelativeOffset), contiene un stream zlib (firma
    ///   "78 DA" u otra cabecera RFC1950 válida) de exactamente CompressedSize
    ///   bytes. Al descomprimir se obtienen los índices de paleta (1 byte por
    ///   píxel) de la imagen: 32*32=1024 bytes para flag_160f.flg, 64*64=4096
    ///   para flag_4030.flg.
    /// - Al final del archivo (tras todas las secciones de datos) aparece la
    ///   paleta de 256 colores (768 bytes = 256 * 3 bytes RGB), posiblemente
    ///   duplicada/rellenada con bytes de relleno adicionales.
    /// </summary>
    public static class FlgReader
    {
        private const int SectionDescriptorStart = 0x2C;
        private const int SectionDescriptorSize = 12;
        private const int SectionDescriptorCount = 3;
        private const int TableEntrySize = 8;
        public const int PaletteColorCount = 256;
        public const int PaletteSizeBytes = PaletteColorCount * 3;

        private class SectionDescriptor
        {
            public int EntryCount;
            public uint DataOffset;
            public uint Unknown;
        }

        /// <summary>
        /// Parsea los 3 descriptores de sección del header y, a continuación, las
        /// tablas de entradas consecutivas (una tabla por sección, en el mismo
        /// orden que los descriptores).
        /// </summary>
        private static List<FlgImageEntry> ParseEntries(byte[] raw, out List<string> warnings)
        {
            warnings = new List<string>();
            var allEntries = new List<FlgImageEntry>();

            var descriptors = new List<SectionDescriptor>();
            for (int s = 0; s < SectionDescriptorCount; s++)
            {
                int descOffset = SectionDescriptorStart + (s * SectionDescriptorSize);
                if (descOffset + SectionDescriptorSize > raw.Length)
                {
                    warnings.Add($"Header truncado: no se pudo leer el descriptor de sección {s}.");
                    break;
                }

                descriptors.Add(new SectionDescriptor
                {
                    EntryCount = BitConverter.ToInt32(raw, descOffset),
                    DataOffset = BitConverter.ToUInt32(raw, descOffset + 4),
                    Unknown = BitConverter.ToUInt32(raw, descOffset + 8)
                });
            }

            int tableCursor = SectionDescriptorStart + (SectionDescriptorCount * SectionDescriptorSize);
            int globalIndex = 0;

            foreach (var descriptor in descriptors)
            {
                if (descriptor.EntryCount <= 0 || descriptor.EntryCount > 100000)
                {
                    warnings.Add($"Número de entradas de sección sospechoso ({descriptor.EntryCount}); se omite esta sección.");
                    continue;
                }

                for (int i = 0; i < descriptor.EntryCount; i++)
                {
                    if (tableCursor + TableEntrySize > raw.Length)
                    {
                        warnings.Add("Tabla de entradas truncada antes de terminar de leer todas las secciones.");
                        return allEntries;
                    }

                    ushort entryIndex = BitConverter.ToUInt16(raw, tableCursor);
                    ushort compressedSize = BitConverter.ToUInt16(raw, tableCursor + 2);
                    uint relativeOffset = BitConverter.ToUInt32(raw, tableCursor + 4);

                    allEntries.Add(new FlgImageEntry
                    {
                        Index = globalIndex++,
                        CompressedSize = compressedSize,
                        RelativeOffset = relativeOffset,
                        AbsoluteOffset = descriptor.DataOffset + relativeOffset
                    });

                    tableCursor += TableEntrySize;
                }
            }

            return allEntries;
        }

        /// <summary>
        /// Intenta descomprimir, con el tamaño comprimido EXACTO dado por la tabla
        /// (a diferencia de para_we8.bin, aquí el tamaño no hay que adivinarlo),
        /// el stream zlib ubicado en <paramref name="absoluteOffset"/>.
        /// </summary>
        private static byte[]? TryDecompressEntry(byte[] raw, uint absoluteOffset, int compressedSize)
        {
            if (compressedSize <= 0 || absoluteOffset + compressedSize > raw.Length)
            {
                return null;
            }

            try
            {
                using var compStream = new MemoryStream(raw, (int)absoluteOffset, compressedSize, writable: false);
                using var zlibStream = new ZLibStream(compStream, CompressionMode.Decompress, leaveOpen: true);
                using var outMs = new MemoryStream();
                zlibStream.CopyTo(outMs);
                return outMs.ToArray();
            }
            catch (InvalidDataException)
            {
                return null;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        /// <summary>
        /// Carga todas las entradas de imagen del archivo .flg indicado,
        /// descomprimiendo cada una (índices de paleta, 1 byte por píxel).
        /// </summary>
        public static List<FlgImageEntry> LoadEntries(string flgPath, out List<string> warnings)
        {
            byte[] raw = File.ReadAllBytes(flgPath);
            var entries = ParseEntries(raw, out warnings);

            foreach (var entry in entries)
            {
                byte[]? decompressed = TryDecompressEntry(raw, entry.AbsoluteOffset, entry.CompressedSize);
                if (decompressed == null)
                {
                    warnings.Add($"No se pudo descomprimir la entrada #{entry.Index} (offset=0x{entry.AbsoluteOffset:X}, size={entry.CompressedSize}).");
                    entry.DecompressedPixelIndices = Array.Empty<byte>();
                }
                else
                {
                    entry.DecompressedPixelIndices = decompressed;
                }
            }

            return entries;
        }

        /// <summary>
        /// Busca y extrae la paleta de 256 colores RGB (768 bytes) del archivo.
        /// Dado que en el volcado analizado la paleta aparece más de una vez
        /// (posible duplicado/relleno), se busca la PRIMERA aparición cuyo
        /// contenido luzca como una paleta válida (heurística: suficiente
        /// variación de bytes, no todo ceros) a partir del final de la última
        /// sección de datos.
        /// </summary>
        public static List<FlgColor> ParsePalette(string flgPath)
        {
            byte[] raw = File.ReadAllBytes(flgPath);
            return ParsePaletteFromBytes(raw);
        }

        private static List<FlgColor> ParsePaletteFromBytes(byte[] raw)
        {
            var warnings = new List<string>();
            var entries = ParseEntries(raw, out warnings);

            // La paleta se busca a partir de justo después de la última entrada de
            // datos conocida (offset absoluto + tamaño comprimido más alto visto).
            int searchStart = 0;
            foreach (var entry in entries)
            {
                int entryEnd = (int)entry.AbsoluteOffset + entry.CompressedSize;
                if (entryEnd > searchStart)
                {
                    searchStart = entryEnd;
                }
            }

            // Redondear hacia arriba a un límite razonable y buscar el primer bloque
            // de 768 bytes con suficiente variación como para ser una paleta real.
            for (int offset = searchStart; offset + PaletteSizeBytes <= raw.Length; offset++)
            {
                if (LooksLikePalette(raw, offset))
                {
                    return ExtractPalette(raw, offset);
                }
            }

            // Fallback: si no se encontró nada tras los datos, probar desde el final
            // del archivo hacia atrás (por si la paleta está al final, como sugiere
            // el volcado proporcionado).
            for (int offset = raw.Length - PaletteSizeBytes; offset >= 0; offset--)
            {
                if (LooksLikePalette(raw, offset))
                {
                    return ExtractPalette(raw, offset);
                }
            }

            return new List<FlgColor>();
        }

        private static bool LooksLikePalette(byte[] raw, int offset)
        {
            // Heurística simple: dentro del bloque de 768 bytes debe haber
            // suficiente variedad (no todo ceros, no todo el mismo valor) y el
            // primer color no debe ser puro negro consecutivo con muchos otros
            // (lo cual indicaría que caímos en una zona de padding).
            int distinctCount = 0;
            byte first = raw[offset];
            bool allSame = true;

            var seen = new HashSet<byte>();
            for (int i = 0; i < PaletteSizeBytes; i++)
            {
                byte b = raw[offset + i];
                seen.Add(b);
                if (b != first)
                {
                    allSame = false;
                }
            }
            distinctCount = seen.Count;

            return !allSame && distinctCount > 20;
        }

        private static List<FlgColor> ExtractPalette(byte[] raw, int offset)
        {
            var palette = new List<FlgColor>(PaletteColorCount);
            for (int i = 0; i < PaletteColorCount; i++)
            {
                int colorOffset = offset + (i * 3);
                palette.Add(new FlgColor(raw[colorOffset], raw[colorOffset + 1], raw[colorOffset + 2]));
            }
            return palette;
        }

        /// <summary>
        /// Convierte los índices de paleta descomprimidos de una entrada en bytes
        /// BGRA32 (4 bytes por píxel), listos para construir un WriteableBitmap o
        /// guardarse como imagen. <paramref name="width"/> y <paramref name="height"/>
        /// deben coincidir con el tamaño esperado (32x32 para flag_160f.flg, 64x64
        /// para flag_4030.flg); si los índices no alcanzan para completar la
        /// imagen, los píxeles restantes se dejan transparentes.
        /// </summary>
        public static byte[] ToBgra32(FlgImageEntry entry, List<FlgColor> palette, int width, int height)
        {
            byte[] bgra = new byte[width * height * 4];
            int pixelCount = width * height;

            for (int p = 0; p < pixelCount; p++)
            {
                int destOffset = p * 4;

                if (p >= entry.DecompressedPixelIndices.Length)
                {
                    // Sin datos suficientes: píxel transparente.
                    continue;
                }

                byte paletteIndex = entry.DecompressedPixelIndices[p];
                if (paletteIndex >= palette.Count)
                {
                    continue;
                }

                FlgColor color = palette[paletteIndex];
                bgra[destOffset] = color.B;
                bgra[destOffset + 1] = color.G;
                bgra[destOffset + 2] = color.R;
                bgra[destOffset + 3] = paletteIndex == 0 ? (byte)0 : (byte)255; // índice 0 = transparente (heurística habitual en estos formatos)
            }

            return bgra;
        }

        /// <summary>
        /// Diagnóstico: vuelca a texto, para inspección manual, los descriptores de
        /// sección, el total de entradas parseadas (con sus offsets/tamaños) y si se
        /// pudo localizar una paleta válida. ÚSESE ESTE MÉTODO PRIMERO contra un
        /// archivo .flg real para confirmar que la estructura deducida es correcta
        /// antes de integrar la extracción en la UI final.
        /// </summary>
        public static string GetRawFileInfo(string flgPath, int maxEntriesToDump = 20)
        {
            byte[] raw = File.ReadAllBytes(flgPath);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Archivo: {flgPath}");
            sb.AppendLine($"Tamaño total: {raw.Length} bytes");
            sb.AppendLine();

            sb.AppendLine("--- Header (primeros 0x50 bytes) ---");
            sb.AppendLine($"0x00 Field00            = {BitConverter.ToUInt32(raw, 0x00)}");
            sb.AppendLine($"0x04 Field04            = {BitConverter.ToUInt32(raw, 0x04)}");
            sb.AppendLine($"0x08 reservado          = {BitConverter.ToUInt32(raw, 0x08)}");
            sb.AppendLine($"0x0C versión(?)         = {raw[0x0C]:X2} {raw[0x0D]:X2} {raw[0x0E]:X2} {raw[0x0F]:X2}");
            sb.AppendLine($"0x20 EntryCountTotal(?) = {BitConverter.ToUInt32(raw, 0x20)}");
            sb.AppendLine($"0x24 u16+u16 sin ident. = 0x{BitConverter.ToUInt16(raw, 0x24):X4} 0x{BitConverter.ToUInt16(raw, 0x26):X4}");
            sb.AppendLine($"0x28 valor sin ident.   = {BitConverter.ToUInt32(raw, 0x28)}");
            sb.AppendLine();

            for (int s = 0; s < SectionDescriptorCount; s++)
            {
                int descOffset = SectionDescriptorStart + (s * SectionDescriptorSize);
                if (descOffset + SectionDescriptorSize > raw.Length)
                {
                    sb.AppendLine($"Descriptor de sección {s}: TRUNCADO (fuera de rango)");
                    continue;
                }

                int count = BitConverter.ToInt32(raw, descOffset);
                uint dataOffset = BitConverter.ToUInt32(raw, descOffset + 4);
                uint unknown = BitConverter.ToUInt32(raw, descOffset + 8);
                sb.AppendLine($"Descriptor de sección {s}: EntryCount={count}, DataOffset=0x{dataOffset:X} ({dataOffset}), Unknown={unknown}");
            }
            sb.AppendLine();

            var entries = ParseEntries(raw, out var warnings);
            sb.AppendLine($"Total de entradas parseadas: {entries.Count}");
            sb.AppendLine();

            sb.AppendLine($"--- Primeras {Math.Min(maxEntriesToDump, entries.Count)} entradas ---");
            for (int i = 0; i < Math.Min(maxEntriesToDump, entries.Count); i++)
            {
                var e = entries[i];
                bool hasZlibSignature = e.AbsoluteOffset + 1 < raw.Length &&
                                         raw[e.AbsoluteOffset] == 0x78;
                sb.AppendLine(
                    $"[{e.Index,4}] relOffset=0x{e.RelativeOffset:X6} absOffset=0x{e.AbsoluteOffset:X6} " +
                    $"compressedSize={e.CompressedSize,5} firmaZlib={(hasZlibSignature ? $"78 {raw[e.AbsoluteOffset + 1]:X2}" : "NO")}");
            }
            sb.AppendLine();

            if (warnings.Count > 0)
            {
                sb.AppendLine("--- Avisos durante el parseo ---");
                foreach (var w in warnings)
                {
                    sb.AppendLine(w);
                }
                sb.AppendLine();
            }

            sb.AppendLine("--- Prueba de descompresión de las primeras entradas ---");
            int testCount = Math.Min(5, entries.Count);
            for (int i = 0; i < testCount; i++)
            {
                var e = entries[i];
                byte[]? decompressed = TryDecompressEntry(raw, e.AbsoluteOffset, e.CompressedSize);
                if (decompressed == null)
                {
                    sb.AppendLine($"[{e.Index}] Descompresión FALLIDA.");
                }
                else
                {
                    sb.AppendLine($"[{e.Index}] Descompresión OK: {decompressed.Length} bytes (esperado 1024 para 32x32 u 4096 para 64x64).");
                }
            }
            sb.AppendLine();

            sb.AppendLine("--- Búsqueda de paleta ---");
            var palette = ParsePaletteFromBytes(raw);
            if (palette.Count == PaletteColorCount)
            {
                sb.AppendLine($"Paleta encontrada con {palette.Count} colores. Primeros 8:");
                for (int i = 0; i < 8; i++)
                {
                    sb.AppendLine($"  [{i}] R={palette[i].R} G={palette[i].G} B={palette[i].B}");
                }
            }
            else
            {
                sb.AppendLine("No se encontró una paleta válida.");
            }

            return sb.ToString();
        }
    }
}