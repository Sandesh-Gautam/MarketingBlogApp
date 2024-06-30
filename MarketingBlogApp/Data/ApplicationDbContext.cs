using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarketingBlogApp.Models;

namespace MarketingBlogApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<TopContributor> TopContributors { get; set; }
        public DbSet<PopularBlog> PopularBlogs { get; set; }
        public DbSet<TrendingBlog> TrendingBlogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BlogPostCategory> BlogPostCategories { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure BlogPost and Author relationship
            modelBuilder.Entity<BlogPost>()
                .HasKey(bp => bp.Id);

            modelBuilder.Entity<BlogPost>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(bp => bp.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Like and BlogPost relationship
            modelBuilder.Entity<Like>()
                .HasKey(l => l.Id);

            modelBuilder.Entity<Like>()
                .HasOne<BlogPost>()
                .WithMany()
                .HasForeignKey(l => l.BlogPostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Comment and BlogPost relationship
            modelBuilder.Entity<Comment>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Comment>()
                .HasOne<BlogPost>()
                .WithMany()
                .HasForeignKey(c => c.BlogPostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure TopContributor and ApplicationUser relationship
            modelBuilder.Entity<TopContributor>()
                .HasKey(tc => tc.Id);

            modelBuilder.Entity<TopContributor>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(tc => tc.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure PopularBlog and BlogPost relationship
            modelBuilder.Entity<PopularBlog>()
                .HasKey(pb => pb.Id);

            modelBuilder.Entity<PopularBlog>()
                .HasOne<BlogPost>()
                .WithMany()
                .HasForeignKey(pb => pb.BlogPostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Configure TrendingBlog and BlogPost relationship
            modelBuilder.Entity<TrendingBlog>()
                .HasKey(tb => tb.Id);

            modelBuilder.Entity<TrendingBlog>()
                .HasOne<BlogPost>()
                .WithMany()
                .HasForeignKey(tb => tb.BlogPostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Category model
            modelBuilder.Entity<Category>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Category>()
                .Property(c => c.Name)
                .IsRequired();

            // Configure BlogPostCategory as a join table for BlogPost and Category
            modelBuilder.Entity<BlogPostCategory>()
                .HasKey(bc => new { bc.BlogPostId, bc.CategoryId });

            modelBuilder.Entity<BlogPostCategory>()
                .HasOne<BlogPost>()
                .WithMany()
                .HasForeignKey(bc => bc.BlogPostId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BlogPostCategory>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(bc => bc.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserActivity
            modelBuilder.Entity<UserActivity>()
                .HasKey(ua => ua.Id);

            modelBuilder.Entity<UserActivity>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
