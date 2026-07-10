namespace Application.Features.Todo.Validators;

public class UpdateTodoValidator : AbstractValidator<TodoView>
{
    public UpdateTodoValidator()
    {
        Include(new CreateTodoValidator());

        RuleFor(x => x.Id).NotEmpty();
    }
}
