using System;
using System.Diagnostics;

namespace ME {

	public class LongrunTimer {
		
		private readonly Stopwatch _sw;
		private DateTime _startTime;
		private DateTime? _lastStopTime;
		private readonly long _pauseTime;

		/// <summary>
		/// Creates a timer system for calculating when long running tasks should pause their execution.
		/// </summary>
		/// <param name="pauseTime">Time in Milliseconds at which ShouldPause will return true.</param>
		/// <param name="autoStart">If this timer should call it's own Start() function automatically.</param>
		public LongrunTimer(bool autoStart = true) : this(15L/*UnityEngine.Application.targetFrameRate*/, autoStart) {}

		public LongrunTimer(long pauseTime, bool autoStart = true) {
			
			_pauseTime = pauseTime;
			_sw = new Stopwatch();

			if (autoStart) this.Start();

		}

		public bool ShouldPause() {
			return this.milliseconds > this._pauseTime;
		}

		/// <summary>
		/// Starts the internal clock and updates the 'start time' of this timer.
		/// </summary>
		public void Start() {
			_sw.Start();
			_startTime = DateTime.UtcNow;
		}

		/// <summary>
		/// Resets the internal clock, does not change the 'start time' of this timer.
		/// </summary>
		public void Reset() {
			_sw.Reset();
			_sw.Start();
		}

		/// <summary>
		/// Stops the internal clock, and updates the 'last stop time' of this timer.
		/// </summary>
		public void Stop() {
			_sw.Stop();
			_lastStopTime = DateTime.UtcNow;
		}

		/// <summary>
		/// Returns the total time in milliseconds since starting or the last reset
		/// </summary>
		public long milliseconds { get { return _sw.ElapsedMilliseconds; } }

		/// <summary>
		/// Returns the total time in seconds since starting or the last reset
		/// </summary>
		public double seconds { get { return _sw.Elapsed.TotalSeconds; } }

		/// <summary>
		/// Ignores resets to give you the total time in milliseconds since starting. Does account for last call of 'Stop()'.
		/// </summary>
		public double totalMilliseconds { get { return ((_lastStopTime ?? DateTime.UtcNow) - _startTime).TotalMilliseconds; } }

		/// <summary>
		/// Ignores resets to give you the total time in seconds since starting. Does account for last call of 'Stop()'.
		/// </summary>
		public double totalSeconds { get { return ((_lastStopTime ?? DateTime.UtcNow) - _startTime).TotalSeconds; } }
	}
}
