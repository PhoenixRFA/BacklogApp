namespace BacklogApp.IntegrationTests.Helpers
{
    public class MultipleResultsModel<T> where T : class
    {
        public T[]? Items { get; set; }
    }
}
