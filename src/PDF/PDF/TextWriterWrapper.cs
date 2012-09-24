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
using System.IO;
using System.Text;

namespace PageOfBob.NFountain.PDF {
	internal class TextWriterWrapper : TextWriter {
		private readonly TextWriter _writer;
		private int _position;
		private int _line;
		private int _column;
		private char _lastChar = '\0';

		public TextWriterWrapper(TextWriter writer) {
			_writer = writer;
		}

		public override void Write(char value) {
			if (value == '\n') {
				if (_lastChar != '\r') {
					_column = 0;
					_line += 1;
				}
			} else if (value == '\r') {
				_column = 0;
				_line += 1;
			}

			_writer.Write(value);
			_position++;
			_lastChar = value;
		}

		public int Position {
			get { return _position; }
		}

		public int Line {
			get { return _line; }
		}

		public int Column {
			get { return _column; }
		}

		public override Encoding Encoding {
			get { throw new NotImplementedException(); }
		}
	}
}
