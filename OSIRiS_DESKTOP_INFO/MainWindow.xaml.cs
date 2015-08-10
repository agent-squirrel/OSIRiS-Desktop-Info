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

            //Parse command line arguments.
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
            return"No Secondary GPU";
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
    }
}
        
    

