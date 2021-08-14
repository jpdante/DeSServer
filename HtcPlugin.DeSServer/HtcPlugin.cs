using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcSharp.Abstractions;
using HtcSharp.HttpModule;
using HtcSharp.HttpModule.Abstractions;
using HtcSharp.HttpModule.Directive;
using HtcSharp.HttpModule.Http;
using HtcSharp.HttpModule.Mvc;
using HtcSharp.Logging;
using HtcSharp.Shared.IO;
using RedNX.Config;

namespace HtcPlugin.DeSServer {
    public class HtcPlugin : HttpMvc, IPlugin, IHttpPage {
        public string Name => "DeS";
        public string Version => DeSServer.Version.GetVersion();

        internal static readonly ILogger Logger = LoggerManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        internal static DeSConfig Config { get; private set; }
        internal static Core.DeSServer Server { get; private set; }

        public async Task Init(IServiceProvider serviceProvider) {
            string configPath = Path.Combine(PathExt.GetConfigPath(true, "des-server"), "config.json");
            if (!File.Exists(configPath)) {
                Config = new DeSConfig();
                await ConfigManager.SaveToFileAsync(Config, configPath);
            }
            Config = await ConfigManager.LoadFromFileAsync<DeSConfig>(configPath);

            _ = new DatabaseContext(Config.Db);
            Server = new Core.DeSServer();

            LoadControllers(Assembly.GetExecutingAssembly());
            this.RegisterMvc(this);
        }

        public async Task Enable() {
            await Server.Enable();
        }

        public async Task Disable() {
            await Server.Disable();
        }

        public bool IsCompatible(IVersion version) {
            return true;
        }

        public void Dispose() {

        }

        public Task OnHttpPageRequest(DirectiveDelegate next, HtcHttpContext httpContext, string fileName) {
            Logger.LogInfo(fileName);
            return Task.CompletedTask;
        }
    }
}