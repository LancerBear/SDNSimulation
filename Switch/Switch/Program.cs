using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Switch
{
	/// <summary>
	/// 委托函数类，可以作为参数传入函数，实现函数指针的功能
	/// </summary>
	/// <param name="buffer"></param>
	public delegate void DelegateFunc(byte[] buffer, int length, int phyPortNo);

	class Program
	{
		//当前交换机的ID
		public static int iCurSwitchID = -1;

		static void Main(string[] args)
		{
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "系统启动");
			if (0 == args.Length)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "输入参数不存在");
				Environment.Exit(0);
			}
			
			if (!int.TryParse(args[0], out iCurSwitchID))
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "输入参数格式不正确");
				Environment.Exit(0);
			}

			if (iCurSwitchID > Const.MAX_SWITCH_NUM || iCurSwitchID < Const.MIN_SWITCH_NUM)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID过大或过小");
				Environment.Exit(0);
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "交换机ID: " + iCurSwitchID.ToString());

			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			
			//进行系统初始化工作，包括建立拓扑等
			retVal = SystemInit();
			if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "初始化失败");
				Environment.Exit(0);
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "系统初始化完成");
		}

		/// <summary>
		/// 系统初始化
		/// </summary>
		/// <returns></returns>
		private static Const.EN_RET_CODE SystemInit()
		{
			Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
			
			//建立拓扑
			retVal = FileReader.InitFromFile("topology.xml");
			if (Const.EN_RET_CODE.EN_RET_SUCC != retVal)
			{
				return retVal;
			}
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "初始化拓扑完成");

			Test test = new Test();
			test.Init();

			//TODO SDN模块启动
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
