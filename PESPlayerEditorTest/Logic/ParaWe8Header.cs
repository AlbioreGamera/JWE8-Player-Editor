using System;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Represents the parsed header fields of the decompressed para_we8.bin payload.
    /// NOTE: Offsets below are provisional and still pending empirical validation
    /// via ParaWe8Reader.FindSignature against the known sizes of the extracted
    /// sub-files (_000.._004). Adjust once validated.
    /// </summary>
    public class ParaWe8Header
    {
        public int Field00;
        public int DecompressedTotalSize;
        public int SubFileCount;
        public int EntrySize;
        public int TableOffset;
        public int[] SubFileOffsets;
        public int CompressedDataOffset;
        public int CompressedDataSize;

        public static ParaWe8Header Parse(byte[] raw)
        {
            var header = new ParaWe8Header
            {
                Field00 = BitConverter.ToInt32(raw, 0x00),
                DecompressedTotalSize = BitConverter.ToInt32(raw, 0x04),
                SubFileCount = BitConverter.ToInt32(raw, 0x08),
                EntrySize = BitConverter.ToInt32(raw, 0x0C),
                TableOffset = BitConverter.ToInt32(raw, 0x10),
                CompressedDataOffset = BitConverter.ToInt32(raw, 0x28),
                CompressedDataSize = BitConverter.ToInt32(raw, 0x2C)
            };

            header.SubFileOffsets = new int[header.SubFileCount];
            for (int i = 0; i < header.SubFileCount; i++)
            {
                header.SubFileOffsets[i] = BitConverter.ToInt32(raw, 0x14 + (i * 4));
            }

            return header;
        }
    }
}