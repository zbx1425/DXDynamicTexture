using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Zbx1425.DXDynamicTexture {

    public class TouchEventArgs : EventArgs {
        
        public int X, Y;

        public Color Color;

        public MouseButtons Button;

        public bool PreventDefault = false;

        public TouchEventArgs(int x, int y, MouseButtons button) {
            this.X = x; this.Y = y; this.Button = button;
        }
    }
}
