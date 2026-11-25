using System;
using System.IO;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Decodificador para archivos CRI ADX (formato de audio ADPCM usado en varios
    /// juegos de PS2, incluyendo WE8/PES). Soporta el codec "estándar" (tipo 0x03,
    /// predicción de orden 2 con coeficientes fijos derivados del "highpass
    /// frequency"), que es el utilizado por los archivos VP_P0_R*.adx de
    /// unnamed_47.afs. No soporta ADX cifrado (keycode) ni el codec AHX.
    /// </summary>
    public static class AdxDecoder
    {
        /// <summary>
        /// Decodifica un archivo ADX completo (bytes crudos) a PCM de 16 bits y lo
        /// envuelve en un contenedor WAV (RIFF/WAVE) reproducible por
        /// System.Media.SoundPlayer / MediaPlayer.
        /// </summary>
        public static byte[] DecodeToWav(byte[] adxData)
        {
            if (adxData == null || adxData.Length < 20)
            {
                throw new InvalidDataException("El archivo ADX es demasiado pequeño o está vacío.");
            }

            // Cabecera ADX: 0x80 0x00 <copyrightOffset(2 bytes, BE)> ...
            if (adxData[0] != 0x80)
            {
                throw new InvalidDataException("El archivo no tiene la firma ADX esperada (0x80).");
            }

            int copyrightOffset = ReadUInt16BE(adxData, 2);

            byte encodingType = adxData[4];
            int blockSize = adxData[5];
            int sampleBitdepth = adxData[6];
            int channelCount = adxData[7];
            uint sampleRate = ReadUInt32BE(adxData, 8);
            uint sampleCount = ReadUInt32BE(adxData, 12);
            int highpassFrequency = ReadUInt16BE(adxData, 16);

            if (encodingType != 0x03)
            {
                throw new NotSupportedException($"Tipo de codificación ADX no soportado: 0x{encodingType:X2}. Solo se soporta el codec estándar (0x03).");
            }

            if (channelCount < 1 || channelCount > 2)
            {
                throw new NotSupportedException($"Número de canales ADX no soportado: {channelCount}.");
            }

            // Los datos de audio empiezan 4 bytes después del offset de copyright
            // (ese offset apunta al byte justo antes de la cadena "(c)CRI").
            int dataStart = copyrightOffset + 4;

            // Coeficientes de predicción fijos derivados de la frecuencia de corte
            // (highpass), tal como los calcula CRI ADX Encoder / libvgmstream.
            (double coefficient1, double coefficient2) = CalculateCoefficients(highpassFrequency, sampleRate);

            int samplesPerBlock = (blockSize - 2) * 2; // 2 bytes de header por bloque, 4 bits por muestra
            if (samplesPerBlock <= 0)
            {
                throw new InvalidDataException("Tamaño de bloque ADX inválido.");
            }

            short[][] channelSamples = new short[channelCount][];
            int totalSamples = (int)sampleCount;

            for (int ch = 0; ch < channelCount; ch++)
            {
                channelSamples[ch] = DecodeChannel(adxData, dataStart + (ch * blockSize), blockSize, samplesPerBlock, totalSamples, adxData.Length, channelCount, coefficient1, coefficient2);
            }

            return BuildWavFile(channelSamples, channelCount, (int)sampleRate, totalSamples);
        }

        /// <summary>
        /// Decodifica todos los bloques de un canal, saltando "stride" bytes
        /// (blockSize * channelCount) entre bloques consecutivos del mismo canal,
        /// tal como se intercalan los canales en el stream ADX.
        /// </summary>
        private static short[] DecodeChannel(byte[] adxData, int firstBlockOffset, int blockSize, int samplesPerBlock, int totalSamples, int dataLength, int channelCount, double coefficient1, double coefficient2)
        {
            short[] samples = new short[totalSamples];

            double history1 = 0;
            double history2 = 0;

            int stride = blockSize * channelCount;
            int blockOffset = firstBlockOffset;
            int sampleIndex = 0;

            while (sampleIndex < totalSamples && blockOffset + blockSize <= dataLength)
            {
                int scale = ReadUInt16BE(adxData, blockOffset);
                int nibbleDataOffset = blockOffset + 2;

                int samplesInThisBlock = Math.Min(samplesPerBlock, totalSamples - sampleIndex);

                for (int i = 0; i < samplesInThisBlock; i++)
                {
                    int byteIndex = nibbleDataOffset + (i / 2);
                    byte b = adxData[byteIndex];

                    // Nibble alto primero, luego nibble bajo.
                    int nibble = (i % 2 == 0) ? (b >> 4) & 0x0F : b & 0x0F;

                    // Signo extendido de 4 bits.
                    int signedNibble = nibble >= 8 ? nibble - 16 : nibble;

                    double prediction = (coefficient1 * history1) + (coefficient2 * history2);
                    double sampleValue = (signedNibble * scale) + prediction;

                    short clamped = ClampToInt16(sampleValue);

                    samples[sampleIndex] = clamped;
                    sampleIndex++;

                    history2 = history1;
                    history1 = clamped;
                }

                blockOffset += stride;
            }

            return samples;
        }

        private static short ClampToInt16(double value)
        {
            if (value > short.MaxValue) return short.MaxValue;
            if (value < short.MinValue) return short.MinValue;
            return (short)value;
        }

        /// <summary>
        /// Calcula los coeficientes de predicción de orden 2 a partir de la
        /// frecuencia de corte (highpass) embebida en la cabecera, replicando el
        /// cálculo estándar usado por CRI ADX Encoder / libvgmstream.
        /// </summary>
        private static (double, double) CalculateCoefficients(int highpassFrequency, uint sampleRate)
        {
            if (highpassFrequency <= 0 || sampleRate == 0)
            {
                // Valores por defecto razonables si la cabecera no trae highpass.
                return (0.0, 0.0);
            }

            double x = Math.Cos(2.0 * Math.PI * highpassFrequency / sampleRate);
            double a = Math.Sqrt(2.0) - x;
            double b = Math.Sqrt(2.0) - 1.0;
            double c = (a - Math.Sqrt((a + b) * (a - b))) / b;

            double coefficient1 = c * 2.0;
            double coefficient2 = -(c * c);

            return (coefficient1, coefficient2);
        }

        private static byte[] BuildWavFile(short[][] channelSamples, int channelCount, int sampleRate, int totalSamples)
        {
            const int bitsPerSample = 16;
            int blockAlign = channelCount * (bitsPerSample / 8);
            int byteRate = sampleRate * blockAlign;
            int dataSize = totalSamples * blockAlign;

            using var ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms))
            {
                // RIFF header
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(36 + dataSize);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

                // fmt chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1); // PCM
                writer.Write((short)channelCount);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)blockAlign);
                writer.Write((short)bitsPerSample);

                // data chunk
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize);

                for (int i = 0; i < totalSamples; i++)
                {
                    for (int ch = 0; ch < channelCount; ch++)
                    {
                        writer.Write(channelSamples[ch][i]);
                    }
                }
            }

            return ms.ToArray();
        }

        private static int ReadUInt16BE(byte[] data, int offset)
        {
            return (data[offset] << 8) | data[offset + 1];
        }

        private static uint ReadUInt32BE(byte[] data, int offset)
        {
            return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]);
        }
    }
}