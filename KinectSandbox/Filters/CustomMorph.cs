using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace KinectSandbox.Filters
{
    /// <summary>
    /// Merge two images in a single one.
    /// Source percent is used to apply second image piksel onto the first one.
    /// </summary>
    public class CustomMorph : BaseInPlaceFilter2
    {
        private double	sourcePercent = 0.50;

        // private format translation dictionary
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>( );

        /// <summary>
        /// Format translations dictionary.
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        /// <summary>
        /// Percent of source image to keep, [0, 1].
        /// </summary>
        /// 
        /// <remarks><para>The property specifies the percentage of source pixels' to take. The
        /// rest is taken from an overlay image.</para></remarks>
        /// 
        public double SourcePercent
        {
            get { return sourcePercent; }
            set { sourcePercent = Math.Max( 0.0, Math.Min( 1.0, value ) ); }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Morph"/> class.
        /// </summary>
        public CustomMorph( )
        {
            InitFormatTranslations( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Morph"/> class.
        /// </summary>
        /// 
        /// <param name="overlayImage">Overlay image.</param>
        /// 
        public CustomMorph( Bitmap overlayImage )
            : base( overlayImage )
        {
            InitFormatTranslations( );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Morph"/> class.
        /// </summary>
        /// 
        /// <param name="unmanagedOverlayImage">Unmanaged overlay image.</param>
        /// 
        public CustomMorph(UnmanagedImage unmanagedOverlayImage)
            : base( unmanagedOverlayImage )
        {
            InitFormatTranslations( );
        }

        /// <summary>
        /// Initialize format translation dictionary
        /// </summary>
        private void InitFormatTranslations( )
        {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb]    = PixelFormat.Format24bppRgb;
        }

        /// <summary>
        /// Process the filter on the specified image.
        /// </summary>
        /// 
        /// <param name="image">Source image data.</param>
        /// <param name="overlay">Overlay image data.</param>
        ///
        protected override unsafe void ProcessFilter( UnmanagedImage image, UnmanagedImage overlay )
        {
            // get image dimension
            int width  = image.Width;
            int height = image.Height;

            // initialize other variables
            int pixelSize = ( image.PixelFormat == PixelFormat.Format8bppIndexed ) ? 1 : 3;
            int lineSize  = width * pixelSize;
            int offset    = image.Stride - lineSize;
            int ovrOffset = overlay.Stride - lineSize;
            // percentage of overlay image
            double q = 1.0 - sourcePercent;

            // do the job
            byte * ptr = (byte*) image.ImageData.ToPointer( );
            byte * ovr = (byte*) overlay.ImageData.ToPointer( );

            // for each line
            for ( int y = 0; y < height; y++ )
            {
                // for each pixel
                for ( int x = 0; x < lineSize; x++, ptr++, ovr++ )
                {
                    if (*ovr>0) {
                        *ptr = (byte) ( ( sourcePercent * ( *ptr ) ) + ( q * ( *ovr ) ) );
                    }
                }
                ptr += offset;
                ovr += ovrOffset;
            }
        }
    }
}
