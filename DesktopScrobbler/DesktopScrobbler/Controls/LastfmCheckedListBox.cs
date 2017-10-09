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
        private bool _skipIndexTracking = false;

        private int _lastSelectedIndex = -1;

        public LastfmCheckedListBox()
        {
            this.SelectedIndexChanged += (o, ev) =>
            {
                if (!_skipIndexTracking)
                {
                    _lastSelectedIndex = this.SelectedIndex;
                }
            };
        }

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

        protected override void OnGotFocus(EventArgs e)
        {

            if (_lastSelectedIndex == -1 && this.Items.Count > 0)
            {
                _lastSelectedIndex = 0;
            }

            this.SelectedIndex = _lastSelectedIndex;

            this.RefreshItem(this.SelectedIndex);

            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this._skipIndexTracking = true;
            this.SelectedIndex = -1;
            this._skipIndexTracking = false;
        }
    }
}
