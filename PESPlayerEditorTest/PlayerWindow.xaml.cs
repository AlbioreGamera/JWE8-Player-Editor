using JWE8Editor.Logic;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    /// 

    public partial class PlayerWindow : Window
    {
        public Player SelectedPlayer { get; set; }

        public PlayerAssignment SelectedPlayerAssignment { get; set; }
        public static List<Position> Positions { get; } = new List<Position>
            {
                new Position { PositionId = 0, PositionName = "Goal Keeper" },
                new Position { PositionId = 1, PositionName = "Libero" },
                new Position { PositionId = 2, PositionName = "Sweeper" },
                new Position { PositionId = 3, PositionName = "Central Back Stopper" },
                new Position { PositionId = 4, PositionName = "Side Back" },
                new Position { PositionId = 5, PositionName = "Defensive Midfielder" },
                new Position { PositionId = 6, PositionName = "Centre Midfielder" },
                new Position { PositionId = 7, PositionName = "Side Midfielder" },
                new Position { PositionId = 8, PositionName = "Offensive Midfielder" },
                new Position { PositionId = 9, PositionName = "Centre Forward" },
                new Position { PositionId = 10, PositionName = "Wing Forward" },
        };

        public LoadCountries CountriesLoader { get; } = new LoadCountries();
        public static List<Age> Ages { get; } = new List<Age>
        {
            new Age { AgeIndex = 0, AgeValue = ["02","05","04"], AgeName = 15 },
            new Age { AgeIndex = 1, AgeValue = ["0A","0B","0C"], AgeName = 16 },
            new Age { AgeIndex = 2, AgeValue = ["12","13","14"], AgeName = 17 },
            new Age { AgeIndex = 3, AgeValue = ["1A","1B","1C"], AgeName = 18 },
            new Age { AgeIndex = 4, AgeValue = ["22","23","24"], AgeName = 19 },
            new Age { AgeIndex = 5, AgeValue = ["2A","2B","2C"], AgeName = 20 },
            new Age { AgeIndex = 6, AgeValue = ["32","33","34"], AgeName = 21 },
            new Age { AgeIndex = 7, AgeValue = ["3A","3B","3C"], AgeName = 22 },
            new Age { AgeIndex = 8, AgeValue = ["42","43","44"], AgeName = 23 },
            new Age { AgeIndex = 9, AgeValue = ["4A","4B","4C"], AgeName = 24 },
            new Age { AgeIndex = 10,AgeValue = ["52","53","54"], AgeName = 25 },
            new Age { AgeIndex = 11,AgeValue = ["5A","5B","5C"], AgeName = 26 },
            new Age { AgeIndex = 12,AgeValue = ["62","63","64"], AgeName = 27 },
            new Age { AgeIndex = 13,AgeValue = ["6A","6B","6C"], AgeName = 28 },
            new Age { AgeIndex = 14,AgeValue = ["72","73","74"], AgeName = 29 },
            new Age { AgeIndex = 15,AgeValue = ["7A","7B","7C"], AgeName = 30 },
            new Age { AgeIndex = 16,AgeValue = ["82","83","84"], AgeName = 31 },
            new Age { AgeIndex = 17,AgeValue = ["8A","8B","8C"], AgeName = 32 },
            new Age { AgeIndex = 18,AgeValue = ["92","93","94"], AgeName = 33 },
            new Age { AgeIndex = 19,AgeValue = ["9A","9B","9C"], AgeName = 34 },
            new Age { AgeIndex = 20,AgeValue = ["A2","A3","A4"], AgeName = 35 },
            new Age { AgeIndex = 21,AgeValue = ["AA","AB","AC"], AgeName = 36 },
            new Age { AgeIndex = 22,AgeValue = ["B2","B3","B4"], AgeName = 37 },
            new Age { AgeIndex = 23,AgeValue = ["BA","BB","BC"], AgeName = 38 },
            new Age { AgeIndex = 24,AgeValue = ["C2","C3","C4"], AgeName = 39 },
            new Age { AgeIndex = 25,AgeValue = ["CA","CB","CC"], AgeName = 40 },
            new Age { AgeIndex = 26,AgeValue = ["D2","D3","D4"], AgeName = 41 },
            new Age { AgeIndex = 27,AgeValue = ["DA","DB","DC"], AgeName = 42 },
            new Age { AgeIndex = 28,AgeValue = ["E2","E3","E4"], AgeName = 43 },
            new Age { AgeIndex = 29,AgeValue = ["EA","EB","EC"], AgeName = 44 },
            new Age { AgeIndex = 30,AgeValue = ["F2","F3","F4"], AgeName = 45 },
            new Age { AgeIndex = 31,AgeValue = ["FA","FB","FC"], AgeName = 46 }
        };

        public LoadCommentaries CommentariesLoader { get; } = new LoadCommentaries();

        public static Array FeetValues { get; } = Enum.GetValues(typeof(Foot));
        public static Array InjuryValues { get; } = Enum.GetValues(typeof(InjuryResistance));
        public static Array SkinColourValues { get; } = Enum.GetValues(typeof(SkinColour));
        public static Array BootsColorValues { get; } = Enum.GetValues(typeof(BootsColor));
        public static Array EyesColor2Values { get; } = Enum.GetValues(typeof(EyesColor2));
        public static Array SleevesValues { get; } = Enum.GetValues(typeof(SleevesLength));
        public static Array YesNoValues { get; } = Enum.GetValues(typeof(YesNo));
        public static Array CapColourValues { get; } = Enum.GetValues(typeof(CapColour));
        public static Array WristbandTypeValues { get; } = Enum.GetValues(typeof(WristbandType));
        public static Array BootsTypeValues { get; } = Enum.GetValues(typeof(BootsType));
        public static Array GlovesOptionValues { get; } = Enum.GetValues(typeof(GlovesOption));
        public static Array SocksHeightValues { get; } = Enum.GetValues(typeof(SocksHeight));
        public static Array FaceTypeValues { get; } = Enum.GetValues(typeof(FaceType));
        public static Array BandanaColourValues { get; } = Enum.GetValues(typeof(BandanaColour));
        public static Array FreeKickTypeValues { get; } = Enum.GetValues(typeof(FreeKickType));
        public static Array PenaltyKickTypeValues { get; } = Enum.GetValues(typeof(PenaltyKickType));

        private MainWindow mainWindow;

        public PlayerWindow(Player selectedPlayer, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            SelectedPlayer = selectedPlayer;
            DataContext = this;

            positionComboBox.ItemsSource = Positions;
            positionComboBox.SelectedItem = Positions.FirstOrDefault(p => p.PositionId == selectedPlayer.Position);

            playerIdTextBox.Text = selectedPlayer.PlayerIndex.ToString();
            nameTextBox.Text = selectedPlayer.Name;
            shirtNameTextBox.Text = selectedPlayer.ShirtName;
            countryComboBox.SelectedIndex = selectedPlayer.Country - 121;
            heightTextBox.Text = selectedPlayer.Height.ToString();
            weightTextBox.Text = selectedPlayer.Weight.ToString();
            positionComboBox.SelectedIndex = selectedPlayer.Position;
            //commentaryComboBox.IsEnabled = true;
            commentaryComboBox.SelectedIndex = selectedPlayer.Commentary;
        }

        private void ApplyPlayerChanges(object sender, RoutedEventArgs e)
        {
            // Update the selected player's data with the new values from the window
            SelectedPlayer.PlayerIndex = int.Parse(playerIdTextBox.Text);
            SelectedPlayer.Name = nameTextBox.Text;
            SelectedPlayer.ShirtName = shirtNameTextBox.Text;
            SelectedPlayer.Commentary = commentaryComboBox.SelectedIndex;
            SelectedPlayer.Country = countryComboBox.SelectedIndex + 121;
            SelectedPlayer.Age = ageComboBox.SelectedIndex;
            SelectedPlayer.Height = int.Parse(heightTextBox.Text);
            SelectedPlayer.Weight = int.Parse(weightTextBox.Text);
            SelectedPlayer.Position = positionComboBox.SelectedIndex;
            mainWindow.nameTextBox.Text = SelectedPlayer.Name;
            mainWindow.shirtNameTextBox.Text = SelectedPlayer.ShirtName;
            mainWindow.commentaryComboBox.SelectedIndex = SelectedPlayer.Commentary;
            mainWindow.countryComboBox.SelectedIndex = SelectedPlayer.Country - 121;
            mainWindow.updatePeopleList();
            mainWindow.team1DataGrid.Items.Refresh();
            mainWindow.team2DataGrid.Items.Refresh();
            mainWindow.personListBox.Items.Refresh();
            this.Close();
        }

        private void ClosePlayerWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private static readonly Regex DigitsOnlyRegex = new Regex("^[0-9]+$");

        private void StatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnlyRegex.IsMatch(e.Text);
        }

        private void StatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            if (value < 0)  
            {
                value = 0;
            }
            else if (value > 99)
            {
                value = 99;
            }

            string clamped = value.ToString();
            if (textBox.Text != clamped)
            {
                textBox.Text = clamped;
            }
        }

        private void StatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (e.Key != Key.Up && e.Key != Key.Down)
            {
                return;
            }

            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            value += e.Key == Key.Up ? 1 : -1;

            if (value < 0)
            {
                value = 0;
            }
            else if (value > 99)
            {
                value = 99;
            }

            textBox.Text = value.ToString();
            textBox.CaretIndex = textBox.Text.Length;
            e.Handled = true;
        }

        private void FaceIdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnlyRegex.IsMatch(e.Text);
        }

        private void FaceIdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            int maxFaceId = LoadFacePlayers.GetMaxFaceId(SelectedPlayer.SkinColour);

            if (value < 0)
            {
                value = 0;
            }
            else if (value > maxFaceId)
            {
                value = maxFaceId;
            }

            string clamped = value.ToString();
            if (textBox.Text != clamped)
            {
                textBox.Text = clamped;
            }
        }

        private void FaceIdTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (e.Key != Key.Up && e.Key != Key.Down)
            {
                return;
            }

            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            value += e.Key == Key.Up ? 1 : -1;

            int maxFaceId = LoadFacePlayers.GetMaxFaceId(SelectedPlayer.SkinColour);

            if (value < 0)
            {
                value = 0;
            }
            else if (value > maxFaceId)
            {
                value = maxFaceId;
            }

            textBox.Text = value.ToString();
            textBox.CaretIndex = textBox.Text.Length;
            e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
