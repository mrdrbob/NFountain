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
using System.Text;

namespace PageOfBob.NFountain
{
	public class ContentNode {
		public ContentNode(ContentNodeType type, string value, ContentNode[] children) {
			Type = type;
			Children = children;
			Value = value;
		}
		
		public string Value { get; private set; }
		public ContentNode[] Children { get; private set; }
		public ContentNodeType Type { get; private set; }
		
		private void ToStringRecursive(StringBuilder sb) {
			if (Value != null)
				sb.Append(Value);
			if (Children != null) {
				foreach (ContentNode node in Children) {
					node.ToStringRecursive(sb);
				}
			}
		}
		
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			ToStringRecursive(sb);
			return sb.ToString();
		}

	}
	
	public enum ContentNodeType {
		UnparseRawText = 0,
		Container,
		Text,
		Bold,
		Italic,
		BoldItalic,
		Underline
	}
}
