using kinectSandboxUI;
using KinectSandboxUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace KinectSandboxUI
{
    public class Settings
    {
       
        [Category(" General"),
        DisplayName("Min. Depth"),
        DescriptionAttribute("Minimum depth of data processed")]
        public uint MinDepth
        {
            get;
            set;
        }

        [Category(" General"),
        DisplayName("Max. Depth"),
        DescriptionAttribute("Maximum depth of data processed")]
        public uint MaxDepth
        {
            get;
            set;
        }

        [Category("Filtering"),
        DisplayName("Stabilization"),
        DescriptionAttribute("Activate stabilization filter")]
        public bool ActivateStabilization
        {
            get;
            set;
        }

        [Category("Filtering"),
        DisplayName("Gradient"),
        DescriptionAttribute("Gradient bitmap used to colorize image.")]
        public string Gradient
        {
            get;
            set;
        }

        [Category("Filtering"),
        DisplayName("Isolines"),
        DescriptionAttribute("Isoline step: 0 desactivate, maximum 100")]
        public uint Isolines
        {
            get;
            set;
        }

        [Category("Output"),
        DisplayName("Rotation"),
        DescriptionAttribute("Rotate image generated for output display")]
        [ItemsSource(typeof(RotationItemsSource))]
        public int Rotation
        {
            get;
            set;
        }

        [Category("Output"),
        DisplayName("Margin"),
        DescriptionAttribute("Margin of image generated for output display")]
        public Thickness  Margin
        {
            get;
            set;
        }

    }
}
