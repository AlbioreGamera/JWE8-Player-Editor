using PlayerLibrary;
using System;
using System.IO;
using System.Text;
using System.Windows;

namespace JWE8Editor.Logic
{
    public class PlayerWriter
    {
        private const int RecordSize = 0x7C;
        private const long HeaderOffset = 124; // mismo offset inicial usado en ParsePlayers

        /// <summary>
        /// Sobrescribe un jugador específico directamente en el archivo binario,
        /// localizándolo por su índice (registro de 124 bytes).
        /// </summary>
        public void WritePlayerToFile(string filePath, Player player)
        {
            long recordOffset = HeaderOffset + (long)(player.PlayerIndex - 1) * RecordSize;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                if (recordOffset + RecordSize > fs.Length)
                {
                    MessageBox.Show("Índice de jugador fuera de rango.");
                    return;
                }

                byte[] buffer = new byte[RecordSize];
                fs.Seek(recordOffset, SeekOrigin.Begin);
                fs.Read(buffer, 0, RecordSize);

                WritePlayerRecord(buffer, player);

                fs.Seek(recordOffset, SeekOrigin.Begin);
                fs.Write(buffer, 0, RecordSize);
            }
        }

        public void WritePlayerRecord(byte[] buffer, Player player)
        {
            static void Pack(ref uint block, int bitOffset, int bitCount, uint fieldValue)
            {
                block = BitFieldReader.WriteBits(block, bitOffset, bitCount, fieldValue);
            }

            // --- Nombre / ShirtName / Commentary (campos fuera de bitfields) ---
            byte[] nameBytes = Encoding.Unicode.GetBytes(player.Name ?? string.Empty);
            Array.Clear(buffer, 0x00, 32);
            Array.Copy(nameBytes, 0, buffer, 0x00, Math.Min(nameBytes.Length, 32));

            byte[] shirtBytes = Encoding.ASCII.GetBytes(player.ShirtName ?? string.Empty);
            Array.Clear(buffer, 0x20, 16);
            Array.Copy(shirtBytes, 0, buffer, 0x20, Math.Min(shirtBytes.Length, 16));

            byte[] commentaryBytes = BitConverter.GetBytes((ushort)player.Commentary);
            Array.Copy(commentaryBytes, 0, buffer, 0x30, 2);

            // --- 0x34 ---
            uint block34 = 0;
            Pack(ref block34, 0, 4, PlayerFieldValidator.ClampEnum(player.RegPosition, 4));
            Pack(ref block34, 4, 7, PlayerFieldValidator.ClampRawRange(player.Attack, 7));
            Pack(ref block34, 11, 7, PlayerFieldValidator.ClampRawRange(player.Defense, 7));
            Pack(ref block34, 18, 7, PlayerFieldValidator.ClampRawRange(player.Balance, 7));
            Pack(ref block34, 25, 7, PlayerFieldValidator.ClampRawRange(player.Stamina, 7));
            BitConverter.GetBytes(block34).CopyTo(buffer, 0x34);

            // --- 0x38 ---
            uint block38 = 0;
            Pack(ref block38, 0, 7, PlayerFieldValidator.ClampRawRange(player.TopSpeed, 7));
            Pack(ref block38, 7, 7, PlayerFieldValidator.ClampRawRange(player.Acceleration, 7));
            Pack(ref block38, 14, 7, PlayerFieldValidator.ClampRawRange(player.Response, 7));
            Pack(ref block38, 21, 7, PlayerFieldValidator.ClampRawRange(player.DribbleAccuracy, 7));
            Pack(ref block38, 28, 1, PlayerFieldValidator.ClampBool(player.CanPlayGK));
            Pack(ref block38, 30, 1, PlayerFieldValidator.ClampBool(player.CanPlayCBW));
            Pack(ref block38, 31, 1, PlayerFieldValidator.ClampBool(player.CanPlayCBT));
            BitConverter.GetBytes(block38).CopyTo(buffer, 0x38);

            // --- 0x3c ---
            uint block3c = 0;
            Pack(ref block3c, 0, 7, PlayerFieldValidator.ClampRawRange(player.DribbleSpeed, 7));
            Pack(ref block3c, 7, 7, PlayerFieldValidator.ClampRawRange(player.ShortPassAccuracy, 7));
            Pack(ref block3c, 14, 7, PlayerFieldValidator.ClampRawRange(player.ShortPassSpeed, 7));
            Pack(ref block3c, 21, 7, PlayerFieldValidator.ClampRawRange(player.LongPassAccuracy, 7));
            Pack(ref block3c, 28, 1, PlayerFieldValidator.ClampBool(player.CanPlaySB));
            Pack(ref block3c, 29, 1, PlayerFieldValidator.ClampBool(player.CanPlayDMF));
            Pack(ref block3c, 30, 1, PlayerFieldValidator.ClampBool(player.CanPlayCMF));
            Pack(ref block3c, 31, 1, PlayerFieldValidator.ClampBool(player.CanPlaySMF));
            BitConverter.GetBytes(block3c).CopyTo(buffer, 0x3c);

            // --- 0x40 ---
            uint block40 = 0;
            Pack(ref block40, 0, 7, PlayerFieldValidator.ClampRawRange(player.LongPassSpeed, 7));
            Pack(ref block40, 7, 7, PlayerFieldValidator.ClampRawRange(player.ShotPower, 7));
            Pack(ref block40, 14, 7, PlayerFieldValidator.ClampRawRange(player.ShotAccuracy, 7));
            Pack(ref block40, 21, 7, PlayerFieldValidator.ClampRawRange(player.ShotTechnique, 7));
            Pack(ref block40, 28, 1, PlayerFieldValidator.ClampBool(player.CanPlayAMF));
            Pack(ref block40, 29, 1, PlayerFieldValidator.ClampBool(player.CanPlayCF));
            Pack(ref block40, 30, 1, PlayerFieldValidator.ClampBool(player.CanPlayWF));
            Pack(ref block40, 31, 1, PlayerFieldValidator.ClampBool(player.Dribbling));
            BitConverter.GetBytes(block40).CopyTo(buffer, 0x40);

            // --- 0x44 ---
            uint block44 = 0;
            Pack(ref block44, 0, 7, PlayerFieldValidator.ClampRawRange(player.Jump, 7));
            Pack(ref block44, 7, 7, PlayerFieldValidator.ClampRawRange(player.Header, 7));
            Pack(ref block44, 14, 7, PlayerFieldValidator.ClampRawRange(player.Technique, 7));
            Pack(ref block44, 21, 7, PlayerFieldValidator.ClampRawRange(player.Curling, 7));
            Pack(ref block44, 28, 1, PlayerFieldValidator.ClampBool(player.PostPlayer));
            Pack(ref block44, 29, 1, PlayerFieldValidator.ClampBool(player.Positioning));
            Pack(ref block44, 30, 1, PlayerFieldValidator.ClampBool(player.Reaction));
            Pack(ref block44, 31, 1, PlayerFieldValidator.ClampBool(player.Scoring));
            BitConverter.GetBytes(block44).CopyTo(buffer, 0x44);

            // --- 0x48 ---
            uint block48 = 0;
            Pack(ref block48, 0, 7, PlayerFieldValidator.ClampRawRange(player.Aggression, 7));
            Pack(ref block48, 7, 7, PlayerFieldValidator.ClampRawRange(player.Mentality, 7));
            Pack(ref block48, 14, 3, PlayerFieldValidator.ClampRawRange(player.Condition, 3));
            Pack(ref block48, 17, 3, PlayerFieldValidator.ClampRawRange(player.Consistency, 3));
            Pack(ref block48, 20, 7, PlayerFieldValidator.ClampRawRange(player.FreeKickAccuracy, 7));
            Pack(ref block48, 27, 1, PlayerFieldValidator.ClampBool(player.Playmaking));
            Pack(ref block48, 28, 1, PlayerFieldValidator.ClampBool(player.Passing));
            Pack(ref block48, 29, 1, PlayerFieldValidator.ClampBool(player.Penalties));
            Pack(ref block48, 30, 1, PlayerFieldValidator.ClampBool(player.OneOnOneScoring));
            Pack(ref block48, 31, 1, PlayerFieldValidator.ClampBool(player.LongThrow));
            BitConverter.GetBytes(block48).CopyTo(buffer, 0x48);

            // --- 0x4c ---
            uint block4c = 0;
            Pack(ref block4c, 0, 7, PlayerFieldValidator.ClampRawRange(player.TeamWorkAbility, 7));
            Pack(ref block4c, 7, 7, PlayerFieldValidator.ClampRawRange(player.Agility, 7));
            Pack(ref block4c, 14, 7, PlayerFieldValidator.ClampRawRange(player.GoalKeepingSkills, 7));
            Pack(ref block4c, 21, 1, PlayerFieldValidator.ClampEnum(player.DominantFoot, 1));
            Pack(ref block4c, 23, 3, PlayerFieldValidator.ClampPlus1(player.WeakFootAccuracy, 3));
            Pack(ref block4c, 26, 3, PlayerFieldValidator.ClampPlus1(player.WeakFootFrequency, 3));
            Pack(ref block4c, 29, 3, PlayerFieldValidator.ClampEnum(player.FreeKickType, 3));
            BitConverter.GetBytes(block4c).CopyTo(buffer, 0x4c);

            // --- 0x50 ---
            uint block50 = 0;
            Pack(ref block50, 2, 1, PlayerFieldValidator.ClampBool(player.OneTouchPass));
            Pack(ref block50, 3, 1, PlayerFieldValidator.ClampBool(player.Marking));
            Pack(ref block50, 4, 1, PlayerFieldValidator.ClampBool(player.DLineControl));
            Pack(ref block50, 5, 1, PlayerFieldValidator.ClampBool(player.DirectGKKick));
            Pack(ref block50, 6, 1, PlayerFieldValidator.ClampBool(player.OneOnOneStopper));
            Pack(ref block50, 7, 1, PlayerFieldValidator.ClampBool(player.PenaltyStopper));
            Pack(ref block50, 8, 1, PlayerFieldValidator.ClampBool(player.BothFeetFeint));
            Pack(ref block50, 9, 1, PlayerFieldValidator.ClampBool(player.Outside));
            Pack(ref block50, 10, 2, PlayerFieldValidator.ClampEnum(player.Injury, 2));
            Pack(ref block50, 12, 1, PlayerFieldValidator.ClampBool(player.SlidingTackle));
            BitConverter.GetBytes(block50).CopyTo(buffer, 0x50);

            // --- 0x54 ---
            uint block54 = 0;
            Pack(ref block54, 2, 2, PlayerFieldValidator.ClampEnum(player.PenaltyKickType, 2));
            Pack(ref block54, 4, 6, PlayerFieldValidator.ClampRawRange(player.GoalCelebration1, 6));
            Pack(ref block54, 11, 6, PlayerFieldValidator.ClampRawRange(player.GoalCelebration2, 6));
            Pack(ref block54, 17, 1, PlayerFieldValidator.ClampBool(player.TacticalDribble));
            Pack(ref block54, 18, 1, PlayerFieldValidator.ClampBool(player.LinePositioning));
            Pack(ref block54, 19, 1, PlayerFieldValidator.ClampBool(player.Side));
            Pack(ref block54, 20, 1, PlayerFieldValidator.ClampBool(player.Center));
            BitConverter.GetBytes(block54).CopyTo(buffer, 0x54);

            // --- 0x58 ---
            uint block58 = 0;
            Pack(ref block58, 0, 6, PlayerFieldValidator.ClampHeight(player.Height));
            Pack(ref block58, 6, 7, PlayerFieldValidator.ClampRawRange(player.Weight, 7));
            Pack(ref block58, 16, 2, PlayerFieldValidator.ClampEnum(player.SkinColour, 2));
            Pack(ref block58, 18, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.HeadHeight, 4));
            Pack(ref block58, 22, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.HeadWidth, 4));
            Pack(ref block58, 26, 3, PlayerFieldValidator.ClampEnum(player.BootsTongueColor, 3));
            Pack(ref block58, 29, 3, PlayerFieldValidator.ClampEnum(player.BootsLineColor2, 3));
            BitConverter.GetBytes(block58).CopyTo(buffer, 0x58);

            // --- 0x5c ---
            uint block5c = 0;
            Pack(ref block5c, 0, 11, PlayerFieldValidator.ClampRawRange(player.HairId, 11));
            Pack(ref block5c, 11, 6, PlayerFieldValidator.ClampRawRange(player.HairColourPattern, 6));
            Pack(ref block5c, 17, 2, PlayerFieldValidator.ClampPlus1(player.EyesColor1, 2));
            Pack(ref block5c, 19, 4, PlayerFieldValidator.ClampEnum(player.EyesColor2, 4));
            Pack(ref block5c, 23, 7, PlayerFieldValidator.ClampPlus1(player.FacialHairType, 7));
            Pack(ref block5c, 30, 2, PlayerFieldValidator.ClampEnum(player.Sleeves, 2));
            BitConverter.GetBytes(block5c).CopyTo(buffer, 0x5c);

            // --- 0x60 ---
            uint block60 = 0;
            Pack(ref block60, 0, 6, PlayerFieldValidator.ClampPlus1(player.FacialHairColour, 6));
            Pack(ref block60, 6, 2, PlayerFieldValidator.ClampRawRange(player.SunGlassesType, 2));
            Pack(ref block60, 8, 3, PlayerFieldValidator.ClampPlus1(player.SunGlassesColour, 3));
            Pack(ref block60, 11, 2, PlayerFieldValidator.ClampRawRange(player.MaskType, 2));
            Pack(ref block60, 13, 3, PlayerFieldValidator.ClampPlus1(player.MaskColour, 3));
            Pack(ref block60, 16, 1, PlayerFieldValidator.ClampEnum(player.CapType, 1));
            Pack(ref block60, 17, 3, PlayerFieldValidator.ClampEnum(player.CapColour, 3));
            Pack(ref block60, 20, 2, PlayerFieldValidator.ClampEnum(player.WristbandType, 2));
            Pack(ref block60, 22, 4, PlayerFieldValidator.ClampEnum(player.BootsType, 4));
            Pack(ref block60, 26, 3, PlayerFieldValidator.ClampEnum(player.BootsBaseColor, 3));
            Pack(ref block60, 29, 3, PlayerFieldValidator.ClampEnum(player.BootsLineColor1, 3));
            BitConverter.GetBytes(block60).CopyTo(buffer, 0x60);

            // --- 0x64 ---
            uint block64 = 0;
            Pack(ref block64, 0, 9, PlayerFieldValidator.ClampPlus1(player.FaceId, 9));
            Pack(ref block64, 9, 3, PlayerFieldValidator.ClampEnum(player.BootsStudsColour, 3));
            Pack(ref block64, 12, 1, PlayerFieldValidator.ClampEnum(player.Tape, 1));
            Pack(ref block64, 13, 6, PlayerFieldValidator.ClampMultiplyBy4(player.HairColorR, 6));
            Pack(ref block64, 19, 6, PlayerFieldValidator.ClampMultiplyBy4(player.HairColorG, 6));
            Pack(ref block64, 25, 6, PlayerFieldValidator.ClampMultiplyBy4(player.HairColorB, 6));
            Pack(ref block64, 31, 1, PlayerFieldValidator.ClampEnum(player.Gloves, 1));
            BitConverter.GetBytes(block64).CopyTo(buffer, 0x64);

            // --- 0x68 ---
            uint block68 = 0;
            Pack(ref block68, 0, 2, PlayerFieldValidator.ClampEnum(player.Socks, 2));
            Pack(ref block68, 2, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.NeckLength, 4));
            Pack(ref block68, 6, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ChestMeasurement, 4));
            Pack(ref block68, 10, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ArmCircumference, 4));
            Pack(ref block68, 14, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.WaisteCircumference, 4));
            Pack(ref block68, 18, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.LegCircumference, 4));
            Pack(ref block68, 22, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.CalfCircumference, 4));
            Pack(ref block68, 26, 2, PlayerFieldValidator.ClampEnum(player.FaceType, 2));
            Pack(ref block68, 28, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.LegLength, 4));
            BitConverter.GetBytes(block68).CopyTo(buffer, 0x68);

            // --- 0x6c ---
            uint block6c = 0;
            Pack(ref block6c, 0, 3, PlayerFieldValidator.ClampEnum(player.BandanaColour, 3));
            Pack(ref block6c, 3, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ShoulderHeight, 4));
            Pack(ref block6c, 7, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ShoulderWidth, 4));
            Pack(ref block6c, 11, 5, PlayerFieldValidator.ClampAge(player.Age));
            Pack(ref block6c, 16, 9, PlayerFieldValidator.ClampRawRange(player.Nationality, 9));
            BitConverter.GetBytes(block6c).CopyTo(buffer, 0x6c);

            // --- 0x74 ---
            uint block74 = 0;
            Pack(ref block74, 0, 3, PlayerFieldValidator.ClampInverted(player.BrowsHeight, 3));
            Pack(ref block74, 3, 3, PlayerFieldValidator.ClampInverted(player.EyesPosition, 3));
            Pack(ref block74, 6, 3, PlayerFieldValidator.ClampInverted(player.EyesAngle, 3));
            Pack(ref block74, 9, 3, PlayerFieldValidator.ClampInverted(player.EyesSize, 3));
            Pack(ref block74, 12, 3, PlayerFieldValidator.ClampInverted(player.NoseHeight, 3));
            Pack(ref block74, 15, 3, PlayerFieldValidator.ClampInverted(player.JawWidth, 3));
            Pack(ref block74, 18, 3, PlayerFieldValidator.ClampInverted(player.JawChin, 3));
            Pack(ref block74, 21, 3, PlayerFieldValidator.ClampInverted(player.NoseWidth, 3));
            Pack(ref block74, 24, 3, PlayerFieldValidator.ClampInverted(player.MouthSize, 3));
            Pack(ref block74, 27, 2, (uint)(player.MouthThicknessOfLips + 1));
            Pack(ref block74, 29, 2, PlayerFieldValidator.ClampPlus1(player.MouthType, 2));
            BitConverter.GetBytes(block74).CopyTo(buffer, 0x74);

            // --- 0x78 ---
            uint block78 = 0;
            Pack(ref block78, 0, 6, PlayerFieldValidator.ClampRawRange(player.BrowsTypeAngle, 6));
            Pack(ref block78, 7, 3, PlayerFieldValidator.ClampPlus1(player.NoseType, 3));
            Pack(ref block78, 11, 3, PlayerFieldValidator.ClampPlus1(player.CheeksType, 3));
            Pack(ref block78, 14, 3, PlayerFieldValidator.ClampInverted(player.MouthPosition, 3));
            Pack(ref block78, 17, 5, PlayerFieldValidator.ClampPlus1(player.EyesType, 5));
            Pack(ref block78, 23, 3, PlayerFieldValidator.ClampPlus1(player.JawType, 3));
            BitConverter.GetBytes(block78).CopyTo(buffer, 0x78);
        }
    }
}