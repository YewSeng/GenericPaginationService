using PaginationDemo.Models;
using PaginationDemo.Utilities.Repositories;
using System.Linq.Expressions;

namespace PaginationDemo.Utilities.Implementations
{
    public class QueryBuilderImplementation<T> : IQueryBuilder<T> where T : class
    {
        private readonly ExternalPatronDbContext _context;
        private IQueryable<T> _query;

        public QueryBuilderImplementation(ExternalPatronDbContext context)
        {
            _context = context;
            _query = _context.Set<T>();
        }

        public IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
        {
            _query = _query.Where(predicate);
            return this;
        }

        public IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query = _query.OrderBy(keySelector);
            return this;
        }

        public IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            _query = _query.OrderByDescending(keySelector);
            return this;
        }

        public IQueryable<T> Build()
        {
            return _query;
        }
    }
}
