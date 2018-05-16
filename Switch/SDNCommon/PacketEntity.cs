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
		public bool isContentStr;

		public PacketEntity(PacketHead head, string strContent)
		{
			this.head = head;
			this.byteContent = Encoding.Default.GetBytes(strContent);
			isContentStr = true;
		}

		public PacketEntity(PacketHead head, byte[] byteContent)
		{
			this.head = head;
			this.byteContent = byteContent;
			isContentStr = false;
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
