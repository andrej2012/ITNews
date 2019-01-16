using BookSiteCommon;
using System;
using System.Collections.Generic;
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
        public int? Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public int? BookId { get; set; }
        public Book Book { get; set; }
        public ICollection<Like> Likes { get; set; }
        public Comment()
        {            
            Likes = new List<Like>();
        }
    }
}
