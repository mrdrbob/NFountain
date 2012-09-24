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

namespace PageOfBob.NFountain.Plugins {
	public class PdfWriterModule : IWriter, IConfigurable {
		 
		PdfWriter writer = new PdfWriter();
		
		public void Transform(IEnumerable<Element> elements, Stream output) {
			writer.Transform(elements, output);
		}
		
		
		
		private class PdfConfigurationCommand : ICommand {
			Action<PdfWriterModule, float> _action;
			FloatCommandArgument _arg;
			FloatCommandArgument[] _args;
			
			public PdfConfigurationCommand(string trigger, string desc, string argName, Action<PdfWriterModule, float> action) {
				Trigger = trigger;
				Description = desc;
				_action = action;
				_arg = new FloatCommandArgument(argName);
				_args = new FloatCommandArgument[] { _arg };
			}
			
			public string Trigger { get; private set; }
			public string Description { get; private set; }
			
			public IEnumerable<CommandArgument> Arguments {
				get { return _args; }
			}
			
			public void Init(IEngine engine) { }
			
			public void Execute(IEngine engine) {
				PdfWriterModule writer = engine.Writer as PdfWriterModule;
				if (writer != null) {
					_action(writer, _arg.Value);
				}
			}
		}
		
		private class FloatCommandArgument : CommandArgument {
			public float Value { get; private set; }
			private string _name;
			
			public FloatCommandArgument(string name) {
				_name = name;
			}
			
			public override bool TryParse(string rawArg) {
				float val;
				bool success = float.TryParse(rawArg, out val);
				if (!success)
					return false;
				Value = val;
				return true;
			}
			
			public override string Name { get { return _name; } }
		}
		
		private readonly ICommand[] _commands = new ICommand[] {
			new PdfConfigurationCommand("page-width", "Sets page width", "width", (w, v) => {
				w.writer._settings.PageWidth = v;
			}),
			new PdfConfigurationCommand("page-height", "Sets page height", "height", (w, v) => {
				w.writer._settings.PageHeight = v;
			}),
			new PdfConfigurationCommand("left-margin", "Sets left margin", "margin", (w, v) => {
				w.writer._settings.LeftMargin = v;
			}),
			new PdfConfigurationCommand("top-margin", "Sets top margin", "margin", (w, v) => {
				w.writer._settings.TopMargin = v;
			}),
			new PdfConfigurationCommand("right-margin", "Sets right margin", "margin", (w, v) => {
				w.writer._settings.RightMargin = v;
			}),
			new PdfConfigurationCommand("bottom-margin", "Sets bottom margin", "margin", (w, v) => {
				w.writer._settings.BottomMargin = v;
			}),
			new TurnOnBoneyardCommand(),
			new TurnOnNotesCommand(),
			new TurnOnSectionsCommand()
		};
		
		public IEnumerable<ICommand> Commands { get { return _commands; } }
		
		private class TurnOnBoneyardCommand : ICommand {
			public string Trigger { get { return "boneyard"; } }
			public string Description { get { return "Displays boneyard content"; } }
			public IEnumerable<CommandArgument> Arguments { get { return Enumerable.Empty<CommandArgument>(); } }
			public void Init(IEngine engine) { }
			
			public void Execute(IEngine engine) {
				PdfWriterModule writer = engine.Writer as PdfWriterModule;
				if (writer == null)
					return;
				
				writer.writer.ShowBoneyards = true;
			}
		}
		
		private class TurnOnNotesCommand : ICommand {
			public string Trigger { get { return "notes"; } }
			public string Description { get { return "Displays notes content"; } }
			public IEnumerable<CommandArgument> Arguments { get { return Enumerable.Empty<CommandArgument>(); } }
			public void Init(IEngine engine) { }
			
			public void Execute(IEngine engine) {
				PdfWriterModule writer = engine.Writer as PdfWriterModule;
				if (writer == null)
					return;
				
				writer.writer.ShowNotes = true;
			}
		}
		
		private class TurnOnSectionsCommand : ICommand {
			public string Trigger { get { return "sections"; } }
			public string Description { get { return "Displays section headings content"; } }
			public IEnumerable<CommandArgument> Arguments { get { return Enumerable.Empty<CommandArgument>(); } }
			public void Init(IEngine engine) { }
			
			public void Execute(IEngine engine) {
				PdfWriterModule writer = engine.Writer as PdfWriterModule;
				if (writer == null)
					return;
				
				writer.writer.ShowSectionHeadings = true;
			}
		}
	}
}
