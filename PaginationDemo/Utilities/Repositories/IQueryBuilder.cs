using System.Linq.Expressions;

namespace PaginationDemo.Utilities.Repositories
{
    public interface IQueryBuilder<T> where T : class
    {
        IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate);
        IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);
        IQueryBuilder<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector);
        IQueryable<T> Build();
    }
}
