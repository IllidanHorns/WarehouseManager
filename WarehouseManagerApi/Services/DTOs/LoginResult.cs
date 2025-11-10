namespace WarehouseManagerApi.Services.DTOs
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }

        public LoginResult(bool isSuccess, int? userId, string? errorMessage)
        {
            IsSuccess = isSuccess;
            UserId = userId;
            ErrorMessage = errorMessage;
        }
    }
}
