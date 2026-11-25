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
        public string VirtualPath = string.Empty;
        public AfsArchive? NestedArchive;
        public AfsArchive OwnerArchive = null!;

        public override string ToString() => $"{VirtualPath} (offset=0x{Offset:X}, size={Size})";
    }

    /// <summary>
    /// Lector de archivos AFS (formato de contenedor usado en varios juegos de PS2,
    /// incluyendo WE8/PES): permite listar y extraer archivos/carpetas contenidos,
    /// incluyendo AFS anidados dentro de otras entradas.
    /// </summary>
    public class AfsArchive
    {
        private const int NameRecordSize = 48; // 32 (name) + 12 (fecha) + 4 (size)

        public List<AfsEntry> Entries { get; } = new List<AfsEntry>();

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
        /// bytes del archivo directamente desde disco, evitando cargar en memoria
        /// entradas potencialmente enormes solo para descartar que sean un AFS anidado.
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
        /// Busca una entrada de primer nivel (no recursivo) cuyo nombre coincida
        /// exactamente (ignorando mayúsculas) con <paramref name="exactName"/>.
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
        /// Busca recursivamente (incluyendo AFS anidados ya expandidos) una entrada
        /// cuya VirtualPath termine con el sufijo indicado.
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
        /// Igual que <see cref="FindByVirtualPathSuffix"/> pero devuelve todas las
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

        private static string Normalize(string path) => path.Replace('/', '\\').Trim('\\');
    }
}