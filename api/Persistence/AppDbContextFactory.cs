using api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=localhost,1433;Database=Wheelzy;User Id=sa;Password=@SQLServer2025;Encrypt=True;TrustServerCertificate=True;")
                .Options;

            return new AppDbContext(options);
        }
    }
}
