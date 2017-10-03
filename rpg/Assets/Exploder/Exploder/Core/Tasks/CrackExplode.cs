// Version 1.5
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEngine;

namespace Exploder
{
    class CrackExplode : ExploderTask
    {
        public CrackExplode(Core Core) : base(Core)
        {
        }

        public override TaskType Type { get { return TaskType.CrackExplode; } }

        public override bool Run(float frameBudget)
        {
            var count = core.postList.Count;
            core.poolIdx = 0;

            var diffPos = Vector3.zero;
            var diffRot = Quaternion.identity;

            if (core.postList.Count > 0)
            {
                if (core.postList[0].skinnedOriginal)
                {
                    diffPos = core.postList[0].skinnedOriginal.transform.position - core.crackedPos;
                    diffRot = core.postList[0].skinnedOriginal.transform.rotation * Quaternion.Inverse(core.crackedRot);
                }
                else
                {
                    diffPos = core.postList[0].original.transform.position - core.crackedPos;
                    diffRot = core.postList[0].original.transform.rotation * Quaternion.Inverse(core.crackedRot);
                }
            }

            while (core.poolIdx < count)
            {
                var fragment = core.pool[core.poolIdx];
                var mesh = core.postList[core.poolIdx];

                core.poolIdx++;

                if (mesh.original != core.parameters.ExploderGameObject)
                {
                    ExploderUtils.SetActiveRecursively(mesh.original, false);
                }
                else
                {
                    ExploderUtils.EnableCollider(mesh.original, false);
                    ExploderUtils.SetVisible(mesh.original, false);
                }

                if (mesh.skinnedOriginal && mesh.skinnedOriginal != core.parameters.ExploderGameObject)
                {
                    ExploderUtils.SetActiveRecursively(mesh.skinnedOriginal, false);
                }
                else
                {
                    ExploderUtils.EnableCollider(mesh.skinnedOriginal, false);
                    ExploderUtils.SetVisible(mesh.skinnedOriginal, false);
                }

                fragment.transform.position += diffPos;
                fragment.transform.rotation *= diffRot;

                fragment.Explode();
            }

            if (core.parameters.DestroyOriginalObject)
            {
                foreach (var mesh in core.postList)
                {
                    if (mesh.original && !mesh.original.GetComponent<Fragment>())
                    {
                        GameObject.Destroy(mesh.original);
                    }

                    if (mesh.skinnedOriginal)
                    {
                        GameObject.Destroy(mesh.skinnedOriginal);
                    }
                }
            }

            if (core.parameters.ExplodeSelf)
            {
                if (!core.parameters.DestroyOriginalObject)
                {
                    ExploderUtils.SetActiveRecursively(core.parameters.ExploderGameObject, false);
                }
            }

            if (core.parameters.HideSelf)
            {
                ExploderUtils.SetActiveRecursively(core.parameters.ExploderGameObject, false);
            }

#if DBG
        ExploderUtils.Log("Crack finished! " + postList.Count + postList[0].original.transform.gameObject.name);
#endif
            Watch.Stop();

            return true;
        }
    }
}
