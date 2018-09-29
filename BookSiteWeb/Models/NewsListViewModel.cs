using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookSiteWeb.Models
{
    public class NewsListViewModel
    {
        public IEnumerable<News> Newses { get; set; }
        public IEnumerable<Tag> Tags { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public string CurrentGenre { get; set; }
    }
}