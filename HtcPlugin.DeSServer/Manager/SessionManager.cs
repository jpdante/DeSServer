using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Model;
using HtcSharp.Core.Logging.Abstractions;
// ReSharper disable InconsistentNaming

namespace HtcPlugin.DeSServer.Manager {
    public class SessionManager {

        private readonly List<Session> _sessions;
        private readonly Queue<uint> _availableSessionIds;
        private readonly List<uint> _allowedInvasionLocations;
        private readonly Dictionary<string, string> _playersPending;
        private readonly Dictionary<string, string> _invadersPending;
        private readonly Dictionary<string, Session> _sessionsByNPID;

        public SessionManager() {
            _sessions = new List<Session>();
            _availableSessionIds = new Queue<uint>();
            for (uint i = 0; i < 1000; i++) {
                _availableSessionIds.Enqueue(i);
            }
            _allowedInvasionLocations = new List<uint> {
                40070,
                40071,
                40072,
                40073,
                40074,
                40170,
                40171,
                40172,
                40270
            };
            _playersPending = new Dictionary<string, string>();
            _invadersPending = new Dictionary<string, string>();
            _sessionsByNPID = new Dictionary<string, Session>();
        }

        public Task Enable() {
            return Task.CompletedTask;
        }

        public Task Disable() {
            return Task.CompletedTask;
        }

        public async Task<bool> CreateSession(Player player, uint blockId, float posX, float posY, float posZ, float rotX, float rotY, float rotZ, int messageId, int mainMsgId, int addMsgCateId, string clientPlayerInfo, int qwcwb, int qwclr, bool isBlack, int playerLevel) {
            var playerInfo = await player.GetPlayerInfo();
            bool success = _availableSessionIds.TryDequeue(out uint id);
            var session = new Session(id, playerInfo, blockId,  posX,  posY,  posZ,  rotX,  rotY,  rotZ,  messageId,  mainMsgId,  addMsgCateId, clientPlayerInfo,  qwcwb,  qwclr, isBlack,  playerLevel);
            _sessions.Add(session);
            _sessionsByNPID.Add(session.PlayerInfo.PlayerId, session);
            HtcPlugin.Logger.LogInfo($"[SessionManager] Create new session for {session.PlayerInfo.PlayerId} ID: {session.Id}");
            return success;
        }

        public async Task<byte[]> GetSessionData(uint blockId, uint sosNum, string[] sosList) {
            HtcPlugin.Logger.LogInfo($"[SessionManager] Get session data");
            foreach (var session in _sessions.Where(session => session.LastHeartbeat.AddSeconds(HtcPlugin.Config.DeSServer.SessionHeartbeatTimeout) <= DateTime.Now).ToArray()) {
                DisposeSession(session);
            }

            var knewSessions = new List<byte[]>();
            var newSessions = new List<byte[]>();
            foreach (var session in _sessions.Where(session => session.BlockId == blockId)) {
                if (sosList.Contains(session.Id.ToString())) {
                    knewSessions.Add(BitConverter.GetBytes(session.Id));
                    HtcPlugin.Logger.LogInfo($"[SessionManager] Adding knew session ID: {session.Id}");
                } else {
                    //if (newSessions.Count >= sosNum) continue;
                    newSessions.Add(await session.Serialize());
                    HtcPlugin.Logger.LogInfo($"[SessionManager] Adding new session ID: {session.Id}");
                }
            }

            await using var memoryStream = new MemoryStream();

            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) knewSessions.Count));
            foreach (byte[] sessionData in knewSessions) {
                await memoryStream.WriteAsync(sessionData);
            }

            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) newSessions.Count));
            foreach (byte[] sessionData in newSessions) {
                await memoryStream.WriteAsync(sessionData);
            }

            return memoryStream.ToArray();
        }

        public byte[] CheckSession(Player player) {
            HtcPlugin.Logger.LogInfo($"[SessionManager] Check session {player.PlayerId}");
            byte[] data;
            if (_invadersPending.TryGetValue(player.PlayerId, out string dataRaw)) {
                _invadersPending.Remove(player.PlayerId);
                data = Encoding.ASCII.GetBytes(dataRaw);
                HtcPlugin.Logger.LogInfo($"[SessionManager] Summoning invation to {player.PlayerId}");
            } else if (_playersPending.TryGetValue(player.PlayerId, out string dataRaw2)) {
                _playersPending.Remove(player.PlayerId);
                data = Encoding.ASCII.GetBytes(dataRaw2);
                HtcPlugin.Logger.LogInfo($"[SessionManager] Connecting player to {player.PlayerId}");
            } else {
                data = new[] {(byte) '\x01'};
            }
            return data;
        }

        public void SetOutOfBlock(Player player) {
            HtcPlugin.Logger.LogInfo($"[SessionManager] {player.PlayerId} out of range");
            if (!_sessionsByNPID.TryGetValue(player.PlayerId, out var session)) return;
            DisposeSession(session);
        }

        public byte[] SummonPlayer(uint ghostId, string npRoomId) {
            foreach (var session in _sessions.Where(x => x.Id == ghostId)) {
                HtcPlugin.Logger.LogInfo($"[SessionManager] {session.PlayerInfo.PlayerId} is attempting to summon ghost ID: {ghostId} NPRoomID: {npRoomId}");
                _playersPending.Add(session.PlayerInfo.PlayerId, npRoomId);
                return new[] {(byte) '\x01'};
            }
            return new[] {(byte) '\x00'};
        }

        public byte[] SummonBlackGhost(string npRoomId) {
            foreach (var session in _sessions.Where(x => _allowedInvasionLocations.Contains(x.BlockId))) {
                HtcPlugin.Logger.LogInfo($"[SessionManager] {session.PlayerInfo.PlayerId} is attempting to summon invader NPRoomID: {npRoomId}");
                _invadersPending.Add(session.PlayerInfo.PlayerId, npRoomId);
                return new[] {(byte) '\x01'};
            }
            return new[] {(byte) '\x00'};
        }

        public void DisconnectPlayer(Player player) {
            if (_sessionsByNPID.TryGetValue(player.PlayerId, out var session)) DisposeSession(session);
            _sessionsByNPID.Remove(player.PlayerId);
            _playersPending.Remove(player.PlayerId);
            _invadersPending.Remove(player.PlayerId);
        }

        public void DisposeSession(Session session) {
            session.Dispose();
            _availableSessionIds.Enqueue(session.Id);
            _sessions.Remove(session);
            _sessionsByNPID.Remove(session.PlayerInfo.PlayerId);
            HtcPlugin.Logger.LogInfo($"[SessionManager] Delete session ID: {session.Id}");
        }

        public void Heartbeat(Player player) {
            if (!_sessionsByNPID.TryGetValue(player.PlayerId, out var session)) return;
            HtcPlugin.Logger.LogInfo($"[Heartbeat] {player.PlayerId}, Session {session.Id}");
            session.LastHeartbeat = DateTime.Now;
        }
    }
}
