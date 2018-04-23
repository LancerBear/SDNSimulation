using SDNCommon;

namespace Host
{
	public sealed class PhyPortManager
	{
		private static PhyPortManager instance = null;
		private static readonly object objLock = new object();
		public PhyPort phyPort;
		public Listener[] arrListener = new Listener[Const.MAX_DEVICE_NUM + 1];

		/// <summary>
		/// 单例模式，构造函数声明为私有
		/// </summary>
		private PhyPortManager()
		{
			for (int i = 0; i < Const.MAX_DEVICE_NUM + 1; i++)
			{
				phyPort = null;
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
			if (iPhyPortNo > Const.MAX_DEVICE_NUM || iPhyPortNo < Const.MIN_DEVICE_NUM)
			{
				return Const.EN_RET_CODE.EN_RET_PHY_PORT_OVERFLOW;
			}

			PhyPort p = new PhyPort(iPhyPortNo, iRemotePort, iLocalPort);
			phyPort = p;
			Listener listener = new Listener(p);
			arrListener[iPhyPortNo] = listener;

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}


		public Const.EN_RET_CODE SendTo(int phyPortNo, byte[] buffer)
		{
			if (phyPortNo > Const.MAX_DEVICE_NUM || phyPortNo < Const.MIN_DEVICE_NUM)
				return Const.EN_RET_CODE.EN_RET_PHY_PORT_OVERFLOW;

			if (phyPort == null)
				return Const.EN_RET_CODE.EN_RET_PHY_PORT_NOT_CONNECTED;

			phyPort.SendTo(buffer);

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
