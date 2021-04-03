using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using MySqlConnector;

namespace HtcPlugin.DeSServer.Manager {
    public class PlayerManager {
        
        public PlayerManager() {

        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public async Task InitPlayer(string playerName, string index) {
            var conn = await DatabaseContext.GetConnection();
            var cmd = new MySqlCommand("INSERT INTO players (guild_id, user_id, level, xp, profile) VALUES", conn);
        }
    }
}