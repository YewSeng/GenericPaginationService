namespace PaginationDemo.Response
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int TotalCount { get; set; }
        public int CurrentCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

        public PagedResponse(IEnumerable<T> data, int count, int page, int size)
        {
            Data = data;
            TotalCount = count;
            CurrentCount = data.Count();
            PageSize = size;
            CurrentPage = page;
            TotalPages = (int)Math.Ceiling(count / (double)size);
            HasNextPage = page < TotalPages;
            HasPreviousPage = page > 1;
        }
    }

}
