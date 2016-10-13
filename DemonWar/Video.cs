using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace WjeWar
{
    class Video
    {

        /// <summary>亮度调节
        /// 
        /// </summary>
        /// <param name="hDC"></param>
        /// <param name="lpRamp"></param>
        /// <returns></returns>
        [DllImport("gdi32.dll", EntryPoint = "GetDeviceGammaRamp")]
        public static extern int GetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);
        static RAMP ramp = new RAMP();

        [DllImport("gdi32.dll", EntryPoint = "SetDeviceGammaRamp")]
        public static extern int SetDeviceGammaRamp(IntPtr hDC, ref RAMP lpRamp);

        [DllImport("user32.dll", EntryPoint = "GetDC")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct RAMP
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Red;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Green;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public UInt16[] Blue;
        }

        /// <summary>对窗体的大小操作
        /// 
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="nIndex"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern int GetWindowLong(IntPtr hwnd, int nIndex);
       
        const int WS_CAPTION = 0xC00000;//有标题栏
        const int WS_THICKFRAME = 0x40000; //调整大小用的边框
        const int WS_MAXIMIZEBOX = 0x10000; //最大化

        enum GWL : int
        {
            GWL_ID = (-12),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20)
        }


        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern long SetWindowLong(IntPtr hwnd, int nIndex, long dwNewLong);

        //获得窗口矩形
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        public static extern int GetWindowRect(IntPtr hWnd, out RECT lpRect);

        // 矩形结构
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll ", EntryPoint = "ClientToScreen")]
        static extern bool ClientToScreen(IntPtr hWnd, ref   Point lp);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", EntryPoint = "GetCursorPos")]
        static extern bool GetCursorPos(ref Point lpPoint);


        /// <summary> MoveWindow对窗体大小的操作
        /// </summary>
        /// <param name="hWnd"> 句柄窗体 </param>
        /// <param name="X"> 居左距离 </param>
        /// <param name="Y"> 居上距离</param>
        /// <param name="nWidth"> 窗体宽度</param>
        /// <param name="nHeight"> 窗体高度</param>
        /// <param name="bRepaint"> 是否重画</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        public static double GamaValue = 0.1;
        public static int SetGamma()
        {
            ramp.Red = new ushort[256];
            ramp.Green = new ushort[256];
            ramp.Blue = new ushort[256];

            for (int i = 1; i < 256; i++)
            {
                // gamma 必须在3和44之间
                ramp.Red[i] = ramp.Green[i] = ramp.Blue[i] = (ushort)(Math.Min(65535, Math.Max(0, Math.Pow((i + 1) / 256.0, 10 * GamaValue) * 65535 + 0.5)));
            }
            return SetDeviceGammaRamp(GetDC(IntPtr.Zero), ref ramp);
        }



        //伪全屏
        public static void PseudoFullScreen(bool isSet)
        {
            int wins = GetWindowLong(War.HWnd, (int)GWL.GWL_STYLE);

            System.Windows.Forms.Screen sc =  System.Windows.Forms.Screen.PrimaryScreen;
            int width = sc.Bounds.Width;
            int height = sc.Bounds.Height;

            if (isSet)
            {
                wins &= ~WS_CAPTION;
                wins &= ~WS_THICKFRAME;
            }
            else
            {
                wins |= WS_CAPTION;
                wins |= WS_THICKFRAME;

                width = sc.Bounds.Width - 400;
                height = sc.Bounds.Height - 200;
            }


            //设置边框样式 369164288
            Video.SetWindowLong(War.HWnd, (int)GWL.GWL_STYLE, wins);
             
            //对窗体大小调整
            Video.MoveWindow(War.HWnd, 0, 0, width, height, true);
        }

        //发送鼠标点击
        public static void SendMouseDown(IntPtr hWnd, int skillsIndex, int number) 
        {
            Point ptPast = new Point();
            GetCursorPos(ref ptPast);

            Point ptIng = new Point();
            ClientToScreen(hWnd, ref ptIng);

            RECT rc;
            GetWindowRect(hWnd, out rc);

            int width = rc.right - (int)ptIng.X;
            double wIndex = 0.78;
            double hIndex = 0.95;

            switch (skillsIndex) 
            {
                case 0: wIndex = 0.85; hIndex = 0.88; break;
                case 1: wIndex = 0.90; hIndex = 0.88; break;
                case 2: wIndex = 0.80; hIndex = 0.95; break;
                case 3: wIndex = 0.85; hIndex = 0.95; break;
                case 4: wIndex = 0.90; hIndex = 0.95; break;
                case 5: wIndex = 0.95; hIndex = 0.95; break;

                case 6: wIndex = 0.66; hIndex = 0.82; break;
                case 7: wIndex = 0.71; hIndex = 0.82; break;
                case 8: wIndex = 0.66; hIndex = 0.89; break;
                case 9: wIndex = 0.71; hIndex = 0.89;break;
                case 10: wIndex = 0.66; hIndex = 0.95; break;
                case 11: wIndex = 0.71; hIndex = 0.95; break;
            }

            width = (int)(width - (width * wIndex));
            width = rc.right - width;

            int height = rc.bottom - (int)ptIng.Y;
            height = (int)(height - (height * hIndex));
            height = rc.bottom - height;

            SetCursorPos(width, height);

            const int MOUSEEVENTF_LEFTDOWN = 0x0002;
            const int MOUSEEVENTF_LEFTUP = 0x0004;

            const uint KEYEVENTF_EXTENDEDKEY = 0x1;
            const uint KEYEVENTF_KEYUP = 0x2;

            const uint VK_Control = 0x11;

            ChangeKey.keybd_event((byte)VK_Control, 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
            
            for (int i = 0; i <= number; i++) 
            {
                ChangeKey.mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                System.Threading.Thread.Sleep(5);
            }
            ChangeKey.keybd_event((byte)VK_Control, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);


            SetCursorPos(ptPast.X, ptPast.Y);
        }

    }
}
