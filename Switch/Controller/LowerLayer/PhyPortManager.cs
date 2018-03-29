using SDNCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
	public sealed class PhyPortManager
	{
		private static PhyPortManager instance = null;
		private static readonly object objLock = new object();
		public PhyPort[] arrPhyPort = new PhyPort[Const.MAX_SWITCH_NUM + 1];
		public Listener[] arrListener = new Listener[Const.MAX_SWITCH_NUM + 1];

		/// <summary>
		/// 单例模式，构造函数声明为私有
		/// </summary>
		private PhyPortManager()
		{
			for (int i = 0; i < Const.MAX_SWITCH_NUM + 1; i++)
			{
				arrPhyPort[i] = null;
				arrListener[i] = null;
			}
		}

		/// <summary>
		/// 获取单例
		/// </summary>
		/// <returns></returns>
		public static PhyPortManager GetInstance()
		{
			if (null == instance)
			{
				lock (objLock)
				{
					//解锁后可能其他线程已经新建了一个实例，所以需要重新判断
					if (null == instance)
					{
						instance = new PhyPortManager();
					}
				}
			}
			return instance;
		}

		/// <summary>
		/// 新增一个模拟物理端口，并为其绑定一个Listener
		/// </summary>
		/// <param name="iPhyPortNo">物理端口号，大于0小于4</param>
		/// <param name="iRemotePort">监听端口</param>
		/// <param name="iLocalPort">发送端口</param>
		/// <returns></returns>
		public Const.EN_RET_CODE AddPort(int iPhyPortNo, int iRemotePort, int iLocalPort)
		{
			if (iPhyPortNo > Const.MAX_SWITCH_NUM || iPhyPortNo < Const.MIN_SWITCH_NUM)
			{
				return Const.EN_RET_CODE.EN_RET_PHY_PORT_OVERFLOW;
			}

			PhyPort p = new PhyPort(iPhyPortNo, iRemotePort, iLocalPort);
			arrPhyPort[iPhyPortNo] = p;
			Listener listener = new Listener(p);
			arrListener[iPhyPortNo] = listener;

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}


		public Const.EN_RET_CODE SendTo(int phyPortNo, byte[] buffer)
		{
			if (phyPortNo > Const.MAX_SWITCH_NUM || phyPortNo < Const.MIN_SWITCH_NUM)
				return Const.EN_RET_CODE.EN_RET_PHY_PORT_OVERFLOW;

			if (arrPhyPort[phyPortNo] == null)
				return Const.EN_RET_CODE.EN_RET_PHY_PORT_NOT_CONNECTED;

			arrPhyPort[phyPortNo].SendTo(buffer);

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// Listener接收到socket包后调用
		/// </summary>
		//public void HandleReceive(byte[] buffer, int length, int phyPortNo)
		//{
		//	Transmitter.CallFunc(buffer, length, phyPortNo);
		//}
	}
}
