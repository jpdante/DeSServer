using System.Collections.Generic;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Model;

namespace HtcPlugin.DeSServer.Manager {
    public class SessionManager {

        public List<Session> _sessions;

        public SessionManager() {
            _sessions = new List<Session>();
        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public void DeleteSession(string playerId) {

        }
    }
}
