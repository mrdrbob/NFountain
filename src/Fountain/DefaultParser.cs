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

using Sprache;

using PageOfBob.NFountain.SDK;

namespace PageOfBob.NFountain
{
	public class DefaultParser : PageOfBob.NFountain.SDK.IInputModule {
		
		public DefaultParser() {
			CreateParser();
		}

		#region Newlines / Text
		public Parser<string> WinNewline;
		public Parser<string> UnixNewline;
		public Parser<string> MacNewline;
		public Parser<string> Newline;
		public Parser<string> EmptyLine;
		public Parser<char> NotNewline;
		public Parser<string> UntilEOL;
		public Parser<string> CompleteLine;
		public Parser<char> UpperCase;
		public Parser<string> UpperCaseLine;
		public Parser<string> IndentedLine;
		#endregion

		#region Heading
		public Parser<string> SimpleHeaderStart;
		public Parser<HeadingElement> SimpleHeader;
		public Parser<HeadingElement> ForcedHeader;
		public Parser<HeadingElement> Header;
		#endregion

		#region Dialog
		public Parser<CharacterElement> Character;
		public Parser<ParentheticalElement> Parenthetical;
		public Parser<DialogElement> Dialog;
		// public Parser<DialogGroupElement> DialogGroupWithParen;
		// public Parser<DialogGroupElement> DialogGroupWithoutParen;
		public Parser<DialogGroupElement> DialogGroup;
		#endregion

		#region Transaction
		public Parser<TransactionElement> NaturalTransaction;
		public Parser<TransactionElement> ForcedTransaction;
		public Parser<TransactionElement> Transaction;
		#endregion

		#region Centered Text
		public Parser<CenteredTextElement> CenteredText;
		#endregion

		#region Action
		public Parser<ActionElement> Action;
		#endregion

		#region Boneyard
		public Parser<BoneyardElement> Boneyard;
		#endregion

		#region Collection
		public Parser<Element> Element;
		public Parser<IEnumerable<Element>> BodyWithoutTitle;
		public Parser<IEnumerable<Element>> BodyWithTitle;
		public Parser<IEnumerable<Element>> Elements;
		#endregion

		#region Linebreak / Noteblock / Section / Synopsis
		public Parser<LineBreakElement> LineBreak;
		public Parser<NoteBlockElement> NoteBlock;
		public Parser<SectionElement> Section;
		public Parser<SynopsisElement> Synopsis;
		#endregion

		#region Title
		public Parser<TitlePartElement> SimpleTitlePart;
		public Parser<TitlePartElement> MultilineTitlePart;
		public Parser<TitleElement> Title;
		#endregion

		private void CreateParser() {
			#region Newlines / Text
			WinNewline = from c in Parse.String("\r\n").Text()
						 select c;
			UnixNewline = from c in Parse.String("\n").Text()
						 select c;
			MacNewline = from c in Parse.String("\r").Text()
						 select c;
			Newline = from c in WinNewline.Or(UnixNewline).Or(MacNewline)
					  select c;
			EmptyLine = from first in Newline
						from second in Newline
						select first + second;
			NotNewline = from c in Parse.Char(x => x != '\r' && x != '\n', "Not newline")
						 select c;
			UntilEOL = from c in NotNewline.AtLeastOnce().Text()
					   select c;
			CompleteLine =
				from l in UntilEOL
				from nl in Newline
				select l;

			UpperCase = from c in Parse.Char(x => x != '\r' && x != '\n' && Char.ToUpperInvariant(x) == x, "Not newline and same upper as lower")
						select c;
			UpperCaseLine = from c in UpperCase.AtLeastOnce().Text()
							from nl in Newline
							select c;
			IndentedLine =
				from c in Parse.String("   ").Or(Parse.String("\t"))
				from ws in Parse.Char(x => x == ' ' || x == '\t', "space or tab").Many()
				from line in CompleteLine
				select line;
			#endregion

			#region Heading
			SimpleHeaderStart = from c in ParseExtensions.StringIgnoreCase("INT")
									.Or(ParseExtensions.StringIgnoreCase("EXT"))
									.Or(ParseExtensions.StringIgnoreCase("EST"))
									.Or(ParseExtensions.StringIgnoreCase("INT./EXT"))
									.Or(ParseExtensions.StringIgnoreCase("INT/EXT"))
									.Or(ParseExtensions.StringIgnoreCase("I/E"))
									.Text()
								select c;

			SimpleHeader = from c in SimpleHeaderStart
						   from rest in UntilEOL
						   from emptyline in EmptyLine
						   select new HeadingElement(c + rest);

			ForcedHeader = from c in Parse.Char('.')
						   from rest in UntilEOL
						   from nl in EmptyLine
						   select new HeadingElement(rest);

			Header = from header in SimpleHeader
					 .Or(ForcedHeader)
					 select header;
			#endregion

			#region Boneyard
			Boneyard =
				from start in Parse.String("/*")
				from content in Parse.AnyChar.Except(Parse.String("*/")).Many().Text()
				from end in Parse.String("*/")
				select new BoneyardElement(content);
			#endregion

			#region Transaction
			NaturalTransaction =
				from line in UpperCase.Except(Parse.String("TO:")).Many().Text()
				from to in Parse.String("TO:").Text()
				from nl in EmptyLine
				select new TransactionElement(line);

			ForcedTransaction =
				from gt in Parse.Char('>')
				from line in UntilEOL
				from nl in EmptyLine
				select new TransactionElement(line);

			Transaction =
				from trans in ForcedTransaction.Or(NaturalTransaction)
				select trans;
			#endregion

			#region CenteredText
			CenteredText =
				from o in Parse.Char('>')
				from content in Parse.Char(x => x != '<', "Not LT").Except(Newline).Many().Text()
				from c in Parse.Char('<')
				from nl in EmptyLine
				select new CenteredTextElement(content.Trim() + nl);
			#endregion

			#region Dialog
			Character = 
				from c in UpperCaseLine
				select new CharacterElement(c);

			Parenthetical = 
				from s in Parse.Char('(')
				from m in Parse.Char(x => x != ')', "Not closing paren").AtLeastOnce().Text()
				from e in Parse.Char(')')
				from nl in Newline
				select new ParentheticalElement("(" + m + ")" + nl);

			Dialog =
				from line in CompleteLine.Many()
				from nl in Newline
				select new DialogElement(string.Join(" ", line));

			DialogGroup =
				from character in Character
				from paren in Parenthetical.Or(Parse.Return<ParentheticalElement>(null))
				from diag in Dialog
				select new DialogGroupElement(character, paren, diag);
			#endregion

			#region Action
			Action =
				from line in CompleteLine.Many()
				from nl in Newline
				select new ActionElement(string.Join(" ", line.ToArray()));
			#endregion

			#region Linebreak / Noteblock / Section / Synopsis
			LineBreak =
				from brk in Parse.String("===")
				from nl in EmptyLine
				select new LineBreakElement();

			NoteBlock =
				from start in Parse.String("[[")
				from content in Parse.AnyChar.Except(Parse.String("]]")).Many().Text()
				from end in Parse.String("]]")
				from nl in EmptyLine
				select new NoteBlockElement(content);

			Section =
				from start in Parse.Char('#').AtLeastOnce()
				from ws in Parse.WhiteSpace.Many()
				from line in UntilEOL
				from nl in EmptyLine
				select new SectionElement(start.Count(), line + nl);

			Synopsis =
				from start in Parse.Char('=')
				from ws in Parse.WhiteSpace.Many()
				from content in CompleteLine.AtLeastOnce()
				from nl in Newline
				select new SynopsisElement(string.Join(" ", content.ToArray()));
			#endregion

			#region Title
			SimpleTitlePart =
				from key in Parse.Char(x => x != ':' && x != '\r' && x != '\n', "Not : or newline").AtLeastOnce().Text()
				from colon in Parse.Char(':')
				from ws in Parse.Char(x => x == ' ' || x == '\t', "space or tab").Many()
				from value in CompleteLine
				select new TitlePartElement(key, value);

			MultilineTitlePart =
				from key in Parse.Char(x => x != ':' && x != '\r' && x != '\n', "Not : or newline").AtLeastOnce().Text()
				from colon in Parse.Char(':')
				from ws in Parse.Char(x => x == ' ' || x == '\t', "Space or tab").Many()
				from nl in Newline
				from lines in IndentedLine.AtLeastOnce()
				select new TitlePartElement(key, string.Join(nl, lines.ToArray()));

			Title =
				from parts in SimpleTitlePart.Or(MultilineTitlePart).AtLeastOnce()
				from nl in Newline
				select new TitleElement(parts.ToArray());
			#endregion

			#region Collection
			Element =
				from element in Boneyard
					.Or<Element>(NoteBlock)
					.Or<Element>(LineBreak)
					.Or<Element>(Section)
					.Or<Element>(Header)
					.Or<Element>(Synopsis)
					.Or<Element>(CenteredText)
					.Or<Element>(Transaction)
					.Or<Element>(DialogGroup)
					.Or<Element>(Action)
				from ws in Newline.Many()
				select element;

			BodyWithoutTitle = 
				from el in Element.AtLeastOnce()
				select el;

			BodyWithTitle =
				from title in Title.Once<Element>()
				from el in Element.AtLeastOnce()
				select title.Concat(el);

			Elements =
				from el in BodyWithTitle.Or(BodyWithoutTitle)
				select el;
			#endregion
		}
		
		public IEnumerable<Element> Transform(Stream str) {
			TextReader r = new StreamReader(str);
			string fullText = r.ReadToEnd();
			
			return Elements.Parse(fullText);
		}
	}
}
