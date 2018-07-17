using Oracle.ManagedDataAccess.Client;
using SmellyEggPasswordManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmellyEggPasswordManager.Controller.HisAddtion
{
    internal class HisControllerForUser
    {
        /// <summary>
        /// DESKey {D515E09B-E299-47e0-BF19-EDFDB6E4C775}
        /// HIS加密解密deskey，不同于lisence的deskey
        /// </summary>
        private static string DESKey = "Core_H_N";
        /// <summary>
        /// 获取表内容实体
        /// </summary>
        /// <returns></returns>
        public async Task<List<HisUser>> GetTableContentObjects()
        {
            string connstr = @"Data Source=(DESCRIPTION =(ADDRESS_LIST =(ADDRESS = (PROTOCOL = TCP)(HOST = 10.16.200.100)(PORT = 1521)))(CONNECT_DATA =(SERVER = DEDICATED)(SERVICE_NAME = rac))); User ID=his;Password=his";
            string sql = @"select pcu.username, pcu.account, pcu.password, ce.valid_state
                            from PRIV_COM_USER pcu, com_employee ce
                            where ce.empl_code = pcu.account";
            try
            {
                using (var conn = new OracleConnection(connstr))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.CommandText = sql;
                        var reader = await cmd.ExecuteReaderAsync();
                        List<HisUser> list = new List<HisUser>();
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        while (await reader.ReadAsync())
                        {
                            HisUser obj = new HisUser();

                            obj.UserName = reader.GetString(0);
                            obj.UserCode = reader.GetString(1);
                            obj.UserPassword = reader.IsDBNull(2) ? DecryService("") : DecryService(reader.GetString(2));
                            obj.ValidState = reader.GetString(3).Equals("1") ? true : false;

                            list.Add(obj);
                        }
                        sw.Stop();
                        Console.WriteLine((sw.ElapsedMilliseconds / 1000).ToString());
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 解密方法
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string DecryService(string password)
        {
            string newpassword = Neusoft.HisCrypto.DESCryptoService.DESDecrypt(password, DESKey);
            return newpassword;
        }
    }
}
