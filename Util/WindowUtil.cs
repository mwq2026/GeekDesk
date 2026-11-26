using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using static GeekDesk.Util.FileIcon;

namespace GeekDesk.Util
{
    public class WindowUtil
    {

        public enum GetWindowCmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [Flags]
        public enum SetWindowPosFlags
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOMOVE = 0x0002,
            SWP_NOZORDER = 0x0004,
            SWP_NOREDRAW = 0x0008,
            SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020,
            SWP_SHOWWINDOW = 0x0040,
            SWP_HIDEWINDOW = 0x0080,
            SWP_NOCOPYBITS = 0x0100,
            SWP_NOOWNERZORDER = 0x0200,
            SWP_NOSENDCHANGING = 0x0400
        }

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(System.Drawing.Point p);

        //取得前台窗口句柄函数 
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        //取得桌面窗口句柄函数 
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string className, string windowName);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string windowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetWindow(HandleRef hWnd, int nCmd);
        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr child, IntPtr parent);
        [DllImport("user32.dll", EntryPoint = "GetDCEx", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetWindowPos(HandleRef hWnd, HandleRef hWndInsertAfter, int x, int y, int cx, int cy, int flags);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr window, IntPtr handle);
        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);   //获取线程ID
        /// <summary>
        /// 枚举窗口时的委托参数
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public delegate bool WndEnumProc(IntPtr hWnd, int lParam);
        /// <summary>
        /// 枚举所有窗口
        /// </summary>
        /// <param name="lpEnumFunc"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(WndEnumProc lpEnumFunc, int lParam);

        /// <summary>
        /// 获取窗口的父窗口句柄
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        private static readonly IntPtr HWND_TOP = IntPtr.Zero;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref LPRECT rect);

        // Import GetDC and ReleaseDC functions from user32.dll
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);


        [StructLayout(LayoutKind.Sequential)]
        public readonly struct LPRECT
        {
            public readonly int Left;
            public readonly int Top;
            public readonly int Right;
            public readonly int Bottom;
        }



        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        public static void DisableMaxWindow(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }

        /// <summary>
        /// 获取 Win32 窗口的一些基本信息。
        /// </summary>
        public struct WindowInfo
        {
            public WindowInfo(IntPtr hWnd, string className, string title, bool isVisible, Rectangle bounds) : this()
            {
                Hwnd = hWnd;
                ClassName = className;
                Title = title;
                IsVisible = isVisible;
                Bounds = bounds;
            }

            /// <summary>
            /// 获取窗口句柄。
            /// </summary>
            public IntPtr Hwnd { get; }

            /// <summary>
            /// 获取窗口类名。
            /// </summary>
            public string ClassName { get; }

            /// <summary>
            /// 获取窗口标题。
            /// </summary>
            public string Title { get; }

            /// <summary>
            /// 获取当前窗口是否可见。
            /// </summary>
            public bool IsVisible { get; }

            /// <summary>
            /// 获取窗口当前的位置和尺寸。
            /// </summary>
            public Rectangle Bounds { get; }

            /// <summary>
            /// 获取窗口当前是否是最小化的。
            /// </summary>
            public bool IsMinimized => Bounds.Left == -32000 && Bounds.Top == -32000;
        }


        /// <summary>
        /// 遍历窗体处理的函数
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="lparam"></param>
        /// <returns></returns>
        private static bool OnWindowEnum(IntPtr hWnd, int lparam)
        {

            // 仅查找顶层窗口。
            //if (GetParent(hWnd) == IntPtr.Zero)
            //{
                // 获取窗口类名。
                var lpString = new StringBuilder(512);
                GetClassName(hWnd, lpString, lpString.Capacity);
                var className = lpString.ToString();

                // 获取窗口标题。
                var lptrString = new StringBuilder(512);
                GetWindowText(hWnd, lptrString, lptrString.Capacity);
                var title = lptrString.ToString().Trim();

                // 获取窗口可见性。
                var isVisible = IsWindowVisible(hWnd);

                // 获取窗口位置和尺寸。
                LPRECT rect = default;
                GetWindowRect(hWnd, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);

                // 添加到已找到的窗口列表。
                windowList.Add(new WindowInfo(hWnd, className, title, isVisible, bounds));
            //}

            return true;
        }
        /// <summary>
        /// 默认的查找窗口的过滤条件。可见 + 非最小化 + 包含窗口标题。
        /// </summary>
        private static readonly Predicate<WindowInfo> DefaultPredicate = x => x.IsVisible && !x.IsMinimized && x.Title.Length > 0;
        /// <summary>
        /// 窗体列表
        /// </summary>
        private static List<WindowInfo> windowList;


        /// <summary>
        /// 查找当前用户空间下所有符合条件的(顶层)窗口。如果不指定条件，将仅查找可见且有标题栏的窗口。
        /// </summary>
        /// <param name="match">过滤窗口的条件。如果设置为 null，将仅查找可见和标题栏不为空的窗口。</param>
        /// <returns>找到的所有窗口信息</returns>
        public static IReadOnlyList<WindowInfo> FindAllWindows(Predicate<WindowInfo> match = null)
        {
            windowList = new List<WindowInfo>();
            //遍历窗口并查找窗口相关WindowInfo信息
            EnumWindows(OnWindowEnum, 0);
            return windowList;
        }



        public static void SetOwner(Window window, Window parentWindow)
        {
            SetOwner(window, new WindowInteropHelper(parentWindow).Handle);
        }

        public static void SetOwner(Window window, IntPtr parentHandle)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.Owner = parentHandle;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static bool WindowIsTop(Window window)
        {
            // 旧实现把“前台窗口标题为空”也当成主窗口在最前。
            // Win11 任务栏 / 托盘溢出层经常没有标题，点击托盘后会被误判，
            // 进而把“显示面板”当成“隐藏面板”。
            return WindowIsForeground(window);
        }

        public static bool WindowIsForeground(Window window)
        {
            if (window == null) return false;
            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero) return false;
            return GetForegroundWindow() == handle;
        }

        public static bool IsWindows11OrLater()
        {
            try
            {
                using (Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key == null) return false;
                    string buildText = key.GetValue("CurrentBuildNumber") as string;
                    if (int.TryParse(buildText, out int build))
                    {
                        return build >= 22000;
                    }
                }
            }
            catch (Exception) { }
            return false;
        }

        public static bool IsDesktopOrShellHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero) return false;
            var className = new StringBuilder(256);
            GetClassName(handle, className, className.Capacity);
            string cls = className.ToString();
            return cls == "Progman" || cls == "WorkerW" || cls == "SHELLDLL_DefView";
        }

        public static bool IsDesktopForeground()
        {
            IntPtr foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero) return false;
            if (IsIgnoredShellWindow(foreground)) return false;

            IntPtr current = foreground;
            for (int i = 0; i < 6 && current != IntPtr.Zero; i++)
            {
                if (IsDesktopOrShellHandle(current)) return true;
                current = GetParent(current);
            }

            var className = new StringBuilder(256);
            GetClassName(foreground, className, className.Capacity);
            string cls = className.ToString();
            if (cls == "XamlExplorerHostIslandWindow" || cls == "Windows.UI.Composition.DesktopWindowContentBridge")
            {
                int pid;
                GetWindowThreadProcessId(foreground, out pid);
                try
                {
                    if (string.Equals(Process.GetProcessById(pid).ProcessName, "explorer", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch (Exception) { }
            }
            return false;
        }

        private static bool IsIgnoredShellWindow(IntPtr handle)
        {
            var className = new StringBuilder(256);
            GetClassName(handle, className, className.Capacity);
            string cls = className.ToString();
            return cls == "Shell_TrayWnd"
                || cls == "Shell_SecondaryTrayWnd"
                || cls == "NotifyIconOverflowWindow"
                || cls == "TopLevelWindowForOverflowXamlIsland"
                || cls == "CabinetWClass"
                || cls == "ExploreWClass";
        }

        private static bool keepAboveDesktopBusy = false;

        /// <summary>
        /// 桌面（WorkerW / Progman）被点到前台时，把可见面板提到桌面之上，但不置顶、不抢激活。
        /// 用来替代 Owner=Progman，避免 Win11 下托盘点击无法前置。
        /// </summary>
        public static void KeepAboveDesktopIfNeeded(Window window)
        {
            if (keepAboveDesktopBusy) return;
            if (window == null) return;
            if (window.Visibility != Visibility.Visible) return;
            if (window.Topmost == true) return;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;
            if (!IsDesktopForeground()) return;

            keepAboveDesktopBusy = true;
            try
            {
                SetWindowPos(new HandleRef(window, hwnd), new HandleRef(null, HWND_TOP), 0, 0, 0, 0,
                    (int)(SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE));
            }
            finally
            {
                keepAboveDesktopBusy = false;
            }
        }

        /// <summary>
        /// Win11 下 Owner 设为桌面窗口会导致 Activate / Show 无法把面板提到前台。
        /// </summary>
        public static void ClearDesktopOwner(Window window)
        {
            if (window == null) return;
            WindowInteropHelper helper = new WindowInteropHelper(window);
            if (IsDesktopOrShellHandle(helper.Owner))
            {
                helper.Owner = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 可靠地显示并激活无任务栏按钮的主面板（应对 Win11 前台限制）。
        /// </summary>
        public static void BringToFront(Window window)
        {
            if (window == null) return;
            ClearDesktopOwner(window);

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Show();
            if (window.Visibility != Visibility.Visible)
            {
                window.Visibility = Visibility.Visible;
            }

            WindowInteropHelper helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.EnsureHandle();
            ShowWindow(hwnd, SW_SHOW);

            IntPtr foreground = GetForegroundWindow();
            if (foreground != hwnd)
            {
                int foregroundPid;
                uint thisTid = GetCurrentThreadId();
                uint foregroundTid = (uint)GetWindowThreadProcessId(foreground, out foregroundPid);
                bool attached = false;
                if (foreground != IntPtr.Zero && foregroundTid != 0 && foregroundTid != thisTid)
                {
                    attached = AttachThreadInput(thisTid, foregroundTid, true);
                }
                try
                {
                    SwitchToThisWindow(hwnd, true);
                    SetForegroundWindow(hwnd);
                }
                finally
                {
                    if (attached)
                    {
                        AttachThreadInput(thisTid, foregroundTid, false);
                    }
                }
            }
            else
            {
                SwitchToThisWindow(hwnd, true);
                SetForegroundWindow(hwnd);
            }

            HandleRef hwndRef = new HandleRef(window, hwnd);
            SetWindowPos(hwndRef, new HandleRef(null, HWND_TOPMOST), 0, 0, 0, 0,
                (int)(SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_SHOWWINDOW));
            if (window.Topmost != true)
            {
                SetWindowPos(hwndRef, new HandleRef(null, HWND_NOTOPMOST), 0, 0, 0, 0,
                    (int)(SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_SHOWWINDOW));
            }

            window.Activate();
            window.Focus();
        }

        private static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }




        public const int GW_CHILD = 5;
   
        public static IntPtr GetDesktopHandle(Window window, DesktopLayer layer)
        {
            HandleRef hWnd;
            IntPtr hDesktop = new IntPtr();
            switch (layer)
            {
                case DesktopLayer.Progman:
                    hDesktop = FindWindow("Progman", null);//第一层桌面  
                    break;
                case DesktopLayer.SHELLDLL:
                    hDesktop = FindWindow("Progman", null);//第一层桌面    
                    hWnd = new HandleRef(window, hDesktop);
                    hDesktop = GetWindow(hWnd, GW_CHILD);//第2层桌面     
                    break;
                case DesktopLayer.FolderView:
                    hDesktop = FindWindow("Progman", null);//第一层桌面    
                    hWnd = new HandleRef(window, hDesktop);
                    hDesktop = GetWindow(hWnd, GW_CHILD);//第2层桌面      
                    hWnd = new HandleRef(window, hDesktop);
                    hDesktop = GetWindow(hWnd, GW_CHILD);//第3层桌面    
                    hWnd = new HandleRef(window, hDesktop);
                    hDesktop = GetWindow(hWnd, GW_CHILD);//第4层桌面    
                    break;
            }
            return hDesktop;
        }

    }

    public enum DesktopLayer
    {
        Progman = 0,
        SHELLDLL = 1,
        FolderView = 2
    }







}
