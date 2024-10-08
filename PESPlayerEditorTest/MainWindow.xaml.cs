﻿using JWE8Editor.Logic;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using PlayerLibrary;
using System;
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

        public FileStream fsEditOvl;

        public FileStream fsDataset;

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

        public LoadCountries CountriesLoader { get; } = new LoadCountries();

        public LoadCommentaries CommentariesLoader { get; } = new LoadCommentaries();

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
        public ObservableCollection<Callname> CallnameCollection { get; set; } = new ObservableCollection<Callname>();
        public ObservableCollection<Stadium> StadiumCollection { get; set; } = new ObservableCollection<Stadium>();
        public string FilePath1 { get; internal set; }
        public string FilePath2 { get; internal set; }
        public string FilePath3 { get; internal set; }
        public string FilePath4 { get; internal set; }
        public string FilePath5 { get; internal set; }

        private BindingList<Callname> filteredCallnames;
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
                    int n;
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
                byte[] buffer = new byte[0x3D]; // 64 bytes
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
                    shirtNameTextBox.Text = selectedPlayer.ShirtName;
                    commentaryComboBox.SelectedIndex = selectedPlayer.Commentary;
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
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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

        public void SaveCallnameData(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                fileStream.Seek(0x103758, SeekOrigin.Begin);
                long endPosition = 0x10CA50;
                Encoding shiftJis = Encoding.GetEncoding("shift_jis");

                foreach (var callnameList in CallnameCollection)
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode, true))
                    {
                        byte[] callnameBytes = shiftJis.GetBytes(callnameList.CommentaryName);
                        int totalLength = (int)callnameList.CommentaryBytes; // The length defined in CommentaryLength
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
            SavePlayerData(FilePath1);
            //SavePlayerAssignmentData(FilePath2);
            //SaveNumberData(FilePath3);
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
                    commentaryComboBox.SelectedIndex = selectedPlayer.Commentary;
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

        private void StadiumListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
    }
}