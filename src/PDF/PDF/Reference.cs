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

namespace PageOfBob.NFountain.PDF {
	internal struct Reference {
		private readonly int _generation;
		private readonly int _id;

		public Reference(int generation, int id) {
			_generation = generation;
			_id = id;
		}

		public int Generation {
			get { return _generation; }
		}

		public int Id {
			get { return _id; }
		}

		public override bool Equals(object obj) {
			if (!(obj is Reference))
				return false;
			Reference r = (Reference)obj;
			return r.Id == Id && r.Generation == Generation;
		}

		public override int GetHashCode() {
			unchecked {
				int hash = 13;
				hash = (hash * 7) + _generation.GetHashCode();
				hash = (hash * 7) + _id.GetHashCode();
				return hash;
			}
		}

		public static bool operator ==(Reference r, Reference r2) {
			return r.Equals(r2);
		}

		public static bool operator !=(Reference r, Reference r2) {
			return !r.Equals(r2);
		}
	}
}
