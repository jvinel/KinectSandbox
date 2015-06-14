using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KinectSandbox.Tools
{
    public class Utility
    {

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
            for (int i = start; i < end; ++i)
            {
                int x = i % 640;
                int y = i / 640;
                // Check if current index is located inside data specified
                if ((x >= xOrigin) && (y >= yOrigin) && (x < xOrigin + width) && (y < yOrigin + height))
                {
                    int depth = depthPixels[i].Depth;
                    // Check depth regarding boundaries
                    if ((depth > minDepth) && (depth < maxDepth))
                    {
                        int intensity = 255- (((depth - minDepth) * 255) / (maxDepth - minDepth));
                        // Scale intensity to a 0..255 interval
                        lockBitmap.SetPixel(x - xOrigin, y - yOrigin, Color.FromArgb(intensity, intensity, intensity));

                    }
                    else
                    {
                        if (depth <= minDepth)
                        {
                            lockBitmap.SetPixel(x - xOrigin, y - yOrigin, Color.White);
                        }
                        else
                        {
                            lockBitmap.SetPixel(x - xOrigin, y - yOrigin, Color.Black);
                        }
                    }
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
