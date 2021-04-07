using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcSharp.Core.Logging.Abstractions;
using HtcSharp.Core.Plugin;
using HtcSharp.Core.Plugin.Abstractions;
using HtcSharp.HttpModule;
using HtcSharp.HttpModule.Http.Abstractions;
using HtcSharp.HttpModule.Http.Abstractions.Extensions;
using RedNX.Config;
using RedNX.IO;

namespace HtcPlugin.DeSServer {
    public class HtcPlugin : HttpMvc, IPlugin {

        public string Name => "DES Server";
        public string Version => DeSServer.Version.GetVersion();

        internal static ILogger Logger { get; private set; }
        internal static DeSConfig Config { get; private set; }
        internal static Core.DeSServer Server { get; private set; }
        internal static string Endpoint { get; private set; }

        public async Task Load(PluginServerContext pluginServerContext, ILogger logger) {
            Logger = logger;
            string configPath = Path.Combine(PathExt.GetConfigPath(true, "des-server"), "config.json");
            if (!File.Exists(configPath)) {
                Config = new DeSConfig();
                await ConfigManager.SaveToFileAsync(Config, configPath);
            }
            Config = await ConfigManager.LoadFromFileAsync<DeSConfig>(configPath);

            Endpoint = $"0.0.0.0:{Config.DeSServer.Port}";
            _ = new DatabaseContext(Config.Db);
            Server = new Core.DeSServer();

            Setup(Assembly.GetExecutingAssembly(), logger);
        }

        public async Task Enable() {
            await Server.Enable();
        }

        public async Task Disable() {
            await Server.Disable();
        }

        public bool IsCompatible(int htcMajor, int htcMinor, int htcPatch) {
            return true;
        }

        public override Task<bool> BeforePageRequest(HttpContext httpContext, string filename) {
            return Task.FromResult(!httpContext.ServerInfo.Endpoints.Contains(Endpoint));
        }

        public override async Task ThrowException(HttpContext httpContext, Exception exception) {
            await httpContext.Response.WriteAsync(exception.Message);
        }
    }
}