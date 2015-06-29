using KinectSandboxLib.Filters;
using KinectSandboxLib.Tools;
using KinectSandboxLib.Workers;
using kinectSandboxUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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


namespace KinectSandboxUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Rotation list of output window: 0, 90, 180, 270
        /// </summary>
        private List<kinectSandboxUI.Rotation> rotationList = new List<kinectSandboxUI.Rotation>();
        /// <summary>
        /// Kinect Worker
        /// </summary>
        private KinectWorker kinectWorker;
        private Thread kinectThread;
        /// <summary>
        /// Topographic Worker
        /// </summary>
        private TopographicWorker topographicWorker;
        private Thread topographicThread;
        /// <summary>
        /// Stabilizing Worker
        /// </summary>
        private StabilizingWorker stabilizingWorker;
        private Thread stabilizingThread;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Start point of data to be analyzed
        /// </summary>
        private System.Drawing.Point startPoint = new System.Drawing.Point(0, 0);
        /// <summary>
        /// Indicate if workers are running
        /// </summary>
        private bool isRunning = false;


        /// <summary>
        /// Set to 'true' when the left mouse-button is down.
        /// </summary>
        private bool isLeftMouseButtonDownOnWindow = false;

        /// <summary>
        /// Set to 'true' when dragging the 'selection rectangle'.
        /// Dragging of the selection rectangle only starts when the left mouse-button is held down and the mouse-cursor
        /// is moved more than a threshold distance.
        /// </summary>
        private bool isDraggingSelectionRect = false;

        /// <summary>
        /// Records the location of the mouse (relative to the window) when the left-mouse button has pressed down.
        /// </summary>
        private System.Windows.Point origMouseDownPoint;

        /// <summary>
        /// The threshold distance the mouse-cursor must move before drag-selection begins.
        /// </summary>
        private static readonly double DragThreshold = 5;

        /// <summary>
        /// Width of current selection
        /// </summary>
        private int selectionWidth=640;
        /// <summary>
        /// Height of current selection
        /// </summary>
        private int selectionHeight=480;


        private int selectedWidth = 640;
        private int selectedHeight = 480;


        private bool selectedWindow = false;
        private OutputWindow outputWindow;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr dest, IntPtr source, int Length);
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            rotationList.Add(new kinectSandboxUI.Rotation(0, "0°"));
            rotationList.Add(new kinectSandboxUI.Rotation(1, "90°"));
            rotationList.Add(new kinectSandboxUI.Rotation(2, "180°"));
            rotationList.Add(new kinectSandboxUI.Rotation(3, "270°"));
            // Initialize logger
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            this.Log().Debug("Initialize UI components");
            InitializeComponent();
            // Bind Key
            
            
        }

        private void HandlerThatSavesEverthing(object obSender, ExecutedRoutedEventArgs e)
        {
            // Do the Save All thing here.
        }
        /// <summary>
        /// Initialize UI component
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Configure combobox rotation
            this.cbRotation.ItemsSource = this.rotationList;
            this.cbRotation.DisplayMemberPath = "Label";
            this.cbRotation.SelectedValuePath = "Id";
            foreach (kinectSandboxUI.Rotation rotation in this.cbRotation.ItemsSource)
            {
                if (rotation.Id == kinectSandboxUI.Properties.Settings.Default.Rotation)
                {
                    this.cbRotation.SelectedItem = rotation;
                    break;
                }
            }
            // Configure combobox gradient
            this.cbGradient.ItemsSource=this.loadGradient(kinectSandboxUI.Properties.Settings.Default.GradientPath);
            this.cbGradient.DisplayMemberPath = "Label";
            this.cbGradient.SelectedValuePath = "Id";
            this.cbGradient.SelectedIndex = 0;
            foreach (Gradient gradient in this.cbGradient.ItemsSource)
            {
                if (gradient.Label == kinectSandboxUI.Properties.Settings.Default.Gradient)
                {
                    this.cbGradient.SelectedItem = gradient;
                    break;
                }
            }

            this.startPoint = new System.Drawing.Point(kinectSandboxUI.Properties.Settings.Default.TopX, kinectSandboxUI.Properties.Settings.Default.TopY);
            this.selectedWidth = kinectSandboxUI.Properties.Settings.Default.SelectionWidth;
            this.selectedHeight = kinectSandboxUI.Properties.Settings.Default.SelectionHeight;

            if ((this.startPoint.X != 0) || (this.startPoint.Y != 0) || (this.selectedHeight != 480) || (this.selectedWidth != 640))
            {
                this.lblbtSelect.Content = "Reset sel.";
                this.selectedWindow = true;
                this.btSelect.IsEnabled = true;
            }

            // Initialize output bitmap
            this.colorBitmap = new WriteableBitmap(this.selectionWidth, this.selectionHeight   , 96.0, 96.0, PixelFormats.Bgr32, null);
            
            
        }


        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            this.launchWorkers();
        }
        /// <summary>
        /// Call settings Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindows = new SettingsWindow();
            if (settingsWindows.ShowDialog()==true) {
                this.cbGradient.ItemsSource = this.loadGradient(kinectSandboxUI.Properties.Settings.Default.GradientPath);
                this.cbGradient.DisplayMemberPath = "Label";
                this.cbGradient.SelectedValuePath = "Id";
                this.cbGradient.SelectedIndex = 0;
                foreach (Gradient gradient in this.cbGradient.ItemsSource)
                {
                    if (gradient.Label == kinectSandboxUI.Properties.Settings.Default.Gradient)
                    {
                        this.cbGradient.SelectedItem = gradient;
                        break;
                    }
                }
                kinectSandboxUI.Properties.Settings.Default.Gradient = ((Gradient)this.cbGradient.SelectedItem).Label;
                
            }
        }

        /// <summary>
        /// Save settings specified in UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btApplyConfig_Click(object sender, RoutedEventArgs e)
        {
            kinectSandboxUI.Properties.Settings.Default.Gradient = ((Gradient)this.cbGradient.SelectedItem).Label;
            kinectSandboxUI.Properties.Settings.Default.Rotation = ((kinectSandboxUI.Rotation)this.cbRotation.SelectedItem).Id;
            
            
            if (this.isRunning)
            {
                this.stopWorkers();
                this.launchWorkers();
            }

        }

        /// <summary>
        /// Load all gradient file in specified folder.
        /// Only jpg file with 256pixels width are considered.
        /// </summary>
        /// <param name="path">Path to load jpg files</param>
        /// <returns></returns>
        private List<Gradient> loadGradient(string path)
        {
            List<Gradient> result = new List<Gradient>();
            int cpt = 0;
            result.Add(new Gradient(cpt++, ""));
            
            if (Directory.Exists(path))
            {
                // Load jpg
                string[] files = Directory.GetFiles(path, "*.jpg");
                foreach (string file in files)
                {
                    Bitmap gradient = (Bitmap)Bitmap.FromFile(file);
                    if (gradient.Width == 256)
                    {
                        result.Add(new Gradient(cpt++, System.IO.Path.GetFileName(file)));
                    }
                }
            }
            return result;
        }

        private Bitmap getGradient()
        {
            Bitmap result = null;
            if (kinectSandboxUI.Properties.Settings.Default.GradientPath != "")
            {
                if (kinectSandboxUI.Properties.Settings.Default.Gradient != "")
                {
                    if (File.Exists(System.IO.Path.Combine(kinectSandboxUI.Properties.Settings.Default.GradientPath, kinectSandboxUI.Properties.Settings.Default.Gradient)))
                    {
                        result = (Bitmap)Bitmap.FromFile(System.IO.Path.Combine(kinectSandboxUI.Properties.Settings.Default.GradientPath, kinectSandboxUI.Properties.Settings.Default.Gradient));
                        if (result.Width == 256)
                        {
                            return result;
                        }
                        else
                        {
                            result = null;
                            this.displayWarning( "Invalid gradient file. Please review application configuration.");
                        }
                    }
                    else
                    {
                        this.displayWarning( "Unable to find gradient " + System.IO.Path.Combine(kinectSandboxUI.Properties.Settings.Default.GradientPath, kinectSandboxUI.Properties.Settings.Default.Gradient) + ". Please review application configuration.");
                    }
                }
                else
                {
                    this.displayWarning(  "No gradient file speficied. Please review application configuration.");
                }
            }
            else
            {
                this.displayWarning( "No folder specified for gradient image. Please review application settings.");
            }
            return result;
        }

        /// <summary>
        /// launch all workers with last available settings
        /// </summary>
        private void launchWorkers()
        {
            // Initialize output bitmap
            this.colorBitmap = new WriteableBitmap(this.selectedWidth, this.selectedHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            // Set the image we display to point to the bitmap where we'll put the image data
            this.img.Source = this.colorBitmap;

            if (outputWindow != null)
            {
                outputWindow.updateBitmap(this.colorBitmap);

            }
            // Initialize Kinect Worker
            this.kinectWorker = new KinectWorker();
            this.kinectWorker.MinDepth = (int) kinectSandboxUI.Properties.Settings.Default.MinDepth;
            this.kinectWorker.MaxDepth = (int) kinectSandboxUI.Properties.Settings.Default.MaxDepth;
            this.kinectWorker.setBoundingBox(this.startPoint, this.selectedWidth, this.selectedHeight);
            // If stabilization required Initialize StabilizingWorker
            if (kinectSandboxUI.Properties.Settings.Default.Stabilization)
            {
                this.stabilizingWorker = new StabilizingWorker(this.kinectWorker);
                this.stabilizingWorker.MinDepth = (int) kinectSandboxUI.Properties.Settings.Default.MinDepth;
                this.stabilizingWorker.MaxDepth = (int) kinectSandboxUI.Properties.Settings.Default.MaxDepth;
                this.stabilizingWorker.setBoundingBox(this.startPoint, this.selectedWidth, this.selectedHeight);
            }
            Bitmap gradient=this.getGradient();
            if (kinectSandboxUI.Properties.Settings.Default.Stabilization)
            {
                this.topographicWorker = new TopographicWorker(this.stabilizingWorker);
            }
            else
            {
                this.topographicWorker = new TopographicWorker(this.kinectWorker);
            }
            this.topographicWorker.MinDepth = (int) kinectSandboxUI.Properties.Settings.Default.MinDepth;
            this.topographicWorker.MaxDepth = (int) kinectSandboxUI.Properties.Settings.Default.MaxDepth;
            this.topographicWorker.Isolines = (int) kinectSandboxUI.Properties.Settings.Default.Isolines;
            this.topographicWorker.setBoundingBox(this.startPoint, this.selectedWidth, this.selectedHeight);
            if (gradient != null)
            {
                this.topographicWorker.Gradient = gradient;
            }
            // Wire output data event
            this.topographicWorker.OutputDataReady += new EventHandler<BitmapReadyEventArgs>(worker_OutputDataReady);
           

            if (this.kinectWorker.IsKinectReady())
            {
                // Launch Threads
                this.Log().Debug("Launch KinectWorker from thread " + Thread.CurrentThread.ManagedThreadId);
                if (this.stabilizingWorker != null)
                {
                    // Launch stabilizing thread
                    stabilizingThread = new Thread(this.stabilizingWorker.Start);
                    stabilizingThread.Name = "StabilizingWorker";
                    stabilizingThread.Start();
                }

                if (this.topographicWorker != null)
                {
                    //launch TopographicWorker Thread
                    topographicThread = new Thread(this.topographicWorker.Start);
                    topographicThread.Name = "TopographicWorker";
                    topographicThread.Start();
                }

                if (this.kinectWorker != null)
                {
                    // Launch KinectWorker Thread
                   kinectThread = new Thread(this.kinectWorker.Start);
                   kinectThread.Name = "KinectWorker";
                   kinectThread.Start();
                }
                this.btStart.IsEnabled = false;
                this.btStop.IsEnabled = true;
                this.btSaveImage.IsEnabled = true;
                this.menSettings.IsEnabled = false;
                this.isRunning = true;
                // Launch stabilizing thread
            }
            else
            {
                this.displayError("No kinect device connected.");
            }
        }


        private void stopWorkers()
        {
            

            if ((this.kinectWorker != null) && (this.kinectThread.IsAlive))
            {
                this.Log().Info("Stopping KinectWorker");
                this.kinectWorker.Stop();
                while (this.kinectThread.IsAlive)
                {
                    Thread.Sleep(10);
                }
            }
            this.isRunning = false;
        }

        private void worker_OutputDataReady(object sender, BitmapReadyEventArgs e)
        {

            try
            {
                Dispatcher.Invoke((Action)delegate()
                {
                    this.DrawImage2(e.Image);
                    this.statsbarFps.Text = (((double)Utility.CalculateFrameRate() / 10)).ToString("0.00", CultureInfo.InvariantCulture) + " fps";
                });
            }
            catch (Exception)
            {

            }
        }

        private void DrawImage(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                this.colorBitmap.Lock();
                CopyMemory(this.colorBitmap.BackBuffer, data.Scan0,
                    (this.colorBitmap.BackBufferStride * bitmap.Height));
                this.colorBitmap.AddDirtyRect(new Int32Rect((int)this.startPoint.X, (int)this.startPoint.Y, bitmap.Width, bitmap.Height));
                this.colorBitmap.Unlock();
            }
            finally
            {
                bitmap.UnlockBits(data);
                bitmap.Dispose();
            }
        }

        private void DrawImage2(Bitmap bitmap)
        {
            // Skip image with a bad format (can occur when changing screen zone selection
            if ((bitmap.Width == this.colorBitmap.Width) && (bitmap.Height == this.colorBitmap.Height))
            {
                BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Byte[] Pixels = new Byte[bitmap.Height * data.Stride];
                Marshal.Copy(data.Scan0, Pixels, 0, Pixels.Length);
                Int32Rect Rect = new Int32Rect(0, 0, bitmap.Width, bitmap.Height);
                this.colorBitmap.Lock();
                this.colorBitmap.WritePixels(Rect, Pixels, data.Stride, 0);
                this.colorBitmap.AddDirtyRect(Rect);
                this.colorBitmap.Unlock();
                bitmap.UnlockBits(data);
                bitmap.Dispose();
            }
        }

        private void displayInfo(string txt) {
            this.statsbarText.Text = txt;
            this.statusBar.Background = System.Windows.Media.Brushes.DarkGray;
        }

        private void displayWarning(string txt)
        {
            this.statsbarText.Text = txt;
            this.statusBar.Background = System.Windows.Media.Brushes.Orange;

        }

        private void displayError(string txt)
        {
            this.statsbarText.Text = txt;
            this.statusBar.Background = System.Windows.Media.Brushes.Red;

        }

        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            this.displayInfo("Stopping all process");
            this.stopWorkers();
            this.btStart.IsEnabled = true;
            this.btStop.IsEnabled = false;
            this.btSaveImage.IsEnabled = false;
            this.menSettings.IsEnabled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.stopWorkers();
            this.saveSettings();
            if (outputWindow != null)
            {
                outputWindow.Close();
            }
        }

        private void btSaveImage_Click(object sender, RoutedEventArgs e)
        {
            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = System.IO.Path.Combine(myPhotos, "KinectSandbox-" + time + ".png");
            // create a png bitmap encoder which knows how to save a .png file
            BitmapEncoder encoder = new PngBitmapEncoder();

            // create frame from the writable bitmap and add to encoder
            encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));
            // write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }
                this.displayInfo("Snapshot saved to: " + path);

            }
            catch (IOException ioe)
            {
                this.Log().Error("Unable to save snapshot " + path, ioe);
                this.displayError("Unable to save snapshot " + path);
            }
        }

        /// <summary>
        /// Quit the applciation "File Menu"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!selectedWindow)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    origMouseDownPoint = new System.Windows.Point((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y);


                    isLeftMouseButtonDownOnWindow = this.img.IsMouseOver;

                    if (isLeftMouseButtonDownOnWindow)
                    {
                        System.Windows.Point relativePoint = this.img.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                        this.displayInfo((int)(origMouseDownPoint.X - relativePoint.X) + "/" + (int)(origMouseDownPoint.Y - relativePoint.Y));
                        this.btSelect.IsEnabled = false;
                        this.CaptureMouse();
                    }

                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Initialize the rectangle used for drag selection.
        /// </summary>
        private void InitDragSelectionRect(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            UpdateDragSelectionRect(pt1, pt2);

            dragSelectionCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Update the position and size of the rectangle used for drag selection.
        /// </summary>
        private void UpdateDragSelectionRect(System.Windows.Point pt1, System.Windows.Point pt2)
        {
            int x, y, width, height;

            //
            // Determine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = (int)pt2.X;
                width = (int)(pt1.X - pt2.X);
            }
            else
            {
                x = (int)pt1.X;
                width = (int)(pt2.X - pt1.X);
            }

            if (pt2.Y < pt1.Y)
            {
                y = (int)pt2.Y;
                height = (int)(pt1.Y - pt2.Y);
            }
            else
            {
                y = (int)pt1.Y;
                height = (int)(pt2.Y - pt1.Y);
            }

            //
            // Update the coordinates of the rectangle used for drag selection.
            //
            Canvas.SetLeft(dragSelectionBorder, x);
            Canvas.SetTop(dragSelectionBorder, y);
            dragSelectionBorder.Width = width;
            dragSelectionBorder.Height = height;
            System.Windows.Point relativePoint = this.img.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            this.displayInfo((int)(origMouseDownPoint.X - relativePoint.X) + "/" + (int)(origMouseDownPoint.Y - relativePoint.Y) + " - " + width + "x" + height);
            this.selectionWidth = width;
            this.selectionHeight = height;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (!selectedWindow)
            {
                if (isDraggingSelectionRect)
                {
                    //
                    // Drag selection is in progress.
                    //
                    System.Windows.Point curMouseDownPoint = e.GetPosition(this);
                    UpdateDragSelectionRect(origMouseDownPoint, curMouseDownPoint);

                    e.Handled = true;
                }
                else if (isLeftMouseButtonDownOnWindow)
                {


                    //
                    // The user is left-dragging the mouse,
                    // but don't initiate drag selection until
                    // they have dragged past the threshold value.
                    //
                    System.Windows.Point curMouseDownPoint = e.GetPosition(this);
                    var dragDelta = curMouseDownPoint - origMouseDownPoint;
                    double dragDistance = Math.Abs(dragDelta.Length);
                    if (dragDistance > DragThreshold)
                    {
                        //
                        // When the mouse has been dragged more than the threshold value commence drag selection.
                        //
                        isDraggingSelectionRect = true;

                        //
                        //  Clear selection immediately when starting drag selection.
                        //


                        InitDragSelectionRect(origMouseDownPoint, curMouseDownPoint);
                    }


                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// handle when a selection has ended
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!selectedWindow)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (isDraggingSelectionRect)
                    {
                        //
                        // Drag selection has ended, apply the 'selection rectangle'.
                        //

                        isDraggingSelectionRect = false;
                        e.Handled = true;
                        System.Windows.Point endPoint = e.GetPosition(this);

                        System.Windows.Point relativePoint = this.img.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                        if (endPoint.X > relativePoint.X + this.img.ActualWidth)
                        {
                            endPoint.X = relativePoint.X + this.img.ActualWidth;
                        }
                        if (endPoint.Y > relativePoint.Y + this.img.ActualHeight)
                        {
                            endPoint.Y = relativePoint.Y + this.img.ActualHeight;
                        }
                        UpdateDragSelectionRect(origMouseDownPoint, endPoint);
                        this.btSelect.IsEnabled = true;
                    }

                    if (isLeftMouseButtonDownOnWindow)
                    {
                        isLeftMouseButtonDownOnWindow = false;
                        this.ReleaseMouseCapture();

                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Select a part of the kinect sensor frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedWindow)
            {
                dragSelectionCanvas.Visibility = Visibility.Collapsed;
                kinectSandboxUI.Properties.Settings.Default.Gradient = ((Gradient)this.cbGradient.SelectedItem).Label;
                kinectSandboxUI.Properties.Settings.Default.Rotation = ((kinectSandboxUI.Rotation)this.cbRotation.SelectedItem).Id;
                System.Windows.Point relativePoint = this.img.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                System.Drawing.Point top = new System.Drawing.Point((int)(origMouseDownPoint.X - relativePoint.X), (int)(origMouseDownPoint.Y - relativePoint.Y));
                // Convert origin, width height to sensor frame size
                this.startPoint = new System.Drawing.Point((int)((top.X * 640) / this.img.ActualWidth), (int)((top.Y * 480) / this.img.ActualHeight));
                this.selectedWidth = (int)((this.selectionWidth * 640) / this.img.ActualWidth);
                this.selectedHeight = (int)((this.selectionHeight * 480) / this.img.ActualHeight);
                this.lblbtSelect.Content = "Reset sel.";
                this.selectedWindow = true;
            }
            else
            {
                this.startPoint = new System.Drawing.Point(0, 0);
                this.selectedWidth = 640;
                this.selectedHeight = 480;
                this.lblbtSelect.Content = "Select";
                this.selectedWindow = false;
                this.btSelect.IsEnabled = false;
            }


            if (this.isRunning)
            {
                this.stopWorkers();
                this.launchWorkers();
            }

        }
        /// <summary>
        /// Display output window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btoutput_Click(object sender, RoutedEventArgs e)
        {
            kinectSandboxUI.Properties.Settings.Default.Rotation = ((kinectSandboxUI.Rotation)this.cbRotation.SelectedItem).Id;
            if (outputWindow == null)
            {
                outputWindow = new OutputWindow(this.colorBitmap);
                outputWindow.Show();
               
            }
            else
            {
                if (outputWindow.IsVisible)
                {
                    outputWindow.Close();
                    outputWindow = null;
                }
                else
                {
                    outputWindow = new OutputWindow(this.colorBitmap);
                    outputWindow.Show();
                }
            }
        }

        /// <summary>
        /// handle press on F11 key to togge output window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                if (outputWindow == null)
                {
                    outputWindow = new OutputWindow(this.colorBitmap);
                    outputWindow.Show();

                }
                else
                {

                    if (outputWindow.IsVisible)
                    {
                        outputWindow.Close();
                        outputWindow = null;
                    }
                    else
                    {
                        outputWindow = new OutputWindow(this.colorBitmap);
                        outputWindow.Show();
                    }
                }
            }
        }

        
        /// <summary>
        /// Change output confgiuration (rotation, flip)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btApplyoutput_Click(object sender, RoutedEventArgs e)
        {
            if (outputWindow != null)
            {
                kinectSandboxUI.Properties.Settings.Default.Rotation = ((kinectSandboxUI.Rotation)this.cbRotation.SelectedItem).Id;
                this.outputWindow.updateLayout();

            }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        private void saveSettings()
        {
            kinectSandboxUI.Properties.Settings.Default.Gradient = ((Gradient)this.cbGradient.SelectedItem).Label;
            kinectSandboxUI.Properties.Settings.Default.Rotation = ((kinectSandboxUI.Rotation)this.cbRotation.SelectedItem).Id;
            kinectSandboxUI.Properties.Settings.Default.TopX = this.startPoint.X;
            kinectSandboxUI.Properties.Settings.Default.TopY = this.startPoint.Y;
            kinectSandboxUI.Properties.Settings.Default.SelectionWidth = this.selectedWidth;
            kinectSandboxUI.Properties.Settings.Default.SelectionHeight = this.selectedHeight;
            kinectSandboxUI.Properties.Settings.Default.Save();
        }
        
    }
}
