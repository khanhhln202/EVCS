namespace EVCS.Services.DTOs.Auth
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public string? UserId { get; set; }
        public string? UserName { get; set; }
    }
}