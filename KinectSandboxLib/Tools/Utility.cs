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
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectSandboxLib.Tools
{
    /// <summary>
    /// Contains static tools used to work with data received
    /// </summary>
    public class Utility
    {
        /// <summary>
        /// Palette used to generate Bitmap (8bpp grayscale indexed)
        /// </summary>
        protected static ColorPalette palette;
        /// <summary>
        /// Convert a depthImagePixel array to a bitmap image. Only data inside selected area (defined by xOrigin, yOrigin, height and width) are considered.
        /// Resulting bitmap has a width, height equal to height, width parameter.
        /// Result is a grey scale image (8bpp), pixel intensity goes from 0 to 255 according to minDepth and maxDepth specified.
        /// </summary>
        /// <param name="depthPixels">Array of depth pixels from Kinect device</param>
        /// <param name="xOrigin">Start X position</param>
        /// <param name="yOrigin">Start Y position</param>
        /// <param name="height">Height of bitmap</param>
        /// <param name="width">Width of bitmap</param>
        /// <param name="minDepth">minimum depth of date converted, every point with value inferior than this is set to 0 intensity</param>
        /// <param name="maxDepth">maximum depth of date converted, every point with value greater than this is set to 255 intensity</param>
        /// <returns>Bitmap 8pp grayscale image</returns>
        public static Bitmap convertToBitmap(DepthImagePixel[] depthPixels, int xOrigin, int yOrigin, int height, int width, int minDepth, int maxDepth)
        {
#if TRACE
            "Utility".Log().Debug("Convert to Bitmap: " + xOrigin + "," + yOrigin + "," + height + "," + width + ", " + minDepth + "," + maxDepth);
#endif
            if (Utility.palette == null)
            {
                Bitmap temp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed);
                Utility.palette = temp.Palette;
#if TRACE
                "Utility".Log().Debug("No palette found, need to create a new one");
#endif
                for (int i = 0; i < 255; i++)
                {
                    Utility.palette.Entries[i] = Color.FromArgb(i, i, i);
                }
            }
            Bitmap result = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            LockBitmap lockBitmap = new LockBitmap(result);
            lockBitmap.LockBits();
            // Start point equals to yOrigin*640 (line width=640 + xOrigin 
            int start = yOrigin * 640 + xOrigin;
            int end = (yOrigin + height) * 640 + xOrigin + width;
            int cpt = start;
            while (cpt < end) 
            {
                int x = (cpt % 640);
                int y = (cpt / 640);
                
                int yOutput = y - yOrigin;
                int xOutput = x - xOrigin ;
                
                // Check if current index is located inside data specified
                if ((xOutput >= 0) && (yOutput >= 0) && (xOutput < width) && (yOutput<height))
                {
                    int depth = depthPixels[cpt].Depth;
                    // Check depth regarding boundaries
                    if ((depth > minDepth) && (depth < maxDepth))
                    {
                        int intensity = 255 - (((depth - minDepth) * 255) / (maxDepth - minDepth));
                        // Scale intensity to a 0..255 interval
                        lockBitmap.SetPixel(xOutput, yOutput, Color.FromArgb(intensity, intensity, intensity));

                    }
                    else
                    {
                        if (depth <= minDepth)
                        {
                            lockBitmap.SetPixel(xOutput, yOutput, Color.White);
                        }
                        else
                        {
                            lockBitmap.SetPixel(xOutput, yOutput, Color.Black);
                        }
                    }
                    cpt++;
                }
                else
                {
                    // Got to next line
                    cpt = (y + 1) * 640 + xOrigin;
                }
            }
            lockBitmap.UnlockBits();
            result.Palette = Utility.palette;
            return result;

        }

        #region Basic Frame Counter
        private static int lastTick = 0;
        private static int lastFrameRate = 0;
        private static int frameRate = 0;
        /// <summary>
        /// Return framerate (10 seconds based). This function shall be called after every UI update
        /// </summary>
        /// <returns>Count of frame updated in the last 10 seconds</returns>
        public static int CalculateFrameRate()
        {
            if (System.Environment.TickCount - lastTick >= 10000)
            {
                lastFrameRate = frameRate;
                frameRate = 0;
                lastTick = System.Environment.TickCount;
            }
            frameRate++;
            return lastFrameRate;
        }
        #endregion
    }
}
