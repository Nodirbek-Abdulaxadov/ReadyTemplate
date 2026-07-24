namespace Todos.Infrastructure.Configurations;

internal sealed class TodoConfiguration : IEntityTypeConfiguration<TodoEntity>
{
    public void Configure(EntityTypeBuilder<TodoEntity> builder)
    {
        builder.ToTable("todos");

        builder.Property(x => x.Title).HasMaxLength(200);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasQueryFilter(x => x.Status != Status.Deleted);
    }
}
