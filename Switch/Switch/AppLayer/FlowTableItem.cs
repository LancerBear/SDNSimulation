using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switch.AppLayer
{
	/// <summary>
	/// 流表项
	/// </summary>

	public class FlowTableItem
	{
		public string strDesIP;
		public int iTransPhyPort;

		public FlowTableItem(string strDesIP, int iTransPhyPort)
		{
			this.strDesIP = strDesIP;
			this.iTransPhyPort = iTransPhyPort;
		}
	}
}
