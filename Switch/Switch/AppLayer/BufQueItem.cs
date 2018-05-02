using SDNCommon;
using System;

namespace Switch
{
	public class BufQueItem
	{
		public DateTime EnQueueTime;
		public PacketInfo packetInfo;
		public string strDesIP;

		public BufQueItem(PacketInfo packetInfo, string strDesIP)
		{
			this.packetInfo = packetInfo;
			this.EnQueueTime = DateTime.Now;
			this.strDesIP = strDesIP;
		}
	}
}
