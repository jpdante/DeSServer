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

        public Task Enable() {
            PlayerManager.Enable();
            MessageManager.Enable();
            GhostManager.Enable();
            ReplayManager.Enable();
            SessionManager.Enable();
            return Task.CompletedTask;
        }

        public Task Disable() {
            PlayerManager.Disable();
            MessageManager.Disable();
            GhostManager.Disable();
            ReplayManager.Disable();
            SessionManager.Disable();
            return Task.CompletedTask;
        }
    }
}
