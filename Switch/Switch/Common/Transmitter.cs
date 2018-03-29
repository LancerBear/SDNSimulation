using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDNCommon;

namespace Switch
{
	/// <summary>
	/// 暴露给上层的接口类，给上层调用完成数据包的发送和接收
	/// </summary>
	public static class Transmitter
	{
		/// <summary>
		/// 委托函数的一个对象，保存回调函数
		/// </summary>
		private static DelegateFunc delegataFunc = null;

		/// <summary>
		/// 暴露给PhyPortManager，在Listener接收到数据包后调用
		/// </summary>
		/// <param name="buffer">接收数据包内容</param>
		/// <returns></returns>
		public static Const.EN_RET_CODE CallFunc(byte[] buffer, int length, int phyPortNo)
		{
			if (delegataFunc == null)
			{
				return Const.EN_RET_CODE.EN_RET_CALL_BACK_FUNC_NOT_SET;
			}

			//阻塞的方式调用回调函数
			delegataFunc(buffer, length, phyPortNo);
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 暴露给上层，设置接收到数据包后的回调函数
		/// </summary>
		/// <param name="func"></param>
		public static void SetCallBackFunc(DelegateFunc func)
		{
			delegataFunc = func;
		}

		/// <summary>
		/// 暴露给上层，通过物理端口发送数据包
		/// </summary>
		/// <param name="PhyPortNo">物理端口号</param>
		/// <param name="buffer">发送内容</param>
		public static Const.EN_RET_CODE SendViaPhyPort(int PhyPortNo, byte[] buffer)
		{
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			retVal = PhyPortManager.GetInstance().SendTo(PhyPortNo, buffer);
			return retVal;
		}
	}
}
