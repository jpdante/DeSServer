using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HtcPlugin.DeSServer.Model {
    public class Replay {

        public uint Id { get; set; }
        public string PlayerId { get; set; }
        public uint BlockId { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public int MsgId { get; set; }
        public int MainMsgId { get; set; }
        public int MsgCateId { get; set; }

        public async Task<byte[]> GenerateHeader() {
            await using var memoryStream = new MemoryStream();
            await memoryStream.WriteAsync(BitConverter.GetBytes(Id));
            await memoryStream.WriteAsync(Encoding.ASCII.GetBytes($"{PlayerId}\x00"));
            await memoryStream.WriteAsync(BitConverter.GetBytes(BlockId));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PosX));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PosY));
            await memoryStream.WriteAsync(BitConverter.GetBytes(PosZ));
            await memoryStream.WriteAsync(BitConverter.GetBytes(RotX));
            await memoryStream.WriteAsync(BitConverter.GetBytes(RotY));
            await memoryStream.WriteAsync(BitConverter.GetBytes(RotZ));
            await memoryStream.WriteAsync(BitConverter.GetBytes(MsgId));
            await memoryStream.WriteAsync(BitConverter.GetBytes(MainMsgId));
            await memoryStream.WriteAsync(BitConverter.GetBytes(MsgCateId));
            return memoryStream.ToArray();
        }

    }
}
