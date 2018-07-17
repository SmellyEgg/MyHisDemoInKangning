using MySql.Data.MySqlClient;
using SmellyEggPasswordManager.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Controller
{
    /// <summary>
    /// 日报
    /// </summary>
    public class NoteController : BaseSqlController
    {
        /// <summary>
        /// 获取日报
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<List<Note>> GetNotes(User user)
        {
            try
            {
                string sql = "SELECT NoteType, NoteContent, NoteTitle FROM `MyNote` WHERE UserName = '{0}'";
                sql = string.Format(sql, user.UserName);
                var reader = await ExcuteQuery(sql);
                List<Note> listNote = new List<Note>();
                while (await reader.ReadAsync())
                {
                    if (reader != null && reader.FieldCount > 0)
                    {
                        Note account = new Note()
                        {
                            NoteType = reader[0].ToString(),
                            NoteContent = reader[1].ToString(),
                            NoteTitle = reader[2].ToString(),
                            UserName = user.UserName
                        };
                        listNote.Add(account);
                    }
                }
                reader.Close();
                reader = null;
                return listNote;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// 删除日报
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<bool> DeleteNote(Note note, string userName)
        {
            try
            {
                string sql = "DELETE FROM `MyNote` WHERE NoteTitle = '{0}' and UserName = '{1}'";
                sql = string.Format(sql, note.NoteTitle, userName);
                var result = await ExcuteNonQuery(sql);
                if (result == 1) return true;
            }
            catch (Exception ex)
            {
                string exx = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// 增加日报
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddNote(Note note, User user)
        {
            try
            {
                using (var conn = new MySqlConnection(Config._connectStr))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        string sql = "INSERT INTO `MyNote`(`UserName`, `NoteType`, `NoteContent`, `NoteTitle`) VALUES (@username,@notetype,@notecontent,@notetitle)";
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@username", user.UserName);
                        cmd.Parameters.AddWithValue("@notetype", note.NoteType);
                        cmd.Parameters.AddWithValue("@notecontent", note.NoteContent);
                        cmd.Parameters.AddWithValue("@notetitle", note.NoteTitle);
                        var result = await cmd.ExecuteNonQueryAsync();
                        if (result == 1) return true;
                    }
                }

                //string sql = "INSERT INTO `MyNote`(`UserName`, `NoteType`, `NoteContent`, `NoteTitle`) VALUES ('{0}','{1}','{2}','{3}')";
                //sql = string.Format(sql, user.UserName, note.NoteType, note.NoteContent, note.NoteTitle);
                //var result = await ExcuteNonQuery(sql);
                //if (result == 1) return true;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 更新日报
        /// </summary>
        /// <param name="account"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> UpdateNote(Note note, Note oldNote, User user)
        {
            try
            {
                using (var conn = new MySqlConnection(Config._connectStr))
                {
                    await conn.OpenAsync();
                    using (var cmd = conn.CreateCommand())
                    {
                        string sql = "UPDATE `MyNote` SET `NoteType`= @notetype,`NoteContent`= @notecontent,`NoteTitle`= @notetitle WHERE UserName = @username and NoteTitle = @notetitle";
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("@username", user.UserName);
                        cmd.Parameters.AddWithValue("@notetype", note.NoteType);
                        cmd.Parameters.AddWithValue("@notecontent", note.NoteContent);
                        cmd.Parameters.AddWithValue("@notetitle", note.NoteTitle);
                        var result = await cmd.ExecuteNonQueryAsync();
                        if (result == 1) return true;
                    }
                }

                //string sql = "UPDATE `MyNote` SET `NoteType`='{0}',`NoteContent`='{1}',`NoteTitle`='{2}' WHERE UserName = '{3}' and NoteTitle = '{4}'";
                //sql = string.Format(sql, note.NoteType, note.NoteContent, note.NoteTitle, user.UserName, oldNote.NoteTitle);
                //var result = await ExcuteNonQuery(sql);
                //if (result == 1) return true;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }
            return false;
        }

    }
}
