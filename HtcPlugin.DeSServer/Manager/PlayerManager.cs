using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using HtcPlugin.DeSServer.Core;
using HtcPlugin.DeSServer.Model;
using HtcSharp.Core.Logging.Abstractions;
using MySqlConnector;
// ReSharper disable InconsistentNaming

namespace HtcPlugin.DeSServer.Manager {
    public class PlayerManager {

        private readonly Dictionary<string, Player> _playersByHost;
        private readonly Dictionary<string, Player> _playersByNPID;
        private readonly List<Player> _players;
        private readonly Timer _timer;
        private bool _running;

        public PlayerManager() {
            _playersByHost = new Dictionary<string, Player>();
            _playersByNPID = new Dictionary<string, Player>();
            _players = new List<Player>();
            _timer = new Timer(5000);
            _timer.Elapsed += TimerOnElapsed;
        }

        private async void TimerOnElapsed(object sender, ElapsedEventArgs e) {
            if (_running) return;
            try {
                _running = true;
                await DisconnectPlayers();
            } catch (Exception ex) {
                HtcPlugin.Logger.LogError(ex);
                HtcPlugin.Logger.LogError(ex.StackTrace);
            } finally {
                _running = false;
            }
        }

        private async Task DisconnectPlayers() {
            await using var conn = await DatabaseContext.GetConnection();
            List<Player> removeList = _players.Where(p => p.LastHeartbeat.AddSeconds(HtcPlugin.Config.DeSServer.PlayerHeartbeatTimeout) <= DateTime.Now).ToList();
            foreach (var player in removeList) {
                await RemovePlayer(player, conn);
            }
        }

        private async Task RemovePlayer(Player player, MySqlConnection conn = null) {
            _playersByHost.Remove(player.Host);
            _playersByNPID.Remove(player.PlayerId);
            _players.Remove(player);
            HtcPlugin.Server.SessionManager.DisconnectPlayer(player);
            HtcPlugin.Logger.LogInfo($"[PlayerManager] {player.PlayerId} disconnected.");
            bool disposeConnection = conn == null;
            conn ??= await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("UPDATE players SET play_time = play_time + @playTime WHERE player_id = @playerId;", conn);
            cmd.Parameters.AddWithValue("playerId", player.PlayerId);
            cmd.Parameters.AddWithValue("playTime", (uint) (DateTime.Now - player.LoginDateTime).TotalSeconds);
            await cmd.ExecuteNonQueryAsync();
            if (disposeConnection) await conn.DisposeAsync();
        }

        public Task Enable() {
            _timer.Start();
            return Task.CompletedTask;
        }

        public Task Disable() {
            _timer.Stop();
            return Task.CompletedTask;
        }

        public async Task<string> InitPlayer(string remoteHost, string playerName, string index) {
            var playerId = $"{playerName}{index}";
            await using var conn = await DatabaseContext.GetConnection();
            await using var cmd = new MySqlCommand("INSERT INTO players (player_id, grade_s, grade_a, grade_b, grade_c, grade_d, logins, sessions, msg_rating, tendency, play_time) VALUES (@playerId, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0) ON DUPLICATE KEY UPDATE logins = logins + 1;", conn);
            cmd.Parameters.AddWithValue("playerId", playerId);
            await cmd.ExecuteNonQueryAsync();
            if (_playersByNPID.TryGetValue(playerId, out var p1)) await RemovePlayer(p1, conn);
            if (_playersByHost.TryGetValue(playerId, out var p2)) await RemovePlayer(p2, conn);
            var player = new Player(playerId, remoteHost);
            _playersByHost.TryAdd(remoteHost, player);
            _playersByNPID.TryAdd(playerId, player);
            _players.Add(player);
            HtcPlugin.Logger.LogInfo($"[PlayerManager] {player.PlayerId} connected.");
            return playerId + "\x00";
        }

        public bool GetPlayerByHost(string host, out Player player) => _playersByHost.TryGetValue(host, out player);
        public bool GetPlayerByNPID(string npid, out Player player) => _playersByNPID.TryGetValue(npid, out player);

        public void Heartbeat(Player player, bool fromOtherPlayer = false) {
            HtcPlugin.Logger.LogInfo($"[Heartbeat] {player.PlayerId}");
            foreach (var p in _players) {
                p.LastHeartbeat = DateTime.Now;
            }
        }
    }
}