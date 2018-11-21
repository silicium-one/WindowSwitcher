using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WindowSwitcher
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            else
            {
                try
                {
                    var hWnds = new List<IntPtr>();
                    var interval = int.Parse(args[0]);
                    for (int i = 1; i < args.Length; i ++)
                    {
                        var hWnd = WindowsOperationClass.FindWindow(null, args[i]);
                        if (hWnd == IntPtr.Zero)
                        {
                            Console.WriteLine(@"Окно с заголовком ""{0}"" не найдено", args[i]);
                        }
                        else
                        {
                            hWnds.Add(hWnd);
                        }
                    }

                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new MainForm(interval, hWnds));
                }
                catch
                {
                    Console.WriteLine(@"Первый аргумент командной строки - количество секунд, далее названия окон в кавычках");
                }
            }
        }
    }
}
