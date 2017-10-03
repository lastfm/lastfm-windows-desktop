using LastFM.Common.Classes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transitions;

namespace LastFM.Common.Helpers
{

    public static class NotificationHelper
    {
        // SetWindowPos()
        internal const Int32 HWND_TOPMOST = -1;
        internal const Int32 SWP_NOACTIVATE = 0x0010;

        // ShowWindow()
        internal const Int32 SW_SHOWNOACTIVATE = 4;

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, Int32 flags);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, Int32 hWndInsertAfter, Int32 X, Int32 Y, Int32 cx, Int32 cy, uint uFlags);


        private static List<PopupNotificationUi> _notifications = new List<PopupNotificationUi>();

        public static void ShowNotification(Form owner, string title, string notificationText)
        {
            PopupNotificationUi notificationWindow = new PopupNotificationUi(title, notificationText);

            owner.Invoke(new MethodInvoker(() => { 
                var workingArea = Screen.GetWorkingArea(owner);

                // Get the total height of all the windows displayed, to find out where the bottom point point
                // for the new window will be
                var totalDisplayWindowHeight = (notificationWindow.Height * _notifications.Count) + (5 * _notifications.Count);

                var newWindowTopPoint = (workingArea.Height - totalDisplayWindowHeight) - (notificationWindow.Height + 5);
                var newWindowLeftpoint = workingArea.Width;

                // Display the window with no  focus
                ShowWindow(notificationWindow.Handle, SW_SHOWNOACTIVATE);

                // Move it to the bottom right
                SetWindowPos(notificationWindow.Handle, HWND_TOPMOST, newWindowLeftpoint, newWindowTopPoint, notificationWindow.Width, notificationWindow.Height, SWP_NOACTIVATE);

                // Add this to the queue
                _notifications.Add(notificationWindow);

                Transition showingAnimation = new Transition(new TransitionType_Acceleration(1000));
                showingAnimation.Tag = notificationWindow;

                showingAnimation.TransitionCompletedEvent += async (o, ev) =>
                {
                    Transition completedTransition = o as Transition;
                    if (completedTransition != null)
                    {
                        Notification_OnShownComplete(completedTransition.Tag as PopupNotificationUi);
                    }
                };

                showingAnimation.add(notificationWindow, "Left", newWindowLeftpoint - (notificationWindow.Width + 11));
                showingAnimation.run();
            }));
        }

        internal static async void Notification_OnShownComplete(PopupNotificationUi notificationWindow)
        {
            await Task.Delay(3000);

            Transition hidingTransition = new Transition(new TransitionType_Acceleration(1000));
            hidingTransition.Tag = notificationWindow;
            hidingTransition.add(notificationWindow, "Left", notificationWindow.Left + (notificationWindow.Width + 11));

            hidingTransition.TransitionCompletedEvent += async (o, ev) => 
            {
                Transition completedTransition = o as Transition;
                if (completedTransition != null)
                {
                    Notification_OnHideComplete(completedTransition.Tag as PopupNotificationUi);
                }
            };
            hidingTransition.run();
        }

        internal static void Notification_OnHideComplete(PopupNotificationUi sender)
        {
            _notifications.Remove(sender);
        }

    }
}
