using Oracle.ManagedDataAccess.Client;
using SmellyEggPasswordManager.Controller.Oracle;
using SmellyEggPasswordManager.Models.HisDemo;
using SmellyEggPasswordManager.Models.HisDemo.HisRoleModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Controller.HisAddtion
{
    public class HisControllerForRole : oracleBase
    {
        /// <summary>
        /// his角色表
        /// </summary>
        /// <returns></returns>
        public async Task<List<HisRole>> GetRole()
        {
            string sql = @"select t.roleid, t.rolename from priv_com_role t";
            try
            {
                var reader = await this.ExcuteQuery(sql);
                if (object.Equals(reader, null)) return null;
                List<HisRole> list = new List<HisRole>();
                while (await reader.ReadAsync())
                {
                    HisRole obj = new HisRole();
                    obj.RoleId = reader.GetString(0);
                    obj.RoleName = reader.GetString(1);
                    list.Add(obj);
                }
                return list;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// his角色资源表
        /// </summary>
        /// <returns></returns>
        public async Task<List<HisRoleResources>> GetRoleResource()
        {
            string sql = @"select t.id, t.name, t.parent_id, t.role_id from priv_com_role_resource t";
            try
            {
                var reader = await this.ExcuteQuery(sql);
                if (object.Equals(reader, null)) return null;
                List<HisRoleResources> list = new List<HisRoleResources>();
                while (await reader.ReadAsync())
                {
                    HisRoleResources obj = new HisRoleResources();
                    obj.ID = reader.GetString(0);
                    obj.Name = reader.GetString(1);
                    obj.ParentID = reader.GetString(2);
                    obj.RoleID = reader.GetString(3);
                    list.Add(obj);
                }
                return list;
            }
            catch (Exception ex)
            {
                this._errMessage = ex.Message;
                return null;
            }
        }


    }
}
