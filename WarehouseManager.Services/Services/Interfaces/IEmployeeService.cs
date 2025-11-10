using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Employee;

namespace WarehouseManager.Services.Services.Interfaces
{
    public interface IEmployeeService
    {
        public Task<EmployeeSummary> CreateAsync(CreateEmployeeCommand command);
        public Task<EmployeeSummary> UpdateAsync(UpdateEmployeeCommand command);
        public Task<bool> ArchiveAsync(int targetEmployeeId, int currentUserId);
        public Task<PagedResult<EmployeeSummary>> GetPagedAsync(IPaginationFilter filter);
    }
}
