using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testapp.DAL.Models
{
    public class LoginLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string? Device { get; set; }
        public string? Browser { get; set; }
        public string? IPAddress { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public bool IsSuccess { get; set; }
    }
}
