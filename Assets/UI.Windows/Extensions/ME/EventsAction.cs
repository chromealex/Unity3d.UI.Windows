using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class EventsAction<T> {

	private Dictionary<T, List<System.Action>> events = new Dictionary<T, List<System.Action>>();
	
	public void Register(T key, System.Action action) {
		
		List<System.Action> list;
		if (this.events.TryGetValue(key, out list) == true) {
			
			list.Add(action);
			
		} else {
			
			this.events.Add(key, new List<System.Action>() { action });
			
		}
		
	}

	public void Clear() {

		this.events.Clear();

	}

	public void Unregister(T key) {
		
		this.events.Remove(key);
		
	}
	
	public void Unregister(T key, System.Action action) {
		
		List<System.Action> list;
		if (this.events.TryGetValue(key, out list) == true) {
			
			list.Remove(action);
			
		}
		
	}
	
	public void Call(T key) {
		
		List<System.Action> list;
		if (this.events.TryGetValue(key, out list) == true) {
			
			foreach (var item in list) {
				
				if (item != null) item();
				
			}
			
		}
		
	}
	
}
