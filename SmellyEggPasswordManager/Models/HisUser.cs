using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Models
{
    /// <summary>
    /// his基本的用户信息
    /// </summary>
    internal class HisUser
    {
        public string UserName { get; set; }

        public string UserPassword { get; set; }

        public string UserCode { get; set; }

        public bool ValidState { get; set; }
    }
}
