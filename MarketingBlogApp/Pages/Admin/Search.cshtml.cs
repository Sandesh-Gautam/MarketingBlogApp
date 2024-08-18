using MarketingBlogApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarketingBlogApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public SearchModel(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public IEnumerable<dynamic> BlogPosts { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        public string AIAnalysis { get; set; }

        public async Task OnGetAsync()
        {
            BlogPosts = await (
                from bp in _context.BlogPosts
                join author in _context.Users on bp.AuthorId equals author.Id
                where EF.Functions.Like(bp.Title.ToLower(), $"%{SearchTerm.ToLower()}%")
                      || EF.Functions.Like(bp.Content.ToLower(), $"%{SearchTerm.ToLower()}%")
                select new
                {
                    Id = bp.Id,
                    Title = bp.Title,
                    Content = bp.Content,
                    PublishedDate = bp.PublishedDate,
                    ImageUrl = bp.ImageUrl,
                    ViewCount = bp.ViewCount,
                    AuthorName = author.FirstName + " " + author.LastName,
                    AuthorEmail = author.Email,
                    Categories = string.Join(", ", _context.BlogPostCategories
                        .Where(bc => bc.BlogPostId == bp.Id)
                        .Select(bc => bc.Category.Name)
                        .ToList()),
                    LikeCount = _context.Likes.Count(l => l.BlogPostId == bp.Id),
                    Comments = _context.Comments
                        .Where(c => c.BlogPostId == bp.Id)
                        .Select(c => new
                        {
                            Content = c.Content,
                            CommentedDate = c.CommentedDate,
                            UserName = c.User.UserName
                        }).ToList()
                }).ToListAsync();

            // Generate a summary of the retrieved blog posts
            string summary = GenerateSummary(BlogPosts);

            // Optionally, you can pass the summary to AI for further analysis
            AIAnalysis = await GetRelevantDataAsync(SearchTerm, summary);
        }

        private string GenerateSummary(IEnumerable<dynamic> blogPosts)
        {
            if (blogPosts == null || !blogPosts.Any())
            {
                return "No relevant blog posts found.";
            }

            var summaries = blogPosts.Select(bp => $@"
                Title: {bp.Title}
                Author: {bp.AuthorName} ({bp.AuthorEmail})
                Published on: {bp.PublishedDate.ToString("MMMM dd, yyyy")}
                Likes: {bp.LikeCount}, Views: {bp.ViewCount}, Comments: {bp.Comments.Count}
                Categories: {bp.Categories}
                Content Summary: {bp.Content.Substring(0, Math.Min(200, bp.Content.Length))}...
            ");

            return string.Join("\n\n", summaries);
        }

        private async Task<string> GetRelevantDataAsync(string searchTerm, string summary)
        {
            string apiKey = _configuration["OpenAI:ApiKey"];
            string languagePrompt = $@"
    You are an AI tasked with analyzing blog post content. Below is a collection of blog post summaries related to the search term '{searchTerm}'.
    
    Please provide a detailed analysis, up to 300 words, including the following:
    - A brief overview of the key themes and topics covered in these posts.
    - Any noticeable trends or patterns in the content, such as common points of discussion or frequent themes.
    - An evaluation of the posts' engagement, such as the number of likes, comments and what this might suggest about their reception.
    - Highlight any particularly interesting or unique insights from the content.
    
    Blog Post Summaries:
    {summary}

    Ensure the analysis is comprehensive yet concise, emphasizing the most relevant information while avoiding unnecessary repetition.
";


            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = "You are processing search queries against a dataset." },
                    new { role = "user", content = languagePrompt }
                },
                temperature = 0.7
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    var chatGptResponse = jsonDocument.RootElement.GetProperty("choices")[0]
                        .GetProperty("message").GetProperty("content").GetString();

                    return chatGptResponse;
                }
                else
                {
                    Console.WriteLine($"API request failed with status code {response.StatusCode}: {response.ReasonPhrase}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing API request: " + ex.Message);
                return null;
            }
        }
    }
}
