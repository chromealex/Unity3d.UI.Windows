using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace UnityEngine.UI.Windows.Extensions.Tiny {

	public class JsonParser : IDisposable {

		const string WHITE_SPACE = " \t\n\r";
		const string WORD_BREAK = " \t\n\r{}[],:\"";

		enum Token { None, CurlyOpen, CurlyClose, SquareOpen, SquareClose, Colon, Comma, String, Number, BoolOrNull };

		StringReader json;
		
		public static object ParseValue(string jsonString) {
			using (var instance = new JsonParser(jsonString)) {
				return instance.ParseValue();
			}
		}

		internal JsonParser(string jsonString) {
			json = new StringReader(jsonString);
		}

		public void Dispose() {
			json.Dispose();
			json = null;
		}

		//** Reading Token **//

		bool EndReached() {
			return json.Peek() == -1;
		}

		char PeekChar() {
			return Convert.ToChar(json.Peek());
		}
		
		char ReadChar() {
			return Convert.ToChar(json.Read());
		}
		
		string ReadWord() {
			StringBuilder word = new StringBuilder();
			while (WORD_BREAK.IndexOf(PeekChar()) == -1) {
				word.Append(ReadChar());
				if (EndReached()) return null;
			}
			return word.ToString();
		}

		void EatWhitespace() {
			while (!EndReached() && WHITE_SPACE.IndexOf(PeekChar()) != -1) {
				json.Read();
			}
		}

		Token PeekToken() {
			EatWhitespace();
			if (EndReached()) return Token.None;
			switch (PeekChar()) {
				case '{':
					return Token.CurlyOpen;
				case '}':
					return Token.CurlyClose;
				case '[':
					return Token.SquareOpen;
				case ']':
					return Token.SquareClose;
				case ',':
					return Token.Comma;
				case '"':
					return Token.String;
				case ':':
					return Token.Colon;
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case '-':
					return Token.Number;
				case 't':
				case 'f':
				case 'n':
					return Token.BoolOrNull;
			}
			return Token.None;
		}
	
		//** Parsing Parts **//

		internal object ParseBoolOrNull() {
			if (PeekToken() == Token.BoolOrNull) {
				string value = ReadWord();
				if (value == "true") return true;
				if (value == "false") return false;
				if (value == "null") return null;
				Console.WriteLine("unexpected value: " + value);
				return null;
			} else {
				Console.WriteLine("unexpected token: " + PeekToken());
				return null;
			}
		}

		internal object ParseNumber() {
			if (PeekToken() == Token.Number) {
				string number = ReadWord();
				if (number.IndexOf('.') == -1) {
					long parsedInt;
					Int64.TryParse(number, out parsedInt);
					return parsedInt;
				}
				double parsedDouble;
				Double.TryParse(number, out parsedDouble);
				return parsedDouble;
			} else {
				Console.WriteLine("unexpected token: " + PeekToken());
				return null;
			}
		}

		internal string ParseString() {
			if (PeekToken() == Token.String) {
				ReadChar(); // ditch opening quote

				StringBuilder s = new StringBuilder();
				while (true) {
					if (EndReached()) return null;
					
					char c = ReadChar();
					switch (c) {
						case '"':
							return s.ToString();
						case '\\':
							if (EndReached()) return null;
							
							c = ReadChar();
							switch (c) {
								case '"':
								case '\\':
								case '/':
									s.Append(c);
									break;
								case 'b':
									s.Append('\b');
									break;
								case 'f':
									s.Append('\f');
									break;
								case 'n':
									s.Append('\n');
									break;
								case 'r':
									s.Append('\r');
									break;
								case 't':
									s.Append('\t');
									break;
								case 'u':
									var hex = new StringBuilder();
									for (int i = 0; i < 4; i++) {
										hex.Append(ReadChar());
									}
									s.Append((char) Convert.ToInt32(hex.ToString(), 16));
									break;
							}
							break;
						default:
							s.Append(c);
							break;
					}
				}
			} else {
				Console.WriteLine("unexpected token: " + PeekToken());
				return null;
			}
		}

		//** Parsing Objects **//

		internal Dictionary<string, object> ParseObject() {
			if (PeekToken() == Token.CurlyOpen) {
				json.Read(); // ditch opening brace

				Dictionary<string, object> table = new Dictionary<string, object>();
				while (true) {
					switch (PeekToken()) {
					case Token.None:
						return null;
					case Token.Comma:
						json.Read();
						continue;
					case Token.CurlyClose:
						json.Read();
						return table;
					default:
						string name = ParseString();
						if (string.IsNullOrEmpty(name)) return null;

						if (PeekToken() != Token.Colon) return null;
						json.Read(); // ditch the colon
						
						table[name] = ParseValue();
						break;
					}
				}
			} else {
				Console.WriteLine("unexpected token: " + PeekToken());
				return null;
			}
		}
		
		internal List<object> ParseArray() {
			if (PeekToken() == Token.SquareOpen) {
				json.Read(); // ditch opening brace

				List<object> array = new List<object>();				
				while (true) {
					switch (PeekToken()) {
					case Token.None:			
						return null;
					case Token.Comma:			
						json.Read(); 
						continue;						
					case Token.SquareClose:	
						json.Read(); 
						return array;
					default:	
						array.Add(ParseValue()); 
						break;
					}
				}
			} else {
				Console.WriteLine("unexpected token: " + PeekToken());
				return null;
			}
		}

		internal object ParseValue() {
			switch (PeekToken()) {
			case Token.String:		
				return ParseString();
			case Token.Number:		
				return ParseNumber();
			case Token.BoolOrNull:		
				return ParseBoolOrNull();
			case Token.CurlyOpen:	
				return ParseObject();
			case Token.SquareOpen:	
				return ParseArray();
			}
			Console.WriteLine("unexpected token: " + PeekToken());
			return null;
		}
	}
}

