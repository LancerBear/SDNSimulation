using SDNCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Switch
{
	/// <summary>
	/// 流表，保存流表
	/// </summary>
	public class FlowTable
	{
		private static SortedList<string, FlowTableItem> SortedItemList;
		private static FlowTable instance = null;
		private static readonly object objLock = new object();
		private static readonly object RWObjLock = new object();

		/// <summary>
		/// 私有构造函数，单例模式
		/// </summary>
		private FlowTable()
		{
			SortedItemList = new SortedList<string, FlowTableItem>();
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
			lock (RWObjLock)
			{
				if (SortedItemList.Count == 0)
				{
					phyPortNo = Const.INVALID_NUM;
					return false;
				}

				int low = 0;
				int high = SortedItemList.Count - 1;
				int mid = Const.INVALID_NUM;

				while (low <= high)
				{
					mid = (low + high) / 2;
					//Console.WriteLine("mid = " + mid);
					if (string.Compare(SortedItemList.ElementAt(mid).Key, desIP) == 0)
					{
						phyPortNo = SortedItemList.ElementAt(mid).Value.iTransPhyPort;
						return true;
					}
					else if (string.Compare(SortedItemList.ElementAt(mid).Key, desIP) > 0)
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
		/// <summary>
		/// 增加流表项
		/// </summary>
		/// <param name="fItem"></param>
		public bool AddItem(FlowTableItem fItem)
		{
			lock (RWObjLock)
			{
				try
				{
					SortedItemList.Add(fItem.strDesIP, fItem);
				}
				//Key是null
				catch (System.ArgumentNullException)
				{
					return false;
				}
				//已经存在键值相同的Item
				catch (System.ArgumentException)
				{
					return true;
				}
				return true;
			}
		}

		/// <summary>
		/// 打印现有流表中的流表项，测试用
		/// </summary>
		public static void PrintItems()
		{
			Console.WriteLine("当前流表项：");
			for (int i = 0; i < SortedItemList.Count; i++)
			{
				Console.WriteLine(SortedItemList.ElementAt(i).Key + "\t" + SortedItemList.ElementAt(i).Value.iTransPhyPort);
			}
		}
	}
}
