using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<byte[]> Serialize() {
            await using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes(Id));
            await memoryStream.WriteAsync(Encoding.ASCII.GetBytes($"{PlayerInfo.PlayerId}\x00"));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PosX));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PosY));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PosZ));
            await memoryStream.WriteAsync(BitConverter.GetBytes(RotX));
            await memoryStream.WriteAsync(BitConverter.GetBytes(RotY));
            await memoryStream.WriteAsync(BitConverter.GetBytes(RotZ));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) MsgId));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) MainMsgId));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) MsgCateId));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) 0));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) PlayerInfo.GradeS));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) PlayerInfo.GradeA));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) PlayerInfo.GradeB));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) PlayerInfo.GradeC));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) PlayerInfo.GradeD));
            await memoryStream.WriteAsync(BitConverter.GetBytes((uint) 0));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PlayerInfo.Sessions));
            await memoryStream.WriteAsync(Encoding.ASCII.GetBytes($"{ClientPlayerInfo}\x00"));
            await memoryStream.WriteAsync(BitConverter.GetBytes(Qwcwb));
            await memoryStream.WriteAsync(BitConverter.GetBytes(Qwclr));
            memoryStream.WriteByte(IsBlack ? (byte) 1 : (byte) 2);
            return memoryStream.ToArray();
        }

        public void Dispose() {

        }
    }
}