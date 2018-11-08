using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DotNetBrowser;
using DotNetBrowser.WPF;
using System.Windows.Threading;
using DotNetBrowser.Events;
using System.Threading;
using System.Windows.Interop;
using System.IO;


namespace finWeb
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        public int CurrentItemId=0;
        public List<WPFBrowserView> Browsers = new List<WPFBrowserView>();
        public List<TabItem> Pages = new List<TabItem>();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            
            var helper = new WindowInteropHelper(this);
            const uint VK_F10 = 0x79;
            const uint MOD_CTRL = 0x0002;
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CTRL, VK_F10))
            {
                // handle error
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OnHotKeyPressed();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed()
        {
            // do stuff
            if (BrowserChr.ResizeMode == ResizeMode.NoResize)
            {
                MessageBox.Show("Выход из полноэкранного режима");
                BrowserChr.ResizeMode = ResizeMode.CanResize;
                BrowserChr.WindowState = WindowState.Normal;
                BrowserChr.WindowStyle = WindowStyle.SingleBorderWindow;
                BrowserChr.Width = 1175;
                BrowserChr.Height = 442;

            }
            else if (BrowserChr.ResizeMode == ResizeMode.CanResize)
            {
                MessageBox.Show("Полноэкранный режим включен");
                BrowserChr.ResizeMode = ResizeMode.NoResize;
                BrowserChr.WindowStyle = WindowStyle.None;
                BrowserChr.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                BrowserChr.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                BrowserChr.WindowState = WindowState.Maximized;
            }
        }
        public string a = " ";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            
        }



        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();
            txtUrl.TextWrapping = TextWrapping.NoWrap;



        }

 

        private void BtnGO_OnClick(object sender, RoutedEventArgs e)
        {
            Browser.URL = txtUrl.Text;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           
        }

        private void BrowserChr_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            tbControl.Width = this.Width;
            tlBr.Width = this.Width;
            Grid1.Height = this.Height;
            Grid1.Width = this.Width;
            Browser.Height = BrowserChr.Height - 50;
            Browser.Width = BrowserChr.Width;
        }

        private void BrowserChr_MouseMove(object sender, MouseEventArgs e)
        {
            //tlBr.Width = this.Width;
            //Grid1.Height = this.Height;
            //Grid1.Width = this.Width;
            //Browser.Height = BrowserChr.Height - 50;
            //Browser.Width = BrowserChr.Width;
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            Browser.BackForwardNavigator.Forward();
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            txtUrl.Text = Browser.BackForwardNavigator.CurrentEntry.Url;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Browser.BackForwardNavigator.Back();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Browser.URL = Browser.URL;
        }

        private void Button_Click (object sender, RoutedEventArgs e)
        {
            Browser.Browser.Print();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            new History().Show();
        }

        private void Browser_FinishLoadingFrameEvent(object sender, FinishLoadingEventArgs e)
        {
            try
            {
                a = Browser.BackForwardNavigator.CurrentEntry.Url;
            }
            catch (Exception exception)
            {
                //Console.WriteLine(exception);
                //throw;
            }
            
            Thread thread = new Thread(urlUpdate);
            thread.Start();
            Thread thread1 = new Thread(tabUpdate);
            thread1.Start();
            Thread thread2 = new Thread(hisUpdate);
            thread2.Start();
        }

        private void Browser_StartLoadingFrameEvent(object sender, StartLoadingArgs e)
        {
            a = Browser.BackForwardNavigator.CurrentEntry.Url;
            Thread thread = new Thread(urlUpdate);
            thread.Start();
            Thread thread1 = new Thread(tabUpdate);
            thread1.Start();
            

        }

        private void urlUpdate()
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                txtUrl.Text = a;
            }));
        }
        private void tabUpdate()
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                tb1.Header = Browser.Browser.Title;
            }));
        }
        private void hisUpdate()
        {
            Thread.Sleep(TimeSpan.FromSeconds(0.1));
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //FileStream fstream = new FileStream(@"C:\\TEMP\\note.dat", FileMode.OpenOrCreate);
                System.IO.StreamWriter txt = new System.IO.StreamWriter(@"C:\\TEMP\\note.dat");
                string history = Browser.URL;
                string title = Browser.Browser.Title;
                txt.WriteLine(Title + " " + history + Environment.NewLine);
                txt.Close();
            }));
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnGO_OnClick(this, new RoutedEventArgs());
            }
        }


        //private void browserView_FinishLoadingFrameEvent(object sender, DotNetBrowser.Events.FinishLoadingEventArgs e)
        //{
        //    if (e.IsMainFrame)
        //    {
        //        String filePath = "C:\\SavedPages\\index.html";
        //        String dirPath = "C:\\SavedPages\\resources";
        //        Browser.Browser.SaveWebPage(filePath, dirPath, SavePageType.COMPLETE_HTML);
        //    }
        //}
        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            //if (e.Key == Key.F10)
            //{
            //    if (WindowState == WindowState.Maximized && !_inStateChange)
            //    {
            //        _inStateChange = true;
            //        WindowState = WindowState.Normal;
            //        WindowStyle = WindowStyle.None;
            //        WindowState = WindowState.Maximized;
            //        ResizeMode = ResizeMode.NoResize;
            //        _inStateChange = false;
            //    }

            //    base.OnStateChanged(e);
            //}
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {

            //if (e.Key == Key.F11)
            //{
            //    if (BrowserChr.ResizeMode == ResizeMode.NoResize)
            //    {
            //        MessageBox.Show("Выход из полноэкранного режима");
            //        BrowserChr.ResizeMode = ResizeMode.CanResize;
            //        BrowserChr.WindowState = WindowState.Normal;
            //        BrowserChr.WindowStyle = WindowStyle.SingleBorderWindow;
            //        BrowserChr.Width = 1175;
            //        BrowserChr.Height = 442;

            //    }
            //    else
            //    {
            //        MessageBox.Show("Полноэкранный режим включен");
            //        BrowserChr.ResizeMode = ResizeMode.NoResize;
            //        BrowserChr.WindowStyle = WindowStyle.None;
            //        BrowserChr.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //        BrowserChr.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            //        BrowserChr.WindowState = WindowState.Maximized;
            //    }
            //}
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.F11)
            //{
            //    if (BrowserChr.ResizeMode == ResizeMode.NoResize)
            //    {
            //        MessageBox.Show("Выход из полноэкранного режима");
            //        BrowserChr.ResizeMode = ResizeMode.CanResize;
            //        BrowserChr.WindowState = WindowState.Normal;
            //        BrowserChr.WindowStyle = WindowStyle.SingleBorderWindow;
            //        BrowserChr.Width = 1175;
            //        BrowserChr.Height = 442;

            //    }
            //    else if(BrowserChr.ResizeMode == ResizeMode.CanResize)
            //    {
            //        MessageBox.Show("Полноэкранный режим включен");
            //        BrowserChr.ResizeMode = ResizeMode.NoResize;
            //        BrowserChr.WindowStyle = WindowStyle.None;
            //        BrowserChr.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //        BrowserChr.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            //        BrowserChr.WindowState = WindowState.Maximized;
            //    }
            //}
        }

        private void tbControl_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.F11)
            //{
            //    if (BrowserChr.ResizeMode == ResizeMode.NoResize)
            //    {
            //        MessageBox.Show("Выход из полноэкранного режима");
            //        BrowserChr.ResizeMode = ResizeMode.CanResize;
            //        BrowserChr.WindowState = WindowState.Normal;
            //        BrowserChr.WindowStyle = WindowStyle.SingleBorderWindow;
            //        BrowserChr.Width = 1175;
            //        BrowserChr.Height = 442;

            //    }
            //    else
            //    {
            //        MessageBox.Show("Полноэкранный режим включен");
            //        BrowserChr.ResizeMode = ResizeMode.NoResize;
            //        BrowserChr.WindowStyle = WindowStyle.None;
            //        BrowserChr.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            //        BrowserChr.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            //        BrowserChr.WindowState = WindowState.Maximized;
            //    }
            //}
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void BtnNew_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
