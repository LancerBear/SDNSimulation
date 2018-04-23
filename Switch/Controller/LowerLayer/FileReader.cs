using SDNCommon;
using System;
using System.Xml;

namespace Controller
{
	public static class FileReader
	{
		/// <summary>
		/// 从文件中读取拓扑信息，记录到邻接矩阵，并完成与各个交换机的连接
		/// </summary>
		/// <param name="fileName">拓扑文件的路径和文件名</param>
		/// <returns></returns>
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

				bool boolTryParseResult = false;

				//遍历所有pair，记录邻接矩阵
				XmlNodeList pairList = root.GetElementsByTagName("pair");
				foreach (XmlNode PairNode in pairList)
				{
					boolTryParseResult = false;

					string strID0;
					string strID1;
					string strDistance;
					string strPhyPort0;
					string strPhyPort1;

					int iID0;
					int iID1;
					int iDistance;
					int iPhyPort0;
					int iPhyPort1;

					strDistance = ((XmlElement)PairNode).GetElementsByTagName("distance")[0].InnerText;

					XmlNodeList switchList = ((XmlElement)PairNode).GetElementsByTagName("switch");

					strID0 = ((XmlElement)switchList[0]).GetElementsByTagName("id")[0].InnerText;
					strPhyPort0 = ((XmlElement)switchList[0]).GetElementsByTagName("PhyPortNo")[0].InnerText;

					strID1 = ((XmlElement)switchList[1]).GetElementsByTagName("id")[0].InnerText;
					strPhyPort1 = ((XmlElement)switchList[1]).GetElementsByTagName("PhyPortNo")[0].InnerText;

					boolTryParseResult = int.TryParse(strDistance, out iDistance);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "距离转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strID0, out iID0);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strPhyPort0, out iPhyPort0);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机物理接口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strID1, out iID1);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strPhyPort1, out iPhyPort1);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机物理接口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					//距离不在范围内
					if (iDistance < Const.MIN_DISTANCE || iDistance > Const.MAX_DISTANCE)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "距离数据不在范围内");
						return Const.EN_RET_CODE.EN_RET_DISTAANCE_OVERFLOW;
					}

					//交换机ID不在范围内
					if (iID0 < Const.MIN_DEVICE_NUM || iID0 > Const.MAX_DEVICE_NUM
						|| iID1 < Const.MIN_DEVICE_NUM || iID1 > Const.MAX_DEVICE_NUM)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID不在范围内");
						return Const.EN_RET_CODE.EN_RET_SWITCH_ID_OVERFLOW;
					}

					//物理端口号不在范围内
					if (iPhyPort0 < Const.MIN_PHY_PORT_NUM || iPhyPort0 > Const.MAX_PHY_PORT_NUM
						|| iPhyPort1 < Const.MIN_PHY_PORT_NUM || iPhyPort1 > Const.MAX_PHY_PORT_NUM)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "物理端口号不在范围内");
						return Const.EN_RET_CODE.EN_RET_PHY_PORT_OVERFLOW;
					}

					//记录到邻接矩阵中
					Program.PathInfoArr[iID0, iID1].distance = iDistance;
					Program.PathInfoArr[iID0, iID1].phyPortNo = iPhyPort0;

					Program.PathInfoArr[iID1, iID0].distance = iDistance;
					Program.PathInfoArr[iID1, iID0].phyPortNo = iPhyPort1;

					//Console.WriteLine("id0:{0}, port0:{1}, id1:{2}, port1:{3}, distance:{4}", iID0, iPhyPort0, iID1, iPhyPort1, iDistance);
				}

				//遍历controller标签
				XmlNodeList controllerList = root.GetElementsByTagName("controller");
				foreach (XmlNode ctrlNode in controllerList)
				{
					boolTryParseResult = false;

					string strSwitchID;
					string strSwitchPort;
					string strControllerPort;
					string strSwitchIP;

					int iSwitchID = Const.INVALID_NUM;
					int iSwitchPort = Const.INVALID_NUM;
					int iControllerPort = Const.INVALID_NUM;

					strSwitchID = ((XmlElement)ctrlNode).GetElementsByTagName("switchID")[0].InnerText;
					strSwitchIP = ((XmlElement)ctrlNode).GetElementsByTagName("switchIP")[0].InnerText;
					strSwitchPort = ((XmlElement)ctrlNode).GetElementsByTagName("switchPort")[0].InnerText;
					strControllerPort = ((XmlElement)ctrlNode).GetElementsByTagName("controllerPort")[0].InnerText;

					boolTryParseResult = int.TryParse(strSwitchID, out iSwitchID);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strSwitchPort, out iSwitchPort);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strControllerPort, out iControllerPort);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "控制器端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					//交换机ID不在范围内
					if (iSwitchID > Const.MAX_DEVICE_NUM || iSwitchID < Const.MIN_DEVICE_NUM)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID不在范围内");
						return Const.EN_RET_CODE.EN_RET_SWITCH_ID_OVERFLOW;
					}

					//交换机或控制器端口号不在范围内
					if (iSwitchPort > Const.MAX_PORT_NUM || iSwitchPort < Const.MIN_PORT_NUM
						|| iControllerPort > Const.MAX_PORT_NUM || iControllerPort < Const.MIN_PORT_NUM)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机或控制器端口号不在范围内");
						return Const.EN_RET_CODE.EN_RET_PORT_OVERFLOW;
					}

					Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
					//将交换机ID作为控制器的物理端口号
					retVal = PhyPortManager.GetInstance().AddPort(iSwitchID, iSwitchPort, iControllerPort);
					if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "添加物理端口失败");
					}

					//记录最大交换机ID
					if (iSwitchID > Program.iMaxDeviceID)
					{
						Program.iMaxDeviceID = iSwitchID;
					}

					//记录IP
					Program.IDtoIP[iSwitchID] = strSwitchIP;
					Program.IPtoID[strSwitchIP] = iSwitchID;
				}

				//遍历Host标签
				XmlNodeList hostList = root.GetElementsByTagName("Host");
				foreach (XmlNode hostNode in hostList)
				{
					boolTryParseResult = false;

					string strHostID;
					string strSwitchID;
					string strDistance;
					string strHostIP;
					string strSwitchPhyPortNum;

					int iHostID = Const.INVALID_NUM;
					int iSwitchID = Const.INVALID_NUM;
					int iDistance = Const.INVALID_NUM;
					int iSwitchPhyPortNum = Const.INVALID_NUM;

					strHostIP = ((XmlElement)hostNode).GetElementsByTagName("HostIP")[0].InnerText;
					strHostID = ((XmlElement)hostNode).GetElementsByTagName("HostID")[0].InnerText;
					strSwitchID = ((XmlElement)hostNode).GetElementsByTagName("switchID")[0].InnerText;
					strDistance = ((XmlElement)hostNode).GetElementsByTagName("distance")[0].InnerText;
					strSwitchPhyPortNum = ((XmlElement)hostNode).GetElementsByTagName("PhyPortNo")[0].InnerText;

					boolTryParseResult = int.TryParse(strHostID, out iHostID);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "主机ID转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strSwitchID, out iSwitchID);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机ID转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strDistance, out iDistance);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "距离转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strSwitchPhyPortNum, out iSwitchPhyPortNum);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "距离转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					//记录最大交换机ID
					if (iHostID > Program.iMaxDeviceID)
					{
						Program.iMaxDeviceID = iHostID;
					}

					//记录IP
					Program.IDtoIP[iHostID] = strHostIP;
					Program.IPtoID[strHostIP] = iHostID;

					//记录路径到邻接矩阵
					Program.PathInfoArr[iSwitchID, iHostID].distance = iDistance;
					Program.PathInfoArr[iSwitchID, iHostID].phyPortNo = iSwitchPhyPortNum;

					Program.PathInfoArr[iHostID, iSwitchID].distance = iDistance;
					//主机的物理端口默认为0
					Program.PathInfoArr[iHostID, iSwitchID].phyPortNo = 0;
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
