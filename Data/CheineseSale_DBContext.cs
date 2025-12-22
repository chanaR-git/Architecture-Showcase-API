using Chinese_sale_api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinese_sale_api.Data
{
    public class CheineseSale_DBContext : DbContext
    {
        public CheineseSale_DBContext(DbContextOptions<CheineseSale_DBContext> options) : base(options) { }
        public DbSet<Customer> Users { get; set; }
        public DbSet<Gift> Gifts { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Donor> Donors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //customer
            modelBuilder.Entity<Customer>().HasKey(u => u.Id);
            modelBuilder.Entity<Customer>().Property(u => u.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Customer>().Property(u => u.Password).IsRequired().HasMaxLength(70);
            modelBuilder.Entity<Customer>().Property(u => u.Email).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Customer>().Property(u => u.Phone).IsRequired().HasMaxLength(15);

            modelBuilder.Entity<Customer>().HasIndex(u => u.Email).IsUnique();
            
            
            //manager
            modelBuilder.Entity<Manager>().HasKey(m => m.Id);
            modelBuilder.Entity<Manager>().Property(m => m.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Manager>().Property(m => m.Password).IsRequired().HasMaxLength(70);
            modelBuilder.Entity<Manager>().Property(m => m.Email).IsRequired().HasMaxLength(50);

            modelBuilder.Entity<Manager>().HasIndex(m => m.Email).IsUnique();

            
            //donor
            modelBuilder.Entity<Donor>().HasKey(d => d.Id);
            modelBuilder.Entity<Donor>().Property(d => d.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Donor>().Property(d => d.Email).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Donor>().Property(d => d.Phone).IsRequired().HasMaxLength(15);

            modelBuilder.Entity<Donor>().HasIndex(d => d.Email).IsUnique();

            //category
            modelBuilder.Entity<Category>().HasKey(c => c.Id);
            modelBuilder.Entity<Category>().Property(c => c.Name).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
            
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Gifts)
                .WithOne(g=>g.Category)
                .HasForeignKey(g=>g.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);



            //gift
            modelBuilder.Entity<Gift>().HasKey(g => g.Id);
            modelBuilder.Entity<Gift>().Property(g => g.Description).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Gift>().Property(g => g.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Gift>().HasIndex(g => g.Name).IsUnique();
            modelBuilder.Entity<Gift>().Property(g => g.ImagePath).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Gift>().Property(g => g.Price).IsRequired();
            modelBuilder.Entity<Gift>().Property(g => g.CategoryId).IsRequired();
            modelBuilder.Entity<Gift>().Property(g => g.DonorId).IsRequired();

            modelBuilder.Entity<Gift>()
                .HasOne(g => g.Donor)
                .WithMany(d => d.MyGifts)
                .HasForeignKey(g => g.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<Gift>()
                .HasOne(g => g.Winner)
                .WithMany(cus => cus.WonGifts)
                .HasForeignKey(g => g.WinnerId).
                OnDelete(DeleteBehavior.SetNull);


            
            //purchases
            modelBuilder.Entity<Purchase>().HasKey(c => c.Id);
            modelBuilder.Entity<Purchase>().Property(c => c.CustomerId).IsRequired();
            modelBuilder.Entity<Purchase>().Property(p => p.GiftId).IsRequired();
            modelBuilder.Entity<Purchase>().Property(p => p.PurchDate).IsRequired();
            
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Gift)
                .WithMany(g => g.Purchases)
                .HasForeignKey(p => p.GiftId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.Customer)
                .WithMany(cus => cus.Purchases)
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
