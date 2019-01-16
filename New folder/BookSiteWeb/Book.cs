using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BookSiteWeb.Models
{
    public enum Genre
    {
        Cars,        
        RealEstate,        
        FreeStuff
    }

    public class Book
    {
        public int BookId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        [StringLength(2083)]
        [DisplayName("Thumbnail")]
        public string ThumbnailURL { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PostedDate { get; set; }
        public Genre? Genre { get; set; }
        public int? Id { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public Book()
        {
            Chapters = new List<Chapter>();
            Comments = new List<Comment>();
        }        
    }
}
