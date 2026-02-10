namespace Chinese_sale_api.DTO
{

    //public class PagedResponse<T>
    //{
    //    public IEnumerable<T> Data { get; set; }
    //    public int PageNumber { get; set; }
    //    public int PageSize { get; set; }
    //    public int TotalRecords { get; set; }
    //    public int TotalPages { get; set; }

    //    public PagedResponse(IEnumerable<T> data, int pageNumber, int pageSize, int totalRecords)
    //    {
    //        Data = data;
    //        PageNumber = pageNumber;
    //        PageSize = pageSize;
    //        TotalRecords = totalRecords;
    //        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    //    }
    //}

    // PaginationParams.cs
    public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // PagedResult.cs
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}