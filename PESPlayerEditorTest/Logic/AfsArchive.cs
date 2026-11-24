using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Representa una entrada (archivo) dentro de un archivo AFS.
    /// </summary>
    public class AfsEntry
    {
        public int Index;
        public string Name = string.Empty;
        public uint Offset;
        public uint Size;

        /// <summary>
        /// Ruta "virtual" completa dentro del contenedor AFS raíz, usando '\' como
        /// separador (ej. "BOOT\BOOT_0\para_we8.bin"). Se rellena al recorrer
        /// recursivamente archivos AFS anidados.
        /// </summary>
        public string VirtualPath = string.Empty;

        /// <summary>
        /// Si esta entrada es a su vez un archivo AFS, aquí se guarda el archivo
        /// anidado ya parseado.
        /// </summary>
        public AfsArchive? NestedArchive;

        /// <summary>
        /// Archivo AFS (físico) al que pertenece esta entrada. Necesario para poder
        /// extraerla correctamente, ya que entradas de AFS anidados viven dentro de
        /// un archivo temporal distinto del AFS raíz.
        /// </summary>
        public AfsArchive OwnerArchive = null!;

        public override string ToString() => $"{VirtualPath} (offset=0x{Offset:X}, size={Size})";
    }

    /// <summary>
    /// Lector de archivos AFS (formato de contenedor usado en varios juegos de PS2,
    /// incluyendo WE8/PES). Replica el comportamiento de herramientas como DKZ Studio
    /// o Game Graphic Studio: permite listar y extraer archivos/carpetas contenidos,
    /// incluyendo AFS anidados dentro de otras entradas.
    /// </summary>
    public class AfsArchive
    {
        private const int NameRecordSize = 48; // 32 (name) + 12 (fecha) + 4 (size)

        public List<AfsEntry> Entries { get; } = new List<AfsEntry>();

        /// <summary>
        /// Ruta en disco del archivo AFS físico que contiene esta instancia (puede ser
        /// el AFS raíz, o el archivo temporal extraído de una entrada anidada).
        /// </summary>
        public string SourcePath { get; private set; } = string.Empty;

        public static bool LooksLikeAfs(byte[] data)
        {
            if (data.Length < 8)
            {
                return false;
            }

            // Firmas válidas conocidas: "AFS\0" y "AFS " (0x41 0x46 0x53 0x00 / 0x20)
            return data[0] == 0x41 && data[1] == 0x46 && data[2] == 0x53 &&
                   (data[3] == 0x00 || data[3] == 0x20);
        }

        /// <summary>
        /// Igual que <see cref="LooksLikeAfs(byte[])"/> pero lee solo los primeros
        /// bytes del archivo directamente desde disco (vía <paramref name="entry"/>),
        /// evitando cargar en memoria entradas potencialmente enormes (ej. gráficos
        /// de varios MB) solo para descartar que sean un AFS anidado.
        /// </summary>
        public static bool LooksLikeAfsHeaderOnly(AfsEntry entry)
        {
            if (entry.Size < 8)
            {
                return false;
            }

            using var fs = new FileStream(entry.OwnerArchive.SourcePath, FileMode.Open, FileAccess.Read);
            fs.Seek(entry.Offset, SeekOrigin.Begin);

            byte[] header = new byte[8];
            int totalRead = 0;
            while (totalRead < header.Length)
            {
                int read = fs.Read(header, totalRead, header.Length - totalRead);
                if (read <= 0)
                {
                    break;
                }
                totalRead += read;
            }

            return totalRead == header.Length && LooksLikeAfs(header);
        }

        /// <summary>
        /// Parsea un archivo AFS desde disco. Si <paramref name="parentVirtualPath"/>
        /// se especifica, se usa como prefijo para construir VirtualPath de cada
        /// entrada (útil al parsear AFS anidados).
        /// </summary>
        public static AfsArchive Load(string filePath, string parentVirtualPath = "")
        {
            byte[] raw = File.ReadAllBytes(filePath);
            return LoadFromBytes(raw, filePath, parentVirtualPath);
        }

        public static AfsArchive LoadFromBytes(byte[] raw, string sourcePath, string parentVirtualPath = "")
        {
            if (!LooksLikeAfs(raw))
            {
                throw new InvalidDataException("El archivo no tiene la firma AFS esperada ('AFS\\0' o 'AFS ').");
            }

            var archive = new AfsArchive { SourcePath = sourcePath };

            uint fileCount = BitConverter.ToUInt32(raw, 4);
            if (fileCount == 0 || fileCount > 100000)
            {
                throw new InvalidDataException($"Número de archivos AFS inválido: {fileCount}.");
            }

            int tableStart = 8;
            var offsets = new uint[fileCount];
            var sizes = new uint[fileCount];

            for (int i = 0; i < fileCount; i++)
            {
                offsets[i] = BitConverter.ToUInt32(raw, tableStart + (i * 8));
                sizes[i] = BitConverter.ToUInt32(raw, tableStart + (i * 8) + 4);
            }

            // Inmediatamente después de la tabla principal viene un puntero adicional
            // (offset, size) hacia la tabla de metadatos (nombres + fecha + tamaño).
            int metaPointerOffset = tableStart + ((int)fileCount * 8);
            uint nameTableOffset = BitConverter.ToUInt32(raw, metaPointerOffset);
            uint nameTableSize = BitConverter.ToUInt32(raw, metaPointerOffset + 4);

            bool hasNameTable = nameTableOffset > 0 && nameTableOffset < raw.Length;

            for (int i = 0; i < fileCount; i++)
            {
                string name = $"file_{i:D4}.bin"; // fallback si no hay tabla de nombres

                if (hasNameTable)
                {
                    long recordOffset = nameTableOffset + ((long)i * NameRecordSize);
                    if (recordOffset + NameRecordSize <= raw.Length)
                    {
                        int nameLen = 0;
                        while (nameLen < 32 && raw[recordOffset + nameLen] != 0x00)
                        {
                            nameLen++;
                        }
                        if (nameLen > 0)
                        {
                            name = Encoding.ASCII.GetString(raw, (int)recordOffset, nameLen);
                        }
                    }
                }

                string virtualPath = string.IsNullOrEmpty(parentVirtualPath)
                    ? name
                    : parentVirtualPath + "\\" + name;

                archive.Entries.Add(new AfsEntry
                {
                    Index = i,
                    Name = name,
                    Offset = offsets[i],
                    Size = sizes[i],
                    VirtualPath = virtualPath,
                    OwnerArchive = archive
                });
            }

            return archive;
        }

        /// <summary>
        /// Extrae los bytes crudos de una entrada directamente desde el archivo físico
        /// propietario (entry.OwnerArchive.SourcePath), sin necesidad de tener todo el
        /// AFS cargado en memoria.
        /// </summary>
        public static byte[] ExtractEntry(AfsEntry entry)
        {
            using var fs = new FileStream(entry.OwnerArchive.SourcePath, FileMode.Open, FileAccess.Read);
            fs.Seek(entry.Offset, SeekOrigin.Begin);
            byte[] buffer = new byte[entry.Size];
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int read = fs.Read(buffer, totalRead, buffer.Length - totalRead);
                if (read <= 0)
                {
                    break;
                }
                totalRead += read;
            }
            return buffer;
        }

        /// <summary>
        /// Busca una entrada de PRIMER NIVEL (no recursivo) cuyo nombre coincida
        /// exactamente (ignorando mayúsculas) con <paramref name="exactName"/>.
        /// Útil para localizar sub-contenedores concretos como "BOOT.afs",
        /// "FLAG.afs" o "GAME.afs" dentro del AFS raíz del disco, sin tener que
        /// expandir ni analizar el resto de entradas.
        /// </summary>
        public AfsEntry? FindFirstLevelEntryByName(string exactName)
        {
            foreach (var entry in Entries)
            {
                if (string.Equals(entry.Name, exactName, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// Recorre recursivamente las entradas, detectando si alguna es a su vez un
        /// AFS anidado y parseándolo. Los archivos anidados se extraen a disco
        /// temporal porque AfsArchive trabaja sobre SourcePath (lectura directa por
        /// offset), no sobre buffers en memoria.
        /// </summary>
        /// <param name="tempDirectory">Carpeta temporal donde volcar los AFS anidados.</param>
        /// <param name="progress">Callback opcional para reportar avance (ej. a una UI).</param>
        public void ExpandNestedArchives(string tempDirectory, IProgress<string>? progress = null)
        {
            Directory.CreateDirectory(tempDirectory);

            for (int i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];

                progress?.Report($"Analizando {entry.VirtualPath} ({i + 1}/{Entries.Count})");

                // Solo se lee el encabezado (8 bytes) para descartar rápidamente
                // entradas grandes (ej. gráficos de varios MB) que no son AFS.
                if (!LooksLikeAfsHeaderOnly(entry))
                {
                    continue;
                }

                byte[] data = ExtractEntry(entry);
                string nestedPath = Path.Combine(tempDirectory, $"nested_{entry.Index:D4}_{entry.Name}");
                File.WriteAllBytes(nestedPath, data);

                entry.NestedArchive = LoadFromBytes(data, nestedPath, entry.VirtualPath);
                entry.NestedArchive.ExpandNestedArchives(tempDirectory, progress);
            }
        }

        /// <summary>
        /// Busca recursivamente (incluyendo AFS anidados ya expandidos) una entrada
        /// cuya VirtualPath termine con el sufijo indicado, ignorando mayúsculas y
        /// usando tanto '\' como '/' como separador válido.
        /// </summary>
        public AfsEntry? FindByVirtualPathSuffix(string suffix)
        {
            string normalizedSuffix = Normalize(suffix);

            foreach (var entry in Entries)
            {
                if (Normalize(entry.VirtualPath).EndsWith(normalizedSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    return entry;
                }

                if (entry.NestedArchive != null)
                {
                    var found = entry.NestedArchive.FindByVirtualPathSuffix(suffix);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Igual que <see cref="FindByVirtualPathSuffix"/> pero devuelve TODAS las
        /// coincidencias en vez de la primera.
        /// </summary>
        public List<AfsEntry> FindAllByNameSuffix(string suffix)
        {
            var results = new List<AfsEntry>();
            CollectAllByNameSuffix(suffix, results);
            return results;
        }

        private void CollectAllByNameSuffix(string suffix, List<AfsEntry> results)
        {
            foreach (var entry in Entries)
            {
                if (entry.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(entry);
                }

                entry.NestedArchive?.CollectAllByNameSuffix(suffix, results);
            }
        }

        /// <summary>
        /// Devuelve todas las entradas (incluyendo las de AFS anidados ya expandidos)
        /// en una sola lista plana. Útil para diagnóstico/depuración.
        /// </summary>
        public List<AfsEntry> GetAllEntriesFlat()
        {
            var results = new List<AfsEntry>();
            CollectAllFlat(results);
            return results;
        }

        private void CollectAllFlat(List<AfsEntry> results)
        {
            foreach (var entry in Entries)
            {
                results.Add(entry);
                entry.NestedArchive?.CollectAllFlat(results);
            }
        }

        private static string Normalize(string path) => path.Replace('/', '\\').Trim('\\');
    }
}