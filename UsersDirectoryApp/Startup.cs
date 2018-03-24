using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UsersDirectoryApp.Startup))]
namespace UsersDirectoryApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
