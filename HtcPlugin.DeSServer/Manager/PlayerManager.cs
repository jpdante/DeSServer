using System.Collections.Generic;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Core;
using HtcPlugin.DeSServer.Model;
using MySqlConnector;
// ReSharper disable InconsistentNaming

namespace HtcPlugin.DeSServer.Manager {
    public class PlayerManager {

        private readonly Dictionary<string, Player> _playersByHost;
        private readonly Dictionary<string, Player> _playersByNPID;

        public PlayerManager() {
            _playersByHost = new Dictionary<string, Player>();
            _playersByNPID = new Dictionary<string, Player>();
        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }


        public async Task<string> InitPlayer(string remoteHost, string playerName, string index) {
            var playerId = $"{playerName}{index}";
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("INSERT INTO players (player_id, grade_s, grade_a, grade_b, grade_c, grade_d, logins, sessions, msg_rating, tendency) VALUES (@playerId, 0, 0, 0, 0, 0, 1, 0, 0, 0) ON DUPLICATE KEY UPDATE logins = logins + 1;", conn);
            cmd.Parameters.AddWithValue("playerId", playerId);
            await cmd.ExecuteNonQueryAsync();
            var player = new Player(playerId);
            _playersByHost.TryAdd(remoteHost, player);
            _playersByNPID.TryAdd(playerId, player);
            return playerId + "\x00";
        }

        public bool GetPlayerByHost(string host, out Player player) => _playersByHost.TryGetValue(host, out player);
        public bool GetPlayerByNPID(string npid, out Player player) => _playersByNPID.TryGetValue(npid, out player);
    }
}