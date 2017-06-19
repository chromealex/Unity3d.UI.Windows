using System.Collections;
using System.Collections.Generic;
using ME;

namespace UnityEngine.UI.Windows.Utilities {

	public static class CSVParser {

		#region CSV
		public static List<string[]> ReadCSV(string text) {
			
			var iStart = 0;
			var csv = new List<string[]>();

			text = text.Replace("\r\n", "\n");

			while (iStart < text.Length) {
				
				var list = ParseCSVline(text, ref iStart);
				if (list == null) break;

				csv.Add(list);

			}
			return csv;

		}

		private static string[] ParseCSVline(string line, ref int iStart) {

			var list = ListPool<string>.Get();

			var textLength = line.Length;
			var iWordStart = iStart;
			var insideQuote = false;

			while (iStart < textLength) {
				
				var c = line[iStart];

				if (insideQuote) {
					
					if (c == '\"') { //--[ Look for Quote End ]------------
						
						if (iStart + 1 >= textLength || line[iStart + 1] != '\"') {  //-- Single Quote:  Quotation Ends

							insideQuote = false;

						} else if (iStart + 2 < textLength && line[iStart + 2] == '\"') {  //-- Tripple Quotes: Quotation ends
							
							insideQuote = false;
							iStart += 2;

						} else {

							iStart++;  // Skip Double Quotes

						}

					}

				} else { //-----[ Separators ]----------------------

					if (c == ',') {

						CSVParser.AddCSVtoken(ref list, ref line, iStart, ref iWordStart);

					} else if (c == '\n' || c == '\r') {

						break;

					} else { //--------[ Start Quote ]--------------------

						if (c == '\"') {
							
							insideQuote = true;

						}

					}

				}

				iStart++;

			}

			CSVParser.AddCSVtoken(ref list, ref line, iStart, ref iWordStart);

			if (iStart < textLength) {

				iStart++;

			}

			var arr = list.ToArray();
			ListPool<string>.Release(list);

			return arr;

		}

		private static void AddCSVtoken(ref List<string> list, ref string line, int iEnd, ref int iWordStart) {
			
			var text = line.Substring(iWordStart, iEnd - iWordStart);
			iWordStart = iEnd + 1;

			text = text.Replace(@"""""", @"""").Trim();
			if (text.Length > 1 && text[0] == '"' && text[text.Length - 1] == '"') {

				text = text.Substring(1, text.Length - 2);

			}

			if (text.Contains(",") == true) {

				text = text.Trim().Trim(' ', '"');

			}

			list.Add(text);

		}
		#endregion

	}

}