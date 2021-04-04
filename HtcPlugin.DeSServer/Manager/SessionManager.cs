using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HtcPlugin.DeSServer.Model;
using HtcSharp.Core.Logging.Abstractions;

namespace HtcPlugin.DeSServer.Manager {
    public class SessionManager {

        private readonly List<Session> _sessions;
        private readonly Queue<uint> _availableSessionIds;
        private readonly Dictionary<string, string> _playersPending;
        private readonly Dictionary<string, string> _invadersPending;
        private readonly Dictionary<string, Session> _sessionsByNPID;

        public SessionManager() {
            _sessions = new List<Session>();
            _availableSessionIds = new Queue<uint>();
            for (uint i = 0; i < 1000; i++) {
                _availableSessionIds.Enqueue(i);
            }
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

        public byte[] CheckSession(Player player) {
            byte[] data;
            if (_invadersPending.TryGetValue(player.PlayerId, out string dataRaw)) {
                data = Encoding.ASCII.GetBytes(dataRaw);
            } else if (_playersPending.TryGetValue(player.PlayerId, out string dataRaw2)) {
                data = Encoding.ASCII.GetBytes(dataRaw2);
            } else {
                data = new[] {(byte) '\x01'};
            }
            return data;
        }

        public void SetOutOfBlock(Player player) {
            if (!_sessionsByNPID.TryGetValue(player.PlayerId, out var session)) return;
            DisposeSession(session);
        }

        public void DisposeSession(Session session) {
            session.Dispose();
            _availableSessionIds.Enqueue(session.Id);
            _sessions.Remove(session);
            _sessionsByNPID.Remove(session.PlayerInfo.PlayerId);
            HtcPlugin.Logger.LogInfo($"[SessionManager] Delete session ID: {session.Id}");
        }

        public void Heartbeat(Player player) {
            if (_sessionsByNPID.TryGetValue(player.PlayerId, out var session)) {
                session.LastHeartbeat = DateTime.Now;
            }
        }
    }
}
