using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopScrobbler.Controls
{
    // Override some paint issues that are inherent in the CheckedListBox control, by creating our own version
    // that inhterits from the MS one.  As it's only used here, we'll leave it in here.
    public class LastfmCheckedListBox : CheckedListBox
    {
        // Overrides the drawing of a list item
        protected override void OnDrawItem(DrawItemEventArgs ev)
        {
            // If the item being drawn doesn't have a selection state
            if (!ev.State.HasFlag(DrawItemState.Selected))
            {
                // Draw no focus rectangle, and set the background color to the control background colour
                ev = new DrawItemEventArgs(ev.Graphics, ev.Font, ev.Bounds, ev.Index, DrawItemState.NoFocusRect, this.ForeColor, this.BackColor);
            }
            else
            {
                // Draw a focus rectangle, but still set the background color to the control background colour
                ev = new DrawItemEventArgs(ev.Graphics, ev.Font, ev.Bounds, ev.Index, DrawItemState.Focus, this.ForeColor, this.BackColor);
            }

            // Pass our over-riding drawing back to the control
            base.OnDrawItem(ev);
        }
    }
}
