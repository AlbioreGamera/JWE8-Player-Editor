using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private SafeFileHandle filePath;
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

        public ObservableCollection<Player> People { get; set; } = new ObservableCollection<Player>();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "para_we8.bin player files (*.bin_000)|*.bin_000|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                ParseBinaryFile(openFileDialog.FileName);
                foreach (Player person in ParseBinaryFile(openFileDialog.FileName))
                {
                    People.Add(person);
                }
            }

        }

        private List<Player> ParseBinaryFile(string filePath)
        {
            List<Player> players = new List<Player>();

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Seek(124, SeekOrigin.Begin); // Start reading from the 7C byte

                byte[] buffer = new byte[0x7C]; // Each Player

                while (fs.Read(buffer, 0, buffer.Length) == buffer.Length)
                {
                    Player person = new Player
                    {
                        Name = Encoding.Unicode.GetString(buffer, 0, 32).Trim(),
                        ShirtName = Encoding.ASCII.GetString(buffer, 32, 16).Trim(), 
                        Country = BitConverter.ToInt32(buffer, 110),
                        Position = buffer[52] & 0x0F,
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
                nameTextBox.Text = selectedPerson.Name;
                shirtNameTextBox.Text = selectedPerson.ShirtName;
                countryTextBox.Text = selectedPerson.Country.ToString();
                positionComboBox.SelectedIndex = selectedPerson.Position;
            }
        }

    }
}
