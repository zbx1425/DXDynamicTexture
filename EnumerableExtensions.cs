using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Zbx1425.DXDynamicTexture {

    public static class EnumerableExtensions {

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> src) {
            return src
                .Select(obj => {
                    if (obj is IEnumerable<T> arrayObj) {
                        return arrayObj.Flatten();
                    } else {
                        return new T[1] { obj };
                    }
                })
                .SelectMany(x => x);
        }
    }
}
