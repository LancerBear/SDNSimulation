using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			}catch(Exception ex)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, fileName + "拓扑结构文件不存在！");
				return Const.EN_RET_CODE.EN_RET_FILE_NOT_EXIST;
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "读取拓扑文件");

			string strID0;
			string strID1;
			string strPhyPortNo;
			string strLocalPortNum;
			string strRemotePortNum;

			int iPhyPortNo = -1;
			int iLocalPortNum = -1;
			int iRemotePortNum = -1;

			//记录当前交换机的ID在pair中的下标
			int selfIndexInPair = -1;
			try
			{
				XmlElement root = doc.DocumentElement;
				XmlNodeList pairList = root.GetElementsByTagName("pair");

				//遍历所有pair
				foreach (XmlNode PairNode in pairList)
				{
					XmlNodeList switchList = ((XmlElement)PairNode).GetElementsByTagName("switch");
					strID0 = ((XmlElement)switchList[0]).GetElementsByTagName("id")[0].InnerText;
					strID1 = ((XmlElement)switchList[1]).GetElementsByTagName("id")[0].InnerText;
					
					//监测pair 中是否有某个交换机ID和当前交换机的ID相同
					if (strID0 == Program.switchID.ToString())
					{
						selfIndexInPair = 0;
					}
					else if (strID1 == Program.switchID.ToString())
					{
						selfIndexInPair = 1;
					} 
					else
					{
						selfIndexInPair = -1;
						continue;
					}
					
					//pair 中某个交换机ID和当前交换机的ID相同
					if (selfIndexInPair != -1)
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
			}catch(Exception ex)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "Xml格式错误");
				return Const.EN_RET_CODE.EN_XML_FILE_FORMAT_ERR;
			}
			return Const.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
