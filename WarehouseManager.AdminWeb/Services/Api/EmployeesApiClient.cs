using WarehouseManager.Services;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.Employee;

namespace WarehouseManager.AdminWeb.Services.Api;

public class EmployeesApiClient : ApiClientBase
{
    public EmployeesApiClient(HttpClient httpClient) : base(httpClient)
    {
    }

    public Task<PagedResult<EmployeeSummary>> GetPagedAsync(EmployeeFilters filter, CancellationToken cancellationToken = default) =>
        GetAsync<PagedResult<EmployeeSummary>>("api/employees", filter, cancellationToken);

    public Task<EmployeeSummary> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        GetAsync<EmployeeSummary>($"api/employees/{id}", cancellationToken: cancellationToken);

    public Task<EmployeeSummary> CreateAsync(CreateEmployeeCommand command, CancellationToken cancellationToken = default) =>
        PostAsync<CreateEmployeeCommand, EmployeeSummary>("api/employees", command, cancellationToken);

    public Task<EmployeeSummary> UpdateAsync(int id, UpdateEmployeeCommand command, CancellationToken cancellationToken = default) =>
        PutAsync<UpdateEmployeeCommand, EmployeeSummary>($"api/employees/{id}", command, cancellationToken);

    public Task ArchiveAsync(int id, int userId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"api/employees/{id}", new { currentUserId = userId }, cancellationToken);
}

