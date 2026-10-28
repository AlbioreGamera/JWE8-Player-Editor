namespace JWE8Editor.Logic
{
    /// <summary>
    /// Permite extraer campos de bits empaquetados en un uint32, replicando
    /// el orden de bits LSB-first usado por 010 Editor Binary Templates.
    /// </summary>
    public static class BitFieldReader
    {
        /// <summary>
        /// Extrae <paramref name="bitCount"/> bits a partir de <paramref name="bitOffset"/>
        /// (contados desde el bit menos significativo) del valor dado.
        /// </summary>
        public static uint ReadBits(uint value, int bitOffset, int bitCount)
        {
            uint mask = bitCount >= 32 ? uint.MaxValue : (1u << bitCount) - 1;
            return (value >> bitOffset) & mask;
        }

        /// <summary>
        /// Escribe <paramref name="fieldValue"/> (de <paramref name="bitCount"/> bits) dentro
        /// de <paramref name="target"/> en la posición <paramref name="bitOffset"/>, preservando
        /// el resto de los bits.
        /// </summary>
        public static uint WriteBits(uint target, int bitOffset, int bitCount, uint fieldValue)
        {
            uint mask = bitCount >= 32 ? uint.MaxValue : (1u << bitCount) - 1;
            fieldValue &= mask;
            uint clearedTarget = target & ~(mask << bitOffset);
            return clearedTarget | (fieldValue << bitOffset);
        }
    }
}