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

namespace PageOfBob.NFountain
{
	public class DefaultParser {
		
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
		public Parser<string> TabsAndSpaces;
		#endregion

		#region Heading
		public Parser<string> SimpleHeaderStart;
		public Parser<HeadingElement> SimpleHeader;
		public Parser<HeadingElement> ForcedHeader;
		public Parser<HeadingElement> Header;
		#endregion

		#region Dialog
		public Parser<string> Character;
		public Parser<string> Parenthetical;
		public Parser<string> Dialog;
		public Parser<DialogGroupElement> DialogGroup;
		#endregion

		#region Transition
		public Parser<TransitionElement> ToTransition;
		public Parser<TransitionElement> InTransition;
		public Parser<TransitionElement> OutTransition;
		public Parser<TransitionElement> ForcedTransition;
		public Parser<TransitionElement> Transition;
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
		public Parser<KeyValuePair<string,string>> SimpleTitlePart;
		public Parser<KeyValuePair<string,string>> MultilineTitlePart;
		public Parser<TitleElement> Title;
		#endregion

		#region ContentNode
		public Parser<string> EscapedContentChar;
		public Parser<string> NonEscapedContentChar;
		public Parser<ContentNode> PlainText;
		public Parser<ContentNode> Underline;
		public Parser<ContentNode> BoldItalic;
		public Parser<ContentNode> Bold;
		public Parser<ContentNode> Italic;
		public Parser<ContentNode> ContentNodePiece;
		public Parser<ContentNode> ContentNodeBody;
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
			TabsAndSpaces = 
				from tabs in Parse.Char(x => x == '\t' || x == ' ', "tabs or spaces").Many().Text()
				select tabs;
			
			IndentedLine =
				from c in Parse.String("   ").Or(Parse.String("\t"))
				from ws in TabsAndSpaces
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
						   from notdot in Parse.AnyChar.Except(Parse.Char('.'))
						   from rest in UntilEOL
						   from nl in EmptyLine
						   select new HeadingElement(notdot.ToString() + rest);

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

			#region Transition
			ToTransition =
				from line in UpperCase.Except(Parse.String("TO:")).Many().Text()
				from to in Parse.String("TO:").Text()
				from nl in EmptyLine
				select new TransitionElement(line + "TO:");
			
			InTransition =
				from line in UpperCase.Except(Parse.String("IN:")).Many().Text()
				from to in Parse.String("IN:").Text()
				from nl in EmptyLine
				select new TransitionElement(line + "IN:");
			
			OutTransition =
				from line in UpperCase.Except(Parse.String("OUT:")).Many().Text()
				from to in Parse.String("OUT:").Text()
				from nl in EmptyLine
				select new TransitionElement(line + "OUT:");

			ForcedTransition =
				from gt in Parse.Char('>')
				from line in UntilEOL
				from nl in EmptyLine
				select new TransitionElement(line);

			Transition =
				from trans in ForcedTransition
					.Or(ToTransition)
					.Or(InTransition)
					.Or(OutTransition)
				select trans;
			#endregion

			#region CenteredText
			CenteredText =
				from o in Parse.Char('>')
				from content in Parse.Char(x => x != '<', "Not LT").Except(Newline).Many().Text()
				from c in Parse.Char('<')
				from nl in EmptyLine
				select new CenteredTextElement(ContentNodeBody.Parse(content.Trim()));
			#endregion

			#region Dialog
			Character = 
				from ws in TabsAndSpaces
				from c in UpperCaseLine
				select c;

			Parenthetical = 
				from ws in TabsAndSpaces
				from s in Parse.Char('(')
				from m in Parse.Char(x => x != ')', "Not closing paren").AtLeastOnce().Text()
				from e in Parse.Char(')')
				from nl in Newline
				select m;

			Dialog =
				from ws in TabsAndSpaces
				from line in CompleteLine.AtLeastOnce()
				from nl in Newline
				select string.Join(" ", line);

			DialogGroup =
				from character in Character
				from paren in Parenthetical.Or(Parse.Return<string>(null))
				from diag in Dialog
				select new DialogGroupElement(character, paren, ContentNodeBody.Parse(diag));
			#endregion

			#region Action
			Action =
				from line in CompleteLine.Many()
				from nl in Newline
				select new ActionElement( ContentNodeBody.Parse(string.Join(" ", line.ToArray())) );
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
				from nls in Newline.Many()
				select new NoteBlockElement(content);

			Section =
				from start in Parse.Char('#').AtLeastOnce()
				from ws in Parse.WhiteSpace.Many()
				from line in UntilEOL
				from nl in EmptyLine
				select new SectionElement(start.Count(), line);

			Synopsis =
				from start in Parse.Char('=')
				from ws in Parse.WhiteSpace.Many()
				from content in CompleteLine.AtLeastOnce()
				select new SynopsisElement(string.Join(" ", content.ToArray()));
			#endregion

			#region Title
			SimpleTitlePart =
				from key in Parse.Char(x => x != ':' && x != '\r' && x != '\n', "Not : or newline").AtLeastOnce().Text()
				from colon in Parse.Char(':')
				from ws in TabsAndSpaces
				from value in CompleteLine
				select new KeyValuePair<string, string>(key, value);

			MultilineTitlePart =
				from key in Parse.Char(x => x != ':' && x != '\r' && x != '\n', "Not : or newline").AtLeastOnce().Text()
				from colon in Parse.Char(':')
				from ws in TabsAndSpaces
				from nl in Newline
				from lines in IndentedLine.AtLeastOnce()
				select new KeyValuePair<string, string>(key, string.Join(nl, lines.ToArray()));

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
					.Or<Element>(Transition)
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
			
			#region ContentNode
			EscapedContentChar = 
				from slash in Parse.Char('\\')
				from chr in Parse.Char(x => x == '\\' || x == '*' || x == '_', "slash, star, underscore")
				select chr.ToString();
			
			NonEscapedContentChar = 
				from txt in Parse.Char(x => x != '\\' && x != '*' && x != '_', "not slash, star, underscore").AtLeastOnce().Text()
				select txt;
			
			PlainText = 
				from vals in EscapedContentChar.Or(NonEscapedContentChar).AtLeastOnce()
				select new ContentNode(ContentNodeType.Text, string.Join("", vals.ToArray()), null);
			
			Underline = 
				from o in Parse.Char('_')
				from body in BoldItalic.Or(Bold).Or(Italic).Or(PlainText).AtLeastOnce()
				from c in Parse.Char('_')
				select new ContentNode(ContentNodeType.Underline, null, body.ToArray());
			
			BoldItalic = 
				from o in Parse.String("***")
				from body in Bold.Or(Italic).Or(Underline).Or(PlainText).AtLeastOnce()
				from c in Parse.String("***")
				select new ContentNode(ContentNodeType.BoldItalic, null, body.ToArray());
			
			Bold = 
				from o in Parse.String("**")
				from body in BoldItalic.Or(Italic).Or(Underline).Or(PlainText).AtLeastOnce()
				from c in Parse.String("**")
				select new ContentNode(ContentNodeType.Bold, null, body.ToArray());
			
			Italic = 
				from o in Parse.String("*")
				from body in BoldItalic.Or(Bold).Or(Underline).Or(PlainText).AtLeastOnce()
				from c in Parse.String("*")
				select new ContentNode(ContentNodeType.Italic, null, body.ToArray());
			
			ContentNodePiece =
				from val in BoldItalic.Or(Bold).Or(Italic).Or(Underline).Or(PlainText)
				select val;
			
			ContentNodeBody = 
				from body in ContentNodePiece.Many()
				select new ContentNode(ContentNodeType.Container, null, body.ToArray());
			#endregion
		}
	}
}
