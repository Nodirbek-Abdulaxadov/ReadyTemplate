namespace Application.Common.Extensions;

internal static class QueryExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public IQueryable<T> Paging(TableOptions options)
        {
            int page = Math.Max(options.Page, 1);
            return source.Skip((page - 1) * options.PageSize).Take(options.PageSize);
        }

        public IQueryable<T> Ordering<TK>(TableOptions options, Expression<Func<T, TK>> expression)
            => options.Descending is not null && options.Descending == true ?
               source.OrderByDescending(expression) :
               source.OrderBy(expression);
    }

    extension(TableOptions options)
    {
        public int TotalPages(int totalCount)
            => (int)Math.Ceiling((double)totalCount / options.PageSize);
    }
}
