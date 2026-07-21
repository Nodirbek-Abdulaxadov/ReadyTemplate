namespace ReadyTemplate.UnitTests.Todo;

public class UpdateTodoViewValidatorTests
{
    private readonly UpdateTodoViewValidator _validator = new();

    [Fact]
    public void Empty_id_fails()
    {
        var result = _validator.TestValidate(new UpdateTodoView
        {
            Id = Guid.Empty,
            Title = "Valid title",
        });
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Valid_update_passes()
    {
        var result = _validator.TestValidate(new UpdateTodoView
        {
            Id = Guid.NewGuid(),
            Title = "Valid title",
        });
        result.ShouldNotHaveAnyValidationErrors();
    }
}
