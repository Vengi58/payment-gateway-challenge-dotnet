using System.Reflection.Metadata;

using Microsoft.EntityFrameworkCore;
using PaymentGateway.Persistance.Entities;

namespace PaymentGateway.Persistance.Repository
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options)
            : base(options) { }
        //protected override void OnConfiguring
        //    (DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseInMemoryDatabase(databaseName: "PaymentsDbPaymentsDb");
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PaymentEntity>()
                .HasOne(m => m.Merchant)
                .WithMany(p => p.Payments)
                .IsRequired();

            modelBuilder.Entity<MerchantEntity>()
                .HasMany(p => p.Payments)
                .WithOne(m => m.Merchant)
                .IsRequired();

            modelBuilder.Entity<MerchantEntity>().Property(e => e.MerchantId).ValueGeneratedNever();
            modelBuilder.Entity<PaymentEntity>().Property(e => e.PaymentId).ValueGeneratedNever();
        }
        public DbSet<MerchantEntity> Merchants { get; set; }

        public DbSet<PaymentEntity> Payments { get; set; }
    }
}
