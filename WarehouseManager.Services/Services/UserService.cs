using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Models;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Filters.Interfaces;
using WarehouseManager.Services.Helpers;
using WarehouseManager.Services.Services.Base;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManagerContracts.DTOs.User;
using WarehouseManagerContracts.Validation.User;
using BCrypt.Net;
using WarehouseManager.Core.Data;

namespace WarehouseManager.Application.Services;

public class UserService : ArchiveService<User, UserSummary>, IUserService
{
    private readonly IAuditService _auditService;
    private readonly ITransactionManager _transactionManager;

    public UserService(AppDbContext context, IAuditService auditService, ITransactionManager transactionManager) : base(context)
    {
        _auditService = auditService;
        _transactionManager = transactionManager;
    }

    protected override async Task<UserSummary> MapToSummaryAsync(User entity)
    {
        var role = await _context.Roles.FindAsync(entity.RoleId);
        var hasEmployee = await _context.Employees.AnyAsync(e => e.UserId == entity.Id);
        return new UserSummary
        {
            Id = entity.Id,
            Email = entity.Email,
            FirstName = entity.FirstName,
            MiddleName = entity.MiddleName,
            Patronymic = entity.Patronymic,
            PhoneNumber = entity.PhoneNumber,
            RoleId = entity.RoleId,
            RoleName = role?.RoleName ?? "Без роли",
            IsArchived = entity.IsArchived,
            CreationDatetime = entity.CreationDatetime,
            HasEmployeeRecord = hasEmployee
        };
    }

    protected override async Task<List<UserSummary>> MapToSummariesAsync(List<User> entities)
    {
        var roleIds = entities.Select(u => u.RoleId).ToList();
        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id);

        var userIds = entities.Select(u => u.Id).ToList();
        var employeeFlags = _context.Employees
            .Where(e => userIds.Contains(e.UserId))
            .Select(e => e.UserId)
            .ToHashSet();

        return entities.Select(u => new UserSummary
        {
            Id = u.Id,
            Email = u.Email,
            FirstName = u.FirstName,
            MiddleName = u.MiddleName,
            Patronymic = u.Patronymic,
            PhoneNumber = u.PhoneNumber,
            RoleId = u.RoleId,
            RoleName = roles.GetValueOrDefault(u.RoleId)?.RoleName ?? "Без роли",
            IsArchived = u.IsArchived,
            CreationDatetime = u.CreationDatetime,
            HasEmployeeRecord = employeeFlags.Contains(u.Id)
        }).ToList();
    }

    protected override IQueryable<User> ApplyCustomFilters(IQueryable<User> query, IPaginationFilter filter)
    {
        if (filter is not UserFilter userFilter)
            return query;

        if (!string.IsNullOrWhiteSpace(userFilter.SearchTerm))
        {
            var term = userFilter.SearchTerm.ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.MiddleName.ToLower().Contains(term) ||
                u.PhoneNumber.Contains(term));
        }

        if (userFilter.RoleId.HasValue)
            query = query.Where(u => u.RoleId == userFilter.RoleId.Value);

        return query;
    }

    public Task<string> HashPasswordAsync(string password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public async Task<UserSummary> CreateAsync(CreateUserCommand command)
    {
        return await _transactionManager.ExecuteUserWorkflowAsync(async () =>
        {
            var validator = new CreateUserCommandValidator();
            var result = await validator.ValidateAsync(command);
            if (!result.IsValid)
                throw new ModelValidationException(result.Errors.ToList());

            if (await _context.Users.AnyAsync(u => u.Email == command.Email))
                throw new ConflictException("Пользователь с таким email уже существует");
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == command.PhoneNumber))
                throw new ConflictException("Пользователь с таким телефоном уже существует");

            await new EntityCheckHelper(_context).EnsureExistsAndActive<Role>(command.RoleId);

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(command.Password);

            var user = new User
            {
                Email = command.Email,
                PasswordHash = hashedPassword,
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                Patronymic = command.Patronymic,
                PhoneNumber = command.PhoneNumber,
                RoleId = command.RoleId,
                IsArchived = false,
                CreationDatetime = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _auditService.LogOperation(
                userId: command.UserId,
                action: $"Создание пользователя: {user.Email}",
                tableName: "Users",
                recordId: user.Id.ToString()
            );

            return await MapToSummaryAsync(user);
        });
    }

    public async Task<UserSummary> UpdateAsync(UpdateUserCommand command)
    {
        return await _transactionManager.ExecuteUserWorkflowAsync(async () =>
        {
            var targetUser = await new EntityCheckHelper(_context).EnsureExistsAndActive<User>(command.TargetUserId);

            if (await _context.Users.AnyAsync(u => u.Email == command.Email && u.Id != command.TargetUserId))
                throw new ConflictException("Email уже используется другим пользователем");
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == command.PhoneNumber && u.Id != command.TargetUserId))
                throw new ConflictException("Телефон уже используется другим пользователем");

            await new EntityCheckHelper(_context).EnsureExistsAndActive<Role>(command.RoleId);

            var oldEmail = targetUser.Email;
            targetUser.Email = command.Email;
            targetUser.FirstName = command.FirstName;
            targetUser.MiddleName = command.MiddleName;
            targetUser.Patronymic = command.Patronymic;
            targetUser.PhoneNumber = command.PhoneNumber;
            targetUser.RoleId = command.RoleId;

            if (!string.IsNullOrEmpty(command.NewPassword))
            {
                targetUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
            }

            targetUser.UpdateDatetime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _auditService.LogOperation(
                userId: command.UserId,
                action: $"Обновление пользователя",
                oldValue: $"Email: {oldEmail}",
                newValue: $"Email: {command.Email}",
                tableName: "Users",
                recordId: targetUser.Id.ToString()
            );

            return await MapToSummaryAsync(targetUser);
        });
    }

    public async Task<bool> ArchiveAsync(int targetUserId, int currentUserId)
    {
        var user = await new EntityCheckHelper(_context).EnsureExistsAndActive<User>(targetUserId);

        if (targetUserId == currentUserId)
            throw new DomainException("Нельзя архивировать самого себя");

        var cancelledStatus = await _context.OrderStatuses
            .FirstOrDefaultAsync(s => s.StatusName == "Отменён" && !s.IsArchived);

        var hasActiveOrders = await _context.Orders
            .AnyAsync(o => o.UserId == targetUserId &&
                          !o.IsArchived &&
                          (cancelledStatus == null || o.StatusId != cancelledStatus.Id));

        if (hasActiveOrders)
            throw new ConflictException("Нельзя архивировать пользователя: есть активные заказы");

        user.IsArchived = true;
        user.UpdateDatetime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _auditService.LogOperation(
            userId: currentUserId,
            action: $"Архивирование пользователя: {user.Email}",
            tableName: "Users",
            recordId: user.Id.ToString()
        );

        return true;
    }
}