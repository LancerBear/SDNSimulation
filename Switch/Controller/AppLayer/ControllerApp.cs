using SDNCommon;
using System;

namespace Controller
{
	public static class ControllerApp
	{
		/// <summary>
		/// SDN业务初始化
		/// </summary>
		/// <returns></returns>
		public static Const.EN_RET_CODE Init()
		{
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// SDN业务开始，函数内死循环，读消息队列后处理
		/// </summary>
		public static void StartApp()
		{
			PacketInfo packetInfo = null;
			while (true)
			{

				//测试
				PacketEntity packetEntity = new PacketEntity(new PacketHead("3.3.3.3", "4.4.4.4"), "从控制器发出的消息");
				Transmitter.SendViaPhyPort(0, Util.ObjectToBytes(packetEntity));
				//测试

				packetInfo = null;

				//P操作
				Program.PktQueueMutex.WaitOne();
				//消息队列为空，进行V操作之后继续循环读队列
				if (Program.PacketQueue.Count == 0)
				{
					Program.PktQueueMutex.ReleaseMutex();
					continue;
				}
				//消息队列不为空，消息包出队后进行V操作
				packetInfo = Program.PacketQueue.Dequeue();
				Program.PktQueueMutex.ReleaseMutex();

				//处理接收到的消息
				DealReceivePacket(packetInfo);

			}
		}

		/// <summary>
		/// 处理接收到的消息的函数
		/// </summary>
		/// <param name="packetInfo">待处理的消息</param>
		public static void DealReceivePacket(PacketInfo packetInfo)
		{
			int iPhyPortNo = packetInfo.GetPhyPort();
			byte[] buffer = packetInfo.GetPacketByte();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(buffer);
			string content = packet.GetContent();
			string srcIP = packet.GetHead().strSrcIP;
			string desIP = packet.GetHead().strDesIP;
			Console.WriteLine("从端口" + iPhyPortNo + "收到消息:" + content);
			Console.WriteLine("SrcIP: " + srcIP + "\tDesIP" + desIP);
		}
	}
}
