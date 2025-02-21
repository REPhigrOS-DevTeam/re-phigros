using System;
using System.Collections.Generic;
using MainCore.Common;
using MainCore.Settings;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

namespace MainCore.ECS
{
    public class EffectSystemManager : MonoSingleton<EffectSystemManager>
    {
        public Mesh mesh;
        [SerializeField] private Material materialPrefab;

        private readonly Dictionary<Skin, Material> _internalParticleCache = new();

        public Material Material { get; private set; }

        [SerializeField] private int particleCount;
        [SerializeField] private Color colorToDraw;
        [SerializeField] private Button button;
        public Camera drawCamera;

        private EntityArchetype archetype;
        private EntityManager manager;
        private static readonly int MainTextureId = Shader.PropertyToID("_MainTex");

        protected override void OnAwake()
        {
            LoadAllInternalParticle();
            UpdateSkin();
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            archetype = manager.CreateArchetype(
                typeof(HitEffectParticleData),
                typeof(LocalToWorld),
                typeof(LocalTransform) // 使用 LocalTransform 替代 Scale 和 Translation
            );

            World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<MeshDrawer>().BindCamera();
        }

        private void LoadAllInternalParticle()
        {
            foreach (Skin skin in Enum.GetValues(typeof(Skin)))
            {
                Sprite hitParticle = SkinManager.Instance.GetInternalSkinInfo(skin).hitParticle;
                Material material = Instantiate(materialPrefab);
                material.SetTexture(MainTextureId, hitParticle.texture);
                _internalParticleCache.Add(skin, material);
            }
        }

        public void UpdateSkin()
        {
            Material = GlobalSetting.CurrentSkinInfo.isExternal
                ? _internalParticleCache[Skin.Phira]
                : _internalParticleCache[GlobalSetting.CurrentSkinInfo.skin];
        }

        public void CreateParticle(int cnt, Color color, float3 centerPosition, float scale)
        {
            for (int i = 0; i < cnt; i++)
            {
                var entity = manager.CreateEntity(archetype);
                manager.SetComponentData(entity, new HitEffectParticleData
                {
                    centerPosition = centerPosition,
                    scale = scale,
                    time = 0,
                    color = (Vector4)color,
                    spd = Random.Range(0f, 1f) * 80f + 185f,
                    rad = Random.Range(0f, 360f) * Mathf.Deg2Rad,
                });

                // 设置 LocalTransform
                manager.SetComponentData(entity, LocalTransform.FromPositionRotationScale(centerPosition, Quaternion.identity, scale));
            }
        }
    }

    public struct HitEffectParticleData : IComponentData
    {
        public float3 centerPosition;
        public float scale;
        public float time;
        public float4 color;
        public float spd;
        public float rad;
    }

    [BurstCompile]
    public partial class ParticleSystem : SystemBase
    {
        private EntityCommandBufferSystem mBarrier;

        protected override void OnCreate()
        {
            mBarrier = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = mBarrier.CreateCommandBuffer().AsParallelWriter();

            float deltaTime = World.Time.DeltaTime;
            Entities.ForEach((Entity entity, ref LocalTransform transform,
                ref HitEffectParticleData data) =>
            {
                if (data.time > 1f)
                {
                    commandBuffer.DestroyEntity(0, entity);
                    return;
                }

                float a = (6.234f * math.pow(data.time, 3) - 49.572f * data.time * data.time + 49.197f * data.time + 14.964f) * 0.01f;
                float b = ((data.spd) * 9 * data.time / (8 * data.time + 1)) * 0.011f;

                // 更新 LocalTransform 的位置和缩放
                transform.Position = new float3(
                    data.centerPosition.x + b * math.cos(data.rad) * data.scale,
                    data.centerPosition.y + b * math.sin(data.rad) * data.scale,
                    data.centerPosition.z
                );
                transform.Scale = a * data.scale * 1.5f;

                data.color.w = 1 - data.time;
                data.time += deltaTime * 2;
            }).ScheduleParallel();

            mBarrier.AddJobHandleForProducer(Dependency);
        }
    }

    public partial class MeshDrawer : SystemBase
    {
        private static readonly int Color1 = Shader.PropertyToID("_Color");
        private MaterialPropertyBlock _block = new MaterialPropertyBlock();

        private readonly List<Vector4> _colors = new List<Vector4>();
        private Camera _drawCamera;
        private readonly List<Matrix4x4> _matrices = new List<Matrix4x4>();

        protected override void OnCreate()
        {
        }

        public void BindCamera()
        {
            _drawCamera = EffectSystemManager.Instance.drawCamera;
        }

        protected override void OnUpdate()
        {
            _block = new MaterialPropertyBlock();
            _colors.Clear();
            _matrices.Clear();
            int cnt = 0;

            Entities.ForEach((in LocalToWorld matrix, in HitEffectParticleData data) =>
            {
                _colors.Add(data.color);
                _matrices.Add(matrix.Value);
                cnt++;

                if (cnt >= 1023)
                {
                    _block.SetVectorArray(Color1, _colors);
                    Graphics.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0,
                        EffectSystemManager.Instance.Material, _matrices, _block, ShadowCastingMode.Off, false, 7,
                        _drawCamera);
                    _colors.Clear();
                    _matrices.Clear();
                    cnt = 0;
                }
            }).WithoutBurst().Run(); // 使用 Run() 确保在主线程执行

            if (cnt > 0)
            {
                _block.SetVectorArray(Color1, _colors);
                Graphics.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0, EffectSystemManager.Instance.Material,
                    _matrices, _block, ShadowCastingMode.Off, false, 7, _drawCamera);
            }
        }
    }
}