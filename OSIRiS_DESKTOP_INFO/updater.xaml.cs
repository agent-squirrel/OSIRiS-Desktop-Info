using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;

namespace OSIRiS_DESKTOP_INFO
{
    /// <summary>
    /// Interaction logic for updater.xaml
    /// </summary>
    public partial class updater : Window
    {

        WebClient webClient;
        public updater()
        {
            InitializeComponent();
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = false;
            bw.WorkerReportsProgress = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync();
        }

        public void DownloadFile(string urlAddress, string location)
        {

            using (webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);

                // The variable that will be holding the url address (making sure it starts with https://)
                Uri URL = urlAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("https://" + urlAddress);


                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        // The event that will trigger when the WebClient is completed
        private void Completed(object sender, AsyncCompletedEventArgs ex)
        {

            if (ex.Cancelled == true)
            {
                MessageBox.Show("Download has been canceled.");
            }
            else
            {
                copy();
            }
        }


        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; (i <= 10); i++)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Rename the ODIN executable so we can copy a new one into it's place.
                    File.Move("ODIN.exe", "ODIN.exe.bak");
                }
            }
        }
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Download the new version of ODIN.
            DownloadFile("https://gnuplusadam.com/OSIRiS/ODIN/ODIN.exe", System.IO.Path.GetTempPath() + "ODIN.exe");
        }

        private void copy()
        {
            BackgroundWorker bw2 = new BackgroundWorker();
            bw2.WorkerSupportsCancellation = false;
            bw2.WorkerReportsProgress = true;
            bw2.DoWork += new DoWorkEventHandler(bw2_DoWork);
            bw2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw2_RunWorkerCompleted);
            bw2.RunWorkerAsync();
        }

        private void bw2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; (i <= 10); i++)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    // Move the new ODIN executable into the C:\profiles\ directory.
                    File.Move(System.IO.Path.GetTempPath() + "ODIN.exe", @"C:\profiles\ODIN.exe");
                }
            }
        }

        private void bw2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(2000);
            Process.Start("ODIN.exe");
            Application.Current.Shutdown();
        }

    }
}
