using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ME.UAB.Tests.UnityComponents {

	public static class Mesh {

		[NUnit.Framework.Test] public static void MeshFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<MeshRenderer>(system: true)); }
		[NUnit.Framework.Test] public static void TextMesh() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<TextMesh>(system: true)); }
		[NUnit.Framework.Test] public static void MeshRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<MeshRenderer>(system: true)); }
		[NUnit.Framework.Test] public static void SkinnedMeshRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SkinnedMeshRenderer>(system: true)); }

	}

	public static class Effects {

		public class ParticleTest : MonoBehaviour { public ParticleSystem.Particle data; }

		[NUnit.Framework.Test] public static void ParticleSystem() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ParticleSystem>(system: true)); }
		[NUnit.Framework.Test] public static void ParticleSystemRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ParticleSystemRenderer>(system: true)); }
		[NUnit.Framework.Test] public static void Particle() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ParticleTest>(system: true)); }

		[NUnit.Framework.Test] public static void LineRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<LineRenderer>(system: true)); }
		[NUnit.Framework.Test] public static void TrailRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<TrailRenderer>(system: true)); }

		[NUnit.Framework.Test] public static void LensFlare() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<LensFlare>(system: true)); }
		[NUnit.Framework.Test] public static void Projector() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Projector>(system: true)); }

	}

	public static class Physics3D {

		[NUnit.Framework.Test] public static void Rigidbody() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Rigidbody>(system: true)); }
		[NUnit.Framework.Test] public static void CharacterController() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CharacterController>(system: true)); }

		[NUnit.Framework.Test] public static void BoxCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<BoxCollider>(system: true)); }
		[NUnit.Framework.Test] public static void SphereCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SphereCollider>(system: true)); }
		[NUnit.Framework.Test] public static void CapsuleCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CapsuleCollider>(system: true)); }
		[NUnit.Framework.Test] public static void MeshCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<MeshCollider>(system: true)); }
		[NUnit.Framework.Test] public static void WheelCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<WheelCollider>(system: true)); }
		[NUnit.Framework.Test] public static void TerrainCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<TerrainCollider>(system: true)); }

		[NUnit.Framework.Test] public static void Cloth() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Cloth>(system: true)); }

		[NUnit.Framework.Test] public static void HingeJoint() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<HingeJoint>(system: true)); }
		[NUnit.Framework.Test] public static void FixedJoint() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<FixedJoint>(system: true)); }
		[NUnit.Framework.Test] public static void SpringJoint() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SpringJoint>(system: true)); }
		[NUnit.Framework.Test] public static void CharacterJoint() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CharacterJoint>(system: true)); }
		[NUnit.Framework.Test] public static void ConfigurableJoint() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ConfigurableJoint>(system: true)); }

		[NUnit.Framework.Test] public static void ConstantForce() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ConstantForce>(system: true)); }

	}

	public static class Physics2D {

		[NUnit.Framework.Test] public static void Rigidbody2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Rigidbody2D>(system: true)); }

		[NUnit.Framework.Test] public static void BoxCollider2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<BoxCollider2D>(system: true)); }
		[NUnit.Framework.Test] public static void CircleCollider2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CircleCollider2D>(system: true)); }
		[NUnit.Framework.Test] public static void EdgeCollider2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<EdgeCollider2D>(system: true)); }
		[NUnit.Framework.Test] public static void PolygonCollider2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<PolygonCollider2D>(system: true)); }
		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void CapsuleCollider2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CapsuleCollider2D>(system: true)); }
		#endif

		[NUnit.Framework.Test] public static void DistanceJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<DistanceJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void FixedJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<FixedJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void FrictionJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<FrictionJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void HingeJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<HingeJoint2D>(system: true, requiredTypes: new System.Type[] { typeof(Rigidbody2D) })); }
		[NUnit.Framework.Test] public static void RelativeJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<RelativeJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void SliderJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SliderJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void SpringJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SpringJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void TargetJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<TargetJoint2D>(system: true)); }
		[NUnit.Framework.Test] public static void WheelJoint2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<WheelJoint2D>(system: true)); }

		[NUnit.Framework.Test] public static void AreaEffector2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AreaEffector2D>(system: true)); }
		[NUnit.Framework.Test] public static void BuoyancyEffector2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<BuoyancyEffector2D>(system: true)); }
		[NUnit.Framework.Test] public static void PointEffector2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<PointEffector2D>(system: true)); }
		[NUnit.Framework.Test] public static void PlatformEffector2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<PlatformEffector2D>(system: true)); }
		[NUnit.Framework.Test] public static void SurfaceEffector2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SurfaceEffector2D>(system: true)); }

		[NUnit.Framework.Test] public static void ConstantForce2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ConstantForce2D>(system: true)); }

	}

	public static class Navigation {

		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void NavMeshAgent() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.AI.NavMeshAgent>(system: true)); }
		[NUnit.Framework.Test] public static void OffMeshLink() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.AI.OffMeshLink>(system: true)); }
		[NUnit.Framework.Test] public static void NavMeshObstacle() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.AI.NavMeshObstacle>(system: true)); }
		#else
		[NUnit.Framework.Test] public static void NavMeshAgent() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.NavMeshAgent>(system: true)); }
		[NUnit.Framework.Test] public static void OffMeshLink() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.OffMeshLink>(system: true)); }
		[NUnit.Framework.Test] public static void NavMeshObstacle() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.NavMeshObstacle>(system: true)); }
		#endif

	}

	public static class Audio {

		[NUnit.Framework.Test] public static void AudioListener() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioListener>(system: true)); }
		[NUnit.Framework.Test] public static void AudioSource() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioSource>(system: true)); }

		[NUnit.Framework.Test] public static void AudioReverbZone() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioReverbZone>(system: true, requiredTypes: new System.Type[] { typeof(AudioSource) })); }
		//[NUnit.Framework.Test] public static void AudioLowPassFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioLowPassFilter>(system: true)); }
		[NUnit.Framework.Test] public static void AudioHighPassFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioHighPassFilter>(system: true, requiredTypes: new System.Type[] { typeof(AudioSource) })); }
		[NUnit.Framework.Test] public static void AudioEchoFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioEchoFilter>(system: true, requiredTypes: new System.Type[] { typeof(AudioSource) })); }
		[NUnit.Framework.Test] public static void AudioDistortionFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioDistortionFilter>(system: true, requiredTypes: new System.Type[] { typeof(AudioSource) })); }
		[NUnit.Framework.Test] public static void AudioReverbFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioReverbFilter>(system: true, requiredTypes: new System.Type[] { typeof(AudioSource) })); }
		[NUnit.Framework.Test] public static void AudioChorusFilter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<AudioChorusFilter>(system: true, requiredTypes: new System.Type[] { typeof(AudioSource) })); }

	}

	public static class Rendering {

		[NUnit.Framework.Test] public static void Camera() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Camera>(system: true)); }
		[NUnit.Framework.Test] public static void Skybox() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Skybox>(system: true)); }
		[NUnit.Framework.Test] public static void FlareLayer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<FlareLayer>(system: true)); }
		[NUnit.Framework.Test] public static void GUILayer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<GUILayer>(system: true)); }

		[NUnit.Framework.Test] public static void Light() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Light>(system: true)); }
		[NUnit.Framework.Test] public static void LightProbeGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<LightProbeGroup>(system: true)); }
		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void LightProbeProxyVolume() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<LightProbeProxyVolume>(system: true)); }
		#endif
		[NUnit.Framework.Test] public static void ReflectionProbe() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ReflectionProbe>(system: true)); }

		[NUnit.Framework.Test] public static void OcclusionArea() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<OcclusionArea>(system: true)); }
		[NUnit.Framework.Test] public static void OcclusionPortal() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<OcclusionPortal>(system: true)); }
		[NUnit.Framework.Test] public static void LODGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<LODGroup>(system: true)); }

		[NUnit.Framework.Test] public static void SpriteRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SpriteRenderer>(system: true)); }
		[NUnit.Framework.Test] public static void CanvasRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CanvasRenderer>(system: true)); }

		[NUnit.Framework.Test] public static void GUITexture() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<GUITexture>(system: true)); }
		[NUnit.Framework.Test] public static void GUIText() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<GUIText>(system: true)); }

	}

	public static class Layout {

		[NUnit.Framework.Test] public static void RectTransform() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<RectTransform>(system: true)); }
		[NUnit.Framework.Test] public static void Canvas() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Canvas>(system: true)); }
		[NUnit.Framework.Test] public static void CanvasGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CanvasGroup>(system: true)); }

		[NUnit.Framework.Test] public static void CanvasScaler() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.CanvasScaler>(system: true)); }

		[NUnit.Framework.Test] public static void LayoutElement() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.LayoutElement>(system: true)); }
		[NUnit.Framework.Test] public static void ContentSizeFitter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.ContentSizeFitter>(system: true)); }
		[NUnit.Framework.Test] public static void AspectRatioFitter() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.AspectRatioFitter>(system: true)); }
		[NUnit.Framework.Test] public static void HorizontalLayoutGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.HorizontalLayoutGroup>(system: true)); }
		[NUnit.Framework.Test] public static void VerticalLayoutGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.VerticalLayoutGroup>(system: true)); }
		[NUnit.Framework.Test] public static void GridLayoutGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.GridLayoutGroup>(system: true)); }

	}

	public static class Miscellaneous {

		[NUnit.Framework.Test] public static void Animator() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Animator>(system: true)); }
		[NUnit.Framework.Test] public static void Animation() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Animation>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkView() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<NetworkView>(system: true)); }
		[NUnit.Framework.Test] public static void WindZone() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<WindZone>(system: true)); }
		[NUnit.Framework.Test] public static void Terrain() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Terrain>(system: true)); }
		[NUnit.Framework.Test] public static void BillboardRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<BillboardRenderer>(system: true)); }
		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void WorldAnchor() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.VR.WSA.WorldAnchor>(system: true)); }
		#endif

	}

	public static class Analytics {

		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void AnalyticsTracker() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Analytics.AnalyticsTracker>(system: true)); }
		#endif

	}

	public static class Event {

		[NUnit.Framework.Test] public static void EventSystem() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.EventSystems.EventSystem>(system: true)); }
		[NUnit.Framework.Test] public static void EventTrigger() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.EventSystems.EventTrigger>(system: true)); }
		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void HoloLensInputModule() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.EventSystems.HoloLensInputModule>(system: true)); }
		#endif
		[NUnit.Framework.Test] public static void Physics2DRaycaster() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.EventSystems.Physics2DRaycaster>(system: true)); }
		[NUnit.Framework.Test] public static void PhysicsRaycaster() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.EventSystems.PhysicsRaycaster>(system: true)); }
		[NUnit.Framework.Test] public static void StandaloneInputModule() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.EventSystems.StandaloneInputModule>(system: true)); }
		[NUnit.Framework.Test] public static void GraphicRaycaster() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.GraphicRaycaster>(system: true)); }

	}

	public static class Network {

		[NUnit.Framework.Test] public static void NetworkAnimator() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkAnimator>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkDiscovery() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkDiscovery>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkIdentity() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkIdentity>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkLobbyManager() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkLobbyManager>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkLobbyPlayer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkLobbyPlayer>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkManager() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkManager>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkManagerHUD() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkManagerHUD>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkMigrationManager() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkMigrationManager>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkProximityChecker() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkProximityChecker>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkStartPosition() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkStartPosition>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkTransform() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkTransform>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkTransformChild() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkTransformChild>(system: true)); }
		[NUnit.Framework.Test] public static void NetworkTransformVisualizer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.Networking.NetworkTransformVisualizer>(system: true)); }

	}

	public static class AR {

		#if UNITY_5_5_OR_NEWER
		[NUnit.Framework.Test] public static void SpatialMappingCollider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.VR.WSA.SpatialMappingCollider>(system: true)); }
		[NUnit.Framework.Test] public static void SpatialMappingRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.VR.WSA.SpatialMappingRenderer>(system: true)); }
		#endif

	}

	public static class UI {

		public static class Effects {

			[NUnit.Framework.Test] public static void Shadow() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Shadow>(system: true)); }
			[NUnit.Framework.Test] public static void Outline() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Outline>(system: true)); }
			[NUnit.Framework.Test] public static void PositionAsUV1() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.PositionAsUV1>(system: true)); }

		}

		[NUnit.Framework.Test] public static void Image() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Image>(system: true)); }
		[NUnit.Framework.Test] public static void RawImage() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.RawImage>(system: true)); }
		[NUnit.Framework.Test] public static void CanvasGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CanvasGroup>(system: true)); }
		[NUnit.Framework.Test] public static void Canvas() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Canvas>(system: true)); }
		[NUnit.Framework.Test] public static void CanvasRenderer() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<CanvasRenderer>(system: true)); }
		[NUnit.Framework.Test] public static void ScrollRect() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.ScrollRect>(system: true)); }
		[NUnit.Framework.Test] public static void ScrollBar() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Scrollbar>(system: true)); }
		[NUnit.Framework.Test] public static void Slider() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Slider>(system: true)); }
		[NUnit.Framework.Test] public static void Button() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Button>(system: true)); }
		[NUnit.Framework.Test] public static void Text() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Text>(system: true)); }
		[NUnit.Framework.Test] public static void Toggle() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Toggle>(system: true)); }
		[NUnit.Framework.Test] public static void ToggleGroup() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.ToggleGroup>(system: true)); }
		[NUnit.Framework.Test] public static void Dropdown() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Dropdown>(system: true)); }
		[NUnit.Framework.Test] public static void Mask() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Mask>(system: true)); }
		[NUnit.Framework.Test] public static void RectMask2D() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.RectMask2D>(system: true)); }
		[NUnit.Framework.Test] public static void Selectable() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UnityEngine.UI.Selectable>(system: true)); }

	}

	public static class Structs {

		public class Vector2Test : MonoBehaviour { public Vector2 data; }
		public class Vector3Test : MonoBehaviour { public Vector3 data; }
		public class Vector4Test : MonoBehaviour { public Vector4 data; }
		public class QuaternionTest : MonoBehaviour { public Quaternion data; }
		public class Matrix4x4Test : MonoBehaviour { public Matrix4x4 data; }
		public class ColorTest : MonoBehaviour { public Color data; }
		public class FloatTest : MonoBehaviour { public float data; }
		public class IntTest : MonoBehaviour { public int data; }
		public class LongTest : MonoBehaviour { public long data; }
		public class DoubleTest : MonoBehaviour { public double data; }
		public class DecimalTest : MonoBehaviour { public decimal data; }
		public class ULongTest : MonoBehaviour { public ulong data; }
		public class UIntTest : MonoBehaviour { public uint data; }
		public class SByteTest : MonoBehaviour { public sbyte data; }
		public class ByteTest : MonoBehaviour { public byte data; }

		[NUnit.Framework.Test] public static void Vector2() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Vector2Test>(system: true)); }
		[NUnit.Framework.Test] public static void Vector3() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Vector3Test>(system: true)); }
		[NUnit.Framework.Test] public static void Vector4() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Vector4Test>(system: true)); }
		[NUnit.Framework.Test] public static void Quaternion() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<QuaternionTest>(system: true)); }
		[NUnit.Framework.Test] public static void Matrix4x4() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<Matrix4x4Test>(system: true)); }
		[NUnit.Framework.Test] public static void Color() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ColorTest>(system: true)); }
		[NUnit.Framework.Test] public static void Float() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<FloatTest>(system: true)); }
		[NUnit.Framework.Test] public static void Int() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<IntTest>(system: true)); }
		[NUnit.Framework.Test] public static void Long() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<LongTest>(system: true)); }
		[NUnit.Framework.Test] public static void Double() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<DoubleTest>(system: true)); }
		[NUnit.Framework.Test] public static void Decimal() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<DecimalTest>(system: true)); }
		[NUnit.Framework.Test] public static void ULong() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ULongTest>(system: true)); }
		[NUnit.Framework.Test] public static void UInt() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<UIntTest>(system: true)); }
		[NUnit.Framework.Test] public static void SByte() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<SByteTest>(system: true)); }
		[NUnit.Framework.Test] public static void Byte() { NUnit.Framework.Assert.IsTrue(UABTests.CompareType<ByteTest>(system: true)); }

	}

}