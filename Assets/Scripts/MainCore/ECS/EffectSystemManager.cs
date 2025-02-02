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

        // private Dictionary<string, Material> externalParticleCache = new();
        public Material Material { get; private set; }

        [SerializeField] private int particleCount;
        [SerializeField] private Color colorToDraw;
        [SerializeField] private Button button;
        public Camera drawCamera;

        private EntityArchetype archetype;

        //private GameObjectConversionSettings settings;
        private EntityManager manager;
        private static readonly int MainTextureId = Shader.PropertyToID("_MainTex");

        protected override void OnAwake()
        {
            LoadAllInternalParticle();
            UpdateSkin();
            //settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //button.onClick.AddListener(() => CreateParticle(particleCount, colorToDraw, 0, 1));

            archetype = manager.CreateArchetype(typeof(HitEffectParticleData),
                typeof(LocalToWorld),
                typeof(Scale),
                typeof(Translation));

            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MeshDrawer>().BindCamera();
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
            /*var array = new NativeArray<Entity>(cnt, Allocator.Temp);
            manager.CreateEntity(archetype, array);*/
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
        EntityCommandBufferSystem mBarrier;

        protected override void OnCreate()
        {
            mBarrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer = mBarrier.CreateCommandBuffer().AsParallelWriter();

            var deltaTime = Time.DeltaTime;
            Entities.ForEach((Entity entity, ref Translation translation, ref Scale scaler,
                ref HitEffectParticleData data) =>
            {
                if (data.time > 1f)
                {
                    commandBuffer.DestroyEntity(0, entity);
                    return;
                }

                var a = (6.234f * math.pow(data.time, 3) - 49.572f * data.time * data.time + 49.197f * data.time +
                         14.964f);
                a *= .01f;
                var b = ((data.spd) * 9 * data.time / (8 * data.time + 1)) * 0.011f;
                translation.Value.x = data.centerPosition.x + b * math.cos(data.rad) * data.scale;
                translation.Value.y = data.centerPosition.y + b * math.sin(data.rad) * data.scale;
                translation.Value.z = data.centerPosition.z;
                scaler.Value = a * data.scale * 1.5f;
                data.color.w = 1 - data.time;
                data.time += deltaTime * 2;
            }).ScheduleParallel();

            mBarrier.AddJobHandleForProducer(Dependency);
        }
    }

    public class MeshDrawer : ComponentSystem
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
            //Let's collect matrices and colors data and then use DrawMeshInstanced for better performance.
            _block = new();
            _colors.Clear();
            _matrices.Clear();
            var cnt = 0;
            Entities.ForEach((ref LocalToWorld matrix, ref HitEffectParticleData data) =>
            {
                _colors.Add(data.color);
                _matrices.Add(matrix.Value);
                cnt = cnt + 1;
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
                //block.SetColor(Color1, new Color(data.color.x, data.color.y, data.color.z, data.color.w));
                //Graphics.DrawMesh(EffectManager.Instance.mesh, matrix.Value, EffectManager.Instance.material, 0);
                //Graphics.DrawMesh(EffectSystemManager.Instance.mesh, matrix.Value, EffectSystemManager.Instance.material, 1, mainCamera, 0, block);
            });
            if (cnt <= 0) return;
            _block.SetVectorArray(Color1, _colors);
            Graphics.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0, EffectSystemManager.Instance.Material,
                _matrices, _block, ShadowCastingMode.Off, false, 7, _drawCamera);
            //CommandBuffer.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0, EffectSystemManager.Instance.material, 0, matrices.ToArray(),cnt, block);
        }
    }
}