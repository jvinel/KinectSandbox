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
using System.Linq;
using System.Text;
using System.Threading;

namespace KinectSandboxLib.Filters
{
    public abstract class DataFilter : DataFilterInput
    {
        private bool requestStop = false;

        public AutoResetEvent autoResetEvent;

        protected DataFilterInput dataFilterInput;


        protected bool processRunning = false;

        private DepthImagePixel[] inputData;

        public DataFilter(DataFilterInput dataFilterInput)
        {
            autoResetEvent = new AutoResetEvent(false);
            this.dataFilterInput = dataFilterInput;
            this.dataFilterInput.DataReady += new EventHandler<DataReadyEventArgs>(dataFilterInput_DataReady);
        }

        public void Start()
        {
            this.Log().Debug("Filter started with thread " + Thread.CurrentThread.ManagedThreadId);
            while (!this.requestStop)
            {
#if TRACE
                this.Log().Debug("Waiting for next event.");
#endif
                autoResetEvent.WaitOne();
#if TRACE
                this.Log().Debug("New data received from previous filter.");
#endif
                
                if ((!this.requestStop) && (!processRunning))
                {
#if TRACE
                    this.Log().Debug("Launch process on new data.");
#endif

                    this.processRunning = true;
                    this.Process(inputData);
                }


            }
            this.Log().Debug("Filter ended with thread " + Thread.CurrentThread.ManagedThreadId);

        }

        protected abstract void Process(DepthImagePixel[] sourceData);

        protected delegate void ProcessDelegate(DepthImagePixel[] sourceData);

        private void dataFilterInput_DataReady(object sender, DataReadyEventArgs e)
        {
            if (e.ShouldStop) {
                this.requestStop = true;
                OnDataReady(e);
            }
            
            if (!this.processRunning)
            {
                this.inputData = e.Data;
                this.autoResetEvent.Set();
            }
        }

        protected override void OnDataReady(DataReadyEventArgs e)
        {

            base.OnDataReady(e);
            this.processRunning = false;
        }

        protected override void OnOutputDataReady(BitmapReadyEventArgs e)
        {
            base.OnOutputDataReady(e);
            this.processRunning = false;
        }
    }
}
