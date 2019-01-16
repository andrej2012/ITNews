using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookSiteWeb.Models
{
    public class Chapter
    {
        public int ChapterId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }        
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Text { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PostedDate { get; set; }
        public int? BookId { get; set; }
        public Book Book { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public Chapter()
        {
            Ratings = new List<Rating>();            
        }

    }
}
