using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Windows;

namespace WindowsGui
{
    public partial class SettingsWindow : Window
    {
        public event EventHandler ComPortChangedEvent;
        public event EventHandler TemperatureLimitChangedEvent;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
            ListSerialPorts();
        }

        private void LoadSettings()
        {
            txtUrl.Text = (string)Properties.Settings.Default["txtUrl"];
            txtWallet.Text = (string)Properties.Settings.Default["txtWallet"];
            txtRigName.Text = (string)Properties.Settings.Default["txtRigName"];
            if(int.TryParse((string)Properties.Settings.Default["temperatureLimit"], out int temp))
                sldrTemperature.Value = temp;
            lblTemperatureLim.Content = sldrTemperature.Value + "°C";
            chkPostToServer.IsChecked = ApplicationState.PostStatusToServer;
            chkTemperatureLimit.IsChecked = ApplicationState.TemperatureLimitEnabled;
            lblEthminerLocation.Text = ApplicationState.EthminerDir;
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
            ApplicationState.TemperatureLimitEnabled = chkTemperatureLimit.IsChecked ?? false;
            TemperatureLimitChangedEvent(this, EventArgs.Empty);
        }

        private void lblEthminerLocation_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Process.Start($"{ApplicationState.EthminerDir}");
        }

        private void sldrTemperature_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblTemperatureLim == null) return;
            ApplicationState.TemperatureLimit = (decimal)sldrTemperature.Value;
            lblTemperatureLim.Content = sldrTemperature.Value + "°C";
            Properties.Settings.Default["temperatureLimit"] = sldrTemperature.Value.ToString();
            Properties.Settings.Default.Save();
            if(TemperatureLimitChangedEvent != null)
                TemperatureLimitChangedEvent(this, EventArgs.Empty);
        }

        private void chkPostToServer_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationState.PostStatusToServer = chkPostToServer.IsChecked ?? false;
        }

        private void txtUrl_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Uri uriResult;
            bool validUrl = Uri.TryCreate(txtUrl.Text, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
            if (!validUrl) return;

            Properties.Settings.Default["txtUrl"] = txtUrl.Text;
            Properties.Settings.Default.Save();
            ApplicationState.Url = txtUrl.Text;
        }

        private void txtWallet_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Properties.Settings.Default["txtWallet"] = txtWallet.Text;
            Properties.Settings.Default.Save();
            ApplicationState.Wallet = txtWallet.Text;
        }

        private void txtRigName_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            Properties.Settings.Default["txtRigName"] = txtRigName.Text;
            Properties.Settings.Default.Save();
            ApplicationState.RigName = txtRigName.Text;
        }

        private void cboSensors_DropDownClosed(object sender, EventArgs e)
        {
            ApplicationState.ComPort = cboSensors.Text;
            ComPortChangedEvent(this, EventArgs.Empty);
        }
    }
}
