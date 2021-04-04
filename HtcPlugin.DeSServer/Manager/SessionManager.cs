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
            foreach (var session in _sessions.Where(session => session.LastHeartbeat.AddSeconds(30) <= DateTime.Now)) {
                DisposeSession(session);
            }

            var knewSessions = new List<uint>();
            var newSessions = new List<uint>();
            foreach (var session in _sessions.Where(session => session.BlockId == blockId)) {
                if (sosList.Contains(session.Id.ToString())) {
                    knewSessions.Add(session.Id);
                } else {
                    if (newSessions.Count < sosNum) newSessions.Add(session.Id);
                }
            }

            await using var memoryStream = new MemoryStream();

            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) knewSessions.Count));
            foreach (uint sessionId in knewSessions) {
                await memoryStream.WriteAsync(BitConverter.GetBytes(sessionId));
            }

            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) newSessions.Count));
            foreach (uint sessionId in newSessions) {
                await memoryStream.WriteAsync(BitConverter.GetBytes(sessionId));
            }

            return memoryStream.ToArray();
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

        public byte[] SummonPlayer(uint ghostId, string npRoomId) {
            foreach (var session in _sessions.Where(x => x.Id == ghostId)) {
                _playersPending.Add(session.PlayerInfo.PlayerId, npRoomId);
                return new[] {(byte) '\x01'};
            }
            return new[] {(byte) '\x00'};
        }

        public byte[] SummonBlackGhost(string npRoomId) {
            foreach (var session in _sessions.Where(x => _allowedInvasionLocations.Contains(x.BlockId))) {
                _invadersPending.Add(session.PlayerInfo.PlayerId, npRoomId);
                return new[] {(byte) '\x01'};
            }
            return new[] {(byte) '\x00'};
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
