namespace ReadyTemplate.IntegrationTests;

[Collection(nameof(ApiCollection))]
public sealed class TodoEndpointsTests(ApiFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient();

    [Fact]
    public async Task Post_creates_todo_and_returns_it()
    {
        var res = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoView { Title = "Buy milk", Description = "2%" });

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var created = await res.Content.ReadFromJsonAsync<TodoView>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Title.Should().Be("Buy milk");
        created.IsDone.Should().BeFalse();
    }

    [Fact]
    public async Task Get_by_id_returns_created_todo()
    {
        var created = await CreateTodo("Walk the dog");

        var res = await _client.GetAsync($"/api/todos/{created.Id}");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await res.Content.ReadFromJsonAsync<TodoView>();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Get_missing_returns_404()
    {
        var res = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_with_short_title_returns_400_with_errors()
    {
        var res = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoView { Title = "ab" });   // MinimumLength(3)

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await res.Content.ReadFromJsonAsync<ValidationProblem>();
        problem.Should().NotBeNull();
        problem!.Errors.Keys.Should()
            .Contain(k => k.Contains("Title", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Put_updates_todo()
    {
        var created = await CreateTodo("Old title");

        var res = await _client.PutAsJsonAsync("/api/todos",
            new UpdateTodoView { Id = created.Id, Title = "New title", IsDone = true });

        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await res.Content.ReadFromJsonAsync<TodoView>();
        updated!.Title.Should().Be("New title");
        updated.IsDone.Should().BeTrue();
    }

    [Fact]
    public async Task Deleted_todo_is_soft_deleted_and_excluded()
    {
        var created = await CreateTodo("Temp task");

        var del = await _client.DeleteAsync($"/api/todos/{created.Id}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // soft-delete + global query filter -> endi topilmaydi
        var get = await _client.GetAsync($"/api/todos/{created.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_returns_paged_response()
    {
        await CreateTodo("List item");

        var page = await _client.GetFromJsonAsync<TableResponse<TodoView>>(
            "/api/todos?page=1&pageSize=10");

        page.Should().NotBeNull();
        page!.Total.Should().BeGreaterThanOrEqualTo(1);
        page.Items.Should().NotBeEmpty();
    }

    private async Task<TodoView> CreateTodo(string title)
    {
        var res = await _client.PostAsJsonAsync("/api/todos", new CreateTodoView { Title = title });
        res.EnsureSuccessStatusCode();
        return (await res.Content.ReadFromJsonAsync<TodoView>())!;
    }

    // ProblemDetails.errors (GlobalExceptionHandler ValidationException uchun to'ldiradi)
    private sealed record ValidationProblem
    {
        public Dictionary<string, string[]> Errors { get; init; } = [];
    }
}
