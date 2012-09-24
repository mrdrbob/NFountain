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
using System.Linq;
using System.Text;

namespace PageOfBob.NFountain.Plugins {
	public class HtmlWriter : IWriter {
		private string ReadEmbeddedResource(string name) {
			string result = null;
			using(Stream str = this.GetType().Assembly.GetManifestResourceStream(name))
			using(StreamReader r = new StreamReader(str)){
				result = r.ReadToEnd();
			}
			return result;
		}
		
		public static string HtmlEncode(string rawValue) {
			return rawValue;
		}
		
		public static string HtmlEncode(ContentNode node) {
			StringBuilder sb = new StringBuilder();
			HtmlEncodeRecursive(node, sb);
			return sb.ToString();
		}
		
		static void HtmlEncodeRecursive(ContentNode node, StringBuilder sb) {
			string pre, post;
			
			switch (node.Type) {
				case ContentNodeType.UnparseRawText:
					pre = post = null;
					break;
				case ContentNodeType.Container:
					pre = post = null;
					break;
				case ContentNodeType.Text:
					pre = post = null;
					break;
				case ContentNodeType.Bold:
					pre = "<b>";
					post = "</b>";
					break;
				case ContentNodeType.Italic:
					pre = "<i>";
					post = "</i>";
					break;
				case ContentNodeType.BoldItalic:
					pre = "<b><i>";
					post = "</i></b>";
					break;
				case ContentNodeType.Underline:
					pre = "<span class=\"underline\">";
					post = "</span>";
					break;
				default:
					throw new Exception("Invalid value for ContentNodeType");
			}
			
			if (pre != null)
				sb.Append(pre);
			
			if (!string.IsNullOrEmpty(node.Value))
				sb.Append(HtmlEncode(node.Value));
			
			if (node.Children != null) {
				foreach (ContentNode child in node.Children) {
					HtmlEncodeRecursive(child, sb);
				}
			}
			
			if (post != null)
				sb.Append(post);
		}
		
		public void Transform(IEnumerable<Element> elements, Stream output) {
			string headerTemplate = ReadEmbeddedResource("PageOfBob.NFountain.Plugins.Templates.Script-Header.html");
			string footer = ReadEmbeddedResource("PageOfBob.NFountain.Plugins.Templates.Script-Footer.html");
			
			string title = "NFountain Script";
			
			Element first = elements.FirstOrDefault();
			if (first == null)
				return;
			
			if (first.Type == ElementType.Title) {
				TitleElement element = (TitleElement)first;
				var titleKVP = element.Parts.FirstOrDefault(x => x.Key.ToLowerInvariant() == "title");
				if (!string.IsNullOrWhiteSpace(titleKVP.Value)) {
					title = titleKVP.Value.Trim();
				}
			}
			
			
			TextWriter w = new StreamWriter(output);
			w.Write(headerTemplate.Replace("{{Script Name}}", title));
			
			
			foreach (Element item in elements) {
				switch (item.Type) {
					case ElementType.None:
						
						break;
					case ElementType.Heading:
						w.Write("<div class\"heading\">");
						w.Write(HtmlEncode(((HeadingElement)item).Value));
						w.WriteLine("</div>");
						break;
					case ElementType.DialogGroup:
						w.WriteLine("<div class=\"dialog-group\">");
						DialogGroupElement grp = (DialogGroupElement)item;
						
						w.Write("<div class=\"dialog-character\">");
						w.Write(HtmlEncode(grp.Character));
						w.WriteLine("</div>");
						
						if (!string.IsNullOrWhiteSpace(grp.Parenthetical)) {
							w.Write("<div class=\"dialog-character\">");
							w.Write(HtmlEncode(grp.Character));
							w.WriteLine("</div>");
						}
						
						w.Write("<div class=\"dialog-text\">");
						w.Write(HtmlEncode(grp.Dialog));
						w.WriteLine("</div>");
						
						w.WriteLine("</div>");
						break;
					case ElementType.Action:
						w.Write("<div class\"action\">");
						w.Write(HtmlEncode(((ActionElement)item).Content));
						w.WriteLine("</div>");
						break;
					case ElementType.Boneyard:
						w.Write("<!--");
						w.Write(HtmlEncode(((BoneyardElement)item).Value));
						w.WriteLine("-->");
						break;
					case ElementType.Transition:
						w.Write("<div class\"transition\">");
						w.Write(HtmlEncode(((TransitionElement)item).Value));
						w.WriteLine("</div>");
						break;
					case ElementType.CenteredText:
						w.Write("<div class\"centered\">");
						w.Write(HtmlEncode(((CenteredTextElement)item).Content));
						w.WriteLine("</div>");
						break;
					case ElementType.LineBreak:
						w.WriteLine("<br class=\"line-break\" />");
						break;
					case ElementType.NoteBlock:
						w.Write("<div class\"note\" style=\"display: none\">");
						w.Write(HtmlEncode(((NoteBlockElement)item).Value));
						w.WriteLine("</div>");
						break;
					case ElementType.Section:
						w.Write("<div class\"section\">");
						w.Write(HtmlEncode(((SectionElement)item).Value));
						w.WriteLine("</div>");
						break;
					case ElementType.Synopsis:
						w.Write("<div class\"synopsis\" style=\"display: none\">");
						w.Write(HtmlEncode(((SynopsisElement)item).Value));
						w.WriteLine("</div>");
						break;
					case ElementType.Title:
						w.WriteLine("<div class=\"title-group\">");
						foreach (var kvp in ((TitleElement)item).Parts) {
							w.Write("<div class=\"title-part\">");
							w.Write(HtmlEncode(kvp.Value));
							w.WriteLine("</div>");
						}
						w.WriteLine("</div>");
						break;
					default:
						throw new Exception("Invalid value for ElementType");
				}
			}
			
			
			w.Write(footer);
			w.Flush();
		}
	}
}
