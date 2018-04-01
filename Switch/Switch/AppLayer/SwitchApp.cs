using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDNCommon;

namespace Switch.AppLayer
{
	public static class SwitchApp
	{
		/// <summary>
		/// 交换机应用初始化
		/// </summary>
		/// <returns></returns>
		public static Const.EN_RET_CODE Init()
		{
			//设置处理控制器消息的回调函数
			DelegateFunc delegateFunc = new DelegateFunc(DealControllerPacket);
			Transmitter.SetCallBackFunc(delegateFunc);
			
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 交换机程序启动
		/// </summary>
		public static void Start()
		{
			//测试
			if (Program.iCurSwitchID == 0)
			{
				int temp = 0;
				while (++temp < 10)
				{
					PacketEntity packet = new PacketEntity(new PacketHead("1.1.1.1", "2.2.2.2"), "Lancer");
					byte[] buffer = Util.ObjectToBytes(packet);
					Const.EN_RET_CODE retVal = Transmitter.SendViaPhyPort(1, buffer);
					Transmitter.SendViaPhyPort(2, buffer);
					Transmitter.SendViaPhyPort(0, buffer);
					if (retVal == Const.EN_RET_CODE.EN_RET_PACKET_LENGTH_OVERFOLW)
					{

					}
					Console.WriteLine("发送完成");
					Thread.Sleep(1000);
				}
			}


			PacketInfo packetInfo = null;
			while (true)
			{
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
		/// 处理消息队列中的消息
		/// </summary>
		/// <param name="packetInfo">待处理的消息包</param>
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


		public static void DealControllerPacket(PacketInfo packetInfo)
		{

		}
	}
}
