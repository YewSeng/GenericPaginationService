using System;
using System.Collections.Generic;

namespace PaginationDemo.Utilities.Repositories
{
    public interface IPaginationRepository<T> where T : class
    {
        IEnumerable<T> GetPage(int page, int pageSize);
        (IEnumerable<T> Data, int TotalCount) GetPageBySearchTypeAndSearchTerm(int page, int pageSize, Dictionary<string, object> searchCriteria);
        int GetTotalCount();
        int GetTotalPages(int pageSize);
        bool HasNextPage(int page, int pageSize);
        bool HasPreviousPage(int page);
    }
}
