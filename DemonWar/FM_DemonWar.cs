using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Drawing.Imaging;
using Microsoft.Win32;

//W.je
namespace WjeWar
{
    //进程提权使用
    using LUID = Int64;

    public partial class  FM_DemonWar : Form
    {
        public FM_DemonWar()
        {
            InitializeComponent();
        }

        /// <summary>窗体前置
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern int SetForegroundWindow(IntPtr FromUp);

        [DllImport("User32.dll", EntryPoint = "ReleaseDC")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>获得焦点句柄
        /// 
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>重写WndProc()方法，通过监视系统消息，来调用过程
        /// 
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)//监视Windows消息
        {
            const int WM_HOTKEY = 0x0312;//m.Msg值为0x0312表示按下热键
            //const int WM_MOVE = 0x3;


            switch (m.Msg)
            {
                case WM_HOTKEY:
                    ProcessHotkey(m);//按下热键时调用ProcessHotkey()函数
                    break;
            }

            base.WndProc(ref m); //将系统消息传递自父类的WndProc
        }


        /// <summary>是否在聊天
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsChatByVersion(string version)  
        {
            int address = 0;
            if ("1.20E".Equals(version) || "1.21".Equals(version))
                address = 0x45CB8C;
            else if ("1.24E".Equals(version) || "1.24B".Equals(version))
                address = 0xAE8450;

            int isChat = WriteMemory.ReadMemoryValueINT(address, War.ProcessName);

            if (isChat == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>写注册表
        /// 
        /// </summary>
        /// <param name="strName"></param>
        /// <param name="strValue"></param>
        public static void SetRegEditData(string strName, string strValue)
        {
            RegistryKey hklm = Registry.CurrentUser;
            try
            {
                //System.Security.AccessControl.RegistrySecurity rs = new System.Security.AccessControl.RegistrySecurity();
                //rs.AddAccessRule(new System.Security.AccessControl.RegistryAccessRule("Administrator", System.Security.AccessControl.RegistryRights.FullControl, System.Security.AccessControl.InheritanceFlags.ObjectInherit, System.Security.AccessControl.PropagationFlags.InheritOnly, System.Security.AccessControl.AccessControlType.Allow));
                RegistryKey software = hklm.OpenSubKey("SOFTWARE\\Blizzard Entertainment\\Warcraft III", true);
                software.SetValue(strName, strValue, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                MessageBox.Show("塔攻范围可能无法使用，原因："+ex.Message.ToString());
            }
            finally 
            {
                hklm.Close();
            }
        }

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx功能区xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        /// <summary>提权
        /// 
        /// </summary>
        /// <param name="TokenHandle"></param>
        /// <param name="DisableAllPrivileges"></param>
        /// <param name="NewState"></param>
        /// <param name="Zero"></param>
        /// <param name="Null1"></param>
        /// <param name="Null2"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            UInt32 Zero,
            IntPtr Null1,
            IntPtr Null2);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal static extern IntPtr GetCurrentProcess();

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            int DesiredAccess,
            ref IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern bool LookupPrivilegeValue(
            string lpSystemName,
            string lpName,
            ref LUID lpLuid);

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct LUID_AND_ATTRIBUTES
        {
            public LUID_AND_ATTRIBUTES(LUID Luid, int Attributes)
            {
                this.Luid = Luid;
                this.Attributes = Attributes;
            }
            public LUID Luid;
            public int Attributes;
        }


        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        internal const string SE_PRIVILEGE_NAMETEXT = "SeDebugPrivilege";


        //提权
        public static bool SetPrivilege()
        {
            TOKEN_PRIVILEGES tmpKP = new TOKEN_PRIVILEGES();

            tmpKP.PrivilegeCount = 1;

            LUID_AND_ATTRIBUTES[] LAA = new LUID_AND_ATTRIBUTES[1];

            LAA[0] = new LUID_AND_ATTRIBUTES(0, SE_PRIVILEGE_ENABLED);

            tmpKP.Privileges = LAA;

            bool retVal = false;

            IntPtr hdlProcessHandle = IntPtr.Zero;
            IntPtr hdlTokenHandle = IntPtr.Zero;
            try
            {
                hdlProcessHandle = GetCurrentProcess();

                retVal = OpenProcessToken(hdlProcessHandle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref hdlTokenHandle);

                retVal = LookupPrivilegeValue(null, SE_PRIVILEGE_NAMETEXT, ref tmpKP.Privileges[0].Luid);

                retVal = AdjustTokenPrivileges(hdlTokenHandle, false, ref tmpKP, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                WriteMemory.CloseHandle(hdlProcessHandle);
                WriteMemory.CloseHandle(hdlTokenHandle);
            }

            return retVal;
        }

       


        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx自定义方法 开始xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx



        /// <summary>加载配置
        /// 
        /// </summary>
        public void LoadConfig()
        {
            CK_OpenFullFigure.Checked = Properties.Settings.Default.OpenFullFigure;
            CK_FromFalseAllscreen.Checked = Properties.Settings.Default.FromFalseAllscreen;
            CK_VideoGama.Checked = Properties.Settings.Default.VideoGama;
            RB_KeyDownCloseConnection.Checked = Properties.Settings.Default.KeyDownCloseConnection;
            RB_CloseDescConnection.Checked = Properties.Settings.Default.CloseDescConnection;
            CB_EnlargeHorizon.Checked = Properties.Settings.Default.EnlargeHorizon;
            CB_ScreenBad.Checked = Properties.Settings.Default.ScreenBad;
            CB_ClearFog.Checked = Properties.Settings.Default.ClearFog;
            TX_KeySpeak.Text = Properties.Settings.Default.KeySpeak;

            TX_KeyNum7.Text = Properties.Settings.Default.KeyNum7;
            TX_KeyNum8.Text = Properties.Settings.Default.KeyNum8;
            TX_KeyNum4.Text = Properties.Settings.Default.KeyNum4;
            TX_KeyNum5.Text = Properties.Settings.Default.KeyNum5;
            TX_KeyNum1.Text = Properties.Settings.Default.KeyNum1;
            TX_KeyNum2.Text = Properties.Settings.Default.KeyNum2;
        }


        /// <summary>保存配置
        /// 
        /// </summary>
        public void SaveConfig()
        {
            Properties.Settings.Default.OpenFullFigure = CK_OpenFullFigure.Checked;
            Properties.Settings.Default.FromFalseAllscreen = CK_FromFalseAllscreen.Checked;
            Properties.Settings.Default.VideoGama = CK_VideoGama.Checked;
            Properties.Settings.Default.KeyDownCloseConnection = RB_KeyDownCloseConnection.Checked;
            Properties.Settings.Default.CloseDescConnection = RB_CloseDescConnection.Checked;
            Properties.Settings.Default.EnlargeHorizon = CB_EnlargeHorizon.Checked;
            Properties.Settings.Default.ScreenBad = CB_ScreenBad.Checked;
            Properties.Settings.Default.ClearFog = CB_ClearFog.Checked;

            Properties.Settings.Default.DotaImbaCmd = CB_DotaImbaCmd.SelectedIndex;

           
            Properties.Settings.Default.KeySpeak = TX_KeySpeak.Text;

            Properties.Settings.Default.KeyNum7 = TX_KeyNum7.Text;
            Properties.Settings.Default.KeyNum8 = TX_KeyNum8.Text;
            Properties.Settings.Default.KeyNum4 = TX_KeyNum4.Text;
            Properties.Settings.Default.KeyNum5 = TX_KeyNum5.Text;
            Properties.Settings.Default.KeyNum1 = TX_KeyNum1.Text;
            Properties.Settings.Default.KeyNum2 = TX_KeyNum2.Text;
            Properties.Settings.Default.Save();

        }

       
        /// <summary>根据数组索引断开端口
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void ClosRemoteByIndex(int index) 
        {
            if (Disconnecter.GetRemoteProt().Length >= index)
            {
                int[] RemoteProt = Disconnecter.GetRemoteProt();
                Array.Sort(RemoteProt);

                for (int i = 1; i < RemoteProt.Length; i++) 
                {
                    Console.WriteLine(RemoteProt[i]);
                }

                try
                {
                    Disconnecter.CloseRemotePort(RemoteProt[index]);
                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                }
            }
        }

        /// <summary>转UTF-8
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string ConvertEncode(string input)
        {
            byte[] b_utf8 = Encoding.UTF8.GetBytes(input);
            byte[] dst_utf8 = Encoding.Convert(Encoding.Default, Encoding.UTF8, b_utf8, 0, b_utf8.Length);
            string result = Encoding.UTF8.GetString(dst_utf8, 0, dst_utf8.Length);
            return result;
        }


        public void NotWinAccess(Win32Exception ex) 
        {
            MessageBox.Show(ex.Message.ToString() + "\n权限不足，如果是Windows7或Vista用户请右键管理员运行或使用该软件Win7版！ ");
            FM_DemonWar_FormClosing(null, null);
            FM_DemonWar_FormClosed(null, null);
        }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx自定义方法 结束xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx窗体控件事件 开始xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


        private bool isChat = false;

        public bool IsChat
        {
            get { return isChat; }
            set 
            {
                if (isChat != value)
                {
                    if (value)
                    {
                        ChangeKey.KeyModifyOFF(this.Handle);
                    }
                    else
                    {
                        string[] keyGroup = new string[] {
                                TX_KeyNum7.Text , TX_KeyNum8.Text,
                                TX_KeyNum4.Text , TX_KeyNum5.Text,
                                TX_KeyNum1.Text , TX_KeyNum2.Text,
                                TX_KeySpeak.Text, TX_Skill0.Text,
                                TX_Skill1.Text  , TX_Skill2.Text,
                                TX_Skill3.Text  , TX_Skill4.Text,
                                TX_Skill5.Text  , TX_bag1.Text,
                                TX_bag2.Text    , TX_bag3.Text,
                                TX_bag4.Text    , TX_bag5.Text,
                                TX_bag6.Text };

                        ChangeKey.KeyModifyOn(this.Handle, keyGroup);
                    }
                    isChat = value;
                }

                isChat = value;
            }
        }


        private IntPtr foregroundWin = IntPtr.Zero;

        bool isElse = true; //标示：是否执行最后的else;

        //封装字段：当前前置窗体
        public IntPtr ForegroundWin
        {
            set 
            {
                if (foregroundWin != value)
                {
                    foregroundWin = value;
                    if (value == War.HWnd)
                    {
                        isElse = true;
                        string[] keyGroup = new string[] {
                                TX_KeyNum7.Text , TX_KeyNum8.Text,
                                TX_KeyNum4.Text , TX_KeyNum5.Text,
                                TX_KeyNum1.Text , TX_KeyNum2.Text,
                                TX_KeySpeak.Text, TX_Skill0.Text,
                                TX_Skill1.Text  , TX_Skill2.Text,
                                TX_Skill3.Text  , TX_Skill4.Text,
                                TX_Skill5.Text  , TX_bag1.Text,
                                TX_bag2.Text    , TX_bag3.Text,
                                TX_bag4.Text    , TX_bag5.Text,
                                TX_bag6.Text };

                        ChangeKey.KeyModifyOn(this.Handle, keyGroup);

                        if (CK_VideoGama.Checked)
                        {
                            Video.GamaValue = War.GamaValue;
                            Video.SetGamma();
                        }
                        else
                        {
                            War.GamaValue = 0.1;
                            Video.GamaValue = War.GamaValue;
                            Video.SetGamma();
                        }   
                    }
                    else
                    {
                        if (isElse) 
                        {
                            isElse = false;
                            Video.GamaValue = 0.1;
                            Video.SetGamma();
                            ChangeKey.KeyModifyOFF(this.Handle);
                        }
                    }  
                }
            }
        }

        


        public void AutoStart() 
        {
             DialogResult result = MessageBox.Show("检测到您重新启动游戏，是否开启选项？","提示",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
             if (result == DialogResult.Yes)
             {
                 CK_FromFalseAllscreen_CheckedChanged(null, null); //气全图
                 CB_DisplayInvisible_CheckedChanged(null, null); //显示隐形
                 CB_ClearFog_CheckedChanged(null, null); //清除迷雾
                 CB_FogSelected_CheckedChanged(null, null);
                 CB_PassAH_CheckedChanged(null, null); //过-Ah
                 CB_ShowHp_CheckedChanged(null, null); //1.20E显血
                 CB_ScreenBad_CheckedChanged(null, null); //DotaIMBA防切屏崩溃
                 CK_ShowMana_CheckedChanged(null, null); //显蓝
                 CK_OpenFullFigure_CheckedChanged(null, null); //窗口全屏
                 CB_EnlargeHorizon_CheckedChanged(null, null); //扩大视野
                 CK_VideoGama_CheckedChanged(null, null); //亮度调整
                 CB_SkillNoCD_CheckedChanged(null, null); //技能无CD
             }
        }


        private void FM_DemonWar_Load(object sender, EventArgs e)
        {
            bool IsProcess = SetPrivilege(); //获得内存操作权限
            if (IsProcess)
            {
                War.GameName = "Warcraft III";
                War.ProcessName = "War3";
                War.DllName = "game.dll";
                War.State = "未运行";
                War.GamaValue = 0.1;


                War.CallForm = new War.callForm(AutoStart);

                War.HWnd = Api.FindWindow(War.GameName, War.GameName);


                if (War.HWnd != IntPtr.Zero)
                {
                    War.BaseAddre = WriteMemory.GetDllAddre(War.ProcessName, War.DllName);
                    //War.Version = GetWarVersion.GetVersion(War.ProcessName, War.DllName);
                    //string WarPath = GetWarVersion.GetUrPath(War.ProcessName);
                    //War.Path = WarPath.Substring(0, WarPath.LastIndexOf("\\"));
                    //War.PId = WriteMemory.GetPidByProcessName(War.ProcessName);
                }

                LoadConfig(); //加载配置
                TM_State.Enabled = true; //打开计时器
            }
            else
            {
                MessageBox.Show("取权失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }


        private void TM_State_Tick(object sender, EventArgs e)
        {

            War.HWnd = Api.FindWindow(War.GameName, War.GameName);

            if (War.HWnd != IntPtr.Zero)
            {
                ForegroundWin = GetForegroundWindow();
                IsChat = IsChatByVersion(War.Version);

                if (LB_WarSate.ForeColor != Color.Red) 
                {
                    LB_WarSate.ForeColor = Color.Red;
                }
                LB_Version.Text = "版本：" + War.Version;
            }
            else 
            {
                if (LB_WarSate.ForeColor != Color.Gray) 
                {
                    LB_WarSate.ForeColor = Color.Gray;
                }
            }
            LB_WarSate.Text = War.State;

        }

        private void WarOver_Click(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                //GameOver
                Disconnecter.CloseLocalPort(6112);
            }
        }

        private void Click_CloseAllRemote(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                string[] TCPlist = Disconnecter.Connections();
                
                //TCP连接的位置
                if (RB_CloseDescConnection.Checked)
                {
                    int[] remoteProt = Disconnecter.GetRemoteProt();
                    string orderRemote = TX_ClosOrderRemote.Text;

                    if (orderRemote.Length > remoteProt.Length) 
                    {
                        orderRemote = orderRemote.Substring(0, remoteProt.Length);
                    }
                    if (remoteProt.Length != 0) 
                    {
                        for (int i = 0; i < TX_ClosOrderRemote.Text.Length; i++)
                        {
                            Disconnecter.CloseRemotePort(remoteProt[(int)orderRemote[i]]);
                        }
                    }
                    
                }
                else 
                {
                    //MessageBox.Show("请选择踢出方式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }


        //按下设定的键时调用该函数
        private void ProcessHotkey(Message m) 
        {
                IntPtr id = m.WParam;
                string sid = id.ToString();
                switch (sid)
                {
                    case "7":
                        int[] Num7 = { 103};
                        ChangeKey.KeyBoardDo(Num7, War.HWnd);
                        break;
                    case "8":
                        int[] Num8 = { 104 };
                        ChangeKey.KeyBoardDo(Num8, War.HWnd);
                        break;
                    case "4":
                        int[] Num4 = { 100 };
                        ChangeKey.KeyBoardDo(Num4, War.HWnd);
                        break;
                    case "5":
                        int[] Num5 = { 101 };
                        ChangeKey.KeyBoardDo(Num5, War.HWnd);
                        break;
                    case "1":
                        int[] Num1 = { 97 };
                        ChangeKey.KeyBoardDo(Num1, War.HWnd);
                        break;
                    case "2":
                        int[] Num2 = { 98 };
                        ChangeKey.KeyBoardDo(Num2, War.HWnd);
                        break;
                    case "10":
                        ClosRemoteByIndex(1);
                        break;
                    case "11":
                        ClosRemoteByIndex(2);
                        break;
                    case "12":
                        ClosRemoteByIndex(3);
                        break;
                    case "13":
                        ClosRemoteByIndex(4);
                        break;
                    case "14":
                        ClosRemoteByIndex(5);
                        break;
                    case "15":
                        ClosRemoteByIndex(6);
                        break;
                    case "16":
                        ClosRemoteByIndex(7);
                        break;
                    case "17":
                        ClosRemoteByIndex(8);
                        break;
                    case "18":
                        ClosRemoteByIndex(9);
                        break;
                    case "20":
                        if (War.GamaValue > 0.05)
                            Video.GamaValue = War.GamaValue -= 0.01;
                        if (CK_VideoGama.Checked) Video.SetGamma(); break;
                    case "21":
                        if (War.GamaValue < 0.1)
                            Video.GamaValue = War.GamaValue += 0.01;
                        if (CK_VideoGama.Checked) Video.SetGamma(); break;
                    case "22":
                        if (War.HWnd == GetForegroundWindow())
                        {
                            if (War.IsCanSpeak) 
                            {
                                string str = TX_SpeakContent.Text;
                                if (str != "")
                                {
                                    War.IsCanSpeak = false;
                                    Clipboard.SetText(ConvertEncode(str));
                                    ChangeKey.WarSpeak(War.HWnd);
                                    War.IsCanSpeak = true;
                                }
                            }
                        } break;
                    case "23":
                        CrowdedRoom.VsRoom();
                        break;
                    case "24":
                        War.LensValue = War.LensValue + 300; break;
                    case "25":
                        Video.SendMouseDown(War.HWnd, 0, 30); break;
                    case "26":
                        Video.SendMouseDown(War.HWnd, 1, 30); break;
                    case "27":
                        Video.SendMouseDown(War.HWnd, 2, 30); break;
                    case "28":
                        Video.SendMouseDown(War.HWnd, 3, 30); break;
                    case "29":
                        Video.SendMouseDown(War.HWnd, 4, 30); break;
                    case "30":
                        Video.SendMouseDown(War.HWnd, 5, 30); break;
                    case "31": 
                        Video.SendMouseDown(War.HWnd, 6, 30); break;
                    case "32":
                        Video.SendMouseDown(War.HWnd, 7, 30); break;
                    case "33":
                        Video.SendMouseDown(War.HWnd, 8, 30); break;
                    case "34":
                        Video.SendMouseDown(War.HWnd, 9, 30); break;
                    case "35":
                        Video.SendMouseDown(War.HWnd, 10,30); break;
                    case "36":
                        Video.SendMouseDown(War.HWnd, 11,30); break;
                }
        }

        

        private void CK_OpenFullFigure_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CK_OpenFullFigure.Checked) 
                {
                    switch (War.Version) 
                    {
                        case "1.20E": WriteMemory.SetWriteMemoryOneTwoE(); break;
                        case "1.24E": WriteMemory.SetWriteMemoryOneFourE(); break;
                        case "1.24B": WriteMemory.SetWriteMemoryOneFourB(); break;
                    }
                    //前置窗体
                    SetForegroundWindow(War.HWnd);
                }
            }
        }

        private void CK_FromFalseAllscreen_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                if (CK_FromFalseAllscreen.Checked)
                {
                    Video.PseudoFullScreen(true);
                }
                else 
                {
                    Video.PseudoFullScreen(false);
                }
            }
        }

        private void CK_ShowMana_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CK_ShowMana.Checked) 
                {
                    bool isOpen = false;

                    if (File.Exists(War.Path + "\\" + "TempReplay.w3g"))
                    {
                        DialogResult result = MessageBox.Show("游戏中开启此选项可能会导致魔兽崩溃,如果只运行并未开始游戏可开启该功能,是否开启？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                        if (result == DialogResult.Yes)
                        {
                            isOpen = true;
                        }
                    }
                    else 
                    {
                        isOpen = true;
                    }
                    if (isOpen) 
                    {
                        bool isOk = DllInject.ManaStart("mana.dll", true);
                    }
                }
            }
        }


        //按键前事件验证
        private void TX_PreviewKey_Validate(object sender, PreviewKeyDownEventArgs e)
        {
            TextBox key = (TextBox)sender;
            if (!"Delete".Equals(e.KeyCode.ToString()) && !"Back".Equals(e.KeyCode.ToString()))
            {
                string keyContent = ChangeKey.KeyFilter(e, ((TextBox)sender).Name);

                if (keyContent != "")
                {
                    key.Text = e.KeyCode.ToString();
                }
            }
            else
            {
                key.Text = "";
            }
        }

        //按键后事件验证
        public void TX_KeyNumModify_Validate(Object sender, KeyEventArgs e) 
        {
            switch (e.Modifiers.ToString().ToLower())
            {
                case "alt": if (e.KeyCode.ToString().ToLower().IndexOf("menu") == -1)
                        if (sender is TextBox)
                            (sender as TextBox).Text = "Alt+" + e.KeyCode.ToString(); break;
                case "shift": if (e.KeyCode.ToString().ToLower().IndexOf("shiftkey") == -1)
                       if (sender is TextBox)
                            (sender as TextBox).Text = "Shift+" + e.KeyCode.ToString(); break;
                case "control": if (e.KeyCode.ToString().ToLower().IndexOf("controlkey") == -1)
                        if (sender is TextBox)
                            (sender as TextBox).Text = "Control+" + e.KeyCode.ToString(); break;
            }
        }



        private void CK_VideoGama_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                if (CK_VideoGama.Checked)
                {
                    int ResultGammaValue = Video.SetGamma();

                    ChangeKey.UninstallKey(this.Handle, new int[] { 21, 22 });

                    ChangeKey.KeyModify(this.Handle,20, "Up", 1);
                    ChangeKey.KeyModify(this.Handle,21, "Down", 1);
                }
                else
                {
                    War.GamaValue = 0.1;
                    Video.GamaValue = War.GamaValue;
                    Video.SetGamma();

                    ChangeKey.UninstallKey(this.Handle, new int[] { 21, 22 });
                }
            }
        }

        private void RB_CloseConnection_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                int[] keyIndexGroup ={ 10, 11, 12, 13, 14, 15, 16, 17, 18 };

                if (RB_CloseDescConnection.Checked)
                {
                    ChangeKey.UninstallKey(this.Handle, keyIndexGroup);
                    TX_ClosOrderRemote.Enabled = true;
                    BT_CloseALLRemote.Enabled = true;
                }
                else if (RB_KeyDownCloseConnection.Checked)
                {
                    ChangeKey.UninstallKey(this.Handle, keyIndexGroup);

                    ChangeKey.KeyModify(this.Handle, 10, "D1", 1);
                    ChangeKey.KeyModify(this.Handle, 11, "D2", 1);
                    ChangeKey.KeyModify(this.Handle, 12, "D3", 1);
                    ChangeKey.KeyModify(this.Handle, 13, "D4", 1);
                    ChangeKey.KeyModify(this.Handle, 14, "D5", 1);
                    ChangeKey.KeyModify(this.Handle, 15, "D6", 1);
                    ChangeKey.KeyModify(this.Handle, 16, "D7", 1);
                    ChangeKey.KeyModify(this.Handle, 17, "D8", 1);
                    ChangeKey.KeyModify(this.Handle, 18, "D9", 1);

                    TX_ClosOrderRemote.Enabled = false;
                    BT_CloseALLRemote.Enabled = false;
                }
            }            
        }



        private void TX_KeySpeak_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (!"Alt".Equals(e.Modifiers.ToString()))
            {
                string keyContent = ChangeKey.KeyFilter(e, "");

                if (keyContent != "")
                {
                    TX_KeySpeak.Text = keyContent;
                }
            }
        }


        private void CN_DotaImbaCmd_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(CB_DotaImbaCmd.SelectedIndex)
            {
                case 0: TX_SpeakContent.Text = "-sdimstscfefnfrakbb";  break;
                case 1: TX_SpeakContent.Text = "-ardmimstakssscfefnbb-nd"; break;
                case 2: TX_SpeakContent.Text = "-ardmimstakssscfefnbb"; break;
                case 3: TX_SpeakContent.Text = "-ardmimstakssscfefnbb-wtf"; break;
                case 4: TX_SpeakContent.Text = "-ay"; break;
                case 5: TX_SpeakContent.Text = "-apimoxay"; break;
            }
        }


        private void BT_DeleteKeyModify_Click(object sender, EventArgs e)
        {
            TX_KeyNum7.Text = "";
            TX_KeyNum8.Text = "";
            TX_KeyNum4.Text = "";
            TX_KeyNum5.Text = "";
            TX_KeyNum1.Text = "";
            TX_KeyNum2.Text = "";
            TX_KeySpeak.Text = "";
        }
        private void FM_DemonWar_FormClosing(object sender, FormClosingEventArgs e)
        {
            Video.GamaValue = (float)0.1; //恢复屏幕亮度
            Video.SetGamma();
            SaveConfig(); //保存配置
        }

        private void FM_DemonWar_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose(true);
            this.Close();
            CrowdedRoom.OverCrowded();
            Application.Exit();
            Application.ExitThread();
        }

        private void CB_DisplayInvisible_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_DisplayInvisible.Checked)
                {
                    switch (War.Version)
                    {
                        case "1.20E": WriteMemory.DisplayInvisible120E(); break;
                        case "1.24E": WriteMemory.DisplayInvisible124E(); break;
                        case "1.24B": WriteMemory.DisplayInvisible124B(); break;
                    }
                }
            }
        }

        private void CB_FogSelected_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_FogSelected.Checked)
                {
                    switch (War.Version)
                    {
                        case "1.20E": WriteMemory.FogSelected120E(); break;
                        case "1.24E": WriteMemory.FogSelected124E(); break;
                        case "1.24B": WriteMemory.FogSelected124B(); break;
                    }
                }
            }
        }

        private void CB_PassAH_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_PassAH.Checked)
                {
                    switch (War.Version)
                    {
                        case "1.20E": WriteMemory.PassAH120E(); break;
                        case "1.24E": WriteMemory.PassAH124E(); break;
                        case "1.24B": WriteMemory.PassAH124B(); break;
                    }
                }
            }
        }

        private void CB_EnlargeHorizon_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_EnlargeHorizon.Checked)
                {
                    ChangeKey.KeyModify(this.Handle,24, "Left", 1);
                }
                else 
                {
                    ChangeKey.UnregisterHotKey(Handle, 24);
                }
            }
        }

        private void BT_VSROOM_Click(object sender, EventArgs e)
        {
            if (BT_VSROOM.Text.IndexOf("V S 擠 房") != -1)
            {
                BT_VSROOM.Text = "选择房间按下 Alt+空格 键";

                ChangeKey.KeyModify(this.Handle,23, "Space", 1);
            }
            else 
            {
                BT_VSROOM.Text = "V S 擠 房";
                CrowdedRoom.iSquit = true;
                ChangeKey.UnregisterHotKey(Handle, 23);
                CrowdedRoom.OverCrowded();
            }
        }

        private void BT_HFROOM_Click(object sender, EventArgs e)
        {
            if (BT_HFROOM.Text.IndexOf("浩 方 挤 房") != -1)
            {
                BT_HFROOM.Text = "取 消 挤 房";

                CrowdedRoom.HfRoom();
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx浩方配置
            }
            else
            {
                BT_HFROOM.Text = "浩 方 挤 房";
                CrowdedRoom.iSquit = true;
                CrowdedRoom.OverCrowded();
            }
        }

        private void CB_RowerRange_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                byte[][] modelByte = { WjeWar.Properties.Resources.AncientProtector, WjeWar.Properties.Resources.ziggurat };
                string[] modelPath = { "Buildings\\nightelf\\AncientProtector", "Buildings\\Undead\\ziggurat" };
                string[] modelName = { "AncientProtector.mdx", "ziggurat.mdx" };

                if (CB_RowerRange.Checked)
                {
                    string path = War.Path + "\\" + modelPath[0];
                    bool isSetup = FileManage.SetupModel(modelByte[0], path, modelName[0]);

                    path = War.Path + "\\" + modelPath[1];
                    isSetup = FileManage.SetupModel(modelByte[1], path, modelName[1]);
                }
                else
                {
                    FileManage.deleteFile(War.Path +"\\"+ modelPath[0] +"\\"+ modelName [0]);
                    FileManage.deleteFile(War.Path + "\\" + modelPath[1] + "\\" + modelName[1]);
                }
            }
        }

        private void CB_SkillsMarkers_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {

                byte[] modelByte = WjeWar.Properties.Resources.heroflamelord;
                string modelName = "heroflamelord.mdx";

                if (CB_SkillsMarkers.Checked)
                {
                    bool isSetup = FileManage.SetupModel(modelByte, War.Path, modelName);
                    if (isSetup)
                    {
                        SetRegEditData("Allow Local Files", "00000001");
                    }
                    else
                    {
                        MessageBox.Show("安装失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else 
                {
                    FileManage.deleteFile(War.Path + "\\" + modelName);
                }
            }
        }

        private void CB_ShowFuzzy_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {

                byte[][] modelByte = { WjeWar.Properties.Resources.FrostGlowBall64, WjeWar.Properties.Resources.Sword_1H_Miev_D_01, WjeWar.Properties.Resources.HeroWarden, WjeWar.Properties.Resources.yuelun };
                string modelPath = "Units\\nightelf\\HeroWarden";
                string[] modelName = { "FrostGlowBall64.blp", "Sword_1H_Miev_D_01.blp", "HeroWarden.mdx", "yuelun.mdx" };
                string path = War.Path + "\\" + modelPath;

                if (CB_ShowFuzzy.Checked)
                {
                    bool isSetup = false;
                    for (int i = 0; i < modelName.Length; i++)
                    {
                        isSetup = FileManage.SetupModel(modelByte[i], path, modelName[i]);
                    }

                    if (isSetup)
                    {
                        SetRegEditData("Allow Local Files", "00000001");
                    }
                    else
                    {
                        MessageBox.Show("安装失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else 
                {
                    FileManage.deleteFile(path + "\\" + modelName[0]);
                    FileManage.deleteFile(path + "\\" + modelName[1]);
                    FileManage.deleteFile(path + "\\" + modelName[2]);
                    FileManage.deleteFile(path + "\\" + modelName[3]);
                }
            }
        }



        private void CB_ShowHp_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_ShowHp.Checked)
                {
                    switch (War.Version)
                    {
                        case "1.20E": WriteMemory.patch(0x17F141, "x75"); break;
                    }
                }
            }
        }

        private void CB_ScreenBad_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_ScreenBad.Checked)
                {
                    switch (War.Version)
                    {
                        case "1.20E": WriteMemory.ScreenBad120E(); break;
                        case "1.24E": WriteMemory.ScreenBad124E(); break;
                        case "1.24B": WriteMemory.ScreenBad124B(); break;
                    }
                }
            }
        }


        private void TX_ClosOrderRemote_TextChanged(object sender, EventArgs e)
        {
            bool isInt = System.Text.RegularExpressions.Regex.IsMatch(TX_ClosOrderRemote.Text, @"^[0-9]*[1-9][0-9]*$");
        }

        private void LB_NoCdSet_Click(object sender, EventArgs e)
        {
            if (PN_SkillNoCd.Visible)
            {
                PN_SkillNoCd.Hide();
            }
            else 
            {
                PN_SkillNoCd.Show();
            }
        }


        private void BT_DelAllNoCdKey_Click(object sender, EventArgs e)
        {
            TX_Skill0.Text = "";
            TX_Skill1.Text = "";
            TX_Skill2.Text = "";
            TX_Skill3.Text = "";
            TX_Skill4.Text = "";
            TX_Skill5.Text = "";
        }

        private void CB_SkillNoCD_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero) 
            {
                if (CB_SkillNoCD.Checked) 
                {
                    switch (War.Version)
                    {
                        case "1.20E":
                            byte[] fileByte = WjeWar.Properties.Resources.NOCD120E;
                            DllInject.inject(fileByte, War.ProcessName, War.Path ,"NOCD_1.20e.dll"); 
                            break;
                        case "1.24E": break;
                        case "1.24B":
                            fileByte = WjeWar.Properties.Resources.NOCD124B;
                            DllInject.inject(fileByte, War.ProcessName,War.Path ,"NOCD_1.24b.dll"); 
                            break;
                    }
                }
            }
        }

        private void LB_NoCdHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "使用方法：\n 1.开启【技能无CD】后点击【设置】根据游戏中技能位置设置快捷键! \n "
                            +"2.请先释放 有施法动作的技能（如:拍拍熊第二个技能:超强力量）\n"
                            + " 3.按下【技能无CD】【设置】的快捷键相对位置，施放无指向性技能（如:拍拍熊 第一个技能）\n 4.成功释放后造成无法移动时，按 H 键保持原位恢复。\n"
                            +"例子：设置潮汐的第四个技能快捷键为【F4】，对敌人使用第一个技能后，马上按下F4施放大招，无法移动时请按H，保持原位恢复。"
                , "| 无CD使用方法 |");
        }

        private void LB_About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("未经允许,请忽非法传播！", "提示", MessageBoxButtons.OK);
        }

        private void 打开WjeWarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WjeNotifyIcon_MouseDoubleClick(null,null);
        }

        private void 开启基本功能ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.CK_OpenFullFigure.Checked = false;
            this.CK_OpenFullFigure.Checked = true;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FM_DemonWar_FormClosing(null,null);
            FM_DemonWar_FormClosed(null,null);
        }

        private void WjeNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WjeNotifyIcon.Visible = false;
            this.ShowInTaskbar = true;
            this.Show();
            WindowState = FormWindowState.Normal;
        }

        private void FM_DemonWar_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) 
            {
                this.Hide();
                this.ShowInTaskbar = false;
                this.WjeNotifyIcon.Visible = true;
            }
        }

        private void LB_Wje_Click(object sender, EventArgs e)
        {
            MessageBox.Show("github:https://github.com/wuethan \n如有建议或Bug请提交邮箱：wuethanopen@gmail.com", "提示");
        }

        private void CB_ClearFog_CheckedChanged(object sender, EventArgs e)
        {
            if (War.HWnd != IntPtr.Zero)
            {
                if (CB_ClearFog.Checked)
                {
                    switch (War.Version)
                    {
                        case "1.20E": WriteMemory.ClearFog120E(); break;
                        case "1.24E": WriteMemory.ClearFog124E(); break;
                        case "1.24B": WriteMemory.ClearFog124B(); break;
                    }
                }
            }
        }

        private void LB_update_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process currentProcess;
            currentProcess = System.Diagnostics.Process.Start("http://catfan.me/csharp/blog");
        }

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx窗体控件事件 结束xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


    }
}