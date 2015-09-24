using UnityEngine;
using System.Collections;

public class ParticleSystemCached : MonoBehaviour {
	
	public ParticleSystem mainParticleSystem;
	public ParticleSystem[] particleSystems;
	public int count;
	public bool resetOnStop;

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

	public float duration {

		get {

			return (this.mainParticleSystem != null) ? this.mainParticleSystem.duration : 0f;

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

		if (this.resetOnStop == true) {
			
			if (this.mainParticleSystem != null) {
				
				this.mainParticleSystem.Clear();
				this.mainParticleSystem.Stop();
				
			}
			
		} else {

			if (this.mainParticleSystem != null) {

				this.mainParticleSystem.Stop();
				
			}

		}

	}

    public void SetTime(float time, bool withChildren) {

        if (withChildren == true) {

            for (int i = 0; i < this.particleSystems.Length; ++i) {

                this.particleSystems[i].time = time;

            }

        } else {

            if (this.mainParticleSystem != null) {

                this.mainParticleSystem.time = time;

            }

        }

    }

    public void Rewind(float time, bool withChildren, bool noRestart) {
        
        if (withChildren == true) {
			
			for (int i = 0; i < this.particleSystems.Length; ++i) {

			    var ps = this.particleSystems[i];

                if (ps != null) {

                    ps.randomSeed = 1;

                    if (noRestart == false) {

                        ps.Simulate(time, withChildren: false, restart: true);

                    }

                    ps.Simulate(time, withChildren: false, restart: false);

                }

			}

		} else {

			if (this.mainParticleSystem != null) {

                this.mainParticleSystem.randomSeed = 1;

			    if (noRestart == false) {

			        this.mainParticleSystem.Simulate(time, withChildren: false, restart: true);

                }

			    this.mainParticleSystem.Simulate(time, withChildren: false, restart: false);

            }

		}

	}

	public void Play(bool withChildren) {
		
		if (withChildren == true) {

			for (int i = 0; i < this.count; ++i) {

				if (this.particleSystems[i] != null) {

					this.particleSystems[i].Play();

				}

			}

		} else {

			if (this.mainParticleSystem != null) {

				this.mainParticleSystem.Play();
				
			}

		}

	}
	
    public void Stop(bool withChildren, bool resetOnStop) {

        if (withChildren == true) {

            for (int i = 0; i < this.count; ++i) {

                if (this.resetOnStop == true || resetOnStop == true) {

                    if (this.particleSystems[i] != null) {

                        this.particleSystems[i].Clear();
                        this.particleSystems[i].Stop();

                    }

                } else {

                    if (this.particleSystems[i] != null) {

                        this.particleSystems[i].Stop();

                    }

                }

            }

        } else {

            if (this.resetOnStop == true || resetOnStop == true) {

                if (this.mainParticleSystem != null) {

                    this.mainParticleSystem.Clear();
                    this.mainParticleSystem.Stop();

                }

            } else {

                if (this.mainParticleSystem != null) {

                    this.mainParticleSystem.Stop();

                }

            }

        }
        
    }

	public void Stop(bool withChildren) {
		
		this.Stop(withChildren, false);

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
