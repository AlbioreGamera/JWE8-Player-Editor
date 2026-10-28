using System;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Aplica los mismos clamps/transformaciones inversas que las funciones
    /// Write* del template de 010 Editor, antes de volver a empaquetar
    /// los valores en los bitfields del archivo binario.
    /// </summary>
    public static class PlayerFieldValidator
    {
        public static uint ClampAge(int displayedAge)
        {
            if (displayedAge < 15) displayedAge = 15;
            if (displayedAge > 46) displayedAge = 46; // 5 bits => max 31 + 15
            return (uint)(displayedAge - 15);
        }

        public static uint ClampHeight(int displayedHeight)
        {
            if (displayedHeight < 148) displayedHeight = 148;
            if (displayedHeight > 148 + 63) displayedHeight = 148 + 63; // 6 bits
            return (uint)(displayedHeight - 148);
        }

        public static uint ClampPlus1(int displayedValue, int bitCount)
        {
            int max = (1 << bitCount) - 1 + 1; // rango de salida tras +1
            if (displayedValue < 1) displayedValue = 1;
            if (displayedValue > max) displayedValue = max;
            return (uint)(displayedValue - 1);
        }

        public static uint ClampPhysicalAttribute(int displayedValue, int bitCount)
        {
            int max = ((1 << bitCount) - 1) - 7;
            int rawMin = -7;
            int rawMax = max;
            if (displayedValue < rawMin) displayedValue = rawMin;
            if (displayedValue > rawMax) displayedValue = rawMax;
            return (uint)(displayedValue + 7);
        }

        public static uint ClampInverted(int displayedValue, int invertFrom)
        {
            if (displayedValue < 0) displayedValue = 0;
            if (displayedValue > invertFrom) displayedValue = invertFrom;
            return (uint)(invertFrom - displayedValue);
        }

        public static uint ClampMultiplyBy4(int displayedValue, int bitCount)
        {
            int maxRaw = (1 << bitCount) - 1;
            int maxDisplayed = maxRaw * 4;
            if (displayedValue < 0) displayedValue = 0;
            if (displayedValue > maxDisplayed) displayedValue = maxDisplayed;
            return (uint)(displayedValue / 4);
        }

        public static uint ClampRawRange(int displayedValue, int bitCount)
        {
            uint max = bitCount >= 32 ? uint.MaxValue : (1u << bitCount) - 1;
            if (displayedValue < 0) displayedValue = 0;
            if ((uint)displayedValue > max) displayedValue = (int)max;
            return (uint)displayedValue;
        }

        public static uint ClampEnum<TEnum>(TEnum value, int bitCount) where TEnum : Enum
        {
            uint raw = Convert.ToUInt32(value);
            uint max = bitCount >= 32 ? uint.MaxValue : (1u << bitCount) - 1;
            if (raw > max) raw = max;
            return raw;
        }

        public static uint ClampBool(bool value) => value ? 1u : 0u;
    }
}