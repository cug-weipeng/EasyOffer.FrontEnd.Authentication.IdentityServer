using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer.Models
{
    public class UserDto
    {
        /// <summary>
        /// 用户编号
        /// </summary>	
        public long UserId { get; set; }

        /// <summary>
        /// 全名
        /// </summary>	
        public string FullName { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// 姓氏
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 生日
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 电子邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 头像
        /// </summary>	
        public string Avatar { get; set; }

        /// <summary>
        /// 用户推广代码
        ///</summary>
        public string RedeemCode { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>	
        public int Integrals { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 用户邮箱是否验证
        /// </summary>
        public bool Verified { get; set; }

        /// <summary>
        /// 下次评估时间
        /// </summary>
        public DateTime? NextEvaluationTime { get; set; }

        /// <summary>
        /// 创建时间。
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
