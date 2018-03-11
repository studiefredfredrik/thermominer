using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WindowsGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer temperatureTimer = new Timer(
                CheckTemperature, 
                new AutoResetEvent(false), 
                1000, 1000 * 60 * 1
            );
        Timer serverTimer = new Timer(
                PostToServer, 
                new AutoResetEvent(false), 
                1000, 1000 * 60 * 5
            );

        public MainWindow()
        {
            InitializeComponent();
            
        }

        public static void CheckTemperature(Object stateInfo)
        {

        }

        public static void PostToServer(Object stateInfo)
        {

        }

        private static void WriteToConsole(MainWindow runnerClass, string text)
        {
            runnerClass.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => {
                    runnerClass.txtMinerOutput.Text = text;
                }));
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Settings_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SettingsWindow settings = new SettingsWindow();
            settings.Show();
        }
    }
}
