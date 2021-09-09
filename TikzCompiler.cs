using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace TikzGraphGen
{
    public class TikzCompiler
    {
        public static readonly string FILE_SAVE_EXTENSION = ".tgg";
        public static readonly string FILE_PLACEHOLDER = "";
        public bool Compiling { get; private set; }

        private string _fileLocation;
        private string _fileName;

        public TikzCompiler()
        {
            _fileLocation = FILE_PLACEHOLDER;
            _fileName = FILE_PLACEHOLDER;
        }
        public TikzCompiler(string location, string name)
        {
            _fileLocation = location;
            _fileName = name;
        }

        public bool HasFileName()
        {
            return !_fileName.Equals(FILE_PLACEHOLDER) && !_fileLocation.Equals(FILE_PLACEHOLDER) && File.Exists(GetFilePath());
        }

        public void Save(Graph data, bool changeName) //TODO: clear update flag when calling Save in original program
        {
            if (!Compiling && data.UpdateFlag)
            {
                bool cont = true;
                if (!HasFileName() || changeName)
                    cont = PromptFileName();

                if (cont)
                {
                    ConvertToTikzAsync(data);
                }
            }
        }

        private bool PromptFileName()
        {
            SaveFileDialog save = new()
            {
                Filter = FILE_SAVE_EXTENSION,
                FileName = "Untitled",
                DefaultExt = FILE_SAVE_EXTENSION,
                Title = "Save File As:",
                InitialDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                AddExtension = true,
                CheckPathExists = true,
                OverwritePrompt = true,
                RestoreDirectory = true,
                ValidateNames = true
            };
            if (save.ShowDialog().Equals(DialogResult.OK))
            {
                _fileLocation = Path.GetFullPath(save.FileName);
                _fileName = Path.GetFileNameWithoutExtension(save.FileName);
                return true;
            }

            return false;
        }

        private async void ConvertToTikzAsync(Graph data)
        {
            Compiling = true;
            StreamWriter ostream = new(File.OpenWrite(GetFilePath()));
            foreach(string s in GetNextLine(data))
            {
                await ostream.WriteLineAsync(s);
            }
            await ostream.FlushAsync();
            ostream.Close();
            Compiling = false;
        }

        private static IEnumerable<string> GetNextLine(Graph data)
        {
            Color _ = data.BGColor;
            //TODO: FINISH THIS
            yield return "\n";
            yield return "\n";
        }

        public void ConvertToPDF(Graph data)
        {
            if (!Compiling)
            {
                Save(data, false);

                string path = GetFilePath();
                path = path[0..^4];
                ProcessStartInfo info = new()
                {
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    FileName = "cmd.exe",
                    Arguments = $"latex {path}.tex && dvips -P pdf {path}.dvi && ps2pdf {path}.ps"
                };
                Process.Start(info);
            }
        }

        public string GetFilePath()
        {
            return _fileLocation + Path.DirectorySeparatorChar + _fileName + FILE_SAVE_EXTENSION;
        }

        public Graph ConvertFromFile(string filename)
        {
            _fileLocation = Path.GetFullPath(filename);
            _fileName = Path.GetFileNameWithoutExtension(filename);
            //TODO: Finish this
            return new Graph(new GraphInfo());
        }
    }
}
