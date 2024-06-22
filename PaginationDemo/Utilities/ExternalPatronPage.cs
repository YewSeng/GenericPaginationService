using Microsoft.EntityFrameworkCore;
using PaginationDemo.Models;
using PaginationDemo.Utilities.Implementations;
using PaginationDemo.Utilities.Repositories;
using System.Linq.Expressions;

namespace PaginationDemo.Utilities
{
    public class ExternalPatronPage : PaginationImplementation<ExternalPatron>
    {
        private readonly IQueryable<ExternalPatron> _queryableExternalPatrons;
        private readonly IQueryBuilder<ExternalPatron> _queryBuilder;
        public ExternalPatronPage(ExternalPatronDbContext context, IQueryBuilder<ExternalPatron> queryBuilder) : 
            base(context, queryBuilder, "DateJoined")
        {
            _queryableExternalPatrons = context.Set<ExternalPatron>();
            _queryBuilder = queryBuilder;
        }

        public override IEnumerable<ExternalPatron> GetPage(int page, int pageSize)
        {
            // The default sorting order is ExternalPatronId
            return base.GetPage(page, pageSize);
        }

        public override (IEnumerable<ExternalPatron> Data, int TotalCount) GetPageBySearchTypeAndSearchTerm(int page, int pageSize,
            Dictionary<string, object> searchCriteria)
        {
            // Default sort order is ExternalPatronId
            return base.GetPageBySearchTypeAndSearchTerm(page, pageSize, searchCriteria);
        }
    }
}
