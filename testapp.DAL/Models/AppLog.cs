using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testapp.DAL.Models
{
    public class AppLog
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
