using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace Switch
{
	class Program
	{
		public const int MAX_SWITCH_NUM = 127;
		public const int MIN_SWITCH_NUM = 0;
		public static int switchID = -1;
		static void Main(string[] args)
		{
			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "系统启动");
			if (0 == args.Length)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "输入参数不存在");
				Environment.Exit(0);
			}
			
			if (!int.TryParse(args[0], out switchID))
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "输入参数格式不正确");
				Environment.Exit(0);
			}

			if (switchID > MAX_SWITCH_NUM || switchID < MIN_SWITCH_NUM)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID过大或过小");
				Environment.Exit(0);
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "交换机ID: " + switchID.ToString());

			RetVal.EN_RET_CODE retVal = RetVal.EN_RET_CODE.EN_RET_INIT;
			
			//进行系统初始化工作，包括建立拓扑等
			retVal = SystemInit();
			if (retVal != RetVal.EN_RET_CODE.EN_RET_SUCC)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "初始化失败");
				Environment.Exit(0);
			}
		}

		/// <summary>
		/// 系统初始化
		/// </summary>
		/// <returns></returns>
		private static RetVal.EN_RET_CODE SystemInit()
		{
			RetVal.EN_RET_CODE retVal = RetVal.EN_RET_CODE.EN_RET_INIT;
			retVal = FileReader.InitFromFile("topology.xml");
			if (RetVal.EN_RET_CODE.EN_RET_SUCC != retVal)
			{
				return retVal;
			}
			return RetVal.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
