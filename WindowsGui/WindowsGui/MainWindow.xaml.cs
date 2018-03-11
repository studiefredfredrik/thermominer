using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Management;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace WindowsGui
{
    public partial class MainWindow : Window
    {
        Timer temperatureTimer;
        Timer serverTimer;

        private string pathOfExe = "";
        private string pathOfWorkingDir = "";
        private string pathOfBatFile = "";
        private Process process;
        SerialPort serialPort;
        decimal? currentTemperature = null;
        decimal? currentHashrate = null;
        bool currentlyMining = false;

        public MainWindow()
        {
            InitializeComponent();
            TryCopyEthminerToOutputDir();
            StartTimers();
        }

        private void StartTimers()
        {
            temperatureTimer = new Timer(
                CheckTemperature,
                new AutoResetEvent(false),
                1000 * 60 * 1, 1000 * 60 * 1
            );
            serverTimer = new Timer(
                PostToServer,
                new AutoResetEvent(false),
                1000 * 60 * 5, 1000 * 60 * 5
            );
        }

        private void TryCopyEthminerToOutputDir()
        {
            pathOfWorkingDir = AppDomain.CurrentDomain.BaseDirectory;
            pathOfExe = $"{AppDomain.CurrentDomain.BaseDirectory}/ethminer.exe";
            pathOfBatFile = $"{AppDomain.CurrentDomain.BaseDirectory}/runner.bat";

            if (!File.Exists(pathOfExe))
                File.WriteAllBytes(pathOfExe, Properties.Resources.ethminer);

            if (!File.Exists(pathOfBatFile))
                File.WriteAllText(pathOfBatFile, Properties.Resources.runner);
        }

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid)
        {
            // Cannot close 'system idle process'.
            if (pid == 0)
            {
                return;
            }
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StopMining();
            WriteToConsole(this, "Process failed! Killing");

            WriteToConsole(this, "Restarting process, giving it 15 seconds to cool off first...");
            Thread.Sleep(15000);

            StartMining();
            WriteToConsole(this, "Process restarted, fingers crossed");
        }

        private void runCommand()
        {
            // Create Process
            process = new Process();
            process.StartInfo.WorkingDirectory = pathOfWorkingDir;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c " + "runner.bat";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit(); // <--- can be removed
        }

        private static void WriteToConsole(MainWindow runnerClass, string text)
        {
            runnerClass.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => {
                    runnerClass.txtMinerOutput.Text += text + "\n";
                    runnerClass.minerOutputScrollview.ScrollToEnd();
                }));
        }

        private void StartMining()
        {
            if (currentlyMining) return;
            currentlyMining = true;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                runCommand();
            }).Start();

            Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => {
                    lblStatus.Content = "MINING";
            }));
        }

        private void StopMining()
        {
            if (!currentlyMining) return;
            currentlyMining = false;

            KillProcessAndChildren(process.Id);

            Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => {
                    lblStatus.Content = "NOT MINING";
            }));
        }

        public void CheckTemperature(Object stateInfo)
        {
            WriteToConsole(this, "Temperature checker running...");
            if (!ApplicationState.TemperatureLimitEnabled) // Not tracking temp
                return;
            if (currentTemperature == null)
                return;

            if (currentTemperature.Value > ApplicationState.TemperatureLimit)
            {
                StopMining();
                WriteToConsole(this, "High temperature, stopping...");
            }
            else
            {
                StartMining();
                WriteToConsole(this, "Low temperature, starting...");
            }
        }

        private List<decimal> extractHashratesFromConsole(string consoleOutput)
        {
            List<int> foundIndexes = new List<int>();
            int x = 0;
            while (true)
            {
                x = consoleOutput.IndexOf("gpu/", x);
                if (x == -1) break;
                foundIndexes.Add(x);
                x++;
            }

            List<decimal> hashrates = new List<decimal>();
            foreach (var idx in foundIndexes)
            {
                string numberTxt = consoleOutput.Substring(idx + 5, 6);
                numberTxt = numberTxt.Replace(" ", "");
                if (decimal.TryParse(numberTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out var rate))
                {
                    hashrates.Add(rate);
                }
            }
            return hashrates;
        }

        int divider = 20;
        int dividerCounter = 0;
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrEmpty(outLine.Data)) return;

            WriteToConsole(this, outLine.Data);

            if (dividerCounter < divider)
            {
                dividerCounter++;
                return;
            }
            dividerCounter = 0;

            if (!outLine.Data.Contains("Speed ")) return;

            var hashrates = extractHashratesFromConsole(outLine.Data);

            currentHashrate = hashrates.Sum();

            hashrates.Insert(0, hashrates.Sum());

            Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => {
                    ClearTextBoxClutter(txtHashrates);
                    txtHashrates.Text += (string.Join(" - ", hashrates.ToArray()) + "\n");
                    hashrateScrollview.ScrollToEnd();
                }));
        }

        private static void ClearTextBoxClutter(TextBlock textBox)
        {
            if (textBox.Text.Length > 10000)
            {
                textBox.Text.Remove(0, 5000);
            }
        }

        private static readonly HttpClient client = new HttpClient();
        public static void PostToServer(Object stateInfo)
        {
            if (!ApplicationState.PostStatusToServer) return;

            var stats = new MinerStatus
            {
                address = ApplicationState.Wallet,
                name = ApplicationState.RigName,
                hashrate = ApplicationState.CurrentHashrate,
                temperature = ApplicationState.CurrentTemperature
            };

            PostJson(ApplicationState.Url, stats);
        }

        private static void PostJson(string uri, MinerStatus postParameters)
        {
            string postData = JsonConvert.SerializeObject(postParameters);
            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = bytes.Length;
            httpWebRequest.ContentType = "application/json";
            using (Stream requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Count());
            }
            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
        }

        public class MinerStatus
        {
            public string address;
            public string name;
            public decimal? temperature;
            public decimal? hashrate;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartMining();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            ApplicationState.TemperatureLimitEnabled = false; // Turn off thermostat
            StopMining();
        }

        private void Settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string serialTxt = serialPort.ReadLine();
            serialTxt.Replace("\r", "");
            if (decimal.TryParse(serialTxt, NumberStyles.Any, CultureInfo.InvariantCulture, out var temp))
            {
                Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() => {
                        lblCurrentTemperature.Content = temp + "°C";
                }));
                currentTemperature = temp;
            }
            else
            {
                Dispatcher.Invoke(DispatcherPriority.Normal,
                    new Action(() => {
                        lblCurrentTemperature.Content = "--";
                    }));
                currentTemperature = null;
            }
        }
    }
}
