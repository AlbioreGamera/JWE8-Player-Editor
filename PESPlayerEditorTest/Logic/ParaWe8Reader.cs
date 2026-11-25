using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace JWE8Editor.Logic
{
    public static class ParaWe8Reader
    {
        public static byte[] Decompress(string paraWe8BinPath)
        {
            byte[] raw = File.ReadAllBytes(paraWe8BinPath);

            var candidate = FindNextValidZlibStream(raw, 0);
            if (candidate == null)
                throw new InvalidDataException("No se encontró ningún stream zlib válido en el archivo.");

            return candidate.Value.Decompressed;
        }

        /// <summary>
        /// Descomprime los 5 sub-archivos independientes (unknow_000.._004) que
        /// contiene para_we8.bin, cada uno como su propio stream zlib concatenado.
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
                    break;

                results.Add(candidate.Value.Decompressed);

                searchStart = candidate.Value.StartOffset + candidate.Value.ConsumedBytes;

                if (candidate.Value.ConsumedBytes <= 0)
                    break;
            }

            return results;
        }

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
                return null;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        /// <summary>
        /// Búsqueda binaria de la longitud mínima de bytes de entrada que permite
        /// completar la descompresión del stream (ZLibStream no expone de forma
        /// fiable cuántos bytes consumió realmente tras CopyTo).
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
                    result = mid;
                    high = mid - 1;
                }
                else
                {
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

        private static ZlibStreamResult? FindNextValidZlibStream(byte[] data, int startIndex)
        {
            for (int i = startIndex; i < data.Length - 1; i++)
            {
                byte cmf = data[i];
                byte flg = data[i + 1];

                if ((cmf & 0x0F) != 8)
                    continue;

                int cmfHigh = cmf >> 4;
                if (cmfHigh > 7)
                    continue;

                if (((cmf * 256) + flg) % 31 != 0)
                    continue;

                byte[]? decompressed = TryDecompressWithLimit(data, i, data.Length - i);
                if (decompressed == null)
                    continue;

                int? consumed = FindExactConsumedBytes(data, i, decompressed);
                if (consumed == null || consumed.Value <= 0)
                    continue;

                return new ZlibStreamResult(i, consumed.Value, decompressed);
            }

            return null;
        }
    }
}