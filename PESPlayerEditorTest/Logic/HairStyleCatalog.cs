using System;
using System.Collections.Generic;
using System.IO;

namespace JWE8Editor.Logic
{
    /// <summary>
    /// Carga el catálogo de peinados (HairStyle) desde un archivo de texto
    /// con formato "indice=NOMBRE", evitando declarar un enum de miles de miembros.
    /// </summary>
    public static class HairStyleCatalog
    {
        private static readonly Dictionary<int, string> _names = new Dictionary<int, string>();
        private static bool _isLoaded;

        public static IReadOnlyDictionary<int, string> Names
        {
            get
            {
                EnsureLoaded();
                return _names;
            }
        }

        public static string GetName(int hairId)
        {
            EnsureLoaded();
            return _names.TryGetValue(hairId, out string? name) ? name : $"UNKNOWN_{hairId}";
        }

        private static void EnsureLoaded()
        {
            if (_isLoaded)
            {
                return;
            }

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "HairStyles.txt");

            if (!File.Exists(filePath))
            {
                _isLoaded = true;
                return;
            }

            foreach (string line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string indexPart = line.Substring(0, separatorIndex).Trim();
                string namePart = line.Substring(separatorIndex + 1).Trim();

                if (int.TryParse(indexPart, out int hairId))
                {
                    _names[hairId] = namePart;
                }
            }

            _isLoaded = true;
        }
    }
}