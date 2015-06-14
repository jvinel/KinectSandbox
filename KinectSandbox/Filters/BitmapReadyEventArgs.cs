﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace KinectSandbox.Filters
{
    /// <summary>
    /// Event raised when a data filter has produce some output
    /// </summary>
    public class BitmapReadyEventArgs: EventArgs
    {
        /// <summary>
        /// Data generated by filter
        /// </summary>
        public Bitmap Image { get; set; }
        /// <summary>
        /// Arguments to event: data produced by filter
        /// </summary>
        /// <param name="data"></param>
        public BitmapReadyEventArgs(Bitmap image)
        {
            this.Image = image;
        }

    }
}
