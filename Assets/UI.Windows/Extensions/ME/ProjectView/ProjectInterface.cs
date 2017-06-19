using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	public interface IProjectDependency {

		#if UNITY_EDITOR
		IProjectDependency[] GetProjectDependencies();
		#endif

	};

}
