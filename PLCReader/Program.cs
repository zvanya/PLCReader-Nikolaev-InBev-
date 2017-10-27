using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace PLCReader
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Mutex m = new Mutex(false, "ProcessMonitorPLCRead");

            if (!m.WaitOne(TimeSpan.FromSeconds(1), false))
            {
                MessageBox.Show("В системе запущен другой экземпляр программы!");
                return;
            }
            else
            {
                try
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm());
                }
                finally
                {
                    m.ReleaseMutex();
                }
            }
        }
    }
}
