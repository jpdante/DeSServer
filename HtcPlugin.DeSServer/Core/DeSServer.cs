using System.Threading.Tasks;
using HtcPlugin.DeSServer.Manager;

namespace HtcPlugin.DeSServer.Core {
    public class DeSServer {

        public MessageManager MessageManager { get; }
        public SessionManager SessionManager { get; }
        public PlayerManager PlayerManager { get; }
        public ReplayManager ReplayManager { get; }
        public GhostManager GhostManager { get; }

        public DeSServer() {
            MessageManager = new MessageManager();
            SessionManager = new SessionManager();
            PlayerManager = new PlayerManager();
            ReplayManager = new ReplayManager();
            GhostManager = new GhostManager();
        }

        public async Task Enable() {
            await PlayerManager.Enable();
            await MessageManager.Enable();
            await GhostManager.Enable();
            await ReplayManager.Enable();
            await SessionManager.Enable();
        }

        public async Task Disable() {
            await PlayerManager.Disable();
            await MessageManager.Disable();
            await GhostManager.Disable();
            await ReplayManager.Disable();
            await SessionManager.Disable();
        }
    }
}
