using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Switch
{
	public class Listener
	{
		private Thread listenerThread;
		private Socket listenerSocket;
		private EndPoint epRemotePoint;

		/// <summary>
		/// 构造函数，构造一个Listener对象，并新建线程进行监听
		/// </summary>
		/// <param name="pyhPort"></param>
		public Listener(PhyPort pyhPort)
		{
			//this.listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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

			while (true)
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
				}catch(SocketException ex)
				{
					Console.WriteLine("SocketException" + ex.ErrorCode);
					continue;
				}
				//TODO 
				//调用处理函数
				PhyPortManager.GetInstance().HandleReceive(buffer, length);
			}

		}
	}
}
