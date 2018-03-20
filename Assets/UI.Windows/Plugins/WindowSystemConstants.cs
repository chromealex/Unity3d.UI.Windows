
namespace UnityEngine.UI.Windows {

	public static class Constants {

		#if UNITY_EDITOR
		public const bool LOGS_ENABLED = true;
		#else
			#if LOGS_ENABLED
			public const bool LOGS_ENABLED = true;
			#else
			public const bool LOGS_ENABLED = false;
			#endif
		#endif

	}

}