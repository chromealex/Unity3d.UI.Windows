using UnityEngine;
using System.Collections;

namespace UnityEngine.UI.Windows.Extensions {

	public interface IProjectDependency {

		IProjectDependency[] GetProjectDependencies();

	};

}
