namespace SmellyEggPasswordManager.Models
{
    /// <summary>
    /// 用户类
    /// </summary>
    public class User
    {
        private string userName = string.Empty;

        private string userPassword = string.Empty;

        private string passwordKey = string.Empty;

        private bool isLogged = false;



        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get => userName; set => userName = value; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string UserPassword { get => userPassword; set => userPassword = value; }

        /// <summary>
        /// 密钥
        /// </summary>
        public string PasswordKey { get => passwordKey; set => passwordKey = value; }

        /// <summary>
        /// 是否已经登陆成功
        /// </summary>
        public bool IsLogged { get => isLogged; set => isLogged = value; }
    }
}
