﻿using SDNCommon;
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
		public static Queue<Packet> PacketQueue = new Queue<Packet>();

		//消息队列互斥锁
		public static Mutex PktQueueMutex = new Mutex();

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
	}
}