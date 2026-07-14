namespace Application.Features.Todo;

public sealed class TodoFeatures(IApplicationDbContext dbContext)
{
    [Query]
    public async Task<TableResponse<TodoView>> GetAllAsync(TableOptions options, CancellationToken cancellationToken = default)
    {
        var entities = dbContext.Todos.AsNoTracking();

        entities = ApplyFilters(entities, options);
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
            ?? throw new NotFoundException($"Todo '{id}' not found");

        return entity.MapToView();
    }

    [Command]
    public async Task<TodoView> CreateAsync(CreateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var entity = TodoMapper.MapFromView(command.View);

        dbContext.Todos.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.MapToView();
    }

    [Command]
    public async Task<TodoView> UpdateAsync(UpdateTodoCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Todos
            .FirstOrDefaultAsync(x => x.Id == command.View.Id, cancellationToken)
            ?? throw new NotFoundException($"Todo {command.View.Id} not found");

        TodoMapper.ApplyTo(command.View, entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.MapToView();
    }

    [Command]
    public async Task<Guid> DeleteAsync(DeleteTodoCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Todos
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException($"Todo '{command.Id}' not found");

        dbContext.Todos.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
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

    private static IQueryable<TodoEntity> ApplyFilters(IQueryable<TodoEntity> source, TableOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            source = source.Where(x =>
                EF.Functions.Like(x.Title, $"%{options.Search}%") ||
                (x.Description != null && EF.Functions.Like(x.Description, $"%{options.Search}%")));
        }

        if (options.From.HasValue)
        {
            source = source.Where(x => x.CreatedAt >= options.From.Value);
        }

        if (options.To.HasValue)
        {
            source = source.Where(x => x.CreatedAt <= options.To.Value);
        }

        return source;
    }
}
