using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
[assembly: OwinStartup(typeof(CrmCorner.Startup))]
namespace CrmCorner
{

        public class Startup
        {


        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    // Diğer yapılandırmalar...

        //    // appsettings.json dosyasından yapılandırma yüklemek için
        //    IConfiguration config = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //        .Build();

        //    services.AddSingleton<IConfiguration>(config);
        //}


    }
}

