namespace WarehouseManager.Services
{
    public class PagedResult<T>
    {
        public required IEnumerable<T> Items { get; init; }
        public required int TotalCount { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
