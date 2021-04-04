using System;

namespace HtcPlugin.DeSServer.Model {
    public class Ghost {

        public string PlayerId { get; private set; }
        public uint BlockId { get; private set; }
        public byte[] ReplayData { get; private set; }
        public DateTime CreationTime { get; private set; }

        public Ghost(string playerId, uint blockId, byte[] replayData) {
            PlayerId = playerId;
            BlockId = blockId;
            ReplayData = replayData;
            CreationTime = DateTime.Now;
        }
    }
}
