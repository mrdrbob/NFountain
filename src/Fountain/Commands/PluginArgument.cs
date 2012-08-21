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

using PageOfBob.NFountain.Configuration;

namespace PageOfBob.NFountain.Commands
{
	internal class PluginArgument : CommandArgument {
		private Engine _engine;
		private PluginType _type;
		public NFountainPlugin Plugin { get; private set; }
		
		public PluginArgument(Engine engine, PluginType type) {
			_engine = engine;
			_type = type;
		}
		
		public override bool TryParse(string rawArg) {
			Plugin = _engine.LoadPlugin(rawArg, _type);
			return Plugin != null;
		}
		
		public override string Name { get { return "Plugin ID"; } }
	}

}
