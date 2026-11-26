using GeekDesk.Constant;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static GeekDesk.Util.ShowWindowFollowMouse;

namespace GeekDesk.Util
{
    public class ShowWindowFollowMouse
    {

        public enum MousePosition
        {
            CENTER = 1,
            LEFT_TOP = 2,
            LEFT_BOTTOM = 3,
            RIGHT_TOP = 4,
            RIGHT_BOTTOM = 5,
            LEFT_CENTER = 6,
            RIGHT_CENTER = 7
        }

        public static void FollowMouse(Window window)
        {
            // Get the mouse position
            var mousePosition = System.Windows.Forms.Control.MousePosition;

            // Get the window size
            var windowWidth = window.Width;
            var windowHeight = window.Height;

            Console.WriteLine("windowWidth +  windowHeight:" + windowWidth + "+" +  windowHeight);

            // Get the screen where the mouse is located
            var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point(mousePosition.X, mousePosition.Y));
            var workingArea = screen.WorkingArea;

            // Get the DPI scaling factor for the screen
            //float dpiX, dpiY;
            //using (var graphics = System.Drawing.Graphics.FromHwnd(IntPtr.Zero))
            //{
            //    dpiX = graphics.DpiX / 96f; // 96 is the default DPI
            //    dpiY = graphics.DpiY / 96f; // 96 is the default DPI
            //}

            float dpiX = GetDpi( true);
            float dpiY = GetDpi(false);

            // Convert mouse position to logical pixels based on DPI
            double mouseX = mousePosition.X / dpiX;
            double mouseY = mousePosition.Y / dpiY;

            // Calculate target position to center the window on the mouse
            double targetLeft = mouseX - windowWidth / 2;
            double targetTop = mouseY - windowHeight / 2;

            // Ensure the window does not exceed the screen boundaries
            if (targetLeft < workingArea.Left / dpiX)
                targetLeft = workingArea.Left / dpiX;
            if (targetLeft + windowWidth / dpiX > workingArea.Right / dpiX)
                targetLeft = workingArea.Right / dpiX - windowWidth / dpiX;

            if (targetTop < workingArea.Top / dpiY)
                targetTop = workingArea.Top / dpiY;
            if (targetTop + windowHeight / dpiY > workingArea.Bottom / dpiY)
                targetTop = workingArea.Bottom / dpiY - windowHeight / dpiY;

            // Update window position
            window.Left = targetLeft * dpiX;
            window.Top = targetTop * dpiY;
        }

        private static float GetDpi(bool isX)
        {
            IntPtr hdc = WindowUtil.GetDC(IntPtr.Zero);
            int dpi = isX ? WindowUtil.GetDeviceCaps(hdc, LOGPIXELSX) : WindowUtil.GetDeviceCaps(hdc, LOGPIXELSY);
            WindowUtil.ReleaseDC(IntPtr.Zero, hdc);
            return dpi / 96f;
        }

        /// <summary>
        /// 设备像素到 WPF DIP 的系数（physical * scale = DIP）。
        /// </summary>
        private static void GetDipScale(Window window, out double scaleX, out double scaleY)
        {
            scaleX = 0;
            scaleY = 0;
            try
            {
                PresentationSource source = window != null ? PresentationSource.FromVisual(window) : null;
                if (source == null && window != null)
                {
                    IntPtr hwnd = new WindowInteropHelper(window).Handle;
                    if (hwnd != IntPtr.Zero)
                    {
                        source = HwndSource.FromHwnd(hwnd);
                    }
                }
                if (source != null && source.CompositionTarget != null)
                {
                    Matrix matrix = source.CompositionTarget.TransformFromDevice;
                    scaleX = matrix.M11;
                    scaleY = matrix.M22;
                }
            }
            catch (Exception) { }

            if (scaleX <= 0 || scaleY <= 0)
            {
                float dpiX = GetDpi(true);
                float dpiY = GetDpi(false);
                scaleX = dpiX > 0 ? 1.0 / dpiX : 1.0;
                scaleY = dpiY > 0 ? 1.0 / dpiY : 1.0;
            }
        }

        private static IntPtr GetScreenHandleFromMouse()
        {
            // Get the mouse position
            var mousePosition = System.Windows.Forms.Control.MousePosition;

            // Convert mouse position to a POINT structure
            System.Drawing.Point point = new System.Drawing.Point(mousePosition.X, mousePosition.Y);

            // Get the window handle from the point
            IntPtr windowHandle = WindowUtil.WindowFromPoint(point);

            return windowHandle;
        }

        // Constants for DPI
        private const int HORZRES = 8;
        private const int VERTRES = 10;
        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

       

        /// <summary>
        /// 随鼠标位置显示面板  后面三个参数暂时没有使用
        /// </summary>
        public static void Show(Window window, MousePosition position, double widthOffset = 0, double heightOffset = 0)
        {
            if (window == null) return;

            // Cursor.Position / Screen.WorkingArea 是设备像素；WPF Left/Top 是 DIP。
            // 用窗口的 TransformFromDevice（否则退回系统 DPI）做同一套换算，避免截断和主副屏缩放不一致。
            System.Drawing.Point cursor = System.Windows.Forms.Cursor.Position;
            Screen currentScreen = Screen.FromPoint(cursor);
            GetDipScale(window, out double scaleX, out double scaleY);

            double mouseX = cursor.X * scaleX;
            double mouseY = cursor.Y * scaleY;

            System.Drawing.Rectangle workingArea = currentScreen.WorkingArea;
            double screenLeft = workingArea.Left * scaleX;
            double screenTop = workingArea.Top * scaleY;
            double screenRight = workingArea.Right * scaleX;
            double screenBottom = workingArea.Bottom * scaleY;

            double width = window.ActualWidth > 0 ? window.ActualWidth : window.Width;
            double height = window.ActualHeight > 0 ? window.ActualHeight : window.Height;
            if (double.IsNaN(width) || width <= 0) width = window.MinWidth;
            if (double.IsNaN(height) || height <= 0) height = window.MinHeight;

            double left = mouseX - width / 2 + Constants.SHADOW_WIDTH;
            double top = mouseY - height / 2 + Constants.SHADOW_WIDTH;

            if (left < screenLeft - Constants.SHADOW_WIDTH)
            {
                left = screenLeft - Constants.SHADOW_WIDTH;
            }
            if (left + width > screenRight + Constants.SHADOW_WIDTH)
            {
                left = screenRight - width + Constants.SHADOW_WIDTH;
            }

            if (top < screenTop - Constants.SHADOW_WIDTH)
            {
                top = screenTop - Constants.SHADOW_WIDTH;
            }
            if (top + height > screenBottom + Constants.SHADOW_WIDTH)
            {
                top = screenBottom - height + Constants.SHADOW_WIDTH;
            }

            window.Left = left;
            window.Top = top;
            





            //// 显示屏幕信息
            //Console.WriteLine("鼠标当前所在屏幕的信息:");
            //Console.WriteLine($"屏幕设备名称: {currentScreen.DeviceName}");
            //Console.WriteLine($"屏幕分辨率: {currentScreen.Bounds.Width}x{currentScreen.Bounds.Height}");
            //Console.WriteLine($"屏幕工作区: {currentScreen.WorkingArea}");
            //Console.WriteLine($"主屏幕: {currentScreen.Primary}");

            //Console.WriteLine(p.X + "=" + p.Y);
            //float dpiX = GetDpi(true);
            //float dpiY = GetDpi(false);
            //p.X = (int)(p.X / dpiX);
            //p.Y = (int)(p.Y / dpiY);
            //Console.WriteLine(p.X + "=" + p.Y);
            //double left = SystemParameters.VirtualScreenLeft;
            //double top = SystemParameters.VirtualScreenTop;
            //double width = SystemParameters.VirtualScreenWidth;
            //double height = SystemParameters.WorkArea.Bottom;

            //Console.WriteLine("VirtualScreenTop:" + SystemParameters.VirtualScreenTop);
            //Console.WriteLine("VirtualScreenLeft:" + SystemParameters.VirtualScreenLeft);
            //Console.WriteLine("VirtualScreenWidth:" + SystemParameters.VirtualScreenWidth);
            //Console.WriteLine("VirtualScreenHeight:" + SystemParameters.VirtualScreenHeight);
            //Console.WriteLine("WorkArea.Bottom:" + SystemParameters.WorkArea.Bottom);
            //Console.WriteLine(" window.Height:" + window.Height);
            //double right = width - Math.Abs(left);
            //double bottom = height - Math.Abs(top);

            //double afterWidth;
            //double afterHeight;

            //switch (position)
            //{

            //    case MousePosition.LEFT_BOTTOM:
            //        afterWidth = 0;
            //        afterHeight = window.Height;
            //        break;
            //    case MousePosition.LEFT_TOP:
            //        afterWidth = 0;
            //        afterHeight = 0;
            //        break;
            //    case MousePosition.LEFT_CENTER:
            //        afterWidth = 0;
            //        afterHeight = window.Height / 2;
            //        break;
            //    case MousePosition.RIGHT_BOTTOM:
            //        afterWidth = window.Width;
            //        afterHeight = window.Height;
            //        break;
            //    case MousePosition.RIGHT_TOP:
            //        afterWidth = window.Width;
            //        afterHeight = 0;
            //        break;
            //    case MousePosition.RIGHT_CENTER:
            //        afterWidth = window.Width;
            //        afterHeight = window.Height / 2;
            //        break;
            //    default:
            //        afterWidth = window.Width / 2;
            //        afterHeight = window.Height / 2;
            //        break;
            //}

            //afterWidth += widthOffset;
            //afterHeight -= heightOffset;

            //if (p.X - afterWidth < left)
            //{
            //    //判断是否在最左边缘
            //    window.Left = left - Constants.SHADOW_WIDTH;
            //}
            //else if (p.X + afterWidth > right)
            //{
            //    //判断是否在最右边缘
            //    window.Left = right - window.Width + Constants.SHADOW_WIDTH;
            //}
            //else
            //{
            //    window.Left = p.X - afterWidth;
            //}


            //if (p.Y - afterHeight < top)
            //{
            //    //判断是否在最上边缘
            //    window.Top = top - Constants.SHADOW_WIDTH;
            //}
            //else if (p.Y + afterHeight > bottom)
            //{
            //    Console.WriteLine("p.Y:" + p.Y);
            //    Console.WriteLine("afterHeight:" + afterHeight);
            //    Console.WriteLine("bottom:" + bottom);
            //    //判断是否在最下边缘
            //    window.Top = bottom - window.Height + Constants.SHADOW_WIDTH;
            //}
            //else
            //{
            //    window.Top = p.Y - afterHeight;
            //}
        }

    }
}
