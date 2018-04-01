
using System;

namespace SDNCommon
{
	[Serializable]
	public class PacketHead
    {
		public string strSrcIP;
		public string strDesIP;

		public PacketHead(string strSrcIP, string strDesIP)
		{
			this.strSrcIP = strSrcIP;
			this.strDesIP = strDesIP;
		}
    }
}
