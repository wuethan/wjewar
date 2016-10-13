using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WjeWar
{
    class War
    {
        public static void WarInit()
        {
            War.BaseAddre = IntPtr.Zero;
            War.HWnd = IntPtr.Zero;
            War.IsChat = false;
            War.LensValue = 1650;
            War.Path = "";
            War.PId = 0;
            War.Version = "";
            War.IsCanSpeak = true;
        }

        public delegate void callForm();
        public static callForm CallForm;


        private static string gameName;

        private static string processName;

        private static int pId;

        private static string path;

        private static string version;

        private static string dllName;
        private static IntPtr baseAddre;

        private static bool isChat;

        private static IntPtr hWnd;
        private static string[] keyGroup;
        private static string state;
        private static double gamaValue;


        private static int lensValue;
        private static bool isCanSpeak = true;

        public static string GameName
        {
            get { return War.gameName; }
            set { War.gameName = value; }
        }

        public static string ProcessName
        {
            get { return War.processName; }
            set { War.processName = value; }
        }

        public static int PId
        {
            get 
            {
                if (War.pId == 0) 
                {
                    War.PId = WriteMemory.GetPidByProcessName(War.ProcessName);
                }
                return War.pId; 
            }
            set { War.pId = value; }
        }

        public static string Path
        {
            get 
            {
                if (War.path == null || War.path == "")
                {
                    string WarPath = GetWarVersion.GetUrPath(War.ProcessName);
                    War.Path = WarPath.Substring(0, WarPath.LastIndexOf("\\"));
                }
                return War.path;
            }
            set { War.path = value; }
        }

        public static string Version
        {
            get 
            {
                if (War.version == null || War.version == "")
                {
                    War.Version = GetWarVersion.GetVersion(War.ProcessName, War.DllName);
                }
                return War.version; 
            }
            set { War.version = value; }
        }

        public static string DllName
        {
            get { return War.dllName; }
            set { War.dllName = value; }
        }

        public static IntPtr BaseAddre
        {
            get 
            {
                if (War.baseAddre == IntPtr.Zero)
                War.BaseAddre = WriteMemory.GetDllAddre(War.ProcessName, War.DllName);

                return War.baseAddre; 
            }
            set { War.baseAddre = value; }
        }

        public static bool IsChat
        {
            get { return War.isChat; }
            set { War.isChat = value;}
        }

        public static IntPtr HWnd
        {
            get { return War.hWnd;}
            set 
            {
                if (War.hWnd != value)
                {
                    War.hWnd = value;
                    if (value != IntPtr.Zero)
                    {
                        War.State = "已运行";
                        War.Version = GetWarVersion.GetVersion(War.ProcessName, War.DllName);
                    }
                    else
                    {
                        War.State = "未启动";
                        Video.GamaValue = 0.1;
                        Video.SetGamma();
                        ChangeKey.KeyModifyOFF(War.hWnd);
                        War.WarInit();
                    }
                }
            }
        }

        public static string[] KeyGroup
        {
            get { return War.keyGroup; }
            set { War.keyGroup = value; }
        }

        public static string State
        {
            get { return War.state; }
            set 
            {
                if (state != value) 
                {
                    if ("未启动".Equals(War.state))
                    {
                        War.CallForm();
                    }
                    War.state = value;
                }
            }
        }


        public static double GamaValue
        {
            get { return War.gamaValue; }
            set { War.gamaValue = value; }
        }

        public static int LensValue
        {
            get 
            {
                if (War.lensValue == 0) 
                {
                    byte[] newLensValue = WriteMemory.ReadMemoryValueBYTE(0x9485BC, War.ProcessName);
                    lensValue = (int)BitConverter.ToSingle(newLensValue, 0);
                }

                return War.lensValue; 
            }
            set 
            {
                if (value > 3100)
                {
                    value = 1650;
                }
                else if (value < 1650)
                {
                    value = 3100;
                }
                War.lensValue = value;

                switch (War.Version)
                {
                    case "1.20E": WriteMemory.ChangeVision120E(War.lensValue);
                        break;
                    case "1.24E": WriteMemory.ChangeVision124BE(War.lensValue);
                        break;
                    case "1.24B": WriteMemory.ChangeVision124BE(War.lensValue);
                        break;
                }
                const uint VK_PRIOR = 0x21;
                const uint VK_NEXT = 0x22;
                ChangeKey.SendVkMessage(War.HWnd, VK_PRIOR);
                ChangeKey.SendVkMessage(War.HWnd, VK_NEXT);
            }
        }

        public static bool IsCanSpeak
        {
            get { return War.isCanSpeak; }
            set { War.isCanSpeak = value; }
        }
    }
}
