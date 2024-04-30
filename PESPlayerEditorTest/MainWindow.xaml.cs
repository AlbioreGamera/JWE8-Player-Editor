using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        public static List<Country> Countries { get; } = new List<Country>
            {
                new Country { CountryIndex = 0, CountryId = 121, CountryName = "Free" },
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
                new Country { CountryIndex = 70, CountryId = 191, CountryName = "Nigieria" },
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

        List<Player> players = new List<Player>();

        OpenFileDialog openFileDialog = new OpenFileDialog();

        public ObservableCollection<Player> People { get; set; } = new ObservableCollection<Player>();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Filter = "para_we8.bin player files (*.bin_000)|*.bin_000|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ParseBinaryFile(openFileDialog.FileName);
                foreach (Player person in ParseBinaryFile(openFileDialog.FileName))
                {
                    People.Add(person);
                }
                personListBox.SelectedIndex = 0;
            }

        }

        private List<Player> ParseBinaryFile(string filePath)
        {

            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fileStream.Seek(124, SeekOrigin.Begin); // Start reading from the 7C byte

                byte[] buffer = new byte[0x7C]; // Each Player

                int playerIndex = 1;
                while (fileStream.Read(buffer, 0, buffer.Length) == buffer.Length)
                {
                    Player person = new Player
                    {
                        PlayerIndex = playerIndex++,
                        Name = Encoding.Unicode.GetString(buffer, 0, 32).Trim(),
                        ShirtName = Encoding.ASCII.GetString(buffer, 32, 16).Trim(),
                        unparsedData1 = BitConverter.ToString(buffer, 42, 10),
                        Country = BitConverter.ToInt32(buffer, 110),
                        unparsedData2 = BitConverter.ToString(buffer, 53, 58),
                        Position = buffer[52] & 0x0F,
                        unparsedData3 = BitConverter.ToString(buffer, 111, 13).Replace("-", " "),
                    };
                    People.Add(person);
                }
            }
            return players;
        }

        private void PersonListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (personListBox.SelectedItem != null)
            {
                Player selectedPerson = (Player)personListBox.SelectedItem;

                nameLabel.Content = selectedPerson.Name;
                playerIdTextBox.Text = selectedPerson.PlayerIndex.ToString();
                nameTextBox.Text = selectedPerson.Name;
                shirtNameTextBox.Text = selectedPerson.ShirtName;
                countryComboBox.SelectedIndex = selectedPerson.Country - 121;
                positionComboBox.SelectedIndex = selectedPerson.Position;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (personListBox.SelectedItem != null)
            {
                Player selectedPlayer = (Player)personListBox.SelectedItem;
                selectedPlayer.Name = nameTextBox.Text;
                selectedPlayer.ShirtName = shirtNameTextBox.Text;
                selectedPlayer.Country = countryComboBox.SelectedIndex + 121;
                positionComboBox.SelectedIndex = positionComboBox.SelectedIndex;
                // Optionally, you can update the ListBox to reflect the changes
                personListBox.Items.Refresh();
            }
        }

        private void SaveChangesToFile(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                foreach (var player in People)
                {
                    // Calcular el desplazamiento en el archivo basado en el índice del jugador
                    long byteOffset = player.PlayerIndex * 0x7C; // 0x7C es el tamaño de cada jugador

                    // Convertir el desplazamiento de bytes a desplazamiento de bits y sumar 124 (15 bytes * 8 bits por byte) para el desplazamiento inicial
                    long bitOffset = byteOffset * 8; // Comenzar a escribir en el bit 124

                    // Volver al inicio del archivo para escribir los cambios
                    fileStream.Seek(0, SeekOrigin.Begin);

                    using (BinaryWriter writer = new BinaryWriter(fileStream, Encoding.Unicode, true))
                    {
                        fileStream.Seek(bitOffset / 8, SeekOrigin.Begin);
                        byte[] nameBytes = Encoding.Unicode.GetBytes(player.Name.PadRight(12, '\0'));
                        writer.Write(nameBytes);

                        fileStream.Seek((bitOffset + 32 * 8) / 8, SeekOrigin.Begin);
                        byte[] shirtNameBytes = Encoding.ASCII.GetBytes(player.ShirtName.PadRight(16, '\0').Substring(0, 16));
                        writer.Write(shirtNameBytes);

                        //fileStream.Seek((bitOffset + 52 * 8) / 8, SeekOrigin.Begin);
                        //byte existingPosition = (byte)(fileStream.ReadByte() & 0x0F);

                        // Escribir solo si el valor existente no es cero
                        //if (existingPosition != 0)
                        //{
                        //    fileStream.Seek(-1, SeekOrigin.Current); // Retroceder un byte para sobrescribir el valor existente
                        //    writer.Write(existingPosition); // Escribir el valor existente
                        //}

                        fileStream.Seek((int)(bitOffset + 110 * 8) / 8, SeekOrigin.Begin);
                        writer.Write(player.Country);
                    }
                }
            }
        }

        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
             SaveChangesToFile(openFileDialog.FileName);
             People.Clear();
             ParseBinaryFile(openFileDialog.FileName);
        }
    }
}
