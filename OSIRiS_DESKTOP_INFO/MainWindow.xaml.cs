using System;
using System.Management; 
using System.Windows;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Input;

namespace OSIRiS_DESKTOP_INFO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //BEGIN ENVIRON LOGIC//

        //Import DLLs for attaching ODIN to the Desktop.
        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //Attach ODIN to the Desktop process.
        public static void SetOnDesktop(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            IntPtr hWndProgMan = FindWindow("Progman", "Program Manager");
            SetParent(hWnd, hWndProgMan);
        }

        //Prevent focus stealing.
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //Suppress ALT+F4 closure of form.
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                e.Handled = true;
            }
        }



        //END ENVIRON LOGIC//

        public MainWindow()
        {
            InitializeComponent();
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

            //Initilize the labels with WMI queries.
            CPUlabel.Content += getcpu();
            RAMlabel.Content += getram();
            GPUlabel.Content += getgpu();
            GPUlabel2.Content += getsecondarygpu();
            //Peform some actions if we get a blank string back from the secondary GPU query.
            if ((string)GPUlabel2.Content == "" || (string)GPUlabel2.Content == "Microsoft Basic Render Driver" )
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
        }
   }
}






        
    

