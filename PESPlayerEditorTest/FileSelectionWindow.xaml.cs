﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
            LoadFilePaths();
            _mainWindow = mainWindow;
        }

        public void CheckExistingFiles()
        {
            List<FilePathInfo> filePathInfos = LoadFilePaths();

            foreach (var filePathInfo in filePathInfos)
            {
                if (filePathInfo.Name != null)
                {
                    if (filePathInfo.Name.Equals("para_we8_bin_000"))
                    {
                        _mainWindow.FilePath1 = filePathInfo.FilePath;
                    }

                    if (filePathInfo.Name.Equals("para_we8_bin_002"))
                    {
                        _mainWindow.FilePath2 = filePathInfo.FilePath;
                    }

                    if (filePathInfo.Name.Equals("para_we8_bin_004"))
                    {
                        _mainWindow.FilePath3 = filePathInfo.FilePath;
                    }

                    if (filePathInfo.Name.Equals("defaultdataset"))
                    {
                        _mainWindow.FilePath4 = filePathInfo.FilePath;
                    }

                    if (filePathInfo.Name.Equals("edit_ovl"))
                    {
                        _mainWindow.FilePath5 = filePathInfo.FilePath;
                    }
                }
            }
        }

        public void SaveFilePaths(List<FilePathInfo> filePathInfos)
        {
            string folderPath = Path.Combine("config");
            string cfgFilePath = Path.Combine(folderPath, "filepaths.cfg");

            Directory.CreateDirectory(folderPath);

            using (StreamWriter writer = new StreamWriter(cfgFilePath))
            {
                foreach (var filePathInfo in filePathInfos)
                {
                    writer.WriteLine(filePathInfo);
                }
            }
        }

        public List<FilePathInfo> LoadFilePaths()
        {
            List<FilePathInfo> filePathInfos = new List<FilePathInfo>();
            string cfgFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\filepaths.cfg");

            System.IO.Directory.GetCurrentDirectory();

            if (File.Exists(cfgFilePath))
            {
                using (StreamReader reader = new StreamReader(cfgFilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            filePathInfos.Add(new FilePathInfo { Name = parts[0], FilePath = parts[1] });
                        }
                    }
                }
            }

            return filePathInfos;
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

        private void SelectFile3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "para_we8.bin player files (*.bin_004)|*.bin_004|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string bin_004 = openFileDialog.FileName;
                para_we8_bin_004.Text = openFileDialog.FileName;
            }
        }

        private void SelectFile4_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "defaultdataset.ovl file (*.ovl)|*.ovl|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string dataset = openFileDialog.FileName;
                defaultdataset.Text = openFileDialog.FileName;
            }
        }

        private void SelectFile5_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "edit.ovl file (*.ovl)|*.ovl|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string edit = openFileDialog.FileName;
                edit_ovl.Text = openFileDialog.FileName;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string selectedFilePath_000 = para_we8_bin_000.Text;
            string selectedFilePath_002 = para_we8_bin_002.Text;
            string selectedFilePath_004 = para_we8_bin_004.Text;
            string selectedFilePath_edit = edit_ovl.Text;
            string selectedFilePath_dataset = defaultdataset.Text;

            if (string.IsNullOrEmpty(selectedFilePath_000) || string.IsNullOrEmpty(selectedFilePath_002) || string.IsNullOrEmpty(selectedFilePath_004))
            {
                MessageBox.Show("Please select all files before proceeding.");
                return; // Exit the method
            }

            if(string.IsNullOrEmpty(selectedFilePath_dataset))
            {
                MessageBox.Show("No Stadium data will be loaded.");
            }
            else
            {
                _mainWindow.ParseStadiums(selectedFilePath_dataset);
                _mainWindow.stadiumListBox.IsEnabled = true;
                _mainWindow.stadiumListBox.SelectedIndex = 0;
            }

            if (string.IsNullOrEmpty(selectedFilePath_edit))
            {
                MessageBox.Show("No Callname data will be loaded.");
            }
            else
            {
                _mainWindow.ParseCallnames(selectedFilePath_edit);
                _mainWindow.callnameListBox.IsEnabled = true;
                _mainWindow.callnameListBox.SelectedIndex = 0;
                _mainWindow.callnameTextBoxC.IsEnabled = true;
            }

            _mainWindow.ParsePlayers(selectedFilePath_000);
            _mainWindow.personListBox.IsEnabled = true;
            _mainWindow.personListBox.SelectedIndex = 0;

            _mainWindow.ParsePlayerAssignment(selectedFilePath_002, selectedFilePath_004);
            _mainWindow.ParseAssignment(selectedFilePath_002, selectedFilePath_004);

            List<FilePathInfo> filePathInfos = new List<FilePathInfo>
            {
                new FilePathInfo { Name = "para_we8_bin_000", FilePath = selectedFilePath_000 },
                new FilePathInfo { Name = "para_we8_bin_002", FilePath = selectedFilePath_002 },
                new FilePathInfo { Name = "para_we8_bin_004", FilePath = selectedFilePath_004 },
                new FilePathInfo { Name = "edit_ovl", FilePath = selectedFilePath_dataset },
                new FilePathInfo { Name = "defaultdataset", FilePath = selectedFilePath_edit },
            };

            SaveFilePaths(filePathInfos);
            _mainWindow.saveHeader.IsEnabled = true;
            _mainWindow.saveAsHeader.IsEnabled = true;
            _mainWindow.PopulateTeamsComboBox();
            this.Close();
        }
    }
}
