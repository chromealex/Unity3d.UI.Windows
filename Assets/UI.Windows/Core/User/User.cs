using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.UserInfo {

	public enum Gender : byte {

		Any,
		Male,
		Female,

	};

	[System.Serializable]
	public class UserFilter {

		[System.NonSerialized]
		public string dateFrom;
		[System.NonSerialized]
		public string dateTo;
		[System.NonSerialized]
		public long idFrom;
		[System.NonSerialized]
		public long idTo;
		[System.NonSerialized]
		public Gender gender;
		[System.NonSerialized]
		public int birthYearFrom;
		[System.NonSerialized]
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
			this.gender = (Gender)UnityEditor.EditorGUILayout.EnumPopup("Gender", this.gender);
			
			this.birthYearFrom = UnityEditor.EditorGUILayout.Popup("Birth Year From", this.birthYearFrom, this.yearsArray);
			this.birthYearTo = UnityEditor.EditorGUILayout.Popup("Birth Year To", this.birthYearTo, this.yearsArray);

			changed = GUI.changed;

			return changed;

		}
		#endif

	}

	public class User {

		public System.Action<long> onSetUserId;
		public System.Action<string> onSetUserId2;
		public System.Action<int> onSetBirthYear;
		public System.Action<Gender> onSetGender;

		private long _id;
		public long id {
			
			set {
				
				this._id = value;
				if (this.onSetUserId != null) this.onSetUserId.Invoke(value);

			}
			
			get {
				
				return this._id;
				
			}
			
		}
		
		private string _id2;
		public string id2 {
			
			set {
				
				this._id2 = value;
				if (this.onSetUserId2 != null) this.onSetUserId2.Invoke(value);

			}
			
			get {
				
				return this._id2;
				
			}
			
		}

		private Gender _gender;
		public Gender gender {
			
			set {

				this._gender = value;
				if (this.onSetGender != null) this.onSetGender.Invoke(value);

			}
			
			get {
				
				return this._gender;
				
			}
			
		}
		
		private int _birthYear;
		public int birthYear {
			
			set {
				
				this._birthYear = value;
				if (this.onSetBirthYear != null) this.onSetBirthYear.Invoke(value);

			}
			
			get {
				
				return this._birthYear;
				
			}
			
		}

		private string _customParameter;
		public string customParameter {

			set {

				this._customParameter = value;

			}

			get {

				if (this._customParameter == null) return string.Empty;

				return this._customParameter;

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