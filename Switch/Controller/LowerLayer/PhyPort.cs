using SDNCommon;
using System;
using System.Net;
using System.Net.Sockets;

namespace Controller
{
	public class PhyPort
	{
		public bool connected = false;
		public int iRemotePort;
		private int iLocalPort;
		public int PhyPortNo;
		public Socket socket;
		public IPEndPoint ipepLocalPoint;
		public EndPoint epRemptePoint;

		/// <summary>
		/// 构造函数，初始化一个模拟物理端口，初始化Socket
		/// </summary>
		/// <param name="PyhPortNo"></param>
		/// <param name="iRemotePort"></param>
		/// <param name="iLocalPort"></param>
		public PhyPort(int PyhPortNo, int iRemotePort, int iLocalPort)
		{
			this.connected = true;
			this.PhyPortNo = PyhPortNo;
			this.iRemotePort = iRemotePort;
			this.iLocalPort = iLocalPort;

			//本地ip端口
			this.ipepLocalPoint = new IPEndPoint(Const.ipAddress, iLocalPort);
			
			//通过udp协议通信
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			//监听的ip端口
			this.epRemptePoint = (EndPoint)new IPEndPoint(Const.ipAddress, iRemotePort);

			//绑定本地ip端口
			try
			{
				this.socket.Bind(this.ipepLocalPoint);
			}catch (Exception)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "端口" + iLocalPort.ToString() + "被占用");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// 给PhyPortManager调用,发送数据包
		/// </summary>
		/// <param name="buffer">发送内容</param>
		/// <returns></returns>
		public Const.EN_RET_CODE SendTo(byte[] buffer)
		{
			int sendBytes = this.socket.SendTo(buffer, buffer.Length, SocketFlags.None, this.epRemptePoint);
			if (sendBytes == 0)
				return Const.EN_RET_CODE.EN_RET_ERR;
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
