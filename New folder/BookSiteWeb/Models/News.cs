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
        Java,
        C,
        Algorithms,
        MachineLearning,
        Web,
        Python,
        IOS,
        Investing,
        Education,
        Microsoft,
        Funny,
        Ruby,
        PHP,
        JavaScript
    }

    public class News
    {
        public int NewsId { get; set; }
        [StringLength(100)]
        public string Name { get; set; }        
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [StringLength(5000)]
        [DataType(DataType.MultilineText)]
        public string TextNews { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        [StringLength(2083)]
        [DisplayName("Thumbnail")]
        public string ThumbnailURL { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PostedDate { get; set; }
        [StringLength(100)]
        public string TagName { get; set; }
        public Genre? Genre { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public double Rate { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }       
        public ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public News()
        {            
            Comments = new List<Comment>();
            Tags = new List<Tag>();
        }        
    }
}
