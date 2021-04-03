using System;

namespace HtcPlugin.DeSServer.Model {
    public class Ghost {

        public string PlayerId { get; private set; }
        public int BlockId { get; private set; }
        public DateTime CreationTime { get; private set; }
        public byte[] ReplayData { get; private set; }

        public Ghost() {
            CreationTime = DateTime.Now;
            ReplayData = new byte[0];
        }

        public byte[] GetReplayData() {
            return ReplayData;
        }
    }
}
