using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace testapp.DAL.Context
{
    public class DapperConnectionFactory
    {
        private readonly IConfiguration _config;
        public DapperConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()
        {
            var connStr = _config.GetConnectionString("DefaultConnection");
            return new SqlConnection(connStr);
        }

        public IDbConnection CreateReportConnection()
        {
            var connStr = _config.GetConnectionString("ReportConnection");
            return new SqlConnection(connStr);
        }
    }
}
