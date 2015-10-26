using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Plugins.Analytics {

	[System.Serializable]
	public class UserFilter {
		
		public string dateFrom;
		public string dateTo;
		public long idFrom;
		public long idTo;
		public User.Gender gender;
		public int birthYearFrom;
		public int birthYearTo;

		#if UNITY_EDITOR
		private string[] yearsArray = null;
		public bool OnGUI(GUISkin skin) {

			var changed = false;

			if (this.yearsArray == null || this.yearsArray.Length == 0) {
				
				var years = new List<string>();
				years.Add("Any");
				for (int i = 1900; i <= System.DateTime.Now.Year; ++i) {
					
					years.Add(i.ToString());
					
				}
				
				this.yearsArray = years.ToArray();
				
			}

			UnityEditor.EditorGUILayout.LabelField("Filter:", UnityEditor.EditorStyles.boldLabel);

			ME.CalendarGUI.DrawField("Date From", this.dateFrom, (date) => this.dateFrom = date);
			ME.CalendarGUI.DrawField("Date To", this.dateTo, (date) => this.dateTo = date);
			
			this.idFrom = UnityEditor.EditorGUILayout.LongField("User ID From", this.idFrom, skin.textField);
			this.idTo = UnityEditor.EditorGUILayout.LongField("User ID To", this.idTo, skin.textField);
			this.gender = (User.Gender)UnityEditor.EditorGUILayout.EnumPopup("Gender", this.gender);
			
			this.birthYearFrom = UnityEditor.EditorGUILayout.Popup("Birth Year From", this.birthYearFrom, this.yearsArray);
			this.birthYearTo = UnityEditor.EditorGUILayout.Popup("Birth Year To", this.birthYearTo, this.yearsArray);

			changed = GUI.changed;

			return changed;

		}
		#endif

	}

	public class User {
		
		public enum Gender : byte {
			
			Any,
			Male,
			Female,
			
		};
		
		private long _id;
		public long id {
			
			set {
				
				this._id = value;
				Analytics.SetUserId(value);
				
			}
			
			get {
				
				return this._id;
				
			}
			
		}
		
		private string _id2;
		public string id2 {
			
			set {
				
				this._id2 = value;
				Analytics.SetUserId(value);
				
			}
			
			get {
				
				return this._id2;
				
			}
			
		}

		private Gender _gender;
		public Gender gender {
			
			set {

				this._gender = value;
				Analytics.SetUserGender(value);
				
			}
			
			get {
				
				return this._gender;
				
			}
			
		}
		
		private int _birthYear;
		public int birthYear {
			
			set {
				
				this._birthYear = value;
				Analytics.SetUserBirthYear(birthYear);
				
			}
			
			get {
				
				return this._birthYear;
				
			}
			
		}

		private static User _instance = new User();
		public static User instance {
			
			get {
				
				return User._instance;
				
			}
			
		}
		
	};

}