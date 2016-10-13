using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WjeWar
{

    class WriteMemory
    {
        //关闭一个内核对象
        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        public static extern void CloseHandle(IntPtr hObject);

        //读取byte[]内存
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAdress,byte[] lpBuffer, int size, int lpNumberOfBytesWritten);

        //读取int内存值
        [DllImportAttribute("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);


        //写内存float[]
        [DllImportAttribute("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, float[] lpBuffer, int nSize, IntPtr lpNumberOfBytesWritten);

        //写内存byte[]
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, Byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        //写内存byte[]
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, Byte[] lpBuffer, int nSize, int lpNumberOfBytesWritten);

        //写内存int
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, int lpBaseAddress, int lpBuffer,int nSize, out int lpNumberOfBytesWritten);

        //写string 
        [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
        public static extern int WriteProcessMemory(IntPtr hwnd, int baseaddress, string buffer, int nsize, int filewriten);

        //获得句柄ID
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        public static extern int GetWindowThreadProcessId(int hWnd, IntPtr ProcessId);

        //OpenProcess用来打开一个已存在的进程对象,并返回进程的句柄。 
        [DllImportAttribute("kernel32.dll", EntryPoint = "OpenProcess")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        //改变保护区域
        [DllImport("kernel32.dll", EntryPoint = "VirtualProtectEx")]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        //改变某地址保护区域
        [DllImport("kernel32.dll", EntryPoint = "VirtualProtectEx")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, ref uint lpfOldProtect);

        //进程快照
        [DllImport("Kernel32.dll", EntryPoint = "CreateToolhelp32Snapshot")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processid);

        //得到进程信息
        [DllImport("Kernel32.dll", EntryPoint = "Module32First")]
        public static extern int Module32First(IntPtr Handle, ref   MODULEENTRY32 Me);

        [DllImport("Kernel32.dll", EntryPoint = "Module32Next")]
        public static extern int Module32Next(IntPtr Handle, ref   MODULEENTRY32 Me);

        //枚举进程信息
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        public enum Protection : uint
        {
            NoAccess = 0x00000001,
            ReadOnly = 0x00000002,
            ReadWrite = 0x00000004,
            WriteCopy = 0x00000008,
            Execute = 0x00000010,
            ExecuteRead = 0x00000020,
            ExecuteWriteCopy = 0x00000040,
            Guard = 0x00000100,
            NoCache = 0x00000200,
            WriteCombine = 0x00000400
        }

        //00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F 
        //10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F
        //20 21 22 ……

        //根据进程名获取PID
        public static int GetPidByProcessName(string processName)
        {
            Process[] arrayProcess = Process.GetProcessesByName(processName);

            foreach (Process p in arrayProcess)
            {
                return p.Id;
            }
            return 0;
        }


        //遍历进程加载的Dll信息
        public static IntPtr GetDllAddre(string ProcessName, string DllName)
        {
            IntPtr handle = CreateToolhelp32Snapshot(8, (uint)War.PId);
            IntPtr GameDllAddre = IntPtr.Zero;

            MODULEENTRY32 Module32 = new MODULEENTRY32();
            if (handle != IntPtr.Zero)
            {
                Module32.dwSize = (uint)1024;
                int Mhandle = Module32First(handle, ref Module32);
                while (Mhandle != 0)
                {
                    Mhandle = Module32Next(handle, ref Module32);

                    if (Module32.szModule.ToLower().Equals(DllName.ToLower()))
                    {
                        GameDllAddre = Module32.hModule;
                        break;
                    }
                }
                if (GameDllAddre == IntPtr.Zero && War.HWnd != IntPtr.Zero) 
                {
                    GetWarVersion.GetModules(GetWarVersion.GetProcess(ProcessName).Handle, DllName);
                    GameDllAddre = GetWarVersion.dllBaseInfo.BaseAddress;
                }
            }

            CloseHandle(handle);

            return GameDllAddre;
        }


        //读byte数组内存
        public static byte[] ReadMemoryValueBYTE(int baseAddress, string processName)
        {
            try
            {
                byte[] buffer = new byte[4];

                //内存操作权限(最高0x1F0FFF)
                int PROCESS_ALL_ACCESS = 0x1F0FFF;    

                //第三个参数 获取PID的方法
                IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, War.PId);

                //将制定内存中的值读入缓冲区 
                ReadProcessMemory(hProcess, (IntPtr)((int)War.BaseAddre + baseAddress), buffer, buffer.Length, 0);

                CloseHandle(hProcess);

                return buffer;
            }
            catch
            {
                return new byte[4];
            }
        }

        //读整数内存
        public static int ReadMemoryValueINT(int baseAddress, string processName)
        {
            try
            {
                byte[] buffer = new byte[4];

                //内存操作权限(最高0x1F0FFF)
                int PROCESS_ALL_ACCESS = 0x1F0FFF;

                //获取缓冲区地址 
                IntPtr byteAddress = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

                //打开进程权限
                IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, War.PId);

                //将制定内存中的值读入缓冲区 
                ReadProcessMemory(hProcess, (IntPtr)((int)War.BaseAddre + baseAddress), byteAddress, 4, IntPtr.Zero);

                CloseHandle(hProcess);

                return Marshal.ReadInt32(byteAddress);
            }
            catch
            {
                return 0;
            }
        }

        //写入内存
        public static void patch(int BaseAddress,  string strBuffer)
        {
            int DllAddr = (int)War.BaseAddre;

            int GradeBaseAddress = DllAddr + BaseAddress;
            int RealSize;
            IntPtr hProcess = IntPtr.Zero;
            try
            {
                int PROCESS_ALL_ACCESS = 0x1F0FFF;
                hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, War.PId);

                uint oldpro = 0;
                uint PAGE_EXECUTE_READWRITE = 0x40;
                //改变对WAR内存的保护
                VirtualProtectEx(hProcess, (IntPtr)(DllAddr + 0x01000), (UIntPtr)0x87E000, PAGE_EXECUTE_READWRITE, out oldpro);

                if (strBuffer.IndexOf('/') != -1)
                {
                    int BufferLength = strBuffer.Split('/').Length;
                    byte[] BufferValue = new byte[BufferLength];

                    for (int i = 0; i < BufferLength; i++)
                    {
                        string Buffer = strBuffer.Split('/')[i];
                        byte Value = Convert.ToByte("0" + Buffer, 16);
                        BufferValue[i] = Value;
                        if (!WriteProcessMemory(hProcess, GradeBaseAddress, BufferValue, BufferValue.Length, out RealSize)) {
                            System.Windows.Forms.MessageBox.Show("写入失败:" + BaseAddress.ToString());
                        }
                    }
                }
                else 
                {
                    byte Value = Convert.ToByte("0" + strBuffer, 16);
                    byte[] BufferValue = new byte[1];
                    BufferValue[0] = Value;
                    if (!WriteProcessMemory(hProcess, GradeBaseAddress, BufferValue, BufferValue.Length, out RealSize)) 
                    {
                        System.Windows.Forms.MessageBox.Show("写入失败:" + BaseAddress.ToString());
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

      

        //1.20E版本
        public static void SetWriteMemoryOneTwoE()
        {
            //大地图显示单位   
            patch(0x2A0930, "xD2");
            
            //分辨幻影   
            patch(0x1ACFFC, "x40/xC3");

            //显示神符   
            patch(0x2A07C5, "x49/x4B/x33/xDB/x33/xC9");

            //小地图去除迷雾   
            patch(0x147C53, "xEC");

            //显示单位   
            patch(0x1491A8, "x00");

            //显示隐形   
            patch(0x1494E0, "x33/xC0/x0F/x85");

            //敌方信号   
            patch(0x321CC4, "x39/xC0/x0F/x85");
            patch(0x321CD7, "x39/xC0/x75");

            //他人提示   
            patch(0x124DDD, "x39/xC0/x0F/x85");

            //盟友头像   
            //patch(0x137BA5, "xE7/x7D");
            //patch(0x137BB1, "xEB/xCE/x90/x90/x90/x90");

            //建筑显资源  
            patch(0x13EF03, "xEB");

            //允许交易   
            patch(0x127B3D, "x40/xB8/x64");

            //显示技能   
            patch(0x12DC1A, "x33");
            patch(0x12DC1B, "xC0");
            patch(0x12DC5A, "x33");
            patch(0x12DC5B, "xC0");
            patch(0x1BFABE, "xEB");
            patch(0x442CC0, "x90");
            patch(0x442CC1, "x40");
            patch(0x442CC2, "x30");
            patch(0x442CC3, "xC0");
            patch(0x442CC4, "x90");
            patch(0x442CC5, "x90");
            patch(0x443375, "x30");
            patch(0x443376, "xC0");
            patch(0x45A641, "x90");
            patch(0x45A642, "x90");
            patch(0x45A643, "x33");
            patch(0x45A644, "xC0");
            patch(0x45A645, "x90");
            patch(0x45A646, "x90");
            patch(0x45E79E, "x90");
            patch(0x45E79F, "x90");
            patch(0x45E7A0, "x33");
            patch(0x45E7A1, "xC0");
            patch(0x45E7A2, "x90");
            patch(0x45E7A3, "x90");
            patch(0x466527, "x90");
            patch(0x466528, "x90");
            patch(0x46B258, "x90");
            patch(0x46B259, "x33");
            patch(0x46B25A, "xC0");
            patch(0x46B25B, "x90");
            patch(0x46B25C, "x90");
            patch(0x46B25D, "x90");
            patch(0x4A11A0, "x33");
            patch(0x4A11A1, "xC0");
            patch(0x54C0BF, "x90");
            patch(0x54C0C0, "x33");
            patch(0x54C0C1, "xC0");
            patch(0x54C0C2, "x90");
            patch(0x54C0C3, "x90");
            patch(0x54C0C4, "x90");
            patch(0x5573FE, "x90");
            patch(0x5573FF, "x90");
            patch(0x557400, "x90");
            patch(0x557401, "x90");
            patch(0x557402, "x90");
            patch(0x557403, "x90");
            patch(0x55E15C, "x90");
            patch(0x55E15D, "x90");

            //资源条   
            patch(0x150981, "xEB/x02");
            patch(0x1509FE, "xEB/x02");
            patch(0x151597, "xEB/x02");
            patch(0x151647, "xEB/x02");
            patch(0x151748, "xEB/x02");
            patch(0x1BED19, "xEB/x02");
            patch(0x314A9E, "xEB/x02");
            patch(0x21EAD4, "xEB");
            patch(0x21EAE8, "x03");

            //野外显血   
            patch(0x166E5E, "x90/x90/x90/x90/x90/x90/x90/x90");
            patch(0x16FE0A, "x33/xC0/x90/x90");

            //无限取消   
            patch(0x23D60F, "xEB");
            patch(0x21EAD4, "x03");
            patch(0x21EAE8, "x03");

            //防秒
            //patch(0x704BB0, "x83/xF9/x00/xF/x85/x3/x0/x0/x0/x6A/x3/x59/x83/xF9/x12/xF/x87/xEF/x87/x83/xFF/xE9/x4B/x86/xB3/xFF");
            //patch(0x23D20C, "xE9/x9F/x79/x4C/x00");

            //特殊
            //过检测


            ////设置单位动作
            //patch(0x2C1E10, "xC3/x90/x90");
            
            ////用户控制强制关闭
            //patch(0x2D3300, "xC3/x90/x90");

            ////防选单位 -过MH
            //patch(0x2C5A7E, "x90/x90");

            ////防清选
            //patch(0x2C5AB0, "xC3");
        }




        //1.24E
        public static void SetWriteMemoryOneFourE()
        {
            //大地图显示单位
            patch(0x39EBBC, "x70");
            patch(0x3A2030,"x90/x90");
            patch(0x3A20DB,"x90/x90");

            //分辨幻想   
            patch(0x28357C, "x40");
            patch(0x28357D, "xC3");

            //显示物品神符   
            patch(0x3A201B, "xEB");
            patch(0x40A864, "x90");
            patch(0x40A865, "x90");

            //清除小地图迷雾
            patch(0x357065, "x90/x90");

            //显示小地图单位
            patch(0x361F7C, "x00");

            //敌方信号
            patch(0x43F9A6, "x3B");
            patch(0x43F9A9, "x85");
            patch(0x43F9B9, "x3B");
            patch(0x43F9BC, "x85");

            //小地图他人提示
            patch(0x3345E9, "x39/xC0/x0F/x85");

            //显示资源
            patch(0x36058A, "x90");
            patch(0x36058B, "x90");

            //建筑规模
            patch(0x34E8E2, "xB8");
            patch(0x34E8E3, "xC8");
            patch(0x34E8E4, "x00");
            patch(0x34E8E5, "x00");
            patch(0x34E8E7, "x90");
            patch(0x34E8EA, "xB8");
            patch(0x34E8EB, "x64");
            patch(0x34E8EC, "x00");
            patch(0x34E8ED, "x00");
            patch(0x34E8EF, "x90");


            //显示技能
            patch(0x2031EC, "x90");
            patch(0x2031ED, "x90");
            patch(0x2031EE, "x90");
            patch(0x2031EF, "x90");
            patch(0x2031F0, "x90");
            patch(0x2031F1, "x90");
            patch(0x34FDE8, "x90");
            patch(0x34FDE9, "x90");

            //显示冷却时间
            patch(0x28ECFE, "xEB");
            patch(0x34FE26, "x90");
            patch(0x34FE27, "x90");
            patch(0x34FE28, "x90");
            patch(0x34FE29, "x90");

            //无限取消
            patch(0x57BA7C, "xEB");
            patch(0x5B2D77, "x03");
            patch(0x5B2D8B, "x03");


            //数显移速
            patch(0x87EA63, "x25/x30/x2e/x32/x66/x7c/x52/x00");
            patch(0x87EA70, "x8d/x4c/x24/x18/xd9/x44/x24/x60/x83/xec/x08/xdd/x1c/x24/x68");
            int tmp = 0x87EA63 + (int)GetDllAddre("War3","game.dll");
           
            byte[] tmpB = BitConverter.GetBytes((int)(new IntPtr(tmp)));
            
            string temC = "";
            for(int i=0;i<tmpB.Length;i++)
            {
                if (i == tmpB.Length - 1)
                {
                    temC += "x" + Convert.ToString(tmpB[i], 16).ToString();
                }
                else 
                {
                    temC += "x" + Convert.ToString(tmpB[i], 16).ToString() + "/";
                }
                
            }

            patch(0x87EA7F, temC);
            patch(0x87EA83, "x57/x51/xe8/xbc/xd2/xe6/xff/x83/xc4/x14/x58/x57/x8d/x4c/x24/x18/xff/xe0");
            patch(0x339C54, "xe8/x17/x4e/x54/x00");


            //数显攻速
            patch(0x87EA63,"x25/x30/x2e/x32/x66/x7c/x52/x00");
            patch(0x87EA70,"x8d/x4c/x24/x18/xd9/x44/x24/x60/x83/xec/x08/xdd/x1c/x24/x68");
            tmp = 0x87EA63 + (int)GetDllAddre("War3", "game.dll");

            tmpB = BitConverter.GetBytes((int)(new IntPtr(tmp)));

            temC = "";
            for (int i = 0; i < tmpB.Length; i++)
            {
                temC += "x" + Convert.ToString(tmpB[i], 16) + "/";
            }

            patch(0x87EA7F, temC);
            patch(0x87EA83, "x57/x51/xe8/xbc/xd2/xe6/xff/x83/xc4/x14/x58/x57/x8d/x4c/x24/x18/xff/xe0");
            patch(0x339DF4, "xe8/x77/x4c/x54/x00");


            //防秒
            //patch(0x87EFBC, "x83/xF9/x00/x75/x03/x6A/x03/x59/x83/xF9/x12/x0F/x87/xA5/x85/xD3/xFF/xE9/x11/x84/xD3/xFF");
            //patch(0x5B73DA, "xE9/xDD/x7B/x2C/x00");

            //建筑显资
            patch(0x360584, "xEB");


            //过检测

            ////设置单位动作
            //patch(0x3C6CE0, "xC3");

            ////房用户强制关闭
            //patch(0x3B43C0, "xC3/x90");

            ////防选单位 过-MH
            //patch(0x3C84C7, "xEB/x11");
            //patch(0x3C84E7, "xEB/x11");

            ////防清选
            //patch(0x3BC5E0, "xC3");

        }

        //1.24B
        public static void SetWriteMemoryOneFourB() 
        {
            //小地图显示单位   
            patch(0x3A201D, "xEB");

            //分辨幻影   
            patch(0x28351C, "x40/xC3");

            //显示神符   
            patch(0x4076CA, "x90/x90");
            patch(0x3A1F5B, "xEB");

            //小地图去除迷雾   
            patch(0x356FA5, "x90/x90");

            //小地图显隐形 
            patch(0x361EAB, "x90/x90/x39/x5E/x10/x90/x90/xB8/x00/x00/x00/x00/xEB/x07");

            //小地图显示隐形   
            patch(0x361EBC, "x00");

            //敌方信号   
            patch(0x43F956, "x3B");
            patch(0x43F959, "x85");
            patch(0x43F969, "x3B");
            patch(0x43F96C, "x85");

            //他人提示   
            patch(0x334529, "x39/xC0/x0F/x85");

           //资源面板   
            patch(0x3604CA, "x90/x90");

            //允许交易   
            patch(0x34E822, "xB8/xE0/x03/x00");
            patch(0x34E827, "x90");
            patch(0x34E82A, "xB8/x64/x90/x90");
            patch(0x34E82F, "x90");

            //查看技能   
            patch(0x28EC8E, "xEB");
            patch(0x20318C, "x90/x90/x90/x90/x90/x90");
            patch(0x34FD28, "x90/x90");
            patch(0x34FD66, "x90/x90/x90/x90");

            //无限取消   
            patch(0x57B9FC, "xEB");
            patch(0x5B2CC7, "x03");
            patch(0x5B2CDB, "x03");

            //防秒
            //patch(0x87EEFC, "x83/xF9/x00/x75/x03/x6A/x03/x59/x83/xF9/x12/x0F/x87/xB5/x85/xD3/xFF/xE9/x21/x84/xD3/xFF");
            //patch(0x5B732A, "xE9/xCD/x7B/x2C/x00");

            //建筑显资
            patch(0x3604C4, "xEB");

            ////设置单位动作
            //patch(0x3C6C21, "xC3/x90/x90/x90");

            ////防用户强制关闭
            //patch(0x3B4300, "xC3/x90");

            ////防清选单位 过-MH   
            //patch(0x3C8407, "xEB/x11");
            //patch(0x3C8427, "xEB/x11");

        }

        //120E安全选项xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static void DisplayInvisible120E() 
        {
            //大地图显示隐形   
            patch(0x17D4C2, "x90/x90");
            patch(0x17D4CC, "xEB/x00/xEB/x00/x75/x30");
        }


        //public static void HostileFace120E() 
        //{
        //    //显示敌方头像   
        //    patch(0x137BA5, "xE7/x7D");
        //    patch(0x137BAC, "x85/xA3/x02/x00/x00/xEB/xCE/x90/x90/x90/x90");
        //}

        public static void ClearFog120E()
        {
            //大地图清除迷雾   
            patch(0x406B53, "x90/x8B/x09");
        }


        public static void FogSelected120E() 
        {
            //视野外点选   
            patch(0x1BD5A7, "x90/x90");
            patch(0x1BD5BB, "xEB");
        }

        public static void PassAH120E() 
        {
            //反-AH   
            patch(0x2C240C, "x3C/x4C/x74/x04/xB0/xFF/xEB/x04/xB0/xB0/x90/x90");
            patch(0x2D34ED, "xE9/xB3/x00/x00/x00/x90");
        }
        //120E安全选项xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


        //124E安全选项xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static void DisplayInvisible124E() 
        {
            ////小地图显隐形
            //patch(0x285CD2, "xEB");

            patch(0x39EBBC, "x75");
            patch(0x3A2030, "x90/x90");
            patch(0x3A20DB, "x90/x90");

            patch(0x362391, "x3B");
            patch(0x362394, "x85");
            patch(0x39A51B, "x90/x90/x90/x90/x90");
            patch(0x39A520, "x90");
            patch(0x39A52E, "x90/x90");
            patch(0x39A530, "x90/x90/x90/x90/x90/x90/x33/xC0/x40");  
        }

        //public static void HostileFace124E()
        //{
        //    //盟友头像   
        //    patch(0x371640, "xE8/x3B/x28/x03/x00/x85/xC0/x0F/x84/x8F/x02/x00/x00/xEB/xC9/x90/x90/x90/x90");
        //}

        public static void ClearFog124E()
        {
            //清除迷雾
            patch(0x74D1B9, "xB2/x00/x90/x90/x90/x90");
        }

        public static void FogSelected124E()
        {
            //视野外选择单位
            patch(0x285CBC, "x90");
            patch(0x285CBD, "x90");
            patch(0x285CD2, "xEB");
        }

        public static void PassAH124E()
        {
            //反-AH
            patch(0x3C6EDC, "xB8/xFF/x00/x00/x00/xEB");
            patch(0x3CC3B2, "xEB");
        }

        //124B安全选项xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static void DisplayInvisible124B()
        {
            //大小地图显隐形
            patch(0x3622D1, "x3B");
            patch(0x3622D4, "x85");
            patch(0x39A45B, "x90/x90/x90/x90/x90/x90");
            patch(0x39A46E, "x90/x90/x90/x90/x90/x90/x90/x90/x33/xC0/x40");
        }

        //public static void HostileFace124B()
        //{
        //    //敌人头像   
        //    patch(0x371640, "xE8/x3B/x28/x03/x00/x85/xC0/x0F/x85/x8F/x02/x00/x00/xEB/xC9/x90/x90/x90/x90");
        //}

        public static void ClearFog124B()
        {
            //大地图清除迷雾   
            patch(0x74D103, "xC6/x04/x3E/x01/x90/x46");
        }


        public static void FogSelected124B()
        {
            //资源条 野外显血 视野外点击   
            patch(0x285C4C, "x90/x90");
            patch(0x285C62, "xEB");
        }

        public static void PassAH124B()
        {
            //反-AH   
            patch(0x3C6E1C, "xB8/xFF/x00/x00/x00/xEB");
            patch(0x3CC2F2, "xEB");
        }

        public static void ScreenBad120E() 
        {
            //防切屏
            patch(0x2DD4B0, "xC3/x90/x90");
        }

        public static void ScreenBad124E() 
        {
            //防切屏
            patch(0x3B5280, "xC3/x90");
        }

        public static void ScreenBad124B() 
        {
            //防切屏
            patch(0x3B51C0, "xC3/x90");
        }



        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx安全选项over


        //改变魔兽视野120E
        public static void ChangeVision120E(int scope) 
        {
            LensScope(0x0FDF7E, scope);
        }

        //改变魔兽视野124B,1.24E
        public static void ChangeVision124BE(int scope)
        {
            LensScope(0x9485BC, scope);
        }


        //视野调整
        public static void LensScope(int GradeBaseAddress,int value) 
        {
            int DllAddr = (int)War.BaseAddre;
            
            IntPtr hProcess = IntPtr.Zero;
            int PROCESS_ALL_ACCESS = 0x1F0FFF;
            hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, War.PId);

            uint oldpro = 0;
            uint PAGE_EXECUTE_READWRITE = 0x40;

            //改变对WAR内存的保护
            VirtualProtectEx(hProcess, (IntPtr)(DllAddr + 0x01000), (UIntPtr)0x87E000, PAGE_EXECUTE_READWRITE, out oldpro);

            uint[] oldProtect = new uint[2];
            Protection protection = Protection.ExecuteWriteCopy;

            IntPtr address = (IntPtr)(DllAddr + GradeBaseAddress);

            VirtualProtectEx(hProcess, address, 4, Convert.ToUInt32(protection), ref oldProtect[0]);

            byte[] distance = BitConverter.GetBytes(float.Parse(value.ToString()));

            WriteProcessMemory(hProcess, address, distance, distance.Length, 0);

            VirtualProtectEx(hProcess, address, 4, oldProtect[0], ref oldProtect[1]);

            CloseHandle(hProcess);
        }
    }
}
