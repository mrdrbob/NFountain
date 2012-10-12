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
using System.Collections.Generic;

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

		public bool HasChildren { get { return Children != null && Children.Length > 0; } }
		
		#region Linearization
		private class LinearizerState {
			public LinearizerState(LinearizerState parent, ContentNode node, int pos, FontStyle font) {
				Parent = parent;
				Node = node;
				Pos = pos;
				Font = font;
			}
			
			public LinearizerState Parent { get; private set; }
			public ContentNode Node { get; private set; }
			public int Pos { get; private set; }
			public FontStyle Font { get; private set; }
			
			public bool NextKidReady { get { return Node.Children != null && Pos < Node.Children.Length; } }
			public LinearizerState Child() {
				return new LinearizerState(this, Node.Children[Pos], 0, NodeTypeToFontStyle(Node.Children[Pos].Type));
			}
			public LinearizerState Next() {
				return new LinearizerState(Parent, Node, Pos + 1, Font);
			}
			
			public FontStyle FindStyle() {
				LinearizerState state = this;
				while(state.Font == FontStyle.None && state != null) {
					state = state.Parent;
				}
				return state == null ? FontStyle.Plain : state.Font;
			}
		}
		
		// Linearizes the tree structure of a content node into single words with
		// font style attached, making it easier to output.
		public IEnumerable<WordAndFont> Linearize() {
			Stack<LinearizerState> stack = new Stack<ContentNode.LinearizerState>();
			
			LinearizerState current = new ContentNode.LinearizerState(null, this, 0, FontStyle.Plain);
			
			// Descend first
			stack.Push(current);
			while(current.NextKidReady) {
				current = current.Child();
				stack.Push(current);
			}
			
			do {
				// Grab item
				current = stack.Pop();
				
				// Any value to output?
				if (current.Node.Value != null) {
					FontStyle style = current.FindStyle();
					foreach (string t in current.Node.Value.Split(new char[] { ' ', '\t', '\r', '\n'})) {
						string trimmed = t.Trim();
						if (trimmed.Length == 0)
							continue;
						yield return new WordAndFont(style, trimmed);
					}
				}
				
				// Do we have kids, and a next kid to go to?
				if (current.NextKidReady) {
					// Put myself back on the stack
					stack.Push(current = current.Next());
					
					// The the depth of the kid's kids.
					while(current.NextKidReady) {
						current = current.Child();
						stack.Push(current);
					}
				}
			} while(stack.Count > 0);
		}
		#endregion
		
		public static FontStyle NodeTypeToFontStyle(ContentNodeType type) {
			switch (type) {
				case PageOfBob.NFountain.ContentNodeType.Bold:
					return FontStyle.Bold;
				case PageOfBob.NFountain.ContentNodeType.Italic:
					return FontStyle.Italic;
				case PageOfBob.NFountain.ContentNodeType.BoldItalic:
					return FontStyle.BoldItalic;
				case PageOfBob.NFountain.ContentNodeType.Underline:
					return FontStyle.Underline;
				default:
					return FontStyle.None;
			}
		}
	}
	
	
	public enum FontStyle {
		None = 0,
		Plain = 1,
		Bold = 1 << 2,
		Italic = 1 << 3,
		Underline = 1 << 4,
		BoldItalic = Bold | Italic
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
