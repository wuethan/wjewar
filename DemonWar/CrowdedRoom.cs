using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace WjeWar
{
    unsafe class CrowdedRoom
    {
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        private extern static IntPtr FindWindowEx(IntPtr phWnd, IntPtr chWnd, string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetTopWindow")]
        private extern static IntPtr GetTopWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public extern static UInt32 PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, long lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public extern static int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, long lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private extern static int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, string lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        public extern static IntPtr GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /*[DllImport("user32.dll", EntryPoint = "SetWindowText")]
        private extern static IntPtr SetWindowText(IntPtr hWnd, String lpString);*/

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private extern static int GetWindowThreadProcessId(IntPtr hWnd, ref int lpdwProcessId);

        [DllImport("Kernel32.dll", EntryPoint = "OpenProcess")]
        private extern static IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("Kernel32.dll", EntryPoint = "VirtualAllocEx")]
        private extern static int VirtualAllocEx(IntPtr hwnd, int lpaddress, int size, int type, int tect);

        [DllImport("Kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private extern static bool ReadProcessMemory(IntPtr hProcess, int lpBaseAddress, ref Point lpBuffer, int nSize, int lpNumberOfBytesRead);

        [DllImport("user32.dll", EntryPoint = "GetClassName")]
        public static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public delegate bool EnumChildWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32.dll", EntryPoint = "EnumChildWindows")]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildWindowsProc lpEnumFunc, int lParam);

        public static void StartListening(IntPtr hwndParent)
        {
            EnumChildWindowsProc myEnumChild = new EnumChildWindowsProc(EumWinChiPro);
            try
            {
                EnumChildWindows(hwndParent, myEnumChild, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private static bool EumWinChiPro(IntPtr hwnd, int lParam)
        {
            StringBuilder sb = new StringBuilder(1256);
            GetClassName(hwnd, sb, 1257);
            string className = sb.ToString();
            if (className != null && className != "")
            {
                if (className.Trim().IndexOf("SysListView32") != -1)
                {
                    hWnd = hwnd;
                }
            }
            return true;
        }

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETNEXTITEM = LVM_FIRST + 12;
        public const int LVNI_SELECTED = 0x0002;
        public const int PROCESS_ALL_ACCESS = 0x000F0000 | 0x00100000 | 0xFFF;
        public const int MEM_COMMIT = 0x1000;
        public const int PAGE_READWRITE = 0x04;
        public const int LVM_GETITEMPOSITION = (LVM_FIRST + 16);
        public const int WM_LBUTTONDBLCLK = 0x0203;
        public const int MK_LBUTTON = 0x0001;
        public const int WM_CLOSE = 0x0010;
        public const int WM_SETTEXT = 0x000C;
        public const int BM_CLICK = 0x00F5;

        public const int WM_MOUSEMOVE = 0x200;



        public static int interval; //挤房间隔
        public static int reInterval; //重新挤房时间
        public static string pfName; //平台名

        public static string wasteWin; //无用的窗体名
        public static string wasteClass;

        public static string waitingWin; //进入房间窗体名
        public static string waitingClass; //进入房间类名

        public static string btOkName;
        public static string btCloseName;

        private static IntPtr hWnd = IntPtr.Zero;
        public static IntPtr pHwnd = IntPtr.Zero;
        public static bool iSquit = false;
        private static IntPtr hProcess = IntPtr.Zero;
        private static int baseaddress = 0;

        public static Thread threadAnther;

        //定位房间
        public static void FindRoom()
        {
            string title = "";

            do
            {
                hWnd = FindWindowEx(IntPtr.Zero, hWnd, null, null);
                title = GetWinTitle(hWnd, pfName).Trim();

            } while (hWnd != IntPtr.Zero && title.IndexOf(pfName.Trim()) == -1);

            if (title != pfName)
            {
                iSquit = true;
                MessageBox.Show("请打开" + pfName + "！","提示",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                pHwnd = hWnd;
                hWnd = GetTopWindow(hWnd);
                hWnd = GetTopWindow(hWnd);
                hWnd = FindWindowEx(hWnd, IntPtr.Zero, "#32770", null);
                hWnd = FindWindowEx(hWnd, IntPtr.Zero, "SysListView32", null);

                //遍历子窗体查找SysListView32
                if (hWnd == IntPtr.Zero)
                {
                    StartListening(pHwnd);
                }
            }
        }

        //挤房间
        public static void CrowdRoom()
        {
            int index = SendMessage(hWnd, LVM_GETNEXTITEM, -1, LVNI_SELECTED);
            int PID = 0;
            hProcess = IntPtr.Zero;
            baseaddress = 0;
            GetWindowThreadProcessId(hWnd, ref PID);
            hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, PID);
            baseaddress = VirtualAllocEx(hProcess, 0, sizeof(Point), MEM_COMMIT, PAGE_READWRITE);
            SendMessage(hWnd, LVM_GETITEMPOSITION, index, (long)baseaddress);

            threadAnther = new Thread(new ThreadStart(Repeat));
            threadAnther.Start();
        }

        public static void Repeat()
        {
            IntPtr cHwnd = IntPtr.Zero;
            while (iSquit == false)
            {
                Point pt = new Point(0, 0);
                ReadProcessMemory(hProcess, baseaddress, ref pt, sizeof(Point), 0);

                PostMessage(hWnd, WM_MOUSEMOVE, MK_LBUTTON, pt.X | (pt.Y << 16));
                PostMessage(hWnd, WM_LBUTTONDBLCLK, MK_LBUTTON, pt.X | (pt.Y << 16));

                Thread.Sleep(interval);
                
                cHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, wasteClass, wasteWin);

                if (cHwnd != IntPtr.Zero)
                {
                    IntPtr sHwnd1 = FindWindowEx(cHwnd, IntPtr.Zero, "Button", btCloseName);
                    IntPtr sHwnd2 = FindWindowEx(cHwnd, IntPtr.Zero, "Button", btOkName);
                    if (sHwnd1 != IntPtr.Zero && sHwnd2 != IntPtr.Zero)
                    {
                        SendMessage(cHwnd, WM_CLOSE, 0, 0);
                        iSquit = true;
                        if (CrowdedRoom.threadAnther != null)
                        {
                            if (CrowdedRoom.threadAnther.IsAlive) 
                            {
                                CrowdedRoom.threadAnther.Abort();
                            }
                        }
                        break;
                    }
                    SendMessage(cHwnd, WM_CLOSE, 0, 0);

                }
                
                cHwnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, waitingClass, waitingWin);
                if (cHwnd != IntPtr.Zero)
                {
                    SendMessage(cHwnd, WM_CLOSE, 0, 0);
                }
                Thread.Sleep(reInterval);
            }
            
            iSquit = false;
        }


        //获得窗体标题
        private static string GetWinTitle(IntPtr hWnd, string pfName) 
        {
            if (pfName != "" && pfName != null) 
            {
                StringBuilder winName = new StringBuilder(512);
                GetWindowText(hWnd, winName, winName.Capacity);
                if (winName.Length != 0) 
                {
                    if (winName.ToString().Trim().IndexOf(pfName) != -1) 
                    {
                        return winName.ToString(0, pfName.Length);
                    }
                }
            }
            return "";
        }

        //结束挤房
        public static void OverCrowded() 
        {
            if (threadAnther != null)
            {
                if (threadAnther.IsAlive)
                    threadAnther.Abort();
            }
        }

        //VS挤房
        public static void VsRoom() 
        {
            pfName = "VS";
            FindRoom();
            interval = 2500;
            reInterval = 12000;
            wasteWin = "";
            wasteClass = "#32770";
            waitingWin = "";
            btCloseName = "否";
            btOkName = "是";
            waitingClass = "#32770";
            iSquit = false;
            CrowdRoom();
        }

        //浩方挤房
        public static void HfRoom() 
        {
            pfName = "浩方电竞平台";
            FindRoom();
            interval = 5000;
            reInterval = 12000;
            wasteWin = "浩方电竞平台";
            wasteClass = "#32770";
            waitingWin = "进入房间";
            btCloseName = "取消";
            btOkName = "是";
            waitingClass = "#32770";
            iSquit = false;
            CrowdRoom();
        }
    }
}
