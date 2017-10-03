// Version 1.5
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using System.Collections.Generic;
using UnityEngine;

namespace Exploder
{
    abstract class Postprocess : ExploderTask
    {
        protected Postprocess(Core Core) : base(Core)
        {
        }

        public override void Init()
        {
            base.Init();

            if (!core.splitMeshIslands)
            {
                core.postList = new List<MeshObject>(core.meshSet);
            }

            var fragmentsNum = core.postList.Count;

            FragmentPool.Instance.Allocate(fragmentsNum, core.parameters.MeshColliders, core.parameters.Use2DCollision, core.parameters.FragmentPrefab);
            FragmentPool.Instance.SetDeactivateOptions(core.parameters.DeactivateOptions, core.parameters.FadeoutOptions, core.parameters.DeactivateTimeout);
            FragmentPool.Instance.SetExplodableFragments(core.parameters.ExplodeFragments, core.parameters.DontUseTag);
            FragmentPool.Instance.SetFragmentPhysicsOptions(core.parameters.FragmentOptions, core.parameters.Use2DCollision);
            FragmentPool.Instance.SetSFXOptions(core.parameters.SFXOptions);

            core.poolIdx = 0;
            core.pool = FragmentPool.Instance.GetAvailableFragments(fragmentsNum);

            // run sfx
            if (core.parameters.SFXOptions.ExplosionSoundClip)
            {
                if (!core.audioSource)
                {
                    core.audioSource = core.gameObject.AddComponent<AudioSource>();
                }

                core.audioSource.PlayOneShot(core.parameters.SFXOptions.ExplosionSoundClip);
            }
        }
    }
}
