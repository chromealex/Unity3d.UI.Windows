using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Analytics;
using UnityEngine.UI.Windows.UserInfo;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Net.Api {

    public enum StatType : byte {
        OnStatEvent = 0,
        OnStatScreenTransition,
        OnStatTransaction,
        OnStatScreenPoint,
        OnStatGetScreen,
        OnStatResScreen,
        OnStatGetScreenTransition,
        OnStatResScreenTransition,
		OnStatSetUserId,
		OnStatSetUserGender,
		OnStatUserBirthYear,
		OnStatGetHeatmapData,
		OnStatResHeatmapData,
		OnStatSetUserDuid,

    }

	[System.Serializable]
	public class AuthTO  {
		public static short version = 1;
		public string key;
	}

    public class StatTO {
        private StatType statType;

        protected StatTO(StatType statType) {
            this.statType = statType;
        }

        public StatType GetTypeTO() {
            return statType;
        }
    }

    [System.Serializable]
    public class StatEvent : StatTO {
        public StatEvent() : base(StatType.OnStatEvent) {
        }

        public StatEvent(int screenId, string group1, string group2, string group3, int weight) : this() {
            this.screenId = screenId;
            this.group1 = group1;
            this.group2 = group2;
            this.group3 = group3;
            this.weight = weight;
        }

        public int screenId;
        public string group1;
        public string group2;
        public string group3;
        public int weight;

    }

    [System.Serializable]
    public class StatScreenTransition : StatTO {
        public StatScreenTransition() : base(StatType.OnStatScreenTransition) {
        }
        public StatScreenTransition(int index, int screenId, int toScreenId, bool popup) : this() {
            this.index = index;
            this.screenId = screenId;
            this.toScreenId = toScreenId;
            this.popup = popup;
        }
        public int index;
        public int screenId;
        public int toScreenId;
	    public bool popup;
    }

    [System.Serializable]
    public class StatTransaction : StatTO {
        public StatTransaction() : base(StatType.OnStatTransaction) {
        }
        public StatTransaction(int screenId, string productId, decimal price, string currency, string receipt, string signature) : this() {
            this.screenId = screenId;
            this.productId = productId;
            this.price = price;
            this.currency = currency;
            this.receipt = receipt;
            this.signature = signature;
        }
        public int screenId;
        public string productId;
        public decimal price;
        public string currency;
        public string receipt;
        public string signature;
    }

    [System.Serializable]
    public class StatScreenPoint : StatTO {
        public StatScreenPoint() : base(StatType.OnStatScreenPoint) {
        }
        public StatScreenPoint(int screenId, int screenWidth, int screenHeight, byte tag, float x, float y) : this() {
            this.screenId = screenId;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.tag = tag;
            this.x = x;
            this.y = y;
        }
        public int screenId;
        public int screenWidth;
        public int screenHeight;
        public byte tag;
        public float x;
        public float y;
    }

	[System.Serializable]
	public class StatSetUserId : StatTO {
		public StatSetUserId() : base(StatType.OnStatSetUserId) {
		}
		public StatSetUserId(string id) : this() {
			this.id = id;
		}
		public string id;
	}

	[System.Serializable]
	public class StatSetUserDuid : StatTO {
		public StatSetUserDuid() : base(StatType.OnStatSetUserDuid) {
		}
		public StatSetUserDuid(string id) : this() {
			this.id = id;
		}
		public string id;
	}

	[System.Serializable]
	public class StatSetUserGender : StatTO {
		public StatSetUserGender() : base(StatType.OnStatSetUserGender) {
		}
		public StatSetUserGender(string gender) : this() {
			this.gender = gender;
		}
		public string gender;
	}

	[System.Serializable]
	public class StatUserBirthYear : StatTO {
		public StatUserBirthYear() : base(StatType.OnStatUserBirthYear) {
		}
		public StatUserBirthYear(int birthYear) : this() {
			this.birthYear = birthYear;
		}
		public int birthYear;
	}

#if UNITY_EDITOR
    [System.Serializable]
    public abstract class StatResTOB : StatTO {
        public StatResTOB(StatType type) : base(type) {
        }

		public abstract Result GetResult();

	    public int idx;
	}

    [System.Serializable]
    public class StatResTO<T> : StatResTOB where T : Result {
        public StatResTO(StatType type) : base(type) {
        }

		public T result;

        public override Result GetResult() {
		    return result;
		}
    }

    [System.Serializable]
    public class StatReqTO : StatTO {
        public StatReqTO(StatType type) : base(type) {
        }

        public int idx;
    }

	[System.Serializable]
    public class StatGetScreen : StatReqTO {
        public StatGetScreen() : base(StatType.OnStatGetScreen) {
        }
        public StatGetScreen(int screenId) : this() {
            this.screenId = screenId;
        }
        public string key;
        public int screenId;
    }

    [System.Serializable]
    public class StatGetScreenTransition : StatReqTO {
        public StatGetScreenTransition() : base(StatType.OnStatGetScreenTransition) {
        }
        public StatGetScreenTransition(int index, int screenId, int toScreenId) : this() {
            this.index = index;
            this.screenId = screenId;
            this.toScreenId = toScreenId;
        }
        public string key;
        public int index;
        public int screenId;
        public int toScreenId;
    }

    [System.Serializable]
    public class StatGetHeatmapData : StatReqTO {
        public StatGetHeatmapData() : base(StatType.OnStatGetHeatmapData) {
        }
        public StatGetHeatmapData(int screenId, int screenWidth, int screenHeight, UserFilter filter) : this() {
            this.screenId = screenId;
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.filter = filter;
        }
		public int screenId;
		public int screenWidth;
		public int screenHeight;
		public UserFilter filter;
    }

    [System.Serializable]
    public class StatResScreen : StatResTO<ScreenResult> {
        public StatResScreen() : base(StatType.OnStatResScreen) {
        }
    }

    [System.Serializable]
    public class StatResScreenTransition : StatResTO<ScreenResult> {
        public StatResScreenTransition() : base(StatType.OnStatResScreenTransition) {
        }
    }

    [System.Serializable]
    public class StatResHeatmapData : StatResTO<HeatmapResult> {
        public StatResHeatmapData() : base(StatType.OnStatResHeatmapData) {
        }
    }
#endif
}
