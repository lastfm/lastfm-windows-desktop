using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LastFM.Common.Helpers
{
    public static class FormHelper
    {
        public static Point GetRelativeLocation(Form form, Control control)
        {
            Point controlLoc = form.PointToScreen(control.Location);
            Point relativeLoc = new Point(controlLoc.X - form.Location.X, controlLoc.Y - form.Location.Y);

            return relativeLoc;
        }

    }
}
