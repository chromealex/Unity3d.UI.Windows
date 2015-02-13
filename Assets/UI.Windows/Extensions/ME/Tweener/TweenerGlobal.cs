using UnityEngine;
using System.Collections;

public class TweenerGlobal : MonoBehaviour {
	
	public static ME.Tweener instance;
	public static ME.Tweener gameTimeInstance;
	
	public ME.Tweener tweener;
	public ME.Tweener gameTimeTweener;
	
	public void Awake() {
		
		TweenerGlobal.instance = this.tweener;
		TweenerGlobal.gameTimeInstance = this.gameTimeTweener;
		
	}
	
}
