using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPass {
	public class NetPassDataHead {
		private static readonly int _dataHeadSize = 6;
		private ushort _type = 0;
		private int _dataLength = 0;
		private string _receiver = string.Empty;

		public static int DataHeadSize {
			get {
				return _dataHeadSize;
			}
		}

		public ushort Type {
			set {
				_type = value;
			}

			get {
				return _type;
			}
		}

		public int DataLength {
			set {
				_dataLength = value;
			}

			get {
				return _dataLength;
			}
		}

		public string Reveiver {
			get {
				return _receiver;
			}

			set {
				_receiver = value;
			}
		}

		public byte[] Bytes {
			get {
				return GetByte();
			}
		}

		public NetPassDataHead(ushort type, int dataLength, string receiver = "") {
			this._type = type;
			this._dataLength = dataLength;
			this._receiver = receiver;
		}

		public NetPassDataHead(byte[] headByteArray) {
			this._type = BitConverter.ToUInt16(headByteArray, 0);
			this._dataLength = BitConverter.ToInt32(headByteArray, 2);
			this._receiver = Encoding.UTF8.GetString(headByteArray, 6, _dataHeadSize - 6).TrimEnd('\0');
		}

		private byte[] GetByte() {
			byte[] byteArray = new byte[_dataHeadSize];
			byte[] type = BitConverter.GetBytes((ushort)this._type);
			byte[] dataLength = BitConverter.GetBytes(this._dataLength);
			byte[] receiver = Encoding.UTF8.GetBytes(_receiver);

			int index = 0;

			foreach (byte data in type) {
				byteArray[index] = data;
				index++;
			}

			foreach (byte data in dataLength) {
				byteArray[index] = data;
				index++;
			}

			foreach (byte data in receiver) {
				byteArray[index] = data;
				index++;
			}

			return byteArray;
		}
	}
}
