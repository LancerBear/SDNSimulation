using SDNCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
	public static class ControllerApp
	{
		public static Const.EN_RET_CODE Init()
		{
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// SDN业务开始，函数内死循环，读消息队列后处理
		/// </summary>
		public static void StartApp()
		{
			PacketInfo packet = null;
			while (true)
			{
				packet = null;

				//P操作
				Program.PktQueueMutex.WaitOne();
				//消息队列为空，进行V操作之后继续循环读队列
				if (Program.PacketQueue.Count == 0)
				{
					Program.PktQueueMutex.ReleaseMutex();
					continue;
				}
				//消息队列不为空，消息包出队后进行V操作
				packet = Program.PacketQueue.Dequeue();
				Program.PktQueueMutex.ReleaseMutex();

				
			}
		}


		public static void DealReceivePacket()
		{

		}
	}
}
