using JWE8Editor.Logic;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SafeFileHandle? filePath;

        public FileStream? fileStream;

        public FileStream? fsPlayerAssignment;

        public FileStream? fsNumbers;

        public FileStream? fsEditOvl;

        public FileStream? fsDataset;

        List<Player> players = new List<Player>();

        List<PlayerAssignment> playerAssignment = new List<PlayerAssignment>();

        OpenFileDialog openFileDialog = new OpenFileDialog();

        public ObservableCollection<Player> People { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<PlayerAssignment> PeopleA { get; set; } = new ObservableCollection<PlayerAssignment>();
        public ObservableCollection<Callname> CallnameCollection { get; set; } = new ObservableCollection<Callname>();
        public ObservableCollection<Stadium> StadiumCollection { get; set; } = new ObservableCollection<Stadium>();
        public ObservableCollection<League> LeagueCollection { get; set; } = new ObservableCollection<League>();
        public ObservableCollection<Team> TeamCollection { get; set; } = new ObservableCollection<Team>();

        public JWE8Editor.Logic.LoadCommentaries CommentariesLoader { get; } = new JWE8Editor.Logic.LoadCommentaries();
        public JWE8Editor.Logic.LoadCountries CountriesLoader { get; } = new JWE8Editor.Logic.LoadCountries();

        public string? FilePath1 { get; internal set; }
        public string? FilePath2 { get; internal set; }
        public string? FilePath3 { get; internal set; }
        public string? FilePath4 { get; internal set; }
        public string? FilePath5 { get; internal set; }
        public string? FilePath6 { get; internal set; }

        // TODO: confirmar offsets reales con editor hexadecimal
        private const long LeagueBlockStart = 0x00000000; // <-- reemplazar con offset real
        private const long LeagueBlockEnd = 0x00000000;    // <-- reemplazar con offset real
        private const int LeagueRecordSize = 82;           // 21 (nombre+pad) * 2 + 40 padding aprox, confirmar
        private const int LeagueNameFieldSize = 21;        // bytes por copia del nombre

        // Offsets confirmados a partir de SLPM_657.40
        private const long TeamBlockStart = 0x00204480;
        private const long TeamBlockEnd = 0x00205d6f; // último byte (inclusive) de todo el listado, incluyendo el grupo "Reserve"

        // Estos equipos traen un cuarto campo extra (nombre duplicado/extendido) que el juego
        // ignora y que debe descartarse al parsear. Identificados por su abreviación, que es
        // el dato más estable del registro.
        private static readonly HashSet<string> TeamsWithExtraField = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "URW", // Urawa Reds
            "YFM", // Yokohama F・Marinos
            "BSK", // Bucheon SK
            "ASM", // AS Monaco
            "DEF", // Default
        };

        // Estos equipos no deben tener su romaji ni abreviación editados porque la lógica del
        // editor depende de ellos (detección de campo extra, filtros especiales, etc.).
        private static readonly HashSet<string> ProtectedTeamCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "URW", // Urawa Reds
            "YFM", // Yokohama F・Marinos
            "BSK", // Bucheon SK
            "ASM", // AS Monaco
            "DEF", // Default
        };

        private class TeamField
        {
            public long StartOffset;
            public byte[] Bytes = Array.Empty<byte>();
            public int TotalLength; // bytes útiles + relleno (0x00) hasta el siguiente campo
        }

        /// <summary>
        /// Decodifica los bytes de un campo de nombre de equipo. Los nombres con
        /// caracteres acentuados (ej. "Coruña") se guardan en UTF-8 (p.ej. "ñ" = C3 B1),
        /// mientras que algunos nombres japoneses usan Shift-JIS. Se intenta primero
        /// UTF-8 en modo estricto (lanza excepción si los bytes no son UTF-8 válido) y,
        /// si falla, se recurre a Shift-JIS como respaldo para no romper esos registros.
        /// </summary>
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        private static string DecodeTeamFieldBytes(byte[] bytes, Encoding shiftJisFallback)
        {
            try
            {
                return StrictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                return shiftJisFallback.GetString(bytes);
            }
        }

        private TeamField? ReadNullTerminatedField(FileStream fs)
        {
            long start = fs.Position;
            List<byte> bytes = new List<byte>();
            int b;
            while ((b = fs.ReadByte()) != -1 && b != 0x00)
            {
                bytes.Add((byte)b);
            }

            int nullCount = 0;
            while (b == 0x00)
            {
                nullCount++;
                b = fs.ReadByte();
            }

            if (b != -1 && b != 0x00)
            {
                fs.Position -= 1;
            }

            if (bytes.Count == 0 && nullCount == 0)
            {
                return null; // EOF
            }

            return new TeamField
            {
                StartOffset = start,
                Bytes = bytes.ToArray(),
                TotalLength = bytes.Count + nullCount
            };
        }

        public void ParseTeams(string filePath)
        {
            TeamCollection.Clear();

            using (FileStream fsTeams = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding shiftJis = Encoding.GetEncoding("shift_jis");

                fsTeams.Seek(TeamBlockStart, SeekOrigin.Begin);

                int teamIndex = 1;

                while (fsTeams.Position <= TeamBlockEnd)
                {
                    TeamField? nameField = ReadNullTerminatedField(fsTeams);
                    if (nameField == null)
                    {
                        break;
                    }

                    TeamField? romajiField = ReadNullTerminatedField(fsTeams);
                    TeamField? codeField = ReadNullTerminatedField(fsTeams);

                    if (romajiField == null || codeField == null)
                    {
                        break;
                    }

                    string code = Encoding.ASCII.GetString(codeField.Bytes).Trim();

                    TeamCollection.Add(new Team
                    {
                        TeamId = teamIndex++,
                        TeamName = DecodeTeamFieldBytes(nameField.Bytes, shiftJis).Trim(),
                        TeamNameRomaji = DecodeTeamFieldBytes(romajiField.Bytes, shiftJis).Trim(),
                        TeamCode = code,
                        NameOffset = nameField.StartOffset,
                        NameFieldLength = nameField.TotalLength,
                        RomajiOffset = romajiField.StartOffset,
                        RomajiFieldLength = romajiField.TotalLength,
                        CodeOffset = codeField.StartOffset,
                        CodeFieldLength = codeField.TotalLength
                    });

                    // Urawa Reds y Yokohama F・Marinos traen un cuarto campo extra que se descarta.
                    if (TeamsWithExtraField.Contains(code))
                    {
                        ReadNullTerminatedField(fsTeams);
                    }
                }
            }
        }
        public void PopulateTeamsComboBox()
        {
            try
            {
                List<string> teamNames = TeamCollection
                    .Select(t => t.TeamName ?? string.Empty)
                    .ToList();

                team1ComboBox.IsEnabled = true;
                team2ComboBox.IsEnabled = true;
                team1ComboBox.ItemsSource = teamNames;
                team2ComboBox.ItemsSource = teamNames;
                team1ComboBox.SelectedIndex = 0;
                team2ComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading teams: {ex.Message}");
            }
        }

        public void TeamListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (teamListBox.SelectedItem is Team selectedTeam)
            {
                bool isProtected = ProtectedTeamCodes.Contains(selectedTeam.TeamCode ?? string.Empty);

                teamNameTextBox.Text = selectedTeam.TeamName;
                teamNameTextBox.MaxLength = selectedTeam.NameFieldLength;

                teamRomajiTextBox.Text = selectedTeam.TeamNameRomaji;
                teamRomajiTextBox.MaxLength = selectedTeam.RomajiFieldLength;

                teamCodeTextBox.Text = selectedTeam.TeamCode;
                teamCodeTextBox.MaxLength = selectedTeam.CodeFieldLength;
                teamCodeTextBox.IsEnabled = !isProtected;
            }
        }

        private void ApplyTeam_Click(object sender, RoutedEventArgs e)
        {
            if (teamListBox.SelectedItem is not Team selectedTeam)
            {
                return;
            }

            if (string.IsNullOrEmpty(FilePath6))
            {
                MessageBox.Show("No se cargó el archivo SLPM_657.40, no se puede guardar los datos del equipo.");
                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding shiftJis = Encoding.GetEncoding("shift_jis");

            // Los nombres con caracteres acentuados (ej. "Coruña") se guardan en UTF-8 en el
            // archivo original, por lo que al reescribirlos se debe usar la misma codificación
            // para no generar bytes inválidos que luego se muestren como mojibake.
            Encoding utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            bool isProtected = ProtectedTeamCodes.Contains(selectedTeam.TeamCode ?? string.Empty);

            using (FileStream fs = new FileStream(FilePath6, FileMode.Open, FileAccess.Write))
            {
                WriteTeamField(fs, utf8, teamNameTextBox.Text ?? string.Empty, selectedTeam.NameOffset, selectedTeam.NameFieldLength,
                    updatedBytes => selectedTeam.TeamName = DecodeTeamFieldBytes(updatedBytes, shiftJis).TrimEnd('\0'));

                if (!isProtected)
                {
                    WriteTeamField(fs, utf8, teamRomajiTextBox.Text ?? string.Empty, selectedTeam.RomajiOffset, selectedTeam.RomajiFieldLength,
                        updatedBytes => selectedTeam.TeamNameRomaji = DecodeTeamFieldBytes(updatedBytes, shiftJis).TrimEnd('\0'));

                    WriteTeamField(fs, Encoding.ASCII, teamCodeTextBox.Text ?? string.Empty, selectedTeam.CodeOffset, selectedTeam.CodeFieldLength,
                        updatedBytes => selectedTeam.TeamCode = Encoding.ASCII.GetString(updatedBytes).TrimEnd('\0'));
                }
            }

            teamListBox.Items.Refresh();

            MessageBox.Show(isProtected
                ? "Nombre del equipo actualizado. El romaji y la abreviación están bloqueados para este equipo."
                : "Datos del equipo actualizados.");
        }
        private void WriteTeamField(FileStream fs, Encoding encoding, string newValue, long offset, int fieldLength, Action<byte[]> onWritten)
        {
            byte[] valueBytes = encoding.GetBytes(newValue);

            if (valueBytes.Length > fieldLength)
            {
                MessageBox.Show($"El valor excede el espacio disponible en el archivo ({fieldLength} bytes) y será truncado.");
                Array.Resize(ref valueBytes, fieldLength);
            }

            // El resto del campo se rellena con 0x00 (el array ya nace en ceros)
            byte[] fieldBuffer = new byte[fieldLength];
            Array.Copy(valueBytes, fieldBuffer, valueBytes.Length);

            fs.Seek(offset, SeekOrigin.Begin);
            fs.Write(fieldBuffer, 0, fieldBuffer.Length);

            onWritten(fieldBuffer);
        }

        private void OpenFileSelectionWindow_Click(object sender, RoutedEventArgs e)
        {
            FileSelectionWindow fileSelectionWindow = new FileSelectionWindow(this);
            fileSelectionWindow.CheckExistingFiles();
            fileSelectionWindow.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to close the application?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                // Close the application
                Application.Current.Shutdown();
            }
        }

        public List<Player> ParsePlayers(string filePath)
        {
            const int RecordSize = 0x7C; // 124 bytes, coincide con PES4WE8Player

            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fileStream.Seek(124, SeekOrigin.Begin);

                byte[] buffer = new byte[RecordSize];
                int playerIndex = 1;

                while (fileStream.Read(buffer, 0, buffer.Length) == buffer.Length)
                {
                    People.Add(ParsePlayerRecord(buffer, playerIndex++));
                }
            }
            return players;
        }

        private Player ParsePlayerRecord(byte[] buffer, int playerIndex)
        {
            uint block34 = BitConverter.ToUInt32(buffer, 0x34);
            uint block38 = BitConverter.ToUInt32(buffer, 0x38);
            uint block3c = BitConverter.ToUInt32(buffer, 0x3c);
            uint block40 = BitConverter.ToUInt32(buffer, 0x40);
            uint block44 = BitConverter.ToUInt32(buffer, 0x44);
            uint block48 = BitConverter.ToUInt32(buffer, 0x48);
            uint block4c = BitConverter.ToUInt32(buffer, 0x4c);
            uint block50 = BitConverter.ToUInt32(buffer, 0x50);
            uint block54 = BitConverter.ToUInt32(buffer, 0x54);
            uint block58 = BitConverter.ToUInt32(buffer, 0x58);
            uint block5c = BitConverter.ToUInt32(buffer, 0x5c);
            uint block60 = BitConverter.ToUInt32(buffer, 0x60);
            uint block64 = BitConverter.ToUInt32(buffer, 0x64);
            uint block68 = BitConverter.ToUInt32(buffer, 0x68);
            uint block6c = BitConverter.ToUInt32(buffer, 0x6c);
            uint block74 = BitConverter.ToUInt32(buffer, 0x74);
            uint block78 = BitConverter.ToUInt32(buffer, 0x78);

            static uint Bits(uint v, int offset, int count) => BitFieldReader.ReadBits(v, offset, count);

            var player = new Player
            {
                PlayerIndex = playerIndex,
                Name = Encoding.Unicode.GetString(buffer, 0x00, 32).TrimEnd('\0').Trim(),
                ShirtName = Encoding.ASCII.GetString(buffer, 0x20, 16).TrimEnd('\0').Trim(),
                Commentary = BitConverter.ToUInt16(buffer, 0x30),

                // 0x34
                RegPosition = (PlayerPosition)Bits(block34, 0, 4),
                Attack = (int)Bits(block34, 4, 7),
                Defense = (int)Bits(block34, 11, 7),
                Balance = (int)Bits(block34, 18, 7),
                Stamina = (int)Bits(block34, 25, 7),

                // 0x38
                TopSpeed = (int)Bits(block38, 0, 7),
                Acceleration = (int)Bits(block38, 7, 7),
                Response = (int)Bits(block38, 14, 7),
                DribbleAccuracy = (int)Bits(block38, 21, 7),
                CanPlayGK = Bits(block38, 28, 1) != 0,
                CanPlayCBW = Bits(block38, 30, 1) != 0,
                CanPlayCBT = Bits(block38, 31, 1) != 0,

                // 0x3c
                DribbleSpeed = (int)Bits(block3c, 0, 7),
                ShortPassAccuracy = (int)Bits(block3c, 7, 7),
                ShortPassSpeed = (int)Bits(block3c, 14, 7),
                LongPassAccuracy = (int)Bits(block3c, 21, 7),
                CanPlaySB = Bits(block3c, 28, 1) != 0,
                CanPlayDMF = Bits(block3c, 29, 1) != 0,
                CanPlayCMF = Bits(block3c, 30, 1) != 0,
                CanPlaySMF = Bits(block3c, 31, 1) != 0,

                // 0x40
                LongPassSpeed = (int)Bits(block40, 0, 7),
                ShotPower = (int)Bits(block40, 7, 7),
                ShotAccuracy = (int)Bits(block40, 14, 7),
                ShotTechnique = (int)Bits(block40, 21, 7),
                CanPlayAMF = Bits(block40, 28, 1) != 0,
                CanPlayCF = Bits(block40, 29, 1) != 0,
                CanPlayWF = Bits(block40, 30, 1) != 0,
                Dribbling = Bits(block40, 31, 1) != 0,

                // 0x44
                Jump = (int)Bits(block44, 0, 7),
                Header = (int)Bits(block44, 7, 7),
                Technique = (int)Bits(block44, 14, 7),
                Curling = (int)Bits(block44, 21, 7),
                PostPlayer = Bits(block44, 28, 1) != 0,
                Positioning = Bits(block44, 29, 1) != 0,
                Reaction = Bits(block44, 30, 1) != 0,
                Scoring = Bits(block44, 31, 1) != 0,

                // 0x48
                Aggression = (int)Bits(block48, 0, 7),
                Mentality = (int)Bits(block48, 7, 7),
                Condition = (int)Bits(block48, 14, 3),
                Consistency = (int)Bits(block48, 17, 3),
                FreeKickAccuracy = (int)Bits(block48, 20, 7),
                Playmaking = Bits(block48, 27, 1) != 0,
                Passing = Bits(block48, 28, 1) != 0,
                Penalties = Bits(block48, 29, 1) != 0,
                OneOnOneScoring = Bits(block48, 30, 1) != 0,
                LongThrow = Bits(block48, 31, 1) != 0,

                // 0x4c
                TeamWorkAbility = (int)Bits(block4c, 0, 7),
                Agility = (int)Bits(block4c, 7, 7),
                GoalKeepingSkills = (int)Bits(block4c, 14, 7),
                DominantFoot = (Foot)Bits(block4c, 21, 1),
                WeakFootAccuracy = (int)Bits(block4c, 23, 3) + 1,
                WeakFootFrequency = (int)Bits(block4c, 26, 3) + 1,
                FreeKickType = (FreeKickType)Bits(block4c, 29, 3),

                // 0x50
                OneTouchPass = Bits(block50, 2, 1) != 0,
                Marking = Bits(block50, 3, 1) != 0,
                DLineControl = Bits(block50, 4, 1) != 0,
                DirectGKKick = Bits(block50, 5, 1) != 0,
                OneOnOneStopper = Bits(block50, 6, 1) != 0,
                PenaltyStopper = Bits(block50, 7, 1) != 0,
                BothFeetFeint = Bits(block50, 8, 1) != 0,
                Outside = Bits(block50, 9, 1) != 0,
                Injury = (InjuryResistance)Bits(block50, 10, 2),
                SlidingTackle = Bits(block50, 12, 1) != 0,

                // 0x54
                PenaltyKickType = (PenaltyKickType)Bits(block54, 2, 2),
                GoalCelebration1 = (int)Bits(block54, 4, 6),
                GoalCelebration2 = (int)Bits(block54, 11, 6),
                TacticalDribble = Bits(block54, 17, 1) != 0,
                LinePositioning = Bits(block54, 18, 1) != 0,
                Side = Bits(block54, 19, 1) != 0,
                Center = Bits(block54, 20, 1) != 0,

                // 0x58
                Height = (int)Bits(block58, 0, 6) + 148,
                Weight = (int)Bits(block58, 6, 7),
                SkinColour = (SkinColour)Bits(block58, 16, 2),
                HeadHeight = (int)Bits(block58, 18, 4) - 7,
                HeadWidth = (int)Bits(block58, 22, 4) - 7,
                BootsTongueColor = (BootsColor)Bits(block58, 26, 3),
                BootsLineColor2 = (BootsColor)Bits(block58, 29, 3),

                // 0x5c
                HairId = (int)Bits(block5c, 0, 11),
                HairColourPattern = (int)Bits(block5c, 11, 6),
                EyesColor1 = (int)Bits(block5c, 17, 2) + 1,
                EyesColor2 = (EyesColor2)Bits(block5c, 19, 4),
                FacialHairType = (int)Bits(block5c, 23, 7) + 1,
                Sleeves = (SleevesLength)Bits(block5c, 30, 2),

                // 0x60
                FacialHairColour = (int)Bits(block60, 0, 6) + 1,
                SunGlassesType = (int)Bits(block60, 6, 2),
                SunGlassesColour = (int)Bits(block60, 8, 3) + 1,
                MaskType = (int)Bits(block60, 11, 2),
                MaskColour = (int)Bits(block60, 13, 3) + 1,
                CapType = (YesNo)Bits(block60, 16, 1),
                CapColour = (CapColour)Bits(block60, 17, 3),
                WristbandType = (WristbandType)Bits(block60, 20, 2),
                BootsType = (BootsType)Bits(block60, 22, 4),
                BootsBaseColor = (BootsColor)Bits(block60, 26, 3),
                BootsLineColor1 = (BootsColor)Bits(block60, 29, 3),

                // 0x64
                FaceId = (int)Bits(block64, 0, 9) + 1,
                BootsStudsColour = (BootsColor)Bits(block64, 9, 3),
                Tape = (YesNo)Bits(block64, 12, 1),
                HairColorR = (int)Bits(block64, 13, 6) * 4,
                HairColorG = (int)Bits(block64, 19, 6) * 4,
                HairColorB = (int)Bits(block64, 25, 6) * 4,
                Gloves = (GlovesOption)Bits(block64, 31, 1),

                // 0x68
                Socks = (SocksHeight)Bits(block68, 0, 2),
                NeckLength = (int)Bits(block68, 2, 4) - 7,
                ChestMeasurement = (int)Bits(block68, 6, 4) - 7,
                ArmCircumference = (int)Bits(block68, 10, 4) - 7,
                WaisteCircumference = (int)Bits(block68, 14, 4) - 7,
                LegCircumference = (int)Bits(block68, 18, 4) - 7,
                CalfCircumference = (int)Bits(block68, 22, 4) - 7,
                FaceType = (FaceType)Bits(block68, 26, 2),
                LegLength = (int)Bits(block68, 28, 4) - 7,

                // 0x6c
                BandanaColour = (BandanaColour)Bits(block6c, 0, 3),
                ShoulderHeight = (int)Bits(block6c, 3, 4) - 7,
                ShoulderWidth = (int)Bits(block6c, 7, 4) - 7,
                Age = (int)Bits(block6c, 11, 5) + 15,
                Nationality = (int)Bits(block6c, 16, 9),

                // 0x74
                BrowsHeight = 3 - (int)Bits(block74, 0, 3),
                EyesPosition = 3 - (int)Bits(block74, 3, 3),
                EyesAngle = 3 - (int)Bits(block74, 6, 3),
                EyesSize = 3 - (int)Bits(block74, 9, 3),
                NoseHeight = 3 - (int)Bits(block74, 12, 3),
                JawWidth = 3 - (int)Bits(block74, 15, 3),
                JawChin = 3 - (int)Bits(block74, 18, 3),
                NoseWidth = 3 - (int)Bits(block74, 21, 3),
                MouthSize = 3 - (int)Bits(block74, 24, 3),
                MouthThicknessOfLips = (int)Bits(block74, 27, 2) - 1,
                MouthType = (int)Bits(block74, 29, 2) + 1,

                // 0x78
                BrowsTypeAngle = (int)Bits(block78, 0, 6),
                NoseType = (int)Bits(block78, 7, 3) + 1,
                CheeksType = (int)Bits(block78, 11, 3) + 1,
                MouthPosition = 3 - (int)Bits(block78, 14, 3),
                EyesType = (int)Bits(block78, 17, 5) + 1,
                JawType = (int)Bits(block78, 23, 3) + 1,
            };

            player.Country = (int)player.Nationality;
            player.Position = (int)player.RegPosition;

            return player;
        }

        public List<PlayerAssignment> ParsePlayerAssignment(string filePath, string numberFilePath)
        {
            using (FileStream fsPlayerAssignment = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (FileStream fsNumbers = new FileStream(numberFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fsPlayerAssignment.Seek(0, SeekOrigin.Begin);
                byte[] buffer = new byte[0x40];
                List<PlayerAssignment> playerAssignments = new List<PlayerAssignment>();
                int teamIndex = 1;
                int playerIndexWithinTeam = 1;

                while (fsPlayerAssignment.Read(buffer, 0, buffer.Length) > 0)
                {
                    for (int i = 0; i < buffer.Length; i += 2)
                    {
                        byte[] playerBytes = new byte[] { buffer[i], buffer[i + 1] };
                        int player = BitConverter.ToUInt16(playerBytes, 0);

                        // Read the player's number from the numbers file
                        int number = ReadNumber(fsNumbers, teamIndex, playerIndexWithinTeam);

                        // Retrieve the player object from the People collection based on the player index
                        Player? playerObj = People.FirstOrDefault(p => p.PlayerIndex == player);

                        playerAssignments.Add(new PlayerAssignment { Player = playerObj, PlayerIndex = playerIndexWithinTeam, Team = teamIndex, Number = number });
                        playerIndexWithinTeam++;
                        if (playerIndexWithinTeam > 32)
                        {
                            teamIndex++;
                            playerIndexWithinTeam = 1;
                        }
                    }
                }

                return playerAssignments;
            }
        }

        public void ParseAssignment(string filePath, string numberFilePath)
        {
            foreach (PlayerAssignment personA in ParsePlayerAssignment(filePath, numberFilePath))
            {
                PeopleA.Add(personA);
            }
        }

        public List<Callname> ParseCallnames(string editfilePath)
        {
            using (FileStream fsEditOvl = new FileStream(editfilePath, FileMode.Open, FileAccess.Read))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                fsEditOvl.Seek(0x103758, SeekOrigin.Begin); // Start reading from byte 103758
                List<Callname> callnames = new List<Callname>();
                long endPosition = 0x10CA50; // 1101392 in decimal
                Encoding shiftJis = Encoding.GetEncoding("shift_jis");
                int callnameIndex = 1;

                while (fsEditOvl.Position < endPosition)
                {
                    List<byte> bytes = new List<byte>();
                    List<byte> nullbytes = new List<byte>();
                    int b;
                    while ((b = fsEditOvl.ReadByte()) != -1 && b != 0x00)
                    {
                        bytes.Add((byte)b);
                    }

                    // Count consecutive 0x00 bytes
                    while (b == 0x00)
                    {
                        nullbytes.Add((byte)b);
                        b = fsEditOvl.ReadByte();
                    }

                    // Put back the last byte read if it is not 0x00 and is not -1
                    if (b != -1 && b != 0x00)
                    {
                        fsEditOvl.Position -= 1;
                    }

                    if (bytes.Count > 0)
                    {
                        // Convert the bytes to Shift-JIS string
                        string callnameStr = shiftJis.GetString(bytes.ToArray()).Trim();

                        // Add to the collection
                        Callname callname = new Callname
                        {
                            CommentaryIndex = callnameIndex++,
                            CommentaryName = callnameStr,
                            CommentaryCode = "NODATA",
                            CommentaryBytes = bytes.Count + nullbytes.Count
                        };
                        CallnameCollection.Add(callname);
                    }
                }

                return callnames;
            }
        }

        public List<Stadium> ParseStadiums(string datasetFilePath)
        {
            using (FileStream fsDataset = new FileStream(datasetFilePath, FileMode.Open, FileAccess.Read))
            {
                fsDataset.Seek(0x84030, SeekOrigin.Begin);
                byte[] buffer = new byte[0x3D]; // 61 bytes
                List<Stadium> stadiums = new List<Stadium>();
                int stadiumIndex = 1;
                long startPosition = fsDataset.Position;
                long endPosition = 0x8497B;

                while (fsDataset.Read(buffer, 0, buffer.Length) == buffer.Length && fsDataset.Position <= endPosition)
                {
                    Stadium stadium = new Stadium
                    {
                        StadiumIndex = stadiumIndex++,
                        StadiumName = Encoding.UTF8.GetString(buffer, 0, 61).Trim(), // Assuming the name takes the first 61 bytes
                        Seats = 0,
                        Zone = 0,
                        Year = 0
                    };
                    StadiumCollection.Add(stadium);
                }

                return stadiums;
            }
        }

        private int ReadNumber(FileStream fsNumbers, int teamIndex, int playerIndexWithinTeam)
        {
            // Assuming each team has 32 players and each player's number is 1 byte
            int totalPlayerIndex = (teamIndex - 1) * 32 + playerIndexWithinTeam;
            fsNumbers.Seek(totalPlayerIndex - 1, SeekOrigin.Begin); // Assuming playerIndexWithinTeam is 1-based
            return fsNumbers.ReadByte() + 1;
        }

        public void PersonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (personListBox.SelectedItem != null)
            {
                Player selectedPlayer = (Player)personListBox.SelectedItem;

                if (selectedPlayer != null)
                {
                    nameTextBox.Text = selectedPlayer.Name;
                    commentaryComboBox.SelectedIndex = selectedPlayer.Commentary;
                    countryComboBox.SelectedIndex = selectedPlayer.Country - 121;

                    PlayerAssignment? assignment = PeopleA.FirstOrDefault(a => a.Player == selectedPlayer);
                    UpdatePlayerDetailPanels(selectedPlayer, assignment);
                }
            }
        }

        private void UpdatePlayerDetailPanels(Player selectedPlayer, PlayerAssignment? assignment)
        {
            jerseyTextBox.Text = assignment?.Number.ToString() ?? string.Empty;

            Team? team = assignment != null
                ? TeamCollection.FirstOrDefault(t => t.TeamId == assignment.Team)
                : null;
            currentTeamTextBox.Text = team?.TeamName ?? string.Empty;

            ageTextBox.Text = selectedPlayer.Age.ToString();
            heightTextBox.Text = selectedPlayer.Height.ToString();
            weightTextBox.Text = selectedPlayer.Weight.ToString();
            positionTextBox.Text = PlayerWindow.Positions
                .FirstOrDefault(p => p.PositionId == selectedPlayer.Position)?.PositionName ?? string.Empty;
        }

        private void SavePlayerData(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                foreach (var player in People)
                {
                    if (player != null)
                    {
                        // Calculate the byte offset in the file based on the player's index
                        long byteOffset = player.PlayerIndex * 0x7C; // 0x7C is the size of each player record

                        // Convert the byte offset to a bit offset and add 124 (15 bytes * 8 bits per byte) for the initial offset
                        long bitOffset = byteOffset * 8; // Start writing at bit 124

                        // Return to the beginning of the file to write the changes
                        fileStream.Seek(0, SeekOrigin.Begin);

                        using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode, true))
                        {
                            // Set the file stream position to the bit offset divided by 8 to write at the correct byte position
                            fileStream.Seek(bitOffset / 8, SeekOrigin.Begin);
                            byte[] nameBytes = Encoding.Unicode.GetBytes((player.Name ?? string.Empty).PadRight(12, '\0'));
                            writer.Write(nameBytes);

                            fileStream.Seek((bitOffset + 32 * 8) / 8, SeekOrigin.Begin);
                            byte[] shirtNameBytes = Encoding.ASCII.GetBytes((player.ShirtName ?? string.Empty).PadRight(16, '\0').Substring(0, 16));
                            writer.Write(shirtNameBytes);

                            // Calculate the byte position based on the bit offset
                            int position = (int)(bitOffset + 52 * 8) / 8;

                            // Read the byte at the position
                            fileStream.Seek(position, SeekOrigin.Begin);
                            byte b = (byte)(fileStream.ReadByte() & 0xF0);  // Clear the lower nibble

                            // Set the position bytes
                            byte[] positionBytes = BitConverter.GetBytes((int)player.Position);
                            byte buffer52 = (byte)((positionBytes[0]) & 0x0F);

                            b |= buffer52;

                            // Write the updated position byte
                            fileStream.Seek(position, SeekOrigin.Begin);
                            fileStream.WriteByte(b);

                            fileStream.Seek((int)(bitOffset + 110 * 8) / 8, SeekOrigin.Begin);
                            writer.Write(player.Country);
                        }
                    }
                }
                fileStream.Flush();
                fileStream.Close();
                MessageBoxResult result = MessageBox.Show("Data saved successfully.", "Save Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void SavePlayerAssignmentData(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                foreach (var playerAssignment in PeopleA)
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode, true))
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        byte[] playersAssignment = BitConverter.GetBytes(playerAssignment.Player?.PlayerIndex ?? 0);
                        writer.Write(playersAssignment);
                    }

                }
            }
        }

        public void SaveNumberData(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {

            }
        }

        public void SaveCallnameData(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                fileStream.Seek(0x103758, SeekOrigin.Begin);
                Encoding shiftJis = Encoding.GetEncoding("shift_jis");

                foreach (var callnameList in CallnameCollection)
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode, true))
                    {
                        byte[] callnameBytes = shiftJis.GetBytes(callnameList.CommentaryName ?? string.Empty);
                        int totalLength = callnameList.CommentaryBytes ?? 0; // The length defined in CommentaryLength
                        byte[] paddedBytes = new byte[totalLength];

                        // Copy callnameBytes into paddedBytes
                        Array.Copy(callnameBytes, paddedBytes, callnameBytes.Length);

                        // The rest of paddedBytes is already filled with 0x00 by default

                        writer.Write(paddedBytes);
                    }

                }
            }
        }

        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
            SaveAllPlayersData(FilePath1);
            //SavePlayerAssignmentData(FilePath2);
            //SaveNumberData(FilePath3);
            BackupFileIfExists(FilePath4);
            SaveCallnameData(FilePath4);
            //People.Clear();
            //PeopleA.Clear();
            //ParsePlayers(FilePath1);
            //ParsePlayerAssignment(FilePath2, FilePath3);
            //ParseAssignment(FilePath2, FilePath3);
            //updatePeopleList();
            //team1DataGrid.Items.Refresh();
            //team2DataGrid.Items.Refresh();
            //personListBox.Items.Refresh();

            MessageBox.Show("Data saved successfully.", "Save Confirmation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearFilter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(PeopleA);
            view.Filter = null;
        }

        private void team1ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterPeopleAByTeamId(team1ComboBox, team1DataGrid);
            team1DataGrid.IsEnabled = true;
        }

        private void team2ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterPeopleAByTeamId(team2ComboBox, team2DataGrid);
            team2DataGrid.IsEnabled = true;
        }

        private void FilterPeopleAByTeamId(ComboBox teamCombobox, DataGrid dataGrid)
        {
            int selectedTeamId = teamCombobox.SelectedIndex + 1;
            var filteredPeopleA = PeopleA.Where(player => player.Team == selectedTeamId)
                .OrderBy(player => player.PlayerIndex)
                .ToList();
            dataGrid.ItemsSource = filteredPeopleA;
        }

        private Point dragStartPoint;

        /// <summary>
        /// Intercambia el jugador (Player) y su número de camiseta (Number) entre dos
        /// slots. Los campos Team/PlayerIndex de cada PlayerAssignment no se modifican,
        /// ya que representan la posición física fija dentro del archivo binario
        /// (cada slot tiene su propio byte reservado para el número de camiseta).
        /// </summary>
        private void SwapPlayerAssignments(PlayerAssignment first, PlayerAssignment second)
        {
            (first.Player, second.Player) = (second.Player, first.Player);
            (first.Number, second.Number) = (second.Number, first.Number);
        }

        private static DataGridRow? FindParentRow(DependencyObject? source)
        {
            while (source != null && source is not DataGridRow)
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source as DataGridRow;
        }

        private void PlayerAssignmentGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            dragStartPoint = e.GetPosition(null);
        }

        private void PlayerAssignmentGrid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || sender is not DataGrid dataGrid)
            {
                return;
            }

            Point currentPosition = e.GetPosition(null);
            Vector diff = dragStartPoint - currentPosition;

            if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            DataGridRow? row = FindParentRow(e.OriginalSource as DependencyObject);
            if (row == null || row.Item is not PlayerAssignment assignment)
            {
                return;
            }

            var dragData = new DataObject(typeof(PlayerAssignment), assignment);
            DragDrop.DoDragDrop(dataGrid, dragData, DragDropEffects.Move);
        }

        private void PlayerAssignmentGrid_Drop(object sender, DragEventArgs e)
        {
            if (sender is not DataGrid targetDataGrid || !e.Data.GetDataPresent(typeof(PlayerAssignment)))
            {
                return;
            }

            if (e.Data.GetData(typeof(PlayerAssignment)) is not PlayerAssignment sourceAssignment)
            {
                return;
            }

            DataGridRow? targetRow = FindParentRow(e.OriginalSource as DependencyObject);
            if (targetRow == null || targetRow.Item is not PlayerAssignment targetAssignment)
            {
                return;
            }

            if (ReferenceEquals(sourceAssignment, targetAssignment))
            {
                return;
            }

            bool sameTeam = sourceAssignment.Team == targetAssignment.Team;

            // Si el equipo de origen está cargado en ambas listas (team1 y team2 muestran el
            // mismo equipo), no se permite transferir jugadores entre slots de distinto
            // "Team" aunque se suelten en el otro DataGrid: solo se permite reordenar dentro
            // del mismo equipo en ese caso.
            bool bothListsShowSameTeam = team1ComboBox.SelectedIndex == team2ComboBox.SelectedIndex;

            if (!sameTeam && bothListsShowSameTeam)
            {
                return;
            }

            SwapPlayerAssignments(sourceAssignment, targetAssignment);

            team1DataGrid.Items.Refresh();
            team2DataGrid.Items.Refresh();
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void OpenPlayer(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox personListBox && personListBox.SelectedItem != null)
            {

                Player selectedPlayer = (Player)personListBox.SelectedItem;
                PlayerWindow playerWindow = new PlayerWindow(selectedPlayer, this);
                playerWindow.Show();
            }
        }

        public void OpenPlayerFromDataGrid(DataGrid dataGrid)
        {
            PlayerAssignment selectedPlayerAssignment = (PlayerAssignment)dataGrid.SelectedItem;
            Player? selectedPlayer = selectedPlayerAssignment.Player;
            if (selectedPlayer == null)
            {
                return;
            }
            PlayerWindow playerWindow = new PlayerWindow(selectedPlayer, this, selectedPlayerAssignment);
            playerWindow.ShowDialog();
        }

        public void Team1DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItems.Count == 1)
            {
                if (dataGrid.SelectedItem != null)
                {
                    OpenPlayerFromDataGrid(dataGrid);
                }
            }
        }

        public void Team2DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItems.Count == 1)
            {
                if (dataGrid.SelectedItem != null)
                {
                    OpenPlayerFromDataGrid(dataGrid);
                }
            }
        }

        private void team1DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataGridSelectedPlayer(sender);
        }

        private void team2DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataGridSelectedPlayer(sender);
        }

        public void dataGridSelectedPlayer(object sender)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItems.Count == 1)
            {
                PlayerAssignment selectedPlayerAssignment = (PlayerAssignment)dataGrid.SelectedItem;

                if (selectedPlayerAssignment.Player != null)
                {
                    Player selectedPlayer = selectedPlayerAssignment.Player;
                    nameTextBox.Text = selectedPlayer.Name;
                    commentaryComboBox.SelectedIndex = selectedPlayer.Commentary;
                    countryComboBox.SelectedIndex = selectedPlayer.Country - 121;

                    UpdatePlayerDetailPanels(selectedPlayer, selectedPlayerAssignment);
                }

            }
        }

        public void updatePeopleList()
        {
            People.CollectionChanged += People_CollectionChanged;
            PeopleA.CollectionChanged += PeopleA_CollectionChanged;
        }

        private void People_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Player player in e.NewItems!)
                {
                    // Check if the player exists in PeopleA
                    PlayerAssignment? assignment = PeopleA.FirstOrDefault(a => a.PlayerIndex == player.PlayerIndex);
                    if (assignment != null)
                    {
                        assignment.Player = player;
                        // Update other properties of assignment if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Player player in e.OldItems!)
                {
                    // Remove the player from PeopleA
                    PlayerAssignment? assignment = PeopleA.FirstOrDefault(a => a.PlayerIndex == player.PlayerIndex);
                    if (assignment != null)
                    {
                        assignment.Player = null;
                        // Update other properties of assignment if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (Player player in e.NewItems!)
                {
                    // Update the player in PeopleA
                    PlayerAssignment? assignment = PeopleA.FirstOrDefault(a => a.PlayerIndex == player.PlayerIndex);
                    if (assignment != null)
                    {
                        assignment.Player = player;
                        // Update other properties of assignment if needed
                    }
                }
            }
        }

        private void PeopleA_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PlayerAssignment assignment in e.NewItems!)
                {
                    // Check if the assignment's player exists in People
                    Player? player = People.FirstOrDefault(p => p.PlayerIndex == assignment.PlayerIndex);
                    if (player != null)
                    {
                        player = assignment.Player;
                        // Update other properties of player if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PlayerAssignment assignment in e.OldItems!)
                {
                    // Remove the player from People
                    Player? player = People.FirstOrDefault(p => p.PlayerIndex == assignment.PlayerIndex);
                    if (player != null)
                    {
                        player = null;
                        // Update other properties of player if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (PlayerAssignment assignment in e.NewItems!)
                {
                    // Update the player in People
                    Player? player = People.FirstOrDefault(p => p.PlayerIndex == assignment.PlayerIndex);
                    if (player != null)
                    {
                        player = assignment.Player;
                        // Update other properties of player if needed
                    }
                }
            }
        }

        private void StadiumListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (stadiumListBox.SelectedItem is Stadium selectedStadium)
            {
                stadiumNameTextBox.Text = selectedStadium.StadiumName;
            }
        }

        private void ApplyStadium_Click(object sender, RoutedEventArgs e)
        {
            if (stadiumListBox.SelectedItem is Stadium selectedStadium)
            {
                selectedStadium.StadiumName = stadiumNameTextBox.Text;
                WriteStadiumName(FilePath4, selectedStadium.StadiumIndex, stadiumNameTextBox.Text);
                stadiumListBox.Items.Refresh();
            }
        }


        private void CallnameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Callname selectedPlayerAssignment = (Callname)callnameListBox.SelectedItem;
            callnameTextBoxC.Text = selectedPlayerAssignment.CommentaryName;
            callnameFileTexBoxC.Text = selectedPlayerAssignment.CommentaryCode;
            Callname_Information.Header = "Callname #" + selectedPlayerAssignment.CommentaryIndex + " (" + selectedPlayerAssignment.CommentaryBytes + " bytes)";
        }


        private void ApplyCallname_Click(object sender, RoutedEventArgs e)
        {
            Callname selectedPlayerAssignment = (Callname)callnameListBox.SelectedItem;
            selectedPlayerAssignment.CommentaryName = callnameTextBoxC.Text;
            callnameListBox.Items.Refresh();
        }

        private void TestParaWe8_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"D:\PES\PlayStation 2\Traducciones\Traducción JWE8\BOOT\BOOT_0\para_we8.bin";

                string info = JWE8Editor.Logic.ParaWe8Reader.GetRawFileInfo(path, 128);

                MessageBox.Show(info, "ParaWe8 Test Result");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n\n{ex.StackTrace}", "ParaWe8 Test Error");
            }
        }

        private const int StadiumRecordSize = 0x3D; // 61 bytes
        private const long StadiumBlockStart = 0x84030;
        private const long StadiumBlockEnd = 0x8497B;

        /// <summary>
        /// Escribe el nombre actualizado de un estadio en el archivo de dataset,
        /// respetando el límite de bytes del registro (61 bytes) y evitando
        /// corromper el registro siguiente.
        /// </summary>
        public void WriteStadiumName(string datasetFilePath, int stadiumIndex, string newName)
        {
            long offset = StadiumBlockStart + ((stadiumIndex - 1) * StadiumRecordSize);

            if (offset + StadiumRecordSize > StadiumBlockEnd + StadiumRecordSize)
            {
                MessageBox.Show("Índice de estadio fuera de rango.");
                return;
            }

            byte[] nameBytes = Encoding.UTF8.GetBytes(newName);

            // Reservamos al menos 1 byte para el terminador nulo final del registro
            const int maxNameBytes = StadiumRecordSize - 1;

            if (nameBytes.Length > maxNameBytes)
            {
                MessageBox.Show($"El nombre excede el límite de {maxNameBytes} bytes y será truncado.");
                Array.Resize(ref nameBytes, maxNameBytes);
            }

            byte[] record = new byte[StadiumRecordSize]; // ya queda en 0x00 por defecto
            Array.Copy(nameBytes, record, nameBytes.Length);

            using (FileStream fs = new FileStream(datasetFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(record, 0, record.Length);
            }
        }

        public List<League> ParseLeagues(string datasetFilePath)
        {
            var leagues = new List<League>();

            using (FileStream fsDataset = new FileStream(datasetFilePath, FileMode.Open, FileAccess.Read))
            {
                fsDataset.Seek(LeagueBlockStart, SeekOrigin.Begin);
                byte[] buffer = new byte[LeagueRecordSize];
                int leagueIndex = 1;

                while (fsDataset.Read(buffer, 0, buffer.Length) == buffer.Length && fsDataset.Position <= LeagueBlockEnd)
                {
                    // El nombre está duplicado al inicio del registro; tomamos la primera copia
                    string name = Encoding.UTF8.GetString(buffer, 0, LeagueNameFieldSize).TrimEnd('\0').Trim();

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        leagues.Add(new League
                        {
                            LeagueIndex = leagueIndex++,
                            LeagueName = name,
                            CountryCode = 0 // la tabla de códigos está en otro bloque; mapear por separado si es necesario
                        });

                        LeagueCollection.Add(leagues[^1]);
                    }
                }
            }

            return leagues;
        }

        public void WriteLeagueName(string datasetFilePath, int leagueIndex, string newName)
        {
            long offset = LeagueBlockStart + ((leagueIndex - 1) * LeagueRecordSize);
            const int maxNameBytes = LeagueNameFieldSize - 1; // reservar terminador nulo

            byte[] nameBytes = Encoding.UTF8.GetBytes(newName);

            if (nameBytes.Length > maxNameBytes)
            {
                MessageBox.Show($"El nombre excede el límite de {maxNameBytes} bytes y será truncado.");
                Array.Resize(ref nameBytes, maxNameBytes);
            }

            byte[] nameField = new byte[LeagueNameFieldSize];
            Array.Copy(nameBytes, nameField, nameBytes.Length);

            using (FileStream fs = new FileStream(datasetFilePath, FileMode.Open, FileAccess.ReadWrite))
            {
                // Se escribe en ambas copias del nombre (patrón observado: nombre duplicado)
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Write(nameField, 0, nameField.Length);
                fs.Write(nameField, 0, nameField.Length);
            }
        }

        private void LeagueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (leagueListBox.SelectedItem is League selectedLeague)
            {
                leagueNameTextBox.Text = selectedLeague.LeagueName;
            }
        }

        private void ApplyLeague_Click(object sender, RoutedEventArgs e)
        {
            if (leagueListBox.SelectedItem is League selectedLeague)
            {
                selectedLeague.LeagueName = leagueNameTextBox.Text;
                WriteLeagueName(FilePath4, selectedLeague.LeagueIndex, leagueNameTextBox.Text);
                leagueListBox.Items.Refresh();
            }
        }

        public void WritePlayerRecord(byte[] buffer, Player player)
        {
            static void Pack(byte[] buf, int offset, ref uint block, int bitOffset, int bitCount, uint fieldValue)
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
            Pack(buffer, 0x34, ref block34, 0, 4, PlayerFieldValidator.ClampEnum(player.RegPosition, 4));
            Pack(buffer, 0x34, ref block34, 4, 7, PlayerFieldValidator.ClampRawRange(player.Attack, 7));
            Pack(buffer, 0x34, ref block34, 11, 7, PlayerFieldValidator.ClampRawRange(player.Defense, 7));
            Pack(buffer, 0x34, ref block34, 18, 7, PlayerFieldValidator.ClampRawRange(player.Balance, 7));
            Pack(buffer, 0x34, ref block34, 25, 7, PlayerFieldValidator.ClampRawRange(player.Stamina, 7));
            BitConverter.GetBytes(block34).CopyTo(buffer, 0x34);

            // --- 0x38 ---
            uint block38 = 0;
            Pack(buffer, 0x38, ref block38, 0, 7, PlayerFieldValidator.ClampRawRange(player.TopSpeed, 7));
            Pack(buffer, 0x38, ref block38, 7, 7, PlayerFieldValidator.ClampRawRange(player.Acceleration, 7));
            Pack(buffer, 0x38, ref block38, 14, 7, PlayerFieldValidator.ClampRawRange(player.Response, 7));
            Pack(buffer, 0x38, ref block38, 21, 7, PlayerFieldValidator.ClampRawRange(player.DribbleAccuracy, 7));
            Pack(buffer, 0x38, ref block38, 28, 1, PlayerFieldValidator.ClampBool(player.CanPlayGK));
            Pack(buffer, 0x38, ref block38, 30, 1, PlayerFieldValidator.ClampBool(player.CanPlayCBW));
            Pack(buffer, 0x38, ref block38, 31, 1, PlayerFieldValidator.ClampBool(player.CanPlayCBT));
            BitConverter.GetBytes(block38).CopyTo(buffer, 0x38);

            // --- 0x3c ---
            uint block3c = 0;
            Pack(buffer, 0x3c, ref block3c, 0, 7, PlayerFieldValidator.ClampRawRange(player.DribbleSpeed, 7));
            Pack(buffer, 0x3c, ref block3c, 7, 7, PlayerFieldValidator.ClampRawRange(player.ShortPassAccuracy, 7));
            Pack(buffer, 0x3c, ref block3c, 14, 7, PlayerFieldValidator.ClampRawRange(player.ShortPassSpeed, 7));
            Pack(buffer, 0x3c, ref block3c, 21, 7, PlayerFieldValidator.ClampRawRange(player.LongPassAccuracy, 7));
            Pack(buffer, 0x3c, ref block3c, 28, 1, PlayerFieldValidator.ClampBool(player.CanPlaySB));
            Pack(buffer, 0x3c, ref block3c, 29, 1, PlayerFieldValidator.ClampBool(player.CanPlayDMF));
            Pack(buffer, 0x3c, ref block3c, 30, 1, PlayerFieldValidator.ClampBool(player.CanPlayCMF));
            Pack(buffer, 0x3c, ref block3c, 31, 1, PlayerFieldValidator.ClampBool(player.CanPlaySMF));
            BitConverter.GetBytes(block3c).CopyTo(buffer, 0x3c);

            // --- 0x40 ---
            uint block40 = 0;
            Pack(buffer, 0x40, ref block40, 0, 7, PlayerFieldValidator.ClampRawRange(player.LongPassSpeed, 7));
            Pack(buffer, 0x40, ref block40, 7, 7, PlayerFieldValidator.ClampRawRange(player.ShotPower, 7));
            Pack(buffer, 0x40, ref block40, 14, 7, PlayerFieldValidator.ClampRawRange(player.ShotAccuracy, 7));
            Pack(buffer, 0x40, ref block40, 21, 7, PlayerFieldValidator.ClampRawRange(player.ShotTechnique, 7));
            Pack(buffer, 0x40, ref block40, 28, 1, PlayerFieldValidator.ClampBool(player.CanPlayAMF));
            Pack(buffer, 0x40, ref block40, 29, 1, PlayerFieldValidator.ClampBool(player.CanPlayCF));
            Pack(buffer, 0x40, ref block40, 30, 1, PlayerFieldValidator.ClampBool(player.CanPlayWF));
            Pack(buffer, 0x40, ref block40, 31, 1, PlayerFieldValidator.ClampBool(player.Dribbling));
            BitConverter.GetBytes(block40).CopyTo(buffer, 0x40);

            // --- 0x44 ---
            uint block44 = 0;
            Pack(buffer, 0x44, ref block44, 0, 7, PlayerFieldValidator.ClampRawRange(player.Jump, 7));
            Pack(buffer, 0x44, ref block44, 7, 7, PlayerFieldValidator.ClampRawRange(player.Header, 7));
            Pack(buffer, 0x44, ref block44, 14, 7, PlayerFieldValidator.ClampRawRange(player.Technique, 7));
            Pack(buffer, 0x44, ref block44, 21, 7, PlayerFieldValidator.ClampRawRange(player.Curling, 7));
            Pack(buffer, 0x44, ref block44, 28, 1, PlayerFieldValidator.ClampBool(player.PostPlayer));
            Pack(buffer, 0x44, ref block44, 29, 1, PlayerFieldValidator.ClampBool(player.Positioning));
            Pack(buffer, 0x44, ref block44, 30, 1, PlayerFieldValidator.ClampBool(player.Reaction));
            Pack(buffer, 0x44, ref block44, 31, 1, PlayerFieldValidator.ClampBool(player.Scoring));
            BitConverter.GetBytes(block44).CopyTo(buffer, 0x44);

            // --- 0x48 ---
            uint block48 = 0;
            Pack(buffer, 0x48, ref block48, 0, 7, PlayerFieldValidator.ClampRawRange(player.Aggression, 7));
            Pack(buffer, 0x48, ref block48, 7, 7, PlayerFieldValidator.ClampRawRange(player.Mentality, 7));
            Pack(buffer, 0x48, ref block48, 14, 3, PlayerFieldValidator.ClampRawRange(player.Condition, 3));
            Pack(buffer, 0x48, ref block48, 17, 3, PlayerFieldValidator.ClampRawRange(player.Consistency, 3));
            Pack(buffer, 0x48, ref block48, 20, 7, PlayerFieldValidator.ClampRawRange(player.FreeKickAccuracy, 7));
            Pack(buffer, 0x48, ref block48, 27, 1, PlayerFieldValidator.ClampBool(player.Playmaking));
            Pack(buffer, 0x48, ref block48, 28, 1, PlayerFieldValidator.ClampBool(player.Passing));
            Pack(buffer, 0x48, ref block48, 29, 1, PlayerFieldValidator.ClampBool(player.Penalties));
            Pack(buffer, 0x48, ref block48, 30, 1, PlayerFieldValidator.ClampBool(player.OneOnOneScoring));
            Pack(buffer, 0x48, ref block48, 31, 1, PlayerFieldValidator.ClampBool(player.LongThrow));
            BitConverter.GetBytes(block48).CopyTo(buffer, 0x48);

            // --- 0x4c ---
            uint block4c = 0;
            Pack(buffer, 0x4c, ref block4c, 0, 7, PlayerFieldValidator.ClampRawRange(player.TeamWorkAbility, 7));
            Pack(buffer, 0x4c, ref block4c, 7, 7, PlayerFieldValidator.ClampRawRange(player.Agility, 7));
            Pack(buffer, 0x4c, ref block4c, 14, 7, PlayerFieldValidator.ClampRawRange(player.GoalKeepingSkills, 7));
            Pack(buffer, 0x4c, ref block4c, 21, 1, PlayerFieldValidator.ClampEnum(player.DominantFoot, 1));
            Pack(buffer, 0x4c, ref block4c, 23, 3, PlayerFieldValidator.ClampPlus1(player.WeakFootAccuracy, 3));
            Pack(buffer, 0x4c, ref block4c, 26, 3, PlayerFieldValidator.ClampPlus1(player.WeakFootFrequency, 3));
            Pack(buffer, 0x4c, ref block4c, 29, 3, PlayerFieldValidator.ClampEnum(player.FreeKickType, 3));
            BitConverter.GetBytes(block4c).CopyTo(buffer, 0x4c);

            // --- 0x50 ---
            uint block50 = 0;
            Pack(buffer, 0x50, ref block50, 2, 1, PlayerFieldValidator.ClampBool(player.OneTouchPass));
            Pack(buffer, 0x50, ref block50, 3, 1, PlayerFieldValidator.ClampBool(player.Marking));
            Pack(buffer, 0x50, ref block50, 4, 1, PlayerFieldValidator.ClampBool(player.DLineControl));
            Pack(buffer, 0x50, ref block50, 5, 1, PlayerFieldValidator.ClampBool(player.DirectGKKick));
            Pack(buffer, 0x50, ref block50, 6, 1, PlayerFieldValidator.ClampBool(player.OneOnOneStopper));
            Pack(buffer, 0x50, ref block50, 7, 1, PlayerFieldValidator.ClampBool(player.PenaltyStopper));
            Pack(buffer, 0x50, ref block50, 8, 1, PlayerFieldValidator.ClampBool(player.BothFeetFeint));
            Pack(buffer, 0x50, ref block50, 9, 1, PlayerFieldValidator.ClampBool(player.Outside));
            Pack(buffer, 0x50, ref block50, 10, 2, PlayerFieldValidator.ClampEnum(player.Injury, 2));
            Pack(buffer, 0x50, ref block50, 12, 1, PlayerFieldValidator.ClampBool(player.SlidingTackle));
            BitConverter.GetBytes(block50).CopyTo(buffer, 0x50);

            // --- 0x54 ---
            uint block54 = 0;
            Pack(buffer, 0x54, ref block54, 2, 2, PlayerFieldValidator.ClampEnum(player.PenaltyKickType, 2));
            Pack(buffer, 0x54, ref block54, 4, 6, PlayerFieldValidator.ClampRawRange(player.GoalCelebration1, 6));
            Pack(buffer, 0x54, ref block54, 11, 6, PlayerFieldValidator.ClampRawRange(player.GoalCelebration2, 6));
            Pack(buffer, 0x54, ref block54, 17, 1, PlayerFieldValidator.ClampBool(player.TacticalDribble));
            Pack(buffer, 0x54, ref block54, 18, 1, PlayerFieldValidator.ClampBool(player.LinePositioning));
            Pack(buffer, 0x54, ref block54, 19, 1, PlayerFieldValidator.ClampBool(player.Side));
            Pack(buffer, 0x54, ref block54, 20, 1, PlayerFieldValidator.ClampBool(player.Center));
            BitConverter.GetBytes(block54).CopyTo(buffer, 0x54);

            // --- 0x58 ---
            uint block58 = 0;
            Pack(buffer, 0x58, ref block58, 0, 6, PlayerFieldValidator.ClampHeight(player.Height));
            Pack(buffer, 0x58, ref block58, 6, 7, PlayerFieldValidator.ClampRawRange(player.Weight, 7));
            Pack(buffer, 0x58, ref block58, 16, 2, PlayerFieldValidator.ClampEnum(player.SkinColour, 2));
            Pack(buffer, 0x58, ref block58, 18, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.HeadHeight, 4));
            Pack(buffer, 0x58, ref block58, 22, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.HeadWidth, 4));
            Pack(buffer, 0x58, ref block58, 26, 3, PlayerFieldValidator.ClampEnum(player.BootsTongueColor, 3));
            Pack(buffer, 0x58, ref block58, 29, 3, PlayerFieldValidator.ClampEnum(player.BootsLineColor2, 3));
            BitConverter.GetBytes(block58).CopyTo(buffer, 0x58);

            // --- 0x5c ---
            uint block5c = 0;
            Pack(buffer, 0x5c, ref block5c, 0, 11, PlayerFieldValidator.ClampRawRange(player.HairId, 11));
            Pack(buffer, 0x5c, ref block5c, 11, 6, PlayerFieldValidator.ClampRawRange(player.HairColourPattern, 6));
            Pack(buffer, 0x5c, ref block5c, 17, 2, PlayerFieldValidator.ClampPlus1(player.EyesColor1, 2));
            Pack(buffer, 0x5c, ref block5c, 19, 4, PlayerFieldValidator.ClampEnum(player.EyesColor2, 4));
            Pack(buffer, 0x5c, ref block5c, 23, 7, PlayerFieldValidator.ClampPlus1(player.FacialHairType, 7));
            Pack(buffer, 0x5c, ref block5c, 30, 2, PlayerFieldValidator.ClampEnum(player.Sleeves, 2));
            BitConverter.GetBytes(block5c).CopyTo(buffer, 0x5c);

            // --- 0x60 ---
            uint block60 = 0;
            Pack(buffer, 0x60, ref block60, 0, 6, PlayerFieldValidator.ClampPlus1(player.FacialHairColour, 6));
            Pack(buffer, 0x60, ref block60, 6, 2, PlayerFieldValidator.ClampRawRange(player.SunGlassesType, 2));
            Pack(buffer, 0x60, ref block60, 8, 3, PlayerFieldValidator.ClampPlus1(player.SunGlassesColour, 3));
            Pack(buffer, 0x60, ref block60, 11, 2, PlayerFieldValidator.ClampRawRange(player.MaskType, 2));
            Pack(buffer, 0x60, ref block60, 13, 3, PlayerFieldValidator.ClampPlus1(player.MaskColour, 3));
            Pack(buffer, 0x60, ref block60, 16, 1, PlayerFieldValidator.ClampEnum(player.CapType, 1));
            Pack(buffer, 0x60, ref block60, 17, 3, PlayerFieldValidator.ClampEnum(player.CapColour, 3));
            Pack(buffer, 0x60, ref block60, 20, 2, PlayerFieldValidator.ClampEnum(player.WristbandType, 2));
            Pack(buffer, 0x60, ref block60, 22, 4, PlayerFieldValidator.ClampEnum(player.BootsType, 4));
            Pack(buffer, 0x60, ref block60, 26, 3, PlayerFieldValidator.ClampEnum(player.BootsBaseColor, 3));
            Pack(buffer, 0x60, ref block60, 29, 3, PlayerFieldValidator.ClampEnum(player.BootsLineColor1, 3));
            BitConverter.GetBytes(block60).CopyTo(buffer, 0x60);

            // --- 0x64 ---
            uint block64 = 0;
            Pack(buffer, 0x64, ref block64, 0, 9, PlayerFieldValidator.ClampPlus1(player.FaceId, 9));
            Pack(buffer, 0x64, ref block64, 9, 3, PlayerFieldValidator.ClampEnum(player.BootsStudsColour, 3));
            Pack(buffer, 0x64, ref block64, 12, 1, PlayerFieldValidator.ClampEnum(player.Tape, 1));
            Pack(buffer, 0x64, ref block64, 13, 6, PlayerFieldValidator.ClampMultiplyBy4(player.HairColorR, 6));
            Pack(buffer, 0x64, ref block64, 19, 6, PlayerFieldValidator.ClampMultiplyBy4(player.HairColorG, 6));
            Pack(buffer, 0x64, ref block64, 25, 6, PlayerFieldValidator.ClampMultiplyBy4(player.HairColorB, 6));
            Pack(buffer, 0x64, ref block64, 31, 1, PlayerFieldValidator.ClampEnum(player.Gloves, 1));
            BitConverter.GetBytes(block64).CopyTo(buffer, 0x64);

            // --- 0x68 ---
            uint block68 = 0;
            Pack(buffer, 0x68, ref block68, 0, 2, PlayerFieldValidator.ClampEnum(player.Socks, 2));
            Pack(buffer, 0x68, ref block68, 2, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.NeckLength, 4));
            Pack(buffer, 0x68, ref block68, 6, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ChestMeasurement, 4));
            Pack(buffer, 0x68, ref block68, 10, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ArmCircumference, 4));
            Pack(buffer, 0x68, ref block68, 14, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.WaisteCircumference, 4));
            Pack(buffer, 0x68, ref block68, 18, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.LegCircumference, 4));
            Pack(buffer, 0x68, ref block68, 22, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.CalfCircumference, 4));
            Pack(buffer, 0x68, ref block68, 26, 2, PlayerFieldValidator.ClampEnum(player.FaceType, 2));
            Pack(buffer, 0x68, ref block68, 28, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.LegLength, 4));
            BitConverter.GetBytes(block68).CopyTo(buffer, 0x68);

            // --- 0x6c ---
            uint block6c = 0;
            Pack(buffer, 0x6c, ref block6c, 0, 3, PlayerFieldValidator.ClampEnum(player.BandanaColour, 3));
            Pack(buffer, 0x6c, ref block6c, 3, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ShoulderHeight, 4));
            Pack(buffer, 0x6c, ref block6c, 7, 4, PlayerFieldValidator.ClampPhysicalAttribute(player.ShoulderWidth, 4));
            Pack(buffer, 0x6c, ref block6c, 11, 5, PlayerFieldValidator.ClampAge(player.Age));
            Pack(buffer, 0x6c, ref block6c, 16, 9, PlayerFieldValidator.ClampRawRange(player.Nationality, 9));
            BitConverter.GetBytes(block6c).CopyTo(buffer, 0x6c);

            // --- 0x74 ---
            uint block74 = 0;
            Pack(buffer, 0x74, ref block74, 0, 3, PlayerFieldValidator.ClampInverted(player.BrowsHeight, 3));
            Pack(buffer, 0x74, ref block74, 3, 3, PlayerFieldValidator.ClampInverted(player.EyesPosition, 3));
            Pack(buffer, 0x74, ref block74, 6, 3, PlayerFieldValidator.ClampInverted(player.EyesAngle, 3));
            Pack(buffer, 0x74, ref block74, 9, 3, PlayerFieldValidator.ClampInverted(player.EyesSize, 3));
            Pack(buffer, 0x74, ref block74, 12, 3, PlayerFieldValidator.ClampInverted(player.NoseHeight, 3));
            Pack(buffer, 0x74, ref block74, 15, 3, PlayerFieldValidator.ClampInverted(player.JawWidth, 3));
            Pack(buffer, 0x74, ref block74, 18, 3, PlayerFieldValidator.ClampInverted(player.JawChin, 3));
            Pack(buffer, 0x74, ref block74, 21, 3, PlayerFieldValidator.ClampInverted(player.NoseWidth, 3));
            Pack(buffer, 0x74, ref block74, 24, 3, PlayerFieldValidator.ClampInverted(player.MouthSize, 3));
            Pack(buffer, 0x74, ref block74, 27, 2, (uint)(player.MouthThicknessOfLips + 1));
            Pack(buffer, 0x74, ref block74, 29, 2, PlayerFieldValidator.ClampPlus1(player.MouthType, 2));
            BitConverter.GetBytes(block74).CopyTo(buffer, 0x74);

            // --- 0x78 ---
            uint block78 = 0;
            Pack(buffer, 0x78, ref block78, 0, 6, PlayerFieldValidator.ClampRawRange(player.BrowsTypeAngle, 6));
            Pack(buffer, 0x78, ref block78, 7, 3, PlayerFieldValidator.ClampPlus1(player.NoseType, 3));
            Pack(buffer, 0x78, ref block78, 11, 3, PlayerFieldValidator.ClampPlus1(player.CheeksType, 3));
            Pack(buffer, 0x78, ref block78, 14, 3, PlayerFieldValidator.ClampInverted(player.MouthPosition, 3));
            Pack(buffer, 0x78, ref block78, 17, 5, PlayerFieldValidator.ClampPlus1(player.EyesType, 5));
            Pack(buffer, 0x78, ref block78, 23, 3, PlayerFieldValidator.ClampPlus1(player.JawType, 3));
            BitConverter.GetBytes(block78).CopyTo(buffer, 0x78);
        }

        private object ClampRawRange(int weight, int v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sobrescribe un jugador específico directamente en el archivo binario,
        /// localizándolo por su índice (registro de 124 bytes).
        /// </summary>
        public void WritePlayerToFile(string filePath, Player player)
        {
            const int RecordSize = 0x7C;
            const long HeaderOffset = 124; // mismo offset inicial usado en ParsePlayers

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

        /// <summary>
        /// Crea una copia de respaldo del archivo original antes de sobrescribirlo,
        /// agregando la extensión adicional ".bkp" (ej. "para_we8.bin_000.bkp").
        /// Si ya existe un backup previo, se sobrescribe con el contenido actual del archivo origen.
        /// </summary>
        private void BackupFileIfExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return;
            }

            string backupPath = filePath + ".bkp";
            File.Copy(filePath, backupPath, overwrite: true);
        }

        private void SaveAllPlayersData(string filePath)
        {
            BackupFileIfExists(filePath);

            foreach (var player in People)
            {
                if (player != null)
                {
                    WritePlayerToFile(filePath, player);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FilterPersonListBox(SearchPlayerTextBox.Text);
        }
        private void SearchPlayerTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchPlayerTextBox.Text == "Search Player...")
            {
                SearchPlayerTextBox.Text = string.Empty;
            }
        }

        private void SearchPlayerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterPersonListBox(SearchPlayerTextBox.Text);
        }

        private void FilterPersonListBox(string searchText)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(People);

            if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search Player...")
            {
                view.Filter = null;
                return;
            }

            view.Filter = obj =>
            {
                if (obj is not Player player)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(player.Name)
                    && player.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
            };
        }
    }
}