namespace BuildingBlocks.Infrastructure.Persistence;

// Applied by ModuleDbContext for every module, so the audit trail lands in the
// module's own schema (isolation is preserved — no shared audit table).
internal sealed class AuditConfiguration : IEntityTypeConfiguration<AuditEntity>
{
    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.ToTable("audit_entities");
        builder.Property(x => x.TableName).HasMaxLength(100);
        builder.Property(x => x.OldValue).HasColumnType("jsonb");
        builder.Property(x => x.NewValue).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TableName, x.EntityId });
    }
}
