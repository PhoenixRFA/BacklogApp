namespace BacklogApp.IntegrationTests.Helpers
{
    public class SingleResultsModel<T> where T : class
    {
        public T? Item { get; set; }
    }
}
