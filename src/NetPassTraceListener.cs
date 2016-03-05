using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace NetPass {
	class NetPassTraceListener : TraceListener {
		public override void Write(string message) {
			File.AppendAllText(@"./log.log", message);
		}

		public override void WriteLine(string message) {
			File.AppendAllText(@"./log.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss    ") + message + Environment.NewLine);
		}
	}
}
