using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace WjeWar
{
    static class Program
    {
        


        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool runone;
            System.Threading.Mutex run = new System.Threading.Mutex(true, "Wje", out runone);
            if (runone)
            {
              run.ReleaseMutex();
              Application.EnableVisualStyles();
              Application.SetCompatibleTextRenderingDefault(false);
              Application.Run(new FM_DemonWar());
            }
            else
            {
                FM_DemonWar.SetForegroundWindow(Api.FindWindow(null, new FM_DemonWar().Text));
            }

        }
    }
}