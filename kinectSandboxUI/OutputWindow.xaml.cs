using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace kinectSandboxUI
{
    /// <summary>
    /// Interaction logic for OutputXaml.xaml
    /// </summary>
    public partial class OutputWindow : Window
    {
        private WriteableBitmap colorBitmap;

        private static Action EmptyDelegate = delegate() { };

        private Screen screen;
        public OutputWindow(WriteableBitmap colorBitmap)
        {
            this.colorBitmap = colorBitmap;
            this.screen = this.GetScreen();
            this.Top = this.screen.Bounds.Y;
            this.Left = this.screen.Bounds.X;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.imageOutput.Source = colorBitmap;
            this.WindowState = WindowState.Maximized;
            this.updateLayout();
            Selector.SetIsSelected(this.contentControl, true);
        }

        private Screen GetScreen()
        {
            if (kinectSandboxUI.Properties.Settings.Default.OutputScreen!="") {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.DeviceName == kinectSandboxUI.Properties.Settings.Default.OutputScreen)
                    {
                        return screen;
                    }
                }
            }
            
            return Screen.PrimaryScreen;
        }

        public void updateLayout()
        {
            int rotation = 0;
            switch (kinectSandboxUI.Properties.Settings.Default.Rotation) {
                case 1: rotation = 90;
                    break;
                case 2: rotation = 180;
                    break;
                case 3: rotation = 270;
                    break;
            }
            this.imageOutput.RenderTransformOrigin = new Point(0.5,0.5);
            TransformGroup group = new TransformGroup();


            RotateTransform rotateTransform = new RotateTransform(rotation);
            rotateTransform.Angle = rotation;

            if (kinectSandboxUI.Properties.Settings.Default.FlipHorizontal) {
                ScaleTransform scaleTransform = new ScaleTransform(-1,1);
                group.Children.Add(scaleTransform);
            }

            group.Children.Add(rotateTransform);
            this.imageOutput.RenderTransform = group;

            //this.imageOutput.Margin = new Thickness(kinectSandboxUI.Properties.Settings.Default.MarginLeft, kinectSandboxUI.Properties.Settings.Default.MarginTop, kinectSandboxUI.Properties.Settings.Default.MarginRight, kinectSandboxUI.Properties.Settings.Default.MarginBottom);
            //this.imageOutput.

            this.imageOutput.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        public void updateBitmap(WriteableBitmap colorBitmap)
        {
            this.colorBitmap = colorBitmap;
            this.imageOutput.Source = colorBitmap;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                this.Close();
            }
           
        }

       
        
    }
}
