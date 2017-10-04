using LastFM.Common.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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

        internal const Int32 SM_CXVIRTUALSCREEN = 78;
        internal const Int32 SM_YVIRTUALSCREEN = 79;

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, Int32 flags);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, Int32 hWndInsertAfter, Int32 X, Int32 Y, Int32 cx, Int32 cy, uint uFlags);

        [DllImport("user32.dll")]
        internal static extern int GetSystemMetrics(Int32 smIndex);

        private static List<PopupNotificationUi> _notifications = new List<PopupNotificationUi>();
        private static List<Transition> _runningTransitions = new List<Transition>();

        public static void ShowNotification(Form owner, string title, string notificationText)
        {
            // The might be useful at some point for DPI awareness
            //int virtualScreenWidth = GetSystemMetrics(SM_CXVIRTUALSCREEN);
            //int virtualScreenHeight = GetSystemMetrics(SM_YVIRTUALSCREEN);
            if (!Core.ApplicationIsShuttingDown)
            {
                PopupNotificationUi notificationWindow = new PopupNotificationUi(title, notificationText);

                owner.Invoke(new MethodInvoker(() =>
                {
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
                            _runningTransitions.Remove(completedTransition);
                            Notification_OnShownComplete(completedTransition.Tag as PopupNotificationUi);
                        }
                    };

                    _runningTransitions.Add(showingAnimation);

                    showingAnimation.add(notificationWindow, "Left", newWindowLeftpoint - (notificationWindow.Width + 11));
                    showingAnimation.run();
                }));
            }
        }

        internal static async void Notification_OnShownComplete(PopupNotificationUi notificationWindow)
        {
            if (!Core.ApplicationIsShuttingDown)
            {
                await Task.Delay(3000);

                Transition hidingTransition = new Transition(new TransitionType_Acceleration(1000));

                hidingTransition.Tag = notificationWindow;
                hidingTransition.add(notificationWindow, "Left", notificationWindow.Left + (notificationWindow.Width + 11));
                _runningTransitions.Add(hidingTransition);

                hidingTransition.TransitionCompletedEvent += async (o, ev) =>
                {
                    Transition completedTransition = o as Transition;
                    if (completedTransition != null)
                    {
                        Notification_OnHideComplete(completedTransition.Tag as PopupNotificationUi);

                        _runningTransitions.Remove(completedTransition);
                    }
                };
                hidingTransition.run();
            }
        }

        internal static void Notification_OnHideComplete(PopupNotificationUi sender)
        {
            if (!Core.ApplicationIsShuttingDown)
            {
                if (!sender.IsDisposed && !sender.Disposing)
                {
                    sender.Invoke(new MethodInvoker(sender.Close));
                }
                _notifications.Remove(sender);
            }
        }

        internal static void ClearNotifications()
        {
            // Stop all running transitions
            foreach (Transition runningTransition in _runningTransitions)
            {
                runningTransition.Stop();
            }
            _runningTransitions.Clear();

            List<PopupNotificationUi> loadedNotifications = _runningTransitions.Select(item => item.Tag as PopupNotificationUi).ToList();

            foreach (PopupNotificationUi notificationWindow in loadedNotifications)
            {
                if (notificationWindow != null && !notificationWindow.IsDisposed && !notificationWindow.Disposing)
                {
                    notificationWindow.Invoke(new MethodInvoker(notificationWindow.Close));
                }
            }
        }
    }
}
