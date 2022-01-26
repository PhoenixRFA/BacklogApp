namespace BacklogApp.Services
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
        DateTime Today { get;}
        
        DateTime FixedNow { get; }
        DateTime FixedUtcNow { get; }
        DateTime FixedToday { get; }
        
        void ReleaseNow();
        void ReleaseUtcNow();
        void ReleaseToday();
    }

    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Today => DateTime.Today;

        private DateTime? _now;
        public DateTime FixedNow => _now ??= DateTime.Now;
        public void ReleaseNow() => _now = null;

        private DateTime? _utcNow;
        public DateTime FixedUtcNow => _utcNow ??= DateTime.UtcNow;
        public void ReleaseUtcNow() => _utcNow = null;

        private DateTime? _today;
        public DateTime FixedToday => _today ??= DateTime.Today;
        public void ReleaseToday() => _today = null;
    }
}
