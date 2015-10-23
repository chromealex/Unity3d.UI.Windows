#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using System.Globalization;
using UnityEditor;

namespace ME {

	public class CalendarGUI {

		public static void DrawField(string label, string date, System.Action<string> onSelect, string dateFormat = "d/M/yyyy") {

			GUILayout.BeginHorizontal();
			GUILayout.EndHorizontal();
			var allRect = GUILayoutUtility.GetLastRect();

			GUILayout.BeginHorizontal();
			{

				var indentedRect = EditorGUI.IndentedRect(new Rect());

				GUILayout.Space(indentedRect.x);
				GUILayout.Label(label, GUILayout.Width(EditorGUIUtility.labelWidth - indentedRect.x));

				var rect = GUILayoutUtility.GetLastRect();
				rect = new Rect(rect.x + rect.width, rect.y, allRect.width - rect.width - indentedRect.x, rect.height);
				var result = GUI.Button(rect, date, EditorStyles.popup);

				Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y + rect.height));
				rect.x = vector.x;
				rect.y = vector.y;

				if (result == true) {

					CalendarPopup.Create(rect, date, dateFormat, onSelect);

				} else {

					if (string.IsNullOrEmpty(date) == true) {

						onSelect(new Calendar(date, dateFormat).ToString());

					}
					
				}

			}
			GUILayout.EndHorizontal();

		}

	}

	public class CalendarPopup : EditorWindow {

		private const float MIN_WIDTH = 314f;
		private const float MIN_HEIGHT = 170f;

		public Rect screenRect;
		public string date;
		public string dateFormat;

		private Calendar calendar;
		private System.Action<string> onSelect;

		public static CalendarPopup Create(Rect screenRect, string date, string dateFormat, System.Action<string> onSelect) {

			var popup = CreateInstance<CalendarPopup>();
			popup.date = date;
			popup.dateFormat = dateFormat;
			popup.screenRect = screenRect;
			popup.onSelect = onSelect;

			popup.Init();

			return popup;

		}

		public void Init() {

			this.ShowAsDropDown(new Rect(this.screenRect.x, this.screenRect.y, 1, 1), new Vector2(Mathf.Max(CalendarPopup.MIN_WIDTH, this.screenRect.width), Mathf.Max(CalendarPopup.MIN_HEIGHT, this.screenRect.height)));
			this.Focus();
			this.wantsMouseMove = true;
			
			this.calendar = new Calendar(this.date, this.dateFormat);
			if (this.onSelect != null) this.onSelect(this.calendar.ToString());

		}

		public void OnGUI() {

			if (this.calendar == null) return;

			var newDate = this.calendar.OnGUI(GUI.skin.button);
			if (newDate != this.date) {

				this.date = newDate;
				if (this.onSelect != null) this.onSelect(this.date);

			}

		}

	}

	public class Calendar {

		public readonly string[] daysOfWeek = new string[7] {

			"Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"

		};
		
		public int[] dayLabels = new int[42];         //Holds 42 labels
		public string[] months = new string[12];            //Holds the months
		public string headerLabel;         //The label used to show the Month

		private int monthCounter = DateTime.Now.Month - 1;
		private int yearCounter = 0;
		private DateTime iMonth;
		private DateTime curDisplay;

		private string dateFormat;
		
		public Calendar(string date, string dateFormat) {

			this.dateFormat = dateFormat;

			CultureInfo enUS = new CultureInfo("en-US");
			DateTime result;
			if (DateTime.TryParseExact(date, dateFormat, enUS, DateTimeStyles.None, out result) == false) {

				result = DateTime.Now;

			}

			ClearLabels();
			CreateMonths(result);                
			CreateCalendar(iMonth);

		}

		private GUIStyle selected;
		public string OnGUI(GUIStyle button) {
			
			const float width = 40f;

			if (this.selected == null) {

				this.selected = new GUIStyle(button);
				this.selected.normal = this.selected.active;

			}
			
			GUILayout.BeginHorizontal();
			{

				if (GUILayout.Button("Prev") == true) {

					this.PreviousMonth();

				}

				GUILayout.FlexibleSpace();
				
				GUILayout.Label(this.headerLabel);

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Next") == true) {

					this.NextMonth();

				}
				
				if (GUILayout.Button("Now") == true) {
					
					iMonth = DateTime.Now;
					this.UpdateHeader();
					
				}

			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			{

				for (int i = 0; i < daysOfWeek.Length; ++i) {

					var dayOfWeek = daysOfWeek[i];
					GUILayout.Label(dayOfWeek, GUILayout.Width(width));

				}

			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginVertical();
			{
				
				GUILayout.BeginHorizontal();

				var xCount = 7;
				var k = 0;
				for (int i = 0; i < this.dayLabels.Length; ++i) {

					var value = this.dayLabels[i];
					if (value == 0) {

						var oldState = GUI.enabled;
						GUI.enabled = false;
						GUILayout.Button(string.Empty, button, GUILayout.Width(width));
						GUI.enabled = oldState;

					} else {

						var style = (this.dayLabels[i] == this.iMonth.Day) ? this.selected : button;

						if (GUILayout.Button(value.ToString(), style, GUILayout.Width(width)) == true) {

							this.CalendarItemClicked(value);

						}

					}

					++k;
					if (k >= xCount) {

						k = 0;
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();

					}

				}

				GUILayout.EndHorizontal();

			}
			GUILayout.EndVertical();

			return this.ToString();

		}

		public override string ToString() {

			return new DateTime(this.iMonth.Year, this.iMonth.Month, this.iMonth.Day).ToString(this.dateFormat);

		}

		private void UpdateHeader() {
			
			this.headerLabel = this.months[this.iMonth.Month - 1] + " " + this.iMonth.Year.ToString();

		}

		public void CalendarItemClicked(int day) {

			this.iMonth = new DateTime(this.iMonth.Year, this.iMonth.Month, day);

			ClosePopup();

		}

		/*Adds al the months to the Months Array and sets the current month in the header label*/
		private void CreateMonths(DateTime dateTime) {

			for (int i = 0; i < 12; ++i) {

				iMonth = new DateTime(dateTime.Year, i + 1, 1);
				months[i] = iMonth.ToString("MMMM");

			}

			iMonth = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
			this.UpdateHeader();

		}

		private void CreateCalendar(DateTime month) {

			curDisplay = new DateTime(month.Year, month.Month, 1);
			
			int curDays = GetDays(curDisplay.DayOfWeek);
			int index = 0;
			
			if (curDays > 0) {

				index = (curDays - 1);

			} else {

				index = curDays;

			}

			while (curDisplay.Month == month.Month) {

				dayLabels[index] = curDisplay.Day;
				curDisplay = curDisplay.AddDays(1);
				index++;

			}
			
			this.UpdateHeader();

		}
		
		private int GetDays(DayOfWeek day) {

			switch (day) {
				//case DayOfWeek.Sunday:
				//	return 1;
				case DayOfWeek.Monday:
					return 1;
				case DayOfWeek.Tuesday:
					return 2;
				case DayOfWeek.Wednesday:
					return 3;
				case DayOfWeek.Thursday:
					return 4;
				case DayOfWeek.Friday:
					return 5;
				case DayOfWeek.Saturday:
					return 6;
				case DayOfWeek.Sunday:
					return 7;
				default:
					throw new Exception("Unexpected DayOfWeek: " + day);
			}

		}
		
		public void NextMonth() {

			++monthCounter;
			if (monthCounter > 11) {

				monthCounter = 0;
				++yearCounter;

			}
			//headerLabel = iMonth.Year + " " + (DateTime.Now.Year + yearCounter);
			yearCounter = 0;
			ClearLabels();
			iMonth = iMonth.AddMonths(1);
			CreateCalendar(iMonth);

		}
		
		public void PreviousMonth() {

			--monthCounter;
			if (monthCounter < 0) {

				monthCounter = 11;
				--yearCounter;

			}
			
			//headerLabel = iMonth.Year + " " + (DateTime.Now.Year + yearCounter);
			yearCounter = 0;
			ClearLabels();
			iMonth = iMonth.AddMonths(-1);
			CreateCalendar(iMonth);

		}

		public void ShowPopup() {

		}
		
		public void ClosePopup() {

		}

		private void ClearLabels() {

			if (dayLabels == null) return;

			for (int x = 0; x < dayLabels.Length; ++x) {

				dayLabels[x] = 0;

			}
		}

	}

}
#endif