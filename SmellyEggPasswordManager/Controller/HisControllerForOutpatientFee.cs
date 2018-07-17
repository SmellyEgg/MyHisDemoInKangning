using ClosedXML.Excel;
using FastMember;
using Oracle.ManagedDataAccess.Client;
using SmellyEggPasswordManager.Models;
using SmellyEggPasswordManager.Models.HisDemo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmellyEggPasswordManager.Controller
{
    internal class HisControllerForOutpatientFee
    {
        public async Task<List<OutPatientFeeDetail>> ReadOutPatientFee(string mydate, string begindate)
        {
            string connstr = @"Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.16.200.100)(PORT = 1521)))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = rac))); User ID=his;Password=his";
            string mysql = @"select t.clinic_code,
                               fun_get_Age(t.birthday),
                               to_char(t.birthday, 'yyyy/mm/dd'),
                               t.sex_code,
                               (select mcd.diag_name
                                  from met_cas_diagnose mcd
                                 where mcd.inpatient_no = t.clinic_code
                                   and mcd.persson_type = '0'
                                   and mcd.main_flag = '1'
                                   and rownum < 2) as diagname,
                               (select mcd.icd_code
                                  from met_cas_diagnose mcd
                                 where mcd.inpatient_no = t.clinic_code
                                   and mcd.persson_type = '0'
                                   and mcd.main_flag = '1'
                                   and rownum < 2) as diagname,
                               to_char(t.see_date, 'yyyy/mm/dd'),
                               (select cd.dept_name
                                  from com_department cd
                                 where cd.dept_code = t.see_dpcd) as seedepartment,
                               t.pact_code, 
                               t.pact_name
                          from fin_opr_register t
                         where t.reg_date >= to_date('{0}', 'yyyy-mm-dd')
                           and t.reg_date < to_date('{1}', 'yyyy-mm-dd')
                           and not exists
                         (select 1
                                  from fin_opr_register forr
                                 where forr.clinic_code = t.clinic_code
                                   and forr.trans_type = '2'
                                   and forr.reg_date >= to_date('{0}', 'yyyy-mm-dd')
                                   and forr.reg_date < to_date('{1}', 'yyyy-mm-dd'))";
            mysql = string.Format(mysql, mydate, begindate);

            //Console.WriteLine("查询的时间:{0}--{1}", mydate, begindate);

            using (var conn = new OracleConnection(connstr))
            {
                await conn.OpenAsync();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    //先获取基本信息
                    cmd.CommandText = mysql;
                    var reader = await cmd.ExecuteReaderAsync();
                    List<OutPatientFeeDetail> listall = new List<OutPatientFeeDetail>();
                    while (await reader.ReadAsync())
                    {
                        OutPatientFeeDetail obj = new OutPatientFeeDetail();
                        obj.患者编号 = reader.GetString(0);
                        obj.年龄 = await reader.IsDBNullAsync(1) ? string.Empty : reader.GetString(1);
                        obj.出生日期 = await reader.IsDBNullAsync(2) ? string.Empty : reader.GetString(2);
                        obj.性别 = await reader.IsDBNullAsync(3) ? string.Empty : reader.GetString(3);
                        obj.疾病名称 = await reader.IsDBNullAsync(4) ? string.Empty : reader.GetString(4);
                        obj.ICD10编码 = await reader.IsDBNullAsync(5) ? string.Empty : reader.GetString(5);
                        obj.就诊日期 = await reader.IsDBNullAsync(6) ? string.Empty : reader.GetString(6);
                        obj.就诊科室 = await reader.IsDBNullAsync(7) ? string.Empty : reader.GetString(7);
                        obj.PactCode = await reader.IsDBNullAsync(8) ? string.Empty : reader.GetString(8);
                        obj.参保类型 = await reader.IsDBNullAsync(9) ? string.Empty : reader.GetString(9);
                        listall.Add(obj);
                    }
                    //Console.WriteLine("获取基本信息完成， 数量：" + listall.Count + "--" + DateTime.Now.ToLongDateString());
                    //获取费用信息
                    var listAllClinicCode = listall.Select(p => p.患者编号).ToList();
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(300000);
                    sb.Append("insert into temporaryCodes  ");
                    int index = 0;
                    foreach (var item in listAllClinicCode)
                    {
                        if (index == 0)
                        {
                            sb.Append(string.Format(" select '{0}' from dual ", item));
                            index++;
                        }
                        else
                        {
                            sb.Append(string.Format(" union select '{0}' from dual", item));
                        }
                    }
                    var trans = conn.BeginTransaction();
                    //先把clinicCode插入临时表
                    cmd.CommandText = sb.ToString();
                    await cmd.ExecuteNonQueryAsync();
                    //Console.WriteLine("插入临时表完成，临时表数量：" + index.ToString());
                    //获取所有的费用明细
                    string sqlTotalFee = @"select tt.pub_cost,
                                                   tt.pay_cost,
                                                   tt.own_cost,
                                                   tt.fee_code,
                                                   tt.clinic_code
                                              from fin_opb_feedetail tt
                                             where tt.clinic_code in (select code from temporaryCodes)";
                    cmd.CommandText = sqlTotalFee;
                    reader = null;
                    reader = await cmd.ExecuteReaderAsync();
                    List<MiniFee> listTotalFee = new List<MiniFee>();
                    while (await reader.ReadAsync())
                    {
                        MiniFee obj = new MiniFee();
                        obj.PubCost = await reader.IsDBNullAsync(0) ? 0 : reader.GetDouble(0);
                        obj.PayCost = await reader.IsDBNullAsync(1) ? 0 : reader.GetDouble(1);
                        obj.OwnCost = await reader.IsDBNullAsync(2) ? 0 : reader.GetDouble(2);
                        obj.FeeCode = await reader.IsDBNullAsync(3) ? string.Empty : reader.GetString(3);
                        obj.ClinicCode = reader.GetString(4);

                        listTotalFee.Add(obj);
                    }
                    //Console.WriteLine("获取费用明细信息完成， 数量：" + listTotalFee.Count + "--" + DateTime.Now.ToLongDateString());
                    //获取所有的挂号费用
                    string regSql = @"select foa.clinic_no, sum(foa.tot_cost)
                                      from fin_opb_accountcardfee foa
                                     where foa.clinic_no in (select code from temporaryCodes) group by foa.clinic_no";
                    cmd.CommandText = regSql;
                    reader = null;
                    reader = await cmd.ExecuteReaderAsync();
                    List<MiniFee> listRegFee = new List<MiniFee>();
                    while (await reader.ReadAsync())
                    {
                        MiniFee obj = new MiniFee();
                        obj.ClinicCode = reader.GetString(0);
                        obj.RegFee = await reader.IsDBNullAsync(1) ? 0 : reader.GetDouble(1);
                        listRegFee.Add(obj);
                    }
                    //Console.WriteLine("获取挂号费用信息完成， 数量：" + listRegFee.Count + "--" + DateTime.Now.ToLongDateString());
                    //获取所有的记账费用
                    string pubFeeSql = @"select fis.inpatient_no, fis.pub_cost, fis.pay_cost, fis.own_cost
                                           from fin_ipr_siinmaininfo fis
                                          where fis.inpatient_no in (select code from temporaryCodes)";

                    cmd.CommandText = pubFeeSql;
                    reader = null;
                    reader = await cmd.ExecuteReaderAsync();
                    List<MiniFee> listPubFee = new List<MiniFee>();
                    while (await reader.ReadAsync())
                    {
                        MiniFee obj = new MiniFee();
                        obj.ClinicCode = reader.GetString(0);
                        obj.PubCost = await reader.IsDBNullAsync(1) ? 0 : reader.GetDouble(1);
                        obj.PayCost = await reader.IsDBNullAsync(2) ? 0 : reader.GetDouble(2);
                        obj.OwnCost = await reader.IsDBNullAsync(3) ? 0 : reader.GetDouble(3);
                        listPubFee.Add(obj);
                    }
                    //Console.WriteLine("获取记账费用信息完成， 数量：" + listPubFee.Count + "--" + DateTime.Now.ToLongDateString());
                    //提交事务，清除临时表
                    trans.Commit();
                    //开始合并费用

                    //获取总费用
                    List<String> listdrugCode = new List<string>() { "001", "002", "003" };
                    List<string> listAllCode = new List<string>() { "001", "002", "003", "005", "006", "008", "009", "036" };
                    listall.AsParallel().ForAll(p => {
                        p.门诊总费用 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号)).Sum(a => a.PubCost + a.PayCost + a.OwnCost) + listRegFee.Where(a => a.ClinicCode.Equals(p.患者编号)).FirstOrDefault().RegFee;
                        p.治疗费 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("006")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//治疗费
                        p.药品费 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && listdrugCode.Contains(a.FeeCode)).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//药费
                        p.西药 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("001")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//西药
                        p.中草药 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("002")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//重要
                        p.中成药 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("003")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//中成药
                        p.挂号费 = listRegFee.Where(a => a.ClinicCode.Equals(p.患者编号)).FirstOrDefault().RegFee;//挂号费
                        p.诊察费 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("036")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//中成药
                        p.检查费 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("005")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//中成药
                        p.手术费 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("008")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//中成药
                        p.化验费 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && a.FeeCode.Equals("009")).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//中成药
                        p.其他费用 = listTotalFee.Where(a => a.ClinicCode.Equals(p.患者编号) && !listAllCode.Contains(a.FeeCode)).Sum(a => a.PubCost + a.PayCost + a.OwnCost);//中成药

                        //先处理自费病人
                        if (p.PactCode.Equals("1") || p.PactCode.Equals("10"))
                        {
                            p.保险统筹基金支付费用 = 0;
                            p.个人账户支付费用 = 0;
                            p.患者自付费用 = p.门诊总费用;
                        }
                        else if (p.PactCode.Equals("2"))
                        {
                            p.保险统筹基金支付费用 = listPubFee.Where(a => a.ClinicCode.Equals(p.患者编号)).Sum(a => a.PubCost);
                            p.个人账户支付费用 = listPubFee.Where(a => a.ClinicCode.Equals(p.患者编号)).Sum(a => a.PayCost);
                            p.患者自付费用 = listPubFee.Where(a => a.ClinicCode.Equals(p.患者编号)).Sum(a => a.OwnCost);
                        }
                        //性别
                        if ("M".Equals(p.性别))
                        {
                            p.性别 = "男";
                        }
                        else if ("F".Equals(p.性别))
                        {
                            p.性别 = "女";
                        }
                        else
                        {
                            p.性别 = "不详";
                        }
                    });

                    //Console.WriteLine("合并费用信息完成，开始写入excel表中!");

                    //var vvvv = 1;
                    //写入数据到Excel表中
                    //WriteToExcelNew(listall, begindate + "-" + mydate);
                    return listall;

                    //Console.WriteLine("写入excel完成，该excel为" + mydate + ".xls");

                }
            }
        }

        /// <summary>
        /// 写入excel
        /// </summary>
        /// <param name="listsource"></param>
        public void WriteToExcel(List<SeeDoctor> listsource)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();

            if (xlApp == null)
            {
                MessageBox.Show("Excel is not properly installed!!");
                return;
            }


            Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            object misValue = System.Reflection.Missing.Value;

            xlWorkBook = xlApp.Workbooks.Add(misValue);
            xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            //设置第一行
            xlWorkSheet.Cells[1, 1] = "科室名称";
            DateTime dtbegin = new DateTime(2018, 05, 01);
            int column = 2;
            while (dtbegin < new DateTime(2018, 06, 01))
            {
                xlWorkSheet.Cells[1, column] = dtbegin.ToShortDateString();
                dtbegin = dtbegin.AddDays(1);
                xlWorkSheet.Columns[column].ColumnWidth = 18;
                column++;
            }

            for (int i = 1; i <= listsource.Count; i++)
            {
                xlWorkSheet.Cells[i + 1, 1] = listsource[i - 1].name;

                for (int j = 2; j < 33; j++)
                {
                    xlWorkSheet.Cells[i + 1, j] = listsource[i - 1].listCount[j - 2];
                }
            }



            xlWorkBook.SaveAs("d:\\456.xls", Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal, misValue, misValue, misValue, misValue, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkBook.Close(true, misValue, misValue);
            xlApp.Quit();

            Marshal.ReleaseComObject(xlWorkSheet);
            Marshal.ReleaseComObject(xlWorkBook);
            Marshal.ReleaseComObject(xlApp);

            MessageBox.Show("Excel file created , you can find the file d:\\csharp-Excel.xls");

        }

        /// <summary>
        /// 读取所有的数据
        /// </summary>
        /// <returns></returns>
        public async Task ReadAllData()
        {
            string connstr = @"Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.16.200.100)(PORT = 1521)))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = rac))); User ID=his;Password=his";
            //string sql = @"select t.see_docd,
            //               t.see_date,
            //               (select ce.empl_name
            //                  from com_employee ce
            //                 where ce.empl_code = t.see_docd)
            //          from fin_opr_register t
            //         where t.reg_date > to_date('2016-01-01', 'yyyy-mm-dd')
            //           and t.reg_date < to_date('2018-07-01', 'yyyy-mm-dd')
            //           and t.see_docd is not null";
            string sql = @" select t.see_dpcd,
                                   t.reg_date,
                                    (select cd.dept_name
                                       from com_department cd
                                      where cd.dept_code = t.see_dpcd)
                               from fin_opr_register t
                              where t.clinic_code not in
                                    (select forr.clinic_code
                                       from fin_opr_register forr
                                      where forr.trans_type = '2'
                                        and forr.reg_date between to_date('2018-05-01', 'yyyy-mm-dd') and
                                            to_date('2018-06-01', 'yyyy-mm-dd'))
                                and t.reg_date between to_date('2018-05-01', 'yyyy-mm-dd') and
                                    to_date('2018-06-01', 'yyyy-mm-dd')
                                and t.see_dpcd not in ('0155', '0122', '0107', '0120')";
            try
            {
                using (var conn = new OracleConnection(connstr))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = sql;
                        var reader = await cmd.ExecuteReaderAsync();
                        List<SeeDoctorData> listall = new List<SeeDoctorData>();
                        //Stopwatch sw = new Stopwatch();
                        //sw.Start();
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                SeeDoctorData obj = new SeeDoctorData();

                                obj.mycode = reader.GetString(0);
                                obj.mydate = reader.GetDateTime(1);
                                obj.myname = reader.GetString(2);
                                //obj.UserPassword = reader.IsDBNull(2) ? DecryService("") : DecryService(reader.GetString(2));
                                //obj.ValidState = reader.GetString(3).Equals("1") ? true : false;
                                listall.Add(obj);
                            }
                            catch
                            { }
                        }
                        sw.Stop();
                        Console.WriteLine("sql运行时间：" + (sw.ElapsedMilliseconds / 1000).ToString() + "秒");
                        //return list;
                        if (!Object.Equals(listall, null) && listall.Count > 0)
                        {
                            List<SeeDoctor> listdoctor = new List<SeeDoctor>();

                            sw.Reset();
                            sw.Start();
                            listall.ForEach(p => {
                                if (listdoctor.FindIndex(a => a.employCode.Equals(p.mycode)) == -1)
                                {
                                    listdoctor.Add(new SeeDoctor() { employCode = p.mycode, name = p.myname });
                                }
                            });
                            sw.Stop();
                            Console.WriteLine(("时间花费：" + sw.ElapsedMilliseconds / 1000).ToString());
                            //初始化所有月份的值为0
                            listdoctor.ForEach(p => {
                                for (int i = 0; i < 31; i++)
                                {
                                    p.listCount.Add(0);
                                }
                            });
                            //循环所有的天数查找并进行赋值
                            DateTime dtBegin = new DateTime(2018, 05, 01);
                            DateTime dtEnd = new DateTime(2018, 06, 01);
                            DateTime dtValue = new DateTime(2018, 05, 01);


                            while (dtValue < dtEnd)
                            {
                                int doctorIndex = 0;
                                foreach (var doctor in listdoctor)
                                {
                                    int dayIndex = dtValue.Day - dtBegin.Day;

                                    var listcurrent = listall.Where(p => p.mycode.Equals(doctor.employCode) && p.mydate.ToShortDateString().Equals(dtValue.ToShortDateString())).ToList();
                                    if (!object.Equals(listcurrent, null))
                                    {
                                        listdoctor[doctorIndex].listCount[dayIndex] = listcurrent.Count;
                                    }
                                    doctorIndex++;
                                }
                                //增加一天
                                dtValue = dtValue.AddDays(1);
                            }
                            //int iiii = 0;
                            //WriteToExcel(listdoctor);
                            //计算累加的结果
                            var total = listdoctor.Sum(p => p.listCount.Sum());
                            MessageBox.Show(total.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 读取门诊一年的费用
        /// </summary>
        /// <returns></returns>
        public async Task ReadOneYearOutPatientFee()
        {
            List<OutPatientFeeDetail> listAll = new List<OutPatientFeeDetail>();
            HisControllerForOutpatientFee hfo = new HisControllerForOutpatientFee();
            DateTime dtBegin = new DateTime(2017, 01, 01);
            DateTime dtEnd = new DateTime(2017, 12, 31);
            while (dtBegin <= dtEnd)
            {
                Console.WriteLine("开始读取数据{0}--{1}", dtBegin, dtBegin.AddDays(1));
                var newlist = await hfo.ReadOutPatientFee(dtBegin.ToShortDateString(), dtBegin.AddDays(1).ToShortDateString());
                if (!object.Equals(newlist, null))
                {
                    listAll.AddRange(newlist);
                }
                Console.WriteLine("获取了{0}--{1}的数据", dtBegin, dtBegin.AddDays(1));
                dtBegin = dtBegin.AddDays(1);
            }

            Console.WriteLine("开始写入到excel");
            DataTable table = new DataTable();
            using (var reader = ObjectReader.Create(listAll, "患者编号", "年龄", "出生日期", "性别", "疾病名称", "ICD10编码", "就诊日期", "就诊科室", "门诊总费用", "治疗费", "药品费", "西药", "中成药", "中草药", "挂号费", "诊察费", "检查费", "手术费", "化验费", "其他费用", "参保类型", "保险统筹基金支付费用", "个人账户支付费用", "患者自付费用", "患者自付费用", "医疗救助负担"))
            {
                table.Load(reader);
            }
            var workbook = new XLWorkbook();

            workbook.Worksheets.Add(table, "WorksheetName");
            workbook.SaveAs("E:\\门诊所有费用.xlsx");
            //WriteToExcelNew(listAll, "门诊所有费用");
            Console.WriteLine("写入excel完成");
        }


    }
}
