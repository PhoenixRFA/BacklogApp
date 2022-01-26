using BacklogApp.Models.Db;

namespace BacklogApp.Models.Users
{
    public record RefreshTokenResult(RefreshToken Token, UserModel User);
}
