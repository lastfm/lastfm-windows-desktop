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
    public partial class PopupNotificationUi : Form
    {
        private Transition _showingTransition = null;
        private Transition _hidingTransition = null;

        public PopupNotificationUi()
        {
            InitializeComponent();
            this.FormClosing += OnFormClosing;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs formClosingEventArgs)
        {
            
        }

        public PopupNotificationUi(string title, string text)
        {
            InitializeComponent();

            lblTitle.Text = title;
            lblText.Text = text;
        }
    }
}
