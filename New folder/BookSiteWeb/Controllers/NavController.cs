using BookSiteWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookSiteWeb.Controllers
{
    public class NavController : Controller
    {
        private ApplicationDbContext repository = new ApplicationDbContext();
                
        public PartialViewResult Menu(string genre = null)
        {
            ViewBag.SelectedGenre = genre;

            IEnumerable<string> genres = repository.Newses
                .Select(book => book.Genre.Value.ToString())
                .Distinct()
                .OrderBy(x => x);
            
            return PartialView("FlexMenu",genres);
        }
    }
}