using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Palaver2.Startup))]
namespace Palaver2
{

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}