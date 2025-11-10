using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using WarehouseManager.Services.Exceptions;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManager.Services.Summary;
using WarehouseManager.Wpf.Static;
using Microsoft.EntityFrameworkCore;
using WarehouseManager.Core.Data;
using WarehouseManager.Core.Models;

namespace WarehouseManager.Wpf.ViewModels
{
    public partial class UsersViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _context;

        [ObservableProperty]
        private ObservableCollection<UserSummary> _users = new();

        [ObservableProperty]
        private UserSummary? _selectedUser;

        [ObservableProperty]
        private string _searchTerm = "";

        private Role? _selectedRole;

        public Role? SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRoleSelected));
                // Если выбран элемент с Id = -1 ("Все роли"), сбрасываем фильтр
                if (value != null && value.Id == -1)
                {
                    _selectedRole = null;
                    SelectedRoleId = null;
                    OnPropertyChanged(nameof(SelectedRole));
                }
                else
                {
                    SelectedRoleId = value?.Id;
                }
            }
        }

        public bool IsRoleSelected => SelectedRole != null && SelectedRole.Id != -1;

        private int? _selectedRoleId;

        public int? SelectedRoleId
        {
            get => _selectedRoleId;
            set
            {
                _selectedRoleId = value;
                OnPropertyChanged();
            }
        }

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private int _pageSize = 20;

        public ObservableCollection<int> AvailablePageSizes { get; } = new() { 10, 15, 20, 30 };
        public ObservableCollection<Role> Roles { get; } = new();

        public UsersViewModel(IUserService userService, AppDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        [RelayCommand]
        public async Task LoadUsersAsync()
        {
            await LoadUsersPageAsync(CurrentPage);
        }

        [RelayCommand]
        public async Task LoadRolesAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .Where(r => !r.IsArchived)
                    .OrderBy(r => r.RoleName)
                    .ToListAsync();

                Roles.Clear();
                // Добавляем элемент "Все роли" для сброса фильтра
                Roles.Add(new Role { Id = -1, RoleName = "Все роли", IsArchived = false });
                foreach (var role in roles)
                {
                    Roles.Add(role);
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке ролей: " + ex.Message;
            }
        }

        [RelayCommand]
        private async Task FilterAsync()
        {
            CurrentPage = 1;
            await LoadUsersPageAsync(1);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadUsersPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadUsersPageAsync(CurrentPage);
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadUsersPageAsync(CurrentPage);
        }

        [RelayCommand]
        private void ClearRoleFilter()
        {
            SelectedRole = null;
            _ = FilterAsync();
        }

        [RelayCommand]
        private void OpenCreateWindow()
        {
            var createWindow = new View.CreateUserWindow();
            createWindow.ShowDialog();
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private void OpenUpdateWindow(UserSummary? user)
        {
            var userToEdit = user ?? SelectedUser;
            if (userToEdit == null)
                return;

            SelectedUser = userToEdit;
            var updateWindow = new View.UpdateUserWindow(userToEdit);
            updateWindow.ShowDialog();
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task DeleteUserAsync(UserSummary? user)
        {
            var userToDelete = user ?? SelectedUser;
            if (userToDelete == null)
                return;

            // Подтверждение удаления
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить пользователя?\n\nEmail: {userToDelete.Email}\nФИО: {userToDelete.FirstName} {userToDelete.MiddleName} {userToDelete.Patronymic ?? ""}",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                if (CurrentUser.UserId == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    return;
                }

                await _userService.ArchiveAsync(userToDelete.Id, CurrentUser.UserId.Value);

                MessageBox.Show(
                    "Пользователь успешно удалён!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Перезагружаем список
                await LoadUsersAsync();
            }
            catch (ConflictException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(
                    ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (DomainException ex)
            {
                ErrorMessage = ex.Message;
                MessageBox.Show(
                    ex.Message,
                    "Ошибка удаления",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (System.Exception ex)
            {
                string errorMessage;
                if (ex.Message.Contains("saving the entity changes") || ex.Message.Contains("inner exception"))
                {
                    var innerEx = ex.InnerException;
                    if (innerEx != null)
                    {
                        errorMessage = $"Ошибка сохранения: {innerEx.Message}";
                    }
                    else
                    {
                        errorMessage = "Ошибка сохранения данных. Проверьте корректность введенных данных и права доступа.";
                    }
                }
                else
                {
                    errorMessage = "Ошибка при удалении пользователя: " + ex.Message;
                }

                ErrorMessage = errorMessage;
                MessageBox.Show(
                    errorMessage,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadUsersPageAsync(int page)
        {
            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var filter = new UserFilter
                {
                    Page = page,
                    PageSize = PageSize,
                    IncludeArchived = false,
                    SearchTerm = string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                    RoleId = SelectedRoleId
                };

                var result = await _userService.GetPagedAsync(filter);

                Users.Clear();
                foreach (var user in result.Items)
                {
                    Users.Add(user);
                }

                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
                CurrentPage = page;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "Ошибка при загрузке пользователей: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            _ = LoadUsersPageAsync(1);
        }

        partial void OnSelectedUserChanged(UserSummary? value)
        {
            if (value != null)
            {
                // Можно добавить логику при выборе пользователя
            }
        }
    }
}

