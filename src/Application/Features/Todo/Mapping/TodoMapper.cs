namespace Application.Features.Todo.Mapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class TodoMapper
{
    public static partial TodoView MapToView(this TodoEntity entity);
    public static partial List<TodoView> MapToViewList(this List<TodoEntity> entities);
    public static partial TodoEntity MapFromView(CreateTodoView view);
    public static partial void ApplyTo(this UpdateTodoView view, TodoEntity entity);
}
