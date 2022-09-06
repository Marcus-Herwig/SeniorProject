using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SeniorProject
{
    class Startup
    {
        string appsettingsPath;
        ChatAppConfiguration _config;
        public Startup()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string basePath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            this.appsettingsPath = basePath + "\\appsetttings.json";
            this._config = new ChatAppConfiguration();
        }

        public ChatAppConfiguration ConfigureApp()
        {
            try
            {
                IConfigurationRoot? config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(this.appsettingsPath, optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
                config.Bind(this._config);
            }
            catch (Exception e)
            {
                Console.WriteLine("failed to get access to config file to configure app settings");
                Console.WriteLine(e.Message);
            }
            return this._config;
        }
    }
}
