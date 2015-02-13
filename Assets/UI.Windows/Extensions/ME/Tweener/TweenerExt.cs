using UnityEngine;

public static class TweenerExt
{
	public static ME.Tweener.Tween<CanvasGroup> addTweenAlpha(this ME.Tweener tweener, CanvasGroup canvasGroup, float duration, float end)
	{
		return tweener.addTweenAlpha(canvasGroup, duration, canvasGroup.alpha, end);
	}

	public static ME.Tweener.Tween<CanvasGroup> addTweenAlpha(this ME.Tweener tweener, CanvasGroup canvasGroup, float duration, float start, float end)
	{
		return tweener.addTween(canvasGroup, duration, start, end)
			.onUpdate((c, t) => { if (c != null) c.alpha = t; });
	}

	public static ME.Tweener.Tween<ParticleSystem.Particle> addTween<T>(this ME.Tweener tweener, ParticleSystem.Particle particle, float duration, Color start, Color end)
	{
		return tweener.addTween(particle, duration, 0f, 1f)
			.onUpdate((p, t) => { p.color = Color.Lerp(start, end, t); });
	}

	// position
	public static ME.Tweener.Tween<Transform> addTween(this ME.Tweener tweener, Transform transform, float duration, Vector3 start, Vector3 end)
	{
		return tweener.addTween(transform, duration, 0.0f, 1.0f)
			.onUpdate((tr, t) => { if (tr) tr.localPosition = Vector3.Lerp(start, end, t); });
	}

	public static ME.Tweener.Tween<Transform> addTween(this ME.Tweener tweener, Transform transform, float duration, Vector3 end)
	{
		return tweener.addTween(transform, duration, transform.localPosition, end);
	}
	
	public static ME.Tweener.Tween<RectTransform> addTween(this ME.Tweener tweener, RectTransform transform, float duration, Vector2 end)
	{
		return tweener.addTween(transform, duration, transform.anchoredPosition, end);
	}

	public static ME.Tweener.Tween<RectTransform> addTween(this ME.Tweener tweener, RectTransform transform, float duration, Vector2 start, Vector2 end)
	{
		return tweener.addTween(transform, duration, 0.0f, 1.0f)
			.onUpdate((tr, t) => { if (tr) tr.anchoredPosition = Vector2.Lerp(start, end, t); });
	}

	// rotation
	public static ME.Tweener.Tween<Transform> addTween(this ME.Tweener tweener, Transform transform, float duration, Quaternion start, Quaternion end)
	{
		return tweener.addTween(transform, duration, 0.0f, 1.0f)
			.onUpdate((tr, t) => { if (tr) tr.localRotation = Quaternion.Slerp(start, end, t); });
	}

	public static ME.Tweener.Tween<Transform> addTween(this ME.Tweener tweener, Transform transform, float duration, Quaternion end)
	{
		return tweener.addTween(transform, duration, transform.localRotation, end);
	}

	// scale
	public static ME.Tweener.Tween<Transform> addTweenScale(this ME.Tweener tweener, Transform transform, float duration, Vector3 start, Vector3 end)
	{
		return tweener.addTween(transform, duration, 0.0f, 1.0f)
			.onUpdate((tr, t) => { if (tr) tr.localScale = Vector3.Lerp(start, end, t); });
	}

	public static ME.Tweener.Tween<Transform> addTweenScale(this ME.Tweener tweener, Transform transform, float duration, Vector3 end)
	{
		return tweener.addTweenScale(transform, duration, transform.localScale, end);
	}
	
	public static ME.Tweener.Tween<Transform> addTweenScaleXY(this ME.Tweener tweener, Transform transform, float duration, Vector3 start, Vector3 end)
	{
		return tweener.addTween(transform, duration, 0.0f, 1.0f)
			.onUpdate((tr, t) => {
				
				if (tr != null) {
					
					var lerp = Vector3.Lerp(start, end, t);
					tr.localScale = new Vector3(lerp.x, lerp.y, tr.localScale.z);
					
				}
				
			});
	}

	public static ME.Tweener.Tween<Transform> addTweenScaleXY(this ME.Tweener tweener, Transform transform, float duration, Vector3 end)
	{
		return tweener.addTweenScaleXY(transform, duration, transform.localScale, end);
	}

	
	// euler rotation
	public static ME.Tweener.Tween<Transform> addTweenEuler(this ME.Tweener tweener, Transform transform, float duration, Vector3 start, Vector3 end)
	{
		return tweener.addTween(transform, duration, 0.0f, 1.0f)
			.onUpdate((tr, t) =>
			{
				/*var euler = transform.localEulerAngles;
				euler.x = Mathf.Lerp(start.x, end.x, t);
				euler.y = Mathf.Lerp(start.y, end.y, t);
				euler.z = Mathf.Lerp(start.z, end.z, t);*/
				if (tr) tr.localEulerAngles = Vector3.Lerp(start, end, t);
			});
	}

	public static ME.Tweener.Tween<Transform> addTweenEuler(this ME.Tweener tweener, Transform transform, float duration, Vector3 end)
	{
		return tweener.addTweenEuler(transform, duration, transform.localEulerAngles, end);
	}

	
	private static void _setZAngle(Transform transform, float angle)
	{
		if (transform == null) return;
		var euler = transform.localEulerAngles;
		euler.z = angle;
		transform.localEulerAngles = euler;
	}
	
	public static ME.Tweener.Tween<Transform> addTweenZRotation(this ME.Tweener tweener, Transform transform, float duration, float start, float end)
	{
		return tweener.addTween(transform, duration, start, end)
			.onUpdate(_setZAngle);
	}

	public static ME.Tweener.Tween<Transform> addTweenZRotation(this ME.Tweener tweener, Transform transform, float duration, float end)
	{
		return tweener.addTweenZRotation(transform, duration, transform.localEulerAngles.z, end);
	}
}
