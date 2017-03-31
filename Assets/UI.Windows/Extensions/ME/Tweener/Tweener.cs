
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ME {

	public class Tweener : MonoBehaviour {

		public interface ITransition {

			float interpolate(float start, float distance, float elapsedTime, float duration);

		}

        public struct MultiTag {

            public object tag1;
            public object tag2;
            public object tag3;

            public static bool operator ==(MultiTag mt1, MultiTag mt2) {

                if (mt1.tag1 != mt2.tag1) return false;
                if (mt1.tag2 != mt2.tag2) return false;
                if (mt1.tag3 != mt2.tag3) return false;

                return false;

            }

            public static bool operator !=(MultiTag mt1, MultiTag mt2) {

                return (mt1 == mt2) == false;

            }

			public override bool Equals(object obj) {

				if (obj == null) return false;

				var tag = (MultiTag)obj;
				return tag == this;

			}

			public override int GetHashCode() {
				
				return (this.tag1 != null ? this.tag1.GetHashCode() : 0) ^ (this.tag2 != null ? this.tag2.GetHashCode() : 0) ^ (this.tag3 != null ? this.tag3.GetHashCode() : 0);

			}

            public bool HasValue() {

				return this.tag1 != null || this.tag2 != null || this.tag3 != null;

            }

        }
	
		public interface ITween {

		    int Count { get; }
		    bool isCompleted();
			void RaiseCancel();
			void RaiseComplete();
			object getTag();
		    MultiTag getMultiTag();
		    bool hasMultiTag();
		    object getGroup();
            void update(float dt, bool debug);
			bool isDirty { get; set; }
		    void ClearAll();
			int GetLoops();

#if UNITY_EDITOR
		    //string stackTrace { get; }
#endif

		}
		
		public enum TimerType {
			Fixed,
			Game,
		}

		public class Tween<T>: ITween {

			public bool isDirty {
				get;
				set;
			}

			private static int INFINITE_LOOPS = -1;
			private T _obj;
			private float _start;
			
			private class Target {
				public float start;
				public float value;
				public float duration;
				public float currentDelay = 0f;
				public float delay = 0f;
				public ITransition transition;
				public bool inverse = false;
			}
			
			private readonly List<Target> _targets = new List<Target>(1);

            public int Count { get { return this._targets.Count; } }

            int _currentTarget = 0;
			private bool _started = false;
			private float _elapsed = 0f;
			private bool _completed = false;
			private object _tag;
		    private MultiTag _multiTag = default(MultiTag);
			private bool _multiTagFlag = false;
			private object _group;
			private int _loops = 1;
			private System.Action<T> _begin = null;
			private System.Action<T, float> _update = null;
			private System.Action<T> _complete = null;
			private System.Action<T> _cancel = null;
            //public string stackTrace { get; private set; }

            public Tween(T obj, float duration, float start, float end) {
				_obj = obj;
				Target target = new Target();
				target.start = start;
				target.value = end;
				target.duration = duration;
				target.transition = Ease.Linear;
				
				_completed = false;
				_started = false;
				
				_targets.Add(target);

				isDirty = false;

#if UNITY_EDITOR
                // this.stackTrace = (new System.Diagnostics.StackTrace()).ToString();
#endif

            }

			public int GetLoops() {

				return this._loops;

			}

			public void RaiseBegin() {
				
				if (_begin != null)
					_begin(_obj);
				
			}

			public void RaiseComplete() {
				
				if (_complete != null)
					_complete(_obj);
				
			}

		    public void ClearAll() {
		        
                this._targets.Clear();

                /*
		        if (_complete != null) {
                    _complete.Invoke(_obj);
                }
                */

		        _obj = default(T);
                _begin = null;
                _update = null;
                _complete = null;
                _cancel = null;
		        _tag = null;
		        _multiTag = default(MultiTag);
		        _group = null;

		    }

            public void RaiseCancel() {
				
				if (_cancel != null)
					_cancel(_obj);

				this.isDirty = false;
				
			}
	
			public bool isCompleted() {
				return _completed;
			}
	
			public object getTag() {
				return _tag;
			}

		    public MultiTag getMultiTag() {
		        return _multiTag;
		    }

		    public bool hasMultiTag() {
		        return _multiTagFlag;
		    }

            public object getGroup() {
                return _group;
            }

            public void update(float dt, bool debug = false) {

				var target = _targets[_currentTarget];
				//if (debug == true) Debug.Log(_currentTarget + " " + _elapsed + " " + target.duration);
				
				target.currentDelay += dt;
				if (target.currentDelay <= target.delay) return;
				
				_elapsed += dt;
				
				if (_started == false && _begin != null) {
					
					_begin(_obj);
					_started = true;
					
				}

				if (_elapsed >= target.duration) {
					if ((_currentTarget + 1) == _targets.Count) {

						if (_update != null) _update(_obj, target.inverse ? target.start : target.value);
						
						if (_loops != INFINITE_LOOPS) {
							
							_completed = (--_loops == 0);
							
						}
						
						if (!_completed) {
							
							_elapsed = 0f;
							_currentTarget = 0;
							
						}
						
						if (_loops == INFINITE_LOOPS || _loops == 0) {
							
							RaiseComplete();
							
						}
						
						return;
					}
	
					_elapsed = _elapsed - target.duration;
					target = _targets[++_currentTarget];
				}

				if (_update != null) {
					float t = target.inverse ? target.duration - _elapsed : _elapsed;
					float v = target.transition.interpolate(target.start, target.value - target.start, t, target.duration);
					_update(_obj, v);
				}
	
				return;
			}

			public Tween<T> ease(ITransition value) {

                _targets[_targets.Count - 1].transition = value;
	
				return this;

			}
			
			public Tween<T> onBegin(System.Action<T> func) {
				if (func != null) _begin += func;
				return this;
			}
			
			public Tween<T> onBegin(System.Action func) {
				if (func != null) _begin += self => func();
				return this;
			}

			public Tween<T> onUpdate(System.Action<T, float> func) {
				if (func != null) _update += func;
				return this;
			}
			
			public Tween<T> onComplete(System.Action<T> func) {
				if (func != null) _complete += func;
				return this;
			}

			public Tween<T> onComplete(System.Action func) {
				if (func != null) _complete += self => func();
				return this;
			}
			
			public Tween<T> onCancel(System.Action<T> func) {
				if (func != null) _cancel += func;
				return this;
			}
			
			public Tween<T> onCancel(System.Action func) {
				if (func != null) _cancel += self => func();
				return this;
			}

			public Tween<T> tag(object value, object group = null) {
				_multiTag = default(MultiTag);
				_multiTagFlag = false;
			    _group = group;
				_tag = value;
				return this;
			}

		    public Tween<T> multiTag(MultiTag value, object group = null) {
                _multiTag = value;
				_multiTagFlag = true;
                _group = group;
                _tag = null;
                return this;
            }

		    public Tween<T> delay(float delay) {

				_targets[_targets.Count - 1].delay = delay;
				_targets[_targets.Count - 1].currentDelay = 0f;

				return this;

			}

			public Tween<T> repeat() {
				_loops = INFINITE_LOOPS;
				return this;
			}
	
			public Tween<T> repeat(int loops) {
				
				_loops = loops;
				return this;
				
			}

            public Tween<T> setValue(float value) {

                _elapsed = value * _targets[_targets.Count - 1].duration;

                return this;

            }
	
			public Tween<T> addTarget(float duration, float end) {
				
				var target = new Target();
				target.start = _targets[_targets.Count - 1].value;
				target.value = end;
				target.duration = duration;
				target.transition = _targets[0].transition;
				_targets.Add(target);
				
				return this;
				
			}
	
			public Tween<T> addTarget(float duration, float end, ITransition transition) {
				
				var target = new Target();
				target.start = _targets[_targets.Count - 1].value;
				target.value = end;
				target.duration = duration;
				target.transition = transition;
				_targets.Add(target);
				
				return this;
				
			}
	
			public Tween<T> reflect() {
				
				var reflectedTargets = new List<Target>(capacity: _targets.Count);

                for(var i = 0; i < _targets.Count; ++i) {
                    var target = _targets[i];

					var reflected = new Target();
					reflected.start = target.start;
					reflected.value = target.value;
					reflected.transition = target.transition;
					reflected.duration = target.duration;
					reflected.inverse = !target.inverse;
					reflectedTargets.Add(reflected);
				}

				_targets.AddRange(reflectedTargets);

				return this;
				
			}
			
		}

		public TimerType timerType = TimerType.Game;
		public bool repeatByDefault = false;
		public bool debug = false;
		private readonly List<ITween> _tweens = new List<ITween>(100);

		public Tween<T> addTween<T>(T obj, float duration, float start, float end) {
			
			var tween = new Tween<T>(obj, duration, start, end);
			if (repeatByDefault)
				tween.repeat();
			
			_tweens.Add(tween);
			return tween;
			
		}
		
		public bool hasTag(string tag) {

			for (var i = 0; i < this._tweens.Count; ++i) {

				var tween = this._tweens[i];
		        if (tween.getTag() != null && tween.getTag().ToString() == tag) return true;

		    }

		    return false;
			
		}

        public bool hasTag(MultiTag multiTag) {

			for (var i = 0; i < this._tweens.Count; ++i) {

				var tween = this._tweens[i];
                if (tween.hasMultiTag() == true && tween.getMultiTag() == multiTag) return true;

            }

            return false;

        }

        public bool hasTag(object tag) {

			for (var i = 0; i < this._tweens.Count; ++i) {

				var tween = this._tweens[i];
                if (tween.getTag() == tag) return true;

            }

            return false;

		}

	    public int Count {

	        get { return this._tweens.Sum(x => x.Count); }

	    }

	    public void ClearAll() {

	        for (var i = 0; i < this._tweens.Count; ++i) {

                this._tweens[i].ClearAll();

            }

            this._tweens.Clear();

        }

		public List<ITween> GetTweens() {

			return this._tweens;

		}

        public void removeTweens(MultiTag multiTag) {

            this.removeTweens(multiTag, immediately: false);

        }

        public void removeTweens(string tweenerTag) {
			
			this.removeTweens(tweenerTag, immediately: false);
			
		}
		
		public void removeTweens(object tweenerTag) {
			
			this.removeTweens(tweenerTag, immediately: false);
			
		}
		
		public void removeGroup(object tweenerGroup) {
			
			this.removeGroup(tweenerGroup, immediately: false);
			
		}

        public void removeTweens(MultiTag multiTag, bool immediately) {

            Mark(tween => tween.hasMultiTag() == true && tween.getMultiTag() == multiTag, immediately);

        }

        public void removeTweens(string tweenerTag, bool immediately) {

			Mark(tween => tween.getTag() != null && tween.getTag().ToString() == tweenerTag, immediately);
			
		}

		public void removeTweens(object tweenerTag, bool immediately) {

			Mark(tween => tween.getTag() == tweenerTag, immediately);
			
		}

		public void removeGroup(object tweenerGroup, bool immediately) {

			Mark(tween => tween.getGroup() == tweenerGroup, immediately);

	    }

		public virtual void Update() {

			this.Update((this.timerType == TimerType.Fixed) ? Time.unscaledDeltaTime : Time.deltaTime);

		}

		protected void Update(float deltaTime) {

			_update(deltaTime, this.debug);
			
		}
		
		void OnDestroy() {

			foreach (var each in _tweens) {

				each.RaiseCancel();
				
			}
			
		}
		
		void _update(float dt, bool debug = false) {

			for (var i = 0; i < this._tweens.Count; ++i) {

				var each = this._tweens[i];
				if (each.isCompleted() || each.isDirty == true) {

					if (each.isDirty == true && each.isCompleted() == false) each.RaiseCancel();

					_tweens.RemoveAt(i);
					--i;

				} else {

					each.update(dt, debug);

				}

			}

			/*for (var node = _tweens.First; node != null;) {

				var next = node.Next;
				var each = node.Value;

				if (each.isCompleted() || each.isDirty == true) {

					if (each.isDirty == true && each.isCompleted() == false) each.RaiseCancel();

					_tweens.Remove(node);

				} else {

					each.update(dt, debug);
					
				}

				node = next;
				
			}*/

		}

		void Mark(System.Func<ITween, bool> predicate, bool immediately = false) {

			for (var i = 0; i < this._tweens.Count; ++i) {

				var tween = this._tweens[i];
                if (predicate != null && predicate(tween) == false) continue;
				tween.isDirty = true;

		    }

            /*
            foreach (var each in _tweens.Where( predicate )) {

				each.isDirty = true;

			}
            */

			if (immediately == true) {

				for (var i = 0; i < this._tweens.Count; ++i) {

					var each = this._tweens[i];
					if (each.isCompleted() || each.isDirty == true) {

						if (each.isDirty == true && each.isCompleted() == false) each.RaiseCancel();

						_tweens.RemoveAt(i);
						--i;

					}

				}

			}

		}
	
	}
	
	public class EaseTransition: Tweener.ITransition {

		private System.Func<float, float, float, float, float> _func;
	
		public EaseTransition(System.Func<float, float, float, float, float> func) {
			_func = func;

		}

		public float interpolate(float start, float distance, float elapsedTime, float duration) {

			return _func(elapsedTime, start, distance, duration);

		}

	}
	
	public class CurveTransition: Tweener.ITransition {

		AnimationCurve _curve;
	
		public CurveTransition(AnimationCurve curve) {

			_curve = curve;

		}

		public float interpolate(float start, float distance, float elapsedTime, float duration) {

			float curveDuration = 0.0f;
			if (_curve.length > 0)
				curveDuration = _curve.keys[_curve.length - 1].time;
			
			float t = _curve.Evaluate(curveDuration * elapsedTime / duration);
	
			return start + t * distance;

		}

	}
	
	public static class Ease {
		
		public enum Type : byte {
			
			Linear = 0,
			InQuad,
			OutQuad,
			InOutQuad,
			InCubic,
			OutCubic,
			InOutCubic,
			InQuart,
			OutQuart,
			InOutQuart,
			InQuint,
			OutQuint,
			InOutQuint,
			InSine,
			OutSine, 
			InOutSine, 
			InExpo,
			OutExpo, 
			InOutExpo, 
			InCirc,
			OutCirc, 
			InOutCirc,
			InElastic,
			OutElastic,
			InOutElastic,
			InBack,
			OutBack,
			InOutBack,
			InBounce,
			OutBounce,
			InOutBounce,
			OutInQuad,
			OutInCubic,
			OutInQuart,
			OutInQuint,
			OutInSine, 
			OutInExpo, 
			OutInCirc,
			OutInElastic,
			OutInBack,
			OutInBounce,
			
		};
		
		public static Tweener.ITransition GetByType(Type type) {
			
			return easings[(byte)type];
			
		}
		
		public static EaseTransition Linear = new EaseTransition(Easings.Linear);

		public static EaseTransition InQuad = new EaseTransition(Easings.QuadEaseIn);
		public static EaseTransition OutQuad = new EaseTransition(Easings.QuadEaseOut);
		public static EaseTransition InOutQuad = new EaseTransition(Easings.QuadEaseInOut);
		public static EaseTransition OutInQuad = new EaseTransition(Easings.QuadEaseOutIn);

		public static EaseTransition InCubic = new EaseTransition(Easings.CubicEaseIn);
		public static EaseTransition OutCubic = new EaseTransition(Easings.CubicEaseOut);
		public static EaseTransition InOutCubic = new EaseTransition(Easings.CubicEaseInOut);
		public static EaseTransition OutInCubic = new EaseTransition(Easings.CubicEaseOutIn);

		public static EaseTransition InQuart = new EaseTransition(Easings.QuartEaseIn);
		public static EaseTransition OutQuart = new EaseTransition(Easings.QuartEaseOut);
		public static EaseTransition InOutQuart = new EaseTransition(Easings.QuartEaseInOut);
		public static EaseTransition OutInQuart = new EaseTransition(Easings.QuartEaseOutIn);

		public static EaseTransition InQuint = new EaseTransition(Easings.QuintEaseIn);
		public static EaseTransition OutQuint = new EaseTransition(Easings.QuintEaseOut);
		public static EaseTransition InOutQuint = new EaseTransition(Easings.QuintEaseInOut);
		public static EaseTransition OutInQuint = new EaseTransition(Easings.QuintEaseOutIn);

		public static EaseTransition InSine = new EaseTransition(Easings.SineEaseIn);
		public static EaseTransition OutSine = new EaseTransition(Easings.SineEaseOut);
		public static EaseTransition InOutSine = new EaseTransition(Easings.SineEaseInOut);
		public static EaseTransition OutInSine = new EaseTransition(Easings.SineEaseOutIn);

		public static EaseTransition InExpo = new EaseTransition(Easings.ExpoEaseIn);
		public static EaseTransition OutExpo = new EaseTransition(Easings.ExpoEaseOut);
		public static EaseTransition InOutExpo = new EaseTransition(Easings.ExpoEaseInOut);
		public static EaseTransition OutInExpo = new EaseTransition(Easings.ExpoEaseOutIn);

		public static EaseTransition InCirc = new EaseTransition(Easings.CircEaseIn);
		public static EaseTransition OutCirc = new EaseTransition(Easings.CircEaseOut);
		public static EaseTransition InOutCirc = new EaseTransition(Easings.CircEaseInOut);
		public static EaseTransition OutInCirc = new EaseTransition(Easings.CircEaseOutIn);

		public static EaseTransition InElastic = new EaseTransition(Easings.ElasticEaseIn);
		public static EaseTransition OutElastic = new EaseTransition(Easings.ElasticEaseOut);
		public static EaseTransition InOutElastic = new EaseTransition(Easings.ElasticEaseInOut);
		public static EaseTransition OutInElastic = new EaseTransition(Easings.ElasticEaseOutIn);

		public static EaseTransition InBack = new EaseTransition(Easings.BackEaseIn);
		public static EaseTransition OutBack = new EaseTransition(Easings.BackEaseOut);
		public static EaseTransition InOutBack = new EaseTransition(Easings.BackEaseInOut);
		public static EaseTransition OutInBack = new EaseTransition(Easings.BackEaseOutIn);

		public static EaseTransition InBounce = new EaseTransition(Easings.BounceEaseIn);
		public static EaseTransition OutBounce = new EaseTransition(Easings.BounceEaseOut);
		public static EaseTransition InOutBounce = new EaseTransition(Easings.BounceEaseOutIn);
		public static EaseTransition OutInBounce = new EaseTransition(Easings.BounceEaseInOut);
		
		private static Tweener.ITransition[] easings = new Tweener.ITransition[] {
			Linear,
			InQuad,
			OutQuad,
			InOutQuad,
			InCubic,
			OutCubic,
			InOutCubic,
			InQuart,
			OutQuart,
			InOutQuart,
			InQuint,
			OutQuint,
			InOutQuint,
			InSine,
			OutSine, 
			InOutSine,
			InExpo,
			OutExpo, 
			InOutExpo,
			InCirc,
			OutCirc, 
			InOutCirc,
			InElastic,
			OutElastic,
			InOutElastic,
			InBack,
			OutBack,
			InOutBack,
			InBounce,
			OutBounce,
			InOutBounce,
			OutInQuad,
			OutInCubic,
			OutInQuart,
			OutInQuint,
			OutInSine, 
			OutInExpo, 
			OutInCirc,
			OutInElastic,
			OutInBack,
			OutInBounce,
		};

	}

	public static class Easings {

		#region Linear

		/// <summary>
		/// Easing equation function for a simple linear tweening, with no easing.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float Linear( float t, float b, float c, float d )
		{
			return c * t / d + b;
		}

		#endregion

		#region Expo

		/// <summary>
		/// Easing equation function for an exponential (2^t) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ExpoEaseOut( float t, float b, float c, float d )
		{
			return ( t == d ) ? b + c : c * ( -Mathf.Pow( 2, -10 * t / d ) + 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for an exponential (2^t) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ExpoEaseIn( float t, float b, float c, float d )
		{
			return ( t == 0 ) ? b : c * Mathf.Pow( 2, 10 * ( t / d - 1 ) ) + b;
		}

		/// <summary>
		/// Easing equation function for an exponential (2^t) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ExpoEaseInOut( float t, float b, float c, float d )
		{
			if ( t == 0 )
				return b;

			if ( t == d )
				return b + c;

			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * Mathf.Pow( 2, 10 * ( t - 1 ) ) + b;

			return c / 2 * ( -Mathf.Pow( 2, -10 * --t ) + 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for an exponential (2^t) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ExpoEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return ExpoEaseOut( t * 2, b, c / 2, d );

			return ExpoEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Circular

		/// <summary>
		/// Easing equation function for a circular (sqrt(1-t^2)) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CircEaseOut( float t, float b, float c, float d )
		{
			return c * Mathf.Sqrt( 1 - ( t = t / d - 1 ) * t ) + b;
		}

		/// <summary>
		/// Easing equation function for a circular (sqrt(1-t^2)) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CircEaseIn( float t, float b, float c, float d )
		{
			return -c * ( Mathf.Sqrt( 1 - ( t /= d ) * t ) - 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CircEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) < 1 )
				return -c / 2 * ( Mathf.Sqrt( 1 - t * t ) - 1 ) + b;

			return c / 2 * ( Mathf.Sqrt( 1 - ( t -= 2 ) * t ) + 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a circular (sqrt(1-t^2)) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CircEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return CircEaseOut( t * 2, b, c / 2, d );

			return CircEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Quad

		/// <summary>
		/// Easing equation function for a quadratic (t^2) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuadEaseOut( float t, float b, float c, float d )
		{
			return -c * ( t /= d ) * ( t - 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for a quadratic (t^2) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuadEaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t + b;
		}

		/// <summary>
		/// Easing equation function for a quadratic (t^2) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuadEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * t * t + b;

			return -c / 2 * ( ( --t ) * ( t - 2 ) - 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a quadratic (t^2) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuadEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return QuadEaseOut( t * 2, b, c / 2, d );

			return QuadEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Sine

		/// <summary>
		/// Easing equation function for a sinusoidal (sin(t)) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float SineEaseOut( float t, float b, float c, float d )
		{
			return c * Mathf.Sin( t / d * ( Mathf.PI / 2 ) ) + b;
		}

		/// <summary>
		/// Easing equation function for a sinusoidal (sin(t)) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float SineEaseIn( float t, float b, float c, float d )
		{
			return -c * Mathf.Cos( t / d * ( Mathf.PI / 2 ) ) + c + b;
		}

		/// <summary>
		/// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float SineEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * ( Mathf.Sin( Mathf.PI * t / 2 ) ) + b;

			return -c / 2 * ( Mathf.Cos( Mathf.PI * --t / 2 ) - 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for a sinusoidal (sin(t)) easing in/out: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float SineEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return SineEaseOut( t * 2, b, c / 2, d );

			return SineEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Cubic

		/// <summary>
		/// Easing equation function for a cubic (t^3) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CubicEaseOut( float t, float b, float c, float d )
		{
			return c * ( ( t = t / d - 1 ) * t * t + 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a cubic (t^3) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CubicEaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * t + b;
		}

		/// <summary>
		/// Easing equation function for a cubic (t^3) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CubicEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * t * t * t + b;

			return c / 2 * ( ( t -= 2 ) * t * t + 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for a cubic (t^3) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float CubicEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return CubicEaseOut( t * 2, b, c / 2, d );

			return CubicEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Quartic

		/// <summary>
		/// Easing equation function for a quartic (t^4) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuartEaseOut( float t, float b, float c, float d )
		{
			return -c * ( ( t = t / d - 1 ) * t * t * t - 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a quartic (t^4) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuartEaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * t * t + b;
		}

		/// <summary>
		/// Easing equation function for a quartic (t^4) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuartEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * t * t * t * t + b;

			return -c / 2 * ( ( t -= 2 ) * t * t * t - 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for a quartic (t^4) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuartEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return QuartEaseOut( t * 2, b, c / 2, d );

			return QuartEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Quintic

		/// <summary>
		/// Easing equation function for a quintic (t^5) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuintEaseOut( float t, float b, float c, float d )
		{
			return c * ( ( t = t / d - 1 ) * t * t * t * t + 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a quintic (t^5) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuintEaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * t * t * t + b;
		}

		/// <summary>
		/// Easing equation function for a quintic (t^5) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuintEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * t * t * t * t * t + b;
			return c / 2 * ( ( t -= 2 ) * t * t * t * t + 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for a quintic (t^5) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float QuintEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return QuintEaseOut( t * 2, b, c / 2, d );
			return QuintEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Elastic

		/// <summary>
		/// Easing equation function for an elastic (exponentially decaying sine wave) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ElasticEaseOut( float t, float b, float c, float d )
		{
			if ( ( t /= d ) == 1 )
				return b + c;

			float p = d * .3f;
			float s = p / 4.0f;

			return ( c * Mathf.Pow( 2, -10 * t ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) + c + b );
		}

		/// <summary>
		/// Easing equation function for an elastic (exponentially decaying sine wave) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ElasticEaseIn( float t, float b, float c, float d )
		{
			if ( ( t /= d ) == 1 )
				return b + c;

			float p = d * .3f;
			float s = p / 4;

			return -( c * Mathf.Pow( 2, 10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) ) + b;
		}

		/// <summary>
		/// Easing equation function for an elastic (exponentially decaying sine wave) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ElasticEaseInOut( float t, float b, float c, float d )
		{
			if ( ( t /= d / 2 ) == 2 )
				return b + c;

			float p = d * ( .3f * 1.5f );
			float s = p / 4;

			if ( t < 1 )
				return -.5f * ( c * Mathf.Pow( 2, 10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) ) + b;
			return c * Mathf.Pow( 2, -10 * ( t -= 1 ) ) * Mathf.Sin( ( t * d - s ) * ( 2 * Mathf.PI ) / p ) * .5f + c + b;
		}

		/// <summary>
		/// Easing equation function for an elastic (exponentially decaying sine wave) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float ElasticEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return ElasticEaseOut( t * 2, b, c / 2, d );
			return ElasticEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Bounce

		/// <summary>
		/// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BounceEaseOut( float t, float b, float c, float d )
		{
			if ( ( t /= d ) < ( 1 / 2.75 ) )
				return c * ( 7.5625f * t * t ) + b;
			else if ( t < ( 2 / 2.75f ) )
				return c * ( 7.5625f * ( t -= ( 1.5f / 2.75f ) ) * t + .75f ) + b;
			else if ( t < ( 2.5f / 2.75f ) )
				return c * ( 7.5625f * ( t -= ( 2.25f / 2.75f ) ) * t + .9375f ) + b;
			else
				return c * ( 7.5625f * ( t -= ( 2.625f / 2.75f ) ) * t + .984375f ) + b;
		}

		/// <summary>
		/// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BounceEaseIn( float t, float b, float c, float d )
		{
			return c - BounceEaseOut( d - t, 0, c, d ) + b;
		}

		/// <summary>
		/// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BounceEaseInOut( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return BounceEaseIn( t * 2, 0, c, d ) * .5f + b;
			else
				return BounceEaseOut( t * 2 - d, 0, c, d ) * .5f + c * .5f + b;
		}

		/// <summary>
		/// Easing equation function for a bounce (exponentially decaying parabolic bounce) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BounceEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return BounceEaseOut( t * 2, b, c / 2, d );
			return BounceEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}

		#endregion

		#region Back

		/// <summary>
		/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out: 
		/// decelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BackEaseOut( float t, float b, float c, float d )
		{
			return c * ( ( t = t / d - 1 ) * t * ( ( 1.70158f + 1 ) * t + 1.70158f ) + 1 ) + b;
		}

		/// <summary>
		/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in: 
		/// accelerating from zero velocity.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BackEaseIn( float t, float b, float c, float d )
		{
			return c * ( t /= d ) * t * ( ( 1.70158f + 1 ) * t - 1.70158f ) + b;
		}

		/// <summary>
		/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing in/out: 
		/// acceleration until halfway, then deceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BackEaseInOut( float t, float b, float c, float d )
		{
			float s = 1.70158f;
			if ( ( t /= d / 2 ) < 1 )
				return c / 2 * ( t * t * ( ( ( s *= ( 1.525f ) ) + 1 ) * t - s ) ) + b;
			return c / 2 * ( ( t -= 2 ) * t * ( ( ( s *= ( 1.525f ) ) + 1 ) * t + s ) + 2 ) + b;
		}

		/// <summary>
		/// Easing equation function for a back (overshooting cubic easing: (s+1)*t^3 - s*t^2) easing out/in: 
		/// deceleration until halfway, then acceleration.
		/// </summary>
		/// <param name="t">Current time in seconds.</param>
		/// <param name="b">Starting value.</param>
		/// <param name="c">Final value.</param>
		/// <param name="d">Duration of animation.</param>
		/// <returns>The correct value.</returns>
		public static float BackEaseOutIn( float t, float b, float c, float d )
		{
			if ( t < d / 2 )
				return BackEaseOut( t * 2, b, c / 2, d );
			return BackEaseIn( ( t * 2 ) - d, b + c / 2, c / 2, d );
		}
		#endregion
		/*
		static float _inBounce(float startValue, float changeValue, float time, float duration) {
			return changeValue - _outBounce(duration - time, changeValue, 0f, duration) + startValue;
		}

		static float _outBounce(float startValue, float changeValue, float time, float duration) {
			if ((time /= duration) < (1 / 2.75f)) {
				return changeValue * (7.5625f * time * time) + startValue;
			}
			if (time < (2 / 2.75f)) {
				return changeValue * (7.5625f * (time -= (1.5f / 2.75f)) * time + 0.75f) + startValue;
			}
			if (time < (2.5f / 2.75f)) {
				return changeValue * (7.5625f * (time -= (2.25f / 2.75f)) * time + 0.9375f) + startValue;
			}
			return changeValue * (7.5625f * (time -= (2.625f / 2.75f)) * time + 0.984375f) + startValue;
		}

		static float _inOutBounce(float startValue, float changeValue, float time, float duration) {
			if (time < duration * 0.5f) {
				return _inBounce(time * 2, changeValue, 0f, duration) * 0.5f + startValue;
			}
			return _outBounce(time * 2 - duration, changeValue, 0f, duration) * 0.5f + changeValue * 0.5f + startValue;
		}
		
		static float _linear(float start, float distance, float elapsedTime, float duration) {
			if (elapsedTime > duration)
				elapsedTime = duration;
			return distance * (elapsedTime / duration) + start;
		}
	
		static float _inQuad(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			return distance * elapsedTime * elapsedTime + start;
		}
	
		static float _outQuad(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			return -distance * elapsedTime * (elapsedTime - 2.0f) + start;
		}
	
		static float _inOutQuad(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
			if (elapsedTime < 1)
				return distance / 2.0f * elapsedTime * elapsedTime + start;
			elapsedTime--;
			return -distance / 2.0f * (elapsedTime * (elapsedTime - 2.0f) - 1.0f) + start;
		}
	
		static float _inCubic(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			return distance * elapsedTime * elapsedTime * elapsedTime + start;
		}
	
		static float _outCubic(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			elapsedTime--;
			return distance * (elapsedTime * elapsedTime * elapsedTime + 1) + start;
		}
	 
		static float _inOutCubic(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
			if (elapsedTime < 1)
				return distance / 2 * elapsedTime * elapsedTime * elapsedTime + start;
			elapsedTime -= 2;
			return distance / 2 * (elapsedTime * elapsedTime * elapsedTime + 2) + start;
		}
	 
		static float _inQuart(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
	
		static float _outQuart(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			elapsedTime--;
			return -distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1) + start;
		}
	
		static float _inOutQuart(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
			if (elapsedTime < 1)
				return distance / 2 * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
			elapsedTime -= 2;
			return -distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2) + start;
		}
	
		static float _inQuint(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
		}
	 
		static float _outQuint(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			elapsedTime--;
			return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 1) + start;
		}
	
		static float _inOutQuint(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
			if (elapsedTime < 1)
				return distance / 2 * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
			elapsedTime -= 2;
			return distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime + 2) + start;
		}
	
		static float _inSine(float start, float distance, float elapsedTime, float duration) {
			if (elapsedTime > duration)
				elapsedTime = duration;
			return -distance * Mathf.Cos(elapsedTime / duration * (Mathf.PI / 2)) + distance + start;
		}
	
		static float _outSine(float start, float distance, float elapsedTime, float duration) {
			if (elapsedTime > duration)
				elapsedTime = duration;
			return distance * Mathf.Sin(elapsedTime / duration * (Mathf.PI / 2)) + start;
		}
	
		static float _inOutSine(float start, float distance, float elapsedTime, float duration) {
			if (elapsedTime > duration)
				elapsedTime = duration;
			return -distance / 2 * (Mathf.Cos(Mathf.PI * elapsedTime / duration) - 1) + start;
		}
	
		static float _inExpo(float start, float distance, float elapsedTime, float duration) {
			if (elapsedTime > duration)
				elapsedTime = duration;
			return distance * Mathf.Pow(2, 10 * (elapsedTime / duration - 1)) + start;
		}
	 
		static float _outExpo(float start, float distance, float elapsedTime, float duration) {
			if (elapsedTime > duration)
				elapsedTime = duration;
			return distance * (-Mathf.Pow(2, -10 * elapsedTime / duration) + 1) + start;
		}
		 
		static float _inOutExpo(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
			if (elapsedTime < 1)
				return distance / 2 * Mathf.Pow(2, 10 * (elapsedTime - 1)) + start;
			elapsedTime--;
			return distance / 2 * (-Mathf.Pow(2, -10 * elapsedTime) + 2) + start;
		}
	
		static float _inCirc(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			return -distance * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
		}
	
		static float _outCirc(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
			elapsedTime--;
			return distance * Mathf.Sqrt(1 - elapsedTime * elapsedTime) + start;
		}
	
		static float _inOutCirc(float start, float distance, float elapsedTime, float duration) {
			elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
			if (elapsedTime < 1)
				return -distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
			elapsedTime -= 2;
			return distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) + 1) + start;
		}*/

		/*static float _inElastic(float start, float distance, float elapsedTime, float duration)
		{
			if (elapsedTime > duration)
				elapsedTime = duration;

			if (elapsedTime == 0.0f)
				return start;

			if ((elapsedTime /= duration) == 1.0f)
				return start + distance;

			float p = duration * 0.3f;
			float a = distance;
			float s = p / 4.0f;
			float postFix = a * Mathf.Pow(2.0f, 10.0 * (elapsedTime -= 1.0f));

			return -(postFix * Mathf.Sin((elapsedTime * duration - s) * (2.0 * Mathf.PI) / p)) + start;
		}

		static float _outElastic(float start, float distance, float elapsedTime, float duration)
		{
			if (elapsedTime > duration)
				elapsedTime = duration;

			if (elapsedTime == 0.0f)
				return start;

			if ((elapsedTime /= duration) == 1.0f)
				return start + distance;

			float p = duration * 0.3f;
			float a = distance;
			float s = p / 4.0f;

			return a * Mathf.Pow(2.0, -10.0 * elapsedTime) * Mathf.Sin((elapsedTime * duration - s) * (2.0 * Mathf.PI) / p) + distance + start;
		}

		static float _inOutElastic(float start, float distance, float elapsedTime, float duration)
		{
			// TODO: implement
			return 0.0f;
		}*/

	}
	
}