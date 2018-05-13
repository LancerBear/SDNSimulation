using SDNCommon;
using System;
using System.Net;
using System.Text;

namespace Host
{
	public static class HostApp
	{
		/// <summary>
		/// 主机应用初始化
		/// </summary>
		/// <returns></returns>
		public static Const.EN_RET_CODE Init()
		{
			//设置处理控制器消息的回调函数
			DelegateFunc delegateFunc = new DelegateFunc(DealRecPacketInfo);
			Transmitter.SetCallBackFunc(delegateFunc);

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 开始主机业务，死循环读取命令
		/// </summary>
		public static void Start()
		{
			while (true)
			{
				//命令字符串
				string cmdStr = Console.ReadLine();

				Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;

				retVal = DealCommandStr(cmdStr);
				if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
				{
					continue;
				}
			}
		}

		/// <summary>
		/// 处理命令字符串
		/// </summary>
		/// <param name="cmdStr"></param>
		/// <returns></returns>
		private static Const.EN_RET_CODE DealCommandStr(string cmdStr)
		{
			//分割命令
			string[] cmdArry = System.Text.RegularExpressions.Regex.Split(cmdStr, @"[ ]+");
			//foreach (string i in cmdStrArry)
			//{
			//	Console.WriteLine(i);
			//}

			switch(cmdArry[0])
			{
				case "send":
					//命令参数个数不正确
					if (cmdArry.Length != 3)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "命令参数个数不正确");
						return Const.EN_RET_CODE.EN_RET_CMD_PARA_INVALID;
					}

					try
					{
						IPAddress.Parse(cmdArry[1]);
					}
					catch(Exception)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "不是有效的IP地址");
						return Const.EN_RET_CODE.EN_RET_CMD_IP_INVALID;
					}
					PacketHead packetHead = new PacketHead(Program.strCurHostIP, cmdArry[1], PacketHead.EN_PACKET_TYPE.EN_NORMAL_PACKET);
					PacketEntity packetEntity = new PacketEntity(packetHead, cmdArry[2]);
					Transmitter.Send(Util.ObjectToBytes(packetEntity));
					break;

				//命令不正确
				default:
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "无效命令" + "\"" + cmdArry[0] + "\"");
					return Const.EN_RET_CODE.EN_RET_CMD_INVALID;
			}

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}

		/// <summary>
		/// 接收到消息之后的回调函数
		/// </summary>
		/// <param name="packetInfo"></param>
		private static void DealRecPacketInfo(PacketInfo packetInfo)
		{
			PacketEntity packetEntity = (PacketEntity)Util.BytesToObject(packetInfo.GetPacketByte());
			PacketHead head = packetEntity.GetHead();
			string desIP = head.strDesIP;
			string srcIP = head.strSrcIP;
			string content = packetEntity.GetStrContent();

			if (Program.strCurHostIP == desIP)
			{
				Console.WriteLine("收到来自 " + srcIP + "主机的消息: " + content);
			}
			else
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "收到消息目的IP与本机不符" + desIP + " " + Program.strCurHostIP);
			}
		}


	}
}
