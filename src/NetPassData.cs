using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPass {
	public class NetPassData {
		private NetPassDataHead _dataHead = null;
		private byte[] _data = null;

		public NetPassDataHead DataHead {
			get {
				return _dataHead;
			}
		}
		
		public byte[] Data {
			get {
				return _data;
			}
		}

		public int Length {
			get {
				return _data.Length;
			}
		}

		public NetPassData(ushort type, byte[] dataByteArray, string headTag = "") {
			this._dataHead = new NetPassDataHead(type, dataByteArray.Length, headTag);
			this._data = dataByteArray;
		}

		public NetPassData(NetPassDataHead dataHead, byte[] dataByteArray) {
			this._dataHead = dataHead;
			this._data = dataByteArray;
		}

		public override string ToString() {
			if (_data != null) {
				return Encoding.UTF8.GetString(_data);
			}
			else {
				return string.Empty;
			}
		}
		
	}
}
