using EscolaOnLine.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EscolaOnLine.Data;

public class EscolaDbContext : IdentityDbContext<IdentityUser>
{
    public EscolaDbContext(DbContextOptions<EscolaDbContext> options)
            : base(options)
    {
    }
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.Categoria)
                    .HasDatabaseName("IX_Courses_Categoria");
        });

        builder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(s => s.Id);

            entity.HasIndex(s => s.UserId)
                    .HasDatabaseName("IX_Students_UserId");
        });

        builder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollaments");
            entity.HasKey(e => new { e.CourseId, e.StudentId });

            entity.HasIndex(e => new { e.CourseId, e.StudentId })
                                .IsUnique()
                                .HasDatabaseName("IX_Enrollments_Course_Student");


            entity.HasOne(e => e.Course)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Enrollments)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

        });

    }

}