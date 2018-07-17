using SmellyEggPasswordManager.Controller;
using SmellyEggPasswordManager.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmellyEggPasswordManager.Views
{
    /// <summary>
    /// MyRegisterPage.xaml 的交互逻辑
    /// </summary>
    public partial class MyRegisterPage : Page
    {
        private Frame MainFrame;

        private LoginController _lcController;

        public MyRegisterPage(Frame mainFrame)
        {
            InitializeComponent();
            _lcController = new LoginController();
            MainFrame = mainFrame;
        }

        private void backButtonClick(object sender, RoutedEventArgs e)
        {
            if (!object.Equals(MainFrame, null) && MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void txtUserKey_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ButtonRegister(null, null);
            }
        }

        private async void ButtonRegister(object sender, RoutedEventArgs e)
        {
            if (!Valid())
            {
                txtTipsShow.Text = "请检查你的输入";
            }
            else
            {
                ShowLoadingAnimation();
                User user = new User() { UserName = txtUserName.Text, UserPassword = txtPassword.Text, PasswordKey = txtUserKey.Text};
                var result = await Task.Run(()=> _lcController.TryRegister(user));
                ShowLoadingAnimation(false);
                if (result)
                {
                    MessageBox.Show("注册成功");
                    MainFrame.GoBack();
                }
                else
                {
                    txtTipsShow.Text = "注册失败，请换一个用户名!";
                }
            }
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
        /// 验证
        /// </summary>
        /// <returns></returns>
        private bool Valid()
        {
            if (string.IsNullOrEmpty(txtUserName.Text) ||
                string.IsNullOrEmpty(txtPassword.Text) ||
                string.IsNullOrEmpty(txtUserKey.Text))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
