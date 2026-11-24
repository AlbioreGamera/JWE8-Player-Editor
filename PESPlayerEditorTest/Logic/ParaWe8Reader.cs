using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JWE8Editor.Logic
{
    public static class ParaWe8Reader
    {
        /// <summary>
        /// Descomprime el primer stream zlib válido encontrado en para_we8.bin.
        /// </summary>
        public static byte[] Decompress(string paraWe8BinPath)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);

            var candidate = FindNextValidZlibStream(raw, 0);
            if (candidate == null)
                throw new InvalidDataException("No se encontró ningún stream zlib válido en el archivo.");

            return candidate.Value.Decompressed;
        }

        /// <summary>
        /// Descomprime TODOS los streams zlib consecutivos encontrados en el archivo,
        /// devolviendo un buffer descomprimido por cada uno (en orden de aparición).
        /// Confirmado empíricamente contra DKZ Studio / Zlib Editor: para_we8.bin
        /// contiene 5 sub-archivos independientes (unknow_000.._004), cada uno con
        /// su propio stream zlib, concatenados uno tras otro.
        /// </summary>
        public static List<byte[]> DecompressAll(string paraWe8BinPath)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);
            var results = new List<byte[]>();

            int searchStart = 0;
            while (searchStart < raw.Length - 1)
            {
                var candidate = FindNextValidZlibStream(raw, searchStart);
                if (candidate == null)
                    break; // no hay más streams zlib válidos

                results.Add(candidate.Value.Decompressed);

                searchStart = candidate.Value.StartOffset + candidate.Value.ConsumedBytes;

                if (candidate.Value.ConsumedBytes <= 0)
                    break; // salvaguarda contra loop infinito
            }

            return results;
        }

        /// <summary>
        /// Resultado de localizar y descomprimir con éxito un stream zlib.
        /// </summary>
        private readonly struct ZlibStreamResult
        {
            public readonly int StartOffset;
            public readonly int ConsumedBytes;
            public readonly byte[] Decompressed;

            public ZlibStreamResult(int startOffset, int consumedBytes, byte[] decompressed)
            {
                StartOffset = startOffset;
                ConsumedBytes = consumedBytes;
                Decompressed = decompressed;
            }
        }

        /// <summary>
        /// Intenta descomprimir completamente un stream zlib a partir del buffer
        /// <paramref name="data"/>, empezando en <paramref name="offset"/> y usando
        /// como máximo <paramref name="availableLength"/> bytes de entrada.
        /// Devuelve el resultado descomprimido si <paramref name="availableLength"/>
        /// es suficiente para completar la descompresión SIN que sobren bytes sin
        /// consumir del stream deflate (indicador de que el límite es exacto o,
        /// al menos, no corta el stream a mitad). Si faltan bytes (stream truncado)
        /// o los datos no son válidos, devuelve null.
        /// </summary>
        private static byte[]? TryDecompressWithLimit(byte[] data, int offset, int availableLength)
        {
            try
            {
                using var compStream = new MemoryStream(data, offset, availableLength, writable: false);
                using var zlibStream = new ZLibStream(compStream, CompressionMode.Decompress, leaveOpen: true);
                using var outMs = new MemoryStream();
                zlibStream.CopyTo(outMs);
                return outMs.ToArray();
            }
            catch (InvalidDataException)
            {
                // Datos insuficientes/truncados o corruptos con este límite.
                return null;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        /// <summary>
        /// Dado un offset de inicio que ya contiene una cabecera zlib matemáticamente
        /// válida, determina el número EXACTO de bytes comprimidos que ocupa ese
        /// stream mediante búsqueda binaria: se prueba a descomprimir con distintas
        /// longitudes de entrada hasta encontrar la longitud mínima que permite
        /// completar la descompresión con éxito.
        ///
        /// Esto es necesario porque <see cref="ZLibStream"/> (y DeflateStream en
        /// general) NO garantizan que <c>Stream.Position</c> del stream subyacente,
        /// tras terminar de descomprimir, refleje el número real de bytes consumidos
        /// por el stream deflate: suelen hacer lecturas en bloque ("read-ahead") para
        /// su buffer interno, dejando la posición muy por delante del final real del
        /// stream. Confiar en esa posición hace que, al buscar el siguiente stream
        /// zlib concatenado, se salte de más y nunca se encuentre (síntoma observado:
        /// solo se extraía el primer sub-archivo, _000).
        ///
        /// La búsqueda binaria es válida porque la propiedad es monótona: si una
        /// longitud L de bytes de entrada permite completar la descompresión con
        /// éxito, cualquier longitud mayor también lo permite (el exceso de bytes,
        /// que pertenecen al siguiente stream, simplemente se ignoran una vez
        /// alcanzado el final lógico del stream deflate actual).
        /// </summary>
        private static int? FindExactConsumedBytes(byte[] data, int offset, byte[] decompressedReference)
        {
            int maxLength = data.Length - offset;

            int low = 1;
            int high = maxLength;
            int result = -1;

            while (low <= high)
            {
                int mid = low + ((high - low) / 2);

                byte[]? decompressed = TryDecompressWithLimit(data, offset, mid);

                if (decompressed != null && BytesEqual(decompressed, decompressedReference))
                {
                    // Esta longitud ya alcanza para completar el stream exacto;
                    // probamos a reducir para encontrar el mínimo.
                    result = mid;
                    high = mid - 1;
                }
                else
                {
                    // Longitud insuficiente (stream truncado) o datos corruptos;
                    // probamos con más bytes disponibles.
                    low = mid + 1;
                }
            }

            return result > 0 ? result : null;
        }

        private static bool BytesEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Busca, a partir de <paramref name="startIndex"/>, el siguiente offset que
        /// contenga una cabecera zlib matemáticamente válida (no solo firmas fijas
        /// como 78 DA) Y que además descomprima correctamente. Esto es necesario
        /// porque streams zlib pequeños (ej. 40, 303 bytes comprimidos) pueden usar
        /// combinaciones de cabecera distintas a las 3 firmas "comunes", y una
        /// búsqueda ingenua de firmas fijas las pasa por alto.
        ///
        /// Validación de cabecera zlib (RFC 1950):
        /// - CMF (byte 0): los 4 bits bajos (CM) deben ser 8 (método "deflate").
        /// - (CMF * 256 + FLG) debe ser múltiplo de 31.
        /// Como esta validación matemática puede dar algún falso positivo en datos
        /// binarios arbitrarios, se confirma intentando descomprimir realmente;
        /// si falla, se continúa buscando desde el siguiente byte.
        ///
        /// El número exacto de bytes comprimidos consumidos se calcula aparte, vía
        /// <see cref="FindExactConsumedBytes"/>, en vez de confiar en la posición
        /// del stream tras CopyTo (ver comentario de ese método para el porqué).
        /// </summary>
        private static ZlibStreamResult? FindNextValidZlibStream(byte[] data, int startIndex)
        {
            for (int i = startIndex; i < data.Length - 1; i++)
            {
                byte cmf = data[i];
                byte flg = data[i + 1];

                if ((cmf & 0x0F) != 8)
                    continue; // CM debe ser 8 (deflate)

                int cmfHigh = cmf >> 4;
                if (cmfHigh > 7)
                    continue; // CINFO válido: tamaño de ventana <= 32K (log2-8 <= 7)

                if (((cmf * 256) + flg) % 31 != 0)
                    continue; // checksum de cabecera zlib inválido

                // Candidato matemáticamente válido: intentar descomprimir usando
                // todos los bytes restantes del buffer como límite superior.
                byte[]? decompressed = TryDecompressWithLimit(data, i, data.Length - i);
                if (decompressed == null)
                    continue; // falso positivo; seguir buscando desde el siguiente byte

                int? consumed = FindExactConsumedBytes(data, i, decompressed);
                if (consumed == null || consumed.Value <= 0)
                    continue; // no se pudo determinar un límite exacto consistente

                return new ZlibStreamResult(i, consumed.Value, decompressed);
            }

            return null;
        }

        /// <summary>
        /// Busca todas las ocurrencias de una secuencia de bytes (firma) dentro de un buffer.
        /// Útil para validar empíricamente offsets reales de sub-archivos.
        /// </summary>
        public static List<int> FindSignature(byte[] haystack, byte[] signature)
        {
            var results = new List<int>();
            if (signature == null || signature.Length == 0 || haystack.Length < signature.Length)
                return results;

            for (int i = 0; i <= haystack.Length - signature.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < signature.Length; j++)
                {
                    if (haystack[i + j] != signature[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    results.Add(i);
            }

            return results;
        }

        /// <summary>
        /// Diagnóstico: devuelve, para cada stream zlib detectado, su offset de inicio,
        /// bytes comprimidos consumidos y tamaño descomprimido resultante. Útil para
        /// confirmar manualmente la correspondencia con unknow_000.._004 reportada
        /// por herramientas como DKZ Studio / Zlib Editor.
        /// </summary>
        public static string GetRawFileInfo(string paraWe8BinPath, int dumpBytesAfter = 64)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Tamaño total del archivo: {raw.Length} bytes");
            sb.AppendLine();

            int searchStart = 0;
            int index = 0;
            while (searchStart < raw.Length - 1)
            {
                var candidate = FindNextValidZlibStream(raw, searchStart);
                if (candidate == null)
                {
                    sb.AppendLine("No se encontraron más streams zlib válidos.");
                    break;
                }

                sb.AppendLine($"Stream #{index}: offset=0x{candidate.Value.StartOffset:X} ({candidate.Value.StartOffset}), " +
                    $"comprimido={candidate.Value.ConsumedBytes} bytes, descomprimido={candidate.Value.Decompressed.Length} bytes");

                searchStart = candidate.Value.StartOffset + candidate.Value.ConsumedBytes;
                index++;

                if (candidate.Value.ConsumedBytes <= 0)
                    break;
            }

            return sb.ToString();
        }
    }
}