using JWE8Editor.Logic;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Ventana que lista todas las caras preestablecidas disponibles para el
    /// SkinColour del jugador actual, junto con los jugadores de la base de
    /// datos cargada que ya tienen asignado cada FaceId, permitiendo reasignar
    /// (relink) el FaceId del jugador seleccionado en PlayerWindow.
    /// </summary>
    public partial class FaceRelinkWindow : Window
    {
        private static readonly string[] SkinColourPrefixes = { "W", "Y", "R", "K" };

        public int? SelectedFaceId { get; private set; }

        public ObservableCollection<FaceRelinkRow> Rows { get; } = new ObservableCollection<FaceRelinkRow>();

        public FaceRelinkWindow(SkinColour skinColour, IEnumerable<Player> allPlayers, int currentPlayerIndex)
        {
            InitializeComponent();
            DataContext = this;

            faceRelinkDataGrid.ItemsSource = Rows;

            BuildRows(skinColour, allPlayers, currentPlayerIndex);
        }

        private void BuildRows(SkinColour skinColour, IEnumerable<Player> allPlayers, int currentPlayerIndex)
        {
            int prefixIndex = (int)skinColour;
            if (prefixIndex < 0 || prefixIndex >= SkinColourPrefixes.Length)
            {
                return;
            }

            string prefix = SkinColourPrefixes[prefixIndex];

            // Jugadores (excluyendo al que se está editando) agrupados por FaceId,
            // para listar quién más tiene asignada cada cara. Solo se consideran
            // los jugadores con FaceType = PRESET, ya que son los únicos que usan
            // un FaceId de cara preestablecida; los de tipo BUILD/ORIGINAL no aplican.
            var playersByFaceId = allPlayers
                .Where(p => p != null && p.SkinColour == skinColour && p.PlayerIndex != currentPlayerIndex
                    && p.FaceType == FaceType.PRESET)
                .GroupBy(p => p.FaceId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var faceCodes = LoadFacePlayers.FacePlayers
                .Where(f => f.FaceCode != null && f.FaceCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f.FaceCode, StringComparer.OrdinalIgnoreCase);

            foreach (FacePlayer face in faceCodes)
            {
                if (!int.TryParse(face.FaceCode!.Substring(prefix.Length), out int faceId))
                {
                    continue;
                }

                string assignedPlayers = playersByFaceId.TryGetValue(faceId, out List<Player>? matches)
                    ? string.Join(", ", matches.Select(p => p.Name))
                    : string.Empty;

                Rows.Add(new FaceRelinkRow
                {
                    FaceId = faceId,
                    FaceCode = face.FaceCode,
                    FaceName = face.ToString(),
                    AssignedPlayers = assignedPlayers
                });
            }
        }

        private void ConfirmSelection()
        {
            if (faceRelinkDataGrid.SelectedItem is FaceRelinkRow row)
            {
                SelectedFaceId = row.FaceId;
                DialogResult = true;
                Close();
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void FaceRelinkDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }
    }
}