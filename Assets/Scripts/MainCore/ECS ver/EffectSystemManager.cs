using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MainCore.ECS_ver
{
    public class EffectSystemManager : MonoBehaviour
    {
        public Mesh mesh;
        [SerializeField] private Material materialPrefab;
        private Dictionary<Skin, Material> internalParticleCache = new();
        // private Dictionary<string, Material> externalParticleCache = new();
        public Material Material { get; private set; }

        [SerializeField] private int particleCount;
        [SerializeField] private Color colorToDraw;
        [SerializeField] private Button button;
        public Camera drawCamera;

        private EntityArchetype archetype;

        //private GameObjectConversionSettings settings;
        private EntityManager manager;
        private static readonly int Texture1 = Shader.PropertyToID("Texture");
        public static EffectSystemManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            LoadAllInternalParticle();
            Material = GlobalSetting.CurrentSkinInfo.isExternal ? internalParticleCache[Skin.Phira] : internalParticleCache[GlobalSetting.CurrentSkinInfo.skin];
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
                Sprite hitParticle = HitEffectManager.GetInstance().GetInternalSkinInfo(skin).hitParticle;
                Material material = Instantiate(materialPrefab);
                material.SetTexture(Texture1, hitParticle.texture);
                internalParticleCache.Add(skin, material);
            }
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
                    color = (Vector4) color,
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
        private MaterialPropertyBlock block = new MaterialPropertyBlock();

        List<Vector4> colors = new List<Vector4>();
        private Camera drawCamera;
        List<Matrix4x4> matrices = new List<Matrix4x4>();

        protected override void OnCreate()
        {
        }

        public void BindCamera()
        {
            drawCamera = EffectSystemManager.Instance.drawCamera;
        }

        protected override void OnUpdate()
        {
            //Let's collect matrices and colors data and then use DrawMeshInstanced for better performance.
            block = new();
            colors.Clear();
            matrices.Clear();
            var cnt = 0;
            Entities.ForEach((ref LocalToWorld matrix, ref HitEffectParticleData data) =>
            {
                colors.Add(data.color);
                matrices.Add(matrix.Value);
                cnt = cnt + 1;
                if (cnt >= 1023)
                {
                    block.SetVectorArray(Color1, colors);
                    Graphics.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0,
                        EffectSystemManager.Instance.Material, matrices, block, ShadowCastingMode.Off, false, 7,
                        drawCamera);
                    colors.Clear();
                    matrices.Clear();
                    cnt = 0;
                }
                //block.SetColor(Color1, new Color(data.color.x, data.color.y, data.color.z, data.color.w));
                //Graphics.DrawMesh(EffectManager.Instance.mesh, matrix.Value, EffectManager.Instance.material, 0);
                //Graphics.DrawMesh(EffectSystemManager.Instance.mesh, matrix.Value, EffectSystemManager.Instance.material, 1, mainCamera, 0, block);
            });
            if (cnt <= 0) return;
            block.SetVectorArray(Color1, colors);
            Graphics.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0, EffectSystemManager.Instance.Material,
                matrices, block, ShadowCastingMode.Off, false, 7, drawCamera);
            //CommandBuffer.DrawMeshInstanced(EffectSystemManager.Instance.mesh, 0, EffectSystemManager.Instance.material, 0, matrices.ToArray(),cnt, block);
        }
    }
}