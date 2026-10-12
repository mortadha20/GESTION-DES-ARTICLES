using Microsoft.EntityFrameworkCore;

namespace GESTION_DES_ARTICLES.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Alimentaire" },
                new Category { CategoryId = 2, CategoryName = "Hygiène" },
                new Category { CategoryId = 3, CategoryName = "Nettoyage" }
            );
        }
    }
}