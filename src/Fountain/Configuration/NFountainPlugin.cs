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
using System.Configuration;

namespace PageOfBob.NFountain.Configuration
{
	public class NFountainPlugin : ConfigurationElement {
		[ConfigurationProperty("key", IsRequired=true)]
		public string Key { 
			get { return (string)base["key"]; }
			set { base["key"] = value; }
		}
		
		[ConfigurationProperty("assembly", IsRequired=true)]
		public string Assembly {
			get { return (string)base["assembly"]; }
			set { base["assembly"] = value; }
		}
		
		[ConfigurationProperty("type", IsRequired=true)]
		public string Type {
			get { return (string)base["type"]; }
			set { base["type"] = value; }
		}
		
		[ConfigurationProperty("pluginType", IsRequired=true)]
		public PluginType PluginType {
			get { return (PluginType)base["pluginType"]; }
			set { base["pluginType"] = value; }
		}
	}
}
