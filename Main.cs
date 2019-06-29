using System;
using System.IO;
using System.Configuration;
using System.Reflection;
using Gtk;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Xunit;
using NLog.Config;
using NLog.Targets;

namespace TelegramApp
{
    class Program
    {
        public static readonly Client Client;
        public static readonly IConfigurationRoot Config;
        public static Logger Logger;

        static Program()
        { 
            Config = new ConfigurationBuilder()
                .AddJsonFile("appconfig.json")
                .AddUserSecrets<Program>()
                .Build();

            int api_id;
            var api_hash = Config["API_HASH"];
            Assert.True(int.TryParse(Config["API_ID"], out api_id), "Failed to read api_id");
            Assert.True(api_hash != null, "Failed to read api_hash");

            Client = new Client(api_id, api_hash);      
        }

        private static void SetupLogging()
        {
            var nlogConfig = new LoggingConfiguration();

            var fileTarget = new FileTarget("file")
            {
                FileName = Path.Combine(DataStorage.UserDataFolder, "nlog.log"),
                KeepFileOpen = true,
                ConcurrentWrites = false,
                DeleteOldFileOnStartup = true,
            };

            nlogConfig.AddTarget(fileTarget);
            nlogConfig.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Debug, fileTarget));

            var consoleTarget = new ConsoleTarget("console");
            nlogConfig.AddTarget(consoleTarget);
            nlogConfig.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Warn, consoleTarget));

            LogManager.Configuration = nlogConfig;
            LogManager.EnableLogging();
            Logger = LogManager.GetCurrentClassLogger();
        }

        private static void SetupDllPaths()
        {
            var dllDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Config["GtkDllPath"]);
            Environment.SetEnvironmentVariable("PATH", Environment.GetEnvironmentVariable("PATH") + ";" + dllDirectory);
            Assert.False(string.IsNullOrEmpty(dllDirectory), "Gtk dll directory not found");
        }

        [STAThread]
        public static void Main(string[] args)
        {
            SetupLogging();
            SetupDllPaths();

            Application.Init();

            var app = new Application("org.telega_sharp.telega_sharp", GLib.ApplicationFlags.None);
            app.Register(GLib.Cancellable.Current);

            var win = new MainWindow();

            app.AddWindow(win);

            win.Show();
            Application.Run();
        }
    }
}
