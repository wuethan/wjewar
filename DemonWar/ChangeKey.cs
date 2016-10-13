using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WjeWar
{
    class ChangeKey
    {

        /// <summary>模拟按键
        /// 
        /// </summary>
        /// <param name="bVk"></param>
        /// <param name="bScan"></param>
        /// <param name="dwFlags"></param>
        /// <param name="dwExtraInfo"></param>
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        /// <summary>模拟鼠标
        /// 
        /// </summary>
        /// <param name="dwFlags"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="cButtons"></param>
        /// <param name="dwExtraInfo"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        /// <summary>注册系统热键
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <param name="fsModifiers"></param>
        /// <param name="vk"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "RegisterHotKey")]
        public static extern bool RegisterHotKey(
         IntPtr hWnd,
         int id,
         uint fsModifiers,
         Keys vk
        );

        ////组合键枚举
        //public enum KeyModifiers
        //{
        //    None = 0,
        //    Alt = 1,
        //    Control = 2,
        //    Shift = 4,
        //    Windows = 8
        //}


        /// <summary>卸载注册过的热键
        /// 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "UnregisterHotKey")]
        public static extern bool UnregisterHotKey(
         IntPtr hWnd,
         int id
        );

        /// <summary>发送消息给指定窗体
        /// 
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="wMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, uint wParam, uint lParam);

        const uint KEYEVENTF_EXTENDEDKEY = 0x1;
        const uint KEYEVENTF_KEYUP = 0x2;


        public static void KeyBoardDo(int[] key, IntPtr hWnd)
        {

            foreach(int k in key)
            {
                keybd_event((byte)k,0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
            }
            foreach(int k in key)
            {
                //发送一个松开Alt键的消息给War
                SendMessage(hWnd, 0x0105, 0x00000012, 0xC0380001);
                keybd_event((byte)k, 0x45,KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            }
        }

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const uint VK_CONTROL = 0x11;

        //模拟War喊话
        public static void WarSpeak(IntPtr hWnd)
        {
            KeyBoardDo(new int[] { 13 }, hWnd);
            keybd_event((byte)0x11, 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event((byte)86, 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event((byte)0x11, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            keybd_event((byte)16, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            KeyBoardDo(new int[] { 13 }, hWnd);
        }

        //发送按键消息
        public static void SendVkMessage(IntPtr hWnd,uint VKValue) 
        {
            SendMessage(hWnd, WM_KEYDOWN, VKValue, 0);
            SendMessage(hWnd, WM_KEYUP, VKValue, 0);
        }

        //注册热键自定方法
        public static void KeyModify(IntPtr hWnd , int Num, string Key, int Group)
        {
            uint NewNum = (uint)Num;
            Keys NewKey = (Keys)Enum.Parse(typeof(Keys), Key);
            UnregisterHotKey(hWnd, Num);
            RegisterHotKey(hWnd, Num, (uint)Group, NewKey);
        }

        //卸载系统热键
        public static void UninstallKey(IntPtr hWnd,int[] keyIndex) 
        {
            for (int i = 0; i < keyIndex.Length; i++) 
            {
                UnregisterHotKey(hWnd, keyIndex[i]);
            }
        }

        //注册单热键或组合热键验证
        public static void KeyRegisterValidate(IntPtr hWnd,string keyValue, int sid)
        {
            if (keyValue != "" && keyValue.IndexOf('+') == -1)
            {
                ChangeKey.KeyModify(hWnd, sid, keyValue, 0);
            }
            else
            {
                switch (keyValue.Split('+')[0].ToLower())
                {
                    case "alt": ChangeKey.KeyModify(hWnd, sid, keyValue.Split('+')[1], 1); break;
                    case "control": ChangeKey.KeyModify(hWnd, sid, keyValue.Split('+')[1], 2); break;
                    case "shift": ChangeKey.KeyModify(hWnd, sid, keyValue.Split('+')[1], 4); break;
                }
            }
        }

        //过滤组合单按键
        public static string KeyFilter(PreviewKeyDownEventArgs e, string keyName)
        {
            if (!"Alt".ToLower().Equals(e.Modifiers.ToString().ToLower()) && !"Shift".Equals(e.Modifiers.ToString()) && !"Control".Equals(e.Modifiers.ToString()))
            {
                if (!keyName.Equals(e.KeyCode.ToString()))
                {
                    return e.KeyCode.ToString();
                }
            }
            return "";
        }

        /// <summary>卸载包裹改键
        /// 
        /// </summary>
        public static void KeyModifyOFF(IntPtr hWnd)
        {
            int[] keGroup ={ 7, 8, 4, 5, 1, 2, 22, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
            ChangeKey.UninstallKey(hWnd, keGroup);
        }

        /// <summary>安装包裹改键
        /// 
        /// </summary>
        public static void KeyModifyOn(IntPtr hWnd,string[] sender)
        {
            KeyModifyOFF(hWnd);

            KeyRegisterValidate(hWnd, sender[0], 7);
            KeyRegisterValidate(hWnd, sender[1], 8);
            KeyRegisterValidate(hWnd, sender[2], 4);
            KeyRegisterValidate(hWnd, sender[3], 5);
            KeyRegisterValidate(hWnd, sender[4], 1);
            KeyRegisterValidate(hWnd, sender[5], 2);
            KeyRegisterValidate(hWnd, sender[6], 22);
            KeyRegisterValidate(hWnd, sender[7], 25);
            KeyRegisterValidate(hWnd, sender[8], 26);
            KeyRegisterValidate(hWnd, sender[9], 27);
            KeyRegisterValidate(hWnd, sender[10], 28);
            KeyRegisterValidate(hWnd, sender[11], 29);
            KeyRegisterValidate(hWnd, sender[12], 30);
            KeyRegisterValidate(hWnd, sender[13], 31);
            KeyRegisterValidate(hWnd, sender[14], 32);
            KeyRegisterValidate(hWnd, sender[15], 33);
            KeyRegisterValidate(hWnd, sender[16], 34);
            KeyRegisterValidate(hWnd, sender[17], 35);
            KeyRegisterValidate(hWnd, sender[18], 36);
        }
    }
}
