using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Switch
{
	class Test
	{
		public void Init()
		{
			DelegateFunc delegateFunc = new DelegateFunc(OnMsg);
			Transmitter.SetCallBackFunc(delegateFunc);

			if (Program.iCurSwitchID == 0)
			{
				string msg = "Test";
				byte[] buffer = Encoding.Default.GetBytes(msg);
				Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
				while (true)
				{
					Thread.Sleep(1000);
					retVal = Transmitter.SendViaPhyPort(1, buffer);
					retVal = Transmitter.SendViaPhyPort(2, buffer);
					if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "发送失败");
						continue;
					}
					Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "发送成功");
					
				}
			}
		}

		public void OnMsg(byte[] buffer, int length, int phyPortNo)
		{
			string strMsg = Encoding.Default.GetString(buffer, 0, length);
			Console.WriteLine("端口 " + phyPortNo + "传入数据 : " + strMsg);
		}
	}
}
