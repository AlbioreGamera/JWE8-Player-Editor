using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Globalization;
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
    /// Interaction logic for FileSelectionWindow.xaml
    /// </summary>
    public partial class FileSelectionWindow : Window
    {
        public SafeFileHandle filePath;

        private readonly MainWindow _mainWindow;

        public FileSelectionWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void SelectFile1_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "para_we8.bin player files (*.bin_000)|*.bin_000|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string bin_000 = openFileDialog.FileName;
                para_we8_bin_000.Text = openFileDialog.FileName;
            }
        }

        private void SelectFile2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "para_we8.bin player files (*.bin_002)|*.bin_002|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string bin_002 = openFileDialog.FileName;
                para_we8_bin_002.Text = openFileDialog.FileName;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath_000 = para_we8_bin_000.Text;
            string selectedFilePath_002 = para_we8_bin_002.Text;

            _mainWindow.ParsePlayers(selectedFilePath_000);
            foreach (Player person in _mainWindow.ParsePlayers(selectedFilePath_000))
            {
                _mainWindow.People.Add(person);
            }
            _mainWindow.personListBox.SelectedIndex = 0;

            _mainWindow.ParsePlayerAssignment(selectedFilePath_002);
            foreach (PlayerAssignment personA in _mainWindow.ParsePlayerAssignment(selectedFilePath_002))
            {
                _mainWindow.PeopleA.Add(personA);
            }
            _mainWindow.personListBox.SelectedIndex = 0;

            this.Close();



        }
    }
}
