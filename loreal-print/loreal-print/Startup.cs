using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(loreal_print.Startup))]
namespace loreal_print
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
