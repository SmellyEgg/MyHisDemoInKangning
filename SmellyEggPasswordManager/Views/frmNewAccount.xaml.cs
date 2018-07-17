using SmellyEggPasswordManager.Controller;
using SmellyEggPasswordManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SmellyEggPasswordManager.Views
{
    /// <summary>
    /// frmNewAccount.xaml 的交互逻辑
    /// </summary>
    public partial class frmNewAccount : Window
    {
        private User _currentUser;

        private LoginController _lcController;

        public frmNewAccount(List<string> listType, User user, string accountType = "")
        {
            InitializeComponent();
            _lcController = new LoginController();
            _currentUser = user;

            MyAccountTypeCombo.ItemsSource = null;
            MyAccountTypeCombo.ItemsSource = listType;

            if (!string.IsNullOrEmpty(accountType) && !"所有分组".Equals(accountType))
            {
                MyAccountTypeCombo.SelectedIndex = listType.FindIndex(p => accountType.Equals(p));
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Valid())
            {
                MessageBox.Show("请检查你的输入");
            }
            else
            {
                ShowLoadingAnimation();
                
                Account account = new Account() { AccountName = txtAccountName.Text, AccountPassword = txtAccountPassword.Password, AccountType = MyAccountTypeCombo.Text};
                var result = await Task.Run(()=> _lcController.AddAccount(account, _currentUser));
                ShowLoadingAnimation(false);
                if (result)
                {
                    MessageBox.Show("新增账户成功！");
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("新增账户失败！");
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void txtRepeatPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(null, null);
            }
        }

        private bool Valid()
        {
            if (string.IsNullOrEmpty(MyAccountTypeCombo.Text)
                || string.IsNullOrEmpty(txtAccountName.Text)
                || string.IsNullOrEmpty(txtAccountPassword.Password)
                || string.IsNullOrEmpty(txtRepeatPassword.Password))
            {
                return false;
            }
            else if (!txtAccountPassword.Password.Equals(txtRepeatPassword.Password))
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
