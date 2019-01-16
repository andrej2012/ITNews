using BookSiteCommon;
using BookSiteWeb.Hubs;
using BookSiteWeb.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BookSiteWeb.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext repository = new ApplicationDbContext();
        public int pageSize = 5;
        public ActionResult Index(string genre, int page = 1)
        {
            NewsListViewModel model = new NewsListViewModel
            {
                Newses = repository.Newses
                .Where(b => genre == null || b.Genre.Value.ToString() == genre)
                .OrderByDescending(book => book.Rate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = genre == null ?
                        repository.Newses.Count() :
                        repository.Newses.Where(book => book.Genre.Value.ToString() == genre).Count()
                },
                CurrentGenre = genre
            };            

            return View(model);
        }

        public async Task<ActionResult> IndexTags(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tags = await repository.Tags.FindAsync(id);
            if (tags == null)
            {
                return HttpNotFound();
            }            
            ICollection<News> mynews = tags.News;  
            return View(mynews);
        }
        public ActionResult _TagSummary()
        {
            return View();
        }

        // GET: Ad/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NewsAndCommentViewModel detailsNews = new NewsAndCommentViewModel();            
            News myNews = await repository.Newses.FindAsync(id);
            if (myNews == null)
            {
                return HttpNotFound();
            }
            detailsNews.Newses = myNews;            
            detailsNews.Comments = repository.Comments.Where(b => b.NewsId == id);
            detailsNews.Tags = myNews.Tags;
            foreach (Comment n in detailsNews.Comments)
            {
                n.CountLike = repository.Likes.Where(b => b.CommentId == n.CommentId).Count();
            }
            string userId = User.Identity.GetUserId();
            if (userId != null)
            {
                ApplicationUser myUser = repository.Users.Find(userId);
                detailsNews.UserId = userId;
                detailsNews.ImageURL = myUser.ImageURL;
                detailsNews.UserName = myUser.UserName;
            }            
            if (detailsNews == null)
            {
                return HttpNotFound();
            }            
            return View(detailsNews);
        }

        [Authorize]
        [HttpPost]
        public ActionResult _CreateComment(Comment comment)
        {
            if(comment.Text != null)
            {
                comment.PostedDate = DateTime.Now;
                repository.Comments.Add(comment);
                repository.SaveChangesAsync();
            }            
            SendMessage();
            return RedirectToAction("Details/"+ comment.NewsId);
        }

        [Authorize]
        [HttpPost]
        public ActionResult _DeliteComment(Comment comment)
        {
            if (comment == null)
            {
                return HttpNotFound();
            }

            if(comment.UserId == User.Identity.GetUserId())
            {
                var likes = repository.Likes.Where(b => b.CommentId == comment.CommentId);
                foreach (Like x in likes)
                {
                    repository.Likes.Remove(x);
                }
                Comment newComment = repository.Comments.Find(comment.CommentId);
                repository.Comments.Remove(newComment);
                repository.SaveChangesAsync();
            }            
            SendMessage();
            return RedirectToAction("Details/" + comment.NewsId);
        }

        [Authorize]
        [HttpPost]
        public ActionResult _TakeLike(Comment ad)
        {
            string userId = User.Identity.GetUserId();
            IEnumerable<Like> like = repository.Likes.Where(b => b.CommentId == ad.CommentId).Where(b => b.UserId == userId);            
            if (like.Count() == 0)
            {
                Like newlike = new Like();
                newlike.UserId = User.Identity.GetUserId();
                newlike.CommentId = ad.CommentId;
                repository.Likes.Add(newlike);                
            }
            else
            {
                foreach(var n in like)
                {
                    repository.Likes.Remove(n);
                }
            }
            repository.SaveChangesAsync();
            SendMessage();
            return RedirectToAction("Details/" + ad.NewsId);
        }

        [Authorize]
        [HttpPost]
        public ActionResult _TakeRate(FormCollection form)
        {
            var newsId = int.Parse(form["NewsId"]);
            var rating = int.Parse(form["Rating"]);

            string userId = User.Identity.GetUserId();
            IEnumerable<Rating> ratings = repository.Ratings.Where(b => b.NewsId == newsId).Where(b => b.UserId == userId);
            if (ratings.Count() != 0)
            {
                foreach (var n in ratings)
                {
                    repository.Ratings.Remove(n);
                }
            }

            Rating artRate = new Rating()
            {
                NewsId = newsId,
                Rate = rating,
                UserId = User.Identity.GetUserId()
            };
            repository.Ratings.Add(artRate);
            repository.SaveChanges();

            IEnumerable<Rating> rates = repository.Ratings.Where(b => b.NewsId == newsId);
            double maxRate = 0;
            foreach(var t in rates)
            {
                maxRate += t.Rate;
            }
            News news = repository.Newses.Find(newsId);
            news.Rate = maxRate / rates.Count();

            repository.SaveChanges();

            return RedirectToAction("Details/" + artRate.NewsId);
        }

        // GET: Ad/Details/5
        public new ActionResult Profile(string userId)
        {            
            if (userId == null)
            {
                userId = User.Identity.GetUserId();                
            }
            ApplicationUser ad = repository.Users.Find(userId);
            if (ad == null)
            {
                return HttpNotFound();
            }
            IEnumerable<News> news = repository.Newses.Where(b => b.UserId == userId);
            foreach(var t in news)
            {
                ad.Newses.Add(t);
            }
            return View(ad);
        }

        private void SendMessage()
        {
            // Получаем контекст хаба
            var context =
                Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            // отправляем сообщение
            context.Clients.All.displayMessage();
        }
    }
}