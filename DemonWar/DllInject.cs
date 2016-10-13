using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace WjeWar
{
    class DllInject
    {

        //----------------------------------------DLL注入的API-------------------------------------------

        [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx")] 
        private static extern int VirtualAllocEx(IntPtr hwnd, int lpaddress, int size, int type, int tect);
        
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        public static extern int GetProcAddress(int hwnd, string lpname);
        
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleA")]
        private static extern int GetModuleHandleA(string name);
        
        [DllImport("kernel32.dll", EntryPoint = "CreateRemoteThread")]
        private static extern int CreateRemoteThread(IntPtr hwnd, int attrib, int size, int address, int par, int flags, int threadid);

        /// <summary>动态加载DLL
        /// 
        /// </summary>
        /// <param name="DllName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        public static extern IntPtr LoadLibrary(string DllName);

        /// <summary>使用加载的Dll函数
        /// 
        /// </summary>
        /// <param name="Dllhandle"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        public static extern IntPtr GetProcAddress(IntPtr Dllhandle, string FunctionName);

        /// <summary>等待内核对象信号
        /// 
        /// </summary>
        /// <param name="hHandle"></param>
        /// <param name="dwMilliseconds"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject")]
        private static extern int WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);



        //------------------------------------------------------------------------------------------------

        private delegate bool HaveFun();

        public static bool ManaStart(string dllname,bool isMana) 
        {
            bool IsHaveFun = true;
            IntPtr Handle = (IntPtr)0;
            string filePath = "";
            int baseaddress;
            int temp = 0;
            int Kernddr;
            int yan;
            bool ManaState = true;

            int dlllength;
            dlllength = dllname.Length + 1;

            Process[] process = Process.GetProcessesByName(War.ProcessName);
            
            Handle = process[0].Handle;
            filePath = War.Path;

            baseaddress = VirtualAllocEx(Handle, 0, dlllength, 4096, 4); //申请内存空间 

            WriteMemory.WriteProcessMemory(Handle, baseaddress, dllname, dlllength, temp); //写内存 

            Kernddr = GetProcAddress(GetModuleHandleA("Kernel32"), "LoadLibraryA"); //取得loadlibarary在kernek32.dll地址

            yan = CreateRemoteThread(Handle, 0, 0, Kernddr, baseaddress, 0, temp); //创建远程线程。

            if (yan != 0)
            {
                ManaState = true;
            }

            if (ManaState && isMana) 
            {
                byte[] manaByte = WjeWar.Properties.Resources.mana;

                
                if (!System.IO.File.Exists(filePath + "\\" + dllname))
                {
                    System.IO.FileStream fs = new System.IO.FileStream(filePath + "\\" + dllname, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
                    fs.Write(manaByte, 0, manaByte.Length);
                    fs.Flush();
                    fs.Close();
                }

                IntPtr ManaDll = LoadLibrary(filePath + "\\" + dllname);

                if (ManaDll != IntPtr.Zero)
                {
                    IntPtr api = GetProcAddress(ManaDll, "HaveFun");
                    try
                    {
                        HaveFun HaveFun = (HaveFun)(Delegate)Marshal.GetDelegateForFunctionPointer(api, typeof(HaveFun));
                        IsHaveFun = HaveFun();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                    }
                    
                }
            }

            return IsHaveFun;
        }


        public static bool inject(byte[] fileByte , string proName ,string path , string dllname)
        {
            const UInt32 INFINITE = 0xFFFFFFFF;
            const Int32 PAGE_EXECUTE_READWRITE = 0x40;
            const Int32 MEM_COMMIT = 0x1000;
            const Int32 MEM_RESERVE = 0x2000;
            Int32 AllocBaseAddress;

            string dllPath = path + "\\" + dllname;

            if (!System.IO.File.Exists(dllPath))
            {
                FileManage.FileCreate(fileByte, path, dllname);
            }

            Process[] process = Process.GetProcessesByName(proName);
            IntPtr hWnd = process[0].Handle;

            int umstrcnt = Encoding.Default.GetByteCount(dllPath);

            AllocBaseAddress = VirtualAllocEx(hWnd, 0, umstrcnt, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

            IntPtr AddrWM = Marshal.StringToHGlobalAnsi(dllPath);

            int readSize;
            bool isWrite = WriteMemory.WriteProcessMemory(hWnd, AllocBaseAddress, (int)AddrWM, umstrcnt, out readSize);

            Marshal.FreeHGlobal(AddrWM);

            int loadaddr = GetProcAddress(GetModuleHandleA("kernel32.dll"), "LoadLibraryA");

            IntPtr ThreadHwnd = (IntPtr)CreateRemoteThread(hWnd, 0, 0, loadaddr, AllocBaseAddress, 0, 0);

            WaitForSingleObject(ThreadHwnd, INFINITE);
        
            return true;
        }
    }
}
