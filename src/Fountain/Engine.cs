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
using System.Configuration;
using System.IO;
using System.Reflection;

using PageOfBob.NFountain.Configuration;

namespace PageOfBob.NFountain {
	internal class Engine : IEngine {
		Dictionary<Tuple<string, PluginType>,NFountainPlugin> _plugins;
		Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();
		
		public IParser Parser { get; set; }
		public IWriter Writer { get; set; }
		public TextReader Input { get; set; }
		public Stream Output { get; set; }
		
		public Engine() { 
			AddCommand(new Commands.SetInputCommand());
			AddCommand(new Commands.SetOutputCommand());
			AddCommand(new Commands.SetWriterCommand());
		}
		
		public int Execute(string[] args) {
			try {
				System.Collections.IEnumerator argsEn = args.GetEnumerator();
				while(argsEn.MoveNext()) {
					string arg = (string)argsEn.Current;
					
					if (arg.StartsWith("-")) {
						ICommand cmd;
						if (!_commands.TryGetValue(arg.Substring(1), out cmd)) {
							// Show usage
							return 2;
						}
						
						cmd.Init(this);
						
						if (cmd.Arguments != null) {
							foreach (CommandArgument cmdarg in cmd.Arguments) {
								if (!argsEn.MoveNext() || !cmdarg.TryParse((string)argsEn.Current)) {
									Console.WriteLine("Failed to parse argument {0} for -{1} command.", cmdarg.Name, cmd.Trigger);
									return 3;
								}
							}
						}
						
						cmd.Execute(this);
						
					} else {
						// Show usage
						return 1;
					}
				}
				
				if (Input == null)
					Input = Console.In;
				if (Output == null)
					Output = Console.OpenStandardOutput();
				
				if (Parser == null)
					Parser = new Plugins.DefaultParserModule();
				if (Writer == null)
					Writer = new Plugins.DefaultWriterModule();
				
				var parsed = Parser.Transform(Input);
				Writer.Transform(parsed, Output);
#if !DEBUG
			} catch(Exception ex) {
				Console.Error.WriteLine("An uncaught exception has occurred:");
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine(ex.StackTrace);
				while((ex = ex.InnerException) != null) {
					Console.Error.WriteLine("Inner Exception: " + ex.Message);
					Console.Error.WriteLine(ex.StackTrace);
				}
#endif
			} finally {
				if (Input != null)
					Input.Dispose();
				if (Output != null)
					Output.Dispose();
			}
			return 0;
		}
		
		public NFountainPlugin LoadPlugin(string key, PluginType type) {
			if (_plugins == null) {
				_plugins = new Dictionary<Tuple<string, PluginType>, NFountainPlugin>();
				NFountainConfigurationSection config = (NFountainConfigurationSection)ConfigurationManager.GetSection("nFountainConfiguration");
				foreach (NFountainPlugin plugin in config.Instances) {
					_plugins.Add(new Tuple<string, PluginType>(plugin.Key, plugin.PluginType), plugin);
				}
			}
			
			var tupleKey = new Tuple<string, PluginType>(key, type);
			if (_plugins.ContainsKey(tupleKey)) {
				return _plugins[tupleKey];
			}
			
			return null;
		}
		
		public IParser LoadParser(NFountainPlugin module) {
			Assembly asm = Assembly.Load(module.Assembly);
			IParser instance = asm.CreateInstance(module.Type) as IParser;
			return instance;
		}
		
		public IWriter LoadWriter(NFountainPlugin module) {
			Assembly asm = Assembly.Load(module.Assembly);
			object instance = asm.CreateInstance(module.Type);
			IWriter asWriter = instance as IWriter;
			return asWriter;
		}
		
		public void AddCommand(ICommand command) {
			_commands.Add(command.Trigger, command);
		}
	}
}
