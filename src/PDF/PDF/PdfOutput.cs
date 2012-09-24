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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PageOfBob.NFountain.PDF
{
	public class PdfOutput {
		private readonly TextWriterWrapper _writer;
		private int _indent;

		public PdfOutput(TextWriter writer) {
			_writer = new TextWriterWrapper(writer);
		}

		public void WriteCompositor(Compositor comp) {
			_writer.WriteLine("%PDF-1.4");
			int itemCount = 0;
			foreach (IndirectObject indirectObject in comp.Objects) {
				indirectObject.Offset = _writer.Position;
				Write(indirectObject);
				itemCount += 1;
			}

			int xrefOffset = _writer.Position;
			_writer.WriteLine("xref");
			_writer.WriteLine("{0}  {1}", comp.StartID, itemCount + 1);
			_writer.WriteLine("0000000000 65535 f");
			foreach (IndirectObject indirectObject in comp.Objects) {
				_writer.WriteLine("{0:0000000000} {1:00000} n", indirectObject.Offset, indirectObject.Reference.Generation);
			}
			_writer.WriteLine("trailer");
			WriteObject(new DictionaryObject()
				.Set("Size", new IntegerNumberObject(itemCount))
				.Set("Root", comp.CatalogReference)
			);
			_writer.WriteLine();
			_writer.WriteLine("startxref");
			_writer.WriteLine(xrefOffset);
			_writer.WriteLine("%%EOF");

		}

		PdfOutput WriteIndent() {
			_writer.Write(new string('\t', _indent));
			return this;
		}
		PdfOutput Write(string val) { 
			_writer.Write(val);
			return this;
		}
		PdfOutput WriteLine() { return this.WriteLine(""); }
		PdfOutput WriteLine(string val) {
			_writer.WriteLine(val);
			return this;
		}
		PdfOutput Indent() {
			_indent++;
			return this;
		}
		PdfOutput Outdent() {
			_indent = Math.Max(0, _indent - 1);
			return this;
		}
		PdfOutput Write(BaseObject obj) {
			WriteObject(obj);
			return this;
		}
		PdfOutput Write(Command cmd) {
			WriteCommand(cmd as TextCommand);
			return this;
		}

		#region Base Objects
		private bool WriteObject(BaseObject obj) {
			if (obj == null)
				return false;

			bool found = WriteObject(obj as IntegerNumberObject)
				|| WriteObject(obj as RealNumberObject)
				|| WriteObject(obj as StringObject)
				|| WriteObject(obj as ArrayObject)
				|| WriteObject(obj as DictionaryObject)
				|| WriteObject(obj as IndirectReferenceObject)
				|| WriteObject(obj as IndirectObject)
				|| WriteObject(obj as NameObject)
				|| WriteObject(obj as StreamObject);

			if (!found) {
				Console.WriteLine("!ERR - COULD NOT FIND {0}", obj.GetType().FullName);
			}

			return found;
		}

		private bool WriteObject(IntegerNumberObject obj) {
			if (obj == null)
				return false;
			Write(obj.Value.ToString());
			return true;
		}

		private bool WriteObject(RealNumberObject obj) {
			if (obj == null)
				return false;
			Write(obj.Value.ToString("0.##"));
			return true;
		}

		private bool WriteObject(StringObject obj) {
			if (obj == null)
				return false;
			WriteEscapedString(obj.Value, _writer);
			return true;
		}

		private bool WriteObject(NameObject obj) {
			if (obj == null)
				return false;

			WriteEscapedName(obj.Value, _writer);
			return true;
		}

		private bool WriteObject(ArrayObject obj) {
			if (obj == null)
				return false;

			Write("[ ");
			for (int i = 0; i < obj.Objects.Count; i++) {
				WriteObject(obj.Objects[i]);
				Write(" ");
			}
			Write("]");

			return true;
		}

		private bool WriteObject(DictionaryObject obj) {
			if (obj == null)
				return false;
			
			WriteLine("<<").Indent();
			foreach (var baseObject in obj.Objects) {
				WriteIndent();
				WriteObject(baseObject.Key);
				Write(" ");
				WriteObject(baseObject.Value);
				WriteLine();
			}

			Outdent().WriteIndent().Write(">>");
			return true;
		}

		private bool WriteObject(IndirectReferenceObject reference) {
			if (reference == null)
				return false;

			Write(reference.Value.Id.ToString())
				.Write(" ")
				.Write(reference.Value.Generation.ToString())
				.Write(" R");

			return true;
		}

		private bool WriteObject(IndirectObject obj) {
			if (obj == null)
				return false;

			WriteIndent()
				.Write(obj.Reference.Id.ToString())
				.Write(" ")
				.Write(obj.Reference.Generation.ToString())
				.WriteLine(" obj").Indent();
			WriteIndent().WriteObject(obj.Value);
			WriteLine().Outdent().WriteIndent().WriteLine("endobj");

			return true;
		}

		private bool WriteObject(StreamObject obj) {
			if (obj == null)
				return false;
			
			
			var options = new DictionaryObject()
				.Set("Length", obj.Length)
				/*
				.Set("Filters", new ArrayObject()
				     .Add(new NameObject("ASCII85Decode"))
				     .Add(new NameObject("LZWDecode")))
				*/
				;
			WriteObject(options);
			WriteLine().WriteLine("stream");
			
			/*
			StringWriter w = new StringWriter();
			PdfOutput inner = new PdfOutput(w);
			inner.WriteStream(obj.Value);
			string raw = w.ToString();
			
			// byte[] compressed = LZW.Encode(raw);
			var lzwByteStream = LZW.ToByteStream(raw);
			
			w = new StringWriter();
			
			Ascii85.Encode(lzwByteStream, w);
			
			int start = _writer.Position;
			_writer.Write(w);
			_writer.WriteLine();
			int length = _writer.Position - start;
			// */
			
			// /*
			int start = _writer.Position;
			WriteStream(obj.Value);
			int length = _writer.Position - start;
			// */
			
			Write("endstream");
			
			obj.Length.Get<IntegerNumberObject>().Value = length;

			return true;
		}
		#endregion

		#region Streams
		internal bool WriteStream(BaseStream stream) {
			return WriteStream(stream as TextCommandStream);
		}

		private bool WriteStream(TextCommandStream stream) {
			if (stream == null)
				return false;

			WriteIndent().WriteLine("BT").Indent().WriteLine();

			foreach (var textCommand in stream.List) {
				WriteIndent().Write(textCommand).WriteLine();
			}

			Outdent().WriteIndent().WriteLine("ET");

			return true;
		}
		#endregion

		#region Commands
		private bool WriteCommand(TextCommand cmd) {
			if (cmd == null)
				return false;

			return WriteCommand(cmd as NextlineWithOffset)
				|| WriteCommand(cmd as NextLine)
				|| WriteCommand(cmd as PrintString)
				|| WriteCommand(cmd as SetTextMatrix)
				|| WriteCommand(cmd as SetFontCommand)
				/* || WriteCommand(cmd as SetGray) */ ;
		}

		private bool WriteCommand(NextlineWithOffset cmd) {
			if (cmd == null)
				return false;

			Write(cmd.X)
				.Write(" ")
				.Write(cmd.Y)
				.Write(" Td");

			return true;
		}

		private bool WriteCommand(NextLine cmd) {
			if (cmd == null)
				return false;

			Write("T*");

			return true;
		}

		private bool WriteCommand(PrintString cmd) {
			if (cmd == null)
				return false;

			WriteObject(cmd.Value);
			Write(" Tj");

			return true;
		}
		
		private bool WriteCommand(SetTextMatrix cmd) {
			if (cmd == null)
				return false;

			Write(string.Format("{0:0.#} {1:0.#} {2:0.#} {3:0.#} {4:0.#} {5:0.#} Tm", 
			      cmd.Matrix[0],
			      cmd.Matrix[1],
			      cmd.Matrix[2],
			      cmd.Matrix[3],
			      cmd.Matrix[4],
			      cmd.Matrix[5]
			));

			return true;
		}
		
		private bool WriteCommand(SetFontCommand cmd) {
			if (cmd == null)
				return false;
			
			Write(cmd.FontName)
				.Write(" ")
				.Write(cmd.FontSize)
				.Write(" Tf");
			
			return true;
		}
		
		/*
		private bool WriteCommand(SetGray cmd) {
			if (cmd == null)
				return false;
			
			Write(cmd.Grayscale)
				.Write(" G");
			
			return true;
		} */
		#endregion

		#region Name
		static void WriteEscapedName(string name, TextWriter writer) {
			writer.Write('/');
			foreach (char c in name) {
				if (c == '#') {
					writer.Write("#23");
				} else if (c >= '!' && c <= '~') {
					writer.Write(c);
				} else {
					writer.Write('#');
					byte[] b = Encoding.ASCII.GetBytes(c.ToString());
					writer.Write("{0:X}", b[0]);
				}
			}
		}
		#endregion

		#region String Object Escaping
		private static readonly Dictionary<char, string> EscapeMapping = new Dictionary<char, string> {
			{'\\', "\\\\"},
			{'\n', "\\n"},
			{'\r', "\\r"},
			{'\t', "\\t"},
			{'\b', "\\b"},
			{'(', "\\("},
			{')', "\\)"}
		};
		private const int BreakEncodedStringsChars = 100;
		private static void WriteEscapedString(string value, TextWriter writer) {
			writer.Write('(');

			int length = 0;
			foreach (char c in value) {
				string escaped;
				if (EscapeMapping.TryGetValue(c, out escaped)) {
					writer.Write(escaped);
				} else {
					writer.Write(c);
				}
				length++;
				if ((c == ' ' || c == '\t') && length > BreakEncodedStringsChars) {
					writer.Write("\\\n");
					length = 0;
				}
			}

			writer.Write(')');
		}
		#endregion
	}
}
