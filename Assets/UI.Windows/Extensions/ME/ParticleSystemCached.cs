using UnityEngine;
using System.Collections;

public class ParticleSystemCached : MonoBehaviour {
	
	public ParticleSystem mainParticleSystem;
	public ParticleSystem[] particleSystems;
	public int count;
	
	public Color32 startColor {
		
		set {
			
			if (this.mainParticleSystem != null) this.mainParticleSystem.startColor = value;
			
		}
		
		get {
			
			return (this.mainParticleSystem != null) ? this.mainParticleSystem.startColor : Color.white;
			
		}
		
	}
	
	public bool loop {
		
		set {
			
			if (this.mainParticleSystem != null) this.mainParticleSystem.loop = value;
			
		}
		
		get {
			
			return (this.mainParticleSystem != null) ? this.mainParticleSystem.loop : false;
			
		}
		
	}

#if UNITY_EDITOR
	[ContextMenu("Setup")]
	public void Setup() {
		
		this.particleSystems = this.GetComponentsInChildren<ParticleSystem>(true);
		this.mainParticleSystem = (this.particleSystems.Length > 0) ? this.particleSystems[0] : null;
		this.count = this.particleSystems.Length;
		
	}

	public void OnValidate() {

		this.Setup();

	}
#endif
	
	public void Play() {

		if (this.mainParticleSystem != null) this.mainParticleSystem.Play();
		
	}
	
	public void Stop() {

		if (this.mainParticleSystem != null) this.mainParticleSystem.Stop();
		
	}
	
	public void Play(bool withChildren) {

		for (int i = 0; i < this.count; ++i) {

			if (this.particleSystems[i] != null) this.particleSystems[i].Play();

		}
		
	}
	
	public void Stop(bool withChildren) {
		
		for (int i = 0; i < this.count; ++i) {

			if (this.particleSystems[i] != null) this.particleSystems[i].Stop();

		}
		
	}
	
	private ParticleSystem.Particle[] cache = new ParticleSystem.Particle[0];
	public void Stop(bool withChildren, float time) {
		
		for (int i = 0; i < this.count; ++i) {
			
			var ps = this.particleSystems[i];
			if (ps != null) {
				
				this.cache = new ParticleSystem.Particle[ps.particleCount];
				var count = ps.GetParticles(this.cache);
				for (int p = 0; p < count; ++p) {
					
					this.cache[p].lifetime = Random.Range(0f, time);
					
				}
				ps.SetParticles(this.cache, count);
				ps.Stop();
				
			}
			
		}
		
	}
	
}
