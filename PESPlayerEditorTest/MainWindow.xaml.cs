﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using PlayerLibrary;


namespace PESPlayerEditorTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SafeFileHandle filePath;

        public FileStream fileStream;

        public FileStream fsPlayerAssignment;

        public FileStream fsNumbers;

        public Player SelectedPlayer { get; set; }

        public static List<Position> Positions { get; } = new List<Position>
            {
                new Position { PositionId = 0, PositionCode = "GK", PositionName = "Goal Keeper" },
                new Position { PositionId = 1, PositionCode = "LI", PositionName = "Libero" },
                new Position { PositionId = 2, PositionCode = "SW", PositionName = "Sweeper" },
                new Position { PositionId = 3, PositionCode = "CB", PositionName = "Central Back Stopper" },
                new Position { PositionId = 4, PositionCode = "SB", PositionName = "Side Back" },
                new Position { PositionId = 5, PositionCode = "DMF", PositionName = "Defensive Midfielder" },
                new Position { PositionId = 6, PositionCode = "CMF", PositionName = "Centre Midfielder" },
                new Position { PositionId = 7, PositionCode = "SMF", PositionName = "Side Midfielder" },
                new Position { PositionId = 8, PositionCode = "OMF", PositionName = "Offensive Midfielder" },
                new Position { PositionId = 9, PositionCode = "CF", PositionName = "Centre Forward" },
                new Position { PositionId = 10, PositionCode = "WF", PositionName = "Wing Forward" },
        };

        public static List<Country> Countries { get; } = new List<Country>
            {
                new Country { CountryIndex = 0, CountryId = 249, CountryName = "Free Nationality" },
                new Country { CountryIndex = 1, CountryId = 122, CountryName = "Albania" },
                new Country { CountryIndex = 2, CountryId = 123, CountryName = "Austria" },
                new Country { CountryIndex = 3, CountryId = 124, CountryName = "Belarus" },
                new Country { CountryIndex = 4, CountryId = 125, CountryName = "Belgium" },
                new Country { CountryIndex = 5, CountryId = 126, CountryName = "Bosnia" },
                new Country { CountryIndex = 6, CountryId = 127, CountryName = "Bulgaria" },
                new Country { CountryIndex = 7, CountryId = 128, CountryName = "Croatia" },
                new Country { CountryIndex = 8, CountryId = 129, CountryName = "Cyprus" },
                new Country { CountryIndex = 9, CountryId = 130, CountryName = "Czechia" },
                new Country { CountryIndex = 10, CountryId = 131, CountryName = "Denmark" },
                new Country { CountryIndex = 11, CountryId = 132, CountryName = "England" },
                new Country { CountryIndex = 12, CountryId = 133, CountryName = "Estonia" },
                new Country { CountryIndex = 13, CountryId = 134, CountryName = "Finland" },
                new Country { CountryIndex = 14, CountryId = 135, CountryName = "France" },
                new Country { CountryIndex = 15, CountryId = 136, CountryName = "Germany" },
                new Country { CountryIndex = 16, CountryId = 137, CountryName = "Georgia" },
                new Country { CountryIndex = 17, CountryId = 138, CountryName = "Greece" },
                new Country { CountryIndex = 18, CountryId = 139, CountryName = "Hungary" },
                new Country { CountryIndex = 19, CountryId = 140, CountryName = "Finland" },
                new Country { CountryIndex = 20, CountryId = 141, CountryName = "Ireland" },
                new Country { CountryIndex = 21, CountryId = 142, CountryName = "Israel" },
                new Country { CountryIndex = 22, CountryId = 143, CountryName = "Italy" },
                new Country { CountryIndex = 23, CountryId = 144, CountryName = "Latvia" },
                new Country { CountryIndex = 24, CountryId = 145, CountryName = "Lietchenstein" },
                new Country { CountryIndex = 25, CountryId = 146, CountryName = "Lithuania" },
                new Country { CountryIndex = 26, CountryId = 147, CountryName = "Luxembourg" },
                new Country { CountryIndex = 27, CountryId = 148, CountryName = "Macedonia" },
                new Country { CountryIndex = 28, CountryId = 149, CountryName = "Moldova" },
                new Country { CountryIndex = 29, CountryId = 150, CountryName = "Netherlands" },
                new Country { CountryIndex = 30, CountryId = 151, CountryName = "Northern Ireland" },
                new Country { CountryIndex = 31, CountryId = 152, CountryName = "Norway" },
                new Country { CountryIndex = 32, CountryId = 153, CountryName = "Poland" },
                new Country { CountryIndex = 33, CountryId = 154, CountryName = "Portugal" },
                new Country { CountryIndex = 34, CountryId = 155, CountryName = "Romania" },
                new Country { CountryIndex = 35, CountryId = 156, CountryName = "Russia" },
                new Country { CountryIndex = 36, CountryId = 157, CountryName = "Scotland" },
                new Country { CountryIndex = 37, CountryId = 158, CountryName = "Serbia and Montenegro" },
                new Country { CountryIndex = 38, CountryId = 159, CountryName = "Slovakia" },
                new Country { CountryIndex = 39, CountryId = 160, CountryName = "Slovenia" },
                new Country { CountryIndex = 40, CountryId = 161, CountryName = "Spain" },
                new Country { CountryIndex = 41, CountryId = 162, CountryName = "Sweden" },
                new Country { CountryIndex = 42, CountryId = 163, CountryName = "Switzerland" },
                new Country { CountryIndex = 43, CountryId = 164, CountryName = "Turkey" },
                new Country { CountryIndex = 44, CountryId = 165, CountryName = "Ukraine" },
                new Country { CountryIndex = 45, CountryId = 166, CountryName = "Uzbekistan" },
                new Country { CountryIndex = 46, CountryId = 167, CountryName = "Wales" },
                new Country { CountryIndex = 47, CountryId = 168, CountryName = "Algeria" },
                new Country { CountryIndex = 48, CountryId = 169, CountryName = "Angola" },
                new Country { CountryIndex = 49, CountryId = 170, CountryName = "Benin" },
                new Country { CountryIndex = 50, CountryId = 171, CountryName = "Burkina Faso" },
                new Country { CountryIndex = 51, CountryId = 172, CountryName = "Burundi" },
                new Country { CountryIndex = 52, CountryId = 173, CountryName = "Cape Verde" },
                new Country { CountryIndex = 53, CountryId = 174, CountryName = "Cameroon" },
                new Country { CountryIndex = 54, CountryId = 175, CountryName = "Comoros" },
                new Country { CountryIndex = 55, CountryId = 176, CountryName = "DR Congo" },
                new Country { CountryIndex = 56, CountryId = 177, CountryName = "Cote d'Ivoire" },
                new Country { CountryIndex = 57, CountryId = 178, CountryName = "Egypt" },
                new Country { CountryIndex = 58, CountryId = 179, CountryName = "Ethiopia" },
                new Country { CountryIndex = 59, CountryId = 180, CountryName = "Gabon" },
                new Country { CountryIndex = 60, CountryId = 181, CountryName = "Gambia" },
                new Country { CountryIndex = 61, CountryId = 182, CountryName = "Ghana" },
                new Country { CountryIndex = 62, CountryId = 183, CountryName = "Guinea" },
                new Country { CountryIndex = 63, CountryId = 184, CountryName = "Liberia" },
                new Country { CountryIndex = 64, CountryId = 185, CountryName = "Lybia" },
                new Country { CountryIndex = 65, CountryId = 186, CountryName = "Mali" },
                new Country { CountryIndex = 66, CountryId = 187, CountryName = "Mauritius" },
                new Country { CountryIndex = 67, CountryId = 188, CountryName = "Morocco" },
                new Country { CountryIndex = 68, CountryId = 189, CountryName = "Mozambique" },
                new Country { CountryIndex = 69, CountryId = 190, CountryName = "Namibia" },
                new Country { CountryIndex = 70, CountryId = 191, CountryName = "Nigeria" },
                new Country { CountryIndex = 71, CountryId = 192, CountryName = "Rwanda" },
                new Country { CountryIndex = 72, CountryId = 193, CountryName = "Senegal" },
                new Country { CountryIndex = 73, CountryId = 194, CountryName = "Sierra Leone" },
                new Country { CountryIndex = 74, CountryId = 195, CountryName = "South Africa" },
                new Country { CountryIndex = 75, CountryId = 196, CountryName = "Sudan" },
                new Country { CountryIndex = 76, CountryId = 197, CountryName = "Togo" },
                new Country { CountryIndex = 77, CountryId = 198, CountryName = "Tunisia" },
                new Country { CountryIndex = 78, CountryId = 199, CountryName = "Zambia" },
                new Country { CountryIndex = 79, CountryId = 200, CountryName = "Zimbabwe" },
                new Country { CountryIndex = 80, CountryId = 201, CountryName = "Bermuda" },
                new Country { CountryIndex = 81, CountryId = 202, CountryName = "Canada" },
                new Country { CountryIndex = 82, CountryId = 203, CountryName = "Costa Rica" },
                new Country { CountryIndex = 83, CountryId = 204, CountryName = "Curacao" },
                new Country { CountryIndex = 84, CountryId = 205, CountryName = "Guatemala" },
                new Country { CountryIndex = 85, CountryId = 206, CountryName = "Honduras" },
                new Country { CountryIndex = 86, CountryId = 207, CountryName = "Jamaica" },
                new Country { CountryIndex = 87, CountryId = 208, CountryName = "Mexico" },
                new Country { CountryIndex = 88, CountryId = 209, CountryName = "Panama" },
                new Country { CountryIndex = 89, CountryId = 210, CountryName = "Trinidad and Tobago" },
                new Country { CountryIndex = 90, CountryId = 211, CountryName = "United States" },
                new Country { CountryIndex = 91, CountryId = 212, CountryName = "Argentina" },
                new Country { CountryIndex = 92, CountryId = 213, CountryName = "Bolivia" },
                new Country { CountryIndex = 93, CountryId = 214, CountryName = "Brazil" },
                new Country { CountryIndex = 94, CountryId = 215, CountryName = "Chile" },
                new Country { CountryIndex = 95, CountryId = 216, CountryName = "Colombia" },
                new Country { CountryIndex = 96, CountryId = 217, CountryName = "Ecuador" },
                new Country { CountryIndex = 97, CountryId = 218, CountryName = "Grenada" },
                new Country { CountryIndex = 98, CountryId = 219, CountryName = "Paraguay" },
                new Country { CountryIndex = 99, CountryId = 220, CountryName = "Peru" },
                new Country { CountryIndex = 100, CountryId = 221, CountryName = "Uruguay" },
                new Country { CountryIndex = 101, CountryId = 222, CountryName = "Suriname" },
                new Country { CountryIndex = 102, CountryId = 223, CountryName = "Venezuela" },
                new Country { CountryIndex = 103, CountryId = 224, CountryName = "China" },
                new Country { CountryIndex = 104, CountryId = 225, CountryName = "India" },
                new Country { CountryIndex = 105, CountryId = 226, CountryName = "Iran" },
                new Country { CountryIndex = 106, CountryId = 227, CountryName = "Japan" },
                new Country { CountryIndex = 107, CountryId = 228, CountryName = "Korea" },
                new Country { CountryIndex = 108, CountryId = 229, CountryName = "Lebanon" },
                new Country { CountryIndex = 109, CountryId = 230, CountryName = "Pakistan" },
                new Country { CountryIndex = 110, CountryId = 231, CountryName = "Palestine" },
                new Country { CountryIndex = 111, CountryId = 232, CountryName = "Qatar" },
                new Country { CountryIndex = 112, CountryId = 233, CountryName = "Saudi Arabia" },
                new Country { CountryIndex = 113, CountryId = 234, CountryName = "Thailand" },
                new Country { CountryIndex = 114, CountryId = 235, CountryName = "Vietnam" },
                new Country { CountryIndex = 115, CountryId = 236, CountryName = "Australia" },
                new Country { CountryIndex = 116, CountryId = 237, CountryName = "New Zealand" },
                new Country { CountryIndex = 117, CountryId = 238, CountryName = "Tahiti" },
            };

        public static List<Age> Ages { get; } = new List<Age>
        {
            new Age { AgeIndex = 0, AgeValue = ["02","05","04","05","06","07"], AgeName = 15 },
            new Age { AgeIndex = 1, AgeValue = ["0A","0B","0C","0D","0E","0F"], AgeName = 16 },
            new Age { AgeIndex = 2, AgeValue = ["12","13","14","15","16","17"], AgeName = 17 },
            new Age { AgeIndex = 3, AgeValue = ["1A","1B","1C","1D","1E","1F"], AgeName = 18 },
            new Age { AgeIndex = 4, AgeValue = ["22","23","24","25","26","27"], AgeName = 19 },
            new Age { AgeIndex = 5, AgeValue = ["2A","2B","2C","2D","2E","2F"], AgeName = 20 },
            new Age { AgeIndex = 6, AgeValue = ["32","33","34","35","36","37"], AgeName = 21 },
            new Age { AgeIndex = 7, AgeValue = ["3A","3B","3C","3D","4E","3F"], AgeName = 22 },
            new Age { AgeIndex = 8, AgeValue = ["42","43","44","45","46","47"], AgeName = 23 },
            new Age { AgeIndex = 9, AgeValue = ["4A","4B","4C","4D","4E","4F"], AgeName = 24 },
            new Age { AgeIndex = 10,AgeValue = ["52","53","54","55","56","57"], AgeName = 25 },
            new Age { AgeIndex = 11,AgeValue = ["5A","5B","5C","5D","5E","5F"], AgeName = 26 },
            new Age { AgeIndex = 12,AgeValue = ["62","63","64","65","66","67"], AgeName = 27 },
            new Age { AgeIndex = 13,AgeValue = ["6A","6B","6C","6D","6E","6F"], AgeName = 28 },
            new Age { AgeIndex = 14,AgeValue = ["72","73","74","75","76","77"], AgeName = 29 },
            new Age { AgeIndex = 15,AgeValue = ["7A","7B","7C","7D","7E","7F"], AgeName = 30 },
            new Age { AgeIndex = 16,AgeValue = ["82","83","84","85","86","87"], AgeName = 31 },
            new Age { AgeIndex = 17,AgeValue = ["8A","8B","8C","8D","8E","8F"], AgeName = 32 },
            new Age { AgeIndex = 18,AgeValue = ["92","93","94","95","95","97"], AgeName = 33 },
            new Age { AgeIndex = 19,AgeValue = ["9A","9B","9C","9D","9E","9F"], AgeName = 34 },
            new Age { AgeIndex = 20,AgeValue = ["A2","A3","A4","A5","A6","A7"], AgeName = 35 },
            new Age { AgeIndex = 21,AgeValue = ["AA","AB","AC","AD","AE","AF"], AgeName = 36 },
            new Age { AgeIndex = 22,AgeValue = ["B2","B3","B4","B5","B6","B7"], AgeName = 37 },
            new Age { AgeIndex = 23,AgeValue = ["BA","BB","BC","BD","BE","BF"], AgeName = 38 },
            new Age { AgeIndex = 24,AgeValue = ["C2","C3","C4","C4","C5","C6"], AgeName = 39 },
            new Age { AgeIndex = 25,AgeValue = ["CA","CB","CC","CD","CE","CF"], AgeName = 40 },
            new Age { AgeIndex = 26,AgeValue = ["D2","D3","D4","D4","D5","D6"], AgeName = 41 },
            new Age { AgeIndex = 27,AgeValue = ["DA","DB","DC","DD","DE","DF"], AgeName = 42 },
            new Age { AgeIndex = 28,AgeValue = ["E2","E3","E4","E5","E6","E7"], AgeName = 43 },
            new Age { AgeIndex = 29,AgeValue = ["EA","EB","EC","ED","EE","EF"], AgeName = 44 },
            new Age { AgeIndex = 30,AgeValue = ["F2","F3","F4","F5","F6","F7"], AgeName = 45 },
            new Age { AgeIndex = 31,AgeValue = ["FA","FB","FC","FD","FE","FF"], AgeName = 46 }
        };

        List<Player> players = new List<Player>();

        List<PlayerAssignment> playerAssignment = new List<PlayerAssignment>();

        OpenFileDialog openFileDialog = new OpenFileDialog();

        public ObservableCollection<Player> People { get; set; } = new ObservableCollection<Player>();
        public ObservableCollection<PlayerAssignment> PeopleA { get; set; } = new ObservableCollection<PlayerAssignment>();
        public string FilePath1 { get; internal set; }
        public string FilePath2 { get; internal set; }
        public string FilePath3 { get; internal set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void PopulateTeamsComboBox()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "team.cfg");
                List<string> teamNames = new List<string>();

                foreach (string line in File.ReadLines(filePath))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        string[] parts = line.Split(';');
                        if (parts.Length >= 3)
                        {
                            teamNames.Add(parts[2].ToUpper());
                        }
                    }
                }
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

            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fileStream.Seek(124, SeekOrigin.Begin);

                byte[] buffer = new byte[0x7C];

                int playerIndex = 1;
                while (fileStream.Read(buffer, 0, buffer.Length) == buffer.Length)
                {
                    string ageHexValue = buffer[109].ToString("X2");
                    Age age = Ages.FirstOrDefault(a => a.AgeValue.Contains(ageHexValue));

                    byte[] commentaryHex = new byte[2]; Array.Copy(buffer, 48, commentaryHex, 0, 2);

                    Player person = new Player
                    {
                        PlayerIndex = playerIndex++,
                        Name = Encoding.Unicode.GetString(buffer, 0, 32).Trim(),
                        ShirtName = Encoding.ASCII.GetString(buffer, 32, 16).Trim(),
                        Commentary = BitConverter.ToInt16(commentaryHex, 0),
                        Position = buffer[52] & 0x0F,
                        Height = buffer[88] - 44,
                        Weight = buffer[89] - 44,
                        Age = age?.AgeIndex ?? 0,
                        Country = BitConverter.ToInt32(buffer, 110),
                    };
                    People.Add(person);
                }
            }
            return players;
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
                        Player playerObj = People.FirstOrDefault(p => p.PlayerIndex == player);

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
                    shirtNameTextBox.Text = selectedPlayer.ShirtName;
                    callnameTextBox.Text = selectedPlayer.Commentary.ToString();
                    countryComboBox.SelectedIndex = selectedPlayer.Country - 121;
                }
            }
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
                            byte[] nameBytes = Encoding.Unicode.GetBytes(player.Name.PadRight(12, '\0'));
                            writer.Write(nameBytes);

                            fileStream.Seek((bitOffset + 32 * 8) / 8, SeekOrigin.Begin);
                            byte[] shirtNameBytes = Encoding.ASCII.GetBytes(player.ShirtName.PadRight(16, '\0').Substring(0, 16));
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
            using(FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                foreach (var playerAssignment in PeopleA)
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode, true))
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        byte[] playersAssignment = BitConverter.GetBytes(playerAssignment.Player.PlayerIndex);
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

        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
             SavePlayerData(FilePath1);
             //SavePlayerAssignmentData(FilePath2);
             //SaveNumberData(FilePath3);
             People.Clear();
             //PeopleA.Clear();
             ParsePlayers(FilePath1);
             //ParsePlayerAssignment(FilePath2, FilePath3);
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
            var filteredPeopleA = PeopleA.Where(player => player.Team == selectedTeamId).ToList();
            dataGrid.ItemsSource = filteredPeopleA;
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
            Player selectedPlayer = selectedPlayerAssignment.Player;
            PlayerWindow playerWindow = new PlayerWindow(selectedPlayer, this);
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
                    shirtNameTextBox.Text = selectedPlayer.ShirtName;
                    callnameTextBox.Text = selectedPlayer.Commentary.ToString();
                    countryComboBox.SelectedIndex = selectedPlayer.Country - 121;
                }

            }
        }

        public void updatePeopleList()
        {
            People.CollectionChanged += People_CollectionChanged;
            PeopleA.CollectionChanged += PeopleA_CollectionChanged;
        }

        private void People_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Player player in e.NewItems)
                {
                    // Check if the player exists in PeopleA
                    PlayerAssignment assignment = PeopleA.FirstOrDefault(a => a.PlayerIndex == player.PlayerIndex);
                    if (assignment != null)
                    {
                        assignment.Player = player;
                        // Update other properties of assignment if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Player player in e.OldItems)
                {
                    // Remove the player from PeopleA
                    PlayerAssignment assignment = PeopleA.FirstOrDefault(a => a.PlayerIndex == player.PlayerIndex);
                    if (assignment != null)
                    {
                        assignment.Player = null;
                        // Update other properties of assignment if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (Player player in e.NewItems)
                {
                    // Update the player in PeopleA
                    PlayerAssignment assignment = PeopleA.FirstOrDefault(a => a.PlayerIndex == player.PlayerIndex);
                    if (assignment != null)
                    {
                        assignment.Player = player;
                        // Update other properties of assignment if needed
                    }
                }
            }
        }

        private void PeopleA_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PlayerAssignment assignment in e.NewItems)
                {
                    // Check if the assignment's player exists in People
                    Player player = People.FirstOrDefault(p => p.PlayerIndex == assignment.PlayerIndex);
                    if (player != null)
                    {
                        player = assignment.Player;
                        // Update other properties of player if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PlayerAssignment assignment in e.OldItems)
                {
                    // Remove the player from People
                    Player player = People.FirstOrDefault(p => p.PlayerIndex == assignment.PlayerIndex);
                    if (player != null)
                    {
                        player = null;
                        // Update other properties of player if needed
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (PlayerAssignment assignment in e.NewItems)
                {
                    // Update the player in People
                    Player player = People.FirstOrDefault(p => p.PlayerIndex == assignment.PlayerIndex);
                    if (player != null)
                    {
                        player = assignment.Player;
                        // Update other properties of player if needed
                    }
                }
            }
        }
    }
}
