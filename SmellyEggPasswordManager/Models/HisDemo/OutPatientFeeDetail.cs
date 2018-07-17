using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmellyEggPasswordManager.Models.HisDemo
{
    internal class OutPatientFeeDetail
    {
        public string 患者编号 = string.Empty;

        public string 年龄 = string.Empty;

        public string 出生日期 = string.Empty;

        public string 性别 = string.Empty;

        public string 疾病名称 = string.Empty;

        public string ICD10编码 = string.Empty;

        public string 就诊日期 = string.Empty;

        public string 就诊科室 = string.Empty;

        public double 门诊总费用 = 0;

        public  double 治疗费 = 0;

        public double 药品费 = 0;

        public double 西药 = 0;

        public double 中成药 = 0;

        public double 中草药 = 0;

        public double 挂号费 = 0;

        public double 诊察费 = 0;

        public double 检查费 = 0;

        public double 手术费 = 0;

        public double 化验费 = 0;

        public double 其他费用 = 0;

        public string 参保类型 = string.Empty;

        public double 保险统筹基金支付费用 = 0;

        public double 个人账户支付费用 = 0;

        public double 患者自付费用 = 0;

        public double 医疗救助负担 = 0;

        public string PactCode = string.Empty;



    }
}
