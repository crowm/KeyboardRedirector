#region Copyright (C) 2009 Nate

/* 
 *	Copyright (C) 2009 Nate
 *	http://nate.dynalias.net
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using MS;

namespace KeyboardRedirector
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (IsThereAnInstanceOfThisProgramAlreadyRunning(true))
            {
                Application.Exit();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Application.Run(new KeyboardRedirectorForm());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Log.LogException(ex);
        }

        static bool IsThereAnInstanceOfThisProgramAlreadyRunning(bool activateThePreviousInstance)
        {
            Process thisProcess = Process.GetCurrentProcess();
            Process[] processList = Process.GetProcessesByName(thisProcess.ProcessName);

            if (processList.Length == 1)
                return false; // There's just the current process.

            if (activateThePreviousInstance)
            {
                foreach (Process process in processList)
                {
                    if (process.Id != thisProcess.Id)
                    {
                        // Activate the previous instance.
                        List<IntPtr> windowHandles = ProcessWindowHandleObtainer.GetWindowHandle(process.Id);
                        foreach (IntPtr handle in windowHandles)
                        {
                            StringBuilder windowText = new StringBuilder(260);
                            Win32.GetWindowText(handle, windowText, 260);
                            if (windowText.ToString() == "Keyboard Redirector")
                            {
                                Win32.ShowWindow(handle, Win32.SW.RESTORE);
                                Win32.SetForegroundWindow(handle);
                            }
                        }
                        break;
                    }
                }
            }

            return true;
        }


    }

    class ProcessWindowHandleObtainer
    {
        public static List<IntPtr> GetWindowHandle(int processId)
        {
            ProcessWindowHandleObtainer obtainer = new ProcessWindowHandleObtainer();
            obtainer._processId = (uint)processId;
            Win32.EnumWindowsProc proc = new Win32.EnumWindowsProc(obtainer.EnumWindowsCallback);
            Win32.EnumWindows(proc, 0);
            return obtainer._windowHandles;
        }

        uint _processId = 0;
        List<IntPtr> _windowHandles = new List<IntPtr>();

        private bool EnumWindowsCallback(IntPtr hwnd, int lParam)
        {
            uint pid;
            Win32.GetWindowThreadProcessId(hwnd, out pid);
            if (pid == _processId)
            {
                _windowHandles.Add(hwnd);
            }
            return true;
        }
    }
}