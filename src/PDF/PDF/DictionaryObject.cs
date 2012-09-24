/*
	   Copyright 2012 Bob Davidson
	
	   Licensed under the Apache License, Version 2.0 (the "License");
	   you may not use this file except in compliance with the License.
	   You may obtain a copy of the License at
	
	       http://www.apache.org/licenses/LICENSE-2.0
	
	   Unless required by applicable law or agreed to in writing, software
	   distributed under the License is distributed on an "AS IS" BASIS,
	   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	   See the License for the specific language governing permissions and
	   limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace PageOfBob.NFountain.PDF {
	internal class DictionaryObject : BaseObject {
		private readonly List<KeyValuePair<NameObject, BaseObject>> _values = new List<KeyValuePair<NameObject, BaseObject>>();
		private readonly Dictionary<string, int> _lookup = new Dictionary<string, int>();

		public DictionaryObject Set(string key, BaseObject value) {
			if (value is IndirectObject)
				value = new IndirectReferenceObject(((IndirectObject)value).Reference);

			int index;

			if(_lookup.TryGetValue(key, out index)) {
				throw new NotImplementedException();
			}

			_lookup.Add(key, _values.Count);
			_values.Add(new KeyValuePair<NameObject, BaseObject>(new NameObject(key), value));

			return this;
		}

		public DictionaryObject SetIfNotAlready(string key, BaseObject value) {
			if (_lookup.ContainsKey(key))
				return this;

			return Set(key, value);
		}

		public IEnumerable<KeyValuePair<NameObject, BaseObject>> Objects {
			get { return _values; }
		}

		public T Get<T>(string key) where T : BaseObject {
			int index;
			if (!_lookup.TryGetValue(key, out index))
				return null;
			return _values[index].Value as T;
		}
	}
}
