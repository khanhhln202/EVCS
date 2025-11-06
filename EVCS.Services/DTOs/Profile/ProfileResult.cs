namespace EVCS.Services.DTOs.Profile
{
    public class ProfileResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public UserProfileDto? Profile { get; set; }
    }
}