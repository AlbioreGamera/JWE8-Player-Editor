using JWE8Editor.Logic;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using PlayerLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace PESPlayerEditorTest
{
    /// <summary>
    /// Interaction logic for FileSelectionWindow.xaml
    /// </summary>
    public partial class FileSelectionWindow : Window
    {
        public SafeFileHandle filePath;

        private readonly MainWindow _mainWindow;

        private Dictionary<string, string> _savedFilePaths = new Dictionary<string, string>();

        public FileSelectionWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            LoadFilePaths();
            _mainWindow = mainWindow;
            PopulateTextBoxesFromSavedPaths();
        }

        /// <summary>
        /// Carga, si existen y siguen siendo válidas (archivo presente en disco),
        /// las rutas "fuente" guardadas en una sesión anterior (AFS raíz, OVER.afs
        /// y SLPM_657.40), asignándolas directamente al MainWindow para que la
        /// ventana principal pueda arrancar sin requerir reprocesar manualmente.
        /// NOTA: ya no se guardan/restauran rutas de archivos extraídos
        /// individuales (_000/_002/_004/ovl), ya que ahora esos se regeneran
        /// internamente cada vez que se pulsa OK.
        /// </summary>
        public void CheckExistingFiles()
        {
            List<FilePathInfo> filePathInfos = LoadFilePaths();

            foreach (var filePathInfo in filePathInfos)
            {
                if (filePathInfo.Name == null)
                {
                    continue;
                }

                if (filePathInfo.Name.Equals("data_afs") && File.Exists(filePathInfo.FilePath))
                {
                    _mainWindow.DataAfsSourcePath = filePathInfo.FilePath;
                }

                if (filePathInfo.Name.Equals("over_afs") && File.Exists(filePathInfo.FilePath))
                {
                    _mainWindow.OverAfsSourcePath = filePathInfo.FilePath;
                }

                if (filePathInfo.Name.Equals("slpm_657_40") && File.Exists(filePathInfo.FilePath))
                {
                    _mainWindow.FilePath6 = filePathInfo.FilePath;
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

            _savedFilePaths = filePathInfos
                .Where(f => f.Name != null)
                .GroupBy(f => f.Name)
                .ToDictionary(g => g.Key, g => g.First().FilePath);

            return filePathInfos;
        }

        private void PopulateTextBoxesFromSavedPaths()
        {
            dataAfsPath.Text = GetSavedPathIfExists("data_afs");
            overAfsPath.Text = GetSavedPathIfExists("over_afs");
            slpmPath.Text = GetSavedPathIfExists("slpm_657_40");
        }

        private string GetSavedPathIfExists(string name)
        {
            if (_savedFilePaths.TryGetValue(name, out string savedPath) && File.Exists(savedPath))
            {
                return savedPath;
            }
            return string.Empty;
        }

        private string GetInitialDirectory(string name)
        {
            if (_savedFilePaths.TryGetValue(name, out string savedPath) && !string.IsNullOrEmpty(savedPath))
            {
                string directory = Path.GetDirectoryName(savedPath);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    return directory;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Carpeta temporal usada para volcar los archivos extraídos de los AFS
        /// (sub-archivos de para_we8.bin, archivos extraídos de OVER.afs, etc.).
        /// Se limpia y recrea en cada apertura de un AFS.
        /// </summary>
        private static string GetAfsTempDirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "JWE8Editor_AfsExtract");
            return tempDir;
        }

        private void SelectDataAfs_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "AFS archive (*.afs;*.bin;*.*)|*.afs;*.bin;*.*|All files (*.*)|*.*",
                Title = "Seleccionar archivo AFS raíz (disco del juego)",
                InitialDirectory = GetInitialDirectory("data_afs")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                dataAfsPath.Text = openFileDialog.FileName;
            }
        }

        private void SelectOverAfs_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "OVER.afs archive (*.afs;*.*)|*.afs;*.*|All files (*.*)|*.*",
                Title = "Seleccionar OVER.afs",
                InitialDirectory = GetInitialDirectory("over_afs")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                overAfsPath.Text = openFileDialog.FileName;
            }
        }

        private void SelectSlpm_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "SLPM_657.40 file (SLPM_657.40)|SLPM_657.40|All files (*.*)|*.*",
                Title = "Seleccionar SLPM_657.40",
                InitialDirectory = GetInitialDirectory("slpm_657_40")
            };

            if (openFileDialog.ShowDialog() == true)
            {
                slpmPath.Text = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// Resultado combinado de procesar, en segundo plano, tanto el AFS raíz
        /// como OVER.afs. Las rutas aquí contenidas son archivos temporales
        /// internos (no se muestran en la UI): solo se usan para alimentar el
        /// parseo de MainWindow tras pulsar OK.
        /// </summary>
        private class CombinedExtractionResult
        {
            public string? ParaWe8Bin000Path;
            public string? ParaWe8Bin002Path;
            public string? ParaWe8Bin004Path;
            public string? DefaultDatasetPath;
            public string? EditOvlPath;
            public List<string> Warnings { get; } = new List<string>();
        }

        /// <summary>
        /// Punto de entrada único: procesa el AFS raíz (data) y, si se indicó,
        /// OVER.afs, extrayendo internamente todos los sub-archivos necesarios
        /// sin exponer ninguna ruta intermedia en la UI.
        /// </summary>
        private static CombinedExtractionResult ProcessAll(string dataAfsPath, string? overAfsPath, IProgress<string> progress)
        {
            var combined = new CombinedExtractionResult();

            AfsProcessResult afsResult = ProcessAfs(dataAfsPath, progress);
            combined.ParaWe8Bin000Path = afsResult.ParaWe8Bin000Path;
            combined.ParaWe8Bin002Path = afsResult.ParaWe8Bin002Path;
            combined.ParaWe8Bin004Path = afsResult.ParaWe8Bin004Path;
            combined.Warnings.AddRange(afsResult.Warnings);

            if (!string.IsNullOrEmpty(overAfsPath))
            {
                OverAfsProcessResult overResult = ProcessOverAfs(overAfsPath, progress);
                combined.DefaultDatasetPath = overResult.DefaultDatasetPath;
                combined.EditOvlPath = overResult.EditOvlPath;
                combined.Warnings.AddRange(overResult.Warnings);
            }

            return combined;
        }

        /// <summary>
        /// Resultado del procesamiento en segundo plano del AFS raíz (disco del juego).
        /// </summary>
        private class AfsProcessResult
        {
            public string? ParaWe8Bin000Path;
            public string? ParaWe8Bin002Path;
            public string? ParaWe8Bin004Path;
            public List<string> Warnings { get; } = new List<string>();
        }

        /// <summary>
        /// Lógica de procesamiento del AFS raíz, ejecutada en un hilo de fondo
        /// (Task.Run). No debe tocar directamente controles de la UI; solo reporta
        /// avance a través de <paramref name="progress"/>.
        ///
        /// Solo se expanden los sub-contenedores estrictamente necesarios dentro
        /// del AFS raíz del disco: BOOT.afs (contiene para_we8.bin), FLAG.afs y
        /// GAME.afs (contiene unnamed_47.afs). El resto de entradas del disco
        /// (potencialmente cientos, con gráficos/audio pesado) se ignoran por
        /// completo para evitar procesamiento innecesario.
        /// </summary>
        private static AfsProcessResult ProcessAfs(string afsPath, IProgress<string> progress)
        {
            var result = new AfsProcessResult();

            progress.Report("Leyendo cabecera del archivo AFS raíz...");

            string tempDir = GetAfsTempDirectory();
            Directory.CreateDirectory(tempDir);

            AfsArchive rootArchive = AfsArchive.Load(afsPath);

            // Solo expandir los contenedores de primer nivel que realmente
            // necesitamos, en vez de recorrer y analizar las cientos de entradas
            // del disco completo.
            string[] requiredContainers = { "BOOT.afs", "FLAG.afs", "GAME.afs" };

            foreach (string containerName in requiredContainers)
            {
                AfsEntry? containerEntry = rootArchive.FindFirstLevelEntryByName(containerName);
                if (containerEntry == null)
                {
                    result.Warnings.Add($"No se encontró '{containerName}' en el AFS raíz seleccionado.");
                    continue;
                }

                progress.Report($"Expandiendo {containerName}...");

                byte[] data = AfsArchive.ExtractEntry(containerEntry);
                if (!AfsArchive.LooksLikeAfs(data))
                {
                    result.Warnings.Add($"'{containerName}' no tiene una firma AFS válida; se omite.");
                    continue;
                }

                string nestedPath = Path.Combine(tempDir, containerName);
                File.WriteAllBytes(nestedPath, data);

                containerEntry.NestedArchive = AfsArchive.LoadFromBytes(data, nestedPath, containerEntry.VirtualPath);

                // GAME.afs contiene a su vez unnamed_47.afs; se expande un nivel más
                // únicamente dentro de este contenedor (no recursivo sobre todo el disco).
                if (string.Equals(containerName, "GAME.afs", StringComparison.OrdinalIgnoreCase))
                {
                    progress.Report("Buscando unnamed_47.afs dentro de GAME.afs...");

                    AfsEntry? unnamed47Entry = containerEntry.NestedArchive.FindFirstLevelEntryByName("unnamed_47.afs");
                    if (unnamed47Entry != null)
                    {
                        byte[] unnamed47Data = AfsArchive.ExtractEntry(unnamed47Entry);
                        if (AfsArchive.LooksLikeAfs(unnamed47Data))
                        {
                            string unnamed47Path = Path.Combine(tempDir, "unnamed_47.afs");
                            File.WriteAllBytes(unnamed47Path, unnamed47Data);
                            unnamed47Entry.NestedArchive = AfsArchive.LoadFromBytes(unnamed47Data, unnamed47Path, unnamed47Entry.VirtualPath);
                        }
                        else
                        {
                            result.Warnings.Add("'unnamed_47.afs' no tiene una firma AFS válida; se omite.");
                        }
                    }
                    else
                    {
                        result.Warnings.Add("No se encontró 'unnamed_47.afs' dentro de GAME.afs.");
                    }
                }

                // BOOT.afs contiene a su vez uno o más AFS anidados (ej. "BOOT_0")
                // y es ahí donde realmente vive para_we8.bin (ruta completa:
                // BOOT\BOOT_0\para_we8.bin). Como el nombre exacto del contenedor
                // puede variar entre versiones/regiones del juego, en vez de buscar
                // un nombre fijo se expande CUALQUIER entrada de primer nivel dentro
                // de BOOT.afs cuyo contenido tenga firma AFS válida. Sin expandir
                // este nivel intermedio, las búsquedas recursivas posteriores
                // (FindByVirtualPathSuffix / FindAllByNameSuffix) nunca llegan a
                // para_we8.bin ni a sus sub-archivos _000/_002/_004.
                if (string.Equals(containerName, "BOOT.afs", StringComparison.OrdinalIgnoreCase))
                {
                    progress.Report("Buscando sub-contenedores AFS dentro de BOOT.afs...");

                    bool foundAnyNestedAfs = false;

                    foreach (AfsEntry innerEntry in containerEntry.NestedArchive.Entries)
                    {
                        if (!AfsArchive.LooksLikeAfsHeaderOnly(innerEntry))
                        {
                            continue;
                        }

                        foundAnyNestedAfs = true;

                        byte[] innerData = AfsArchive.ExtractEntry(innerEntry);
                        if (!AfsArchive.LooksLikeAfs(innerData))
                        {
                            result.Warnings.Add($"'{innerEntry.Name}' (dentro de BOOT.afs) no tiene una firma AFS válida; se omite.");
                            continue;
                        }

                        string innerPath = Path.Combine(tempDir, $"BOOT_{innerEntry.Name}");
                        File.WriteAllBytes(innerPath, innerData);
                        innerEntry.NestedArchive = AfsArchive.LoadFromBytes(innerData, innerPath, innerEntry.VirtualPath);
                    }

                    if (!foundAnyNestedAfs)
                    {
                        string entryNames = string.Join(", ", containerEntry.NestedArchive.Entries.Select(e => e.Name));
                        result.Warnings.Add(
                            "No se encontró ningún AFS anidado dentro de BOOT.afs (se esperaba algo como 'BOOT_0'). " +
                            $"Entradas disponibles: {entryNames}");
                    }
                }
            }

            // 1) Buscar primero si _000/_002/_004 existen como ENTRADAS SEPARADAS
            // dentro de los contenedores ya expandidos (ej. "para_we8.bin_000").
            progress.Report("Buscando sub-archivos de para_we8.bin...");

            List<AfsEntry> directSubFiles = rootArchive.FindAllByNameSuffix("para_we8.bin_000")
                .Concat(rootArchive.FindAllByNameSuffix("para_we8.bin_002"))
                .Concat(rootArchive.FindAllByNameSuffix("para_we8.bin_004"))
                .ToList();

            if (directSubFiles.Count > 0)
            {
                foreach (var entry in directSubFiles)
                {
                    byte[] bytes = AfsArchive.ExtractEntry(entry);
                    string outPath = Path.Combine(tempDir, entry.Name);
                    File.WriteAllBytes(outPath, bytes);

                    if (entry.Name.EndsWith("_000", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParaWe8Bin000Path = outPath;
                    }
                    else if (entry.Name.EndsWith("_002", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParaWe8Bin002Path = outPath;
                    }
                    else if (entry.Name.EndsWith("_004", StringComparison.OrdinalIgnoreCase))
                    {
                        result.ParaWe8Bin004Path = outPath;
                    }
                }
            }

            // 2) Si no se encontraron TODAS como entradas separadas, recurrir a la
            // descompresión zlib de "para_we8.bin" para rellenar las que falten.
            // Confirmado contra DKZ Studio / Zlib Editor: el archivo contiene 5
            // sub-archivos independientes (unknow_000.._004), cada uno con su propio
            // stream zlib. El editor solo USA _000 (jugadores), _002 y _004, pero se
            // vuelcan a disco los 5 (incluyendo _001 y _003) para dejar una extracción
            // completa, aunque el resto no se utilice en la UI.
            if (result.ParaWe8Bin000Path == null || result.ParaWe8Bin002Path == null || result.ParaWe8Bin004Path == null)
            {
                AfsEntry? paraWe8Entry = rootArchive.FindByVirtualPathSuffix("para_we8.bin");
                if (paraWe8Entry != null)
                {
                    progress.Report("Descomprimiendo para_we8.bin (zlib)...");

                    byte[] paraWe8Bytes = AfsArchive.ExtractEntry(paraWe8Entry);
                    string paraWe8Path = Path.Combine(tempDir, "para_we8.bin");
                    File.WriteAllBytes(paraWe8Path, paraWe8Bytes);

                    List<byte[]> subFiles = ParaWe8Reader.DecompressAll(paraWe8Path);

                    // Se vuelcan a disco TODOS los sub-archivos detectados
                    // (unknow_000.._004), aunque el editor solo use _000/_002/_004.
                    for (int i = 0; i < subFiles.Count; i++)
                    {
                        string subFilePath = Path.Combine(tempDir, $"para_we8_bin_{i:D3}");
                        File.WriteAllBytes(subFilePath, subFiles[i]);

                        if (i == 0 && result.ParaWe8Bin000Path == null)
                        {
                            result.ParaWe8Bin000Path = subFilePath;
                        }
                        else if (i == 2 && result.ParaWe8Bin002Path == null)
                        {
                            result.ParaWe8Bin002Path = subFilePath;
                        }
                        else if (i == 4 && result.ParaWe8Bin004Path == null)
                        {
                            result.ParaWe8Bin004Path = subFilePath;
                        }
                    }

                    if (subFiles.Count < 5)
                    {
                        result.Warnings.Add(
                            $"Solo se detectaron {subFiles.Count} stream(s) zlib dentro de para_we8.bin " +
                            "(se esperaban 5: unknow_000.._004). Revisa el archivo seleccionado.");
                    }
                }
                else
                {
                    result.Warnings.Add("No se encontró 'para_we8.bin' (BOOT.afs\\BOOT_0\\para_we8.bin) dentro del AFS seleccionado.");
                }
            }

            progress.Report("Finalizando AFS raíz...");

            return result;
        }

        /// <summary>
        /// Resultado del procesamiento de OVER.afs.
        /// </summary>
        private class OverAfsProcessResult
        {
            public string? DefaultDatasetPath;
            public string? EditOvlPath;
            public List<string> Warnings { get; } = new List<string>();
        }

        /// <summary>
        /// Lógica de procesamiento de OVER.afs, ejecutada en un hilo de fondo.
        /// Busca directamente (sin asumir anidamiento) las entradas
        /// "defaultdataset.ovl" y "edit.ovl" dentro de este contenedor.
        /// </summary>
        private static OverAfsProcessResult ProcessOverAfs(string overAfsPath, IProgress<string> progress)
        {
            var result = new OverAfsProcessResult();

            progress.Report("Leyendo cabecera de OVER.afs...");

            string tempDir = GetAfsTempDirectory();
            Directory.CreateDirectory(tempDir);

            AfsArchive overArchive = AfsArchive.Load(overAfsPath);

            progress.Report($"Buscando defaultdataset.ovl y edit.ovl entre {overArchive.Entries.Count} entradas...");

            AfsEntry? datasetEntry = overArchive.FindByVirtualPathSuffix("defaultdataset.ovl");
            if (datasetEntry != null)
            {
                byte[] datasetBytes = AfsArchive.ExtractEntry(datasetEntry);
                string datasetPath = Path.Combine(tempDir, "defaultdataset.ovl");
                File.WriteAllBytes(datasetPath, datasetBytes);
                result.DefaultDatasetPath = datasetPath;
            }
            else
            {
                result.Warnings.Add("No se encontró 'defaultdataset.ovl' dentro de OVER.afs.");
            }

            AfsEntry? editEntry = overArchive.FindByVirtualPathSuffix("edit.ovl");
            if (editEntry != null)
            {
                byte[] editBytes = AfsArchive.ExtractEntry(editEntry);
                string editPath = Path.Combine(tempDir, "edit.ovl");
                File.WriteAllBytes(editPath, editBytes);
                result.EditOvlPath = editPath;
            }
            else
            {
                result.Warnings.Add("No se encontró 'edit.ovl' dentro de OVER.afs.");
            }

            progress.Report("Finalizando OVER.afs...");

            return result;
        }

        /// <summary>
        /// Al pulsar OK se dispara TODO el procesamiento: extracción del AFS raíz
        /// (data), de OVER.afs (si se indicó) y la carga de datos en MainWindow,
        /// todo en segundo plano para no congelar la UI. Ninguna ruta de archivo
        /// extraído se muestra en pantalla: solo se usan internamente.
        /// </summary>
        private async void Ok_Click(object sender, RoutedEventArgs e)
        {
            string selectedDataAfsPath = dataAfsPath.Text;
            string selectedOverAfsPath = overAfsPath.Text;
            string selectedSlpmPath = slpmPath.Text;

            if (string.IsNullOrEmpty(selectedDataAfsPath))
            {
                MessageBox.Show("Selecciona el archivo Data (AFS) antes de continuar.");
                return;
            }

            if (!File.Exists(selectedDataAfsPath))
            {
                MessageBox.Show("El archivo Data (AFS) seleccionado ya no existe.");
                return;
            }

            if (!string.IsNullOrEmpty(selectedOverAfsPath) && !File.Exists(selectedOverAfsPath))
            {
                MessageBox.Show("El archivo OVER.afs seleccionado ya no existe.");
                return;
            }

            if (!string.IsNullOrEmpty(selectedSlpmPath) && !File.Exists(selectedSlpmPath))
            {
                MessageBox.Show("El archivo SLPM_657.40 seleccionado ya no existe.");
                return;
            }

            var progressWindow = new ProgressWindow
            {
                Owner = this
            };
            progressWindow.Show();

            var progress = new Progress<string>(status =>
            {
                progressWindow.UpdateStatus(status);
            });

            CombinedExtractionResult result;

            try
            {
                result = await Task.Run(() => ProcessAll(selectedDataAfsPath, selectedOverAfsPath, progress));
            }
            catch (Exception ex)
            {
                progressWindow.Close();
                MessageBox.Show($"Error al procesar los archivos seleccionados: {ex.Message}\n\n{ex.StackTrace}", "Error");
                return;
            }

            if (result.ParaWe8Bin000Path == null || result.ParaWe8Bin002Path == null || result.ParaWe8Bin004Path == null)
            {
                progressWindow.Close();
                string errorMessage = "No se pudieron extraer todos los datos necesarios de jugadores desde el archivo Data (AFS) seleccionado.";
                if (result.Warnings.Count > 0)
                {
                    errorMessage += "\n\nAvisos:\n" + string.Join("\n", result.Warnings);
                }
                MessageBox.Show(errorMessage, "Error de extracción");
                return;
            }

            ((IProgress<string>)progress).Report("Cargando datos...");

            if (!string.IsNullOrEmpty(result.DefaultDatasetPath))
            {
                _mainWindow.ParseStadiums(result.DefaultDatasetPath);
                _mainWindow.stadiumListBox.IsEnabled = true;
                _mainWindow.stadiumListBox.SelectedIndex = 0;
            }

            if (!string.IsNullOrEmpty(result.EditOvlPath))
            {
                _mainWindow.ParseCallnames(result.EditOvlPath);
                _mainWindow.callnameListBox.IsEnabled = true;
                _mainWindow.callnameListBox.SelectedIndex = 0;
                _mainWindow.callnameTextBoxC.IsEnabled = true;
            }

            if (!string.IsNullOrEmpty(selectedSlpmPath))
            {
                _mainWindow.ParseTeams(selectedSlpmPath);
                _mainWindow.ParseNationalities(selectedSlpmPath);
            }

            _mainWindow.ParsePlayers(result.ParaWe8Bin000Path);
            _mainWindow.personListBox.IsEnabled = true;
            _mainWindow.personListBox.SelectedIndex = 0;

            _mainWindow.ParsePlayerAssignment(result.ParaWe8Bin002Path, result.ParaWe8Bin004Path);
            _mainWindow.ParseAssignment(result.ParaWe8Bin002Path, result.ParaWe8Bin004Path);

            // Se guardan únicamente las rutas "fuente" elegidas por el usuario
            // (AFS raíz, OVER.afs, SLPM), NO las rutas de archivos extraídos
            // internamente: estos se regeneran cada vez que se pulsa OK.
            List<FilePathInfo> filePathInfos = new List<FilePathInfo>
            {
                new FilePathInfo { Name = "data_afs", FilePath = selectedDataAfsPath },
                new FilePathInfo { Name = "over_afs", FilePath = selectedOverAfsPath },
                new FilePathInfo { Name = "slpm_657_40", FilePath = selectedSlpmPath },
            };

            SaveFilePaths(filePathInfos);

            progressWindow.Close();

            if (result.Warnings.Count > 0)
            {
                MessageBox.Show("Carga completada con avisos:\n\n" + string.Join("\n", result.Warnings));
            }

            _mainWindow.saveHeader.IsEnabled = true;
            _mainWindow.saveAsHeader.IsEnabled = true;
            _mainWindow.PopulateTeamsComboBox();
            this.Close();
        }
    }
}