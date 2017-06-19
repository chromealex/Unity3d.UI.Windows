using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ME.UAB.Serializers {

	public class MinMaxCurveSerializer : ISerializer {

		public class Data {

			public float constant;
			public float constantMax;
			public float constantMin;
			public AnimationCurve curve;
			public AnimationCurve curveMax;
			public AnimationCurve curveMin;
			public float curveMultiplier;
			public ParticleSystemCurveMode mode;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(UnityEngine.ParticleSystem.MinMaxCurve);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new UnityEngine.ParticleSystem.MinMaxCurve() {
				mode = data.mode,
				#if UNITY_5_5_OR_NEWER
				constant = data.constant,
				#endif
				constantMin = data.constantMin,
				constantMax = data.constantMax,
				#if UNITY_5_5_OR_NEWER
				curve = data.curve,
				#endif
				curveMin = data.curveMin,
				curveMax = data.curveMax,
				#if UNITY_5_5_OR_NEWER
				curveMultiplier = data.curveMultiplier,
				#endif
			};

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			var data = new Data();
			var tr = (UnityEngine.ParticleSystem.MinMaxCurve)value;
			data.mode = tr.mode;
			#if UNITY_5_5_OR_NEWER
			data.constant = tr.constant;
			#endif
			data.constantMin = tr.constantMin;
			data.constantMax = tr.constantMax;
			#if UNITY_5_5_OR_NEWER
			data.curve = tr.curve;
			#endif
			data.curveMin = tr.curveMin;
			data.curveMax = tr.curveMax;
			#if UNITY_5_5_OR_NEWER
			data.curveMultiplier = tr.curveMultiplier;
			#endif
			field.fields = packer.Serialize(data, serializers);

		}

	}

	public class MinMaxGradientSerializer : ISerializer {

		public class Data {

			public Color color;
			public Color colorMax;
			public Color colorMin;
			public Gradient gradient;
			public Gradient gradientMax;
			public Gradient gradientMin;
			public ParticleSystemGradientMode mode;

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(UnityEngine.ParticleSystem.MinMaxGradient);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			value = new UnityEngine.ParticleSystem.MinMaxGradient() {
				mode = data.mode,
				#if UNITY_5_5_OR_NEWER
				color = data.color,
				#endif
				colorMin = data.colorMin,
				colorMax = data.colorMax,
				#if UNITY_5_5_OR_NEWER
				gradient = data.gradient,
				#endif
				gradientMin = data.gradientMin,
				gradientMax = data.gradientMax,
			};

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			var data = new Data();
			var tr = (UnityEngine.ParticleSystem.MinMaxGradient)value;
			data.mode = tr.mode;
			#if UNITY_5_5_OR_NEWER
			data.color = tr.color;
			#endif
			data.colorMin = tr.colorMin;
			data.colorMax = tr.colorMax;
			#if UNITY_5_5_OR_NEWER
			data.gradient = tr.gradient;
			#endif
			data.gradientMin = tr.gradientMin;
			data.gradientMax = tr.gradientMax;
			field.fields = packer.Serialize(data, serializers);

		}

	}

	public class ParticleSystemSerializer : ISerializer {

		public class Data {

			[System.Serializable]
			public class MainModule {

				public Transform customSimulationSpace;
				public float duration;
				public ParticleSystem.MinMaxCurve gravityModifier;
				public float gravityModifierMultiplier;
				public bool loop;
				public int maxParticles;
				public bool playOnAwake;
				public bool prewarm;
				public float randomizeRotationDirection;
				public ParticleSystemScalingMode scalingMode;
				public ParticleSystemSimulationSpace simulationSpace;
				public float simulationSpeed;
				public ParticleSystem.MinMaxGradient startColor;
				public ParticleSystem.MinMaxCurve startDelay;
				public float startDelayMultiplier;
				public ParticleSystem.MinMaxCurve startLifetime;
				public float startLifetimeMultiplier;
				public ParticleSystem.MinMaxCurve startRotation;
				public bool startRotation3D;
				public float startRotationMultiplier;
				public ParticleSystem.MinMaxCurve startRotationX;
				public float startRotationXMultiplier;
				public ParticleSystem.MinMaxCurve startRotationY;
				public float startRotationYMultiplier;
				public ParticleSystem.MinMaxCurve startRotationZ;
				public float startRotationZMultiplier;
				public ParticleSystem.MinMaxCurve startSize;
				public bool startSize3D;
				public float startSizeMultiplier;
				public ParticleSystem.MinMaxCurve startSizeX;
				public float startSizeXMultiplier;
				public ParticleSystem.MinMaxCurve startSizeY;
				public float startSizeYMultiplier;
				public ParticleSystem.MinMaxCurve startSizeZ;
				public float startSizeZMultiplier;
				public ParticleSystem.MinMaxCurve startSpeed;
				public float startSpeedMultiplier;

			}

			[System.Serializable]
			public class EmissionModule {

				[System.Serializable]
				public class Burst {

					public short maxCount;
					public short minCount;
					public float time;

				}

				[System.Serializable]
				public class Data {

					public bool enabled;
					public ParticleSystem.MinMaxCurve rateOverTime;
					public float rateOverTimeMultiplier;
					public ParticleSystem.MinMaxCurve rateOverDistance;
					public float rateOverDistanceMultiplier;

				}

				public Data data = new Data();
				public Burst[] bursts;

			}

			[System.Serializable]
			public class ShapeModule {

				public bool alignToDirection;
				public float angle;
				public float arc;
				public Vector3 box;
				public bool enabled;
				public float length;
				public int meshMaterialIndex;
				public Mesh mesh;
				public MeshRenderer meshRenderer;
				public float meshScale;
				public ParticleSystemMeshShapeType meshShapeType;
				public float normalOffset;
				public float radius;
				public float randomDirectionAmount;
				public ParticleSystemShapeType shapeType;
				public SkinnedMeshRenderer skinnedMeshRenderer;
				public float sphericalDirectionAmount;
				public bool useMeshColors;
				public bool useMeshMaterialIndex;

			}

			[System.Serializable]
			public class VelocityOverLifetimeModule {

				public bool enabled;
				public ParticleSystemSimulationSpace space;
				public ParticleSystem.MinMaxCurve x;
				public float xMultiplier;
				public ParticleSystem.MinMaxCurve y;
				public float yMultiplier;
				public ParticleSystem.MinMaxCurve z;
				public float zMultiplier;

			}

			[System.Serializable]
			public class LimitVelocityOverLifetimeModule {

				public float dampen;
				public bool enabled;
				public ParticleSystem.MinMaxCurve limit;
				public float limitMultiplier;
				public ParticleSystem.MinMaxCurve limitX;
				public float limitXMultiplier;
				public ParticleSystem.MinMaxCurve limitY;
				public float limitYMultiplier;
				public ParticleSystem.MinMaxCurve limitZ;
				public float limitZMultiplier;
				public bool separateAxes;
				public ParticleSystemSimulationSpace space;

			}

			[System.Serializable]
			public class InheritVelocityModule {

				public ParticleSystem.MinMaxCurve curve;
				public float curveMultiplier;
				public bool enabled;
				public ParticleSystemInheritVelocityMode mode;

			}

			[System.Serializable]
			public class ForceOverLifetimeModule {

				public bool enabled;
				public bool randomized;
				public ParticleSystemSimulationSpace space;
				public ParticleSystem.MinMaxCurve x;
				public float xMultiplier;
				public ParticleSystem.MinMaxCurve y;
				public float yMultiplier;
				public ParticleSystem.MinMaxCurve z;
				public float zMultiplier;

			}

			[System.Serializable]
			public class ColorOverLifetimeModule {

				public ParticleSystem.MinMaxGradient color;
				public bool enabled;

			}

			[System.Serializable]
			public class SizeOverLifetimeModule {

				public bool enabled;
				public bool separateAxes;
				public ParticleSystem.MinMaxCurve size;
				public float sizeMultiplier;
				public ParticleSystem.MinMaxCurve x;
				public float xMultiplier;
				public ParticleSystem.MinMaxCurve y;
				public float yMultiplier;
				public ParticleSystem.MinMaxCurve z;
				public float zMultiplier;

			}

			[System.Serializable]
			public class SizeBySpeedModule {

				public bool enabled;
				public Vector2 range;
				public bool separateAxes;
				public ParticleSystem.MinMaxCurve size;
				public float sizeMultiplier;
				public ParticleSystem.MinMaxCurve x;
				public float xMultiplier;
				public ParticleSystem.MinMaxCurve y;
				public float yMultiplier;
				public ParticleSystem.MinMaxCurve z;
				public float zMultiplier;

			}

			[System.Serializable]
			public class RotationOverLifetimeModule {

				public bool enabled;
				public bool separateAxes;
				public ParticleSystem.MinMaxCurve x;
				public float xMultiplier;
				public ParticleSystem.MinMaxCurve y;
				public float yMultiplier;
				public ParticleSystem.MinMaxCurve z;
				public float zMultiplier;

			}

			[System.Serializable]
			public class RotationBySpeedModule {

				public bool enabled;
				public Vector2 range;
				public bool separateAxes;
				public ParticleSystem.MinMaxCurve x;
				public float xMultiplier;
				public ParticleSystem.MinMaxCurve y;
				public float yMultiplier;
				public ParticleSystem.MinMaxCurve z;
				public float zMultiplier;

			}

			[System.Serializable]
			public class ExternalForcesModule {

				public bool enabled;
				public float multiplier;

			}

			[System.Serializable]
			public class NoiseModule {

				public bool damping;
				public bool enabled;
				public float frequency;
				public int octaveCount;
				public float octaveMultiplier;
				public float octaveScale;
				#if UNITY_5_5_OR_NEWER
				public ParticleSystemNoiseQuality quality;
				#endif
				public ParticleSystem.MinMaxCurve remap;
				public bool remapEnabled;
				public float remapMultiplier;
				public ParticleSystem.MinMaxCurve remapX;
				public float remapXMultiplier;
				public ParticleSystem.MinMaxCurve remapY;
				public float remapYMultiplier;
				public ParticleSystem.MinMaxCurve remapZ;
				public float remapZMultiplier;
				public ParticleSystem.MinMaxCurve scrollSpeed;
				public float scrollSpeedMultiplier;
				public bool separateAxes;
				public ParticleSystem.MinMaxCurve strength;
				public float strengthMultiplier;
				public ParticleSystem.MinMaxCurve strengthX;
				public float strengthXMultiplier;
				public ParticleSystem.MinMaxCurve strengthY;
				public float strengthYMultiplier;
				public ParticleSystem.MinMaxCurve strengthZ;
				public float strengthZMultiplier;

			}

			[System.Serializable]
			public class CollisionModule {

				public ParticleSystem.MinMaxCurve bounce;
				public float bounceMultiplier;
				public LayerMask collidesWith;
				public ParticleSystem.MinMaxCurve dampen;
				public float dampenMultiplier;
				public bool enabled;
				public bool enableDynamicColliders;
				public bool enableInteriorCollisions;
				public ParticleSystem.MinMaxCurve lifetimeLoss;
				public float lifetimeLossMultiplier;
				public int maxCollisionShapes;
				public float maxKillSpeed;
				public float minKillSpeed;
				public ParticleSystemCollisionMode mode;
				public ParticleSystemCollisionQuality quality;
				public float radiusScale;
				public bool sendCollisionMessages;
				public ParticleSystemCollisionType type;
				public float voxelSize;

			}

			[System.Serializable]
			public class TriggersModule {

				public bool enabled;
				#if UNITY_5_5_OR_NEWER
				public ParticleSystemOverlapAction enter;
				public ParticleSystemOverlapAction exit;
				public ParticleSystemOverlapAction inside;
				public ParticleSystemOverlapAction outside;
				#endif
				public float radiusScale;

			}

			[System.Serializable]
			public class SubEmittersModule {

				[System.Serializable]
				public class Data {

					public bool enabled;

				}

				public Data data = new Data();
				public ParticleSystem[] subEmitters;
				#if UNITY_5_5_OR_NEWER
				public ParticleSystemSubEmitterProperties[] properties;
				public ParticleSystemSubEmitterType[] types;
				#endif

			}

			[System.Serializable]
			public class TextureSheetAnimationModule {

				public ParticleSystemAnimationType animation;
				public int cycleCount;
				public bool enabled;
				public float flipU;
				public float flipV;
				public ParticleSystem.MinMaxCurve frameOverTime;
				public float frameOverTimeMultiplier;
				public int numTilesX;
				public int numTilesY;
				public int rowIndex;
				public ParticleSystem.MinMaxCurve startFrame;
				public float startFrameMultiplier;
				public bool useRandomRow;
				#if UNITY_5_5_OR_NEWER
				public UnityEngine.Rendering.UVChannelFlags uvChannelMask;
				#endif

			}

			[System.Serializable]
			public class LightsModule {

				public bool alphaAffectsIntensity;
				public bool enabled;
				public ParticleSystem.MinMaxCurve intensity;
				public float intensityMultiplier;
				public Light light;
				public int maxLights;
				public ParticleSystem.MinMaxCurve range;
				public float rangeMultiplier;
				public float ratio;
				public bool sizeAffectsRange;
				public bool useParticleColor;
				public bool useRandomDistribution;

			}

			[System.Serializable]
			public class TrailModule {

				public ParticleSystem.MinMaxGradient colorOverLifetime;
				public ParticleSystem.MinMaxGradient colorOverTrail;
				public bool dieWithParticles;
				public bool enabled;
				public bool inheritParticleColor;
				public ParticleSystem.MinMaxCurve lifetime;
				public float lifetimeMultiplier;
				public float minVertexDistance;
				public float ratio;
				public bool sizeAffectsLifetime;
				public bool sizeAffectsWidth;
				#if UNITY_5_5_OR_NEWER
				public ParticleSystemTrailTextureMode textureMode;
				#endif
				public ParticleSystem.MinMaxCurve widthOverTrail;
				public float widthOverTrailMultiplier;
				public bool worldSpace;

			}

			public bool useAutoRandomSeed;
			public uint randomSeed;

			public MainModule main = new MainModule();
			public EmissionModule emission = new EmissionModule();
			public ShapeModule shape = new ShapeModule();
			public VelocityOverLifetimeModule velocityOverLifetime = new VelocityOverLifetimeModule();
			public LimitVelocityOverLifetimeModule limitVelocityOverLifetime = new LimitVelocityOverLifetimeModule();
			public InheritVelocityModule inheritVelocity = new InheritVelocityModule();
			public ForceOverLifetimeModule forceOverLifetime = new ForceOverLifetimeModule();
			public ColorOverLifetimeModule colorOverLifetime = new ColorOverLifetimeModule();
			public SizeOverLifetimeModule sizeOverLifetime = new SizeOverLifetimeModule();
			public SizeBySpeedModule sizeBySpeed = new SizeBySpeedModule();
			public RotationOverLifetimeModule rotationOverLifetime = new RotationOverLifetimeModule();
			public RotationBySpeedModule rotationBySpeed = new RotationBySpeedModule();
			public ExternalForcesModule externalForces = new ExternalForcesModule();
			public NoiseModule noise = new NoiseModule();
			public CollisionModule collision = new CollisionModule();
			public TriggersModule trigger = new TriggersModule();
			public SubEmittersModule subEmitters = new SubEmittersModule();
			public TextureSheetAnimationModule textureSheetAnimation = new TextureSheetAnimationModule();
			public LightsModule lights = new LightsModule();
			public TrailModule trails = new TrailModule();

		}

		public string GetId() {

			return this.GetType().Name;

		}

		public bool IsValid(string id) {

			return this.GetId() == id;

		}

		public bool IsValid(object value) {

			return value.GetType() == typeof(ParticleSystem);

		}

		public void DeserializeBeforeRef(UABUnpacker unpacker, UABField field, ref object value, List<ISerializer> serializers) {

			var data = new Data();
			unpacker.Deserialize(data, field.fields, serializers);
			var ps = value as ParticleSystem;

			#if UNITY_5_5_OR_NEWER
			ps.useAutoRandomSeed = data.useAutoRandomSeed;
			if (ps.useAutoRandomSeed == false) ps.randomSeed = data.randomSeed;
			#endif

			#if UNITY_5_5_OR_NEWER
			UABUtils.CopyToParticleSystemModule(data.main, ps.main);
			{
				UABUtils.CopyToParticleSystemModule(data.emission.data, ps.emission);
				var bursts = new UnityEngine.ParticleSystem.Burst[data.emission.bursts.Length];
				for (int i = 0; i < data.emission.bursts.Length; ++i) {

					bursts[i] = UABUtils.CopyToParticleSystemModule(data.emission.bursts[i], bursts[i]);

				}
				ps.emission.SetBursts(bursts, data.emission.bursts.Length);
			}
			#endif
			UABUtils.CopyToParticleSystemModule(data.shape, ps.shape);
			UABUtils.CopyToParticleSystemModule(data.velocityOverLifetime, ps.velocityOverLifetime);
			UABUtils.CopyToParticleSystemModule(data.limitVelocityOverLifetime, ps.limitVelocityOverLifetime);
			UABUtils.CopyToParticleSystemModule(data.inheritVelocity, ps.inheritVelocity);
			UABUtils.CopyToParticleSystemModule(data.forceOverLifetime, ps.forceOverLifetime);
			UABUtils.CopyToParticleSystemModule(data.colorOverLifetime, ps.colorOverLifetime);
			UABUtils.CopyToParticleSystemModule(data.sizeOverLifetime, ps.sizeOverLifetime);
			UABUtils.CopyToParticleSystemModule(data.sizeBySpeed, ps.sizeBySpeed);
			UABUtils.CopyToParticleSystemModule(data.rotationOverLifetime, ps.rotationOverLifetime);
			UABUtils.CopyToParticleSystemModule(data.rotationBySpeed, ps.rotationBySpeed);
			UABUtils.CopyToParticleSystemModule(data.externalForces, ps.externalForces);
			UABUtils.CopyToParticleSystemModule(data.rotationBySpeed, ps.rotationBySpeed);
			#if UNITY_5_5_OR_NEWER
			UABUtils.CopyToParticleSystemModule(data.noise, ps.noise);
			#endif
			UABUtils.CopyToParticleSystemModule(data.collision, ps.collision);
			#if UNITY_5_5_OR_NEWER
			UABUtils.CopyToParticleSystemModule(data.trigger, ps.trigger);
			{
				UABUtils.CopyToParticleSystemModule(data.subEmitters.data, ps.subEmitters);
				for (int i = 0; i < data.subEmitters.subEmitters.Length; ++i) {

					ps.subEmitters.AddSubEmitter(data.subEmitters.subEmitters[i], data.subEmitters.types[i], data.subEmitters.properties[i]);

				}
			}
			//UABUtils.CopyToParticleSystemModule(data.textureSheetAnimation, ps.textureSheetAnimation);
			UABUtils.CopyToParticleSystemModule(data.lights, ps.lights);
			UABUtils.CopyToParticleSystemModule(data.trails, ps.trails);
			#endif

		}

		public void Serialize(UABPacker packer, UABField field, ref object value, List<ISerializer> serializers) {

			field.serializatorId = this.GetId();
			var ps = value as ParticleSystem;
			var data = new Data();

			#if UNITY_5_5_OR_NEWER
			data.useAutoRandomSeed = ps.useAutoRandomSeed;
			if (data.useAutoRandomSeed == false) {

				data.randomSeed = ps.randomSeed;

			} else {

				data.randomSeed = 0;

			}
			#endif

			#if UNITY_5_5_OR_NEWER
			data.main = UABUtils.CopyFromParticleSystemModule(ps.main, data.main);
			{
				data.emission.data = UABUtils.CopyFromParticleSystemModule(ps.emission, data.emission.data);
				var bursts = new UnityEngine.ParticleSystem.Burst[ps.emission.burstCount];
				ps.emission.GetBursts(bursts);
				data.emission.bursts = new Data.EmissionModule.Burst[bursts.Length];
				for (int i = 0; i < bursts.Length; ++i) {

					data.emission.bursts[i] = new Data.EmissionModule.Burst();
					UABUtils.CopyFromParticleSystemModule(bursts[i], data.emission.bursts[i]);

				}
			}
			#endif
			data.shape = UABUtils.CopyFromParticleSystemModule(ps.shape, data.shape);
			data.velocityOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.velocityOverLifetime, data.velocityOverLifetime);
			data.limitVelocityOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.limitVelocityOverLifetime, data.limitVelocityOverLifetime);
			data.inheritVelocity = UABUtils.CopyFromParticleSystemModule(ps.inheritVelocity, data.inheritVelocity);
			data.forceOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.forceOverLifetime, data.forceOverLifetime);
			data.colorOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.colorOverLifetime, data.colorOverLifetime);
			data.colorOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.colorOverLifetime, data.colorOverLifetime);
			data.sizeOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.sizeOverLifetime, data.sizeOverLifetime);
			data.sizeBySpeed = UABUtils.CopyFromParticleSystemModule(ps.sizeBySpeed, data.sizeBySpeed);
			data.rotationOverLifetime = UABUtils.CopyFromParticleSystemModule(ps.rotationOverLifetime, data.rotationOverLifetime);
			data.rotationBySpeed = UABUtils.CopyFromParticleSystemModule(ps.rotationBySpeed, data.rotationBySpeed);
			data.externalForces = UABUtils.CopyFromParticleSystemModule(ps.externalForces, data.externalForces);
			data.rotationBySpeed = UABUtils.CopyFromParticleSystemModule(ps.rotationBySpeed, data.rotationBySpeed);
			#if UNITY_5_5_OR_NEWER
			data.noise = UABUtils.CopyFromParticleSystemModule(ps.noise, data.noise);
			#endif
			data.collision = UABUtils.CopyFromParticleSystemModule(ps.collision, data.collision);
			#if UNITY_5_5_OR_NEWER
			data.trigger = UABUtils.CopyFromParticleSystemModule(ps.trigger, data.trigger);
			{
				data.subEmitters.data = UABUtils.CopyFromParticleSystemModule(ps.subEmitters, data.subEmitters.data);
				data.subEmitters.subEmitters = new ParticleSystem[ps.subEmitters.subEmittersCount];
				data.subEmitters.properties = new ParticleSystemSubEmitterProperties[ps.subEmitters.subEmittersCount];
				data.subEmitters.types = new ParticleSystemSubEmitterType[ps.subEmitters.subEmittersCount];
				for (int i = 0; i < ps.subEmitters.subEmittersCount; ++i) {

					data.subEmitters.subEmitters[i] = ps.subEmitters.GetSubEmitterSystem(i);
					data.subEmitters.properties[i] = ps.subEmitters.GetSubEmitterProperties(i);
					data.subEmitters.types[i] = ps.subEmitters.GetSubEmitterType(i);

				}
			}
			#endif
			data.textureSheetAnimation = UABUtils.CopyFromParticleSystemModule(ps.textureSheetAnimation, data.textureSheetAnimation);
			#if UNITY_5_5_OR_NEWER
			data.lights = UABUtils.CopyFromParticleSystemModule(ps.lights, data.lights);
			data.trails = UABUtils.CopyFromParticleSystemModule(ps.trails, data.trails);
			#endif

			field.fields = packer.Serialize(data, serializers);

		}

	}

}