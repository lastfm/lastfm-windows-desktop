using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transitions;

namespace LastFM.Common.Classes
{
    /// <summary>
    /// The application's Popup Notifications
    /// </summary>
    public partial class PopupNotificationUi : Form
    {
        // An instance of the (single) transition used to display the notificatin
        private Transition _showingTransition = null;

        //An instance of the (single) transitions used to hide the notification
        private Transition _hidingTransition = null;

        // Default constructor for the Popup Notification (useful for design purposes!)
        public PopupNotificationUi()
        {
            InitializeComponent();
        }

        // Overriding constructor for the Popup Notification, that sets the title and body text
        public PopupNotificationUi(string title, string text)
        {
            InitializeComponent();

            lblTitle.Text = title;
            lblText.Text = text;
        }
    }
}
