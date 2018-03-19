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
		public static RetVal.EN_RET_CODE InitFromFile(string fileName)
		{
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(fileName);
			}catch(Exception ex)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, fileName + "拓扑结构文件不存在！");
				return RetVal.EN_RET_CODE.EN_RET_FILE_NOT_EXIST;
			}

			Util.Log(Util.EN_LOG_LEVEL.EN_LOG_INFO, "读取拓扑文件");

			try
			{
				XmlElement root = doc.DocumentElement;
				XmlNodeList pairList = root.GetElementsByTagName("pair");

				foreach (XmlNode PairNode in pairList)
				{
					XmlNodeList switchList = ((XmlElement)PairNode).GetElementsByTagName("switch");
					string strID0 = ((XmlElement)switchList[0]).GetElementsByTagName("id")[0].InnerText;
					string strID1 = ((XmlElement)switchList[1]).GetElementsByTagName("id")[0].InnerText;
					string strPhyPortNo;
					string strLocalPortNum;
					string strRemotePortNum;

					if (strID0 == Program.switchID.ToString())
					{
						strPhyPortNo = ((XmlElement)switchList[0]).GetElementsByTagName("PhyPortNo")[0].InnerText;
						strLocalPortNum = ((XmlElement)switchList[0]).GetElementsByTagName("LocalPortNum")[0].InnerText;
						strRemotePortNum = ((XmlElement)switchList[1]).GetElementsByTagName("LocalPortNum")[0].InnerText;
					}
					else if (strID1 == Program.switchID.ToString())
					{
						strPhyPortNo = ((XmlElement)switchList[1]).GetElementsByTagName("PhyPortNo")[0].InnerText;
						strLocalPortNum = ((XmlElement)switchList[1]).GetElementsByTagName("LocalPortNum")[0].InnerText;
						strRemotePortNum = ((XmlElement)switchList[0]).GetElementsByTagName("LocalPortNum")[0].InnerText;
					}
					else
					{
						continue;
					}
					Console.WriteLine("PhyPortNo = {0}, LocalPortNum = {1}, RemotePortNum = {2}", strPhyPortNo, strLocalPortNum, strRemotePortNum);
				}
			}catch(Exception ex)
			{
				Util.Log(Util.EN_LOG_LEVEL.EN_LOG_FATAL, "Xml格式错误");
			}
			return RetVal.EN_RET_CODE.EN_RET_SUCC;
		}
	}
}
