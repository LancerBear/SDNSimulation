using System;
using System.Text;

namespace SDNCommon
{
	[Serializable]
    public class PacketEntity
    {
		private PacketHead head;
		//private string strContent;
		private byte[] byteContent;

		public PacketEntity(PacketHead head, string strContent)
		{
			this.head = head;
			this.byteContent = Encoding.Default.GetBytes(strContent);
		}

		public PacketEntity(PacketHead head, byte[] byteContent)
		{
			this.head = head;
			this.byteContent = byteContent;
		}

		public PacketHead GetHead()
		{
			return this.head;
		}

		public string GetStrContent()
		{
			return Encoding.Default.GetString(this.byteContent);
		}

		public byte[] GetByteContent()
		{
			return this.byteContent;
		}
	}
}
