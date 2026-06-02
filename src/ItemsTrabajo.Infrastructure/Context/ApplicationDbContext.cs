using System;
using System.Collections.Generic;
using ItemsTrabajo.Domain.Entities;
using ItemsTrabajo.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ItemsTrabajo.Infrastructure.Context;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserWork> UserWorks { get; set; }

    public virtual DbSet<WorkItem> WorkItems { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\INFOELECT;DataBase=work;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUs).HasName("tbl_user_pk");

            entity.Property(e => e.StatusUs).IsFixedLength();
        });

        modelBuilder.Entity<UserWork>(entity =>
        {
            entity.HasKey(e => e.IdUW).HasName("tbl_user_work_pk");

            entity.Property(e => e.Status).IsFixedLength();

            entity.HasOne(d => d.Item).WithMany(p => p.UserWorks).HasConstraintName("tbl_user_work_tbl_work_items_id_wi_fk");

            entity.HasOne(d => d.User).WithMany(p => p.UserWorks).HasConstraintName("tbl_user_work_tbl_user_id_us_fk");
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.HasKey(e => e.IdWi).HasName("tbl_work_items_pk");

            entity.Property(e => e.Relevance).IsFixedLength();
            entity.Property(e => e.StatusWi).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
