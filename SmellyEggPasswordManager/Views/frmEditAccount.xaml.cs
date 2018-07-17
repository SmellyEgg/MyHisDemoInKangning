using SmellyEggPasswordManager.Controller;
using SmellyEggPasswordManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SmellyEggPasswordManager.Views
{
    /// <summary>
    /// frmEditAccount.xaml 的交互逻辑
    /// </summary>
    public partial class frmEditAccount : Window
    {
        private Account _oldAccount;
        private LoginController lcController;
        private User _currentUser;

        public frmEditAccount(Account account, List<string> accountType, User user)
        {
            InitializeComponent();
            lcController = new LoginController();
            _oldAccount = account;
            _currentUser = user;
            SetAccountInfo(account, accountType);
        }

        private void SetAccountInfo(Account account, List<string> accountType)
        {
            MyAccountTypeCombo.ItemsSource = accountType;
            MyAccountTypeCombo.Text = account.AccountType;

            txtAccountName.Text = account.AccountName;
            txtAccountPassword.Password = account.AccountPassword;
            txtRepeatPassword.Password = account.AccountPassword;
        }

        private void txtRepeatPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(null, null);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Valid())
            {
                MessageBox.Show("请检查输入！");
            }
            else
            {
                ShowLoadingAnimation();
                Account newaccount = new Account() { AccountType = MyAccountTypeCombo.Text,
                AccountName = txtAccountName.Text, AccountPassword = txtAccountPassword.Password};
                if (!ValidDifference(newaccount, _oldAccount))
                {
                    ShowLoadingAnimation(false);
                    MessageBox.Show("更新账户成功！");
                    DialogResult = true;
                    return;
                }
                var result = await Task.Run(()=> lcController.UpdateAccount(newaccount, _oldAccount, _currentUser));
                ShowLoadingAnimation(false);
                if (result == true)
                {
                    MessageBox.Show("更新账户成功！");
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("更新账户失败！");
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
        /// 
        /// </summary>
        /// <param name="newaccount"></param>
        /// <param name="oldaccount"></param>
        /// <returns></returns>
        private bool ValidDifference(Account newaccount, Account oldaccount)
        {
            if (newaccount.AccountType.Equals(oldaccount.AccountType)
                && newaccount.AccountName.Equals(oldaccount.AccountName)
                && newaccount.AccountPassword.Equals(oldaccount.AccountPassword))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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
