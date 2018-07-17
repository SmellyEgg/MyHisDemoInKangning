using SmellyEggPasswordManager.Controller;
using SmellyEggPasswordManager.Models;
using Smith.WPF.HtmlEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SmellyEggPasswordManager.Views
{
    /// <summary>
    /// NotePage.xaml 的交互逻辑
    /// </summary>
    public partial class NotePage : Page
    {
        private User _currentUser;
        private Frame _mainFrame;
        private List<Note> _currentDataSource;

        private NoteController _lcController;

        private HtmlEditor _htmlEditor;

        public NotePage(Frame mainframe, User user)
        {
            InitializeComponent();
            //
            Window.GetWindow(mainframe).WindowState = WindowState.Maximized;
            Window.GetWindow(mainframe).Title = "日报管理";
            //Window.GetWindow(this).Title = "日报管理";

            _lcController = new NoteController();
            _htmlEditor = new HtmlEditor();
            _currentUser = user;
            _mainFrame = mainframe;
            Refresh();

            SetBingWallpaper();
        }

        private async Task SetBingWallpaper()
        {
            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.UniformToFill;
            ib.Opacity = 0.75;
            ImageController ic = new ImageController();
            var sourceStr = await Task.Run(() => ic.GetOnlineImageUrlAsync());

            var filename = DateTime.Now.Date.ToShortDateString();
            var path = System.AppDomain.CurrentDomain.BaseDirectory + "ImageCache\\" + filename + Path.GetExtension(sourceStr);
            if (!File.Exists(path))
            {
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    client.DownloadFileAsync(new Uri(sourceStr), path);
                }
            }
            ib.ImageSource = new BitmapImage(new Uri(sourceStr)) { CacheOption = BitmapCacheOption.None };
            MyContentGrid.Background = ib;
        }



        /// <summary>
        /// 刷新
        /// </summary>
        private async void Refresh()
        {
            ShowLoadingAnimation();
            if (object.Equals(_currentUser, null)) return;
            _currentDataSource = await Task.Run(() => _lcController.GetNotes(_currentUser));
            _currentDataSource.ForEach(p => { _htmlEditor.ContentHtml = p.NoteContent; p.NoteText = _htmlEditor.ContentText; });

            if (object.Equals(_currentDataSource, null)) return;
            var filterStr = _currentDataSource.GroupBy(p => p.NoteType)
                .Select(p => p.FirstOrDefault().NoteType).ToList();

           // var result = _currentDataSource.Where(p => p.NoteContent.Equals("123")).Select(p => p.NoteTitle).FirstOrDefault();
            if (filterStr == null) filterStr = new List<string>();
            if (filterStr.Count > 0 && !filterStr.Contains("所有分类"))
            {
                filterStr.Add("所有分类");
            }

            MyListView.ItemsSource = null;
            MyListView.ItemsSource = filterStr;
            MyListView.SelectedIndex = filterStr.Count - 1;

            MyAccountListView.ItemsSource = null;
            MyAccountListView.ItemsSource = _currentDataSource;
            FilterAccount();

            //await SetBingWallpaper();
            ShowLoadingAnimation(false);
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

        private void ShowAccountByType(string accountType)
        {
            var newDataSource = _currentDataSource.Where(p => accountType.Equals(p.NoteType)).ToList();
            if ("所有分类".Equals(accountType))
            {
                newDataSource = _currentDataSource;
            }
            newDataSource = newDataSource.Where(p => p.NoteContent.Contains(txtFilter.Text)).ToList();
            MyAccountListView.ItemsSource = null;
            MyAccountListView.ItemsSource = newDataSource;
        }

        /// <summary>
        /// 新建分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            frmNewGroup frm = new frmNewGroup();
            if (frm.ShowDialog() == true)
            {
                var listSource = MyListView.ItemsSource as List<string>;
                if (object.Equals(listSource, null)) listSource = new List<string>();
                if (!listSource.Contains(frm.GroupName))
                {
                    listSource.Add(frm.GroupName);
                    MyListView.ItemsSource = null;
                    MyListView.ItemsSource = listSource;
                }
                else
                {
                    MessageBox.Show("已经存在相同的分组名称！");
                }
            }
        }


        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                FilterAccount();
            }
        }

        private void FilterAccount()
        {
            if (_currentDataSource == null) return;
            var newsource = _currentDataSource.Where(p => p.NoteContent.ToLower().Contains(txtFilter.Text.ToLower())
                            || p.NoteTitle.ToLower().Contains(txtFilter.Text.ToLower())).ToList();
            MyAccountListView.ItemsSource = null;
            MyAccountListView.ItemsSource = newsource;
            //if (MyListView.SelectedIndex != -1)
            //{
            //    MyListView_SelectionChanged(null, null);
            //}
        }

        private void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            item.IsSelected = true;
        }



        /// <summary>
        /// 删除账户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要删除该日报吗？", "警告", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var selectedAccount = MyAccountListView.SelectedItem as Note;
            ShowLoadingAnimation();
            var result = await Task.Run(() => _lcController.DeleteNote(selectedAccount, _currentUser.UserName));
            ShowLoadingAnimation(false);
            if (result)
            {
                MessageBox.Show("删除成功！");
                Refresh();
            }
            else
            {
                MessageBox.Show("删除失败!");
            }

        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = MyAccountListView.SelectedItem as Note;
            var listType = MyListView.ItemsSource as List<string>;
            frmNewNote frm = new frmNewNote(listType, _currentUser, string.Empty, true, selectedAccount);
            if (frm.ShowDialog() == true)
            {
                Refresh();
            }
        }

        private void MyListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = MyListView.SelectedItem;
            if (!object.Equals(item, null))
            {
                ShowAccountByType(item.ToString());
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLogoutClick(object sender, RoutedEventArgs e)
        {
            if (!object.Equals(_mainFrame, null) && _mainFrame.CanGoBack)
            {
                _mainFrame.GoBack();
            }

        }

        /// <summary>
        /// 新增用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddUserClick(object sender, RoutedEventArgs e)
        {
            var listType = MyListView.ItemsSource as List<string>;
            if (object.Equals(listType, null) || listType.Count < 1)
            {
                MessageBox.Show("请先新建一个分类");
                return;
            }
            var currentType = MyListView.SelectedIndex == -1 ? string.Empty : MyListView.SelectedItem.ToString();
            frmNewNote frm = new frmNewNote(listType, _currentUser, currentType);
            DoubleAnimation animFadeIn = new DoubleAnimation();
            animFadeIn.From = 0;
            animFadeIn.To = 1;
            animFadeIn.Duration = new Duration(TimeSpan.FromSeconds(2));
            frm.BeginAnimation(Window.OpacityProperty, animFadeIn);
            //window.ShowDialog();

            if (frm.ShowDialog() == true)
            {
                Refresh();
            }
        }

        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteGroupClick(object sender, RoutedEventArgs e)
        {
            if (MyListView.SelectedIndex == -1) return;
            var item = MyListView.SelectedItem.ToString();
            var listSource = MyListView.ItemsSource as List<string>;
            listSource.Remove(item);
            MyListView.ItemsSource = null;
            MyListView.ItemsSource = listSource;
        }

        /// <summary>
        /// 解密服务，针对his中的账户密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDecriPtClick(object sender, RoutedEventArgs e)
        {
            frmCrypForHis frm = new frmCrypForHis();
            frm.ShowDialog();
        }
    }
}
