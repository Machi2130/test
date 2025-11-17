using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testapp.DAL.Interfaces
{
    public interface IRolePermissionRepo
    {
        Task<IEnumerable<string>> GetRolesByUserIdAsync(int userId);
        Task<IEnumerable<string>> GetPermissionsByUserIdAsync(int userId);
    }
}
