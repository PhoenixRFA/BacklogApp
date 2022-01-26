using BacklogApp.Models.Db;

namespace BacklogApp.Models.Auth
{
    public record UserViewModel
    {
        public UserViewModel () { }
        public UserViewModel ( UserModel user )
        {
            if( user == null ) throw new ArgumentNullException ( nameof ( user ) );
            
            Id = user.Id ?? default!;
            Username = user.Name ?? default!;
            Email = user.Email ?? default!;
            Photo = user.PhotoId != null ? $"/api/resources/{user.PhotoId}" : null;
        }

        public string Id { get; init; } = default!;
        public string Username { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? Photo { get; init; }
    }
}
