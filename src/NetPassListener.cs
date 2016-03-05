using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;


namespace NetPass {
	public class NetPassListener<T> where T : NetPassProcessHandle {
		private string _serverIp = "0.0.0.0";
		private int _port = 0;
		private int _backlog = 0;

		private TcpListener _tcpListener = null;

		Thread _serverThread = null;
		private PassDataEventHandler[] _passDataEventHandlerArray = new PassDataEventHandler[64];
		private Dictionary<string, NetPassProcessHandle> _processHandleDictionary = new Dictionary<string, NetPassProcessHandle>();

		public NetPassListener(string serverIp, int port, int backlog = 0) {
			this._serverIp = serverIp;
			this._port = port;
			this._backlog = backlog;
		}

		public bool StartServer() {
			if (_serverIp == string.Empty || _port == 0)
				return false;

			_serverThread = new Thread(new ThreadStart(Server));

			_serverThread.Start();

			return true;

		}

		public void RegisterPassDataEventHandler(int passDataHandlerIndex, PassDataEventHandler handler, bool over = false) {
			if (_passDataEventHandlerArray[passDataHandlerIndex] == null || over) {
				_passDataEventHandlerArray[passDataHandlerIndex] = handler;
			}
		}

		private void Server() {

			IPEndPoint ipe = new IPEndPoint(IPAddress.Any, _port);

			_tcpListener = new TcpListener(ipe);
			if (_backlog == 0) {
				_tcpListener.Start();
			}
			else {
				_tcpListener.Start(_backlog);
			}

			TcpClient tcpClient = null;

			while (true) {
				try {
					tcpClient = _tcpListener.AcceptTcpClient();

					if (tcpClient.Connected) {
						string connectName = GetConnectName(tcpClient);
						if (connectName != null) {
							_processHandleDictionary[connectName] = StartProcessHandle(tcpClient);
						}
					}
				}
				catch (Exception ex) {
					Trace.TraceError(ex.ToString());
				}
				finally {
					/*
					if (tcpClient != null) {
						try {
							tcpClient.Close();
						}
						finally {
							tcpClient = null;
						}
					}
					 * */
				}
			}
		}

		private string GetConnectName(TcpClient tcpClient) {
			NetworkStream networkStream = tcpClient.GetStream();
			if (networkStream != null && networkStream.CanRead) {
				byte[] nameByteArray = new byte[256];
				networkStream.Read(nameByteArray, 0, 256);
				string connectName = Encoding.UTF8.GetString(nameByteArray).TrimEnd('\0');
				return connectName;
			}
			else {
				return null;
			}
		}

		private NetPassProcessHandle StartProcessHandle(TcpClient tcpClient) {
			NetPassProcessHandle handle = GetProcessHandle(tcpClient);
			if (handle != null) {
				Thread receiveThread = new Thread(new ThreadStart(handle.Receive));
				Thread sendThread = new Thread(new ThreadStart(handle.Send));
				receiveThread.Start();
				sendThread.Start();
				return handle;
			}
			else {
				Trace.TraceError("Get Handle Failed");
				return null;
			}
		}

		private NetPassProcessHandle GetProcessHandle(TcpClient tcpClient) {
			NetPassProcessHandle handle = System.Activator.CreateInstance<T>();
			handle.Initialize(tcpClient, _passDataEventHandlerArray);
			return handle;
		}

	}
}
