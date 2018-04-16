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
			//测试
			PacketEntity packet = new PacketEntity(new PacketHead("", "", PacketHead.EN_PACKET_TYPE.EN_PACKET_IN), "");
			PacketInfo packetInfo= new PacketInfo(6, Util.ObjectToBytes(packet));
			DealPakcetIn(packetInfo);
			//测试


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
			byte[] buffer = packetInfo.GetPacketByte();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(buffer);
			PacketHead.EN_PACKET_TYPE packetType = packet.GetHead().enPacketType;
			//Console.WriteLine("从端口" + iPhyPortNo + "收到消息:" + content);
			//Console.WriteLine("SrcIP: " + srcIP + "\tDesIP: " + desIP);
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

			switch(packetType)
			{
				case PacketHead.EN_PACKET_TYPE.EN_SWITCH_ONLINE:
					DealSwitchOnlinePacket(packetInfo);
					break;

				case PacketHead.EN_PACKET_TYPE.EN_PACKET_IN:
					DealPakcetIn(packetInfo);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 处理交换机上线消息
		/// </summary>
		/// <param name=""></param>
		public static void DealSwitchOnlinePacket(PacketInfo packetInfo)
		{
			byte[] packetByte = packetInfo.GetPacketByte();
			int iPhyPortNo = packetInfo.GetPhyPort();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(packetByte);
 			string content = packet.GetStrContent();
			string srcIP = packet.GetHead().strSrcIP;
			string desIP = packet.GetHead().strDesIP;
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "交换机" + iPhyPortNo + "上线");

			//回复上线消息
			PacketHead head = new PacketHead("", "", PacketHead.EN_PACKET_TYPE.EN_ACK_SWITCH_ONLINE);
			PacketEntity ackPakcet = new PacketEntity(head, "");
			retVal = Transmitter.SendViaPhyPort(iPhyPortNo, Util.ObjectToBytes(ackPakcet));
			if (Const.EN_RET_CODE.EN_RET_SUCC != retVal)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机上线ACK发送失败");
			}
		}

		/// <summary>
		/// 处理packet_in消息，计算最短路径后，发送packet_out消息
		/// </summary>
		/// <param name="packetInfo"></param>
		public static void DealPakcetIn(PacketInfo packetInfo)
		{
			byte[] packetByte = packetInfo.GetPacketByte();
			int iPhyPortNo = packetInfo.GetPhyPort();
			PacketEntity packet = (PacketEntity)Util.BytesToObject(packetByte);
			string content = packet.GetStrContent();
			string srcIP = packet.GetHead().strSrcIP;
			string desIP = packet.GetHead().strDesIP;
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

			//Console.WriteLine("switch No : " + iPhyPortNo + "srcIP :" + srcIP + " desIP : " + desIP);

			//源点交换机ID
			int srcSwitchID = iPhyPortNo;

			//目的点交换机ID
			int desSwitchID = GetSwitchIDByIP(desIP);
			//记录到当前点的最短路径数值
			int[] distance = new int[Program.iMaxSwitchID + 1];
			//标记是否在最短路径中
			bool[] isInPath = new bool[Program.iMaxSwitchID + 1];
			//最短路径的前驱点
			int[] preSwitchID = new int[Program.iMaxSwitchID + 1];
			
			//初始化数组
			for (int i = 0; i < Program.iMaxSwitchID + 1; i++)
			{
				//设置标记为false，即未计算出到i的最短路
				isInPath[i] = false;

				//初始化为最大值加一
				distance[i] = Const.MAX_DISTANCE + 1;

				//前驱点设置为无效点
				preSwitchID[i] = Const.INVALID_NUM;
				
				//自己到自己的路径长度设为0
				if (i == srcSwitchID)
				{
					distance[i] = 0;
					isInPath[i] = true;
				}

				//两个交换机之间有相连路径
				if (Program.PathInfoArr[srcSwitchID, i].distance != Const.INVALID_NUM)
				{
					distance[i] = Program.PathInfoArr[srcSwitchID, i].distance;

					//前驱暂时设置为源点
					preSwitchID[i] = srcSwitchID;
				}
			}

			//循环n - 1次
			for (int loop = 0; loop < Program.iMaxSwitchID; loop++)
			{
				int minDis = Const.MAX_DISTANCE + 1;
				int nextSwitchID = Const.INVALID_NUM;

				//找距离最小的点
				for (int i = 0; i < Program.iMaxSwitchID + 1; i++)
				{
					if (!isInPath[i])
					{
						if (distance[i] < minDis)
						{
							minDis = distance[i];
							nextSwitchID = i;
						}
					}
				}

				//Console.WriteLine("next : " + nextSwitchID);
				//将距离最小的点加入路径
				isInPath[nextSwitchID] = true;

				//更新数组
				for (int i = 0; i < Program.iMaxSwitchID + 1; i++)
				{
					if (!isInPath[i] && (Program.PathInfoArr[nextSwitchID, i].distance != Const.INVALID_NUM))
					{
						if (minDis + Program.PathInfoArr[nextSwitchID, i].distance < distance[i])
						{
							distance[i] = minDis + Program.PathInfoArr[nextSwitchID, i].distance;
							preSwitchID[i] = nextSwitchID;
						}
					}
				}

				//如果已经找到目的交换机，则退出循环
				if (nextSwitchID == desSwitchID)
				{
					break;
				}

			}

			//for (int i = 0; i < Program.iMaxSwitchID + 1; i++)
			//{
			//	Console.Write(preSwitchID[i] + "\t");
			//}
			//Console.Write("\n");

			////遍历找到最短路的终点
			//for (int i = 0; i < Program.iMaxSwitchID + 1; i++)
			//{
			//	if (!isInPath[i])
			//		continue;
			//	if (i == srcSwitchID)
			//		continue;

			//	//TODO
			//	int curID = desSwitchID;
			//	int preID = preSwitchID[curID];

			//	Console.WriteLine("Path: ");
			//	while (preID != Const.INVALID_NUM)
			//	{
			//		Console.WriteLine(curID);
			//		curID = preID;
			//		preID = preSwitchID[curID];
			//	}
			//}
			//Console.WriteLine("distance: ");
			//for (int i = 0; i < Program.iMaxSwitchID + 1; i++)
			//{
			//	Console.Write(distance[i] + "\t");
			//}
			//Console.Write("\n");

		}

		/// <summary>
		/// 根据交换机的IP查找ID
		/// </summary>
		/// <param name="strIP"></param>
		/// <returns></returns>
		public static int GetSwitchIDByIP(string strIP)
		{
			//TODO
			return 3;
		}
	}
}
