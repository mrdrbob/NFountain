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
using System.Runtime.Remoting.Messaging;
using Sprache;

namespace PageOfBob.NFountain {
	public static class StringTokenizer {
		public class Token {
			public Token(string value, bool isWhitespace) {
				Value = value;
				IsWhitespace = isWhitespace;
			}
			
			public string Value { get; private set; }
			public bool IsWhitespace { get; private set; }
		}
		
		public static Parser<string> Whitespace;
		public static Parser<string> NotWhitespace;
		public static Parser<Token[]> TokenPairs;
		public static Parser<IEnumerable<Token>> Tokenizer;
		
		static StringTokenizer() {
			Whitespace = 
				from ws in Parse.Char(x => x == ' ' || x == '\t', "Whitespace").Many().Text()
				select ws;
			NotWhitespace = 
				from nws in Parse.Char(x => x != ' ' && x != '\t', "Not whitespace").AtLeastOnce().Text()
				select nws;
			TokenPairs = 
				from ws in Whitespace
				from nws in NotWhitespace
				select new Token[] { new Token(" ", true), new Token(nws, false) };
			Tokenizer = 
				from t in TokenPairs.Many()
				select AggregateTokens(t);
		}
		
		public static IEnumerable<Token> Tokenize(string input) {
			return Tokenizer.Parse(input);
		}
		
		private static IEnumerable<Token> AggregateTokens(IEnumerable<Token[]> pairs) {
			foreach (var pair in pairs) {
				foreach (Token token in pair) {
					if (token.Value != null)
						yield return token;
				}
			}
		}
	}
}
