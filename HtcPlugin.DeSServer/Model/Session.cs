using System;

namespace HtcPlugin.DeSServer.Model {
    public class Session : IDisposable {

        public uint Id { get; }
        public PlayerInfo PlayerInfo { get; }
        public uint BlockId { get; }
        public float PosX { get; }
        public float PosY { get; }
        public float PosZ { get; }
        public float RotX { get; }
        public float RotY { get; }
        public float RotZ { get; }
        public int MsgId { get; }
        public int MainMsgId { get; }
        public int MsgCateId { get; }
        public string ClientPlayerInfo { get; }
        public int Qwcwb { get; }
        public int Qwclr { get; }
        public bool IsBlack { get; }
        public int PlayerLevel { get; }
        public DateTime LastHeartbeat;

        public Session(
            uint id, PlayerInfo playerInfo, uint blockId,
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ,
            int msgId, int mainMsgId, int msgCateId,
            string clientPlayerInfo, int qwcwb, int qwclr,
            bool isBlack, int playerLevel) {
            Id = id;
            PlayerInfo = playerInfo;
            BlockId = blockId;
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
            RotX = rotX;
            RotY = rotY;
            RotZ = rotZ;
            MsgId = msgId;
            MainMsgId = mainMsgId;
            MsgCateId = msgCateId;
            ClientPlayerInfo = clientPlayerInfo;
            Qwcwb = qwcwb;
            Qwclr = qwclr;
            IsBlack = isBlack;
            PlayerLevel = playerLevel;
        }


        public void Dispose() {

        }
    }
}