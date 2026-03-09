using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pftc_auth.DataAccess;
using pftc_auth.Models; // Make sure to include your Models namespace
using System.Collections.Generic;

namespace pftc_auth.Controllers
{
    public class SocialController : Controller
    {
        
        private FirestoreRepository _firestoreRepository;
        private ILogger<SocialController> _logger;

        public SocialController(FirestoreRepository firestoreRepository, ILogger<SocialController> logger)
        {
            _firestoreRepository = firestoreRepository;
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var posts =_firestoreRepository.GetPosts().Result; // Consider using async/await for better performance

            return View(posts);
        }
        
        [Authorize]
        public IActionResult create()
        {
            return View();
        }
  
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> create(SocialMediaPost post)
        {
           
                post.PostID = Guid.NewGuid().ToString();
                post.PostDate = DateTimeOffset.UtcNow;
                post.PostAuthor = User.Identity.Name ?? "Anonymous";

                Console.WriteLine($"Creating post: {post.PostID}, Author: {post.PostAuthor}, Date: {post.PostDate}");
                await _firestoreRepository.CreatePost(post);
                Console.WriteLine("Post created successfully, redirecting to Index.");

                return RedirectToAction("Index","Social");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeletePost(string postID)
        {
            try
            {
                SocialMediaPost post = await _firestoreRepository.GetPostById(postID); // Check if post exists before attempting to delete
                if (User.Identity.Name == post.PostAuthor)
                {
                    await _firestoreRepository.DeletePost(postID);
                    Console.WriteLine($"Post {postID} deleted successfully.");
                }
                else
                {
                    return Forbid(); // User is not the author of the post, return 403 Forbidden
                }

                return RedirectToAction("Index", "Social");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user identity: {ex.Message}");
                return Forbid(); // If we can't determine the user's identity, forbid the action
            }
                

        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditPost(string postID)
        {
            SocialMediaPost post = await _firestoreRepository.GetPostById(postID); // Check if post exists before attempting to edit
            if (User.Identity.Name == post.PostAuthor)
            {

                return View(post); // Return the view for editing the post
            }
            else {
                return Forbid(); // User is not the author of the post, return 403 Forbidden
            }
        }
    }
}