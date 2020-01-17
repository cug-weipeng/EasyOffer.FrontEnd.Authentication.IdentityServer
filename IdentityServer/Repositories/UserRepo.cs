using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyOffer.FrontEnd.Authentication.IdentityServer.Entities;
using MySql.Data.MySqlClient;
using Dapper;
using System.Data.Common;

namespace EasyOffer.FrontEnd.Authentication.IdentityServer.Repositories
{
    public class UserRepo : IUserRepo
    {
        DbConnection _MasterConnection = null;
        DbConnection _SlaveConnection = null;

        private const string CONNECTIONSTRING = "server=HGH1-DEV01.qqtoa.com;database=easyoffer;uid=root;pwd=Test@1234;charset=utf8mb4";
        private const string CONNECTIONSTRING_READONLY = "server=HGH1-DEV01.qqtoa.com;database=easyoffer;uid=root;pwd=Test@1234;AllowUserVariables=True;charset=utf8mb4";

        private const string SQL_SELECT_BY_USERID = "SELECT * FROM `user` WHERE `UserId` = ?UserId";
        private const string SQL_SELECT_BY_PROVIDER = "SELECT * FROM `user` WHERE `ExternalProvider` = ?ExternalProvider AND `ExternalSubjectId`= ?ExternalSubjectId";
        private const string SQL_SELECT_BY_EMAIL = "SELECT * FROM `user` WHERE `Email` = ?Email";
        private const string SQL_INSERT = "INSERT INTO `user` (`UserId`,`ExternalProvider`,`ExternalSubjectId`,`FullName`, `FirstName`, `LastName`, `Email`, `PhoneNumber`, `Password`, `Avatar`, `IPAddress`, `Integrals`, `Level`, `Status`, `Remark`, `EmployeeId`, `EmployeeName`, `CreatedTime`, `UpdatedTime`, `Verified`) VALUES (?UserId, ExternalProvider,?ExternalSubjectId,?FullName, ?FirstName, ?LastName, ?Email, ?PhoneNumber, ?Password, ?Avatar, ?IPAddress, ?Integrals, ?Level, ?Status, ?Remark, ?EmployeeId, ?EmployeeName, ?CreatedTime, ?UpdatedTime,?Verified)";
        private const string SQL_UPDATE_USER_BY_ID = @"UPDATE `user` SET `FullName` = ?FullName, `FirstName` = ?FirstName, `LastName` = ?LastName, `Gender` = ?Gender, `BirthDate` = ?BirthDate, `Email` = ?Email, `PhoneNumber` = ?PhoneNumber, `Password` = ?PASSWORD, `Avatar` = ?Avatar, `IPAddress` = ?IPAddress, `Integrals` = ?Integrals, `Level` = ?LEVEL, `NextEvaluationTime` = ?NextEvaluationTime, `Disabled` = ?Disabled, `Remark` = ?Remark, `EmployeeId` = ?EmployeeId, `EmployeeName` = ?EmployeeName, `UpdatedTime` = ?UpdatedTime, `Status` = ?STATUS, `Verified`=?Verified,`Promoter`=?Promoter,`PaymentPassword`=?PaymentPassword  WHERE `UserId` = ?UserId";
        private const string SQL_SELECT_SECRETEKEY = @"SELECT * FROM `user_secretkey` WHERE `UserId` = ?UserId And `ExpiredTime` > ?ExpiredTime And `Deleted`=false";

        public UserRepo()
        {
            _MasterConnection = new MySqlConnection(CONNECTIONSTRING);
            _SlaveConnection = new MySqlConnection(CONNECTIONSTRING_READONLY);
        }

        public UserEntity GetUserByUserId(long userId)
        {
            return _SlaveConnection.QueryFirstOrDefault<UserEntity>(SQL_SELECT_BY_USERID, new { UserId = userId });
        }

        public UserEntity GetUserByProvider(string provider,string uid)
        {
            return _SlaveConnection.QueryFirstOrDefault<UserEntity>(SQL_SELECT_BY_PROVIDER, new { ExternalProvider = provider, ExternalSubjectId = uid });
        }

        public UserEntity GetUserByEmail(string email)
        {
            return _SlaveConnection.QueryFirstOrDefault<UserEntity>(SQL_SELECT_BY_EMAIL, new { Email = email });
        }

        public void Insert(UserEntity entity)
        {
            _MasterConnection.Execute(SQL_INSERT, entity);
        }

        public void UpdateUserById(UserEntity entity)
        {
            _MasterConnection.Execute(SQL_UPDATE_USER_BY_ID, entity);
        }

        //public IEnumerable<UserSecretKeyEntity> GetUserSecret(long userId)
        //{
        //    return _SlaveConnection.Query<UserSecretKeyEntity>(SQL_SELECT_SECRETEKEY, new { UserId = userId, ExpiredTime = DateTime.Now });
        //}

        //public void InsertSecreteKey(UserSecretKeyEntity entity)
        //{
        //    _database.Master.Insert(entity, _database.Transaction);
        //}

        //public void UpdateSecretKey(UserSecretKeyEntity entity)
        //{
        //    _database.Master.Update(entity, _database.Transaction);
        //}
    }
}
