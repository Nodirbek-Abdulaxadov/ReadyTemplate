namespace Application.Features.Todo.Validators;

public class CreateTodoViewValidator : AbstractValidator<CreateTodoView>
{
    public CreateTodoViewValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(200);

        RuleFor(x => x.Deadline)
            .GreaterThan(DateTimeOffset.UtcNow)
            .When(x => x.Deadline.HasValue);
    }
}

public class CreateTodoValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoValidator()
        => RuleFor(x => x.View).SetValidator(new CreateTodoViewValidator());
}
