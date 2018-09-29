using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using BookSiteCommon;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BookSiteWeb.Models
{  
    public class ApplicationUser : IdentityUser
    {
        /*public int Age { get; set; }
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        [StringLength(2083)]
        [DisplayName("Thumbnail")]
        public string ThumbnailURL { get; set; }*/
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Book> Books { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ApplicationUser()
        {
            Books = new List<Book>();
            Comments = new List<Comment>();
            Likes = new List<Like>();
            Ratings = new List<Rating>();
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
        public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
        {
            public DbSet<Book> Books { get; set; }
            public DbSet<Chapter> Chapters { get; set; }
            public DbSet<Like> Likes { get; set; }
            public DbSet<Comment> Comments { get; set; }
            public DbSet<Rating> Ratings { get; set; }
            public ApplicationDbContext()
                : base("BookSiteContext", throwIfV1Schema: false)
            {
            }
            public ApplicationDbContext(string connString) : base(connString)
            {
            }
            

            public static ApplicationDbContext Create()
            {
                return new ApplicationDbContext();
            }
        }
    }    
}