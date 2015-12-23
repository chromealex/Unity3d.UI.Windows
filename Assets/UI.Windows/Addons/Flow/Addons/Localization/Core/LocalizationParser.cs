using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Localization {

	public static class LocalizationParser {

		#region CSV

		public static List<string[]> ReadCSV(string text) {
			
			var iStart = 0;
			var csv = new List<string[]>();

			while (iStart < text.Length) {
				
				var list = ParseCSVline(text, ref iStart);
				if (list == null) break;

				csv.Add(list);

			}
			return csv;

		}

		private static string[] ParseCSVline(string line, ref int iStart) {
			
			var list = new List<string>();

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

					if (c == '\n' || c == ',') {
						
						LocalizationParser.AddCSVtoken(ref list, ref line, iStart, ref iWordStart);
						if (c == '\n') {  // Stop the row on line breaks
							
							iStart++;
							break;

						}

					} else { //--------[ Start Quote ]--------------------

						if (c == '\"') {
							
							insideQuote = true;

						}

					}

				}

				iStart++;

			}

			if (iStart > iWordStart) {
				
				LocalizationParser.AddCSVtoken(ref list, ref line, iStart, ref iWordStart);

			}

			return list.ToArray();

		}

		private static void AddCSVtoken(ref List<string> list, ref string line, int iEnd, ref int iWordStart) {
			
			var text = line.Substring(iWordStart, iEnd - iWordStart);
			iWordStart = iEnd + 1;

			text = text.Replace("\"\"", "\"");
			if (text.Length > 1 && text[0] == '\"' && text[text.Length - 1] == '\"') {

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