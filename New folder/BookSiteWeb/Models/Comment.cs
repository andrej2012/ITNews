using BookSiteCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSiteWeb.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PostedDate { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        public string UserName { get; set; }
        public int CountLike { get; set; }
        public int? NewsId { get; set; }
        public News News { get; set; }
        public ICollection<Like> Likes { get; set; }
        public Comment()
        {            
            Likes = new List<Like>();
        }
    }
}
