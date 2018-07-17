using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace SmellyEggPasswordManager.Controller.Oracle
{
    /// <summary>
    /// oracle基类
    /// </summary>
    public class oracleBase
    {
        private string _connection = string.Empty;

        private readonly string _configPath = System.AppDomain.CurrentDomain.BaseDirectory + "\\smellyConfig.xml";

        private OracleConnection _oracleConnection;

        public string _errMessage = string.Empty;

        public oracleBase()
        {
            InitConnection();
        }

        /// <summary>
        /// 初始化连接
        /// </summary>
        private void InitConnection()
        {
            XmlDocument xml = new XmlDocument();
            if (!File.Exists(_configPath))
            {
                throw new Exception("配置文件不存在，路径：" + _configPath);
            }
            xml.Load(_configPath);
            //数据库连接字符串
            if (xml.GetElementsByTagName("connstring").Count > 0)
            {
                var tmpValue = xml.DocumentElement["connstring"].InnerText.Trim();
                _connection = tmpValue;
                _oracleConnection = new OracleConnection(_connection);
            }
        }

        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public async Task<int> ExcuteNonQuery(string sql)
        {
            if (_oracleConnection.State != System.Data.ConnectionState.Open)
            {
                await _oracleConnection.OpenAsync();
            }
            try
            {
                using (var cmd = _oracleConnection.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = sql;
                    var result = await cmd.ExecuteNonQueryAsync();
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
                _oracleConnection.Close();
            }
        }

        /// <summary>
        /// 执行查询语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public async Task<DbDataReader> ExcuteQuery(string sql)
        {
            if (this._oracleConnection.State != System.Data.ConnectionState.Open)
            {
                await _oracleConnection.OpenAsync();
            }
            try
            {
                using (var cmd = _oracleConnection.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = sql;
                    using (var dataAdapter = new OracleDataAdapter())
                    {
                        var dataSet = new DataSet();
                        dataAdapter.SelectCommand = cmd;
                        dataAdapter.Fill(dataSet);
                        if (!object.Equals(dataSet,null) && dataSet.Tables.Count > 0)
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
                _oracleConnection.Close();
            }
        }




    }
}
