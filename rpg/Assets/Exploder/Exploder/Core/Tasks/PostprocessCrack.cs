// Version 1.5
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace Exploder
{
    class PostprocessCrack : Postprocess
    {
        public PostprocessCrack(Core Core) : base(Core)
        {
        }

        public override TaskType Type { get { return TaskType.PostprocessCrack; } }

        public override void Init()
        {
            base.Init();

            FragmentPool.Instance.ResetTransform();

            if (core.postList.Count > 0)
            {
                if (core.postList[0].skinnedOriginal)
                {
                    core.crackedPos = core.postList[0].skinnedOriginal.transform.position;
                    core.crackedRot = core.postList[0].skinnedOriginal.transform.rotation;
                }
                else
                {
                    core.crackedPos = core.postList[0].original.transform.position;
                    core.crackedRot = core.postList[0].original.transform.rotation;
                }
            }
        }

        public override bool Run(float frameBudget)
        {
            var count = core.postList.Count;

            while (core.poolIdx < count)
            {
                var fragment = core.pool[core.poolIdx];
                var mesh = core.postList[core.poolIdx];

                core.poolIdx++;

                if (!mesh.original)
                {
                    continue;
                }

                ExploderUtils.SetActiveRecursively(fragment.gameObject, false);

                var unityMesh = mesh.mesh.ToUnityMesh();

                fragment.meshFilter.sharedMesh = unityMesh;

                // choose proper material

                if (mesh.option && mesh.option.FragmentMaterial)
                {
                    fragment.meshRenderer.sharedMaterial = mesh.option.FragmentMaterial;
                }
                else
                {
                    if (core.parameters.FragmentOptions.FragmentMaterial != null)
                    {
                        fragment.meshRenderer.sharedMaterial = core.parameters.FragmentOptions.FragmentMaterial;
                    }
                    else
                    {
                        fragment.meshRenderer.sharedMaterial = mesh.material;
                    }
                }

                unityMesh.RecalculateBounds();

                var oldParent = fragment.transform.parent;
                fragment.transform.parent = mesh.parent;
                fragment.transform.position = mesh.position;
                fragment.transform.rotation = mesh.rotation;
                fragment.transform.localScale = mesh.localScale;
                fragment.transform.parent = null;
                fragment.transform.parent = oldParent;

                var plane = mesh.option && mesh.option.Plane2D;

                var use2d = core.parameters.Use2DCollision;

                if (!core.parameters.FragmentOptions.DisableColliders)
                {
                    if (core.parameters.MeshColliders && !use2d)
                    {
                        // dont use mesh colliders for 2d plane
                        if (!plane)
                        {
                            fragment.meshCollider.sharedMesh = unityMesh;
                        }
                    }
                    else
                    {

                        if (core.parameters.Use2DCollision)
                        {
                            MeshUtils.GeneratePolygonCollider(fragment.polygonCollider2D, unityMesh);
                        }
                        else
                        {
                            fragment.boxCollider.center = unityMesh.bounds.center;
                            fragment.boxCollider.size = unityMesh.bounds.extents;
                        }
                    }
                }

                if (mesh.option)
                {
                    mesh.option.DuplicateSettings(fragment.options);
                }

                var force = core.parameters.Force;
                if (mesh.option && mesh.option.UseLocalForce)
                {
                    force = mesh.option.Force;
                }

                // apply force to rigid body
                fragment.ApplyExplosion(mesh.transform, mesh.mesh.centroid, core.parameters.Position, core.parameters.FragmentOptions, core.parameters.UseForceVector,
                                        core.parameters.ForceVector, force, mesh.original, core.parameters.TargetFragments);

#if SHOW_DEBUG_LINES
            UnityEngine.Debug.DrawLine(settings.Position, forceVector * settings.Force, Color.yellow, 3);
#endif

                if (Watch.ElapsedMilliseconds > frameBudget)
                {
                    return false;
                }
            }

            core.cracked = true;

            Watch.Stop();

            return true;
        }
    }
}
