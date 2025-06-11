using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UserAuth.Repository.Models;

public partial class UserAuthContext : DbContext
{
    public UserAuthContext()
    {
    }

    public UserAuthContext(DbContextOptions<UserAuthContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<User2faAuth> User2faAuths { get; set; }

    public virtual DbSet<UsersHistory> UsersHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=UserAuth;Username=postgres;password=tatva123");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.MessagePayload)
                .HasColumnType("character varying")
                .HasColumnName("message_payload");
            entity.Property(e => e.ReciverId).HasColumnName("reciver_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
        });

        modelBuilder.Entity<PasswordResetRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("password_reset_requests_pkey");

            entity.ToTable("password_reset_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Closedate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("closedate");
            entity.Property(e => e.Createdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdate");
            entity.Property(e => e.Guidtoken)
                .HasMaxLength(255)
                .HasColumnName("guidtoken");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .HasColumnName("address");
            entity.Property(e => e.Age).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdAt");
            entity.Property(e => e.DateOfBirth)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deletedAt");
            entity.Property(e => e.DeletedById)
                .HasDefaultValue(0)
                .HasColumnName("deleted_by_id");
            entity.Property(e => e.EditedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("editedAt");
            entity.Property(e => e.EditedById)
                .HasDefaultValue(0)
                .HasColumnName("edited_by_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .HasColumnName("first_name");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("Is_Deleted");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .HasColumnName("phone_number");
            entity.Property(e => e.Pincode).HasColumnName("pincode");
            entity.Property(e => e.Role).HasDefaultValue(0);
            entity.Property(e => e.UserName)
                .HasMaxLength(255)
                .HasColumnName("user_name");
        });

        modelBuilder.Entity<User2faAuth>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("user_2fa_auth_pkey");

            entity.ToTable("user_2fa_auth");

            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Counting)
                .HasDefaultValue(0)
                .HasColumnName("counting");
            entity.Property(e => e.CreateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("create_time");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.ExpireTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expire_time");
            entity.Property(e => e.RememberMe)
                .HasDefaultValue(false)
                .HasColumnName("remember_me");
            entity.Property(e => e.TokenAuth)
                .HasColumnType("character varying")
                .HasColumnName("token_auth");
        });

        modelBuilder.Entity<UsersHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("users_history_pkey");

            entity.ToTable("users_history");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.History)
                .HasMaxLength(500)
                .HasColumnName("history");
            entity.Property(e => e.Operation).HasColumnName("operation");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
