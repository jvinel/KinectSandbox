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
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace KinectSandboxLib.Filters
{
    /// <summary>
    /// Event raised when a data filter has produce some output
    /// </summary>
    public class DataReadyEventArgs: EventArgs
    {
        /// <summary>
        /// Data generated by filter
        /// </summary>
        public DepthImagePixel[] Data { get; set; }

        public bool ShouldStop { get; set; }

        /// <summary>
        /// Arguments to event: data produced by filter
        /// </summary>
        /// <param name="data"></param>
        public DataReadyEventArgs(DepthImagePixel[] data, bool shouldStop=false)
        {
            this.Data = data;
            this.ShouldStop = shouldStop;
        }

    }
}
