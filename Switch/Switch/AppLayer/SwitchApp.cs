using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SDNCommon;

namespace Switch
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
			////测试
			//if (Program.iCurSwitchID == 0)
			//{
			//	int temp = 0;
			//	while (++temp < 10)
			//	{
			//		PacketEntity packet = new PacketEntity(new PacketHead("1.1.1.1", "2.2.2.2", PacketHead.EN_PACKET_TYPE.EN_NORMAL_PACKET), "Lancer");
			//		byte[] buffer = Util.ObjectToBytes(packet);
			//		Transmitter.SendViaPhyPort(1, buffer);
			//		Transmitter.SendViaPhyPort(2, buffer);
			//		Transmitter.SendViaPhyPort(0, buffer);
			//		Console.WriteLine("发送完成");
			//		Thread.Sleep(1000);
			//	}
			//}



			PacketInfo packetInfo = null;
			bool firstLoop = true;
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			while (true)
			{
				if (firstLoop)
				{
					//发送初始化完成信息给控制器
					PacketHead head = new PacketHead("", "", PacketHead.EN_PACKET_TYPE.EN_SWITCH_ONLINE);
					PacketEntity packet = new PacketEntity(head, "");
					retVal = Transmitter.SendViaPhyPort(0, Util.ObjectToBytes(packet));
					if (Const.EN_RET_CODE.EN_RET_SUCC != retVal)
					{
						//TODO
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "初始化完成信息发送失败");
					}
					firstLoop = false;
				}

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
		private static void DealReceivePacket(PacketInfo packetInfo)
		{
			int iPhyPortNo = packetInfo.GetPhyPort();
			byte[] buffer = packetInfo.GetPacketByte();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(buffer);
			PacketHead.EN_PACKET_TYPE packetType = packet.GetHead().enPacketType;

			switch (packetType)
			{
				case PacketHead.EN_PACKET_TYPE.EN_NORMAL_PACKET:
					TranNormalPacket(packetInfo);
					break;

				default:
					break;
			}
			
			//Console.WriteLine("从端口" + iPhyPortNo + "收到消息:" + content);
			//Console.WriteLine("SrcIP: " + srcIP + "\tDesIP" + desIP);

		}


		/// <summary>
		/// 阻塞的方式处理控制器的消息，处理完之前无法接受控制器消息，属于监听子线程
		/// </summary>
		/// <param name="packetInfo"></param>
		private static void DealControllerPacket(PacketInfo packetInfo)
		{
			int iPhyPortNo = packetInfo.GetPhyPort();
			byte[] buffer = packetInfo.GetPacketByte();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(buffer);
			string content = packet.GetStrContent();
			string srcIP = packet.GetHead().strSrcIP;
			string desIP = packet.GetHead().strDesIP;
			PacketHead.EN_PACKET_TYPE packetType = packet.GetHead().enPacketType;

			switch (packetType)
			{
				case PacketHead.EN_PACKET_TYPE.EN_ACK_SWITCH_ONLINE:
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "控制器上线");
					break;

				//收到控制器下发的流表
				case PacketHead.EN_PACKET_TYPE.EN_PACKET_OUT_WITH_FLOW_ITEM:
					DealPacketOut(packetInfo);
					break;

				case PacketHead.EN_PACKET_TYPE.EN_PACKET_OUT_DUMP:
					//TODO
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 转发常规数据包
		/// </summary>
		/// <param name="packetInfo"></param>
		/// <returns></returns>
		private static void TranNormalPacket(PacketInfo packetInfo)
		{
			byte[] packetByte = packetInfo.GetPacketByte();
			PacketEntity packetEntity = (PacketEntity)Util.BytesToObject(packetByte);
			string srcIP = packetEntity.GetHead().strSrcIP;
			string desIP = packetEntity.GetHead().strDesIP;

			Console.WriteLine("收到目的IP是" + desIP + "的数据包");

			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			int tranPort = Const.INVALID_NUM;

			//流表中存在转发选项，直接转发
			if (FlowTable.GetInstance().TryGetItem(desIP, out tranPort))
			{
				Console.WriteLine("匹配到流表项， 向端口" + tranPort + "转发");

				retVal = Transmitter.SendViaPhyPort(tranPort, packetByte);
				if (Const.EN_RET_CODE.EN_RET_SUCC != retVal)
				{
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "数据包转发失败");
				}
			}
			//流表中不存在转发选项，将数据包暂存缓冲区，上报控制器
			else
			{
				Console.WriteLine("未匹配到流表项，放入缓冲队列，向控制器发送流表请求数据包packet_in");
				BufQueItem item = new BufQueItem(packetInfo, desIP);

				//存入缓冲队列
				Program.BufferQueueMutex.WaitOne();
				Program.BufferQueue.Enqueue(item);
				Program.BufferQueueMutex.ReleaseMutex();

				PacketHead head = new PacketHead(srcIP, desIP, PacketHead.EN_PACKET_TYPE.EN_PACKET_IN);
				PacketEntity packetIn = new PacketEntity(head, "");
				retVal = Transmitter.SendViaPhyPort(0, Util.ObjectToBytes(packetIn));
				if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
				{
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "packet_in发送失败");
				}
			}
		}

		/// <summary>
		/// 处理控制器下发的Pakcet_Out消息，监听子线程调用
		/// </summary>
		/// <param name="packetInfo"></param>
		private static void DealPacketOut(PacketInfo packetInfo)
		{
			//Console.WriteLine("packet_out");
			byte[] buffer = packetInfo.GetPacketByte();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(buffer);
			byte[] FlowBuffer = packet.GetByteContent();
			Dictionary<string, int> dictionary = (Dictionary<string, int>)Util.BytesToObject(FlowBuffer);

			Console.WriteLine("收到控制器下发的流表项:");
			for (int i = 0; i < dictionary.Count; i++)
			{
				Console.WriteLine("目的IP " + dictionary.ElementAt(i).Key + " 转发端口 " + dictionary.ElementAt(i).Value);
				FlowTableItem fItem = new FlowTableItem(dictionary.ElementAt(i).Key, dictionary.ElementAt(i).Value);
				bool boolRes = FlowTable.GetInstance().AddItem(fItem);
				if (boolRes != true)
				{
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "加入流表项失败！");
				}
			}
			//打印当前所有的流表项
			//FlowTable.PrintItems();

			//刷新缓冲队列
			RefreshBufferQeueu(dictionary);
		}

		/// <summary>
		/// 刷新缓冲队列，将队列中能匹配到的数据包转发出去
		/// </summary>
		private static void RefreshBufferQeueu(Dictionary<string, int> dictionary)
		{
			if (dictionary.Count == 0)
			{
				return;
			}

			BufQueItem bufQueItem = null;

			//记录上一次匹配到的dictionary中的下标
			int indexOfDic = 0;

			//P操作
			Program.BufferQueueMutex.WaitOne();

			while (Program.BufferQueue.Count != 0)
			{
				bufQueItem = Program.BufferQueue.Dequeue();
				if (bufQueItem == null)
				{
					break;
				}

				DateTime curTime = DateTime.Now;
				TimeSpan ts = curTime.Subtract(bufQueItem.EnQueueTime).Duration();
				Double douInterval = ts.TotalSeconds;
				
				//如果大于保存时间，则将数据包丢弃
				if (douInterval > Const.MAX_BUF_TIME)
				{
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "丢弃数据包");
					continue;
				}

				string strDesIP = bufQueItem.strDesIP;

				//由于数据包目的地址的连续性，一般缓冲区中的下一个数据包目标地址和上一个一样，为了避免重新搜索，记录上一个匹配到的字典下标
				if (dictionary.ElementAt(indexOfDic).Key == strDesIP)
				{
					int port = dictionary.ElementAt(indexOfDic).Value;
					Transmitter.SendViaPhyPort(port, bufQueItem.packetInfo.GetPacketByte());
				}
				else
				{
					//记录是否匹配到字典中的流表项
					bool findInDic = false;

					for (int i = 0; i < dictionary.Count; i++)
					{
						if (dictionary.ElementAt(i).Key == strDesIP)
						{
							findInDic = true;
							indexOfDic = i;
							int port = dictionary.ElementAt(i).Value;

							Console.WriteLine("从缓冲队列取出目的IP是" + strDesIP + "的数据包，向端口" + port + "转发");

							Transmitter.SendViaPhyPort(port, bufQueItem.packetInfo.GetPacketByte());
						}
					}

					//没有匹配到字典中的流表项,重新放入队列
					if (findInDic == false)
					{
						Program.BufferQueue.Enqueue(bufQueItem);
					}
				}

			}

			Program.BufferQueueMutex.ReleaseMutex();
		}
	}
}
