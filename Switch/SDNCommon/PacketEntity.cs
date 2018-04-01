using System;

namespace SDNCommon
{
	[Serializable]
    public class PacketEntity
    {
		private PacketHead head;
		private string strContent;

		public PacketHead GetHead()
		{
			return this.head;
		}

		public string GetContent()
		{
			return this.strContent;
		}
	}
}
