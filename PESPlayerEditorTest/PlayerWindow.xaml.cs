using JWE8Editor.Logic;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                new Position { PositionId = 0, PositionName = "GK", PositionTooltip = "Goal Keeper" },
                new Position { PositionId = 1, PositionName = "LIB" , PositionTooltip = "Libero"},
                new Position { PositionId = 2, PositionName = "SWP", PositionTooltip = "Sweeper" },
                new Position { PositionId = 3, PositionName = "CB", PositionTooltip = "Center Back"  },
                new Position { PositionId = 4, PositionName = "SB", PositionTooltip = "Side Back"  },
                new Position { PositionId = 5, PositionName = "DMF", PositionTooltip = "Defensive Midfielder"  },
                new Position { PositionId = 6, PositionName = "CMF", PositionTooltip = "Centre Midfielder"  },
                new Position { PositionId = 7, PositionName = "SMF", PositionTooltip = "Side Midfielder"  },
                new Position { PositionId = 8, PositionName = "OMF", PositionTooltip = "Offensive Midfielder"  },
                new Position { PositionId = 9, PositionName = "CF", PositionTooltip = "Centre Forward"  },
                new Position { PositionId = 10, PositionName = "WF", PositionTooltip = "Wing Forward"  },
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

        // Handlers genéricos para campos con rango configurable vía Tag="min:max"
        // (Height, Weight, Consistency, Condition, Weak Foot Acc./Freq.).
        private static (int min, int max) ParseRangeTag(object tag, int defaultMin, int defaultMax)
        {
            string[] parts = tag?.ToString()?.Split(':') ?? Array.Empty<string>();
            int min = parts.Length > 0 && int.TryParse(parts[0], out int mn) ? mn : defaultMin;
            int max = parts.Length > 1 && int.TryParse(parts[1], out int mx) ? mx : defaultMax;
            return (min, max);
        }

        private void RangeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                (int min, int max) = ParseRangeTag(textBox.Tag, 0, 99);
                ClampTextBoxValue(textBox, min, max);
            }
        }

        private void RangeTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || (e.Key != Key.Up && e.Key != Key.Down))
            {
                return;
            }

            (int min, int max) = ParseRangeTag(textBox.Tag, 0, 99);
            AdjustTextBoxValue(textBox, e.Key, min, max);
            e.Handled = true;
        }

        // Limita el valor en tiempo real al máximo permitido (Tag="min:max"),
        // de modo que escribir, por ejemplo, "300" cuando el máximo es 205
        // se corrige automáticamente al máximo apenas se detecta el exceso.
        // El mínimo no se fuerza aquí para no bloquear al usuario mientras
        // todavía está escribiendo el número (se aplica en LostFocus).
        private void RangeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            if (string.IsNullOrEmpty(textBox.Text))
            {
                return;
            }

            (int min, int max) = ParseRangeTag(textBox.Tag, 1, 99);

            if (!int.TryParse(textBox.Text, out int value))
            {
                return;
            }

            if (value > max)
            {
                textBox.Text = max.ToString();
                textBox.CaretIndex = textBox.Text.Length;
            }
            else if (value < 0 && min >= 0)
            {
                // Evita valores negativos cuando el rango no los admite.
                textBox.Text = min.ToString();
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

        // Permite marcar/desmarcar un CheckBox al hacer clic en su Label asociado.
        // El Label debe tener Tag="NombreDelCheckBox".
        private void CheckBoxLabel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Label label || label.Tag is not string checkBoxName)
            {
                return;
            }

            if (FindName(checkBoxName) is CheckBox checkBox)
            {
                checkBox.IsChecked = !(checkBox.IsChecked ?? false);
            }
        }

        private void StatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnlyRegex.IsMatch(e.Text);
        }

        private void StatTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox || string.IsNullOrEmpty(textBox.Text))
            {
                return;
            }

            if (int.TryParse(textBox.Text, out int value) && value > 99)
            {
                textBox.Text = "99";
                textBox.CaretIndex = textBox.Text.Length;
            }
        }

        private void StatTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                ClampTextBoxValue(textBox, 1, 99);
            }
        }

        private void StatTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox || (e.Key != Key.Up && e.Key != Key.Down))
            {
                return;
            }

            AdjustTextBoxValue(textBox, e.Key, 1, 99);
            e.Handled = true;
        }

        private void StatArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement button || button.Parent is not StackPanel panel)
            {
                return;
            }

            TextBox textBox = panel.Children.OfType<TextBox>().FirstOrDefault();
            if (textBox == null)
            {
                return;
            }

            int delta = int.TryParse(button.Tag?.ToString(), out int parsedDelta) ? parsedDelta : 1;
            AdjustStatTextBoxValue(textBox, delta, 1, 99);
        }

        private static void AdjustStatTextBoxValue(TextBox textBox, int delta, int min, int max)
        {
            if (!int.TryParse(textBox.Text, out int value))
            {
                value = 0;
            }

            value = Math.Clamp(value + delta, min, max);

            textBox.Text = value.ToString();
            textBox.CaretIndex = textBox.Text.Length;
        }

        private void FaceIdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnlyRegex.IsMatch(e.Text);
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

        private void FaceIdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int maxFaceId = LoadFacePlayers.GetMaxFaceId(SelectedPlayer.SkinColour);
                ClampTextBoxValue(textBox, 0, maxFaceId);
            }
        }

        /// <summary>
        /// Abre FaceRelinkWindow mostrando todas las caras preestablecidas disponibles
        /// para el SkinColour actual del jugador, junto con los jugadores de la base de
        /// datos que ya tienen asignado cada FaceId, permitiendo reasignar el FaceId
        /// del jugador que se está editando.
        /// </summary>
        private void RelinkFaceButton_Click(object sender, RoutedEventArgs e)
        {
            FaceRelinkWindow relinkWindow = new FaceRelinkWindow(SelectedPlayer.SkinColour, mainWindow.People, SelectedPlayer.PlayerIndex)
            {
                Owner = this
            };

            if (relinkWindow.ShowDialog() == true && relinkWindow.SelectedFaceId.HasValue)
            {
                SelectedPlayer.FaceId = relinkWindow.SelectedFaceId.Value;

                // Player no implementa INotifyPropertyChanged, por lo que el TextBox
                // enlazado a SelectedPlayer.FaceId no se refresca automáticamente al
                // cambiar el valor desde código. Se fuerza la actualización manual
                // del binding para reflejar el nuevo FaceId en el formulario.
                faceIdTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
            }
        }

        // Permite dígitos y un signo '-' opcional al inicio, para campos de
        // apariencia/medidas corporales con rangos negativos (p. ej. -7..+7).
        private static readonly Regex SignedDigitsRegex = new Regex("^-?[0-9]*$");

        private void SignedStatTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                e.Handled = true;
                return;
            }

            int caret = textBox.CaretIndex;
            string proposed = textBox.Text.Substring(0, caret) + e.Text + textBox.Text.Substring(caret + textBox.SelectionLength);
            e.Handled = !SignedDigitsRegex.IsMatch(proposed);
        }

        /// <summary>
        /// Generic click handler for appearance/body measurement arrow buttons.
        /// The button's Tag must be formatted as "delta:min:max", e.g. "-1:-7:7".
        /// </summary>
        private void AppearanceArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement button || button.Parent is not StackPanel panel)
            {
                return;
            }

            TextBox textBox = panel.Children.OfType<TextBox>().FirstOrDefault();
            if (textBox == null)
            {
                return;
            }

            string[] parts = button.Tag?.ToString()?.Split(':') ?? Array.Empty<string>();
            int delta = parts.Length > 0 && int.TryParse(parts[0], out int d) ? d : 1;
            int min = parts.Length > 1 && int.TryParse(parts[1], out int mn) ? mn : 0;
            int max = parts.Length > 2 && int.TryParse(parts[2], out int mx) ? mx : 99;

            AdjustStatTextBoxValue(textBox, delta, min, max);
        }

        /// <summary>
        /// Clamp genérico para los campos de apariencia (RepeatButton ◀ TextBox ▶) cuyo
        /// rango es fijo y viene codificado en el Tag del RepeatButton como "delta:min:max".
        /// Se aplica en LostFocus porque estos campos no usan RangeTextBoxStyle y, por lo
        /// tanto, no se clampaban al escribir directamente (solo al usar las flechas).
        /// </summary>
        private void AppearanceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox || textBox.Parent is not StackPanel panel)
            {
                return;
            }

            RepeatButton button = panel.Children.OfType<RepeatButton>().FirstOrDefault();
            if (button == null)
            {
                return;
            }

            string[] parts = button.Tag?.ToString()?.Split(':') ?? Array.Empty<string>();
            int min = parts.Length > 1 && int.TryParse(parts[1], out int mn) ? mn : 0;
            int max = parts.Length > 2 && int.TryParse(parts[2], out int mx) ? mx : 99;

            ClampTextBoxValue(textBox, min, max);
        }

        /// <summary>
        /// El rango válido de Eyes Type depende del SkinColour seleccionado:
        /// SKIN_COLOUR_2 admite 1-46, el resto admite 1-23.
        /// </summary>
        private static int GetMaxEyesType(SkinColour skinColour) =>
            skinColour == SkinColour.SKIN_COLOUR_2 ? 46 : 23;

        private void EyesTypeArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement button || button.Parent is not StackPanel panel)
            {
                return;
            }

            TextBox textBox = panel.Children.OfType<TextBox>().FirstOrDefault();
            if (textBox == null)
            {
                return;
            }

            int delta = int.TryParse(button.Tag?.ToString(), out int d) ? d : 1;
            int maxEyesType = GetMaxEyesType(SelectedPlayer.SkinColour);
            AdjustStatTextBoxValue(textBox, delta, 1, maxEyesType);
        }

        private void EyesTypeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                int maxEyesType = GetMaxEyesType(SelectedPlayer.SkinColour);
                ClampTextBoxValue(textBox, 1, maxEyesType);
            }
        }

        private void PositionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (positionComboBox.SelectedItem is not Position position)
            {
                return;
            }

            switch (position.PositionId)
            {
                case 0: // GK
                    gkCheckBox.IsChecked = true;
                    break;
                case 2: // SWP
                    swCheckBox.IsChecked = true;
                    break;
                case 3: // CB
                    cbCheckBox.IsChecked = true;
                    break;
                case 4: // SB
                    sbCheckBox.IsChecked = true;
                    break;
                case 5: // DMF
                    dmfCheckBox.IsChecked = true;
                    break;
                case 6: // CMF
                    cmfCheckBox.IsChecked = true;
                    break;
                case 7: // SMF
                    smfCheckBox.IsChecked = true;
                    break;
                case 8: // OMF
                    amfCheckBox.IsChecked = true;
                    break;
                case 9: // CF
                    cfCheckBox.IsChecked = true;
                    break;
                case 10: // WF
                    wgCheckBox.IsChecked = true;
                    break;
            }
        }
    }
}
