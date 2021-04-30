using Microsoft.EntityFrameworkCore;
using SuperTouristAPI.Models;

namespace SuperTouristAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<TravelAgency> TravelAgencies { get; set; }

        public DbSet<Tourist> Tourists { get; set; }

        public DbSet<Contract> Contracts { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<ServiceType> ServiceTypes { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<ChatInteraction> ChatInteractions { get; set; }

        public DbSet<ServiceImage> ServiceImages { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<UserImage> UserImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("User");
            builder.Entity<TravelAgency>().ToTable("TravelAgency");
            builder.Entity<Tourist>().ToTable("Tourist");
            builder.Entity<Contract>().ToTable("Contract");
            builder.Entity<Service>().ToTable("Service");
            builder.Entity<ServiceType>().ToTable("ServiceType");
            builder.Entity<Message>().ToTable("Message");
            builder.Entity<ChatInteraction>().ToTable("ChatInteraction");
            builder.Entity<Review>().ToTable("Review");
            builder.Entity<ServiceImage>().ToTable("ServiceImage");
            builder.Entity<UserImage>().ToTable("UserImage");
            builder.Entity<Service>().Property(service => service.Price).HasColumnType("decimal(19,2)").IsRequired(true);
            builder.Entity<Contract>().Property(contract => contract.Price).HasColumnType("decimal(19,2)").IsRequired(true);

            builder.Entity<User>()
                .HasMany(t => t.ChatInteractions)
                .WithOne(t => t.FirstUser)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<User>()
                .HasOne(a => a.ProfileImage)
                .WithOne(a => a.User)
                .HasForeignKey<UserImage>(c => c.UserId);

            builder.Entity<Service>() //Cannot delete, data will be lost for archive
                .HasOne(s => s.TravelAgency)
                .WithMany(a => a.Services)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Service>().Property(m => m.HourTo).IsRequired(false);
        }
    }
}
