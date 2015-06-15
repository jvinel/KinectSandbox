using KinectSandboxLib.Filters;
using KinectSandboxLib.Tools;
using KinectSandboxLib.Workers;
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

namespace KinectSandbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<System.Windows.Media.Color> initColors = new List<Color>();
        Dictionary<short, short> colorGradients = new Dictionary<short, short>();
        //List<Color> colorList;

        private KinectWorker kinectWorker;
        private TopographicWorker topographicWorker;
        private StabilizingWorker stabilizingWorker;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

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



        private int selectionWidth;
        private int selectionHeight;
        private System.Drawing.Point startPoint = new System.Drawing.Point(0, 0);
        private int width = 640;
        private int height = 480;

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr dest, IntPtr source, int Length);

        public MainWindow()
        {
            // Initialize logger
            LoggingExtensions.Logging.Log.InitializeWith<LoggingExtensions.log4net.Log4NetLog>();
            this.Log().Debug("Initialize UI components");
            InitializeComponent();
            this.Log().Debug("Initialize KinectWorker");
            this.kinectWorker = new KinectWorker();
            Bitmap gradient = (Bitmap)Bitmap.FromFile("Gradient\\gradient3.jpg");
            this.stabilizingWorker = new StabilizingWorker(this.kinectWorker);
            this.topographicWorker = new TopographicWorker(this.stabilizingWorker);
            this.topographicWorker.OutputDataReady += new EventHandler<BitmapReadyEventArgs>(kinectWorker_DataReady);
            this.topographicWorker.Gradient = gradient;
        }

        /// <summary>
        /// Event fired when slider of minimum depth is changed.
        /// Need to update KinectWorker settings if it is running
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slMinDepth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Log().Debug("Minimum depth updated to " + (short)Math.Round(slMinDepth.Value));
            if ((slMaxDepth != null) && (slMaxDepth.Value < slMinDepth.Value))
            {
                slMaxDepth.Value = slMinDepth.Value;
            }
            if (this.statusBarText != null)
            {
                this.statusBarText.Text = "Min. depth: " + Math.Round(slMinDepth.Value) + " mm";
            }
            if (this.kinectWorker != null)
            {
                this.kinectWorker.MinDepth = (short)Math.Round(slMinDepth.Value);
            }
        }

        /// <summary>
        /// Event fired when slider of maximum depth is changed.
        /// Need to update KinectWorker settings if it is running
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slMaxDepth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            this.Log().Debug("Maximum depth updated to " + (short)Math.Round(slMaxDepth.Value));
            if ((slMinDepth != null) && (slMaxDepth.Value < slMinDepth.Value))
            {
                slMinDepth.Value = slMaxDepth.Value;
            }
            if (this.statusBarText != null)
            {
                this.statusBarText.Text = "Max. depth: " + Math.Round(slMaxDepth.Value) + " mm";
            }
            //updateColors();
            if (this.kinectWorker != null)
            {
                this.kinectWorker.MaxDepth = (short)Math.Round(slMaxDepth.Value);
            }
        }

        /// <summary>
        /// Initialize bitmap and check kinect device availability
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize output bitmap
            this.colorBitmap = new WriteableBitmap(640, 480, 96.0, 96.0, PixelFormats.Bgr32, null);
            // Set the image we display to point to the bitmap where we'll put the image data
            this.image.Source = this.colorBitmap;
            // Check if kinect device is available
            if (!this.kinectWorker.IsKinectReady())
            {
                this.Log().Error("No kinect device connected. Disabling start button.");
                this.btStart.IsEnabled = false;
                this.statusBarText.Text = "No Kinect device connected ! Check connection and restart application";
            }
        }

        /// <summary>
        /// Stop all threads running before closing application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Log().Debug("Application quitting. Trying to finish cleanly all threads");
            if (null != this.kinectWorker)
            {
                this.kinectWorker.Stop();
                this.topographicWorker.Stop();
                this.stabilizingWorker.Stop();
            }
            
        }

        /// <summary>
        /// Launch kinectworker thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btStart_Click(object sender, RoutedEventArgs e)
        {
            this.Log().Debug("Launch KinectWorker from thread " + Thread.CurrentThread.ManagedThreadId);
            this.kinectWorker.MinDepth = (short)Math.Round(this.slMinDepth.Value);
            this.kinectWorker.MaxDepth = (short)Math.Round(this.slMaxDepth.Value);
            //this.updateFilters();
            this.topographicWorker.MinDepth = (short)Math.Round(this.slMinDepth.Value);
            this.topographicWorker.MaxDepth = (short)Math.Round(this.slMaxDepth.Value);

            this.stabilizingWorker.MinDepth = (short)Math.Round(this.slMinDepth.Value);
            this.stabilizingWorker.MaxDepth = (short)Math.Round(this.slMaxDepth.Value);



            this.btStart.IsEnabled = false;
            this.btStop.IsEnabled = true;
            this.btSnapshot.IsEnabled = true;
            // Launch stabilizing thread
            Thread stabThread = new Thread(this.stabilizingWorker.Start);
            stabThread.Name = "StabilizingWorker";
            stabThread.Start();


            //launch TopographicWorker Thread
            Thread topoThread = new Thread(this.topographicWorker.Start);
            topoThread.Name = "TopographicWorker";
            topoThread.Start();

            // Launch KinectWorker Thread
            Thread kinectThread = new Thread(this.kinectWorker.Start);
            kinectThread.Name = "KinectWorker";
            kinectThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btStop_Click(object sender, RoutedEventArgs e)
        {
            this.Log().Debug("Stopping KinectWorker (user request)");
            this.kinectWorker.Stop();
            this.topographicWorker.Stop();
            this.stabilizingWorker.Stop();

            this.btStart.IsEnabled = true;
            this.btStop.IsEnabled = false;
            this.btSnapshot.IsEnabled = false;
        }

        /// <summary>
        /// Save last image generated to MyPictures folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSnapshot_Click(object sender, RoutedEventArgs e)
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

            }
            catch (IOException ioe)
            {
                this.Log().Error("Unable to save snapshot " + path, ioe);
            }

        }

        private void slStabilize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (chkActivStab.IsChecked == true)
            {
                //this.updateFilters();
            }

        }





        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                origMouseDownPoint = new System.Windows.Point((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y);

                Console.WriteLine("Mouse over image: " + this.image.IsMouseOver);
                isLeftMouseButtonDownOnWindow = this.image.IsMouseOver;

                if (isLeftMouseButtonDownOnWindow)
                {
                    System.Windows.Point relativePoint = this.image.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                    this.statusBarText.Text = (int)(origMouseDownPoint.X - relativePoint.X) + "/" + (int)(origMouseDownPoint.Y - relativePoint.Y);
                    this.btSelect.IsEnabled = false;
                    this.CaptureMouse();
                }

                e.Handled = true;
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
                x = (int) pt2.X;
                width = (int) (pt1.X - pt2.X);
            }
            else
            {
                x = (int) pt1.X;
                width = (int) (pt2.X - pt1.X);
            }

            if (pt2.Y < pt1.Y)
            {
                y =(int)  pt2.Y;
                height = (int) (pt1.Y - pt2.Y);
            }
            else
            {
                y = (int) pt1.Y;
                height = (int) (pt2.Y - pt1.Y);
            }

            //
            // Update the coordinates of the rectangle used for drag selection.
            //
            Canvas.SetLeft(dragSelectionBorder, x);
            Canvas.SetTop(dragSelectionBorder, y);
            dragSelectionBorder.Width = width;
            dragSelectionBorder.Height = height;
            System.Windows.Point relativePoint = this.image.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            this.statusBarText.Text = (int)(origMouseDownPoint.X - relativePoint.X) + "/" + (int)(origMouseDownPoint.Y - relativePoint.Y) + " - " + width + "x" + height;
            this.selectionWidth = width;
            this.selectionHeight = height;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
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

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
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
                    System.Windows.Point relativePoint = this.image.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                    if (endPoint.X > relativePoint.X + 640)
                    {
                        endPoint.X = relativePoint.X + 640;
                    }
                    if (endPoint.Y > relativePoint.Y + 480)
                    {
                        endPoint.Y = relativePoint.Y + 480;
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

        private void btSelect_Click(object sender, RoutedEventArgs e)
        {
            dragSelectionCanvas.Visibility = Visibility.Collapsed;
            btSelect.IsEnabled = false;
            if (this.kinectWorker != null)
            {
                System.Windows.Point relativePoint = this.image.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
                System.Drawing.Point top = new System.Drawing.Point((int)(origMouseDownPoint.X - relativePoint.X), (int)(origMouseDownPoint.Y - relativePoint.Y));
                this.startPoint = top;
                this.height = this.selectionHeight;
                this.width = this.selectionWidth;
                this.kinectWorker.setBoundingBox(top, this.selectionWidth, this.selectionHeight);
            }
        }







        private void kinectWorker_DataReady(object sender, BitmapReadyEventArgs e)
        {
#if TRACE
            this.Log().Debug("Event received from KinectWorker from thread " + Thread.CurrentThread.ManagedThreadId);
#endif
            try
            {
                Dispatcher.Invoke((Action)delegate()
                {
                    this.DrawImage(e.Image);
                    this.statsbarFps.Text = (((double)Utility.CalculateFrameRate() / 10)).ToString("0.00", CultureInfo.InvariantCulture) + " fps";
                });
            }
            catch (Exception)
            {

            }
        }

        private void DrawImage(Bitmap bitmap)
        {
            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle((int)this.startPoint.X, (int)this.startPoint.Y, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
    }
}
