using SDNCommon;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Host
{
	/// <summary>
	/// 委托函数类，可以作为参数传入函数，实现函数指针的功能
	/// </summary>
	/// <param name="buffer"></param>
	public delegate void DelegateFunc(PacketInfo packetInfo);

	class Program
	{
		//当前主机的ID 
		public static int iCurHostID = Const.INVALID_NUM;

		//当前主机的IP
		public static string strCurHostIP = "";

		//消息队列
		public static Queue<PacketInfo> PacketQueue = new Queue<PacketInfo>();

		//消息队列互斥锁
		public static Mutex PktQueueMutex = new Mutex();

		static void Main(string[] args)
		{
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "系统启动");
			if (0 == args.Length)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "输入参数不存在");
				Environment.Exit(0);
			}

			if (!int.TryParse(args[0], out iCurHostID))
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "输入参数格式不正确");
				Environment.Exit(0);
			}

			if (iCurHostID > Const.MAX_DEVICE_NUM || iCurHostID < Const.MIN_DEVICE_NUM)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID过大或过小");
				Environment.Exit(0);
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "主机ID: " + iCurHostID.ToString());

			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

			//进行系统初始化工作，包括建立拓扑等
			retVal = SystemInit();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "初始化失败");
				Environment.Exit(0);
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "系统初始化完成");

			//交换机应用初始化
			retVal = HostApp.Init();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "主机应用初始化失败");
				Environment.Exit(0);
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "主机应用初始化完成");

			//开始交换机应用，其中死循环
			HostApp.Start();
		}

		/// <summary>
		/// 系统初始化
		/// </summary>
		/// <returns></returns>
		private static Const.EN_RET_CODE SystemInit()
		{
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

			//建立拓扑
			retVal = FileReader.InitFromFile("..\\..\\..\\topology.xml");
			if (Const.EN_RET_CODE.EN_RET_SUCC != retVal)
			{
				return retVal;
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "初始化拓扑完成");

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
