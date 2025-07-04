using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryLocationQuerySystem.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    //ApplicationDbContext 代表的是你的创建失败的那个类型
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        var builder = new DbContextOptionsBuilder();
        builder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        return new ApplicationDbContext(builder.Options);
    }
}