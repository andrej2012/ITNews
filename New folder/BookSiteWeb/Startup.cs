using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BookSiteWeb.Startup))]
namespace BookSiteWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
