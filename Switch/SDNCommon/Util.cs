//将log记录在文件中
//#define LOG_TO_FILE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDNCommon
{
	public static class Util
	{
		public enum EN_LOG_LEVEL{
			EN_LOG_INFO = 0,
			EN_LOG_FATAL
		}

		/// <summary>
		/// 取得当前源码的哪一行
		/// </summary>
		/// <returns></returns>
		public static int GetLineNum()
		{
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
			return st.GetFrame(0).GetFileLineNumber();
		}

		/// <summary>
		/// 取当前源码的源文件名
		/// </summary>
		/// <returns></returns>
		public static string GetCurSourceFileName()
		{
			System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);

			return st.GetFrame(0).GetFileName();

		}

		/// <summary>
		/// 记录log，根据宏记录在控制台或文件中
		/// </summary>
		/// <param name="level">log的等级</param>
		/// <param name="logContent">log的字符串内容</param>
		public static void Log(EN_LOG_LEVEL level, string logContent)
		{
			string logString;
			switch(level)
			{
				case Util.EN_LOG_LEVEL.EN_LOG_FATAL:
					logString = "FATAL: " + logContent;
					break;

				case Util.EN_LOG_LEVEL.EN_LOG_INFO:
					logString = "INFO: " + logContent;
					break;

				default:
					logString = "UNCLASSIFIED: " + logContent;
					break;
			}
#if LOG_TO_FILE
			//TODO
#else
			Console.WriteLine(logString);
#endif
		}

	}
}
