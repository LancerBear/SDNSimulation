using SDNCommon;
using System;
using System.Net;
using System.Xml;

namespace Host
{
	class FileReader
	{
		public static Const.EN_RET_CODE InitFromFile(string fileName)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(fileName);
			}
			catch (Exception)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, fileName + "拓扑结构文件不存在！");
				return Const.EN_RET_CODE.EN_RET_FILE_NOT_EXIST;
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "读取拓扑文件");

			try
			{
				XmlElement root = doc.DocumentElement;

				//遍历所有Host
				XmlNodeList hostList = root.GetElementsByTagName("Host");

				foreach (XmlNode hostNode in hostList)
				{
					string strHostID = "";
					string strHostIP = "";
					string strHostPort = "";
					string strSwitchPort = "";

					int iHostID = Const.INVALID_NUM;
					int iHostPort = Const.INVALID_NUM;
					int iSwitchPort = Const.INVALID_NUM;

					strHostID = ((XmlElement)hostNode).GetElementsByTagName("HostID")[0].InnerText;
					strHostIP = ((XmlElement)hostNode).GetElementsByTagName("HostIP")[0].InnerText;

					bool boolTryParseResult = false;

					boolTryParseResult = int.TryParse(strHostID, out iHostID);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "主机ID转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					if (iHostID != Program.iCurHostID)
					{
						continue;
					}

					try
					{
						IPAddress.Parse(strHostIP);
					}
					catch(System.FormatException)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "IP地址不合法");
						return Const.EN_RET_CODE.EN_RET_ERR;
					}

					Program.strCurHostIP = strHostIP;

					strHostPort = ((XmlElement)hostNode).GetElementsByTagName("HostPort")[0].InnerText;
					strSwitchPort = ((XmlElement)hostNode).GetElementsByTagName("switchPort")[0].InnerText;

					boolTryParseResult = int.TryParse(strHostPort, out iHostPort);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "主机端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strSwitchPort, out iSwitchPort);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
					//添加物理端口
					retVal = PhyPortManager.GetInstance().AddPort(0, iSwitchPort, iHostPort);
					if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "添加物理端口失败");
					}
				}
			}
			catch (XmlException)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "Xml格式错误");
				return Const.EN_RET_CODE.EN_XML_FILE_FORMAT_ERR;
			}

			return Const.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
