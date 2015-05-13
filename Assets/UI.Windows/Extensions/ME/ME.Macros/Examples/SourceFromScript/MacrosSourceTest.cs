using UnityEngine;
using System.Collections;

public class MacrosSourceTest : MonoBehaviour {

	#region source macros TEST1Fields
	public int field1;
	#endregion

	// Use this for initialization
	void Start () {
		
		#region source macros TEST1
		var a = 260;
		--a;
		++a;
		Debug.Log(a);
		#endregion

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
