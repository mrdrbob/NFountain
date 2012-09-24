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

namespace PageOfBob.NFountain.Commands {
	internal class SetOutputCommand : ICommand {
		public string Trigger { get { return "output"; } }
		public string Description { get { return "Sets the output"; } }
		private NewFilePathArgument _arg = new NewFilePathArgument();
		
		public IEnumerable<CommandArgument> Arguments { get { return new NewFilePathArgument[] { _arg }; } }
		
		public void Init(IEngine engine) { }
		
		public void Execute(IEngine engine) {
			Engine eng = (Engine)engine;
			eng.Output = File.Create(_arg.Path);
		}
	}
}
