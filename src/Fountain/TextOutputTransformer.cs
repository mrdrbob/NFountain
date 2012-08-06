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

using PageOfBob.NFountain.SDK;

namespace PageOfBob.NFountain {
	public class TextOutputTransformer : IOutputModule {
		
		public void Transform(IEnumerable<Element> elements, Stream output) {
			TextWriter writer = new StreamWriter(output);
			
			foreach (Element element in elements) {
				switch (element.Type) {
					case ElementType.None: break;
					case ElementType.Heading:
						writer.WriteLine(((HeadingElement)element).Value);
			            writer.WriteLine();
						break;
					case ElementType.Character:
						writer.Write("\t\t\t");
						writer.WriteLine(((CharacterElement)element).Value);
						break;
					case ElementType.Parenthetical:
						writer.Write("\t\t\t(");
						writer.Write(((CharacterElement)element).Value);
						writer.WriteLine(")");
						break;
					case ElementType.Dialog:
						writer.Write("\t\t");
						writer.WriteLine(((DialogElement)element).Value);
						writer.WriteLine();
						break;
					case ElementType.DialogGroup:
						DialogGroupElement groupElement = (DialogGroupElement)element;
						writer.Write("\t\t\t");
						writer.WriteLine(groupElement.Character.Value);
						if (groupElement.Parenthetical != null) {
							writer.Write("\t\t\t(");
							writer.Write(groupElement.Parenthetical.Value);
							writer.WriteLine(")");
						}
						writer.Write("\t\t");
						writer.WriteLine(groupElement.Dialog.Value);
						writer.WriteLine();
						break;
					case ElementType.Action:
						writer.WriteLine(((ActionElement)element).Value);
						writer.WriteLine();
						break;
					case ElementType.Boneyard: break;
					case ElementType.Transaction:
						writer.WriteLine(((TransactionElement)element).Value);
						writer.WriteLine();
						break;
					case ElementType.CenteredText:
						writer.WriteLine(((CenteredTextElement)element).Value);
						writer.WriteLine();
						break;
					case ElementType.LineBreak:
						writer.WriteLine();
						writer.WriteLine();
						writer.WriteLine();
						break;
					case ElementType.NoteBlock: break;
					case ElementType.Section: break;
					case ElementType.Synopsis: break;
					case ElementType.TitlePart:
						TitlePartElement titlePart = (TitlePartElement)element;
						writer.Write(titlePart.Key);
						writer.Write(":");
						writer.WriteLine(titlePart.Value);
						break;
					case ElementType.Title:
						
						break;
					default:
						throw new Exception("Invalid value for ElementType");
				}
			}
		}
	}
}
