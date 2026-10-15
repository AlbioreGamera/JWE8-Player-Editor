using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace JWE8Editor.Logic
{
    public static class ParaWe8Reader
    {
        /// <summary>
        /// Descomprime el stream zlib completo de para_we8.bin.
        /// La cabecera de control (76 bytes aprox.) precede al stream zlib;
        /// el offset de inicio del stream zlib (0x44 en la cabecera) ya fue
        /// verificado como correcto: apunta al byte "78 DA".
        /// </summary>
        public static byte[] Decompress(string paraWe8BinPath)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);

            // Localizar el inicio real del stream zlib buscando la firma 78 DA
            // (más robusto que confiar ciegamente en el campo de cabecera 0x44,
            // dado que ya detectamos inconsistencias en otros campos de esa cabecera)
            int zlibStart = FindZlibHeader(raw);
            if (zlibStart < 0)
                throw new InvalidDataException("No se encontró la firma zlib (78 DA) en el archivo.");

            using var compStream = new MemoryStream(raw, zlibStart, raw.Length - zlibStart);
            using var zlibStream = new ZLibStream(compStream, CompressionMode.Decompress);
            using var outMs = new MemoryStream();
            zlibStream.CopyTo(outMs);

            return outMs.ToArray();
        }

        /// <summary>
        /// Descomprime TODOS los streams zlib consecutivos encontrados en el archivo,
        /// devolviendo un buffer descomprimido por cada uno (en orden de aparición).
        /// Esto reemplaza la hipótesis anterior ("un solo stream zlib que produce un
        /// blob con los 5 sub-archivos concatenados"), que fue invalidada empíricamente:
        /// el primer stream zlib descomprime exactamente a 398908 bytes, que coincide
        /// EXACTAMENTE con el tamaño reportado de _000 por sí solo, lo que indica que
        /// cada sub-archivo (_000.._004) tiene su propio stream zlib independiente,
        /// concatenados uno tras otro en para_we8.bin.
        /// </summary>
        public static List<byte[]> DecompressAll(string paraWe8BinPath)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);
            var results = new List<byte[]>();

            int searchStart = 0;
            while (true)
            {
                int zlibStart = FindZlibHeader(raw, searchStart);
                if (zlibStart < 0)
                    break; // no hay más streams zlib

                using var compStream = new MemoryStream(raw, zlibStart, raw.Length - zlibStart);
                using var zlibStream = new ZLibStream(compStream, CompressionMode.Decompress);
                using var outMs = new MemoryStream();
                zlibStream.CopyTo(outMs);

                results.Add(outMs.ToArray());

                // compStream.Position avanza hasta el byte siguiente al último byte
                // consumido por el stream zlib (incluye el checksum Adler-32 final).
                // Usamos eso para saber dónde empezar a buscar el próximo stream.
                long consumed = compStream.Position;
                if (consumed <= 0)
                    break; // salvaguarda contra loop infinito

                searchStart = zlibStart + (int)consumed;

                if (searchStart >= raw.Length - 1)
                    break;
            }

            return results;
        }

        private static int FindZlibHeader(byte[] data) => FindZlibHeader(data, 0);

        private static int FindZlibHeader(byte[] data, int startIndex)
        {
            // Firmas válidas de cabecera zlib con compresión por defecto
            byte[][] signatures = { new byte[] { 0x78, 0xDA }, new byte[] { 0x78, 0x9C }, new byte[] { 0x78, 0x01 } };
            for (int i = startIndex; i < data.Length - 1; i++)
            {
                foreach (var sig in signatures)
                {
                    if (data[i] == sig[0] && data[i + 1] == sig[1])
                        return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Busca todas las ocurrencias de una secuencia de bytes (firma) dentro de un buffer.
        /// Útil para validar empíricamente offsets reales de sub-archivos, en vez de
        /// confiar ciegamente en los campos de la cabecera.
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
        /// Diagnóstico: devuelve el offset de inicio del stream zlib, cuántos bytes
        /// comprimidos consumió, el tamaño total del archivo, y un volcado hexadecimal
        /// de los primeros N bytes inmediatamente después del stream zlib.
        /// Útil para confirmar si _001.._004 están almacenados sin comprimir justo
        /// después del primer (y único) stream zlib encontrado.
        /// </summary>
        public static string GetRawFileInfo(string paraWe8BinPath, int dumpBytesAfter = 64)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);

            int zlibStart = FindZlibHeader(raw, 0);
            if (zlibStart < 0)
                return "No se encontró ningún stream zlib en el archivo.";

            using var compStream = new MemoryStream(raw, zlibStart, raw.Length - zlibStart);
            using (var zlibStream = new ZLibStream(compStream, CompressionMode.Decompress, leaveOpen: true))
            using (var outMs = new MemoryStream())
            {
                zlibStream.CopyTo(outMs);
            }

            long consumed = compStream.Position;
            int afterZlib = zlibStart + (int)consumed;

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Tamaño total del archivo: {raw.Length} bytes");
            sb.AppendLine($"Stream zlib inicia en offset: {zlibStart} (0x{zlibStart:X})");
            sb.AppendLine($"Bytes comprimidos consumidos: {consumed}");
            sb.AppendLine($"Fin del stream zlib (posible inicio de _001): offset {afterZlib} (0x{afterZlib:X})");
            sb.AppendLine($"Bytes restantes después del stream zlib: {raw.Length - afterZlib}");
            sb.AppendLine();
            sb.AppendLine($"Volcado hex de los {dumpBytesAfter} bytes siguientes al stream zlib:");

            int count = Math.Min(dumpBytesAfter, raw.Length - afterZlib);
            for (int i = 0; i < count; i++)
            {
                sb.Append(raw[afterZlib + i].ToString("X2"));
                sb.Append(' ');
                if ((i + 1) % 16 == 0)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}