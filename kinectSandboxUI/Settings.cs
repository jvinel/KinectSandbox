using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinectSandboxUI
{
    public class Settings
    {
        

        [CategoryAttribute("General"),
        DisplayName("Min. Depth"),
        DescriptionAttribute("Minimum depth of data processed")]
        public int MinDepth
        {
            get;
            set;
        }

        [CategoryAttribute("General"),
        DisplayName("Max. Depth"),
        DescriptionAttribute("Maximum depth of data processed")]
        public int MaxDepth
        {
            get;
            set;
        }

        [CategoryAttribute("Filtering"),
        DisplayName("Stabilization"),
        DescriptionAttribute("Activate stabilization filter")]
        public bool ActivateStabilization
        {
            get;
            set;
        }

        [CategoryAttribute("Filtering"),
        DisplayName("Gradient"),
        DescriptionAttribute("Gradient bitmap used to colorize image.")]
        public string Gradient
        {
            get;
            set;
        }

        [CategoryAttribute("Filtering"),
        DisplayName("Isolines"),
        DescriptionAttribute("Isoline step: 0 desactivate, maximum 100")]
        public int isolines
        {
            get;
            set;
        }

    }
}
