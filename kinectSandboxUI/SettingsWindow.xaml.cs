using KinectSandboxUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace kinectSandboxUI
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private List<ScreenConfig> listScreen = new List<ScreenConfig>();
        public SettingsWindow()
        {
            InitializeComponent();
            this.lblGradientPath.Text = kinectSandboxUI.Properties.Settings.Default.GradientPath;
            int cpt = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                listScreen.Add(new ScreenConfig(cpt, screen.DeviceName));
            }
        }

        private void btSelectFolderGradient_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.lblGradientPath.Text = dialog.SelectedPath;
            }
            

        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            kinectSandboxUI.Properties.Settings.Default.GradientPath = (string)this.lblGradientPath.Text;
            if (cbScreen.SelectedItem != null)
            {
                kinectSandboxUI.Properties.Settings.Default.OutputScreen = ((ScreenConfig)this.cbScreen.SelectedItem).Label;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Configure combobox rotation
            this.cbScreen.ItemsSource = this.listScreen;
            this.cbScreen.DisplayMemberPath = "Label";
            this.cbScreen.SelectedValuePath = "Id";
            foreach (ScreenConfig scConfig in this.cbScreen.ItemsSource)
            {
                if (scConfig.Label == kinectSandboxUI.Properties.Settings.Default.OutputScreen)
                {
                    this.cbScreen.SelectedItem = scConfig;
                    break;
                }
            }
           
        }
    }
}
