using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WjeWar
{
    class GetWarVersion
    {
        //获得War Game.dll版本信息
        public static string GetVersion(string processName,string dllName)
        {
            string version = "";
            Process process = GetProcess(processName);
            ProcessModuleCollection modules = null;
            try
            {
                modules = process.Modules;
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            bool isGet = false;
            foreach (ProcessModule mod in modules) 
            {
                if (mod.ModuleName.ToLower() == dllName)
                {
                    version = mod.FileVersionInfo.FileVersion.Replace(", ", ".");
                    version = SimpleVersion(version, ref isGet);
                }
            }

            if (!isGet)
            {
                GetModules(process.Handle, dllName);
                fileName = DeviceName2Path(dllBaseInfo.path);
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(fileName);
                version = fvi.FileVersion.Replace(", ", ".");
                version = SimpleVersion(version, ref isGet);
            }

            return version;
        }

        //获得执行路径
        public static string GetUrPath(string processName) 
        {
            string fileName = "";
            Process myprocess = Process.GetProcessById(War.PId);
            fileName = myprocess.MainModule.FileName;

            return fileName;
        }

        //解析版本
        public static string SimpleVersion(string version,ref bool isGet) 
        {
            switch (version)
            {
                case "1.20.4.6074": isGet = true; return "1.20E";
                case "1.24.4.6387": isGet = true; return "1.24E";
                case "1.24.1.6374": isGet = true; return "1.24B";
            }
            return "Error";
        }

        //返回进程对象
        public static Process GetProcess(string processName) 
        {
            Process process = Process.GetProcessById(War.PId);
            return process;
        }


        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        [DllImport("ntdll.dll", SetLastError = true)]
        public extern static int ZwQueryVirtualMemory(IntPtr ProcessHandle, int BaseAddress, MemoryInformationClass _MemoryInformationClass, IntPtr MemoryInformation, Int32 MemoryInformationLength, out int ReturnLenth);
        
        [DllImport("ntdll.dll", SetLastError = true)]
        public unsafe extern static int ZwQueryVirtualMemory(IntPtr ProcessHandle, int BaseAddress, MemoryInformationClass _MemoryInformationClass, [Out] void* mbi, Int32 MemoryInformationLength, out int Zero);
       
        [DllImport("ntdll.dll", SetLastError = true)]
        public extern static int ZwQueryVirtualMemory(IntPtr ProcessHandle, int BaseAddress, MemoryInformationClass _MemoryInformationClass, [Out] out MEMORY_SECTION_NAME mbi, Int32 MemoryInformationLength, out int Zero);
        [DllImport("ntdll.dll", SetLastError = true)]
        public extern static int ZwQueryVirtualMemory(IntPtr ProcessHandle, int BaseAddress, MemoryInformationClass _MemoryInformationClass, [Out] byte[] MSN, Int32 MemoryInformationLength, out int Zero);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int QueryDosDevice(string lpDeviceName, StringBuilder lpTargetPath, int ucchMax);

        public enum MbiType
        {
            MEM_IMAGE = 0x1000000, MEM_MAPPED = 0x40000, MEM_PRIVATE = 0x20000
        }

        public struct MemoryBasicInformation
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public UInt32 AllocationProtect;
            public IntPtr RegionSize;
            public UInt32 State;
            public UInt32 Protect;
            public UInt32 lType;
        }


        public enum MemoryInformationClass
        {
            MemoryBasicInformation,
            MemoryWorkingSetList,
            MemorySectionName,
            MemoryBasicVlmInformation
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_SECTION_NAME
        {
            public UNICODE_STRING usstring;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 520)]
            public byte[] bt;
        }



        [StructLayout(LayoutKind.Sequential)]
        public struct UNICODE_STRING : IDisposable
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr buffer;


            public UNICODE_STRING(string s)
            {
                Length = (ushort)(s.Length * 2);
                MaximumLength = (ushort)(Length + 2);
                buffer = Marshal.StringToHGlobalUni(s);
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }

            public override string ToString()
            {
                return Marshal.PtrToStringUni(buffer);
            }
        }

        public static string DeviceName2Path(string sbProcImagePath)
        {
            int iRet;
            string strImageFilePath = "";
            if (sbProcImagePath.Length > 0)
            {
                int iDeviceIndex = sbProcImagePath.ToString().IndexOf("\\", "\\Device\\HarddiskVolume".Length);
                string strDevicePath = sbProcImagePath.ToString().Substring(0, iDeviceIndex);
                int iStartDisk = (int)'A';
                while (iStartDisk <= (int)'Z')
                {
                    StringBuilder sbWindowImagePath = new StringBuilder(256);
                    iRet = QueryDosDevice(((char)iStartDisk).ToString() + ":", sbWindowImagePath, sbWindowImagePath.Capacity);
                    if (iRet != 0)
                    {
                        if (sbWindowImagePath.ToString() == strDevicePath)
                        {
                            strImageFilePath = ((char)iStartDisk).ToString() + ":" + sbProcImagePath.ToString().Replace(strDevicePath, "");
                            break;
                        }
                    }
                    iStartDisk++;
                }
            }
            return strImageFilePath;
        }

        static int baseAddress;
        static string fileName;

        public int BaseAddress
        {
            get { return baseAddress; }
            set { baseAddress = value; }
        }

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public static DllBaseInfo dllBaseInfo;

        public unsafe static void GetModules(IntPtr ProcessHandle, string dllName)
        {
            MemoryBasicInformation mbi = new MemoryBasicInformation();
            MEMORY_SECTION_NAME usSectionName = new MEMORY_SECTION_NAME();
            int dwStartAddr = 0x00000000;

            do
            {
                int rt1 = 0;
                if (ZwQueryVirtualMemory(ProcessHandle, dwStartAddr, MemoryInformationClass.MemoryBasicInformation, &mbi, Marshal.SizeOf(mbi), out rt1) >= 0)
                {
                    if (mbi.lType == (int)MbiType.MEM_IMAGE)
                    {
                        byte[] bt = new byte[260 * 2];
                        int rt = 0;
                        int result = ZwQueryVirtualMemory(ProcessHandle, dwStartAddr, MemoryInformationClass.MemorySectionName, out usSectionName, bt.Length, out rt);

                        if (result >= 0 )
                        {
                            UnicodeEncoding une = new UnicodeEncoding();
                            string path = une.GetString(usSectionName.bt).TrimEnd('\0');
                            if (path.Trim().ToLower().LastIndexOf(dllName) != -1) 
                            {
                                dllBaseInfo.BaseAddress = mbi.AllocationBase;
                                dllBaseInfo.path = path;
                                break;
                            }
                        }
                        else 
                        {
                            break;
                        }
                        dwStartAddr += (int)mbi.RegionSize;
                        dwStartAddr -= ((int)mbi.RegionSize % 0x10000);
                    }
                }
                dwStartAddr += 0x10000;
            } while (dwStartAddr < 0x7FFEFFFF);
        }

        public struct DllBaseInfo
        {
            public IntPtr BaseAddress;
            public string path;
        }
    }
}
