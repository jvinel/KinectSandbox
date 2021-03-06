﻿/*
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
using AForge.Imaging.Filters;
using KinectSandboxLib.Filters;
using KinectSandboxLib.Tools;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSandboxLib.Workers
{
    /// <summary>
    /// TopographicWorker main goals is to convert depth frame received from akinect device to a bitmap image.
    /// It also adds isolines and colorize image (from 8bpp grayscale to 24 rgb) using gradient bitmap specified. 
    /// 
    /// </summary>
    public class TopographicWorker: DataFilter
    {
        /// <summary>
        /// Image width: 255 pixels with color gradient used for conversion between grayscale and rgb
        /// </summary>
        public Bitmap Gradient { get; set; }

        public int Isolines { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataFilterInput"></param>
        public TopographicWorker(DataFilterInput dataFilterInput)
            : base(dataFilterInput)
        {
            this.Isolines = 0;
        }

        /// <summary>
        /// Convert a DepthImagePixel array to a bitmap.
        /// Bitmap is colorize if a gradient bitmap has been set, isolines are also added as an overlay
        /// </summary>
        /// <param name="sourceData">DepthImagePixel array (generated by Kinect device depth sensor)</param>
        protected override void Process(DepthImagePixel[] sourceData)
        {
#if TRACE
            this.Log().Debug("New data received to process");
#endif
            Bitmap source = Utility.convertToBitmap(sourceData, (int)this.StartPoint.X, (int)this.StartPoint.Y, this.Height, this.Width, this.MinDepth, this.MaxDepth);
            // Apply a blur on source image (smothing isolines and output)
            Blur blur = new Blur();
            source = blur.Apply(source);
            // Clone bitmap, final will be colorize, than isolines will be applied
            Bitmap final = source.Clone(new System.Drawing.Rectangle(0, 0, source.Width, source.Height), source.PixelFormat);
            // If gradient bitmap available colorize image
            if (this.Gradient != null)
            {
                ColorizedAlpha cAlpha = new ColorizedAlpha(this.Gradient);
                final = cAlpha.Apply(final);
            }
            else
            {
                // Convert isolines to rgb (to merge with final image)
                GrayscaleToRGB gtoRGB = new GrayscaleToRGB();
                // apply the filter
                final = gtoRGB.Apply(final);
            }

            if (Isolines > 0)
            {
                // Generate isolines (startting at level 10, step 25)
                int level = (int) Math.Round((double) (255/this.Isolines));
                int step = level;
                // Isolines bitmap will receive every islone generated, then added to final image
                Bitmap isolines = new Bitmap(this.Width, this.Height, PixelFormat.Format8bppIndexed);
                while (level < 255)
                {
                    // Every point over level will be set to black, otherwise white (easier and faster for edge detection)
                    Threshold threshold = new Threshold(level);
                    Bitmap temp = threshold.Apply(source);

                    // Detect Edge using sobel alogorithmn
                    //SobelEdgeDetector sobel = new SobelEdgeDetector();
                    Edges sobel = new Edges();
                    temp = sobel.Apply(temp);

                    // Add edge bitmap to tempora
                    Add filter = new Add(temp);
                    //filter.SourcePercent = 0.6;
                    // apply the filter
                    filter.ApplyInPlace(isolines);

                    // Set next level value
                    level = level + step;
                }

                // Convert isolines to rgb (to merge with final image)
                GrayscaleToRGB gtoRGB = new GrayscaleToRGB();
                // apply the filter
                Bitmap rgbIsoline = gtoRGB.Apply(isolines);

                // Add isolines to final image
                CustomMorph cMorph = new CustomMorph(rgbIsoline);
                cMorph.SourcePercent = 0.5;
                final = cMorph.Apply(final);
            }

            // Pass bitmap generated to UI (or other filter)
            OnOutputDataReady(new BitmapReadyEventArgs(final));
        }

       
    }
}
