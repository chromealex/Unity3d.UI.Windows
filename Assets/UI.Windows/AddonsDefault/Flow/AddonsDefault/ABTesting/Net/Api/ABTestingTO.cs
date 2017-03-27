using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Extensions;
using System.Linq;

namespace UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api {

	public class ConditionBaseTO {

		public byte type;

		public ConditionBaseTO() {
		}
		public ConditionBaseTO(ConditionBase.Type type) {

			this.type = (byte)type;

		}

	}

	public class LongConditionTO : ConditionBaseTO {

		public LongConditionTO() {
		}
		public LongConditionTO(ConditionBase.Type type) : base(type) {}

		public long from;
		public long to;
		
	}
	
	public class EnumConditionTO : ConditionBaseTO {

		public EnumConditionTO() {
		}
		public EnumConditionTO(ConditionBase.Type type) : base(type) {}

		public int value;
		
	}
	
	public class IntConditionTO : ConditionBaseTO {

		public IntConditionTO() {
		}
		public IntConditionTO(ConditionBase.Type type) : base(type) {}

		public int from;
		public int to;
		
	}
	
	public class FloatConditionTO : ConditionBaseTO {

		public FloatConditionTO() {
		}
		public FloatConditionTO(ConditionBase.Type type) : base(type) {}

		public float from;
		public float to;
		
	}
	
	public class DoubleConditionTO : ConditionBaseTO {

		public DoubleConditionTO() {
		}
		public DoubleConditionTO(ConditionBase.Type type) : base(type) {}

		public double from;
		public double to;
		
	}

	public class PrefsBaseTO {

		public string name;

	}

	public class IntPrefsTO : PrefsBaseTO {
		
		public List<IntConditionTO> values;
		
	}
	
	public class FloatPrefsTO : PrefsBaseTO {
		
		public List<FloatConditionTO> values;
		
	}

	public class AttachItemTO {
		
		public int targetId;
		public int index;

	}

	public class ABTestingItemTO {

		public string comment;

		public AttachItemTO attachItem;

		public List<LongConditionTO> userId;
		public List<EnumConditionTO> userGender;
		public List<IntConditionTO> userBirthYear;
		
		public List<IntPrefsTO> prefsInt;
		public List<FloatPrefsTO> prefsFloat;

	}

	public class ABTestingItemsTO {

		public int sourceWindowId;

		public List<ABTestingItemTO> items;

	}

	public class ABTestingJsonItemTO {

		public int testId;

		public string jsonData;

		public ABTestingJsonItemTO() {
		}
		public ABTestingJsonItemTO(int testId, string jsonData) {
			this.testId = testId;
			this.jsonData = jsonData;
		}
	}

}