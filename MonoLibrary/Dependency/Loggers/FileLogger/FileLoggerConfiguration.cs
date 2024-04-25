using System;
using System.IO;

namespace MonoLibrary.Dependency.Loggers.FileLogger
{
    public class FileLoggerConfiguration
    {
        /// <summary>
        /// Directory path where to save logs.
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Name of the file, including extension.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// When <see langword="true"/>, create '<see cref="Name"/> (1)' file if file with <see cref="Name"/> already exists.
        /// <see langword="true"/> by default.
        /// </summary>
        public bool Multiple { get; set; } = true;

        public string GetFullPath()
        {
            string dir = string.IsNullOrEmpty(Directory) ? System.IO.Directory.GetCurrentDirectory() : Directory;
            dir = Path.Combine(dir, "Logs");
            string filename = string.IsNullOrEmpty(Name) ? "LogFile.log" : Name;
            string name = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            var file = new FileInfo(Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd} - {name}{extension}"));

            int i = 1;
            while (Multiple && file.Exists)
                file = new FileInfo(Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd} - {name} ({i++}){extension}"));

            return file.FullName;
        }
    }
}
