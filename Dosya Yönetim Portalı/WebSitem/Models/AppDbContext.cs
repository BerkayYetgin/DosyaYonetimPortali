using WebSitem.Models;
using AspNetCoreHero.ToastNotification.Notyf.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NETCore.Encrypt.Extensions;

namespace WebSitem.Models
{
    public class AppDbContext : IdentityDbContext <AppUser,AppRole,string>
    {
        private readonly IConfiguration _config;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config) : base(options)
        {
            _config = config;
        }

        public DbSet<Folder> Folders { get; set; }
      
        
        public DbSet<Category> Category { get; set; }

        public DbSet<File> Files { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           

            // Mevcut admin rol ve kullanıcı seed verileri
            var adminRole = Guid.NewGuid().ToString();
            var adminUser = Guid.NewGuid().ToString();

            modelBuilder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = adminRole,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                });

            modelBuilder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "User",
                    NormalizedName = "USER"
                });

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = adminUser,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@admin.com",
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    StorageLimit = 100,
                    EmailConfirmed = true,
                    PasswordHash = new PasswordHasher<AppUser>().HashPassword(null, "Adminnn")
                });

            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUser,
                    RoleId = adminRole
                });
           

            base.OnModelCreating(modelBuilder);


        }
    }
}
