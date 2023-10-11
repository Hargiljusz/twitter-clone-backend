namespace DataService.Utils
{
    public class PageWrapper<T>
    {
        public IEnumerable<T> Content { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; } = 0;
        public int TotalPageCount { get; set; }

        public PageWrapper(IEnumerable<T> content, int pageSize, int pageNumber, int totalPageCount)
        {
            Content = content;
            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPageCount = totalPageCount;
        }
    }
}
