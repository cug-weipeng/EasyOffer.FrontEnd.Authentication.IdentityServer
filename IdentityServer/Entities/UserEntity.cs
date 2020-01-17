using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer.Entities
{
    public class UserEntity
    {
        /// <summary>
        /// 用户编号。
        /// </summary>	
        public long UserId { get; set; }

        /// <summary>
        /// 第三方登陆渠道
        /// </summary>
        public string ExternalProvider { get; set; }

        /// <summary>
        /// 第三方登陆渠道
        /// </summary>
        public string ExternalSubjectId { get; set; }

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
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 头像
        /// </summary>	
        public string Avatar { get; set; }

        /// <summary>
        /// 注册 IP 地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 用户积分
        /// </summary>	
        public int Integrals { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int Level { get; set; }

        public DateTime? NextEvaluationTime { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// 状态 <see cref="Utility.UserStatus"/>
        /// </summary>	s
        public string Status { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Verified { get; set; }

        /// <summary>
        /// 拉新员工
        /// </summary>
        public string Promoter { get; set; }

        /// <summary>
        /// 支付密码
        /// </summary>
        public string PaymentPassword { get; set; }

        ///// <summary>
        ///// 最后登录时间
        ///// </summary>	
        //public DateTime LastLoginTime { get; set; }

        ///// <summary>
        ///// 最后登录 IP
        ///// </summary>
        //public string LastLoginIP { get; set; }

        ///// <summary>
        ///// 当前登录时间
        ///// </summary>	
        //public DateTime CurrentlyLoginTime { get; set; }

        ///// <summary>
        ///// 当前登录 IP
        ///// </summary>	
        //public string CurrentlyLoginIP { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 员工编号
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// 员工姓名
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>	
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }
    }
}
