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

        public PlayerAssignment? SelectedPlayerAssignment { get; set; }
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

        /// <summary>
        /// Lista de edades posibles seg�n el campo de 5 bits "age" del byte 0x6c
        /// (uint32 age:5), cuyo valor crudo (0-31) se traduce a edad real sum�ndole 15
        /// (ReadAge/WriteAge: edad = raw + 15, raw = edad - 15). Reemplaza la tabla
        /// anterior basada en valores hexadecimales por c�digo de equipo/jugador.
        /// </summary>
        public static List<int> Ages { get; } = Enumerable.Range(15, 32).ToList(); // 15..46

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

        public PlayerWindow(Player selectedPlayer, MainWindow mainWindow, PlayerAssignment? selectedPlayerAssignment = null)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            SelectedPlayer = selectedPlayer;
            SelectedPlayerAssignment = selectedPlayerAssignment;
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
