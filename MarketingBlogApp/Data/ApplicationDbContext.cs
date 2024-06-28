using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Models;

namespace MarketingBlogApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BlogPostCategory> BlogPostCategories { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PopularBlog> PopularBlogs { get; set; }
        public DbSet<TrendingBlog> TrendingBlogs { get; set; }
        public DbSet<TopContributor> TopContributors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Configure ApplicationUser entity
                entity.Property(u => u.FirstName).IsRequired();
                entity.Property(u => u.LastName).IsRequired();
                entity.Property(u => u.Address).IsRequired(false);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("getutcdate()");
                entity.Property(u => u.IsDisabled).HasDefaultValue(false);
                entity.Property(u => u.ProfileImage).IsRequired(false);
            });

            modelBuilder.Entity<UserActivity>(entity =>
            {
                // Configure UserActivity entity
                entity.HasKey(ua => ua.Id);
                entity.Property(ua => ua.Activity).IsRequired();
                entity.Property(ua => ua.Timestamp).HasDefaultValueSql("getutcdate()");

                entity.HasOne(ua => ua.User)
                    .WithMany()
                    .HasForeignKey(ua => ua.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired();

                entity.HasMany(c => c.BlogPostCategories)
                      .WithOne(bpc => bpc.Category)
                      .HasForeignKey(bpc => bpc.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BlogPostCategory>(entity =>
            {
                entity.HasKey(bpc => new { bpc.BlogPostId, bpc.CategoryId });

                entity.HasOne(bpc => bpc.BlogPost)
                      .WithMany(bp => bp.BlogPostCategories)
                      .HasForeignKey(bpc => bpc.BlogPostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(bpc => bpc.Category)
                      .WithMany(c => c.BlogPostCategories)
                      .HasForeignKey(bpc => bpc.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(bp => bp.Id);
                entity.Property(bp => bp.Title).IsRequired();
                entity.Property(bp => bp.Content).IsRequired();
                entity.Property(bp => bp.CreatedAt).HasDefaultValueSql("getutcdate()");
                entity.Property(bp => bp.Picture).IsRequired(false);

                entity.HasOne(bp => bp.Category)
                      .WithMany() 
                      .HasForeignKey(bp => bp.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(bp => bp.BlogPostCategories)
                      .WithOne(bpc => bpc.BlogPost)
                      .HasForeignKey(bpc => bpc.BlogPostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(bp => bp.Likes)
                      .WithOne(l => l.BlogPost)
                      .HasForeignKey(l => l.BlogPostId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(bp => bp.Comments)
                      .WithOne(c => c.BlogPost)
                      .HasForeignKey(c => c.BlogPostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.Property(l => l.CreatedAt).HasDefaultValueSql("getutcdate()");

                entity.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade); 

                entity.HasOne(l => l.BlogPost)
                    .WithMany(bp => bp.Likes)
                    .HasForeignKey(l => l.BlogPostId)
                    .OnDelete(DeleteBehavior.Restrict); 
            });



            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("getutcdate()");

                entity.HasOne(c => c.Author)
                    .WithMany()
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.BlogPost)
                    .WithMany(bp => bp.Comments)
                    .HasForeignKey(c => c.BlogPostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<PopularBlog>(entity =>
            {
                entity.HasKey(pb => pb.Id);
                entity.Property(pb => pb.RecordedAt).HasDefaultValueSql("getutcdate()");

                entity.HasOne(pb => pb.BlogPost)
                    .WithMany()
                    .HasForeignKey(pb => pb.PopularBlogPostId)  // Ensure BlogPostId matches the type of BlogPost's primary key
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TrendingBlog>(entity =>
            {
                entity.HasKey(tb => tb.Id);
                entity.Property(tb => tb.RecordedAt).HasDefaultValueSql("getutcdate()");

                entity.HasOne(tb => tb.BlogPost)
                    .WithMany()
                    .HasForeignKey(tb => tb.TrendingBlogPostId)  // Ensure BlogPostId matches the type of BlogPost's primary key
                    .OnDelete(DeleteBehavior.Cascade);
            });



            modelBuilder.Entity<TopContributor>(entity =>
            {
                // Configure TopContributor entity
                entity.HasKey(tc => tc.Id);
                entity.Property(tc => tc.RecordedAt).HasDefaultValueSql("getutcdate()");

                entity.HasOne(tc => tc.User)
                    .WithMany()
                    .HasForeignKey(tc => tc.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
