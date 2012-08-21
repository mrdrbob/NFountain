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

namespace PageOfBob.NFountain.Plugins {
	public class DefaultWriterModule : DefaultWriter, IWriter, IConfigurable {
		
		ICommand[] _commands = new ICommand[] {
			new WriterConfigurationCommand("width", "Sets the width of the page", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.Columns = size;
			})),
			new WriterConfigurationCommand("charindent", "Sets the indent size of character lines", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.CharacterIndent = size;
			})),
			new WriterConfigurationCommand("charwidth", "Sets the width of character lines", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.CharacterWidth = size;
			})),
			new WriterConfigurationCommand("dialogindent", "Sets the indent of dialog lines", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.DialogIndent = size;
			})),
			new WriterConfigurationCommand("dialogwidth", "Sets the width of dialog lines", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.DialogWidth = size;
			})),
			new WriterConfigurationCommand("parenindent", "Sets the indent of parenthetical lines", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.ParentheticalIndent = size;
			})),
			new WriterConfigurationCommand("parenwidth", "Sets the width of parenthetical lines", "size in characters", new Action<DefaultWriter, int> ( (writer, size) => {
				writer.ParentheticalWidth = size;
			}))
		};
		
		public IEnumerable<ICommand> Commands { get { return _commands; } }
		
		private class WriterConfigurationCommand : ICommand {
			Action<DefaultWriter, int> _action;
			IntegerCommandArgument _arg;
			IntegerCommandArgument[] _args;
			
			public WriterConfigurationCommand(string trigger, string desc, string argName, Action<DefaultWriter, int> action) {
				Trigger = trigger;
				Description = desc;
				_action = action;
				_arg = new IntegerCommandArgument(argName);
				_args = new IntegerCommandArgument[] { _arg };
			}
			
			public string Trigger { get; private set; }
			public string Description { get; private set; }
			
			public IEnumerable<CommandArgument> Arguments {
				get { return _args; }
			}
			
			public void Init(IEngine engine) { }
			
			public void Execute(IEngine engine) {
				DefaultWriter writer = engine.Writer as DefaultWriter;
				if (writer != null) {
					_action(writer, _arg.Value);
				}
			}
		}
		
		private class IntegerCommandArgument : CommandArgument {
			public int Value { get; private set; }
			private string _name;
			
			public IntegerCommandArgument(string name) {
				_name = name;
			}
			
			public override bool TryParse(string rawArg) {
				int val;
				bool success = int.TryParse(rawArg, out val);
				if (!success)
					return false;
				Value = val;
				return true;
			}
			
			public override string Name { get { return _name; } }
		}
	}
}
