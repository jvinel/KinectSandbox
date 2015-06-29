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
using AForge.Imaging;
using AForge.Imaging.Filters;
using KinectSandboxLib.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace KinectSandboxLib.Filters
{
    /// <summary>
    /// Transform a grayscale 8bpp bitmap to a 24bpp rgb using gradient image provided
    /// </summary>
    public class ColorizedAlpha : BaseFilter
    {
        /// <summary>
        /// Bitmap used to colorize image.
        /// Bitmap must be at least 255 pixel width
        /// </summary>
        public Bitmap GradientBitmap { get; set; }

        /// <summary>
        /// Private format translation dictionary 
        /// </summary>
        private readonly Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        /// <summary>
        /// Format translation getter
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gradient">Gradient i_mage used to colorize bitmap</param>
        public ColorizedAlpha(Bitmap gradient)
        {
            GradientBitmap = gradient;
            // initialize format translation dictionary
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format24bppRgb;
        }

        /// <summary>
        /// Apply colorize filter, iterating through all bitmap pixels
        /// </summary>
        /// <param name="sourceData"></param>
        /// <param name="destinationData"></param>
        protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData)
        {
            // Use lockbitmap to have a fast access on all pixels
            LockBitmap lockBitmap = new LockBitmap(GradientBitmap);
            lockBitmap.LockBits();
            // get width and height
            int width = sourceData.Width;
            int height = sourceData.Height;

            int srcOffset = sourceData.Stride - width;
            int dstOffset = destinationData.Stride - width * 3;

            // do the job
            var src = (byte*)sourceData.ImageData.ToPointer();
            var dst = (byte*)destinationData.ImageData.ToPointer();

            // for each line
            for (int y = 0; y < height; y++)
            {
                // for each pixel
                for (int x = 0; x < width; x++, src++, dst += 3)
                {
                    // Get piksel from gradient image
                    Color color = lockBitmap.GetPixel(*src, 0);
                    // Set color from gradient piksel.
                    dst[RGB.R] = color.R;
                    dst[RGB.G] = color.G;
                    dst[RGB.B] = color.B;
                }
                src += srcOffset;
                dst += dstOffset;
            }

            // Unlock bitmap
            lockBitmap.UnlockBits();
        }
    }
}
