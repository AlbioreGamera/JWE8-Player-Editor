using PESPlayerEditorTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JWE8Editor.Logic
{
    public class LoadCommentaries
    {
        public static List<Commentary> Commentaries { get; } = new List<Commentary>();

        public LoadCommentaries()
        {
            PopulateCommentary();
        }

        public void PopulateCommentary()
        {
            try
            {
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "commentary.cfg");
                List<string> callNames = new List<string>();

                foreach (string line in File.ReadLines(filePath))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    {
                        Commentary commentary = new Commentary();
                        string[] parts = line.Split(';');
                        if (parts.Length >= 3)
                        {
                            commentary.CommentaryIndex = int.Parse(parts[0]);
                            commentary.CommentaryCode = parts[1];
                            commentary.CommentaryName = "("+parts[1]+") "+parts[2];
                        }
                        Commentaries.Add(commentary);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading callnames: {ex.Message}");
            }
        }
    }
}
