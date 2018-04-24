using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDNCommon;

namespace Controller
{
	/// <summary>
	/// 暴露给上层的接口类，给上层调用完成数据包的发送和接收
	/// </summary>
	public static class Transmitter
	{
		/// <summary>
		/// 暴露给上层，通过物理端口发送数据包
		/// </summary>
		/// <param name="PhyPortNo">物理端口号</param>
		/// <param name="buffer">发送内容</param>
		public static Const.EN_RET_CODE SendViaPhyPort(int PhyPortNo, byte[] buffer)
		{
			if (buffer.Length > Const.MAX_PACKET_LENGTH)
				return Const.EN_RET_CODE.EN_RET_PACKET_LENGTH_OVERFOLW;

			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			retVal = PhyPortManager.GetInstance().SendTo(PhyPortNo, buffer);
			return retVal;
		}
	}
}
