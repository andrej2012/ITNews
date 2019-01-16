using BookSiteWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookSiteCommon
{
    public class Like
    {
        public int LikeId { get; set; }
        public string UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int? CommentId { get; set; }
        public Comment Comment { get; set; }
    }
}
