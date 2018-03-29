using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switch
{
	/// <summary>
	/// 存在消息队列中的消息类
	/// </summary>
	public class Packet
	{
		private int iPhyPortNo;
		private byte[] byteBuffer;

		public Packet(int iPhyPortNo, byte[] buffer)
		{
			this.iPhyPortNo = iPhyPortNo;
			this.byteBuffer = buffer;
		}

		public int GetPhyPort()
		{
			return this.iPhyPortNo;
		}

		public byte[] GetBuffer()
		{
			return this.byteBuffer;
		}
	}
}
