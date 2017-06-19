using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Windows.Plugins.Flow;
using FD = UnityEngine.UI.Windows.Plugins.Flow.Data;
using UnityEngine.UI.Windows.Extensions;
using System.Linq;
using UnityEngine.UI.Windows.Plugins.ABTesting.Net.Api;
using UnityEngine.UI.Windows.UserInfo;

#if UNITY_EDITOR
using UnityEditor.UI.Windows.Internal.ReorderableList.Internal;
using UnityEditor.UI.Windows.Internal.ReorderableList;
using UnityEditor.UI.Windows.Extensions;
using UnityEditor;
using UnityEditorInternal;
#endif

namespace UnityEngine.UI.Windows.Plugins.ABTesting {

	public interface IBaseCondition {
		
		#if UNITY_EDITOR
		void OnGUI();
		#endif

	};

	[System.Serializable]
	public class ABTestingItem {

		[System.Serializable]
		public class AttachItem {

			public int targetId = -1;
			public int index = 0;

			public AttachItem() {}

			public AttachItem(AttachItemTO source) {

				this.targetId = source.targetId;
				this.index = source.index;

			}

			public AttachItemTO GetTO() {

				return new AttachItemTO() { targetId = this.targetId, index = this.index };

			}

		}
		
		public AttachItem attachItem = new AttachItem();

		public List<LongCondition> userId = new List<LongCondition>();
		public List<EnumCondition> userGender = new List<EnumCondition>();
		public List<IntCondition> userBirthYear = new List<IntCondition>();
		
		public List<IntPrefs> prefsInt = new List<IntPrefs>();
		public List<FloatPrefs> prefsFloat = new List<FloatPrefs>();

		public ABTestingItem() {}

		public ABTestingItem(ABTestingItemTO source) {

			this.attachItem = new AttachItem(source.attachItem);
			
			this.userId = new List<LongCondition>();
			foreach (var item in source.userId) this.userId.Add(new LongCondition(item));
			
			this.userGender = new List<EnumCondition>();
			foreach (var item in source.userGender) this.userGender.Add(new EnumCondition(item));
			
			this.userBirthYear = new List<IntCondition>();
			foreach (var item in source.userBirthYear) this.userBirthYear.Add(new IntCondition(item));
			
			this.prefsInt = new List<IntPrefs>();
			foreach (var item in source.prefsInt) this.prefsInt.Add(new IntPrefs(item));
			
			this.prefsFloat = new List<FloatPrefs>();
			foreach (var item in source.prefsFloat) this.prefsFloat.Add(new FloatPrefs(item));

			#if UNITY_EDITOR
			this.comment = source.comment;
			#endif

		}

		public ABTestingItemTO GetTO() {

			var result = new ABTestingItemTO();

			result.attachItem = this.attachItem.GetTO();
			
			result.userId = new List<LongConditionTO>();
			foreach (var item in this.userId) result.userId.Add(item.GetTO());
			
			result.userGender = new List<EnumConditionTO>();
			foreach (var item in this.userGender) result.userGender.Add(item.GetTO());
			
			result.userBirthYear = new List<IntConditionTO>();
			foreach (var item in this.userBirthYear) result.userBirthYear.Add(item.GetTO());
			
			result.prefsInt = new List<IntPrefsTO>();
			foreach (var item in this.prefsInt) result.prefsInt.Add(item.GetTO());
			
			result.prefsFloat = new List<FloatPrefsTO>();
			foreach (var item in this.prefsFloat) result.prefsFloat.Add(item.GetTO());

			#if UNITY_EDITOR
			result.comment = this.comment;
			#endif

			return result;

		}

		public bool Resolve() {

			var result = true;

			var userInfo = User.instance;
			if (result == true) result = this.ResolveLong_INTERNAL(this.userId, userInfo.id);
			if (result == true) result = this.ResolveEnum_INTERNAL(this.userGender, (int)userInfo.gender);
			if (result == true) result = this.ResolveInt_INTERNAL(this.userBirthYear, userInfo.birthYear);
			if (result == true) result = this.ResolvePrefs_INTERNAL(this.prefsInt);
			if (result == true) result = this.ResolvePrefs_INTERNAL(this.prefsFloat);

			return result;

		}

		private bool Resolve_INTERNAL<T>(IList list, System.Func<T, bool> onCondition) where T : IBaseCondition {

			var result = true;
			for (int i = 0; i < list.Count; ++i) {
				
				var item = list[i];
				if (result == true) {
					
					result = onCondition((T)item);
					
				} else {
					
					break;
					
				}
				
			}
			
			return result;

		}
		
		public bool ResolvePrefs_INTERNAL(List<IntPrefs> list) {
			
			return this.Resolve_INTERNAL<IntPrefs>(list, (cond) => cond.Resolve());
			
		}
		
		public bool ResolvePrefs_INTERNAL(List<FloatPrefs> list) {
			
			return this.Resolve_INTERNAL<FloatPrefs>(list, (cond) => cond.Resolve());
			
		}

		public bool ResolveEnum_INTERNAL(IList list, int value) {
			
			return this.Resolve_INTERNAL<EnumCondition>(list, (cond) => cond.Resolve(value));
			
		}

		public bool ResolveLong_INTERNAL(IList list, long value) {
			
			return this.Resolve_INTERNAL<LongCondition>(list, (cond) => cond.Resolve(value));
			
		}

		public bool ResolveInt_INTERNAL(IList list, int value) {
			
			return this.Resolve_INTERNAL<IntCondition>(list, (cond) => cond.Resolve(value));
			
		}
		
		public bool ResolveDouble_INTERNAL(IList list, double value) {
			
			return this.Resolve_INTERNAL<DoubleCondition>(list, (cond) => cond.Resolve(value));
			
		}
		
		public bool ResolveFloat_INTERNAL(IList list, float value) {
			
			return this.Resolve_INTERNAL<FloatCondition>(list, (cond) => cond.Resolve(value));
			
		}

		#if UNITY_EDITOR
		public string comment;
		[System.NonSerialized]
		public Rect editorRect;

		public void OnGeneralGUI() {
			
			GUILayout.Label("Comment", EditorStyles.boldLabel);
			this.comment = EditorGUILayout.TextArea(this.comment);

		}

		public void OnUserInfoGUI(User userInfo) {

			this.Draw("ID", this.userId, () => {
				
				this.userId.Add(new LongCondition(ConditionBase.Type.From | ConditionBase.Type.To, 0L, 0L));
				
			});
			
			CustomGUI.Splitter();
			
			this.Draw("Gender", this.userGender, () => {
				
				this.userGender.Add(new EnumCondition(ConditionBase.Type.Strong, typeof(Gender), 0));
				
			}, onEach: (element) => {

				(element as EnumCondition).enumType = typeof(Gender);

			}, addButton: false, removeButton: false, minCount: 1, maxCount: 1);
			
			CustomGUI.Splitter();
			
			this.Draw("Age", this.userBirthYear, () => {
				
				this.userBirthYear.Add(new IntCondition(ConditionBase.Type.From | ConditionBase.Type.To, 15, 20));
				
			});

		}
		
		public void OnPlayerPrefsGUI() {
			
			for (int i = 0; i < this.prefsInt.Count; ++i) this.prefsInt[i].item = this;
			for (int i = 0; i < this.prefsFloat.Count; ++i) this.prefsFloat[i].item = this;
			
			this.Draw("Int Values", this.prefsInt, () => this.prefsInt.Add(new IntPrefs(string.Empty)));
			
			CustomGUI.Splitter();
			
			this.Draw("Float Values", this.prefsFloat, () => this.prefsFloat.Add(new FloatPrefs(string.Empty)));
			
		}

		public void Draw(string label, IList list, System.Action onAdd, System.Action<object> onEach = null, bool addButton = true, bool removeButton = true, int maxCount = -1, int minCount = -1) {

			var backStyle = ME.Utilities.CacheStyle("UI.Windows.ABTesting.Back", "Back", (name) => {
				
				var _style = new GUIStyle("LargeButtonLeft");
				_style.fixedWidth = 0f;
				return _style;
				
			});
			var removeButtonStyle = ME.Utilities.CacheStyle("UI.Windows.ABTesting.RemoveButton", "Remove", (name) => {
				
				var _style = new GUIStyle("LargeButtonRight");
				_style.fixedWidth = 30f;
				_style.fixedHeight = 30f;
				return _style;
				
			});
			var addButtonStyle = ME.Utilities.CacheStyle("UI.Windows.ABTesting.AddButton", "Add", (name) => {
				
				var _style = new GUIStyle("Button");
				_style.fixedWidth = 30f;
				return _style;
				
			});
			var centeredLabel = ME.Utilities.CacheStyle("UI.Windows.ABTesting.CenteredLabel", "CenteredLabel", (name) => {
				
				var _style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
				_style.margin = new RectOffset(0, 30, 0, 0);
				return _style;
				
			});


			GUILayout.BeginVertical();
			{

				GUILayout.BeginHorizontal();
				{

					GUILayout.Label(label, EditorStyles.boldLabel);
					
					GUILayout.FlexibleSpace();

					if (addButton == true) {

						var iconAddNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Add_Normal);
						if (GUILayout.Button(iconAddNormal, addButtonStyle) == true) {
							
							onAdd();
							
						}

					}

				}
				GUILayout.EndHorizontal();

				if (maxCount >= 0) {

					if (list.Count > maxCount) {
						
						list.RemoveAt(list.Count - 1);
						
					}

				}

				if (minCount >= 0) {

					if (list.Count < minCount) {
						
						onAdd();
						
					}

				}

				for (int i = 0; i < list.Count; ++i) {

					var item = list[i];
					if (onEach != null) onEach(item);

					GUILayout.BeginHorizontal();
					{

						GUILayout.BeginVertical(backStyle);
						{

							(item as IBaseCondition).OnGUI();

						}
						GUILayout.EndVertical();

						if (removeButton == true) {

							var iconNormal = ReorderableListResources.GetTexture(ReorderableListTexture.Icon_Remove_Normal);
							if (GUILayout.Button(iconNormal, removeButtonStyle) == true) {

								list.RemoveAt(i);
								break;

							}

						}

					}
					GUILayout.EndHorizontal();

					if (i < list.Count - 1) GUILayout.Label("- OR -", centeredLabel);

				}

			}
			GUILayout.EndHorizontal();

		}
		#endif

		public void Attach(int windowId, int attachIndex) {
			
			this.attachItem.targetId = windowId;
			this.attachItem.index = attachIndex;

		}

		public void Detach() {

			this.attachItem.targetId = -1;
			this.attachItem.index = 0;

		}

	}

	[System.Serializable]
	public class ABTestingItems {

		public int sourceWindowId = -1;
		public List<ABTestingItem> items = new List<ABTestingItem>();

		public ABTestingItems() {}

		public ABTestingItems(ABTestingItemsTO source) {

			this.sourceWindowId = source.sourceWindowId;
			this.items = new List<ABTestingItem>();
			foreach (var item in source.items) this.items.Add(new ABTestingItem(item));

		}
		
		public ABTestingItemsTO GetTO() {

			var result = new ABTestingItemsTO();

			result.sourceWindowId = this.sourceWindowId;

			result.items = new List<ABTestingItemTO>();
			foreach (var item in this.items) result.items.Add(item.GetTO());

			return result;

		}

		public int Resolve(FD.FlowWindow window, out AttachItem attachItem) {

			var wayId = 0;
			var item = this.items[0].attachItem;

			for (int i = 1; i < this.Count(); ++i) {

				var tItem = this.items[i];
				if (tItem.Resolve() == true) {

					wayId = i;
					item = tItem.attachItem;
					break;

				}

			}

			attachItem = null;

			var index = 0;
			var items = window.attachItems;
			for (int i = 0; i < items.Count; ++i) {

				if (items[i].targetId == item.targetId) {

					if (index == item.index) {

						attachItem = items[i];
						break;

					}

					++index;

				}
				
			}

			return wayId;
			
		}

		public int Count() {

			return this.items.Count;

		}

		public void Attach(int index, int windowId, int attachIndex) {

			this.items[index].Attach(windowId, attachIndex);

		}

		public void RemoveAt(int index) {

			this.items.RemoveAt(index);

		}

		public void AddNew() {
			
			this.items.Add(new ABTestingItem());

		}

		public void Validate(FD.FlowWindow window) {

			#if UNITY_EDITOR
			if (this.items == null) {

				this.items = new List<ABTestingItem>();
				EditorUtility.SetDirty(window);
				FlowSystem.SetDirty();
				FlowSystem.Save();

			}

			if (this.Count() < 2) {
				
				for (int i = this.Count(); i < 2; ++i) this.AddNew();
				EditorUtility.SetDirty(window);
				FlowSystem.SetDirty();
				FlowSystem.Save();

			}

			var data = FlowSystem.GetData();
			if (data != null) {

				var attaches = window.attachItems.ToArray();
				for (int i = 0; i < attaches.Count(); ++i) {
					
					var toId = attaches[i].targetId;
					var toIndex = attaches[i].index;

					if (this.items.FirstOrDefault(x => x.attachItem.targetId == toId && x.attachItem.index == toIndex) == null) {

						var fromId = window.id;
						if (FlowSystem.GetWindow(toId).IsContainer() == false) {

							EditorApplication.delayCall += () => {

								data.Detach(fromId, toIndex, toId, oneWay: false);

							};

						}

					}

				}

				for (int i = 0; i < this.Count(); ++i) {

					var item = this.items[i];

					if (data.AlreadyAttached(window.id, item.attachItem.index, item.attachItem.targetId) == false) {

						// no attachment - remove it
						item.Detach();

					}

				}

			}
			#endif

		}

	}

}