using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanSuite.Data;
using PlanSuite.Models.Persistent;
using PlanSuite.Models.Temporary;

namespace PlanSuite.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext m_Database;
        private readonly ILogger<BlogController> m_Logger;
        private readonly UserManager<ApplicationUser> m_UserManager;
        private readonly SignInManager<ApplicationUser> m_SignInManager;

        public BlogController(ApplicationDbContext database, ILogger<BlogController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            m_Database = database;
            m_Logger = logger;
            m_UserManager = userManager;
            m_SignInManager = signInManager;
        }

        [Route("blog")]
        public IActionResult Index()
        {
            m_Logger.LogInformation($"Grabbing last 10 blog posts");
            // TODO: Show last 10 blog posts
            return View();
        }

        [Route("blog/p={p}")]
        public async Task<IActionResult> PostShortLink(int p)
        {
            var post = await m_Database.BlogPosts.Where(post => post.Id == p).FirstOrDefaultAsync();
            if (post == null)
            {
                return NotFound();
            }

            return RedirectToAction("Post", "Blog", new { slug = post.Slug });
        }

        [Route("blog/{slug}")]
        public async Task<IActionResult> Post(string slug)
        {
            var post = await m_Database.BlogPosts.Where(post => post.Slug == slug).FirstOrDefaultAsync();
            if (post == null)
            {
                return NotFound();
            }

            string authorName = "Unknown";
            var author = await m_UserManager.FindByIdAsync(post.AuthorId.ToString());
            if(author != null)
            {
                authorName = $"<a class=\"ps-link-primary\" href=\"/blog/author/{author.FirstName.ToLower()}-{author.LastName}\" title=\"Posts by {author.FullName.ToLower()}\" rel=\"author\">{author.FullName}</a>";
            }


            BlogPostViewModel viewModel = new BlogPostViewModel();
            viewModel.Title = post.Title;
            viewModel.Content = post.Content;
            viewModel.Id = post.Id;
            viewModel.Author = authorName;
            viewModel.DatePublished = post.DatePosted;
            viewModel.Summary = post.Summary;
            viewModel.Keywords = post.Keywords;
            var user = await m_UserManager.GetUserAsync(User);
            if(user != null)
            {
                var subscribed = await m_Database.BlogSubscriptions.Where(sub => sub.Email == user.Email).FirstOrDefaultAsync() != null;
                viewModel.IsSubscribed = subscribed;
            }
            if(!string.IsNullOrEmpty(post.Image))
            {
                viewModel.ImageSrc = post.Image;
            }

            // Show the post that uses that slug. Slugs should be unique
            return View(viewModel);
        }
    }
}
