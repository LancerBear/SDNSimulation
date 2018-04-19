using SDNCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Controller
{
	class Program
	{
		//邻接矩阵
		public static SinglePathInfo[,] PathInfoArr = new SinglePathInfo[Const.MAX_SWITCH_NUM + 1, Const.MAX_SWITCH_NUM + 1];

		//消息队列
		public static Queue<PacketInfo> PacketQueue = new Queue<PacketInfo>();

		//消息队列互斥锁
		public static Mutex PktQueueMutex = new Mutex();

		//当前拓扑结构中最大交换机ID
		public static int iMaxSwitchID = Const.INVALID_NUM;

		//交换机ID到IP的映射
		public static Dictionary<int, string> IDtoIP = new Dictionary<int, string>();

		//交换机IP到ID的映射
		public static Dictionary<string, int> IPtoID = new Dictionary<string, int>();


		static void Main(string[] args)
		{
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "控制器启动");

			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

			//进行系统初始化
			retVal = SystemInit();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "系统初始化失败！");
				Environment.Exit(0);
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "系统初始化完成");

			//进行控制器应用的初始化
			retVal = ControllerApp.Init();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "控制器初始化失败");
				Environment.Exit(0);
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "控制器初始化完成");

			//开始控制器应用
			ControllerApp.StartApp();
		}

		/// <summary>
		/// 控制器系统初始化
		/// </summary>
		private static Const.EN_RET_CODE SystemInit()
		{
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			
			//初始化邻接矩阵
			retVal = InitPathInfoArr();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
				return retVal;

			//初始化IP
			retVal = InitIPArry();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
				return retVal;

			//清空消息队列
			PacketQueue.Clear();
			
			//从文件读取拓扑信息
			retVal = FileReader.InitFromFile("..\\..\\..\\topology.xml");
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
				return retVal;

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 初始化邻接矩阵，将引用实例化，初始化PathInfo类中的变量设为-1
		/// </summary>
		/// <returns></returns>
		private static Const.EN_RET_CODE InitPathInfoArr()
		{
			for (int i = 0; i < PathInfoArr.GetLength(0); i++)
			{
				for (int j = 0; j < PathInfoArr.GetLength(1); j++)
				{
					PathInfoArr[i, j] = new SinglePathInfo();
					PathInfoArr[i, j].distance = Const.INVALID_NUM;
					PathInfoArr[i, j].phyPortNo = Const.INVALID_NUM;
				}
			}
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 初始化IP数组
		/// </summary>
		/// <returns></returns>
		private static Const.EN_RET_CODE InitIPArry()
		{
			IDtoIP.Clear();
			IPtoID.Clear();
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 打印矩阵信息
		/// </summary>
		public static void ShowPathInfo()
		{
			for (int i = 0; i < iMaxSwitchID + 1; i++)
			{
				Console.Write(i + "\t");
			}
			Console.Write("\n");
			for (int i = 0; i < iMaxSwitchID + 1; i++)
			{
				for (int j = 0; j < iMaxSwitchID + 1; j++)
				{
					Console.Write(PathInfoArr[i, j].distance + "\t");
				}
				Console.Write("\n\n");
			}
		}
	}
}
