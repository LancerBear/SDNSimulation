using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDNCommon
{
	/// <summary>
	/// 存在消息队列中的消息类
	/// </summary>
	public class PacketInfo
	{
		private int iPhyPortNo;
		private PacketEntity packetEntity;

		public PacketInfo(int iPhyPortNo, PacketEntity packetEntity)
		{
			this.iPhyPortNo = iPhyPortNo;
			this.packetEntity = packetEntity;
		}

		public int GetPhyPort()
		{
			return this.iPhyPortNo;
		}

		public PacketEntity GetPacketEntity()
		{
			return this.packetEntity;
		}
	}
}
