namespace Odnogruppniki.Models.DBModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DBContext : DbContext
    {
        public DBContext()
            : base("name=DBContext")
        {
        }

        public static DBContext Create()
        {
            return new DBContext();
        }

        public virtual DbSet<GroupMessage> GroupMessages { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<PersonalMessage> PersonalMessages { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Group>()
                .HasMany(e => e.GroupMessages)
                .WithRequired(e => e.Group)
                .HasForeignKey(e => e.id_in)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Group>()
                .HasMany(e => e.GroupMessages1)
                .WithRequired(e => e.Group1)
                .HasForeignKey(e => e.id_out)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Group>()
                .HasMany(e => e.Users)
                .WithOptional(e => e.Group)
                .HasForeignKey(e => e.id_group);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.Users)
                .WithOptional(e => e.Role)
                .HasForeignKey(e => e.id_role);

            modelBuilder.Entity<User>()
                .HasMany(e => e.PersonalMessages)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.id_in)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.PersonalMessages1)
                .WithRequired(e => e.User1)
                .HasForeignKey(e => e.id_out)
                .WillCascadeOnDelete(false);
        }
    }
}
