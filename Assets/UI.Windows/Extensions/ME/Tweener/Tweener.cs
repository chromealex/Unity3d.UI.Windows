
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ME {

	public class Tweener: MonoBehaviour {

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

			return _func(start, distance, elapsedTime, duration);

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
			InOutBounce
			
		};
		
		public static Tweener.ITransition GetByType(Type type) {
			
			return easings[(byte)type];
			
		}
		
		public static EaseTransition Linear = new EaseTransition(_linear);
		public static EaseTransition InQuad = new EaseTransition(_inQuad);
		public static EaseTransition OutQuad = new EaseTransition(_outQuad);
		public static EaseTransition InOutQuad = new EaseTransition(_inOutQuad);
		public static EaseTransition InCubic = new EaseTransition(_inCubic);
		public static EaseTransition OutCubic = new EaseTransition(_outCubic);
		public static EaseTransition InOutCubic = new EaseTransition(_inOutCubic);
		public static EaseTransition InQuart = new EaseTransition(_inQuart);
		public static EaseTransition OutQuart = new EaseTransition(_outQuart);
		public static EaseTransition InOutQuart = new EaseTransition(_inOutQuart);
		public static EaseTransition InQuint = new EaseTransition(_inQuint);
		public static EaseTransition OutQuint = new EaseTransition(_outQuint);
		public static EaseTransition InOutQuint = new EaseTransition(_inOutQuint);
		public static EaseTransition InSine = new EaseTransition(_inSine);
		public static EaseTransition OutSine = new EaseTransition(_outSine);
		public static EaseTransition InOutSine = new EaseTransition(_inOutSine);
		public static EaseTransition InExpo = new EaseTransition(_inExpo);
		public static EaseTransition OutExpo = new EaseTransition(_outExpo);
		public static EaseTransition InOutExpo = new EaseTransition(_inOutExpo);
		public static EaseTransition InCirc = new EaseTransition(_inCirc);
		public static EaseTransition OutCirc = new EaseTransition(_outCirc);
		public static EaseTransition InOutCirc = new EaseTransition(_inOutCirc);
		
		// TODO: implement
		public static EaseTransition InElastic = new EaseTransition(_inExpo);
		public static EaseTransition OutElastic = new EaseTransition(_outExpo);
		public static EaseTransition InOutElastic = new EaseTransition(_inOutExpo);
		public static EaseTransition InBack = new EaseTransition(_inExpo);
		public static EaseTransition OutBack = new EaseTransition(_outExpo);
		public static EaseTransition InOutBack = new EaseTransition(_inOutExpo);
		public static EaseTransition InBounce = new EaseTransition(_inBounce);
		public static EaseTransition OutBounce = new EaseTransition(_outBounce);
		public static EaseTransition InOutBounce = new EaseTransition(_inOutBounce);
		
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
			InOutBounce
		};

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
		}
	
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