using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Controller
{
    public class BaseSqlController
    {
        public MySqlConnection _myConn;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseSqlController()
        {
            _myConn = new MySqlConnection(Config._connectStr);
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<DbDataReader> ExcuteQuery(string sql)
        {
            if (_myConn.State != System.Data.ConnectionState.Open)
            {
                await _myConn.OpenAsync();
            }


            //var cmd = _myConn.CreateCommand();
            //cmd.CommandText = sql;
            //var reader = await cmd.ExecuteReaderAsync();
            //return reader;


            try
            {
                using (var cmd = _myConn.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = sql;
                    using (var dataAdapter = new MySqlDataAdapter())
                    {
                        var dataSet = new DataSet();
                        dataAdapter.SelectCommand = cmd;
                        dataAdapter.Fill(dataSet);
                        if (!object.Equals(dataSet, null) && dataSet.Tables.Count > 0)
                        {
                            var myreader = dataSet.Tables[0].CreateDataReader();
                            return myreader;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                this._errMessage = ex.Message;
                return null;
            }
            finally
            {
                _myConn.Close();
            }

        }

        private string _errMessage = string.Empty;

        /// <summary>
        /// 执行语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<int> ExcuteNonQuery(string sql)
        {
            if (_myConn.State != System.Data.ConnectionState.Open)
            {
                await _myConn.OpenAsync();
            }

            try
            {
                using (var cmd = _myConn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var result = cmd.ExecuteNonQuery();
                    return result;
                }
            }
            catch (Exception ex)
            {
                this._errMessage = ex.Message;
                return -1;
            }
            finally
            {
                _myConn.Close();
            }

            
        }



    }
}
