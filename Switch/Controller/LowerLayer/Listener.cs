using SDNCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Controller
{
	public class Listener
	{
		private int listenPort;
		private int listenerNo;
		private Thread listenerThread;
		private Socket listenerSocket;
		private EndPoint epRemotePoint;

		/// <summary>
		/// 构造函数，构造一个Listener对象，并新建线程进行监听
		/// </summary>
		/// <param name="pyhPort"></param>
		public Listener(PhyPort pyhPort)
		{
			this.listenPort = pyhPort.iRemotePort;
			this.listenerNo = pyhPort.PhyPortNo;
			this.listenerSocket = pyhPort.socket;
			this.epRemotePoint = new IPEndPoint(IPAddress.Any, 0);

			this.listenerThread = new Thread(ListenerHandleFunc);
			this.listenerThread.Start();
		}

		/// <summary>
		/// 监听socket函数，调用
		/// </summary>
		private void ListenerHandleFunc()
		{
			byte[] buffer = new byte[Const.MAX_PACKET_LENGTH];

			while(true)
			{
				if (this.listenerSocket == null || this.listenerSocket.Available < 1)
				{
					Thread.Sleep(20);
					continue;
				}

				int length = 0;
				try
				{
					length = this.listenerSocket.ReceiveFrom(buffer, ref this.epRemotePoint);
				}
				catch (SocketException)
				{
					//在SendTo的目的端口没有Socket在监听，或原有的监听Socket被关闭时，这里监听会抛出10054异常
					//Console.WriteLine("SocketException" + ex.ErrorCode);
					continue;
				}

				//如果接收到数据的是从监听端口发出的，将消息写入消息队列
				if (((IPEndPoint)this.epRemotePoint).Port == this.listenPort)
				{
					PacketEntity pakcetEntity = (PacketEntity)Util.BytesToObject(buffer);
					PacketInfo packetInfo = new PacketInfo(this.listenPort, pakcetEntity);

					//P操作
					Program.PktQueueMutex.WaitOne();
					//写队列
					Program.PacketQueue.Enqueue(packetInfo);
					//V操作
					Program.PktQueueMutex.ReleaseMutex();
				}
			}

		}
	}
}
