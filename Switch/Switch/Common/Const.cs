﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Switch
{
	public static class Const
	{
		public enum EN_RET_CODE
		{
			EN_RET_INIT = 0,
			EN_RET_SUCC,
			EN_XML_FILE_FORMAT_ERR,
			EN_RET_FILE_NOT_EXIST,
			EN_RET_PHY_PORT_OVERFLOW,
			EN_RET_PHY_PORT_NOT_CONNECTED,
			EN_RET_CALL_BACK_FUNC_NOT_SET,
			EN_RET_ERR
		}

		public static IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

		public const int MAX_PHY_PORT_NUM = 3;
		public const int MIN_PHY_PORT_NUM = 0;

		public const int MAX_PORT_NUM = 65535;
		public const int MIN_PORT_NUM = 0;

		public const int MAX_SWITCH_NUM = 127;
		public const int MIN_SWITCH_NUM = 0;

		public const int MAX_PACKET_LENGTH = 2048;
	}
}