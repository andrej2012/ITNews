using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BookSiteCommon;
using BookSiteWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using PagedList;

namespace BookSiteWeb.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CloudQueue imagesQueue;
        private static CloudBlobContainer imagesBlobContainer;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        #region ManageInitialize
        public ManageController()
        {
            InitializeStorage();
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            InitializeStorage();
        }        

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }        

        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.SetPasswordSuccess ? "Пароль задан."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Настроен поставщик двухфакторной проверки подлинности."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : message == ManageMessageId.AddPhoneSuccess ? "Ваш номер телефона добавлен."
                : message == ManageMessageId.RemovePhoneSuccess ? "Ваш номер телефона удален."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                MyUser = db.Users.Find(userId)
        };
            return View(model);
        }

        public async Task<ActionResult> IndexTags(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tags = await db.Tags.FindAsync(id);
            if (tags == null)
            {
                return HttpNotFound();
            }
            ICollection<News> mynews = tags.News;
            return View(mynews);
        }
        #endregion

        #region AdminUser
        // GET: /Manage/Index
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> IndexAdmin(string Id)
        {
            var userId = Id;
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId),
                MyUser = db.Users.Find(userId)
            };
            return View(model);
        }

        // GET: Ad/Edit/5
        [Authorize(Roles = "admin")]
        public ActionResult EditAdmin(string userId)
        {
            if (userId  == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser ad = db.Users.Find(userId);
            if (ad == null)
            {
                return HttpNotFound();
            }            
            ApplicationUser adnew = new ApplicationUser();
            adnew.UserName = ad.UserName;
            adnew.Age = ad.Age;
            adnew.Description = ad.Description;
            adnew.ImageURL = ad.ImageURL;
            adnew.ThumbnailURL = ad.ThumbnailURL;
            adnew.PhoneNumber = ad.Id;
            return View(adnew);
        }

        // POST: Ad/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> EditAdmin(
            [Bind(Include = "Id,UserName,Age,Description,ImageURL,ThumbnailURL,PhoneNumber")] ApplicationUser ad,
            HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength != 0)
                {                    
                    await DeleteAdBlobsAsync(ad);
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    ad.ImageURL = imageBlob.Uri.ToString();
                }
                if (ad == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ApplicationUser p = db.Users.Find(ad.PhoneNumber);
                if (p == null)
                {
                    return HttpNotFound();
                }                               
                var vm = new EditViewModel
                {
                    Id = p.Id, 
                    UserName = ad.UserName,
                    Age = ad.Age,
                    Description = ad.Description,
                    ImageURL = ad.ImageURL,
                    ThumbnailURL = ad.ThumbnailURL
                };
                vm.MapToModel(p);
                db.SaveChanges();                
                if (imageBlob != null)
                {
                    var queueMessage = new CloudQueueMessage(ad.Id.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);                    
                }
                return RedirectToAction("IndexAdmin/" + ad.PhoneNumber);
            }
            return RedirectToAction("IndexAdmin/"+ ad.PhoneNumber);
        }
        #endregion

        #region AdminNews
        [Authorize(Roles = "admin")]
        public ActionResult IndexNewsAdmin(string userId, string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (userId  == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            IEnumerable<News> dates = db.Newses;
            dates = dates.Where(a => a.UserId == userId);
            
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                dates = dates.Where(s => s.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    dates = dates.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    dates = dates.OrderBy(s => s.PostedDate);
                    break;
                case "date_desc":
                    dates = dates.OrderByDescending(s => s.PostedDate);
                    break;
                default:  // Name ascending 
                    dates = dates.OrderBy(s => s.Name);
                    break;
            }
            return View(dates);
        }

        // GET: Ad/Create
        [Authorize(Roles = "admin")]
        public ActionResult CreateNewsAdmin(string userId)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News news = new News();
            news.UserId = userId;
            return View(news);
        }
        
        // POST: Ad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> CreateNewsAdmin(
            [Bind(Include = "NewsId,Name,Genre,Description,TextNews,ImageURL,ThumbnailURL, TagName, UserId")] News ad,
            HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;           
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength != 0)
                {
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    ad.ImageURL = imageBlob.Uri.ToString();
                }                
                if (ad == null)
                {
                    return HttpNotFound();
                }
                ad.PostedDate = DateTime.Now;
                ad.UserName = db.Users.Find(ad.UserId).UserName;
                if(ad.TagName != null)
                {
                    Tag tags = db.Tags.FirstOrDefault(s => s.TagName.Contains(ad.TagName));
                    if (tags == null)
                    {
                        Tag tag = new Tag();
                        tags = tag;
                    }
                    tags.TagName = ad.TagName;
                    ad.Tags.Add(tags);
                }                
                db.Newses.Add(ad);
                await db.SaveChangesAsync();                

                if (imageBlob != null)
                {
                    var queueMessage = new CloudQueueMessage(ad.NewsId.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);                    
                }
                return RedirectToAction("IndexAdmin/" + ad.UserId);
            }

            return View(ad);
        }
        #endregion

        #region News
        [Authorize(Roles = "writer")]
        public ViewResult IndexNews(string sortOrder, string currentFilter, string searchString, int? page)
        {
            IEnumerable<News> dates = db.Newses;
            dates = dates.Where(a => a.UserId == User.Identity.GetUserId());
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            
            if (!String.IsNullOrEmpty(searchString))
            {
                dates = dates.Where(s => s.Name.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    dates = dates.OrderByDescending(s => s.Name);
                    break;
                case "Date":
                    dates = dates.OrderBy(s => s.PostedDate);
                    break;
                case "date_desc":
                    dates = dates.OrderByDescending(s => s.PostedDate);
                    break;
                default:  // Name ascending 
                    dates = dates.OrderBy(s => s.Name);
                    break;
            }           
            return View(dates);            
        }


        // GET: Ad/Create
        [Authorize(Roles = "writer")]
        public ActionResult CreateNews()
        {
            return View();
        }
        
        // POST: Ad/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "writer")]
        public async Task<ActionResult> CreateNews(
            [Bind(Include = "NewsId,Name,Genre,Description,TextNews,ImageURL,ThumbnailURL, TagName")] News ad,
            HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;
            // A production app would implement more robust input validation.
            // For example, validate that the image file size is not too large.
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength != 0)
                {
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    ad.ImageURL = imageBlob.Uri.ToString();
                }
                if (ad == null)
                {
                    return HttpNotFound();
                }
                ad.PostedDate = DateTime.Now;
                ad.UserId = User.Identity.GetUserId();
                ad.UserName = User.Identity.Name;
                if (ad.TagName != null)
                {
                    Tag tags = db.Tags.FirstOrDefault(s => s.TagName.Contains(ad.TagName));
                    if (tags == null)
                    {
                        Tag tag = new Tag();
                        tags = tag;
                    }
                    tags.TagName = ad.TagName;
                    ad.Tags.Add(tags);
                }              
                db.Newses.Add(ad);
                await db.SaveChangesAsync();               

                if (imageBlob != null)
                {
                    var queueMessage = new CloudQueueMessage(ad.NewsId.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);                    
                }
                return RedirectToAction("IndexNews");
            }

            return View(ad);
        }

        [Authorize(Roles = "writer")]
        public ActionResult EditNews(int? newsId)
        {
            if (newsId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            News ad = db.Newses.FirstOrDefault(b => b.NewsId == newsId);
            if (ad == null)
            {
                return HttpNotFound();
            }            
            News adnew = new News();
            adnew.Name = ad.Name;
            adnew.Genre = ad.Genre;
            adnew.Description = ad.Description;
            adnew.TextNews = ad.TextNews;
            adnew.ImageURL = ad.ImageURL;
            adnew.ThumbnailURL = ad.ThumbnailURL;
            return View(adnew);
        }
        
        // POST: Ad/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "writer")]
        public async Task<ActionResult> EditNews(
            [Bind(Include = "NewsId,Name,Genre,Description,TextNews,ImageURL,ThumbnailURL, TagName")] News ad,
            HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength != 0)
                {                    
                    await DeleteAdBlobsAsyncNews(ad);
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    ad.ImageURL = imageBlob.Uri.ToString();
                }
                News p = db.Newses.FirstOrDefault(b => b.NewsId == ad.NewsId);                
                if (p == null)
                {
                    return HttpNotFound();
                }
                var vm = new EditViewModelNews
                {
                    NewsId = p.NewsId, // the Id you want to update
                    Name = ad.Name,
                    Genre = ad.Genre,
                    Description = ad.Description,
                    TextNews = ad.TextNews,
                    ImageURL = ad.ImageURL,
                    ThumbnailURL = ad.ThumbnailURL
                };
                vm.MapToModel(p);
                if (ad.TagName != null)
                {
                    Tag tags = db.Tags.FirstOrDefault(s => s.TagName.Contains(ad.TagName));
                    if (tags == null)
                    {
                        Tag tag = new Tag();
                        tags = tag;
                    }
                    tags.TagName = ad.TagName;
                    ad.Tags.Add(tags);
                }                
                db.SaveChanges();
                Trace.TraceInformation("Updated AdId {0} in database", ad.NewsId);
                if (imageBlob != null)
                {
                    var queueMessage = new CloudQueueMessage(ad.NewsId.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);                   
                }
                return RedirectToAction("IndexNews");
            }
            return RedirectToAction("IndexNews");
        }
                
        [HttpPost]
        [Authorize(Roles = "writer")]
        public ActionResult DeleteNews(int newsId)
        {            
            News dbEntry = db.Newses.Find(newsId);
            if (dbEntry != null)
            {
                IEnumerable<Rating> rating = db.Ratings.Where(a => a.UserId == dbEntry.UserId);
                foreach(var t in rating)
                {
                    db.Ratings.Remove(t);
                }
                IEnumerable<Comment> comment = db.Comments.Where(a => a.NewsId == dbEntry.NewsId);
                foreach (var t in comment)
                {
                    var likes = db.Likes.Where(b => b.CommentId == t.CommentId);
                    foreach (Like x in likes)
                    {
                        db.Likes.Remove(x);
                    }
                    Comment newComment = db.Comments.Find(t.CommentId);
                    db.Comments.Remove(newComment);
                }
                db.Newses.Remove(dbEntry);
                db.SaveChanges();
            }            
            return RedirectToAction("IndexNews");
        }
        #endregion

        #region Edit
        // GET: Ad/Edit/5
        public ActionResult Edit()
        {
            string userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser ad = db.Users.Find(userId);
            if (ad == null)
            {
                return HttpNotFound();
            }            
            ApplicationUser adnew = new ApplicationUser();
            adnew.UserName = ad.UserName;
            adnew.Age = ad.Age;
            adnew.Description = ad.Description;
            adnew.ImageURL = ad.ImageURL;
            adnew.ThumbnailURL = ad.ThumbnailURL;
            return View(adnew);
        }

        // POST: Ad/Edit/5        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(
            [Bind(Include = "Id,UserName,Age,Description,ImageURL,ThumbnailURL")] ApplicationUser ad,
            HttpPostedFileBase imageFile)
        {
            CloudBlockBlob imageBlob = null;
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength != 0)
                {                   
                    await DeleteAdBlobsAsync(ad);
                    imageBlob = await UploadAndSaveBlobAsync(imageFile);
                    ad.ImageURL = imageBlob.Uri.ToString();
                }
                string userId = User.Identity.GetUserId();
                if (userId == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                ApplicationUser p = db.Users.Find(userId);
                if (p == null)
                {
                    return HttpNotFound();
                }
                var vm = new EditViewModel
                {
                    Id = p.Id, // the Id you want to update
                    UserName = ad.UserName,
                    Age = ad.Age,
                    Description = ad.Description,
                    ImageURL = ad.ImageURL,
                    ThumbnailURL = ad.ThumbnailURL
                };
                vm.MapToModel(p);
                db.SaveChanges();                
                if (imageBlob != null)
                {
                    var queueMessage = new CloudQueueMessage(ad.Id.ToString());
                    await imagesQueue.AddMessageAsync(queueMessage);                    
                }
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
        #endregion 

        #region Roles
        [HttpPost]
        public ActionResult TakeUser(string id)
        {
            IList<string> roles = new List<string>();
            ApplicationUserManager userManager = HttpContext.GetOwinContext()
                                                    .GetUserManager<ApplicationUserManager>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }            
            if (userManager.GetRoles(user.Id).Count() > 1)
            {
                if(userManager.GetRoles(user.Id).Count() == 2)
                {
                    userManager.RemoveFromRole(id, "writer");
                }                
                else
                {
                    userManager.RemoveFromRole(id, "writer");
                    userManager.RemoveFromRole(id, "admin");
                }
            }                
            return RedirectToAction("Users");
        }

        [HttpPost]
        public ActionResult TakeWriter(string id)
        {
            IList<string> roles = new List<string>();
            ApplicationUserManager userManager = HttpContext.GetOwinContext()
                                                    .GetUserManager<ApplicationUserManager>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (userManager.GetRoles(user.Id).Count() > 1)
            {
                if (userManager.GetRoles(user.Id).Count() == 3)
                {
                    userManager.RemoveFromRole(id, "admin");
                }                
            }
            else
            {                
                userManager.AddToRole(id, "writer");                
            }
            return RedirectToAction("Users");
        }

        [HttpPost]
        public ActionResult TakeAdmin(string id)
        {
            IList<string> roles = new List<string>();
            ApplicationUserManager userManager = HttpContext.GetOwinContext()
                                                    .GetUserManager<ApplicationUserManager>();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            if (userManager.GetRoles(user.Id).Count() > 1)
            {
                if (userManager.GetRoles(user.Id).Count() == 2)
                {
                    userManager.AddToRole(id, "admin");
                }
            }
            else
            {
                userManager.AddToRole(id, "writer");
                userManager.AddToRole(id, "admin");
            }
            return RedirectToAction("Users");
        }
        #endregion  

        #region Users and Cloud
        [Authorize(Roles = "admin")]
        public ActionResult Users(string sortOrder, string currentFilter, string searchString, int? page, string[] selected, string Unlock, string Lock)
        {
            string userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser ad = db.Users.Find(userId);
            if (ad == null)
            {
                return HttpNotFound();
            }            
            ad.PostedDate = DateTime.Now;
            db.SaveChanges();

            if (selected != null)
            {                
                foreach (var c in db.Users.Where(co => selected.Contains(co.Id)))
                {
                    if (Lock != null)
                    {
                        c.Lock = true;
                    }
                    else if(Unlock!=null)
                    {
                        c.Lock = false;
                    }            
                    db.Entry(c).State = EntityState.Modified;
                }
            }            
            db.SaveChanges();
            
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var users = from s in db.Users
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
               users = users.Where(s => s.UserName.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(s => s.UserName);
                    break;
                case "Date":
                    users = users.OrderBy(s => s.PostedDate);
                    break;
                case "date_desc":
                    users = users.OrderByDescending(s => s.PostedDate);
                    break;
                default:  // Name ascending 
                    users = users.OrderBy(s => s.UserName);
                    break;
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(users.ToPagedList(pageNumber, pageSize));
            
        }

        private void InitializeStorage()
        {            
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));
                       
            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            imagesBlobContainer = blobClient.GetContainerReference("images");

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            queueClient.DefaultRequestOptions.RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(3), 3);

            imagesQueue = queueClient.GetQueueReference("images");
        }

        private async Task DeleteAdBlobsAsync(ApplicationUser ad)
        {
            if (!string.IsNullOrWhiteSpace(ad.ImageURL))
            {
                Uri blobUri = new Uri(ad.ImageURL);
                await DeleteAdBlobAsync(blobUri);
            }
            if (!string.IsNullOrWhiteSpace(ad.ThumbnailURL))
            {
                Uri blobUri = new Uri(ad.ThumbnailURL);
                await DeleteAdBlobAsync(blobUri);
            }
        }
        private async Task DeleteAdBlobsAsyncNews(News ad)
        {
            if (!string.IsNullOrWhiteSpace(ad.ImageURL))
            {
                Uri blobUri = new Uri(ad.ImageURL);
                await DeleteAdBlobAsync(blobUri);
            }
            if (!string.IsNullOrWhiteSpace(ad.ThumbnailURL))
            {
                Uri blobUri = new Uri(ad.ThumbnailURL);
                await DeleteAdBlobAsync(blobUri);
            }
        }
        private static async Task DeleteAdBlobAsync(Uri blobUri)
        {
            string blobName = blobUri.Segments[blobUri.Segments.Length - 1];            
            CloudBlockBlob blobToDelete = imagesBlobContainer.GetBlockBlobReference(blobName);
            await blobToDelete.DeleteAsync();
        }
        private async Task<CloudBlockBlob> UploadAndSaveBlobAsync(HttpPostedFileBase imageFile)
        {            
            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            
            CloudBlockBlob imageBlob = imagesBlobContainer.GetBlockBlobReference(blobName);
            
            using (var fileStream = imageFile.InputStream)
            {
                await imageBlob.UploadFromStreamAsync(fileStream);
            }
            
            return imageBlob;
        }
        #endregion       

        #region Manage
        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Создание и отправка маркера
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Ваш код безопасности: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Отправка SMS через поставщик SMS для проверки номера телефона
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // Это сообщение означает наличие ошибки; повторное отображение формы
            ModelState.AddModelError("", "Не удалось проверить телефон");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Это сообщение означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Внешнее имя входа удалено."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Запрос перенаправления к внешнему поставщику входа для связывания имени входа текущего пользователя
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Вспомогательные приложения
        public ActionResult AutocompleteSearch(string term)
        {
            var models = db.Tags.Where(a => a.TagName.Contains(term))
                            .Select(a => new { value = a.TagName })
                            .Distinct();

            return Json(models, JsonRequestBehavior.AllowGet);
        }

        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}