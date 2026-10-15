using PESPlayerEditorTest;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JWE8Editor.Logic
{
    public class LoadCommentaries
    {
        private static bool _isPopulated;

        public static List<Commentary> Commentaries { get; } = new List<Commentary>();

        public LoadCommentaries()
        {
            if (!_isPopulated)
            {
                PopulateCommentary();
                _isPopulated = true;
            }
        }

        public static void PopulateCommentary()
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "commentary.cfg");

                var loaded = new List<Commentary>();

                foreach (string line in File.ReadLines(filePath))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] parts = line.Split(';');
                    if (parts.Length < 3)
                    {
                        continue;
                    }

                    if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
                    {
                        continue;
                    }

                    loaded.Add(new Commentary
                    {
                        CommentaryIndex = index,
                        CommentaryCode = parts[1],
                        CommentaryName = $"({parts[1]}) {parts[2]}"
                    });
                }

                Commentaries.Capacity = loaded.Count;
                Commentaries.AddRange(loaded);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading callnames: {ex.Message}");
            }
        }
    }
}
