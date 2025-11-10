using WarehouseManager.Core.Models;

namespace WarehouseManager.Wpf.Static
{
    public static class CurrentUser
    {
        public static User? User { get; set; }
        public static int? UserId => User?.Id;
    }
}
