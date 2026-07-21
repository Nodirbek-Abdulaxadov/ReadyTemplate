namespace ReadyTemplate.UnitTests.Todo;

public class CreateTodoViewValidatorTests
{
    private readonly CreateTodoViewValidator _validator = new();

    [Fact]
    public void Valid_view_passes()
    {
        var result = _validator.TestValidate(new CreateTodoView { Title = "Read a book" });
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Empty_or_short_title_fails(string title)
    {
        var result = _validator.TestValidate(new CreateTodoView { Title = title });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Past_deadline_fails()
    {
        var result = _validator.TestValidate(new CreateTodoView
        {
            Title = "Valid title",
            Deadline = DateTimeOffset.UtcNow.AddDays(-1),
        });
        result.ShouldHaveValidationErrorFor(x => x.Deadline);
    }

    [Fact]
    public void Future_deadline_passes()
    {
        var result = _validator.TestValidate(new CreateTodoView
        {
            Title = "Valid title",
            Deadline = DateTimeOffset.UtcNow.AddDays(1),
        });
        result.ShouldNotHaveValidationErrorFor(x => x.Deadline);
    }
}
