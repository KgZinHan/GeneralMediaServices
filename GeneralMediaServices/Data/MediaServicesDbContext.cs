using Microsoft.EntityFrameworkCore;
using GeneralMediaServices.Models;

namespace GeneralMediaServices.Data
{
    public class MediaServiceDbContext : DbContext
    {
        public MediaServiceDbContext(DbContextOptions<MediaServiceDbContext> options)
            : base(options)
        {
        }
        public DbSet<MediaService> media_data_tb { get; set; }

        public DbSet<User> user_tb {  get; set; } 

        public DbSet<DatabaseSettings> DatabaseSettings { get; set; } = default!;

    }
    
}