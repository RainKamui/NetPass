using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace NetPass {

	public delegate void ProcessDataEventHandler(NetPassData data);
	public delegate void PassDataEventHandler(object ojb);

	public abstract class NetPassProcessHandle {

		#region Field
		
		private int _receiveDelay = 100;
		private int _sendDelay = 100;
		private int _buffSize = 0;
		private bool _isAlive = false; //=

		//private NetPassListener<Object> _listener = null;
		private Queue<byte> _receiveByteQueue = new Queue<byte>();
		private Queue<NetPassData> _sendDataQueue = new Queue<NetPassData>();
		private NetPassDataHead _currentDataHead = null;
		private TcpClient _tcpClient = null; //=
		private NetworkStream _networkStream = null;

		/// <summary>
		/// 0:Head, 1:Data
		/// </summary>
		private short _currentDataType = 0;

		private ProcessDataEventHandler[] _processDataHandlerArray = new ProcessDataEventHandler[64];
		private PassDataEventHandler[] _passDataEventHandlerArrayArray = null;

		public bool IsAlive {
			set {
				this._isAlive = value;
			}

			get {
				return this._isAlive;
			}
		}

		#endregion

		public NetPassProcessHandle() {
		}

		public void Initialize(TcpClient tcpClient, PassDataEventHandler[] passHandle = null, bool alive = true, int receiveDelay = 100, int sendDelay = 100) {
			this._tcpClient = tcpClient;
			this._isAlive = alive;
			this._receiveDelay = receiveDelay;
			this._sendDelay = sendDelay;
			this._buffSize = _tcpClient.ReceiveBufferSize;
			this._passDataEventHandlerArrayArray = passHandle;
			GetNetworkStream();
		}

		public abstract NetPassProcessHandle GetNewInstance();

		/*
		public NetPassProcessHandle(TcpClient tcpClient, bool alive = true, int receiveDelay = 100, int sendDelay = 100) {
			this._tcpClient = tcpClient;
			this._isAlive = alive;
			this._receiveDelay = receiveDelay;
			this._sendDelay = sendDelay;
			this._buffSize = _tcpClient.ReceiveBufferSize;
			GetNetworkStream();
		}
		 * */

		/// <summary>
		/// 获取当前TcpClient的NetworkStream
		/// </summary>
		private void GetNetworkStream() {
			if (_tcpClient != null && _tcpClient.Connected) {
				try {
					_networkStream = _tcpClient.GetStream();
				}
				catch (Exception ex) {
					Trace.TraceError(ex.ToString());
				}
				finally {
					
				}
			}
		}

		/// <summary>
		/// 判断当前TcpClient是否可用已经Handle是否为活跃状态
		/// </summary>
		/// <returns></returns>
		private bool IsAvailable() {
			return _isAlive && _tcpClient != null && _tcpClient.Connected;
		}

		/// <summary>
		/// 接受并处理Data
		/// </summary>
		public void Receive() {
			while (true) {
				if (IsAvailable()) {
					if (_networkStream != null) {
						byte[] readBuffer = null;
						if (_networkStream.CanRead) {
							readBuffer = new byte[_buffSize];
							int numberOfBytesRead = 0;
							do {
								try {
									numberOfBytesRead = _networkStream.Read(readBuffer, 0, _buffSize);
								}
								catch (Exception ex) {
									Trace.TraceError(ex.ToString());
								}
								if (numberOfBytesRead != 0) {
									for (int i = 0; i < numberOfBytesRead; i++)
										_receiveByteQueue.Enqueue(readBuffer[i]);

									if (_currentDataType == 0 && _receiveByteQueue.Count >= NetPassDataHead.DataHeadSize) {
										byte[] dataHeadByteArray = new byte[NetPassDataHead.DataHeadSize];
										for (int i = 0; i < NetPassDataHead.DataHeadSize; i++)
											dataHeadByteArray[i] = _receiveByteQueue.Dequeue();

										_currentDataHead = new NetPassDataHead(dataHeadByteArray);
										_currentDataType = 1;
									}

									if (_currentDataType == 1 && _receiveByteQueue.Count >= _currentDataHead.DataLength) {
										byte[] dataByteArray = new byte[_currentDataHead.DataLength];
										for (uint i = 0; i < _currentDataHead.DataLength; i++)
											dataByteArray[i] = _receiveByteQueue.Dequeue();

										NetPassData data = new NetPassData(_currentDataHead, dataByteArray);
										ParseData(data);
										_currentDataType = 0;
									}
								}
							}
							while (_networkStream.DataAvailable);
						}
					}
					else {
						GetNetworkStream();
					}
				}
				Application.DoEvents();
				Thread.Sleep(_receiveDelay);
			}
		}

		/// <summary>
		/// 发送队列中的Data
		/// </summary>
		public void Send() {
			while (true) {
				if (IsAvailable()) {
					if (_networkStream != null) {
						if (_sendDataQueue.Count > 0) {
							NetPassData data = _sendDataQueue.Dequeue();
							if (_networkStream.CanWrite) {
								try {
									_networkStream.Write(data.DataHead.Bytes, 0, NetPassDataHead.DataHeadSize);
									_networkStream.Write(data.Data, 0, data.Data.Length);
								}
								catch (Exception ex) {
									Trace.TraceError(ex.ToString());
								}
							}
						}
					}
					else {
						GetNetworkStream();
					}
				}
				Application.DoEvents();
				Thread.Sleep(_sendDelay);
			}
		}

		/// <summary>
		/// 将数据添加到发送队列
		/// </summary>
		/// <param name="data">要发送的数据</param>
		public void AddData(NetPassData data) {
			if (data != null) {
				_sendDataQueue.Enqueue(data);
			}
		}

		public void AddData<T>(NetPassProtocol<T> protocol) {
			if (protocol != null) {
				
			}
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		private void Release() {
		
		}

		/// <summary>
		/// 处理数据
		/// </summary>
		/// <param name="data"></param>
		private void ParseData(NetPassData data) {
			if (this._processDataHandlerArray[data.DataHead.Type] != null) {
				this._processDataHandlerArray[data.DataHead.Type](data);
			}
		}

		/// <summary>
		/// 注册类型处理函数
		/// </summary>
		/// <param name="type"></param>
		/// <param name="handler"></param>
		/// <param name="over"></param>
		protected void RegisterProcessDataHandler(ushort type, ProcessDataEventHandler handler, bool over = true) {
			if (over) {
				this._processDataHandlerArray[type] = handler;
			}
			else {
				if (this._processDataHandlerArray[type] != null) {
					this._processDataHandlerArray[type] = handler;
				}
			}
		}

		protected void ProcessObject(int passDataHandlerIndex, object obj) {
			if (_passDataEventHandlerArrayArray != null && _passDataEventHandlerArrayArray[passDataHandlerIndex] != null) {
				_passDataEventHandlerArrayArray[passDataHandlerIndex](obj);
			}
		}
	}
}
