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
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace PageOfBob.NFountain {
	public class DefaultWriter {
		public static void WriteOutput(TextWriter writer, string text, int indent, int width) {
			foreach (string line in BreakLines(text, width)) {
				if (indent > 0)
					writer.Write(new string(' ', indent));
				writer.WriteLine(line);
			}
		}
		
		public static void WriteRightAligned(TextWriter writer, string text, int width) {
			foreach (string line in BreakLines(text, width)) {
				int indent = width - line.Length;
				if (indent > 0)
					writer.Write(new string(' ', indent));
				writer.WriteLine(line);
			}
		}
		
		public static void WriteCentered(TextWriter writer, string text, int width) {
			foreach (string line in BreakLines(text, width)) {
				int indent = (width - line.Length);
				if (indent % 2 != 0)
					indent -= 1;
				
				if (indent > 0)
					writer.Write(new string(' ', indent / 2));
				writer.WriteLine(line);
			}
		}
		
		public static IEnumerable<string> BreakLines(string text, int size) {
			StringBuilder line = null;
			
			foreach (var token in StringTokenizer.Tokenize(text)) {
				if (line == null || token.Value.Length + line.Length >= size) {
					if (line != null)
						yield return line.ToString();
					line = new StringBuilder();
					if (token.IsWhitespace)
						continue;
				}
				
				line.Append(token.Value);
			}
			
			if (line != null)
				yield return line.ToString();
		}
		
		public DefaultWriter() {
			Columns = 80;
			CharacterIndent = 30;
			CharacterWidth = 30;
			DialogIndent = 20;
			DialogWidth = 40;
			ParentheticalIndent = 25;
			ParentheticalWidth = 30;
		}
		
		public int Columns { get; set; }
		public int CharacterIndent { get; set; }
		public int CharacterWidth { get; set; }
		public int DialogIndent { get; set; }
		public int DialogWidth { get; set; }
		public int ParentheticalIndent { get; set; }
		public int ParentheticalWidth { get; set; }
		
		public void Transform(IEnumerable<Element> elements, Stream output) {
			TextWriter writer = new StreamWriter(output);
			
			foreach (Element element in elements) {
				switch (element.Type) {
					case ElementType.None: break;
					case ElementType.Heading:
						WriteOutput(writer, ((HeadingElement)element).Value.ToUpperInvariant(), 0, Columns);
			            writer.WriteLine();
						break;
					case ElementType.DialogGroup:
						DialogGroupElement groupElement = (DialogGroupElement)element;
						WriteOutput(writer, groupElement.Character.ToUpperInvariant(), CharacterIndent, CharacterWidth);
						if (groupElement.Parenthetical != null) {
							WriteOutput(writer, "(" + groupElement.Parenthetical + ")", ParentheticalIndent, ParentheticalWidth);
						}
						WriteOutput(writer, groupElement.Dialog.ToString(), DialogIndent, DialogWidth);
						writer.WriteLine();
						break;
					case ElementType.Action:
						WriteOutput(writer, ((ActionElement)element).Content.ToString(), 0, Columns);
						writer.WriteLine();
						break;
					case ElementType.Transition:
						WriteRightAligned(writer, ((TransitionElement)element).Value, Columns);
						writer.WriteLine();
						break;
					case ElementType.CenteredText:
						WriteCentered(writer, ((CenteredTextElement)element).Content.ToString(), Columns);
						writer.WriteLine();
						break;
					case ElementType.LineBreak:
						writer.WriteLine(new string('=', Columns));
						writer.WriteLine();
						break;
					case ElementType.Boneyard: 
					case ElementType.NoteBlock: 
					case ElementType.Section: 
					case ElementType.Synopsis: 
						break;
					case ElementType.Title:
						
						break;
					default:
						throw new Exception("Invalid value for ElementType");
				}
			}
			
			writer.Flush();
		}
	}
}
