

using Oracle.ManagedDataAccess.Client;
using SmellyEggPasswordManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
//using System.Data.OracleClient;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Runtime.InteropServices;
using SmellyEggPasswordManager.Models.HisDemo;
using System.Data;
using FastMember;
using ClosedXML.Excel;
using SmellyEggPasswordManager.Controller;
using SmellyEggPasswordManager.Controller.HisAddtion;
using SmellyEggPasswordManager.Models.HisDemo.HisRoleModel;
using Microsoft.Win32;
using System.Text;

namespace SmellyEggPasswordManager.Views
{
    /// <summary>
    /// frmCrypForHis.xaml 的交互逻辑
    /// </summary>
    public partial class frmCrypForHis : Window
    {
        HisControllerForUser hfoController;
        public frmCrypForHis()
        {
            InitializeComponent();
            hfoController = new HisControllerForUser();
            SetData();
            InitRole();
        }

        #region his的账户与密码
        /// <summary>
        /// 获取数据
        /// </summary>
        private async void SetData()
        {
            ShowLoadingAnimation();
            var listUser = await Task.Run(() => hfoController.GetTableContentObjects());
            dataGridMain.ItemsSource = listUser;
            ShowLoadingAnimation(false);
        }

        /// <summary>
        /// 手动解密
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualDecrpClick(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 显示等待动画
        /// </summary>
        /// <param name="isLoading"></param>
        private void ShowLoadingAnimation(bool isLoading = true)
        {
            if (isLoading)
            {
                dataGridMain.Visibility = Visibility.Hidden;
                gridMenu.Visibility = Visibility.Hidden;

                myLoading.Visibility = Visibility.Visible;
                myLoading.Spin = true;
                IsEnabled = false;
            }
            else 
            {
                IsEnabled = true;
                myLoading.Spin = false;
                dataGridMain.Visibility = Visibility.Visible;
                gridMenu.Visibility = Visibility.Visible;

                myLoading.Visibility = Visibility.Hidden;
            }
        }

        

        private void txtFilterKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SetFocusItem();
            }
        }

        private void SetFocusItem()
        {
            var source = dataGridMain.ItemsSource as List<HisUser>;
            if (object.Equals(source, null)) return;

            var index = source.FindIndex(p => p.UserName.Contains(txtFilter.Text) || p.UserCode.Contains(txtFilter.Text));
            if (index != -1)
            {
                var item = dataGridMain.Items[index];
                dataGridMain.SelectedItem = item;
                dataGridMain.ScrollIntoView(item);
                //dataGridMain.SelectedItem = source[index];
            }
        }
        #endregion


        #region his一些奇奇怪怪的报表
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            HisControllerForOutpatientFee hfo = new HisControllerForOutpatientFee();
            await hfo.ReadAllData();
        }


        /// <summary>
        /// 读取门诊费用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnReadFeeClick(object sender, RoutedEventArgs e)
        {
            HisControllerForOutpatientFee hfo = new HisControllerForOutpatientFee();
            await hfo.ReadOneYearOutPatientFee();
        }
        #endregion


        #region 菜单检索功能
        private List<HisRoleResources> _listSource = new List<HisRoleResources>();
        private List<HisRole> _listrole = new List<HisRole>();
        /// <summary>
        /// 初始化角色
        /// </summary>
        private async void InitRole()
        {
            HisControllerForRole role = new HisControllerForRole();
            _listrole = await Task.Run(() => role.GetRole());
            listViewRole.ItemsSource = _listrole;
            //资源表
            _listSource = await Task.Run(() => role.GetRoleResource());
            listViewResource.ItemsSource = _listSource;
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //ShowLoadingAnimation();
        }

        private void listViewRole_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = this.listViewRole.SelectedItem as HisRole;
            if (!object.Equals(item, null))
            {
                var newsource = _listSource.Where(p => p.RoleID.Equals(item.RoleId)).ToList();
                listViewResource.ItemsSource = null;
                listViewResource.ItemsSource = newsource;
            }

        }

        private void listViewResource_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void txtRoleName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var newsource = _listrole.Where(p => p.RoleName.Contains(txtRoleName.Text)).ToList();
                listViewRole.ItemsSource = null;
                listViewRole.ItemsSource = newsource;
            }
        }

        private void txtResourcesName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var newsource = _listSource.Where(p => p.Name.Contains(this.txtResourcesName.Text)).ToList();
                listViewResource.ItemsSource = null;
                listViewResource.ItemsSource = newsource;
            }
        }

        /// <summary>
        /// 资源项目双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewResource_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as HisRoleResources;
            if (item != null)
            {
                var index = _listrole.FindIndex(p => p.RoleId.Equals(item.RoleID));
                if (index != -1)
                {
                    this.listViewRole.ItemsSource = null;
                    this.listViewRole.ItemsSource = _listrole;
                    var itemselected = listViewRole.Items[index];
                    listViewRole.SelectedItems.Add(itemselected);
                    this.listViewRole.ScrollIntoView(itemselected);
                }
            }
        }

        #endregion


        /// <summary>
        /// 读取异地医保excel中信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReadExcelOfYDYB_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            if (opf.ShowDialog() == false) return;
            using (XLWorkbook workBook = new XLWorkbook(opf.FileName))
            {
                //Read the first Sheet from Excel file.
                IXLWorksheet workSheet = workBook.Worksheet(4);

                //Loop through the Worksheet rows.
                bool firstRow = true;
                List<YDYBExcel> listsource = new List<YDYBExcel>();
                foreach (IXLRow row in workSheet.Rows())
                {
                    //Use the first row to add columns to DataTable.
                    if (firstRow)
                    {
                        foreach (IXLCell cell in row.Cells())
                        {
                            //dt.Columns.Add(cell.Value.ToString());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        //Add rows to DataTable.
                        //dt.Rows.Add();
                        int i = 0;
                        YDYBExcel obj = new YDYBExcel();
                        obj.AkeCode = row.Cell(1).Value.ToString();
                        obj.LogType = row.Cell(2).Value.ToString();
                        obj.LogContent = row.Cell(3).Value.ToString();
                        obj.ItemCode = row.Cell(4).Value.ToString();
                        obj.RegisterName = row.Cell(5).Value.ToString();
                        obj.EnglistName = row.Cell(6).Value.ToString();
                        obj.DoseType = row.Cell(7).Value.ToString();
                        obj.Spec = row.Cell(8).Value.ToString();
                        obj.ProduceCompany = row.Cell(9).Value.ToString();
                        obj.RegCode = row.Cell(10).Value.ToString();
                        obj.RegCodeMemo = row.Cell(11).Value.ToString();
                        obj.ApprovalDate = row.Cell(12).Value.ToString();
                        listsource.Add(obj);
                    }

                }
                var intcount = listsource.Count;

                StringBuilder sb = new StringBuilder(1000);
                bool isfirst = true;
                string format = @"update fin_com_siitem t
                                   set t.item_code  = '{0}',
                                       t.name       = '{1}',
                                       t.ename      = '{2}',
                                       t.specs      = '{3}',
                                       t.dose_code  = '{4}',
                                       t.spell_code = '{5}',
                                       t.oper_date  = sysdate,
                                       t.ake114     = '{6}',
                                       t.regname    = '{7}',
                                       t.regcode    = '{8}',
                                       t.prodaddr   = '{9}',
                                       t.memo = '{13}'
                                 where t.ake114 = '{10}'
                                   and t.item_code = '{12}';";
                listsource.AsParallel().ForAll(p => {
                    if (p.LogType.Equals("修改"))
                    {
                        sb.Append(string.Format(format, p.ItemCode, p.HisName, p.EnglistName, p.Spec, p.DoseType,
                           p.SpellCode, p.AkeCode, p.RegisterName, p.RegCode, p.ProduceCompany,
                           p.RegCodeMemo));
                        //if (isfirst)
                        //{
                        //    sb.Append("('" + p.AkeCode + "' ");
                        //    isfirst = false;
                        //}
                        //else
                        //{
                        //    sb.Append(", '" + p.AkeCode + "' ");
                        //}
                    }
                });
                var updateSql = sb.ToString();

                //接下来处理一下新增的

            }
        }
    }
}
