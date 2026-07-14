namespace Application.Features.Todo.Validators;

public class UpdateTodoViewValidator : AbstractValidator<UpdateTodoView>
{
    public UpdateTodoViewValidator()
    {
        Include(new CreateTodoViewValidator());

        RuleFor(x => x.Id).NotEmpty();
    }
}

public class UpdateTodoValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoValidator()
        => RuleFor(x => x.View).SetValidator(new UpdateTodoViewValidator());
}
