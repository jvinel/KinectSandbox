using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using KinectSandbox.Filters;
using System.Drawing;
using KinectSandbox.Tools;

namespace KinectSandbox.Workers
{
    class KinectWorker : DataFilterInput
    {

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private DepthImagePixel[] depthPixels;
        
        private bool requestStop = false;

        public AutoResetEvent autoResetEvent;

        public KinectWorker()
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            this.Log().Debug("List all sensors and check if one is connected");
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                this.Log().Info("Kinect sensor found, intializing depth feed at 640x480 30FPS");
                // Turn on the depth stream to receive depth frames
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                // Allocate space to put the depth pixels we'll receive
                this.depthPixels = new DepthImagePixel[this.sensor.DepthStream.FramePixelDataLength];


                // Add an event handler to be called whenever there is new depth frame data
                this.sensor.DepthFrameReady += this.SensorDepthFrameReady;
            }
            
            autoResetEvent = new AutoResetEvent(false);
            //this.filters = new Dictionary<string, DataFilter>();
            //this.filtersSettings = new Dictionary<string, List<KeyValuePair<string, string>>>();
        }

        /// <summary>
        /// Start KinectWorker.
        /// This method is launched in a separate thread, waiting to receive a new depth frame from kinect device.
        /// And passing it to the next filter through OnDataReady event.
        /// </summary>
        public void Start()
        {

            try
            {
                this.requestStop = false;
                this.Log().Debug("Launch sensors from thread " + Thread.CurrentThread.ManagedThreadId);
                this.sensor.Start();

                while (!this.requestStop)
                {
                    // Wait for autoreset event (raised when a new frame is received)
                    autoResetEvent.WaitOne();
#if TRACE
                    this.Log().Debug("AutoReset Event received");
#endif
                    OnDataReady(new DataReadyEventArgs(this.depthPixels));
                    
                }

                if (null != this.sensor)
                {
                    this.sensor.Stop();
                }
            }
            catch (IOException)
            {
                this.sensor = null;
            }
        }

        /// <summary>
        /// Boolean indicating if a Kinect Device is connected and in "Ready" state
        /// </summary>
        /// <returns></returns>
        public bool IsKinectReady()
        {
            return (null != this.sensor);
        }

        /// <summary>
        /// Stop the thread loop
        /// </summary>
        public void Stop()
        {
            this.requestStop = true;
            
            this.autoResetEvent.Set();
        }

        
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
#if TRACE
            this.Log().Debug("Received frame from thread " + Thread.CurrentThread.ManagedThreadId);
#endif
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    depthFrame.CopyDepthImagePixelDataTo(this.depthPixels);
                    // Raise AutoReset event to call next filers (cf. Start method)                    
                    autoResetEvent.Set();
                }
            }
        }

        /// <summary>
        /// Define boundng box of data to be treated.
        /// Data outside this bounding box are ignored.
        /// </summary>
        /// <param name="p">Origin point</param>
        /// <param name="width">Bounding box width</param>
        /// <param name="height">Bounding box height</param>
        public void setBoundingBox(System.Windows.Point p, int width, int height)
        {
            this.StartPoint = p;
            this.Width = width;
            this.Height = height;
        }


        

      

        
    }
}
