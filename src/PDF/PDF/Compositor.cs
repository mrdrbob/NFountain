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

namespace PageOfBob.NFountain.PDF {
	public class Compositor {
		readonly List<IndirectObject> _objects = new List<IndirectObject>();

		private readonly DictionaryObject _catalog;
		private readonly IndirectObject _catalogReference;
		private readonly PageCollectionBuilder _rootCollection;

		private readonly int _generation;
		private readonly int _startID;
		private int _id;

		public Compositor() : this(0, 1) { }
		
		private Compositor(int generation, int startingID) {
			_generation = generation;
			_startID = _id = startingID;

			_catalogReference = IndirectObject(_catalog = Dictionary("Catalog"));
			_rootCollection = new PageCollectionBuilder(this);
			_catalog.Set("Pages", _rootCollection.Reference);

			FontID = 1;
		}

		internal IndirectObject CatalogReference {
			get { return _catalogReference; }
		}

		internal int FontID { get; set; }

		internal int StartID {
			get { return _startID; }
		}

		internal int Generation {
			get { return _generation; }
		}

		internal List<IndirectObject> Objects {
			get { return _objects; }
		}

		internal DictionaryObject Catelog { get { return _catalog; } }

		private Reference NextAvailableReference() {
			return new Reference(_generation, _id++);
		}

		#region Create Objects
		internal DictionaryObject Dictionary(string type) {
			return new DictionaryObject().Set("Type", new NameObject(type));
		}

		internal IndirectObject StreamObject(BaseStream stream) {
			StreamObject obj = new StreamObject(stream);
			IndirectObject reference = IndirectObject(obj);
			obj.Length = IndirectObject(new IntegerNumberObject());
			return reference;
		}

		internal IndirectObject IndirectObject(BaseObject obj) {
			IndirectObject ind = new IndirectObject(NextAvailableReference(), obj);
			_objects.Add(ind);
			return ind;
		}

		internal IndirectReferenceObject ReferenceTo(IndirectObject obj) {
			return new IndirectReferenceObject(obj.Reference);
		}
		#endregion

		public PageCollectionBuilder RootPageCollection { get { return _rootCollection; } }
		
		#region Page Building
		public abstract class PageObjectBuilder<T> where T : PageObjectBuilder<T> {
			internal abstract DictionaryObject Dictionary { get; }
			internal abstract Compositor Compositor { get; }

			public T MediaBox(int lowerLeftX, int lowerLeftY, int upperRightX, int upperRightY) {
				Dictionary.Set("MediaBox", new ArrayObject()
					.Add(new IntegerNumberObject(lowerLeftX))
					.Add(new IntegerNumberObject(lowerLeftY))
					.Add(new IntegerNumberObject(upperRightX))
					.Add(new IntegerNumberObject(upperRightY))
				);
				return (T)this;
			}

			public  ResourceBuilder<T> AddResources() {
				return new ResourceBuilder<T>((T)this);
			}
		}

		public class PageCollectionBuilder : PageObjectBuilder<PageCollectionBuilder> {
			private readonly Compositor _compositor;
			private readonly DictionaryObject _me;
			private readonly IndirectObject _ref;
			private readonly ArrayObject _kids;
			private readonly IntegerNumberObject _count;

			public PageCollectionBuilder(Compositor compositor) {
				_compositor = compositor;
				_ref = compositor.IndirectObject(_me = compositor.Dictionary("Pages")
					.Set("Kids", _kids = new ArrayObject())
					.Set("Count", _count = new IntegerNumberObject())
				);
			}

			internal override Compositor Compositor { get { return _compositor; } }
			internal override DictionaryObject Dictionary { get { return _me; } }
			internal IndirectObject Reference { get { return _ref; } }
			internal ArrayObject Kids { get { return _kids; } }

			public PageBuilder AddPage() {
				_count.Value += 1;
				return new PageBuilder(this);
			}

			public Compositor End() { return Compositor; }
		}

		public class PageBuilder : PageObjectBuilder<PageBuilder> {
			private readonly PageCollectionBuilder _parent;
			private readonly DictionaryObject _dictionary;
			private readonly TextCommandStream _stream;

			public PageBuilder(PageCollectionBuilder parent) {
				_parent = parent;

				IndirectObject pageRef = _parent.Compositor.IndirectObject(
					_dictionary = parent.Compositor.Dictionary("Page")
						.Set("Parent", parent.Reference)
				);
				parent.Kids.Add(pageRef);

				_dictionary.Set("Contents", parent.Compositor.StreamObject(_stream = new TextCommandStream()));
			}

			public PageBuilder SetFont(FontIdentifier name, float size) {
				_stream.List.Add(new SetFontCommand(
					new NameObject(name.Name),
					new RealNumberObject(size)
				));
				return this;
			}

			public PageBuilder NextLine(float x, float y) {
				NextlineWithOffset lastOffset;
				if (_stream.List.Count > 0 
				    && (lastOffset = _stream.List[_stream.List.Count - 1] as NextlineWithOffset) != null) {
					((RealNumberObject)lastOffset.X).Value += x;
					((RealNumberObject)lastOffset.Y).Value += y;
				} else {
					_stream.List.Add(new NextlineWithOffset(new RealNumberObject(x), new RealNumberObject(y)));
				}
				return this;
			}

			public PageBuilder NextLine() {
				_stream.List.Add(new NextLine());
				return this;
			}

			public PageBuilder WriteText(string text) {
				PrintString last;
				StringObject lastStr;
				
				if (_stream.List.Count > 0 
				    && (last = _stream.List[_stream.List.Count - 1] as PrintString) != null
					&& (lastStr = last.Value as StringObject) != null) {
					last.Value = new StringObject(lastStr.Value + text);
			    } else {
			   		_stream.List.Add(new PrintString(new StringObject(text)));
			    }
				return this;
			}
			
			public PageBuilder SetMatrix(float a, float b, float c, float d, float e, float f) {
				_stream.List.Add(new SetTextMatrix(new float[] { a, b, c, d, e, f }));
				return this;
			}
			
			/* public PageBuilder SetGrayscale(float scale) {
				scale = Math.Max(0f, Math.Min(1f, scale));
				_stream.List.Add(new SetGray(new RealNumberObject(scale)));
				return this;
			} */
			
			internal override DictionaryObject Dictionary {
				get { return _dictionary; }
			}

			internal override Compositor Compositor {
				get { return _parent.Compositor; }
			}

			public PageCollectionBuilder End() { return _parent; }
		}

		#region Resources
		public class ResourceBuilder<T> where T : PageObjectBuilder<T> {
			private readonly DictionaryObject _me;
			private readonly T _parent;
			private DictionaryObject _fonts;

			public ResourceBuilder(T parent) {
				_parent = parent;
				parent.Dictionary.Set("Resources", _me = new DictionaryObject());
			}

			public ResourceBuilder<T> AddSimpeType1Font(string baseFont, out FontIdentifier identifier) {
				string name = "F" + _parent.Compositor.FontID.ToString();
				identifier = new FontIdentifier {Name = name};

				IndirectObject reference = _parent.Compositor.IndirectObject(
					_parent.Compositor.Dictionary("Font")
						.Set("Subtype", new NameObject("Type1"))
						.Set("BaseFont", new NameObject(baseFont))
				);

				if (_fonts == null) {
					_fonts = new DictionaryObject();
					_me.Set("Font", _fonts);
				}

				_fonts.Set(name, reference);
				_parent.Compositor.FontID += 1;

				return this;
			}

			public T End() { return _parent; }
		}
		#endregion

		#endregion
	}
}
