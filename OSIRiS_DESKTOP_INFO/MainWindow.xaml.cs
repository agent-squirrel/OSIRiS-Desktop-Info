using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSIRiS_DESKTOP_INFO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Initilize the labels with WMI queries.
            CPUlabel.Content += getcpu();
            RAMlabel.Content += getram();
            GPUlabel.Content += getgpu();
            DRIVElabel.Content += getdrive();
            OSlabel.Content += getOS();
            RESlabel.Content += getres();
        }

        public string getcpu()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return  wmi.GetPropertyValue("Name").ToString();
                }
                catch { }
            }
            return "CPU: Unknown";
        }

        public string getram()
        {
            ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject item in moc)
            {
                try
                {
                  return Convert.ToString(Math.Round(Convert.ToDouble(item.Properties["TotalPhysicalMemory"].Value) / 1073741824, 2)) + " GB";
                }
                catch { }
            }
            return "RAM: Unknown";
        }

        public string getgpu()
        {
            ManagementObjectSearcher mos =
            new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject wmi in mos.Get())
            {
                try
                {
                    return wmi.GetPropertyValue("Name").ToString();
                }
                catch { }
            }
            return "GPU: Unknown";
        }

        public string getdrive()
        {
            double disk = 0;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_LogicalDisk where DriveType=3");
            foreach (ManagementObject wmi in searcher.Get())
            {
                try
                {
                    disk = Math.Round(((((double)Convert.ToDouble(wmi["Size"]) / 1024) / 1024) / 1024), 2);
                    return disk.ToString() + " GB";
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
            return "OS: Unknown";
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
            return "RES: Unknown";
        }

    }
}
        
    

