using LibraryLocationQuerySystem.Data;
using LibraryLocationQuerySystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LibraryLocationQuerySystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDefaultIdentity<StudentUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            }).AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
            builder.Services.AddRazorPages();
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    if (context is not ApplicationDbContext applicationDbContext) return;
                    var any = await applicationDbContext.Roles.AnyAsync(cancellationToken);
                    if (!any)
                    {
                        await applicationDbContext.Roles.AddRangeAsync(
                            new IdentityRole("admin") { NormalizedName = "ADMIN" },
                            new IdentityRole("reader") { NormalizedName = "READER" }
                        );
                    }
                    await applicationDbContext.SaveChangesAsync(cancellationToken);
                });
            });
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await using var storeManagerDbContext = services.GetRequiredService<ApplicationDbContext>();
                await storeManagerDbContext.Database.MigrateAsync();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                //app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}