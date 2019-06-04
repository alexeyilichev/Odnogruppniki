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

        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Faculty> Faculties { get; set; }
        public virtual DbSet<GroupMessage> GroupMessages { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<PersonalInfo> PersonalInfoes { get; set; }
        public virtual DbSet<PersonalMessage> PersonalMessages { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<University> Universities { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>()
                .HasMany(e => e.Groups)
                .WithRequired(e => e.Department)
                .HasForeignKey(e => e.id_department)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Department>()
                .HasMany(e => e.PersonalInfoes)
                .WithRequired(e => e.Department)
                .HasForeignKey(e => e.id_department)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Faculty>()
                .HasMany(e => e.Departments)
                .WithRequired(e => e.Faculty)
                .HasForeignKey(e => e.id_faculty)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Faculty>()
                .HasMany(e => e.PersonalInfoes)
                .WithRequired(e => e.Faculty)
                .HasForeignKey(e => e.id_faculty)
                .WillCascadeOnDelete(false);

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
                .HasMany(e => e.PersonalInfoes)
                .WithRequired(e => e.Group)
                .HasForeignKey(e => e.id_group)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Group>()
                .HasMany(e => e.Users)
                .WithOptional(e => e.Group)
                .HasForeignKey(e => e.id_group);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.PersonalInfoes)
                .WithRequired(e => e.Role)
                .HasForeignKey(e => e.id_role)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.Users)
                .WithOptional(e => e.Role)
                .HasForeignKey(e => e.id_role);

            modelBuilder.Entity<University>()
                .HasMany(e => e.Departments)
                .WithRequired(e => e.University)
                .HasForeignKey(e => e.id_university)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<University>()
                .HasMany(e => e.Faculties)
                .WithRequired(e => e.University)
                .HasForeignKey(e => e.id_university)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<University>()
                .HasMany(e => e.PersonalInfoes)
                .WithRequired(e => e.University)
                .HasForeignKey(e => e.id_university)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.PersonalInfoes)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.id_user)
                .WillCascadeOnDelete(false);

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
