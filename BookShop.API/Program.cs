using BookShop.Context;
using BookShop.Core.Services;
using BookShop.Core.Models;
using BookShop.Core.Repositories;
using BookShop.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

public partial class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        AppDomain.CurrentDomain.SetData("DataDirectory", AppContext.BaseDirectory);
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new()
            {
                Title = "BookShop API",
                Version = "v1",
                Description = "REST API for browsing and ordering books."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Zadaj JWT token v tvare: Bearer [tvoj_token]",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        builder.Services.AddDbContext<AppDbContext>(options =>
           options.UseSqlite(connectionString));

        // Register repositories
        builder.Services.AddScoped<IBookRepository, BookRepository>();
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register services
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();

        builder.Services.AddIdentityApiEndpoints<IdentityUser>()
          .AddRoles<IdentityRole>()
          .AddEntityFrameworkStores<AppDbContext>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await context.Database.MigrateAsync();

            // Seed books
            if (!context.Books.Any())
            {
                

                context.Books.AddRange(
                    new Book { Author = new Author { Id = 1, Name = "Frank Herbert" }, Title = "Dune", PublishedYear = 1965, Price = 12.99m, ISBN = "9780441172719", IsAvailable = true },
                    new Book { Author = new Author { Id = 2, Name = "Ursula K. Le Guin" }, Title = "A Wizard of Earthsea", PublishedYear = 1968, Price = 11.99m, ISBN = "9780547773742", IsAvailable = true }
                );

                await context.SaveChangesAsync();
            }
        }
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "BookShop API v1");
                options.RoutePrefix = "swagger";
            });
        }


        // Add the customer role and profile after Identity registers a user.
        app.MapGroup("/api").WithTags("Identity").MapIdentityApi<IdentityUser>().AddEndpointFilter(async (context, next) =>
        {
            var request = context.Arguments.OfType<RegisterRequest>().FirstOrDefault();

            var result = await next(context);

            if (request is not null && context.HttpContext.Response.StatusCode == StatusCodes.Status200OK)
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var customerService = context.HttpContext.RequestServices.GetRequiredService<ICustomerService>();

                var user = await userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    // Add Customer role
                    await userManager.AddToRoleAsync(user, "Customer");

                    // Extract first and last name from email or use defaults
                    var emailParts = request.Email.Split('@')[0].Split('.', StringSplitOptions.RemoveEmptyEntries);
                    var firstName = emailParts.Length > 0 ? emailParts[0] : "Customer";
                    var lastName = emailParts.Length > 1 ? emailParts[1] : user.Email?.Split('@')[0] ?? "User";

                    // Create associated customer record
                    try
                    {
                        await customerService.CreateCustomerAsync(
                            user.Id,
                            user.Email ?? request.Email,
                            ""
                        );
                    }
                    catch (InvalidOperationException)
                    {
                        // Customer already exists, ignore
                    }
                }
            }

            return result;
        });
        app.UseHttpsRedirection();
        app.MapControllers();


        app.Run();
    }
}
