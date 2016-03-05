using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetPass {
	public abstract class NetPassProtocol<T> {
		protected T _object;
		protected NetPassData _data;
		//protected ushort _type = 0;

		public T Object {
			get {
				return _object;
			}
		}

		public NetPassData Data {
			get {
				return _data;
			}
		}

		public NetPassProtocol(T obj) {
			_object = obj;
			_data = ConvertToData(_object);
		}

		public NetPassProtocol(NetPassData data) {
			_data = data;
			_object = ConvertToObject(data);
		}

		protected abstract T ConvertToObject(NetPassData data);
		protected abstract NetPassData ConvertToData(T obj);

	}
}
