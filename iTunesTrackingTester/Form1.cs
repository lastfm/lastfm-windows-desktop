using ManagedWinapi.Hooks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace iTunesTrackingTester
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwndControl, uint Msg, int wParam, StringBuilder strBuffer); // get text

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwndControl, uint Msg, int wParam, int lParam);  // text length

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Auto)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [DllImport("user32.dll")]
        static extern bool DrawMenuBar(IntPtr hWnd);

        private const int MF_BYCOMMAND = 0x0;
        private const int MF_BYPOSITION = 0x400;
        private const int MF_REMOVE = 0x1000;
        private const int MF_ENABLED = 0x0;
        private const int MF_GRAYED = 0x1;
        private const int MF_DISABLED = 0x2;
        private const int SC_CLOSE = 0xF060;


        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

        private static List<IntPtr> GetAllChildrenWindowHandles(IntPtr hParent, int maxCount)
        {
            List<IntPtr> result = new List<IntPtr>();

            int ct = 0;
            IntPtr prevChild = IntPtr.Zero;
            IntPtr currChild = IntPtr.Zero;

            while (true && ct < maxCount)
            {
                currChild = FindWindowEx(hParent, prevChild, null, null);
                if (currChild == IntPtr.Zero) break;
                result.Add(currChild);
                prevChild = currChild;
                ++ct;
            }

            return result;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private IntPtr _mainWindowHandle = IntPtr.Zero;

        WindowInfo _windowInfo = new WindowInfo();

        private void button1_Click(object sender, EventArgs e)
        {
            Process[] iTunesProcesses = Process.GetProcessesByName("iTunes");

            if (iTunesProcesses.Length > 0)
            {
                Process firstProcess = iTunesProcesses.ToList().First();

                var mainWindowHandle = firstProcess.MainWindowHandle;
                IntPtr _applicationHandle = IntPtr.Zero;

                if (mainWindowHandle != IntPtr.Zero)
                {
                    _mainWindowHandle = mainWindowHandle;

                    IntPtr sysMenuPtr = GetSystemMenu(_mainWindowHandle, false);

                    EnableMenuItem(sysMenuPtr, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                    DrawMenuBar(sysMenuPtr);

                    List<WindowInfo> childInfo = OutputChildInfo(_mainWindowHandle);

                    lstHandles.ValueMember = "WindowHandle";
                    lstHandles.DisplayMember = "DisplayMember";

                    lstHandles.DataSource = childInfo;

                    // Inject a button into the main application window
                    //Win32Button button = new Win32Button();

                    //Win32Window window = Win32Window.FromProcessName("iTunes");

                    //button.Width = 2;
                    //button.Pos_X = window.Width - 2;
                    //button.Pos_Y = 2;
                    //button.Text = "X";
                    //window.AddControl(button);

                }
            }
            else
            {
                MessageBox.Show("No iTunes Process is running!");
            }
        }

        private List<WindowInfo> OutputChildInfo(IntPtr windowHandle)
        {
            List<IntPtr> childHandles = GetAllChildrenWindowHandles(windowHandle, 99);

            List<WindowInfo> info = new List<WindowInfo>();

            for (int i = 0; i < childHandles.Count; ++i)
            {
                WindowInfo ci = new WindowInfo(); // child window info
                ci.handle = childHandles[i];
                ci.caption = GetCaptionText(childHandles[i]);
                ci.parent = windowHandle;

                info.Add(ci);

                Console.WriteLine($"Handle: {ci.handle.ToInt32()}, {ci.caption}");
            }

            foreach(IntPtr handle in childHandles)
            {
                info.AddRange(OutputChildInfo(handle));

                try
                {
                    Control controlItem = Control.FromHandle(handle);
                }
                catch (Exception)
                {
                }
            }

            return info;
        }

        #region IterationCode

        static int GetCaptionTextLength(IntPtr hTextBox)
        {
            // helper for GetCaptionText
            uint WM_GETTEXTLENGTH = 0x000E;
            int result = SendMessage(hTextBox, WM_GETTEXTLENGTH,
              0, 0);
            return result;
        }

        static string GetCaptionText(IntPtr hTextBox)
        {
            uint WM_GETTEXT = 0x000D;
            int len = GetCaptionTextLength(hTextBox);
            if (len <= 0) return null;  // no text. consider empty string instead.
            StringBuilder sb = new StringBuilder(len + 1);
            SendMessage(hTextBox, WM_GETTEXT, len + 1, sb);
            return sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (lstHandles.SelectedItem != null)
            {
                List<WindowInfo> sourceData = lstHandles.DataSource as List<WindowInfo>;

                IntPtr selectedPointer = sourceData[lstHandles.SelectedIndex].handle;

                ClickOnPoint(selectedPointer, new Point());

            }
        }

        public static void ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            var oldPos = Cursor.Position;

            /// get screen coordinates
            ClientToScreen(wndHandle, ref clientPoint);

            /// set cursor on coords, and press mouse
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.Type = 0; /// input type mouse
            inputMouseDown.Data.Mouse.Flags = 0x0002; /// left button down

            var inputMouseUp = new INPUT();
            inputMouseUp.Type = 0; /// input type mouse
            inputMouseUp.Data.Mouse.Flags = 0x0004; /// left button up

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            /// return mouse 
            Cursor.Position = oldPos;
        }
    }

    public class WindowInfo
    {
        public IntPtr handle;
        public int level; // 0 = desktop, 1 = all top-level windows, etc.
        public string caption; // several things such as app title or textbox text
        public IntPtr parent;
        public Int32 WindowHandle
        {
            get { return handle.ToInt32(); }
        }

        public string DisplayMember
        {
            get { return this.ToString(); }
        }

        public override string ToString()
        {
            string captionDisplay;
            if (caption == null) captionDisplay = "no caption";
            else captionDisplay = caption.ToString();

            return "handle = " + handle.ToString("X").PadLeft(8, '0') +
              " caption = " + captionDisplay.ToString().PadRight(40) +
              " parent = " + parent.ToString("X").PadLeft(8, '0');
        }
    }

    #endregion
}
