namespace BacklogApp.Models.Users
{
    public record ChangePasswordModel
    {
        public string? NewPassword { get; init; }
        public string? OldPassword { get; init; }
    }
}
