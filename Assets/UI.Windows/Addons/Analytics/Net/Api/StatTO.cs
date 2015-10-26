using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Analytics;

namespace UnityEngine.UI.Windows.Plugins.Analytics.Net.Api {

    public enum StatType : byte {
        OnStatEvent = 0,
        OnStatScreenTransition,
        OnStatTransition,
        OnStatScreenPoint,
        OnStatGetScreen,
        OnStatResScreen,
        OnStatGetScreenTransition,
        OnStatResScreenTransition
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
        public StatScreenTransition(int index, int screenId, int toScreenId) : this() {
            this.index = index;
            this.screenId = screenId;
            this.toScreenId = toScreenId;
        }
        public int index;
        public int screenId;
        public int toScreenId;
    }

    [System.Serializable]
    public class StatTransition : StatTO {
        public StatTransition() : base(StatType.OnStatTransition) {
        }
        public StatTransition(int screenId, string productId, decimal price, string currency, string receipt, string signature) : this() {
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

#if UNITY_EDITOR
    [System.Serializable]
    public class StatResTO : StatTO {
        public StatResTO(StatType type) : base(type) {
        }

        public long idx;
        public ScreenResult result;
    }

    [System.Serializable]
    public class StatReqTO : StatTO {
        public StatReqTO(StatType type) : base(type) {
        }

        public long idx;
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
    public class StatResScreen : StatResTO {
        public StatResScreen() : base(StatType.OnStatResScreen) {
        }
    }

    [System.Serializable]
    public class StatResScreenTransition : StatResTO {
        public StatResScreenTransition() : base(StatType.OnStatResScreenTransition) {
        }
    }
#endif
}
