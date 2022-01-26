namespace BacklogApp.Services
{
    public static class SystemTime
    {
        private static IDateTimeProvider _instance;

        static SystemTime()
        {
            _instance = new DateTimeProvider();
        }

        public static void SetDateTimeProvider(IDateTimeProvider provider) => _instance = provider;

        public static DateTime Now => _instance.Now;
        public static DateTime UtcNow => _instance.UtcNow;
        public static DateTime Today => _instance.Today;
    }
}
