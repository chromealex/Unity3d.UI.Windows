using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using ME;
using UnityEngine.UI.Windows.Extensions;
using UnityEngine.UI.Windows.Types;
using UnityEngine.UI.Windows.Animations;
using System;
using UnityEngine.UI.Windows.Styles;

namespace UnityEngine.UI.Windows.Plugins.Flow {
	
	[Obsolete("CompletedState was depricated. Use Flow.Data.CompletedState instead.")]
	public enum CompletedState : byte {
		NotReady,
		ReadyButWarnings,
		Ready,
	};

	[System.Serializable]
	public class DefaultElement {

		// Where is the element located?
		public LayoutTag tag;
		public string comment;

		// Whats the element?
		public WindowComponentLibraryLinker library;
		public string elementPath;
		public WindowComponent elementComponent;

	}
	
	[Obsolete("FlowWindow was depricated. Use Flow.Data.FlowWindow instead.")]
	[System.Serializable]
	public class FlowWindow {
		
		[System.Serializable]
		public class AttachItem {

			public static readonly AttachItem Empty = new AttachItem(-1);

			public int targetId;
			
			public TransitionBase transition;
			public TransitionInputParameters transitionParameters;

			#if UNITY_EDITOR
			[HideInInspector]
			public IPreviewEditor editor;
			#endif

			public AttachItem(int targetId) {

				this.targetId = targetId;

			}

		}

		[System.Serializable]
		public struct ComponentLink {
			
			[Header("Base")]
			public int targetWindowId;
			public LayoutTag sourceComponentTag;
			#if UNITY_EDITOR
			[Header("Editor-Only")]
			public string comment;
			#endif

			public ComponentLink(int targetWindowId, LayoutTag sourceComponentTag, string comment) {

				this.targetWindowId = targetWindowId;
				this.sourceComponentTag = sourceComponentTag;
				#if UNITY_EDITOR
				this.comment = string.IsNullOrEmpty(comment) ? string.Empty : ("On " + comment.ToLower().UppercaseWords().Trim().Replace(" ", "") + " Action");
				#endif

			}

		}

		private const int STATES_COUNT = 3;

		public enum Flags : int {

			Default = 0x0,

			IsContainer = 0x1,
			IsSmall = 0x2,
			
			CantCompiled = 0x4,
			
			ShowDefault = 0x8,
			IsFunction = 0x10,
			Reserved1 = 0x20,
			Reserved2 = 0x40,
			Reserved3 = 0x80,
			
			Tag1 = 0x100,
			Tag2 = 0x200,
			Tag3 = 0x400,
			Tag4 = 0x800,
			Tag5 = 0x1000,
			Tag6 = 0x2000,
			Tag7 = 0x4000,
			Tag8 = 0x8000,
			Tag9 = 0x10000,
			Tag10 = 0x20000,
			Tag11 = 0x40000,
			Tag12 = 0x80000,

		};

		public enum StoreType : byte {

			NewScreen,
			ReUseScreen,

		};

		public int id;
		[BitMask(typeof(Flags))]
		public Flags flags;
		public string title = string.Empty;
		public string directory = string.Empty;
		public Rect rect;

		[Obsolete("Attaches not supported. Use FlowData.Upgrade() context function to fix it.")]
		public List<int> attaches = new List<int>();
		public List<ComponentLink> attachedComponents = new List<ComponentLink>();
		public List<AttachItem> attachItems = new List<AttachItem>();

		public Color randomColor;

		public bool isVisibleState = false;

		public int functionRootId = 0;
		public int functionExitId = 0;
		public int functionId = 0;

		public StoreType storeType = StoreType.NewScreen;

		public List<int> tags = new List<int>();

		public List<DefaultElement> comments = new List<DefaultElement>();

		public bool compiled = false;
		public string compiledDirectory = string.Empty;
		public string compiledNamespace = string.Empty;
		public string compiledScreenName = string.Empty;
		public string compiledBaseClassName = string.Empty;
		public string compiledDerivedClassName = string.Empty;
		
		public string smallStyleDefault = "flow node 4";
		public string smallStyleSelected = "flow node 4 on";

		public CompletedState[] states = new CompletedState[STATES_COUNT];

		public int screenWindowId;	// used when storeType == ReUseScreen
		public WindowBase screen;

	}

}