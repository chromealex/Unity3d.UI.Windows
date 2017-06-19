using UnityEngine;
using UnityEngine.UI.Windows;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Extensions;

namespace UnityEngine.UI.Windows.Audio {
	
	public enum ClipType : byte {
		
		Music,
		SFX,
		
	};

	public class AudioPopupAttribute : BitmaskBaseAttribute {

		public ClipType clipType;
		public string fieldName;
		
		public AudioPopupAttribute(ClipType clipType) : base(false) {
			
			this.clipType = clipType;
			this.fieldName = null;
			
		}
		
		public AudioPopupAttribute(string fieldName) : base(false) {
			
			this.fieldName = fieldName;
			
		}

	};

}