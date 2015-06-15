using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;

namespace KinectSandboxLib.Filters
{
    public  class DataFilterInput
    {
        /// <summary>
        /// Event handling new data to be processed
        /// </summary>
        public event EventHandler<DataReadyEventArgs> DataReady;
        public event EventHandler<BitmapReadyEventArgs> OutputDataReady;

        private int minDepth;

        public int MinDepth
        {
            get { return this.minDepth; }
            set
            {
                this.minDepth = value;
            }

        }

        private int maxDepth;
        public int MaxDepth
        {
            get { return this.maxDepth; }
            set
            {
                this.maxDepth = value;
            }

        }

        private Point startPoint = new Point(0, 0);
        private int width = 640;
        private int height = 480;

        public int Width
        {
            get { return this.width; }
            set { this.width = value; }
        }

        public int Height
        {
            get { return this.height; }
            set { this.height = value; }
        }

        public Point StartPoint
        {
            get { return this.startPoint; }
            set { this.startPoint = value; }
        }

        /// <summary>
        /// Handler of data ready to be processed
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataReady(DataReadyEventArgs e)
        {
            #if TRACE
            this.Log().Debug("New data processed and ready for next filter.");
            #endif
            if (DataReady != null)
                DataReady(this, e);
        }

        protected virtual void OnOutputDataReady(BitmapReadyEventArgs e)
        {
            
            if (OutputDataReady != null)
                OutputDataReady(this, e);
        }
    }
}
