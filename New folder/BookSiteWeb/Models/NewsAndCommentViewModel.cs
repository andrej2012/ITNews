using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookSiteWeb.Models
{
    public class NewsAndCommentViewModel
    {
        public News Newses { get; set; }
        public Rating Ratings { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
        public ICollection<Tag> Tags { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }        
    }
}