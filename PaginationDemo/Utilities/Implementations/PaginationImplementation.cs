using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PaginationDemo.Models;
using PaginationDemo.Utilities.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace PaginationDemo.Utilities.Implementations
{
    public class PaginationImplementation<T> : IPaginationRepository<T> where T : class
    {
        protected readonly ExternalPatronDbContext _context;
        private readonly string _defaultSortProperty;

        public PaginationImplementation(ExternalPatronDbContext context, IQueryBuilder<T> queryBuilder, string defaultSortProperty)
        {
            _context = context;
            _defaultSortProperty = defaultSortProperty;
        }
        public virtual IEnumerable<T> GetPage(int page, int pageSize)
        {
            var propertyName = _defaultSortProperty;
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);
            var expression = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), property.Type }, _context.Set<T>().AsQueryable().Expression, lambda);
            var sortedQuery = _context.Set<T>().AsQueryable().Provider.CreateQuery<T>(expression);
            var paginatedQuery = sortedQuery.Skip((page - 1) * pageSize).Take(pageSize);
            return paginatedQuery.ToList();
        }

        public virtual (IEnumerable<T> Data, int TotalCount) GetPageBySearchTypeAndSearchTerm(int page, int pageSize, Dictionary<string, object> searchCriteria)
        {
            if (searchCriteria == null || searchCriteria.Count == 0)
            {
                var data = GetPage(page, pageSize);
                var totalCountData = GetTotalCount();
                return (data, totalCountData);
            }

            var query = GetQueryable();
            var (filteredQuery, totalCountFiltered) = ApplySearchCriteria(query, searchCriteria);

            // Sorting logic
            var propertyName = _defaultSortProperty;
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);
            var expression = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { typeof(T), property.Type }, filteredQuery.Expression, lambda);
            var sortedQuery = filteredQuery.Provider.CreateQuery<T>(expression);

            // Pagination logic
            var paginatedData = sortedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return (paginatedData, totalCountFiltered);
        }

        public int GetTotalCount()
        {
            return _context.Set<T>().Count();
        }

        public int GetTotalPages(int pageSize)
        {
            int totalCount = GetTotalCount();
            return (int)Math.Ceiling((decimal)totalCount / pageSize);
        }

        public bool HasNextPage(int page, int pageSize)
        {
            return page < GetTotalPages(pageSize);
        }

        public bool HasPreviousPage(int page)
        {
            return page > 1;
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return _context.Set<T>().AsQueryable();
        }

        protected virtual (IQueryable<T> FilteredQuery, int TotalCount) ApplySearchCriteria(IQueryable<T> query, Dictionary<string, object> searchCriteria)
        {
            IQueryable<T> filteredQuery = query;
            foreach (var criterion in searchCriteria)
            {
                var propertyName = criterion.Key;
                var propertyValue = criterion.Value;

                var property = typeof(T).GetProperties()
                    .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
                if (property == null) continue;

                var parameter = Expression.Parameter(typeof(T), "x");
                var propertyExpression = Expression.Property(parameter, property);
                var propertyType = property.PropertyType;

                Expression? predicate = null;

                if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
                {
                    predicate = HandleDateTimeSearch(propertyExpression, propertyValue);
                }
                else
                {
                    predicate = HandleOtherTypesSearch(propertyExpression, propertyType, propertyValue);
                }

                if (predicate != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(predicate, parameter);
                    filteredQuery = filteredQuery.Where(lambda);
                }
            }

            int totalCount = filteredQuery.Count();
            return (filteredQuery, totalCount);
        }

        private DateTime ParseMinDate(string? minDateStr)
        {
            if (string.IsNullOrEmpty(minDateStr)) return DateTime.MinValue;

            if (DateTime.TryParseExact(minDateStr, new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss.fff" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime minDate))
            {
                return minDate;
            }

            return DateTime.MinValue;
        }

        private DateTime ParseMaxDate(string? maxDateStr)
        {
            if (string.IsNullOrEmpty(maxDateStr)) return DateTime.MaxValue;

            // If parsing as datetime fails, parse it as date
            if (DateTime.TryParseExact(maxDateStr, new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd HH:mm:ss.fff" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime maxDate))
            {
                // Set the time part of the maxDate to the end of the day (23:59:59.999)
                maxDate = maxDate.Date.AddDays(1).AddTicks(-1);
                return maxDate;
            }

            return DateTime.MaxValue;
        }

        private Expression HandleSingleDateTimeSearch(MemberExpression propertyExpression, string? dateString)
        {
            DateTime date = ParseMinDate(dateString);
            DateTime startOfDay = date;
            DateTime endOfDay = date.Date.AddDays(1).AddTicks(-1);

            var startOfDayExpression = Expression.Constant((DateTime?)startOfDay, typeof(DateTime?));
            var endOfDayExpression = Expression.Constant((DateTime?)endOfDay, typeof(DateTime?));
            var greaterThanOrEqualExpression = Expression.GreaterThanOrEqual(propertyExpression, startOfDayExpression);
            var lessThanOrEqualExpression = Expression.LessThanOrEqual(propertyExpression, endOfDayExpression);

            return Expression.AndAlso(greaterThanOrEqualExpression, lessThanOrEqualExpression);
        }
        private Expression? HandleDateTimeSearch(MemberExpression propertyExpression, object propertyValue)
        {
            if (propertyValue == null) return null;

            // Handle propertyValue as JsonElement
            if (propertyValue is JsonElement propertyValueElement)
            {
                if (propertyValueElement.ValueKind == JsonValueKind.Array)
                {
                    var dateArray = propertyValueElement.EnumerateArray().ToList();

                    if (dateArray.Count == 2)
                    {
                        var minDateStr = dateArray[0].GetString();
                        var maxDateStr = dateArray[1].GetString();

                        DateTime minDate = ParseMinDate(minDateStr);
                        DateTime maxDate = ParseMaxDate(maxDateStr);

                        var minDateExpression = Expression.Constant((DateTime?)minDate, typeof(DateTime?));
                        var maxDateExpression = Expression.Constant((DateTime?)maxDate, typeof(DateTime?));
                        Expression? predicate = null;

                        if (minDate != DateTime.MinValue && maxDate != DateTime.MaxValue)
                        {
                            var greaterThanOrEqualExpression = Expression.GreaterThanOrEqual(propertyExpression, minDateExpression);
                            var lessThanOrEqualExpression = Expression.LessThanOrEqual(propertyExpression, maxDateExpression);
                            predicate = Expression.AndAlso(greaterThanOrEqualExpression, lessThanOrEqualExpression);
                        }
                        else if (minDate != DateTime.MinValue)
                        {
                            // Search from minDate to max value
                            predicate = Expression.GreaterThanOrEqual(propertyExpression, minDateExpression);
                        }
                        else if (maxDate != DateTime.MaxValue)
                        {
                            // Search from min value to maxDate
                            predicate = Expression.LessThanOrEqual(propertyExpression, maxDateExpression);
                        }

                        return predicate;
                    }
                }
                else if (propertyValueElement.ValueKind == JsonValueKind.String)
                {
                    var dateString = propertyValueElement.GetString();
                    return HandleSingleDateTimeSearch(propertyExpression, dateString);
                }
            }
            else if (propertyValue is string dateString)
            {
                return HandleSingleDateTimeSearch(propertyExpression, dateString);
            }

            return null;
        }

        private Expression? HandleOtherTypesSearch(MemberExpression propertyExpression, Type propertyType, object propertyValue)
        {
            if (propertyType == typeof(string))
            {
                return HandleStringSearch(propertyExpression, propertyValue);
            }
            else if (Nullable.GetUnderlyingType(propertyType)?.IsEnum == true || propertyType.IsEnum)
            {
                return HandleEnumSearch(propertyExpression, propertyType, propertyValue);
            }
            else if (IsNumericType(propertyType))
            {
                return HandleNumericSearch(propertyExpression, propertyType, propertyValue);
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                return HandleDateTimeSearch(propertyExpression, propertyValue);
            }
            return null;
        }

        private Expression? HandleStringSearch(MemberExpression propertyExpression, object propertyValue)
        {
            if (propertyValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var searchValues = jsonElement.EnumerateArray()
                                              .Where(x => x.ValueKind == JsonValueKind.String)
                                              .Select(x => x.GetString()?.ToLower())
                                              .Where(x => !string.IsNullOrEmpty(x))
                                              .ToList();

                if (searchValues.Count == 0) return null;

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                if (toLowerMethod == null || containsMethod == null) return null;

                Expression? combinedExpression = null;

                foreach (var stringValue in searchValues)
                {
                    var toLowerExpression = Expression.Call(propertyExpression, toLowerMethod);
                    var searchExpression = Expression.Constant(stringValue);
                    var containsExpression = Expression.Call(toLowerExpression, containsMethod, searchExpression);

                    combinedExpression = combinedExpression == null
                        ? containsExpression
                        : Expression.OrElse(combinedExpression, containsExpression);
                }

                return combinedExpression;
            }
            else if (propertyValue != null)
            {
                var stringValue = propertyValue.ToString()?.ToLower();
                if (stringValue == null) return null;

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

                if (toLowerMethod == null || containsMethod == null) return null;

                var toLowerExpression = Expression.Call(propertyExpression, toLowerMethod);
                var searchExpression = Expression.Constant(stringValue);
                return Expression.Call(toLowerExpression, containsMethod, searchExpression);
            }

            return null;
        }

        private Expression? HandleEnumSearch(MemberExpression propertyExpression, Type propertyType, object propertyValue)
        {
            Type enumType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (propertyValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                var valuesList = jsonElement.EnumerateArray()
                                            .Where(x => x.ValueKind == JsonValueKind.Number)
                                            .Select(x => x.GetInt32())
                                            .Cast<object>()
                                            .ToList();

                if (valuesList.Count > 0)
                {
                    return HandleEnumEnumerableSearch(propertyExpression, propertyType, valuesList);
                }
            }
            else if (Enum.TryParse(enumType, propertyValue.ToString(), out var enumValue))
            {
                var valueExpression = Expression.Constant(enumValue, propertyType);
                return Expression.Equal(propertyExpression, valueExpression);
            }

            return null;
        }

        private Expression? HandleEnumEnumerableSearch(MemberExpression propertyExpression, Type propertyType, IEnumerable<object> valuesList)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (!underlyingType.IsEnum) return null;

            var enumValues = valuesList
                .Where(value => value != null && Enum.IsDefined(underlyingType, value))
                .Select(value => Enum.ToObject(underlyingType, value))
                .ToList();

            if (enumValues.Count == 0)
            {
                return null;
            }

            if (enumValues.Count == 1)
            {
                // If only one enum value provided, handle it as a single value
                var valueExpression = Expression.Constant(enumValues[0], propertyType);
                return Expression.Equal(propertyExpression, valueExpression);
            }
            else
            {
                // If multiple enum values provided, create an OR expression for each value
                Expression? combinedExpression = null;
                foreach (var value in enumValues)
                {
                    var valueExpression = Expression.Constant(value, propertyType);
                    var equalityExpression = Expression.Equal(propertyExpression, valueExpression);
                    combinedExpression = combinedExpression == null
                        ? equalityExpression
                        : Expression.OrElse(combinedExpression, equalityExpression);
                }
                return combinedExpression;
            }
        }
        private Expression? HandleNumericSearch(MemberExpression propertyExpression, Type propertyType, object propertyValue)
        {
            Type nonNullableType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (propertyValue is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.ValueKind switch
                {
                    JsonValueKind.Number when nonNullableType == typeof(int) =>
                        CreateNumericExpression<int>(propertyExpression, jsonElement.GetInt32(), propertyType),
                    JsonValueKind.Number when nonNullableType == typeof(long) =>
                        CreateNumericExpression<long>(propertyExpression, jsonElement.GetInt64(), propertyType),
                    JsonValueKind.Number when nonNullableType == typeof(float) =>
                        CreateNumericExpression<float>(propertyExpression, jsonElement.GetSingle(), propertyType),
                    JsonValueKind.Number when nonNullableType == typeof(double) =>
                        CreateNumericExpression<double>(propertyExpression, jsonElement.GetDouble(), propertyType),
                    JsonValueKind.Number when nonNullableType == typeof(decimal) =>
                        CreateNumericExpression<decimal>(propertyExpression, jsonElement.GetDecimal(), propertyType),
                    _ => null
                };
            }
            else if (propertyValue is int intValue)
            {
                var valueExpression = Expression.Constant(intValue);
                return Expression.Equal(propertyExpression, valueExpression);
            }

            return null;
        }

        private Expression CreateNumericExpression<TValue>(MemberExpression propertyExpression, TValue numericValue, Type propertyType)
        {
            var valueExpression = Expression.Constant(numericValue, propertyType);
            return Expression.Equal(propertyExpression, valueExpression);
        }

        private bool IsNumericType(Type type)
        {
            // Check if the type is a nullable type and get its underlying type if it is
            Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

            // Check if the non-nullable type is a numeric type
            return nonNullableType == typeof(int) || nonNullableType == typeof(double) ||
                   nonNullableType == typeof(float) || nonNullableType == typeof(decimal) ||
                   nonNullableType == typeof(byte) || nonNullableType == typeof(sbyte) ||
                   nonNullableType == typeof(short) || nonNullableType == typeof(ushort) ||
                   nonNullableType == typeof(uint) || nonNullableType == typeof(long) ||
                   nonNullableType == typeof(ulong);
        }
    }
}
