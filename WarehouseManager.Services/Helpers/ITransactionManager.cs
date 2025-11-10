using System;
using System.Threading.Tasks;

namespace WarehouseManager.Application.Services;

public interface ITransactionManager
{
    Task<T> ExecuteOrderWorkflowAsync<T>(Func<Task<T>> action);
    Task<T> ExecuteUserWorkflowAsync<T>(Func<Task<T>> action);
    Task<T> ExecuteCatalogWorkflowAsync<T>(Func<Task<T>> action);
}
