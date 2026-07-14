namespace Infrastructure.Data.Configurations;

public sealed class AuditConfiguration : IEntityTypeConfiguration<AuditEntity>
{
    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.ToTable("audit_entities", "log");
        builder.Property(x => x.TableName).HasMaxLength(100);
        builder.Property(x => x.OldValue).HasColumnType("jsonb");
        builder.Property(x => x.NewValue).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TableName, x.EntityId });
    }
}
