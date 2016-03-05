using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

namespace NetPass {
	public class NetPassConnector<T> where T : NetPassProcessHandle {

		private TcpClient _tcpClient = null;
		private	string _serverIP = string.Empty;
		private int _port = 0;
		private NetPassProcessHandle _handle = null;

		private PassDataEventHandler[] _passDataEventHandlerArray = new PassDataEventHandler[64];

		public NetPassProcessHandle ProcessHandle {
			get {
				return _handle;
			}
		}

		public NetPassConnector(string serverIP, int port) {
			this._serverIP = serverIP;
			this._port = port;
		}

		public void Connect(string connectName) {
			if (_serverIP != string.Empty && _port != 0) {
				IPAddress ipAddress = IPAddress.Parse(_serverIP);
				IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, _port);

				_tcpClient = new TcpClient();
				try {
					_tcpClient.Connect(ipEndPoint);
					if (_tcpClient.Connected) {
						SendConnectName(connectName);
						StartProcessHandle(_tcpClient);
					}
				}
				catch {
					Trace.TraceError("Connect Failed");
				}
			}
			else {
				throw new Exception("Server Address Not Set");
			}
		}

		public void RegisterPassDataEventHandler(int passDataHandlerIndex, PassDataEventHandler handler, bool over = false) {
			if (_passDataEventHandlerArray[passDataHandlerIndex] == null || over) {
				_passDataEventHandlerArray[passDataHandlerIndex] = handler;
			}
		}

		private void SendConnectName(string name) {
			NetworkStream networkStream = _tcpClient.GetStream();
			if (networkStream != null && networkStream.CanWrite) {
				int index = 0;
				byte[] nameSendByteArray = new byte[256];
				byte[] nameByteArray = Encoding.UTF8.GetBytes(name);
				foreach (byte data in nameByteArray) {
					nameSendByteArray[index] = data;
					index++;
				}
				networkStream.Write(nameSendByteArray, 0, nameSendByteArray.Length);
			}
		}

		private bool StartProcessHandle(TcpClient tcpClient) {
			_handle = GetProcessHandle(tcpClient);
			if (_handle != null) {
				Thread receiveThread = new Thread(new ThreadStart(_handle.Receive));
				Thread sendThread = new Thread(new ThreadStart(_handle.Send));
				receiveThread.Start();
				sendThread.Start();
				return true;
			}
			else {
				Trace.TraceError("Get Handle Failed");
				return false;
			}
		}


		private NetPassProcessHandle GetProcessHandle(TcpClient tcpClient) {
			NetPassProcessHandle handle = System.Activator.CreateInstance<T>();
			handle.Initialize(tcpClient, _passDataEventHandlerArray);
			return handle;
		}

	}
}
