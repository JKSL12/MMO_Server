using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AccountServer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseUrls("https://*:5001", "http://*:5000", "https://localhost:5001");
					webBuilder.UseStartup<Startup>();
					//webBuilder.ConfigureKestrel(serverOptions =>
					//{
					//	serverOptions.ListenAnyIP(5000);
					//	serverOptions.ListenAnyIP(5001);
					//	serverOptions.ListenAnyIP(6000);
					//});
				});
	}
}
