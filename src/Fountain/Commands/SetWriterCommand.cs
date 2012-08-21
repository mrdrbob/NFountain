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

using PageOfBob.NFountain.Configuration;

namespace PageOfBob.NFountain.Commands {
	internal class SetWriterCommand : ICommand {
		public string Trigger { get { return "writer"; } }
		public string Description { get { return "Sets which output writer to use."; } }
		private PluginArgument _pluginArgument;
		
		public IEnumerable<CommandArgument> Arguments {
			get { return new CommandArgument[] { _pluginArgument }; }
		}
		
		public void Init(IEngine engine) {
			_pluginArgument = new PluginArgument((Engine)engine, PluginType.Writer);
		}
		
		public void Execute(IEngine engine) {
			if (_pluginArgument.Plugin != null) {
				Engine eng = (Engine)engine;
				IWriter writer = eng.LoadWriter(_pluginArgument.Plugin);
				if (writer == null)
					throw new InvalidOperationException("Plugin could not be loaded as Writer: " + _pluginArgument.Plugin.Assembly + " - " + _pluginArgument.Plugin.Type);
				eng.Writer = writer;
				IConfigurable conf = writer as IConfigurable;
				if (conf != null) {
					foreach (ICommand cmd in conf.Commands) {
						eng.AddCommand(cmd);
					}
				}
			}
		}
	}
}
