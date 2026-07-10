namespace Application.Common.Extensions;

internal static class QueryExtensions
{
    extension<T>(IQueryable<T> source)
    {
        public IQueryable<T> Paging(TableOptions options)
            => source.Skip(options.Page > 0 ? options.Page - 1 : 0)
                      .Take(options.PageSize);

        public IQueryable<T> Ordering<TK>(TableOptions options, Expression<Func<T, TK>> expression)
            => options.Descending ?
               source.OrderByDescending(expression) :
               source.OrderBy(expression);
    }

    extension(TableOptions options)
    {
        public int TotalPages(int totalCount)
            => (int)Math.Ceiling((double)totalCount / options.PageSize);
    }
}
