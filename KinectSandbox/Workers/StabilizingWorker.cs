﻿using KinectSandbox.Filters;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectSandbox.Workers
{
    public class StabilizingWorker : DataFilter
    {

        private DepthImagePixel[] tempData;
        private short MinVariance { get; set; }

        public StabilizingWorker(DataFilterInput dataFilterInput)
            : base(dataFilterInput)
        {
            this.MinVariance=5;
        }

        /// <summary>
        /// Convert a DepthImagePixel array to a bitmap.
        /// Bitmap is colorize if a gradient bitmap has been set, isolines are also added as an overlay
        /// </summary>
        /// <param name="sourceData">DepthImagePixel array (generated by Kinect device depth sensor)</param>
        protected override void Process(DepthImagePixel[] sourceData)
        {
            if ((tempData == null) || (tempData.Length!=sourceData.Length))
            {
                // Initialize buffer
                tempData = new DepthImagePixel[sourceData.Length];
                

            }

            for (int i = 0; i < sourceData.Length; i++)
            {
                short newValue = sourceData[i].Depth;
                short oldValue = tempData[i].Depth;
                    if (isValid(newValue))
                    {
                        // New value is valid
                        if (isValid(oldValue))
                        {

                            // If previous result was also valid, then check that minimun variance is reached
                            if (Math.Abs(newValue - oldValue) > this.MinVariance)
                            {
                                tempData[i].Depth = newValue;
                            } // Otherwise we keep the previous value

                        }
                        else
                        {
                            // New value is valid, but previous result invalid, we keep the new value
                            tempData[i].Depth = newValue;
                        }

                    }
                    
                }

            // Pass bitmap generated to UI (or other filter)
            OnDataReady(new DataReadyEventArgs(tempData));
        }

        private bool isValid(short value)
        {
            if ((value >= this.MinDepth) && (value <= this.MaxDepth))
            {
                return true;
            }
            return false;
        }
    }
}
