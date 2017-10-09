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
        // Whether or not to listen to the selected item changed event
        private bool _skipIndexTracking = false;

        // Which item in the list was the last item selected
        private int _lastSelectedIndex = -1;

        // The constructor for our control
        public LastfmCheckedListBox()
        {
            // Binding to the event that deals with items being selected (no, not checked)
            this.SelectedIndexChanged += (o, ev) =>
            {
                // If we're meant to be listening to this event
                if (!_skipIndexTracking)
                {
                    // Store which item index was just selected
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

        // Overrides the GotFocus event to allow us to manipulate the control into drawing things 
        // according to expected behaviour
        protected override void OnGotFocus(EventArgs e)
        {
            // If we haven't yet set a tracked index, and we've got items in the list
            if (_lastSelectedIndex == -1 && this.Items.Count > 0)
            {
                // Automagically mark the first item as the last selected
                _lastSelectedIndex = 0;
            }

            // Select the last selected item
            this.SelectedIndex = _lastSelectedIndex;

            // Force it to be re-drawn, so that we get the focus rectangle
            this.RefreshItem(this.SelectedIndex);

            // Let the control do whatever else it needs to do
            base.OnGotFocus(e);
        }

        // Overrides the GotFocus event to allow us to manipulate the control into drawing things 
        // according to expected behaviour
        protected override void OnLostFocus(EventArgs e)
        {
            // Stop tracking the index changes
            this._skipIndexTracking = true;

            // De-select whatever is selected
            this.SelectedIndex = -1;

            // Re-bind the selection change tracking
            this._skipIndexTracking = false;
        }
    }
}
