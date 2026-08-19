namespace WebApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLogin { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Unverified;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    }

    public enum UserStatus
    {
        Active,
        Blocked,
        Unverified
    }

}
