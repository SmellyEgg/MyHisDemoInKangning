using SmellyEggPasswordManager.Controller;
using SmellyEggPasswordManager.Models;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmellyEggPasswordManager.Views
{
    /// <summary>
    /// MyLoginPage.xaml 的交互逻辑
    /// </summary>
    public partial class MyLoginPage : Page
    {
        private Frame _mainFrame;

        private LoginController _lcController;

        public MyLoginPage(Frame MainFrame)
        {
            InitializeComponent();
            Init(MainFrame);
        }

        private async void Init(Frame MainFrame)
        {
            _lcController = new LoginController();
            _mainFrame = MainFrame;
            //Window.GetWindow(_mainFrame).Width = this.Width;
            //Window.GetWindow(_mainFrame).Height = this.Height;
            await SetBingWallpaper();
        }

        private async Task SetBingWallpaper()
        {
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            ib.Opacity = 0.5;
            ImageController ic = new ImageController();

            var sourceStr = await Task.Run(() => ic.GetOnlineImageUrlAsync());

            var filename = DateTime.Now.Date.ToShortDateString();
            var path = System.AppDomain.CurrentDomain.BaseDirectory + "ImageCache\\" + filename + Path.GetExtension(sourceStr);
            if (!File.Exists(path))
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri(sourceStr), path);
                }
            }

            ib.ImageSource = new BitmapImage(new Uri(sourceStr)) { CacheOption = BitmapCacheOption.None }; ;
            MyGridMain.Background = ib;
        }

        private void RegisterClick(object sender, MouseButtonEventArgs e)
        {
            _mainFrame.Navigate(new MyRegisterPage(_mainFrame));
        }

        /// <summary>
        /// 按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void passwordKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonLoginClick(null, null);
            }
        }

        /// <summary>
        /// 登陆按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonLoginClick(object sender, RoutedEventArgs e)
        {
            
            if (!Valid())
            {
                txtTipsShow.Text = "请检查你的输入！";
            }
            else
            {
                ShowLoadingAnimation();
                User user = new User() { UserName = txtUserName.Text, UserPassword = txtPassword.Password};
                var result = await Task.Run(()=> _lcController.TryLogin(user));
                ShowLoadingAnimation(false);
                if (!object.Equals(result, null))
                {
                    //MessageBox.Show("登陆成功");
                    //跳转到密码管理界面
                    if (CmbType.SelectedIndex == (int)LoginType.passwordManager)
                    {
                        _mainFrame.Navigate(new PasswordPage(_mainFrame, result));
                    }
                    else if (CmbType.SelectedIndex == (int)LoginType.noteManager)
                    {
                        _mainFrame.Navigate(new NotePage(_mainFrame, result));
                    }
                }
                else
                {
                    txtTipsShow.Text = "账号或者密码不正确";
                }
            }
        }

        internal enum LoginType
        {
            passwordManager,
            noteManager
        }


        /// <summary>
        /// 显示等待动画
        /// </summary>
        /// <param name="isLoading"></param>
        private void ShowLoadingAnimation(bool isLoading = true)
        {
            if (isLoading)
            {
                myLoading.Visibility = Visibility.Visible;
                myLoading.Spin = true;
                IsEnabled = false;
            }
            else
            {
                IsEnabled = true;
                myLoading.Spin = false;
                myLoading.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// 启动登陆
        /// </summary>
        /// <returns></returns>
        private bool LoginProccess()
        {

            return true;
        }

        /// <summary>
        /// 验证有效性
        /// </summary>
        /// <returns></returns>
        private bool Valid()
        {
            if (string.IsNullOrEmpty(txtUserName.Text.Trim()) || string.IsNullOrEmpty(txtPassword.Password.Trim()) || CmbType.SelectedIndex == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private void txtPassword_KeyUp(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                txtTipsForPassword.Visibility = Visibility.Visible;
            }
            else
            {
                txtTipsForPassword.Visibility = Visibility.Hidden;
            }
        }
    }
}
