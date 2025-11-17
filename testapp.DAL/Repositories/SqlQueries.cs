namespace testapp.DAL.Repositories
{
    public class SqlQueries
    {
        public const string reportSql = @"SELECT * FROM MainReport WHERE OperationDate BETWEEN @StartDate AND @EndDate ORDER BY OperationDate DESC";
        public const string allRecordSql= "SELECT * FROM MainReport ORDER BY OperationDate DESC";
        public const string getByIdSql = @"SELECT UserId, Username, Email, PasswordHash, PasswordSalt, IsActive, CreatedAt FROM Users WHERE UserId = @UserId";
        public const string getByUsernameSql = @"SELECT UserId, Username, Email, PasswordHash, PasswordSalt, IsActive, CreatedAt FROM Users WHERE Username = @Username";
        public const string createUserSql = @"
                INSERT INTO Users (Username, Email, PasswordHash, PasswordSalt, IsActive)
                VALUES (@Username, @Email, @PasswordHash, @PasswordSalt, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);";
        public const string roleAssignSql = @"INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";

        public const string getAllUserSql = "SELECT * FROM Users";


        public const string loginLogSql = @"
                INSERT INTO LoginLogs (UserId, Device, IPAddress, LoginTime, IsSuccess)
                VALUES (@UserId, @Device, @IPAddress, @LoginTime, @IsSuccess);
                SELECT CAST(SCOPE_IDENTITY() as int);";

        public const string logsByIdSql = @"SELECT * FROM LoginLogs WHERE UserId = @UserId ORDER BY LoginTime DESC";

        public const string roleByUserIdSql = @"SELECT r.RoleName
                    FROM Roles r
                    INNER JOIN UserRoles ur ON ur.RoleId = r.RoleId
                    WHERE ur.UserId = @UserId";

        public const string perByUserIdSql = @"
                SELECT DISTINCT p.PermissionName
                FROM Permissions p
                INNER JOIN RolePermissions rp ON rp.PermissionId = p.PermissionId
                INNER JOIN UserRoles ur ON ur.RoleId = rp.RoleId
                WHERE ur.UserId = @UserId";

        public const string appLogSql = "SELECT * FROM AppLogs ORDER BY TimeStamp DESC";

        public const string appLogByIdSql = "SELECT * FROM AppLogs WHERE Id = @Id";

        public const string appLogByDateRangeSql = @"SELECT * FROM AppLogs 
                WHERE TimeStamp >= @StartDate AND TimeStamp <= @EndDate
                ORDER BY TimeStamp ASC";
    }
}
