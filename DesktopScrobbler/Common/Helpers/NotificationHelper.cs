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
    // General helper for displaying notifications
    public static class NotificationHelper
    {
        // Win32 API constants for SetWindowPos()
        internal const Int32 HWND_TOPMOST = -1;
        internal const Int32 SWP_NOACTIVATE = 0x0010;

        // Win32 API contants for ShowWindow()
        internal const Int32 SW_SHOWNOACTIVATE = 4;

        // Win32 API declaration for displaying a window
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, Int32 flags);

        // Win32 API for moving a Window
        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, Int32 hWndInsertAfter, Int32 X, Int32 Y, Int32 cx, Int32 cy, uint uFlags);

        // Internal list of popup notification windows
        private static List<PopupNotificationUi> _notifications = new List<PopupNotificationUi>();

        // Internal list of running transition animations
        private static List<Transition> _runningTransitions = new List<Transition>();

        // Method to display a notification, with the given title and body text
        public static void ShowNotification(Form owner, string title, string notificationText)
        {
            // Only run transitions and display popups if the application isn't shutting down
            if (!Core.ApplicationIsShuttingDown)
            {
                // Create a new popup window
                PopupNotificationUi notificationWindow = new PopupNotificationUi(title, notificationText);

                // Warning: this could be potentially stealing focus....
                notificationWindow.TopMost = true;

                owner.BeginInvoke(new MethodInvoker(() =>
                {
                    DisplayNotification(owner, notificationWindow);
                }));
            }
        }

        public static void ShowNotificationSynch(Form owner, string title, string notificationText)
        {
            // Only run transitions and display popups if the application isn't shutting down
            if (!Core.ApplicationIsShuttingDown)
            {
                // Create a new popup window
                PopupNotificationUi notificationWindow = new PopupNotificationUi(title, notificationText);

                // Warning: this could be potentially stealing focus....
                notificationWindow.TopMost = true;

                owner.Invoke(new MethodInvoker(() =>
                {
                    DisplayNotification(owner, notificationWindow);
                }));
            }
        }

        private static void DisplayNotification(Form owner, PopupNotificationUi notificationWindow)
        {
            // Determine what the screen size is of the screen where the specified form is currently located
            var workingArea = Screen.GetWorkingArea(owner);

            // Get the total height of all the open notifications displayed to find out where the bottom point point
            // for the new window will be
            var totalDisplayWindowHeight = (notificationWindow.Height * _notifications.Count) + (5 * _notifications.Count);

            // Determine the 'Y' co-ordinate of the new notification
            var newWindowTopPoint = ((workingArea.Top + workingArea.Height) - totalDisplayWindowHeight) - (notificationWindow.Height + 5);

            // Deteremine the 'X' co-ordinate of the new notification
            var newWindowLeftpoint = (workingArea.Left + workingArea.Width) - (notificationWindow.Width + 11); //workingArea.Width;

            // Determine how wide (in case of DPI changes) the notification should be
            // (note this may be a redundant step)
            var widthToSet = notificationWindow.Width;

            // By default, make the notification window 'zero' size as we're using a 'expanding window' transition
            //notificationWindow.Width = 0;
            notificationWindow.Opacity = 0.0;

            // Move it to the bottom right
            SetWindowPos(notificationWindow.Handle, HWND_TOPMOST, newWindowLeftpoint, newWindowTopPoint, notificationWindow.Width, notificationWindow.Height, SWP_NOACTIVATE);

            // Display the notification with no focus
            ShowWindow(notificationWindow.Handle, SW_SHOWNOACTIVATE);

            // Add this notification to the displayed notifications stack
            _notifications.Add(notificationWindow);

            // Start a new 'display the window' transition
            Transition showingAnimation = new Transition(new TransitionType_Deceleration(500));
            showingAnimation.Tag = notificationWindow;

            // Define what to do when the transition has completed
            showingAnimation.TransitionCompletedEvent += async (o, ev) =>
            {
                Transition completedTransition = o as Transition;
                if (completedTransition != null)
                {
                    // Remove the transition from the queue as it has completed
                    _runningTransitions.Remove(completedTransition);

                    // Raise the delegate event for successful completion of the 'displaying' transition
                    Notification_OnShownComplete(completedTransition.Tag as PopupNotificationUi);
                }
            };

            // Add the transition to the transitions queue (so that if we close the application we can stop all the transitions from running)
            _runningTransitions.Add(showingAnimation);

            // This was in place when the transition used to be 'Slide in the notification from the bottom right of the current window'
            //showingAnimation.add(notificationWindow, "Left", newWindowLeftpoint - (notificationWindow.Width + 11));

            // Where the notification window is displayed, change the window size incremently until it's reached the maximum size
            showingAnimation.add(notificationWindow, "Opacity", 1.0);

            // Run the 'Display' transition
            showingAnimation.run();
        }

        // Delegate function used when a popup notification has successfully completed displaying
        internal static async Task Notification_OnShownComplete(PopupNotificationUi notificationWindow)
        {
            // Only run the transition if the application isn't closing
            if (!Core.ApplicationIsShuttingDown)
            {
                // Wait for 3 seconds so the user has time to see the notification
                await Task.Delay(4000).ConfigureAwait(false);

                // Create a new transition for hiding the notification
                Transition hidingTransition = new Transition(new TransitionType_Acceleration(500));

                // Keep a track of the form instance associated with this notification
                hidingTransition.Tag = notificationWindow;

                // This was relevant when the transition used to slide in from right to left
                //hidingTransition.add(notificationWindow, "Left", notificationWindow.Left + (notificationWindow.Width + 11));

                // Where the notification is displayed incrementally reduce the width of the window until it's no longer visible
                hidingTransition.add(notificationWindow, "Opacity", 0.0);

                // Add the transition to the queue (in case we need to stop it running because the user closes the application)
                _runningTransitions.Add(hidingTransition);

                // Define what to do when the transition has completed
                hidingTransition.TransitionCompletedEvent += async (o, ev) =>
                {
                    Transition completedTransition = o as Transition;
                    if (completedTransition != null)
                    {
                        // Raise the delegate event for successful completion of the 'hiding' transition
                        Notification_OnHideComplete(completedTransition.Tag as PopupNotificationUi);

                        // Remove the transition from the queue as it has completed
                        _runningTransitions.Remove(completedTransition);
                    }
                };

                // Run the transition
                hidingTransition.run();
            }
        }

        // The delegate event for when a notification window has been 'hidden' after having been displayed
        internal static void Notification_OnHideComplete(PopupNotificationUi sender)
        {
            // Only perform this clean up operation if the application is not closing
            // (the action of closing an application cleans up all windows in the same manner)
            if (!Core.ApplicationIsShuttingDown)
            {
                // There could be the potential for a split-fraction-of-a-second-timing event
                // to occur where the previous step validates, but the form has subsequently been
                // destroyed by the user quitting the application, so check the state of the form
                if (!sender.IsDisposed && !sender.Disposing)
                {
                    // Perform a cross-thread safe closure of the form
                    sender.BeginInvoke(new MethodInvoker(sender.Close));
                }

                // Remove the form from the notifications queue
                _notifications.Remove(sender);
            }
        }

        // Method used when the user is quitting the application, to close all notifications and clean up the associated queues
        internal static void ClearNotifications()
        {
            // Stop all running transitions
            foreach (Transition runningTransition in _runningTransitions)
            {
                runningTransition.Stop();
            }

            // Get all of the windows associated with the running transitions
            List<PopupNotificationUi> loadedNotifications = _runningTransitions.Select(item => item.Tag as PopupNotificationUi).ToList();

            // Iterate each popup notification
            foreach (PopupNotificationUi notificationWindow in loadedNotifications)
            {
                // Double check that it's own transition completed event hasn't already destroyed the window instance
                if (notificationWindow != null && !notificationWindow.IsDisposed && !notificationWindow.Disposing)
                {
                    // Perform a thread-safe closure of the window
                    notificationWindow.BeginInvoke(new MethodInvoker(notificationWindow.Close));
                }
            }

            // Release the queue
            _runningTransitions.Clear();

        }
    }
}
