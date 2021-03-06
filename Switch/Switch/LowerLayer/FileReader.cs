﻿using SDNCommon;
using System;
using System.Xml;

namespace Switch
{
	public static class FileReader
	{
		/// <summary>
		/// 从文件中初始化拓扑结构
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static Const.EN_RET_CODE InitFromFile(string fileName)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(fileName);
			}catch(Exception)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, fileName + "拓扑结构文件不存在！");
				return Const.EN_RET_CODE.EN_RET_FILE_NOT_EXIST;
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "读取拓扑文件");
	

			//记录当前交换机的ID在pair中的下标
			int selfIndexInPair = Const.INVALID_NUM;
			try
			{
				XmlElement root = doc.DocumentElement;

				//遍历所有pair
				XmlNodeList pairList = root.GetElementsByTagName("pair");
				
				foreach (XmlNode PairNode in pairList)
				{
					string strID0;
					string strID1;
					string strPhyPortNo;
					string strLocalPortNum;
					string strRemotePortNum;

					int iPhyPortNo = Const.INVALID_NUM;
					int iLocalPortNum = Const.INVALID_NUM;
					int iRemotePortNum = Const.INVALID_NUM;

					XmlNodeList switchList = ((XmlElement)PairNode).GetElementsByTagName("switch");
					strID0 = ((XmlElement)switchList[0]).GetElementsByTagName("id")[0].InnerText;
					strID1 = ((XmlElement)switchList[1]).GetElementsByTagName("id")[0].InnerText;
					
					//检测pair 中是否有某个交换机ID和当前交换机的ID相同
					if (strID0 == Program.iCurSwitchID.ToString())
					{
						selfIndexInPair = 0;
					}
					else if (strID1 == Program.iCurSwitchID.ToString())
					{
						selfIndexInPair = 1;
					} 
					else
					{
						selfIndexInPair = Const.INVALID_NUM;
						continue;
					}
					
					//pair 中某个交换机ID和当前交换机的ID相同
					if (selfIndexInPair != Const.INVALID_NUM)
					{
						strPhyPortNo = ((XmlElement)switchList[selfIndexInPair]).GetElementsByTagName("PhyPortNo")[0].InnerText;
						strLocalPortNum = ((XmlElement)switchList[selfIndexInPair]).GetElementsByTagName("LocalPortNum")[0].InnerText;
						strRemotePortNum = ((XmlElement)switchList[(selfIndexInPair + 1) % 2]).GetElementsByTagName("LocalPortNum")[0].InnerText;
						
						//String转int失败，说明xml数据格式错误，直接抛出异常
						if (!int.TryParse(strPhyPortNo, out iPhyPortNo) || !int.TryParse(strLocalPortNum, out iLocalPortNum)
							|| !int.TryParse(strRemotePortNum, out iRemotePortNum))
						{
							Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "string转int失败");
							throw new Exception("xml格式错误");
						}

						//转换后数值不在规定大小内，同样视为格式不正确，直接抛出异常
						if (iPhyPortNo > Const.MAX_PHY_PORT_NUM || iPhyPortNo < Const.MIN_PHY_PORT_NUM
							|| iLocalPortNum > Const.MAX_PORT_NUM || iLocalPortNum < Const.MIN_PORT_NUM
							|| iRemotePortNum > Const.MAX_PORT_NUM || iRemotePortNum < Const.MIN_PORT_NUM)
						{
							Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "值不在范围内");
							throw new Exception("xml格式错误");
						}
						
						//增加物理端口
						Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
						retVal = PhyPortManager.GetInstance().AddPort(iPhyPortNo, iRemotePortNum, iLocalPortNum);
						if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
						{
							//TODO
							Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "增加物理端口失败");
							return retVal;
						}
					}
				}

				//string转int成功标志
				bool boolTryParseResult = false;

				//遍历controller标签
				XmlNodeList controllerList = root.GetElementsByTagName("controller");

				foreach (XmlNode ctrlNode in controllerList)
				{
					string strSwitchID = ((XmlElement)ctrlNode).GetElementsByTagName("switchID")[0].InnerText;
					
					if (Program.iCurSwitchID.ToString() != strSwitchID)
					{
						continue;
					}

					string strSwitchPort = ((XmlElement)ctrlNode).GetElementsByTagName("switchPort")[0].InnerText;
					string strControllerPort = ((XmlElement)ctrlNode).GetElementsByTagName("controllerPort")[0].InnerText; ;
					int iSwitchPort = Const.INVALID_NUM;
					int iControllerPort = Const.INVALID_NUM;

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

					//增加物理端口，监听控制器发送的数据，默认控制器连接交换机的0端口
					Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
					retVal = PhyPortManager.GetInstance().AddPort(Const.PHY_PORT_FOR_CONTROLLER, iControllerPort, iSwitchPort);
					if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
					{
						//TODO
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "连接控制器失败");
						return retVal;
					}
				}

				//遍历Host标签
				XmlNodeList hostList = root.GetElementsByTagName("Host");
				foreach(XmlNode hostNode in hostList)
				{
					string strSwitchID = ((XmlElement)hostNode).GetElementsByTagName("switchID")[0].InnerText;

					if (Program.iCurSwitchID.ToString() != strSwitchID)
					{
						continue;
					}

					string strSwitchPort = ((XmlElement)hostNode).GetElementsByTagName("switchPort")[0].InnerText;
					string strSwitchPhyPortNum = ((XmlElement)hostNode).GetElementsByTagName("PhyPortNo")[0].InnerText;
					string strHostPort = ((XmlElement)hostNode).GetElementsByTagName("HostPort")[0].InnerText;

					int iSwitchPort = Const.INVALID_NUM;
					int iSwitchPhyPortNum = Const.INVALID_NUM;
					int iHostPort = Const.INVALID_NUM;

					boolTryParseResult = int.TryParse(strSwitchPort, out iSwitchPort);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strSwitchPhyPortNum, out iSwitchPhyPortNum);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "交换机物理端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					boolTryParseResult = int.TryParse(strHostPort, out iHostPort);
					if (boolTryParseResult != true)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "主机端口号转换整型变量失败");
						return Const.EN_RET_CODE.EN_RET_INT_TRY_PARSE_ERR;
					}

					//增加物理端口，监听主机发送的消息
					Const.EN_RET_CODE retVal = Const.EN_RET_CODE.EN_RET_INIT;
					retVal = PhyPortManager.GetInstance().AddPort(iSwitchPhyPortNum, iHostPort, iSwitchPort);
					if (retVal != Const.EN_RET_CODE.EN_RET_SUCC)
					{
						Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "连接主机失败");
						return retVal;
					}
				}
			}
			catch(Exception)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "Xml格式错误");
				return Const.EN_RET_CODE.EN_XML_FILE_FORMAT_ERR;
			}
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
