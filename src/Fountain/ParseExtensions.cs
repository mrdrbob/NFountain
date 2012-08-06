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
using System.Linq;
using System.Collections.Generic;

using Sprache;

namespace PageOfBob.NFountain
{
	public static class ParseExtensions {
		public static Parser<IEnumerable<char>> StringIgnoreCase(string s) {
			if (s == null) {
				throw new ArgumentNullException("s");
			}
			return s.Select(new Func<char, Parser<char>>(ParseExtensions.CharIgnoreCase)).Aggregate(Parse.Return<IEnumerable<char>>(Enumerable.Empty<char>()), (Parser<IEnumerable<char>> a, Parser<char> p) => a.Concat(p.Once<char>())).Named(s);
		}

		public static Parser<char> CharIgnoreCase(char c) {
			return Parse.Char((char ch) => Char.ToLowerInvariant(c) == Char.ToLowerInvariant(ch), c.ToString());
		}
	}
}
