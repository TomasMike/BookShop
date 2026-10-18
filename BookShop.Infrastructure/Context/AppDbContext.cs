using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookShop.Core.Models;

namespace BookShop.Context
{
    //dotnet ef database update --project BookShop.Infrastructure --startup-project BookShop.API
    //dotnet ef migrations add AddBookCatalog --project BookShop.Infrastructure --startup-project BookShop.API
    public class AppDbContext : IdentityDbContext<
        IdentityUser,                        // TUser
        IdentityRole,                        // TRole
        string,                              // TKey
        IdentityUserClaim<string>,           // TUserClaim
        IdentityUserRole<string>,            // TUserRole
        IdentityUserLogin<string>,           // TUserLogin
        IdentityRoleClaim<string>,           // TRoleClaim
        IdentityUserToken<string>>           // TUserToken
    {
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Customer> Customers => Set<Customer>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Customer-User relationship
            builder.Entity<Customer>()
                .HasKey(c => c.Id);

            builder.Entity<Customer>()
                .Property(c => c.UserId)
                .IsRequired();

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "customer-role-id",
                Name = "Customer",
                NormalizedName = "CUSTOMER",
                ConcurrencyStamp = "aa55be3f-92a5-441a-917f-d28108a10ff4"
            });

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "admin-role-id",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "78f6f0fc-bf65-4e5b-9267-477ce7df7adc"
            });
        }
    }
}
