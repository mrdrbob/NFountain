using System;
using System.IO;
using System.Collections.Generic;

using Sprache;

namespace PageOfBob.NFountain.Plugins {
	public class DefaultParserModule : DefaultParser, IParser {
		public IEnumerable<Element> Transform(TextReader str) {
			string fullText = str.ReadToEnd();
			return Elements.Parse(fullText);
		}
	}
}
