using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskBoard.Api.Domain;

namespace TaskBoard.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);
        b.Property(u => u.Name).HasMaxLength(User.NameMaxLength).IsRequired();
        b.Property(u => u.Email).HasMaxLength(User.EmailMaxLength).IsRequired();
        b.Property(u => u.PasswordHash).IsRequired();
        b.HasIndex(u => u.Email).IsUnique();
    }
}

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(Board.NameMaxLength).IsRequired();
        b.HasIndex(x => x.OwnerId);
        b.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> b)
    {
        b.HasKey(t => t.Id);
        b.Property(t => t.Title).HasMaxLength(TaskItem.TitleMaxLength).IsRequired();
        b.Property(t => t.Description).HasMaxLength(TaskItem.DescriptionMaxLength);
        // Status and Priority are stored as int on purpose: sorting by Priority stays correct.
        b.Property(t => t.Status).IsRequired();
        b.Property(t => t.Priority).IsRequired();

        b.HasIndex(t => new { t.BoardId, t.Status });
        b.HasIndex(t => new { t.AssigneeId, t.Status });

        b.HasOne<Board>().WithMany().HasForeignKey(t => t.BoardId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<User>().WithMany().HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.Restrict);
    }
}
