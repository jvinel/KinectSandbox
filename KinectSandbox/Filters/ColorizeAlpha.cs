using AForge.Imaging;
using AForge.Imaging.Filters;
using KinectSandbox.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace KinectSandbox.Filters
{
    public class ColorizedAlpha : BaseFilter
    {
        
        public Bitmap GradientBitmap { get; set; }

        // private format translation dictionary
        private readonly Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        

        public ColorizedAlpha(Bitmap gradient)
        {
            GradientBitmap = gradient;
            // initialize format translation dictionary
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format24bppRgb;
        }

        protected override unsafe void ProcessFilter(UnmanagedImage sourceData, UnmanagedImage destinationData)
        {
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
                    //dst[RGB.A] = *src;


                        Color color = lockBitmap.GetPixel(*src, 0);
                        dst[RGB.R] = color.R;
                        dst[RGB.G] = color.G;
                        dst[RGB.B] = color.B;
                    
                }
                src += srcOffset;
                dst += dstOffset;
            }
            lockBitmap.UnlockBits();
        }
    }
}
