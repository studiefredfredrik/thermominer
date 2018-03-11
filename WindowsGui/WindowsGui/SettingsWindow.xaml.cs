using System.IO.Ports;
using System.Windows;

namespace WindowsGui
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            ListSerialPorts();
        }

        private void LoadSettings()
        {
            txtUrl.Text = (string)Properties.Settings.Default["txtUrlForStatusPost"];
            txtWallet.Text = (string)Properties.Settings.Default["txtAddress"];
            txtRigName.Text = (string)Properties.Settings.Default["txtMinerName"];
            sldrTemperature.Value = int.Parse((string)Properties.Settings.Default["temperatureLimit"]);
            lblTemperatureLimit.Content = sldrTemperature.Value + "°C";
            chkPostToServer.IsChecked = ApplicationState.PostStatusToServer;
            chkTemperatureLimit.IsChecked = ApplicationState.TemperatureLimitEnabled;
        }

        private void ListSerialPorts()
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                cboSensors.Items.Add(s);
            }
        }

        private void chkTemperatureLimit_Checked(object sender, RoutedEventArgs e)
        {

        }

        //private void lblPathOfMiner_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        //{
        //    Process.Start($"{pathOfWorkingDir}");
        //}
    }
}
