using LibraryLocationQuerySystem.Areas.Identity.Data;
using LibraryLocationQuerySystem.Areas.Identity.Models;
using LibraryLocationQuerySystem.Data;
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
            .AddEntityFrameworkStores<StudentUserDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<StoreManagerDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<StudentUserDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
                options.UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    if (context is not StudentUserDbContext studentUserDbContext) return;
                    var any = await studentUserDbContext.Roles.AnyAsync(cancellationToken);
                    if (!any)
                    {
                        await studentUserDbContext.Roles.AddRangeAsync(
                            new IdentityRole("admin") { NormalizedName = "ADMIN" },
                            new IdentityRole("reader") { NormalizedName = "READER" }
                        );
                    }
                    await studentUserDbContext.SaveChangesAsync(cancellationToken);
                });
            });
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                await using var storeManagerDbContext = services.GetRequiredService<StoreManagerDbContext>();
                await storeManagerDbContext.Database.MigrateAsync();
                await using var studentUserDbContext = services.GetRequiredService<StudentUserDbContext>();
                await studentUserDbContext.Database.MigrateAsync();
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