using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TaskManagement.Data.Models;
using TaskManagement.Data.Models.DTOs;

namespace TaskManagement.Data;

public class TaskDbContext : DbContext
{
    public DbSet<TaskItemDto> Tasks { get; set; }

    public string DbPath { get; }
    
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
        // var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        // DbPath = System.IO.Path.Join(path, "tasks.db");
        DbPath = "./Data/tasks.db";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================================

        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.TaskId)
            .ValueGeneratedOnAdd()
            .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.CreationDate)
            .HasDefaultValueSql("datetime('now')");
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.LastStatusChangeDate)
            .HasDefaultValueSql("datetime('now')");
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.Status)
            .HasDefaultValue(TaskStatus.NotStarted);        
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.Name)
            .HasDefaultValue(string.Empty);        
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.Details)
            .HasDefaultValue(string.Empty);
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.Assignees)
            .HasDefaultValue(string.Empty);
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.PlannedTime)
            .HasDefaultValue(0);
        modelBuilder.Entity<TaskItemDto>()
            .Property(t => t.ActualTime)
            .HasDefaultValue(0);
    }
}
