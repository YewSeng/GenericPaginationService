using PaginationDemo.Models;
using PaginationDemo.Response;
using PaginationDemo.Utilities;
using PaginationDemo.Utilities.Repositories;
using System.Linq;

namespace PaginationDemo.Service
{
    public class ExternalPatronService
    {
        private readonly ExternalPatronPage _externalPatronPage;

        public ExternalPatronService(ExternalPatronPage externalPatronPage)
        {
            _externalPatronPage = externalPatronPage;
        }

        public PagedResponse<ExternalPatron> GetAllExternalPatrons(int page, int pageSize)
        {
            var allPatrons = _externalPatronPage.GetPage(page, pageSize);
            int totalCount = _externalPatronPage.GetTotalCount();
            return new PagedResponse<ExternalPatron>(allPatrons, totalCount, page, pageSize);
        }

        public PagedResponse<ExternalPatron> FilterExternalPatrons(Dictionary<string, object> searchCriteria, int page, int pageSize)
        {
            var (filteredPatrons, totalCount) = _externalPatronPage.GetPageBySearchTypeAndSearchTerm(page, pageSize, searchCriteria);
            return new PagedResponse<ExternalPatron>(filteredPatrons, totalCount, page, pageSize);
        }
    }
}


