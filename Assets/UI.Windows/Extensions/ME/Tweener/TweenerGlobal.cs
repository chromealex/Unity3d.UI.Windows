using UnityEngine;

public class TweenerGlobal : MonoBehaviour {
	
	public static ME.Tweener instance;
	public static ME.Tweener gameTimeInstance;
	
	public ME.Tweener tweener;
	public ME.Tweener gameTimeTweener;

    //private static TweenerGlobal _instance;

    public virtual void Awake() {
		
		TweenerGlobal.instance = this.tweener;
		TweenerGlobal.gameTimeInstance = this.gameTimeTweener;

        //TweenerGlobal._instance = this;

    }

    protected virtual void OnDestory() {

        //TweenerGlobal._instance = null;

        if (TweenerGlobal.instance != null) {
            
            Destroy(TweenerGlobal.instance);
            TweenerGlobal.instance = null;

        }

        if (TweenerGlobal.gameTimeInstance != null) {

            Destroy(TweenerGlobal.gameTimeInstance);
            TweenerGlobal.gameTimeInstance = null;

        }

    }

    public static void Count() {

        Debug.Log("~~~ TweenerGlobal.instance = " + TweenerGlobal.instance.Count);
        Debug.Log("~~~ TweenerGlobal.gameTimeInstance = " + TweenerGlobal.gameTimeInstance.Count);

    }

}
