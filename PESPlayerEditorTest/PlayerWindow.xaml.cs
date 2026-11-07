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

        /// <summary>
        /// Raised when the player's changes have been saved, so the caller can refresh
        /// its own UI without PlayerWindow needing knowledge of MainWindow's controls.
        /// </summary>
        public event EventHandler<Player> PlayerSaved;

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

        private readonly MainWindow mainWindow;

        public PlayerWindow(Player selectedPlayer, MainWindow mainWindow, PlayerAssignment? selectedPlayerAssignment = null)
        {
            InitializeComponent();
            this.mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
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

        private void SavePlayer(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(heightTextBox.Text, out int height) ||
                !int.TryParse(weightTextBox.Text, out int weight))
            {
                MessageBox.Show("Height and Weight must be valid numbers.", "Invalid input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedPlayer.Name = nameTextBox.Text;
            SelectedPlayer.ShirtName = shirtNameTextBox.Text;
            SelectedPlayer.Commentary = commentaryComboBox.SelectedIndex;
            SelectedPlayer.Country = countryComboBox.SelectedIndex + 121;
            SelectedPlayer.Height = height;
            SelectedPlayer.Weight = weight;
            SelectedPlayer.Position = positionComboBox.SelectedIndex;

            PlayerSaved?.Invoke(this, SelectedPlayer);

            this.Close();
        }

        private void ClosePlayerWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private static readonly Regex DigitsOnlyRegex = new Regex("^[0-9]+$");

        // Name se almacena como wchar_t[16] (UTF-16): admite letras acentuadas/ñ, espacios, punto, apóstrofe y guion.
        private static readonly Regex NameAllowedCharsRegex = new Regex(@"^[a-zA-Z0-9À-ÿ'.\- ]+$");

        // ShirtName se almacena como char[16] (un solo byte por carácter, sin soporte Unicode):
        // solo ASCII imprimible básico.
        private static readonly Regex ShirtNameAllowedCharsRegex = new Regex(@"^[a-zA-Z0-9'.\- ]+$");

        private void NameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !NameAllowedCharsRegex.IsMatch(e.Text);
        }

        private void NameTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string));
            if (!NameAllowedCharsRegex.IsMatch(pastedText))
            {
                e.CancelCommand();
            }
        }

        private void ShirtNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !ShirtNameAllowedCharsRegex.IsMatch(e.Text);
        }

        private void ShirtNameTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)))
            {
                e.CancelCommand();
                return;
            }

            string pastedText = (string)e.DataObject.GetData(typeof(string));
            if (!ShirtNameAllowedCharsRegex.IsMatch(pastedText))
            {
                e.CancelCommand();
            }
        }

        private static void ClampTextBoxValue(TextBox textBox, int min, int max)
        {
            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            value = Math.Clamp(value, min, max);

            string clamped = value.ToString();
            if (textBox.Text != clamped)
            {
                textBox.Text = clamped;
            }
        }

        private static void AdjustTextBoxValue(TextBox textBox, Key key, int min, int max)
        {
            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            value += key == Key.Up ? 1 : -1;
            value = Math.Clamp(value, min, max);

            textBox.Text = value.ToString();
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void StatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnlyRegex.IsMatch(e.Text);
        }

        private void StatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ClampTextBoxValue(textBox, 0, 99);
            }
        }

        private void StatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || (e.Key != Key.Up && e.Key != Key.Down))
            {
                return;
            }

            AdjustTextBoxValue(textBox, e.Key, 0, 99);
            e.Handled = true;
        }

        private void FaceIdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnlyRegex.IsMatch(e.Text);
        }

        private void FaceIdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int maxFaceId = LoadFacePlayers.GetMaxFaceId(SelectedPlayer.SkinColour);
                ClampTextBoxValue(textBox, 0, maxFaceId);
            }
        }

        private void FaceIdTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || (e.Key != Key.Up && e.Key != Key.Down))
            {
                return;
            }

            int maxFaceId = LoadFacePlayers.GetMaxFaceId(SelectedPlayer.SkinColour);
            AdjustTextBoxValue(textBox, e.Key, 0, maxFaceId);
            e.Handled = true;
        }
    }
}
