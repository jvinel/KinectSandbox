/*
 * This file is part of KinectSandbox application. https://github.com/jvinel/KinectSandbox
 * Copyright (C) 2015 Julien Vinel jvinel@gmail.com
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
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
        /// Define boundng box of data to be treated.
        /// Data outside this bounding box are ignored.
        /// </summary>
        /// <param name="p">Origin point</param>
        /// <param name="width">Bounding box width</param>
        /// <param name="height">Bounding box height</param>
        public void setBoundingBox(System.Drawing.Point p, int width, int height)
        {
            this.StartPoint = p;
            this.Width = width;
            this.Height = height;
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
