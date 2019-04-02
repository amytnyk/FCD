using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCD
{
	class FCDocument
	{
		enum ArgType
		{
			Array,
			Int,
			Float,
			Double,
			Dynamic,
		}

		class Argument
		{
			public string name;
			public ArgType type;
		}

		class Function
		{
			public string name;
			public string name_space;
			public bool is_variadic = false;
			public List<Argument> arguments;

			public Function()
			{
				arguments = new List<Argument>();
			}

			public dynamic execute(params dynamic[] args)
			{
				
			}
		}

		private string document;
		private Dictionary<String, Dictionary<String, dynamic>> defaults;
		public List<String> errors { get; private set; }
		private List<Function> functions;

		public FCDocument()
		{
			errors = new List<string>();
			defaults = new Dictionary<string, Dictionary<string, dynamic>>();
		}

		public dynamic executeFunction(string name, string name_space, params dynamic[] args)
		{
			foreach (var func in functions)
			{
				if (func.name_space == name_space && func.name == name)
					return func.execute(args);
			}
			errors.Add(string.Format("Function with name {0} and namespace {1} not found", name, name_space));
			return null;
		}

		public void Load(string file)
		{
			if (!File.Exists(file))
			{
				errors.Add("Load error: File not found");
				return;
			}
			StreamReader sr = new StreamReader(file);
			document = sr.ReadToEnd();
			sr.Close();
			Parse();
		}

		public dynamic getValue(string key, string name_space = "Default")
		{
			return defaults[name_space][key];
		}

		private ArgType toType(string type)
		{
			ArgType arg_type = ArgType.Dynamic;
			switch (type)
			{
				case "float":
					arg_type = ArgType.Float;
					break;
				case "double":
					arg_type = ArgType.Double;
					break;
				case "int":
					arg_type = ArgType.Int;
					break;
				case "array":
					arg_type = ArgType.Array;
					break;
				default:
					break;
			}
			return arg_type;
		}

		private void Parse()
		{
			string current_namespace = "Default";
			defaults.Add(current_namespace, new Dictionary<string, dynamic>());
			document = document.Replace("\r", string.Empty);
			int pos = 0;
			while (pos < document.Length)
			{
				if (document[pos] == '{')
				{
					int index_end = document.IndexOf('}', pos);
					current_namespace = document.Substring(pos + 1, index_end - pos - 1);
					if (!defaults.ContainsKey(current_namespace))
						defaults.Add(current_namespace, new Dictionary<string, dynamic>());
					pos = index_end + 2;
					continue;
				}
				else if (document[pos] == '@')
				{
					Function new_function = new Function();
					new_function.arguments = new List<Argument>();
					if (document[pos + 1] == '[')
					{
						int end_index = document.IndexOf(']', pos + 1);
						string args = document.Substring(pos + 2, end_index - pos - 2);
						if (args == "...")
							new_function.is_variadic = true;
						else
						{
							string[] arg_list = args.Split(',');
							foreach (var arg in arg_list)
							{
								List<string> splitted = arg.Split(' ').ToList();
								splitted.Remove(string.Empty);
								if (splitted.Count == 1)
								{
									new_function.arguments.Add(new Argument { name = splitted[0], type = ArgType.Dynamic });
								} else
									new_function.arguments.Add(new Argument { name = splitted[0], type = toType(splitted[2]) });
							}
						}
						
					} else
					{

					}

				}
				else
				{
					int index_end = document.IndexOf(' ', pos);
					string name = document.Substring(pos, index_end - pos);
					int index_end_of_line = document.IndexOf('\n', pos);
					if (index_end_of_line == -1)
						index_end_of_line = document.Length;
					int start_index_value = document.IndexOf(' ', document.IndexOf('=', index_end));
					string value = document.Substring(start_index_value + 1, index_end_of_line - start_index_value - 1);
					int int_result;
					float float_result;
					double double_result;
					if (value[0] == '"')
					{
						defaults[current_namespace].Add(name, value.Substring(1, value.Length - 2));
					}
					else if (value.ToLower() == "true")
					{
						defaults[current_namespace].Add(name, true);
					}
					else if (value.ToLower() == "false")
					{
						defaults[current_namespace].Add(name, false);
					}
					else if (value.ToLower() == "null")
					{
						defaults[current_namespace].Add(name, null);
					}
					else if (int.TryParse(value, out int_result))
					{
						defaults[current_namespace].Add(name, int_result);
					}
					else if (float.TryParse(value, out float_result))
					{
						defaults[current_namespace].Add(name, float_result);
					}
					else if (double.TryParse(value, out double_result))
					{
						defaults[current_namespace].Add(name, double_result);
					}
					else
					{
						defaults[current_namespace].Add(name, value);
					}
					pos = index_end_of_line + 1;
					continue;
				}
			}
		}
	}
}
