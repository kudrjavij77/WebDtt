using Microsoft.Owin;
using Owin;

//[assembly: OwinStartupAttribute(typeof(WebDtt.Startup))]
namespace WebDtt
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
