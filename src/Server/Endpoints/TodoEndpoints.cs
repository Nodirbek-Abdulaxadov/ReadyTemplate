namespace Server.Endpoints;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos");

        group.MapGet("/", ([AsParameters] TableOptions options, ISender sender, CancellationToken ct)
            => sender.TodoFeatures().GetAllAsync(options, ct));

        group.MapGet("/{id:guid}", (Guid id, ISender sender, CancellationToken ct)
            => sender.TodoFeatures().GetAsync(id, ct));

        group.MapPost("/", (CreateTodoView view, ISender sender, CancellationToken ct)
            => sender.Send(new CreateTodoCommand(view), ct));

        group.MapPut("/", (UpdateTodoView view, ISender sender, CancellationToken ct)
            => sender.Send(new UpdateTodoCommand(view), ct));

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            await sender.Send(new DeleteTodoCommand(id), ct);
            return Results.NoContent();
        });

        return group;
    }
}
