using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace UiserClient
{
	public class CommandDataPattern {
		public static CommandDataPattern EmptyPattern = new CommandDataPattern();

		public Dictionary<char, bool> options { get; private set; }

		public CommandDataPattern() {
			options = new Dictionary<char, bool>();
		}

		public CommandDataPattern AddOption(char key, bool hasValue) {
			options.Add(key, hasValue);
			return this;
		}
	}

	public class CommandData
	{
		private static Regex isOption = new Regex("^-[a-z]+");
		private static Regex quoteStart = new Regex("^((')|(\"))");
		private static Regex quoteEnd = new Regex("((')|(\"))$");

        public List<string> args { get; private set; }
		public string Cmd { get; private set; }

		public int KeysCount {
			get { return keys.Count; }
		}

		private List<string> mergeSplit;
		private Dictionary<char, int> keys;

		public CommandData(String cmdLine) {
            keys = new Dictionary<char, int>();
            args = new List<string>();

			string[] split = Regex.Split(cmdLine.Trim(), " ");
			mergeSplit = new List<string>(split.Length);
			Cmd = split[0];

			for (int i = 1; i < split.Length; i++) {
				if (quoteStart.IsMatch(split[i])) {
					StringBuilder sb = new StringBuilder();
					while (quoteEnd.IsMatch(split[i]) == false) {
						sb.AppendFormat("{0} ", split[i]);
						i++;
					}
					sb.Append(split[i]);
					string forAdd = sb.ToString();
					mergeSplit.Add(forAdd.Substring(1, forAdd.Length-2));
				}
				else {
					mergeSplit.Add(split[i]);
				}
			}
		}

		public void SetPattern(CommandDataPattern pattern) {
			args.Clear();
			keys.Clear();

			for (int i = 0; i < mergeSplit.Count; i++) {
				if (isOption.IsMatch(mergeSplit[i])) {
					int lenght = mergeSplit[i].Length;
					for (int j = 1; j < lenght; j++) {
						char key = mergeSplit[i][j];
						if (!pattern.options.ContainsKey(key)) {
							throw new BadInputException(String.Format("key not found {0}", key));
						}

						bool hasValue = pattern.options[key];
						if (hasValue) {
							if (lenght > 2) {
								throw new BadInputException(String.Format("inline keys with value {0}", mergeSplit[i]));
							}
							if (i == mergeSplit.Count - 1) {
								throw new BadInputException(String.Format("value not found for {0}", key));
							}
							keys.Add(key, i + 1);
							i++;
						}
						else {
							keys.Add(key, -1);
						}
					}
				}
				else {
					args.Add(mergeSplit[i]);
				}
			}
		}

		public bool IsKey(char key) {
			return keys.ContainsKey(key);
		}

		public string GetKeyValue(char key) {
			int index = keys[key];
			if (index == -1) { throw new EmptyKeyException(); }
			string keyData = mergeSplit[index];

			return keyData;
		}

		public IEnumerable<char> GetKeysArray() {
			foreach (KeyValuePair<char, int> key in keys) {
				yield return key.Key;
			}
		}

		//cmd [OPTIONS] [ARGS]
		//cmd [ARGS] [OPTIONS]

		//qwe '1df 3dsf' "dsf dkjf fjhg" -g -lk -m 10 -t
		//qwe -g -lk -m 10 -t '1df 3dsf' "dsf dkjf fjhg"

		/*private void Parse() {
			if (mergeSplit.Count == 0) { return; }
			if (isOption.IsMatch(mergeSplit[0])) {
				ParseArgs(ParseOptions(0));
			}
			else {
                ParseOptions(ParseArgs(0));
			}
		}

		private int ParseArgs(int start) {
			if (mergeSplit.Count == start) { return start; }
			args.Add(mergeSplit[start]);
			for (int i = start+1; i < mergeSplit.Count; i++) {
				if (!isOption.IsMatch(mergeSplit[i])) {
					args.Add(mergeSplit[i]);
				}
				else {
					return i;
				}
			}
			return mergeSplit.Count;
		}

		private int ParseOptions(int start) {
			if (mergeSplit.Count == start) { return start; }
			AddOptions(start);
			for (int i = start + 1; i < mergeSplit.Count; i++) {
				if (isOption.IsMatch(mergeSplit[i])) {
                    AddOptions(i);
				}
				else {
					if ((i + 1 <= mergeSplit.Count - 1) && (isOption.IsMatch(mergeSplit[i + 1]) == true)) {
						continue;
					}
					return i;
				}
			}
			return mergeSplit.Count;
		}

		private void AddOptions(int index) {
			string forParse = mergeSplit[index];
			if (forParse.Length >= 2) {
                for (int j = 1; j<forParse.Length; j++) {
                    keys.Add(forParse[j], index);
                }
            }
            else {
				throw new BadInputException();
            }
		}
		*/

	}
}
