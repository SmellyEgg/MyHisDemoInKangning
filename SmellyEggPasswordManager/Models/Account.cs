namespace SmellyEggPasswordManager.Models
{
    /// <summary>
    /// 账户类
    /// </summary>
    public class Account
    {
        private string accountName = string.Empty;

        private string accountPassword = string.Empty;

        private string accountType = string.Empty;

        /// <summary>
        /// 用户名
        /// </summary>
        public string AccountName { get => accountName; set => accountName = value; }
        /// <summary>
        /// 用户密码
        /// </summary>
        public string AccountPassword { get => accountPassword; set => accountPassword = value; }
        /// <summary>
        /// 用户类型
        /// </summary>
        public string AccountType { get => accountType; set => accountType = value; }
    }
}
