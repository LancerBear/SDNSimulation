
namespace SDNCommon
{
	/// <summary>
	/// 存在消息队列中的消息类
	/// </summary>
	public class PacketInfo
	{
		private int iPhyPortNo;
		private byte[] packetByte;

		public PacketInfo(int iPhyPortNo, byte[] pakcetByte)
		{
			this.iPhyPortNo = iPhyPortNo;
			this.packetByte = pakcetByte;
		}

		public int GetPhyPort()
		{
			return this.iPhyPortNo;
		}

		public byte[] GetPacketByte()
		{
			return this.packetByte;
		}
	}
}
