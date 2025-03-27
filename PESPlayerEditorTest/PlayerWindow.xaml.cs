using JWE8Editor.Logic;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Interaction logic for PlayerWindow.xaml
    /// </summary>
    /// 

    public partial class PlayerWindow : Window
    {
        public Player SelectedPlayer { get; set; }

        public PlayerAssignment SelectedPlayerAssignment {get; set;}
        public static List<Position> Positions { get; } = new List<Position>
            {
                new Position { PositionId = 0, PositionValue = ["9E"], PositionName = "Goal Keeper" },
                new Position { PositionId = 1, PositionValue = ["41"], PositionName = "Sweeper" },
                new Position { PositionId = 2, PositionValue = ["35,3E"], PositionName = "Centrer Back" },
                new Position { PositionId = 3, PositionValue = ["3F","41","44"], PositionName = "Side Back" }, //41 RIGHT - 44 LEFT
                new Position { PositionId = 4, PositionValue = ["3D"], PositionName = "Defensive Midfielder" },
                new Position { PositionId = 5, PositionValue = ["3E"], PositionName = "Wing Back" }, 
                new Position { PositionId = 6, PositionValue = ["42"], PositionName = "Centre Midfielder" },
                new Position { PositionId = 7, PositionValue = ["46"], PositionName = "Side Midfielder" },// 46 RIGHT - ?? LEFT
                new Position { PositionId = 8, PositionValue = ["4C"], PositionName = "Attacking Midfielder" },
                new Position { PositionId = 9, PositionValue = ["4E"], PositionName = "Wing Forward" },
                new Position { PositionId = 10, PositionValue = ["4C"], PositionName = "Second Striker" },
                new Position { PositionId = 11, PositionValue = ["4D"], PositionName = "Centre Forward" },

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

        private MainWindow mainWindow;

        public PlayerWindow(Player selectedPlayer, MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            SelectedPlayer = selectedPlayer;
            DataContext = this;

            positionComboBox.ItemsSource = Positions;
            positionComboBox.SelectedItem = Positions.FirstOrDefault(p => p.PositionId == selectedPlayer.Position);

            ageComboBox.ItemsSource = Ages;
            ageComboBox.SelectedItem = Ages.FirstOrDefault(a => a.AgeIndex == selectedPlayer.Age);

            playerIdTextBox.Text = selectedPlayer.PlayerIndex.ToString();
            nameTextBox.Text = selectedPlayer.Name;
            shirtNameTextBox.Text = selectedPlayer.ShirtName;
            countryComboBox.SelectedIndex = selectedPlayer.Country;
            ageComboBox.SelectedIndex = selectedPlayer.Age;
            heightTextBox.Text = selectedPlayer.Height.ToString();
            weightTextBox.Text = selectedPlayer.Weight.ToString();
            positionComboBox.SelectedIndex = selectedPlayer.Position;
            commentaryComboBox.IsEnabled = true;
            commentaryComboBox.SelectedIndex = 0; //selectedPlayer.Commentary;
            //Stats
            statOffenseTextBox.Text = selectedPlayer.StatAttack.ToString();
            statDefenseTextBox.Text = selectedPlayer.StatDefense.ToString();
            statBodyBalanceTextBox.Text = selectedPlayer.StatBodyBalance.ToString();
            statStaminaTextBox.Text = selectedPlayer.StatStamina.ToString();
            statTopSpeedTextBox.Text = selectedPlayer.StatTopSpeed.ToString();
            statAccelerationTextBox.Text = selectedPlayer.StatAcceleration.ToString();
            statResponseTextBox.Text = selectedPlayer.StatResponse.ToString();
            statAgilityTextBox.Text = selectedPlayer.StatAgility.ToString();
            statDribbleAccuracyTextBox.Text = selectedPlayer.StatDribbleAccuracy.ToString();
            statDribbleSpeedTextBox.Text = selectedPlayer.StatDribbleSpeed.ToString();
            statShortPassAccTextBox.Text = selectedPlayer.StatShortPassAccuracy.ToString();
            statShortPassSpeedTextBox.Text = selectedPlayer.StatShortPassSpeed.ToString();
            statLongPassAccTextBox.Text = selectedPlayer.StatLongPassAccuracy.ToString();
            statLongPassSpeedTextBox.Text = selectedPlayer.StatLongPassSpeed.ToString();
            statShootAccTextBox.Text = selectedPlayer.StatShotAccuracy.ToString();
            statShootPowerTextBox.Text = selectedPlayer.StatShotPower.ToString();
            statShootTechniqueTextBox.Text = selectedPlayer.StatShotTechnique.ToString();
            statFreeKickTextBox.Text = selectedPlayer.StatFreeKickAccuracy.ToString();
            statSwerveTextBox.Text = selectedPlayer.StatCurve.ToString();
            statHeadingTextBox.Text = selectedPlayer.StatHeader.ToString();
            statJumpTextBox.Text = selectedPlayer.StatJump.ToString();
            statTechniqueTextBox.Text = selectedPlayer.StatTechnique.ToString();
            statAggresionTextBox.Text = selectedPlayer.StatAggression.ToString();
            statMentalityTextBox.Text = selectedPlayer.StatMentality.ToString();
            statTeamWorkTextBox.Text = selectedPlayer.StatTeamWorkAbility.ToString();
            conditionComboBox.SelectedIndex = 0;
            WeakFootAccComboBox.SelectedIndex = 0;
            WeakFootUseComboBox.SelectedIndex = 0;

            //Skills
            //Appeareance
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
