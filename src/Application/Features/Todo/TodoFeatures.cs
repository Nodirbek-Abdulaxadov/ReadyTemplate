namespace Application.Features.Todo;

public sealed class TodoFeatures(IApplicationDbContext dbContext)
{
    [Query]
    public async Task<TableResponse<TodoView>> GetAllAsync(TableOptions options, CancellationToken cancellationToken = default)
    {
        var entities = dbContext.Todos.AsNoTracking();
        entities = Sorting(entities, options);

        int count = await entities.CountAsync(cancellationToken);
        var items = await entities.Paging(options).ToListAsync(cancellationToken);

        return new()
        {
            Total = count,
            TotalPages = options.TotalPages(count),
            Items = items.MapToViewList()
        };
    }

    [Query]
    public async Task<TodoView> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Todo '{id}' topilmadi");

        return entity.MapToView();
    }

    [Command]
    public Task<TodoView> CreateAsync(CreateTodoView view, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TodoView> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<TodoView> UpdateAsync(TodoView view, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static IQueryable<TodoEntity> Sorting(IQueryable<TodoEntity> source, TableOptions options)
        => options.SortLabel switch
        {
            nameof(TodoView.Title) => source.Ordering(options, x => x.Title),
            nameof(TodoView.Description) => source.Ordering(options, x => x.Description),
            nameof(TodoView.Deadline) => source.Ordering(options, x => x.Deadline),
            nameof(TodoView.IsDone) => source.Ordering(options, x => x.IsDone),
            _ => source.Ordering(options, x => x.Id),
        };
}
