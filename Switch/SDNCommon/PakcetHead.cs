
using System;

namespace SDNCommon
{
	[Serializable]
	public class PacketHead
    {
		public enum EN_PACKET_TYPE
		{
			EN_DEFAULT_TYPE = 0,
			EN_SWITCH_ONLINE,
			EN_ACK_SWITCH_ONLINE,
			EN_NORMAL_PACKET,
			EN_PACKET_IN,
			EN_PACKET_OUT_WITH_FLOW_ITEM,
			EN_PACKET_OUT_DOUNP_DUMP
		}
		public string strSrcIP;
		public string strDesIP;
		public EN_PACKET_TYPE enPacketType;

		public PacketHead(string strSrcIP, string strDesIP, EN_PACKET_TYPE packetType)
		{
			this.strSrcIP = strSrcIP;
			this.strDesIP = strDesIP;
			this.enPacketType = packetType;
		}
    }
}
