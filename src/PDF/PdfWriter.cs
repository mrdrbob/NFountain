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
using System.Configuration;
using System.IO;
using System.Linq;
using PageOfBob.NFountain.PDF;

namespace PageOfBob.NFountain.Plugins
{
	public class PdfWriter {
		public bool ShowBoneyards { get; set; }
		public bool ShowNotes { get; set; }
		public bool ShowSectionHeadings { get; set; }
		
		private float _posY = 0;
		private int _pageNumber = 0;
		
		private Compositor.PageBuilder _page;
		FontIdentifier _courier;
		FontIdentifier _courierBold;
		FontIdentifier _courierItalic;
		FontIdentifier _courierBoldItalic;
		internal PageSettings _settings = new PageSettings();
		
		public void Transform(IEnumerable<Element> elements, Stream output) {
			Compositor compositor = new Compositor();
			var root = compositor.RootPageCollection
				.MediaBox(0, 0, (int)PageSettings.InPoints(_settings.PageWidth), (int)PageSettings.InPoints(_settings.PageHeight))
				.AddResources()
					.AddSimpeType1Font("Courier", out _courier)
					.AddSimpeType1Font("Courier-Bold", out _courierBold)
					.AddSimpeType1Font("Courier-Oblique", out _courierItalic)
					.AddSimpeType1Font("Courier-BoldOblique", out _courierBoldItalic)
				.End();
			
			_page = NewPage(root);
			
			WriteTitleElement(elements.FirstOrDefault() as TitleElement);
			
			foreach (var element in elements) {
				bool found = Write(element as DialogGroupElement)
					|| Write(element as ActionElement)
					|| Write(element as HeadingElement)
					|| Write(element as TransitionElement)
					|| Write(element as CenteredTextElement)
					|| Write(element as BoneyardElement)
					|| Write(element as NoteBlockElement)
					|| Write(element as SectionElement);
				
				if (!found)
					Console.WriteLine("NOT FOUND: {0}", element.Type);
			}
			
			TextWriter w = new StreamWriter(output);
			PdfOutput pdfOutput = new PdfOutput(w);
			pdfOutput.WriteCompositor(compositor);
			w.Flush();
		}
		
		private void WriteTitleElement(TitleElement titleElement) {
			if (titleElement == null)
				return;
			
			var title 	= titleElement.Parts.FirstOrDefault(x => x.Key.ToLowerInvariant() == "title");
			var author 	= titleElement.Parts.FirstOrDefault(x => x.Key.ToLowerInvariant() == "author");
			var addy 	= titleElement.Parts.FirstOrDefault(x => x.Key.ToLowerInvariant() == "address");
			
			if (title.Equals(default(KeyValuePair<string, string>)))
				return;
			
			MoveDownInches(_settings.PageTitleDistanceFromTop);
			_page.SetFont(_courierBold, _settings.FontSize);
			WriteCenteredLines(title.Value);
			_page.SetFont(_courier, _settings.FontSize);
			EmptyLine();
			
			if (author.Equals(default(KeyValuePair<string, string>))) {
				MoveDown(4);
			} else {
				EmptyLine();
				WriteCenteredLine("Written by");
				EmptyLine();
				WriteCenteredLines(author.Value);
			}
			
			if (!addy.Equals(default(KeyValuePair<string, string>))) {
				string[] split = addy.Value.Split(new char[] { '\n', '\r' }).Where(x => !string.IsNullOrEmpty(x)).ToArray();
				
				float distFromBottom = PageSettings.InPoints(_settings.BottomMargin) + (split.Length * _settings.EffectiveCharHeight) + PageSettings.InPoints(_settings.AddressDistanceFromBottomMargin);
				float delta = distFromBottom - _posY;

				MoveDownPoints(delta);
				
				IndentInches(_settings.AddressIndent);
				foreach (string str in split) {
					WriteLine(str);
				}
				// Not necessary, since we're starting a new page after this.
				// ChangeIndent(-_settings.AddressIndentCharCount);
			}
			
			/*
			var otherItems = titleElement.Parts.Where(x => x.Key.ToLowerInvariant() != "title" && x.Key.ToLowerInvariant() != "author");
			if (otherItems.Any()) {
				MoveDownInches(_settings.AddressDistanceFromTitle);
				IndentInches(_settings.AddressIndent);
				foreach (var part in otherItems) {
					foreach (string str in part.Value.Split(new char[] { '\n', '\r' })) {
						if (string.IsNullOrEmpty(str))
							continue;
						WriteLine(str);
					}
				}
			}
			*/
			
			_pageNumber--;
			NewPage();
		}
		
		private bool Write(ActionElement action) {
			if (action == null)
				return false;
			
			/*
			string[] lines = BreakIntoLines(action.Content.ToString(), _settings.WritablePageWidth());
			if (!HaveRoomFor(lines.Length))
				NewPage();
			*/
			
			
			WriteContentNode(action.Content, PageSettings.InPoints(_settings.WritablePageWidth()), x => 0, l => {
				if(l > 0) {
					MoveDown(1);
					WriteRightAlignedLine("(CONTINUED)");
				}
				NewPage();
			});
			
			/*
			foreach (string line in lines) {
				WriteLine(line);
			}
			EmptyLine();
			*/
			
			return true;
		}
		
		private bool Write(HeadingElement header) {
			if (header == null)
				return false;
			
			if (!HaveRoomFor(2))
				NewPage();
			
			WriteLine(header.Value.ToUpperInvariant());
			EmptyLine();
			return true;
		}
		
		private bool Write(DialogGroupElement dialog) {
			if (dialog == null)
				return false;
			
			string[] charLine = BreakIntoLines(dialog.Character, _settings.CharacterWidth);
			string[] parenthetical = string.IsNullOrEmpty(dialog.Parenthetical) ? new string[0] : BreakIntoLines(dialog.Parenthetical, _settings.ParentheticalWidth);
			string[] dialogtext = BreakIntoLines(dialog.Dialog.ToString(), _settings.DialogWidth);
			
			int totalLines = charLine.Length + parenthetical.Length + 3;
			
			if (!HaveRoomFor(totalLines) && dialogtext.Length <= 2) {
				NewPage();
			}
			
			IndentInches(_settings.CharacterIndent);
			foreach (string line in charLine) {
				WriteLine(line);
			}
			if (parenthetical.Length > 0) {
				IndentInches(-_settings.CharacterIndent + _settings.ParentheticalIndent);
				for(int x=0, m = parenthetical.Length; x<m; x++) {
					WriteLine( (x == 0 ? "(" : "")
						+ parenthetical[x]
						+ (x == m - 1 ? ")" : "")
					);
				}
				IndentInches(-_settings.ParentheticalIndent);
			} else {
				IndentInches(-_settings.CharacterIndent);
			}
			
			WriteContentNode(dialog.Dialog, PageSettings.InPoints(_settings.DialogWidth), (x) => PageSettings.InPoints(_settings.DialogIndent), l => {
             	if (l > 0) {
             		MoveDown(1);
             		WriteRightAlignedLine("(CONTINUED)");
             		NewPage();
             		IndentInches(_settings.CharacterIndent);
					foreach (string line in charLine) {
						WriteLine(line);
					}
             		IndentInches(-_settings.CharacterIndent + _settings.ParentheticalIndent);
             		WriteLine("(CONT'D)");
             		IndentInches(-_settings.ParentheticalIndent);
             	}
			});
			
			/*
			foreach (string line in dialogtext) {
				WriteLine(line);
			}
			IndentInches(-_settings.DialogIndent);
			*/
			
			EmptyLine();
			
			return true;
		}
		
		private bool Write(TransitionElement trans) {
			if (trans == null)
				return false;
			
			if (!HaveRoomFor(2))
				NewPage();
			
			string[] lines = BreakIntoLines(trans.Value, _settings.WritablePageWidth());
			foreach (string line in lines) {
				WriteRightAlignedLine(line.ToUpperInvariant());
			}
			
			EmptyLine();
			
			return true;
		}
		
		private bool Write(CenteredTextElement centered) {
			if (centered == null)
				return false;
			
			float widthInPoints = PageSettings.InPoints(_settings.WritablePageWidth());
			
			string[] lines = BreakIntoLines(centered.Content.ToString(), widthInPoints);
			if (!HaveRoomFor(lines.Length))
				NewPage();
			
			WriteContentNode(centered.Content, widthInPoints, (x) =>  (widthInPoints - x) / 2, null );
			
			EmptyLine();
			
			return true;
		}
		
		private bool Write(BoneyardElement boneyard) {
			if (boneyard == null)
				return false;
			
			if (!ShowBoneyards)
				return true;
			
			var lines = boneyard.Value.Trim()
				.Replace("\r\n", "\n").Replace('\r','\n')
				.Replace("\n\n", "\r")
				.Replace("\n", "")
				.Split('\r');
			
			_page.SetFont(_courierItalic, _settings.FontSize)
				/* .SetGrayscale(0.9f) */ ;
			foreach (string line in lines) {
				if (line.Length == 0) {
					EmptyLine();
				} else {
					string[] sublines = BreakIntoLines(line, _settings.WritablePageWidth());
					foreach (string subline in sublines) {
						if (!HaveRoomFor(1))
							NewPage();
						WriteLine(subline);
					}
					EmptyLine();
				}
			}
			
			_page.SetFont(_courier, _settings.FontSize)
				/* .SetGrayscale(0f) */ ;
			
			return true;
		}
		
		private bool Write(NoteBlockElement notes) {
			if (notes == null)
				return false;
			
			if (!ShowNotes)
				return true;
			
			var lines = notes.Value.Trim()
				.Replace("\r\n", "\n").Replace('\r','\n')
				.Replace("\n\n", "\r")
				.Replace("\n", "")
				.Split('\r');
			
			_page.SetFont(_courierItalic, _settings.FontSize);
			foreach (string line in lines) {
				if (!HaveRoomFor(1))
					NewPage();
				
				if (line.Length == 0) {
					EmptyLine();
				} else {
					string[] sublines = BreakIntoLines(line, _settings.WritablePageWidth());
					foreach (string subline in sublines) {
						WriteLine(subline);
						EmptyLine();
					}
				}
			}
			
			_page.SetFont(_courier, _settings.FontSize);
			
			return true;
		}
		
		private bool Write(SectionElement section) {
			if (section == null)
				return false;
			
			float sizeChange = ((((2f - section.Depth) / 2f) * 0.5f) + 1f);
			
			if (!ShowSectionHeadings)
				return true;
			
			string[] lines = BreakIntoLines(section.Value, _settings.EffectiveCharWidth * sizeChange , _settings.WritablePageWidth());
			if (!HaveRoomFor(lines.Length))
				NewPage();
			
			_page.SetFont(_courierBold, _settings.FontSize * sizeChange);
			foreach (string line in lines) {
				WriteLine(line); MoveDown(-1);
				MoveDownPoints(-_settings.EffectiveCharHeight * sizeChange);
			}
			
			_page.SetFont(_courier, _settings.FontSize);
			EmptyLine();
			
			return true;
		}
		
		private void WriteLine(string text) {
			_page.WriteText(text);
			EmptyLine();
		}
		
		private void EmptyLine() {
			MoveDown(1);
		}
		
		private void WriteRightAlignedLine(string text) {
			float effectiveWidth = text.Length * _settings.EffectiveCharWidth;
			float pageWidth = PageSettings.InPoints(_settings.WritablePageWidth());
			float leftIndent = (pageWidth - effectiveWidth);
			_page.NextLine(leftIndent, 0);
			WriteLine(text);
			_page.NextLine(-leftIndent, 0);
		}
		
		private void WriteCenteredLines(string text) {
			string[] split = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
			foreach (string line in split) {
				WriteCenteredLine(line);
			}
		}
		private void WriteCenteredLine(string text) {
			float effectiveWidth = text.Length * _settings.EffectiveCharWidth;
			float pageWidth = PageSettings.InPoints(_settings.WritablePageWidth());
			float leftIndent = (pageWidth - effectiveWidth) / 2f;
			_page.NextLine(leftIndent, 0);
			WriteLine(text);
			_page.NextLine(-leftIndent, 0);
		}
		
		public class WordAndFontLine {
			public WordAndFont[] Line { get; private set; }
			public float Indent { get; private set; }
			
			public WordAndFontLine(WordAndFont[] line, float indent) {
				Line = line;
				Indent = indent;
			}
		}
		
		private IEnumerable<WordAndFontLine> ContentNodeToLines(ContentNode node, float widthToFill, Func<float, float> calculateIndent) {
			List<WordAndFont> buffer = new List<WordAndFont>();
			float currentLength = 0;
			var liner = node.Linearize().GetEnumerator();
			
			do {
				bool hasMore = liner.MoveNext();
				float toAdd = hasMore ? liner.Current.Word.Length * _settings.EffectiveCharWidth : 0;
				bool isTooLong = hasMore && (currentLength + toAdd > widthToFill);
				
				if (!hasMore || isTooLong) {
					// Dump the buffer.
					float indent = calculateIndent(currentLength);
					yield return new WordAndFontLine(buffer.ToArray(), indent);
					buffer.Clear();
					currentLength = 0;
				}
				
				if (!hasMore)
					break;
				
				buffer.Add(liner.Current);
				currentLength += toAdd + _settings.EffectiveCharWidth;
			} while(true);
		}
		
		private void WriteContentNode(ContentNode node, float widthToFill, 
		                              Func<float, float> calculateIndent,
		                             Action<int> notEnoughRoom) {
			var lines = ContentNodeToLines(node, widthToFill, calculateIndent).ToArray();
			int linesWritten = 0;
			
			for(int x=0; x<lines.Length; x++) {
				if (!HaveRoomFor(2) && lines.Length > 2) {
					if (notEnoughRoom == null)
						NewPage();
					else
						notEnoughRoom(linesWritten);
				}
				
				WriteContentLine(lines[x]);
				MoveDown(1);
				linesWritten += 1;
			}
			MoveDown(1);
		}
		
		private void WriteContentLine(WordAndFontLine line) {
			FontStyle currentFont = FontStyle.Plain;
			
			_page.NextLine(line.Indent, 0);
			foreach(var i in line.Line) {
				if (i.Style != currentFont) {
					switch (i.Style) {
						case FontStyle.Bold: _page.SetFont(_courierBold, _settings.FontSize); break;
						case FontStyle.Italic: _page.SetFont(_courierItalic, _settings.FontSize); break;
						case FontStyle.BoldItalic: _page.SetFont(_courierBoldItalic, _settings.FontSize); break;
						case FontStyle.Plain: _page.SetFont(_courier, _settings.FontSize); break;
					}
					currentFont = i.Style;
				}
				_page.WriteText(i.Word + " ");
			}
			_page.NextLine(-line.Indent, 0);
			
			if (currentFont != FontStyle.Plain)
				_page.SetFont(_courier, _settings.FontSize);
		}
		
		/*
		private void WriteContentNode(ContentNode node, float widthToFill, Func<float, float> calculateIndent) {
			List<WordAndFont> buffer = new List<WordAndFont>();
			float currentLength = 0;
			var liner = node.Linearize().GetEnumerator();
			FontStyle currentFont = FontStyle.Plain;
			
			do {
				bool hasMore = liner.MoveNext();
				float toAdd = hasMore ? liner.Current.Word.Length * _settings.EffectiveCharWidth : 0;
				bool isTooLong = hasMore && (currentLength + toAdd > widthToFill);
				
				if (!hasMore || isTooLong) {
					// Dump the buffer.
					float indent = calculateIndent(currentLength);
					_page.NextLine(indent, 0);
					foreach(var i in buffer) {
						if (i.Style != currentFont) {
							switch (i.Style) {
								case FontStyle.Bold: _page.SetFont(_courierBold, _settings.FontSize); break;
								case FontStyle.Italic: _page.SetFont(_courierItalic, _settings.FontSize); break;
								case FontStyle.BoldItalic: _page.SetFont(_courierBoldItalic, _settings.FontSize); break;
								case FontStyle.Plain: _page.SetFont(_courier, _settings.FontSize); break;
							}
							currentFont = i.Style;
						}
						_page.WriteText(i.Word + " ");
					}
					_page.NextLine(-indent, 0);
					
					MoveDown(1);
					buffer.Clear();
					currentLength = 0;
				}
				
				if (!hasMore)
					break;
				
				buffer.Add(liner.Current);
				currentLength += toAdd + _settings.EffectiveCharWidth;
			} while(true);
			
			MoveDown(1);
			
			if (currentFont != FontStyle.Plain)
				_page.SetFont(_courier, _settings.FontSize);
		}
		*/
		
		private void MoveDown(int lines) {
			float distance = (float)-lines * _settings.EffectiveCharHeight;
			_page.NextLine(0, distance);
			_posY += distance;
		}
		
		private void MoveDownInches(float inches) {
			float distance = -PageSettings.InPoints(inches);
			_page.NextLine(0, distance);
			_posY += distance;
		}
		
		private void MoveDownPoints(float points) {
			_page.NextLine(0, points);
			_posY += points;
		}
		
		private void IndentInches(float inches) {
			float distance = PageSettings.InPoints(inches);
			_page.NextLine(distance, 0);
		}
		
		private string[] BreakIntoLines(string text, float charWidth, float inches) {
			float totalPoints = PageSettings.InPoints(inches);
			int charsPerLine = (int)(totalPoints / charWidth);
			return DefaultWriter.BreakLines(text, charsPerLine).ToArray();
		}
		
		private string[] BreakIntoLines(string text, float inches) {
			return BreakIntoLines(text, _settings.EffectiveCharWidth, inches);
		}
		
		private bool HaveRoomFor(int lines) {
			return (_posY - (lines * _settings.EffectiveCharHeight)) > PageSettings.InPoints(_settings.BottomMargin);
		}
		
		private Compositor.PageBuilder NewPage() {
			return NewPage(_page.End());
		}
		
		private Compositor.PageBuilder NewPage(Compositor.PageCollectionBuilder builder) {
			_posY = PageSettings.InPoints(_settings.PageHeight) - PageSettings.InPoints(_settings.TopMargin);
			
			_page = builder.AddPage()
				.SetFont(_courier, _settings.FontSize)
				.SetMatrix(1, 0, 0, 1, PageSettings.InPoints(_settings.LeftMargin), PageSettings.InPoints(_settings.PageHeight) - PageSettings.InPoints(_settings.TopMargin));
			
			_pageNumber++;
			
			if (_pageNumber > 1) {
				MoveDownInches(-_settings.PageNumberDistanceTop);
				EmptyLine();
				WriteRightAlignedLine(_pageNumber.ToString() + ".");
				MoveDown(-2);
				MoveDownInches(_settings.PageNumberDistanceTop);
			}
			
			return _page;
		}
	}
}
