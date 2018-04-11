using SDNCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switch.AppLayer
{
	/// <summary>
	/// 流表，保存流表
	/// </summary>
	public class FlowTable
	{
		private SortedList<string, FlowTableItem> list;
		private static FlowTable instance = null;
		private static readonly object objLock = new object();

		/// <summary>
		/// 私有构造函数，单例模式
		/// </summary>
		private FlowTable()
		{
			list = new SortedList<string, FlowTableItem>();
		}

		/// <summary>
		/// 获取单例
		/// </summary>
		/// <returns></returns>
		public static FlowTable GetInstance()
		{
			if (null == instance)
			{
				lock(objLock)
				{
					if (null == instance)
					{
						instance = new FlowTable();
					}
				}
			}
			return instance;
		}

		/// <summary>
		/// 尝试获取流表项，二分查找
		/// </summary>
		/// <param name="desIP">目的IP</param>
		/// <param name="phyPortNo">出参，若成功获取，返回转发物理端口</param>
		/// <returns>流表中有该表项则返回true， 否则返回false</returns>
		public bool TryGetItem(string desIP, out int phyPortNo)
		{
			int low = 0;
			int high = list.Count;
			int mid = Const.INVALID_NUM;

			while (low <= high)
			{
				mid = (low + high) / 2;
				if (string.Compare(list.ElementAt(mid).Key, desIP) == 0)
				{
					phyPortNo = list.ElementAt(mid).Value.iTransPhyPort;
					return true;
				}
				else if (string.Compare(list.ElementAt(mid).Key, desIP) > 0)
				{
					high = mid - 1;
				}
				else
				{
					low = mid + 1;
				}
			}
			phyPortNo = Const.INVALID_NUM;
			return false;
		}
	}
}
