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

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<BlogPostCategory> BlogPostCategories { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<BlackList> Blacklists { get; set; }
        public DbSet<DeletionReason> DeletionReasons { get; set; }
        public DbSet<ManagerAction> ManagerActions { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-Many Relationship between BlogPost and Category
            builder.Entity<BlogPostCategory>()
                .HasKey(bc => new { bc.BlogPostId, bc.CategoryId });
            builder.Entity<BlogPostCategory>()
                .HasOne(bc => bc.BlogPost)
                .WithMany(b => b.BlogPostCategories)
                .HasForeignKey(bc => bc.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<BlogPostCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BlogPostCategories)
                .HasForeignKey(bc => bc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many Relationship between BlogPost and Comment
            builder.Entity<Comment>()
                .HasOne(c => c.BlogPost)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many Relationship between BlogPost and Like
            builder.Entity<Like>()
                .HasOne(l => l.BlogPost)
                .WithMany(b => b.Likes)
                .HasForeignKey(l => l.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-Many Relationship between ApplicationUser and Comment
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Prevent cascade delete

            // One-to-Many Relationship between ApplicationUser and Like
            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);  // Prevent cascade delete

            // One-to-Many Relationship between ApplicationUser and BlogPost
            builder.Entity<BlogPost>()
                .HasOne(b => b.Author)
                .WithMany(u => u.BlogPosts)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

            // One-to-Many Relationship between ApplicationUser and UserActivity
            builder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserActivities)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascade delete

            // Configure FAQ entity
            builder.Entity<FAQ>()
                .ToTable("FAQs") // Optional: specify table name if different from default
                .HasKey(f => f.Id); // Ensure the primary key is set if not using convention

            // Seed data for FAQ
            builder.Entity<FAQ>().HasData(
                new FAQ { Id = 1, Question = "What is the purpose of this site?", Answer = "This site allows users to browse and interact with blog posts." },
                new FAQ { Id = 2, Question = "How can I create a new blog post?", Answer = "You can create a new blog post by navigating to the 'My Posts' section if you are logged in." },
                new FAQ { Id = 3, Question = "How can I contact support?", Answer = "You can contact support using the 'Contact' page." },
                new FAQ { Id = 4, Question = "How can I search for posts?", Answer = "Use the search bar at the top of the blog post list to search for posts." },
                new FAQ { Id = 5, Question = "What categories are available?", Answer = "The categories are listed in the sidebar. Click on a category to filter posts by that category." }
            );
        }
    }
}
