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
	internal class NextlineWithOffset : TextCommand {
		private readonly BaseObject _x;
		private readonly BaseObject _y;

		public NextlineWithOffset(BaseObject x, BaseObject y) {
			_x = x;
			_y = y;
		}

		public BaseObject X {
			get { return _x; }
		}

		public BaseObject Y {
			get { return _y; }
		}
	}
}
