#if UNITY_WEBGL && !UNITY_EDITOR
#else
#define MULTITHREADING_ENABLED
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
#endif
using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
	public class UnityBackgroundWorker : MonoBehaviour
	{
		private static UnityBackgroundWorker _instance;

		[Range(0.01f, 1f)]
		public float UpdateTime = 0.2f;

		private Coroutine _coroutine;
		#if MULTITHREADING_ENABLED
		private BackgroundWorker _bw;
		private Queue<Action> _actionsQueue;
		#else
		private bool _cancelled;
		#endif

		void Awake()
		{
			if (_instance != null && _instance != this)
			{
				Destroy(this);
				return;
			}
			_instance = this;
			#if MULTITHREADING_ENABLED
			_actionsQueue = new Queue<Action>(64);
			_bw = new BackgroundWorker();
			_bw.WorkerSupportsCancellation = true;
			_bw.DoWork += DoBackgoroundWork;
			_bw.RunWorkerCompleted += OnWorkCompleted;
			Debug.Log("UnityBackgroundWorker started in PARALLEL mode");
			#else
			Debug.Log("UnityBackgroundWorker started in COROUTINE mode");
			#endif
		}

		#if MULTITHREADING_ENABLED
		void OnEnable()
		{
			_coroutine = StartCoroutine(Ever());
		}

		private void DoBackgoroundWork(object sender, DoWorkEventArgs e)
		{
			Request r = (Request) e.Argument;
			IEnumerator en = r.ActionP != null ? r.ActionP(r.Argument) : r.Action();
			e.Result = r.EndCallback;
			if (_bw.CancellationPending)
			{
				e.Cancel = true;
				return;
			}
			while (en.MoveNext())
			{
				if (en.Current == null)
					continue;
				if (en.Current is Wait)
					Thread.Sleep((int)(1000f*((Wait)en.Current).Time));
				else if (en.Current is Progress)
				{
					if (r.ProgressCallback != null)
						Invoke(() => r.ProgressCallback(((Progress)en.Current).Percents));
				}
				else Debug.LogWarning("Unknown background event type: " + en.Current);
				if (_bw.CancellationPending)
				{
					e.Cancel = true;
					return;
				}
			}
		}

		private void OnWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled) return;
			Invoke(() => ((Action) e.Result)());
		}

		private void Invoke(Action callback)
		{
			lock (this)
			{
				_actionsQueue.Enqueue(callback);
			}
		}

		private IEnumerator Ever()
		{
			while (true)
			{
				lock (this)
				{
					while (true)
					{
						if (_actionsQueue.Count < 1) break;
						_actionsQueue.Dequeue()();
					}
				}
				yield return new WaitForSeconds(UpdateTime);
			}
			// ReSharper disable once FunctionNeverReturns
		}

		private class Request
		{
			public Request(Func<object, IEnumerator> actionp,
				object argument, Action endCallback, Action<float> progressCallback)
			{
				ActionP = actionp;
				Argument = argument;
				EndCallback = endCallback;
				ProgressCallback = progressCallback;
			}
			public Request(Func<IEnumerator> action,
				Action endCallback, Action<float> progressCallback)
			{
				Action = action;
				EndCallback = endCallback;
				ProgressCallback = progressCallback;
			}
			public Func<object, IEnumerator> ActionP;
			public Func<IEnumerator> Action;
			public object Argument;
			public Action EndCallback;
			public Action<float> ProgressCallback;
		}
		#else
		private IEnumerator DoBackgoroundWork(IEnumerator en,
		Action endCallback, Action<float> progressCallback)
		{
		yield return null; // all calculations will start in the next frame
		while (en.MoveNext())
		{
		if (en.Current == null)
		yield return null;
		else if (en.Current is Wait)
		yield return new WaitForSeconds(((Wait) en.Current).Time);
		else if (en.Current is Progress)
		{
		if (progressCallback != null)
		progressCallback(((Progress) en.Current).Percents);
		yield return null;
		}
		else
		{
		Debug.LogWarning("Unknown background event type: " + en.Current);
		yield return en.Current;
		}
		if (_cancelled)
		break;
		}
		if (!_cancelled)
		endCallback();
		_coroutine = null;
		}
		#endif

		void OnDisable()
		{
			if (_coroutine != null)
				StopCoroutine(_coroutine);
		}

		private void LocalRun(Func<object, IEnumerator> action,
			object argument, Action endCallback, Action<float> progressCallback)
		{
			#if MULTITHREADING_ENABLED
			if (!_bw.IsBusy)
				_bw.RunWorkerAsync(new Request(action, argument, endCallback, progressCallback));
			#else
			_cancelled = false;
			_coroutine = StartCoroutine(DoBackgoroundWork(action(argument), endCallback, progressCallback));
			#endif
		}
		private void LocalRun(Func<IEnumerator> action,
			Action endCallback, Action<float> progressCallback)
		{
			#if MULTITHREADING_ENABLED
			if (!_bw.IsBusy)
				_bw.RunWorkerAsync(new Request(action, endCallback, progressCallback));
			#else
			_cancelled = false;
			_coroutine = StartCoroutine(DoBackgoroundWork(action(), endCallback, progressCallback));
			#endif
		}

		public static void Run(Func<object, IEnumerator> action,
			object argument, Action endCallback, Action<float> progressCallback = null)
		{
			if (_instance == null || !_instance.isActiveAndEnabled)
			{
				Debug.LogError("UnityBackgroundWorker is not instantiated or enabled");
				return;
			}
			_instance.LocalRun(action, argument, endCallback, progressCallback);
		}
		public static void Run(Func<IEnumerator> action, 
			Action endCallback, Action<float> progressCallback = null)
		{
			if (_instance == null || !_instance.isActiveAndEnabled)
			{
				Debug.LogError("UnityBackgroundWorker is not instantiated or enabled");
				return;
			}
			_instance.LocalRun(action, endCallback, progressCallback);
		}

		public static void Stop()
		{
			if (_instance == null || !_instance.isActiveAndEnabled)
			{
				Debug.LogError("UnityBackgroundWorker is not instantiated or enabled");
				return;
			}
			#if MULTITHREADING_ENABLED
			if (_instance._bw.IsBusy)
				_instance._bw.CancelAsync();
			#else
			_instance._cancelled = true;
			#endif
		}

		public class Wait
		{
			public Wait(float time) { Time = time; }
			public float Time;
		}

		public class Progress
		{
			public Progress(float p) { Percents = p; }
			public float Percents;
		}
	}
}