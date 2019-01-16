using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace BookSiteWeb.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
        public ApplicationUser MyUser { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class EditViewModelNews
    {
        public int NewsId { get; set; }
        [Display(Name = "Имя пользователя")]
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
        public Genre? Genre { get; set; }
        public void MapToModel(News p)
        {
            p.Name = Name;           
            p.Description = Description;
            p.TextNews = TextNews;
            p.ImageURL = ImageURL;
            p.Genre = Genre;
            p.ThumbnailURL = ThumbnailURL;
        }
    }

    public class EditViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Имя пользователя")]
        public string UserName { get; set; }
        public int Age { get; set; }
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [StringLength(2083)]
        [DisplayName("Full-size Image")]
        public string ImageURL { get; set; }
        [StringLength(2083)]
        [DisplayName("Thumbnail")]
        public string ThumbnailURL { get; set; }
        public void MapToModel(ApplicationUser p)
        {
            p.UserName = UserName;
            p.Age = Age;
            p.Description = Description;
            p.ImageURL = ImageURL;
            p.ThumbnailURL = ThumbnailURL;
        }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Текущий пароль")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Значение {0} должно содержать символов не менее: {2}.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новый пароль")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение нового пароля")]
        [Compare("NewPassword", ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Номер телефона")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Код")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Номер телефона")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }
}