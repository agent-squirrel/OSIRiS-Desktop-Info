﻿using System;
using Microsoft.Win32;
using System.Management;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace OSIRiS_DESKTOP_INFO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Suppress ALT+F4 closure of the Window.
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            // Subscribe to the powermodechanged Win32 event.
            SystemEvents.PowerModeChanged += this.SystemEvents_PowerModeChanged;
            ShowActivated = false;
            //Parse command line arguments. If 'clear' is found, the 'Clearance' checkbox has been selected in OSIRiS.
            string[] args = Environment.GetCommandLineArgs();
            string stringToCheck = "clear";
            foreach (string x in args)
            {
                if (x.Contains(stringToCheck))
                {
                    clearancebanner.Visibility = Visibility.Visible;
                }
                else
                {
                    clearancebanner.Visibility = Visibility.Collapsed;
                }
            }

            //Check for an old copy of ODIN.

            if (File.Exists(@"C:\profiles\ODIN.exe.bak"))
            {
                File.Delete(@"C:\profiles\ODIN.exe.bak");
            }
            if (File.Exists(@"ODIN.exe.bak"))
            {
                File.Delete(@"ODIN.exe.bak");
            }

            //Initilize the labels with WMI queries.
            CPUlabel.Content += getcpu();
            RAMlabel.Content += getram();
            GPUlabel.Content += getgpu() + " with" + " " + getgpuadapterRAM() + " of VRAM";
            GPUlabel2.Content += getsecondarygpu() + " with" + " " + getsecondarygpuadapterRAM() + " of VRAM";
            //Peform some actions if we get a blank string back from the secondary GPU query.
            if ((string)getsecondarygpu() == "" || (string)getsecondarygpu() == "Microsoft Basic Render Driver" )
            { 
                GPU2.Visibility = Visibility.Collapsed;
                GPUlabel2.Visibility = Visibility.Collapsed;
                DRIVE.Margin = new Thickness(140, 115, 0, 0);
                DRIVElabel.Margin = new Thickness(238, 115, 0, 0);
                OS.Margin = new Thickness(140, 153, 0, 0);
                OSlabel.Margin = new Thickness(306, 153, 0, 0);
                RES.Margin = new Thickness(140, 190, 0, 0);
                RESlabel.Margin = new Thickness(306, 190, 0, 0);
                GPUlabel.Margin = new Thickness(270, 78, 0, 0);
                GPU1.Content += ":";
            }
            else
            {
                GPU1.Content += " 1:";
            }
            DRIVElabel.Content += getdrive();
            OSlabel.Content += getOS();
            RESlabel.Content += getres();
        }

        //Setup strings.

        public string getcpu()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Name").ToString();
                }
                catch { }
            }
            return "Unknown";
        }

        public string getram()
        {
            ManagementScope ms = new ManagementScope();
            ObjectQuery oq = new ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            ManagementObjectCollection moc = mos.Get();
            int amount = 0;
            foreach (ManagementObject mo in moc)
            {
                amount += Convert.ToInt32(Convert.ToInt64(mo["Capacity"]) / 1024 / 1024 / 1024);
            }
            return amount + " GB";
        }

        public string getgpu()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController WHERE DeviceID='VideoController1'");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Description").ToString();
                }
                catch { }
            }
            return "Unknown";
        }

        public string getgpuadapterRAM()
        {
            ManagementScope ms = new ManagementScope();
            ObjectQuery oq = new ObjectQuery("SELECT AdapterRAM FROM Win32_VideoController WHERE DeviceID='VideoController1'");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            ManagementObjectCollection moc = mos.Get();
            int amount = 0;
            foreach (ManagementObject mo in moc)
            {
                amount += Convert.ToInt32(Convert.ToInt64(mo["AdapterRAM"]) / 1024 / 1024);
            }
            return amount + " MB";
        }

        public string getsecondarygpu()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController WHERE DeviceID='VideoController2'");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Description").ToString();
                }
                catch { }
            }
            return "";
        }

        public string getsecondarygpuadapterRAM()
        {
            ManagementScope ms = new ManagementScope();
            ObjectQuery oq = new ObjectQuery("SELECT AdapterRAM FROM Win32_VideoController WHERE DeviceID='VideoController2'");
            ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
            ManagementObjectCollection moc = mos.Get();
            int amount = 0;
            foreach (ManagementObject mo in moc)
            {
                amount += Convert.ToInt32(Convert.ToInt64(mo["AdapterRAM"]) / 1024 / 1024);
            }
            return amount + " MB";
        }

        public string getdrive()
        {
            double disk = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive where Index = 0");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    //The multiplication at the end may seem to be arbitrary but is in fact a rounding required to get the SI standard drive size.
                    //Windows will report the drive size as a binary figure whereas the manufacturers report it as a decimal one.
                    disk = Math.Round(((double)Convert.ToDouble(wmi["Size"]) / 1024 / 1024 / 1024 * 1.073741824));
                    if (disk >= 1000)
                    {
                        double disk2 = disk / 1000;
                        return disk2.ToString() + " TB";
                    }
                    else if (disk >= 10000)
                    {
                        double disk2 = disk / 10000;
                        return disk2.ToString() + " PB";
                    }
                    else
                    {
                        return disk.ToString() + " GB";
                    }
                }
                catch
                {
                }
            }
            return "Unable to Find Drive";
        }


        public string getOS()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Caption").ToString();
                }
                catch { }
            }
            return "Unknown";
        }

        public string getres()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("CurrentHorizontalResolution").ToString() + " x " + wmi.GetPropertyValue("CurrentVerticalResolution").ToString();
                }
                catch { }
            }
            return "Unknown";
        }

        //Wallpaper set class.
        public sealed class Wallpaper
        {
            Wallpaper() { }

            const int SPI_SETDESKWALLPAPER = 20;
            const int SPIF_UPDATEINIFILE = 0x01;
            const int SPIF_SENDWININICHANGE = 0x02;

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

            public enum Style : int
            {
                Tiled,
                Centered,
                Stretched
            }

            public static void Set(string wpaper, Style style)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
                if (style == Style.Stretched)
                {
                    key.SetValue(@"WallpaperStyle", 2.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Centered)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 0.ToString());
                }

                if (style == Style.Tiled)
                {
                    key.SetValue(@"WallpaperStyle", 1.ToString());
                    key.SetValue(@"TileWallpaper", 1.ToString());
                }

                string tempPath = @"C:\profiles\" + wpaper;
                SystemParametersInfo(SPI_SETDESKWALLPAPER,
                    0,
                    tempPath,
                    SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }

        }

        private void ODIN_Loaded(object sender, RoutedEventArgs e)
        {
            //Set the Desktop wallpaper.
            Wallpaper.Set("wallpaper.bmp", Wallpaper.Style.Stretched);
            //Update Check
            string url = "https://gnuplusadam.com/OSIRiS/ODIN/version";
            string versionstring;
            using (var wc = new WebClient())
                try
                {
                    versionstring = wc.DownloadString(url);
                    Version latestVersion = new Version(versionstring);

                    //Get current binary version.
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    Version currentVersion = new Version(fvi.FileVersion);

                    //Compare.
                    if (latestVersion > currentVersion)
                    {
                        updater updatewindow = new updater();
                        updatewindow.Show();
                        this.Hide();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (WebException)
                {
                    return;
                }
        }

        // Recenter and un-minimize the main window on resume from suspend. 
        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode == PowerModes.Resume)
            {
                // Un-minimize.
                if (this.WindowState == WindowState.Minimized)
                    this.WindowState = WindowState.Normal;

                // Re-Center
                double screeHeight = SystemParameters.FullPrimaryScreenHeight;
                double screeWidth = SystemParameters.FullPrimaryScreenWidth;
                this.Top = (screeHeight - this.Height) / 2;
                this.Left = (screeWidth - this.Width) / 2;
            }
        }

        // Handle key combos.
        public void ODIN_KeyUp(object sender, KeyEventArgs e)
        {

            // Reconfiguration Utility.
            // Ctrl + Shift + R 
            if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)) && (e.Key == Key.R))
            {
                try {
                    Process reconfigure_ODIN = new Process();
                    reconfigure_ODIN.StartInfo.FileName = @"C:\profiles\Reconfigure_ODIN.exe";
                    reconfigure_ODIN.Start();
                    reconfigure_ODIN.WaitForExit();
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("The ODIN reconfiguration utility is missing." + Environment.NewLine + "Either ODIN is running without OSIRiS or the utility has been deleted." + Environment.NewLine + "Please contact support via forwarder@gnuplusadam.com and include this error message:" + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

            // Force update.
            // Ctrl + Shift + U 
            if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)) && (e.Key == Key.U))
            {
                string url = "https://gnuplusadam.com/OSIRiS/ODIN/version";
                string versionstring;
                using (var wc = new System.Net.WebClient())
                    try
                    {
                        versionstring = wc.DownloadString(url);
                        Version latestVersion = new Version(versionstring);
                        MessageBoxResult updateconfirm = MessageBox.Show(String.Format("Would you like to update to version {0}?", latestVersion), "Forced Update?", MessageBoxButton.YesNo, MessageBoxImage.Information);
                        if (updateconfirm == MessageBoxResult.Yes)
                        {
                            var form = new updater();
                            this.Hide();
                            form.Show();
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (WebException)
                    {
                        return;
                    }
            }

            // Force quit.
            // Ctrl + Shift + Q
            if ((Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift)) && (e.Key == Key.Q))
            {
                Application.Current.Shutdown();
                return;
            }
        }
    }
}






        
    

