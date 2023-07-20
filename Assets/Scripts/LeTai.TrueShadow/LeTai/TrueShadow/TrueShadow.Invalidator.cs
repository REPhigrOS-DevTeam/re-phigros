using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeTai.TrueShadow
{
    interface IChangeTracker
    {
        void Check();
    }

    class ChangeTracker<T> : IChangeTracker
    {
        readonly IEqualityComparer<T> comparer;
        readonly Func<T> getValue;
        readonly Func<T, T> onChange;
        T previousValue;

        public ChangeTracker(Func<T> getValue,
            Func<T, T> onChange,
            IEqualityComparer<T> comparer = null)
        {
            this.getValue = getValue;
            this.onChange = onChange;
            this.comparer = comparer ?? EqualityComparer<T>.Default;

            previousValue = this.getValue();
        }

        public void Check()
        {
            T newValue = getValue();
            if (!comparer.Equals(newValue, previousValue))
            {
                previousValue = onChange(newValue);
            }
        }

        public void Forget()
        {
            previousValue = getValue();
        }
    }

    public partial class TrueShadow
    {
        Action checkHierarchyDirtiedDelegate;
        ChangeTracker<int>[] hierachyTrackers;
        IChangeTracker[] transformTrackers;


        protected override void OnDidApplyAnimationProperties()
        {
            if (!isActiveAndEnabled) return;

            SetLayoutTextureDirty();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (!isActiveAndEnabled) return;

            SetLayoutTextureDirty();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            if (!isActiveAndEnabled) return;

            SetHierachyDirty();
            this.NextFrames(checkHierarchyDirtiedDelegate);
        }

        public void ModifyMesh(Mesh mesh)
        {
            if (!isActiveAndEnabled) return;

            if (SpriteMesh) Utility.SafeDestroy(SpriteMesh);
            SpriteMesh = Instantiate(mesh);

            SetLayoutTextureDirty();
        }

        public void ModifyMesh(VertexHelper verts)
        {
            if (!isActiveAndEnabled) return;

            // For when pressing play while in prefab mode
            if (!SpriteMesh) SpriteMesh = new Mesh();
            verts.FillMesh(SpriteMesh);

            SetLayoutTextureDirty();
        }

        void InitInvalidator()
        {
            checkHierarchyDirtiedDelegate = CheckHierarchyDirtied;
            hierachyTrackers = new[]
            {
                new ChangeTracker<int>(
                    () => transform.GetSiblingIndex(),
                    newValue =>
                    {
                        SetHierachyDirty();
                        return newValue; // + 1;
                    }
                ),
                new ChangeTracker<int>(
                    () =>
                    {
                        if (shadowRenderer)
                            return shadowRenderer.transform.GetSiblingIndex();
                        return -1;
                    },
                    newValue =>
                    {
                        SetHierachyDirty();
                        return newValue; // + 1;
                    }
                )
            };

            transformTrackers = new IChangeTracker[]
            {
                new ChangeTracker<Vector3>(
                    () => transform.position,
                    newValue =>
                    {
                        SetLayoutDirty();
                        return newValue;
                    }
                ),
                new ChangeTracker<Quaternion>(
                    () => transform.rotation,
                    newValue =>
                    {
                        SetLayoutDirty();
                        if (Cutout)
                            SetTextureDirty();
                        return newValue;
                    }
                ),
            };

            Graphic.RegisterDirtyLayoutCallback(SetLayoutTextureDirty);
            Graphic.RegisterDirtyVerticesCallback(SetLayoutTextureDirty);
            Graphic.RegisterDirtyMaterialCallback(OnGraphicMaterialDirty);

            CheckHierarchyDirtied();
            CheckTransformDirtied();
        }

        void TerminateInvalidator()
        {
            if (Graphic)
            {
                Graphic.UnregisterDirtyLayoutCallback(SetLayoutTextureDirty);
                Graphic.UnregisterDirtyVerticesCallback(SetLayoutTextureDirty);
                Graphic.UnregisterDirtyMaterialCallback(OnGraphicMaterialDirty);
            }
        }

        void OnGraphicMaterialDirty()
        {
            SetLayoutTextureDirty();
            shadowRenderer.UpdateMaterial();
        }

        internal void CheckTransformDirtied()
        {
            if (transformTrackers != null)
            {
                for (var i = 0; i < transformTrackers.Length; i++)
                {
                    transformTrackers[i].Check();
                }
            }
        }

        internal void CheckHierarchyDirtied()
        {
            if (ShadowAsSibling && hierachyTrackers != null)
            {
                for (var i = 0; i < hierachyTrackers.Length; i++)
                {
                    hierachyTrackers[i].Check();
                }
            }
        }

        internal void ForgetSiblingIndexChanges()
        {
            for (var i = 0; i < hierachyTrackers.Length; i++)
            {
                hierachyTrackers[i].Forget();
            }
        }

        void SetLayoutTextureDirty()
        {
            SetLayoutDirty();
            SetTextureDirty();
        }
    }
}