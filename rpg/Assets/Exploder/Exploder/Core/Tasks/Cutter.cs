// Version 1.5
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

#if UNITY_WEBGL
#define DISABLE_MULTITHREADING
#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Exploder
{
    class Cutter : ExploderTask
    {
        private readonly int THREAD_MAX;

        private readonly HashSet<MeshObject> newFragments;
        private readonly HashSet<MeshObject> meshToRemove;
        private readonly MeshCutter cutter;
        private readonly CutterWorker[] workers;
        private readonly System.Random random;

        private int cutCounter = 0;

        private bool workersWorkingCache;

        public Cutter(Core Core) : base(Core)
        {
            // init cutter
            cutter = new MeshCutter();
            cutter.Init(512, 512);
            newFragments = new HashSet<MeshObject>();
            meshToRemove = new HashSet<MeshObject>();

#if DISABLE_MULTITHREADING
            THREAD_MAX = 1;
#else
            THREAD_MAX = Mathf.Clamp((int) Core.parameters.ThreadOptions + 1, 1, 4);
#endif

            UnityEngine.Debug.LogFormat("Exploder: using {0} threads.", THREAD_MAX);

            workers = new CutterWorker[THREAD_MAX - 1];

            random = new System.Random(0);

            for (int i=0; i< THREAD_MAX - 1; i++)
            {
                workers[i] = new CutterWorker(Core, random);
            };
        }

        public override TaskType Type { get { return TaskType.ProcessCutter; } }

        public override void Init()
        {
            base.Init();
            newFragments.Clear();
            meshToRemove.Clear();

            foreach (var worker in workers)
            {
                worker.Init();
            }

            workersWorkingCache = false;

            cutCounter = 0;
        }

        public override void OnDestroy()
        {
            foreach (var worker in workers)
            {
                worker.Terminate();
            }
        }

        public override bool Run(float frameBudget)
        {
            var mainCut = Cut(frameBudget);

            if (mainCut)
            {
                var finished = true;
                foreach (var worker in workers)
                {
                    finished &= worker.IsFinished();
                }

                if (finished)
                {
                    foreach (var worker in workers)
                    {
                        core.meshSet.UnionWith(worker.GetResults());
                    }

                    Watch.Stop();
                    return true;
                }
            }

            return false;
        }

        private bool Cut(float frameBudget)
        {
            bool cutting = true;
            var cycleCounter = 0;
            bool timeBudgetStop = false;

            while (cutting)
            {
                cycleCounter++;

                if (cycleCounter > core.parameters.TargetFragments)
                {
                    ExploderUtils.Log("Explode Infinite loop!");
                    return true;
                }

                newFragments.Clear();
                meshToRemove.Clear();

                cutting = false;

                foreach (var mesh in core.meshSet)
                {
                    if (core.targetFragments[mesh.id] > 1)
                    {
                        var randomPlaneNormal = new Vector3((float)random.NextDouble() * 2.0f - 1.0f,
                                                            (float)random.NextDouble() * 2.0f - 1.0f,
                                                            (float)random.NextDouble() * 2.0f - 1.0f);

                        var plane = new Exploder.Plane(randomPlaneNormal, mesh.mesh.centroid);

                        var triangulateHoles = true;
                        var crossSectionVertexColour = Color.white;
                        var crossSectionUV = new Vector4(0, 0, 1, 1);

                        if (mesh.option)
                        {
                            triangulateHoles = !mesh.option.Plane2D;
                            crossSectionVertexColour = mesh.option.CrossSectionVertexColor;
                            crossSectionUV = mesh.option.CrossSectionUV;
                            core.splitMeshIslands |= mesh.option.SplitMeshIslands;
                        }

                        if (core.parameters.Use2DCollision)
                        {
                            triangulateHoles = false;
                        }

                        List<ExploderMesh> meshes = null;
                        cutter.Cut(mesh.mesh, mesh.transform, plane, triangulateHoles, core.parameters.DisableTriangulation, ref meshes, crossSectionVertexColour, crossSectionUV);
                        cutCounter++;

                        cutting = true;

                        if (meshes != null)
                        {
                            foreach (var cutterMesh in meshes)
                            {
                                newFragments.Add(new MeshObject
                                {
                                    mesh = cutterMesh,

                                    material = mesh.material,
                                    transform = mesh.transform,
                                    id = mesh.id,
                                    original = mesh.original,
                                    skinnedOriginal = mesh.skinnedOriginal,

                                    parent = mesh.transform.parent,
                                    position = mesh.transform.position,
                                    rotation = mesh.transform.rotation,
                                    localScale = mesh.transform.localScale,

                                    option = mesh.option,
                                });
                            }

                            meshToRemove.Add(mesh);

                            if (THREAD_MAX > 1 && !WorkersWorking())
                            {
                                foreach (var worker in workers)
                                {
                                    if (!worker.Started)
                                    {
                                        var frags = newFragments.ToList();
                                        newFragments.Remove(frags[0]);
                                        worker.Run(frags[0]);
                                        break;
                                    }
                                }
                            }

                            core.targetFragments[mesh.id] -= 1;

                            // computation took more than settings.FrameBudget ... 
                            if (Watch.ElapsedMilliseconds > frameBudget && cycleCounter > THREAD_MAX)
                            {
                                timeBudgetStop = true;
                                break;
                            }
                        }
                    }
                }

                core.meshSet.ExceptWith(meshToRemove);
                core.meshSet.UnionWith(newFragments);

                if (timeBudgetStop)
                {
                    break;
                }
            }

            // explosion is finished
            return !timeBudgetStop;
        }

        private bool WorkersWorking()
        {
            if (workersWorkingCache)
            {
                return true;
            }

            foreach (var worker in workers)
            {
                if (!worker.Started)
                {
                    return false;
                }
            }

            workersWorkingCache = true;
            return true;
        }
    }
}
