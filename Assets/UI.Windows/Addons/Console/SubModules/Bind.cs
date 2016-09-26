using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace UnityEngine.UI.Windows.Plugins.Console.SubModules {

	[Alias("b")]
	public class Bind : SubModuleBase {

		public enum ImportFlag {

			Override,
			Merge,

		};
		
		public const string DEFAULT_FILENAME = "binds";
		public const string DEFAULT_LOCAL_FILENAME = "local.binds";

		public enum AutoStartType : byte {

			Add,
			Remove,

		};

		[System.Serializable]
		public class Data {
			
			[System.Serializable]
			public class Item {

				public string name;
				public string[] commands;
				public bool autostart = false;
				
				public Item() {}

				public Item(string name, string[] commands) {
					
					this.name = name;
					this.commands = commands;
					
				}

				public Item(Item source) {
					
					this.name = source.name;
					this.commands = source.commands;
					this.autostart = source.autostart;
					
				}

				protected Item(SerializationInfo info, StreamingContext context) {

					this.name = info.GetString("name");
					this.commands = (string[])info.GetValue("commands", typeof(string[]));
					this.autostart = info.GetBoolean("autostart");
					
				}
				
				public void GetObjectData(SerializationInfo info, StreamingContext context) {
					
					info.AddValue("name", this.name);
					info.AddValue("commands", this.commands);
					info.AddValue("autostart", this.autostart);
					
				}

			}

			public List<Item> items = new List<Item>();

			public Data() {}

			protected Data(SerializationInfo info, StreamingContext context) {
				
				this.items = ((Item[])info.GetValue("items", typeof(Item[]))).ToList();

			}
			
			public void GetObjectData(SerializationInfo info, StreamingContext context) {

				info.AddValue("items", this.items.ToArray());

			}

		}

		public Data data = new Data();

		// Current binding
		private List<string> bindingCommands = new List<string>();
		private string bindingName;
		private bool binding = false;

		public override void OnStartLate() {
			
			base.OnStartLate();
			
			// Autoload default files
			this.Import_INTERNAL(Bind.DEFAULT_FILENAME, silent: true, flag: ImportFlag.Override);
			this.Import_INTERNAL(Bind.DEFAULT_LOCAL_FILENAME, silent: true, flag: ImportFlag.Merge);

		}

		public bool IsBinding() {

			return this.binding;

		}

		public void AddToBind(string cmd) {
			
			this.bindingCommands.Add(cmd);
			
		}
		
		public bool Exists(string name) {
			
			name = name.ToLower().Trim();

			return this.GetItemByName(name) != null;
			
		}
		
		[Executable, Help("Export binding commands")]
		public string Export(string filename) {
			
			filename = filename.ToLower();
			
			var filepath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), filename + ".xml");

			XmlSerializer serializer =  new XmlSerializer(typeof(Data));
			TextWriter writer = new StreamWriter(filepath);
			
			serializer.Serialize(writer, this.data);
			writer.Close();
			
			this.screen.AddLine("File `" + filepath + "` has been exported.");

			return string.Empty;
			
		}
		
		[Executable, Help("Import binding commands")]
		public string Import(string filename, ImportFlag flag) {

			return this.Import_INTERNAL(filename, silent: false, flag: flag);

		}

		private string Import_INTERNAL(string filename, bool silent, ImportFlag flag) {

			filename = filename.ToLower();

			var filepath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), filename + ".xml");
			if (File.Exists(filepath) == false) {

				if (silent == true) return string.Empty;

				return "File `" + filepath + "` not found.";

			}
			
			this.screen.AddLine("<color=grey>Importing `" + filepath + "`...</color>");

			XmlSerializer serializer =  new XmlSerializer(typeof(Data));
			FileStream fs = new FileStream(filepath, FileMode.Open);
			
			var data = (Data)serializer.Deserialize(fs);
			if (flag == ImportFlag.Override) {

				this.data = data;
				this.AutoStartPlay(this.data);

			} else if (flag == ImportFlag.Merge) {
				
				foreach (var newItem in data.items) {
					
					if (this.Exists(newItem.name) == false) {

						var item = new Data.Item(newItem);
						this.data.items.Add(item);

					}
			    
			    }

				foreach (var item in this.data.items) {

					foreach (var newItem in data.items) {

						if (item.name.ToLower().Trim() == newItem.name.ToLower().Trim()) {

							item.autostart = newItem.autostart;
							item.commands = newItem.commands;

							if (item.autostart == true) {

								this.Play(item.name);

							}

						}

					}

				}

			}

			fs.Close();

			this.Save();

			this.screen.AddLine("<color=grey>File `" + filepath + "` has been imported.</color>");

			return string.Empty;
			
		}

		[Executable, Help("Remove binding command")]
		public string Remove(string name) {
			
			name = name.ToLower();

			if (this.Exists(name) == false) {
				
				return "Bind with name `" + name + "` doesn't exists!";
				
			}

			this.data.items.RemoveAll((item) => {

				var result = item.name == name;
				
				if (result == true) this.screen.AddLine("Command " + name + " removed");

				return result;

			});

			this.Save();

			return string.Empty;
			
		}
		
		[Executable, Help("List all bindings")]
		public string List() {

			this.screen.AddLine("----------------------------------------------");
			
			var cnt = 0;
			foreach (var item in this.data.items) {
				
				this.screen.AddLine("Command `" + item.name + "`:");
				this.List(item.name);
				++cnt;

			}

			this.screen.AddLine("Commands: " + cnt);
			this.screen.AddLine("----------------------------------------------");

			return string.Empty;

		}
		
		[Executable, Help("List binding by name")]
		public string List(string name) {
			
			name = name.ToLower();

			if (this.Exists(name) == false) {
				
				return "Bind with name `" + name + "` doesn't exists!";
				
			}
			
			var item = this.GetItemByName(name);

			this.screen.AddLine("Autostart: " + item.autostart.ToString());

			for (int i = 0; i < item.commands.Length; ++i) {
				
				this.screen.AddLine("\t" + item.commands[i]);

			}

			return string.Empty;

		}

		[Executable, Help("Begin binding command")]
		public string Begin(string name) {

			if (this.Exists(name) == true) {
				
				return "Bind with name `" + name + "` already exists!";
				
			}

			this.binding = true;
			this.bindingName = name.ToLower();
			this.bindingCommands.Clear();

			return string.Empty;

		}
		
		[Executable, Help("Close current binding")]
		public string End() {
			
			if (this.IsBinding() == false) {
				
				return "Bind needs to be opened!";
				
			}

			var item = new Data.Item(this.bindingName, this.bindingCommands.ToArray());
			this.data.items.Add(item);

			this.Save();
			
			for (int i = 0; i < this.bindingCommands.Count; ++i) this.screen.AddLine("Put command into binding: " + this.bindingCommands[i]);
			this.screen.AddLine("Commands added into " + this.bindingName + ": " + this.bindingCommands.Count);

			this.binding = false;
			this.bindingName = string.Empty;
			this.bindingCommands.Clear();

			return string.Empty;

		}

		[Executable, Help("Play binding by name")]
		public string Play(string name) {

			name = name.ToLower();

			if (this.Exists(name) == false) {
				
				return "Bind with name `" + name + "` doesn't exists!";
				
			}

			var item = this.GetItemByName(name);
			foreach (var cmd in item.commands) {
				
				ConsoleManager.OnCommand(cmd);
				
			}

			this.screen.AddLine("Bind `" + name + "` played.");

			return string.Empty;
			
		}
		
		[Executable, Help("Add autostart command")]
		public string AutoStart(string bindName) {

			return this.AutoStart(AutoStartType.Add, bindName);

		}

		[Executable, Help("Add/Remove autostart command. Type: Add, Remove")]
		public string AutoStart(AutoStartType type, string bindName) {
			
			if (this.Exists(bindName) == false) {
				
				return "Bind with name `" + bindName + "` doesn't exists!";
				
			}
			
			var item = this.GetItemByName(bindName);
			if (type == AutoStartType.Add) {

				item.autostart = true;

			} else if (type == AutoStartType.Remove) {
				
				item.autostart = false;

			}
			this.Save();

			if (type == AutoStartType.Add) {
				
				this.screen.AddLine("Bind `" + bindName + "` added to autostart.");

			} else if (type == AutoStartType.Remove) {
				
				this.screen.AddLine("Bind `" + bindName + "` removed from autostart.");

			}

			return string.Empty;
			
		}

		[Executable, Help("List all autostart commands")]
		public string AutoStart() {
			
			this.screen.AddLine("----------------------------------------------");
			
			var cnt = 0;
			foreach (var item in this.data.items) {

				if (item.autostart == true) {

					this.screen.AddLine("Command `" + item.name + "`:");
					this.List(item.name);
					
					++cnt;

				}

			}
			
			this.screen.AddLine("Commands: " + cnt);
			this.screen.AddLine("----------------------------------------------");

			return string.Empty;
			
		}

		public Data.Item GetItemByName(string name) {
			
			return this.data.items.FirstOrDefault((item) => item.name.ToLower().Trim() == name.ToLower().Trim());
			
		}

		#region Util
		public void Save() {

			/*var data = System.Convert.ToBase64String(this.Serialize(this.data));

			PlayerPrefs.SetString("Console.Binds.Data", data);*/

		}
		
		/*public void Load() {

			var data = PlayerPrefs.GetString("Console.Binds.Data", string.Empty);
			if (string.IsNullOrEmpty(data) == true) return;

			this.data = this.Deserialize(System.Convert.FromBase64String(data));

		}*/

		public void AutoStartPlay(Data data) {

		    var list = new List<string>();
			foreach (var item in data.items) {

				if (item.autostart == true) {

                    list.Add(item.name);

				}

			}

            foreach (var item in list) this.Play(item);

		}

		private byte[] Serialize(Data package) {
			
			var formatter = new BinaryFormatter();
			var stream = new MemoryStream();
			formatter.Serialize(stream, package);
			
			var bytes = stream.ToArray();
			stream.Close();
			
			return bytes;
			
		}
		
		private Data Deserialize(byte[] bytes) {
			
			var formatter = new BinaryFormatter();
			var stream = new MemoryStream(bytes);
			var package = (Data)formatter.Deserialize(stream);
			
			stream.Close();
			
			return package;
			
		}
		#endregion

	}

}