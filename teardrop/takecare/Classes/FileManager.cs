using System.IO;

namespace takecare
{
    public class FileManager
    {
        public static string RemoveExtension(string filePath, string extension)
        {
            return filePath.Replace(extension, string.Empty);
        }

        public static string AddExtension(string filePath, string extension)
        {
            return string.Concat(filePath, extension);
        }

        public static bool HasExtension(string filePath, string extension)
        {
            return filePath.Contains(extension) && !filePath.Contains("System Volume Information");
        }

        public static void WriteHtmlFile(string path, string message)
        {
            File.WriteAllText($@"{path}\message.html", message);
        }
    }
}
