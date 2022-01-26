using BacklogApp.Services;

namespace BacklogApp.Models.Db
{
    public class RefreshToken
    {
        public string Token { get; set; } = default!;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedFromIp { get; set; } = default!;
        public DateTime? Revoked { get; set; }
        public string? RevokedFromIp { get; set; }
        public string? ReplacedBy { get; set; }
        public string? Reason { get; set; }

        public bool IsRevoked => Revoked.HasValue;
        public bool IsExpired => SystemTime.UtcNow > Expires;
        public bool IsActive => !IsRevoked && !IsExpired;

        public void Revoke(string ip, string? reason = null, string? newToken = null)
        {
            Revoked = SystemTime.UtcNow;
            RevokedFromIp = ip;
            ReplacedBy = newToken;
            Reason = reason;
        }
    }
}
