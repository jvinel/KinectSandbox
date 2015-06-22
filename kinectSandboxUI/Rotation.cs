using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kinectSandboxUI
{
    public class Rotation
    {
        public int Id { get; set; }
        public string Label { get; set; }

        public Rotation(int id, string label) {
            this.Id = id;
            this.Label = label;
        }
        
    }
}
