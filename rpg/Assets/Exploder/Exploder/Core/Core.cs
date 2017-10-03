// Version 1.5
// ©2016 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

//#define DBG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Exploder
{
    class Core : Singleton<Core>
    {
        public void Initialize(ExploderObject exploder)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            parameters = new ExploderParams(exploder);

            // init pool
            FragmentPool.Instance.Allocate(parameters.FragmentPoolSize, parameters.MeshColliders, parameters.Use2DCollision, parameters.FragmentPrefab);
            FragmentPool.Instance.SetDeactivateOptions(parameters.DeactivateOptions, parameters.FadeoutOptions, parameters.DeactivateTimeout);
            FragmentPool.Instance.SetExplodableFragments(parameters.ExplodeFragments, parameters.DontUseTag);
            FragmentPool.Instance.SetFragmentPhysicsOptions(parameters.FragmentOptions, parameters.Use2DCollision);
            FragmentPool.Instance.SetSFXOptions(parameters.SFXOptions);
            frameWatch = new Stopwatch();
            explosionWatch = new Stopwatch();

            //
            // init queue
            //
            queue = new ExploderQueue(this);

            //
            // init tasks
            //
            tasks = new ExploderTask[(int) TaskType.TaskMax];

            tasks[(int)TaskType.Preprocess] = new Preprocess(this);
            tasks[(int)TaskType.ProcessCutter] = new Cutter(this);
            tasks[(int)TaskType.IsolateMeshIslands] = new IsolateMeshIslands(this);
            tasks[(int)TaskType.PostprocessExplode] = new PostprocessExplode(this);
            tasks[(int)TaskType.PostprocessCrack] = new PostprocessCrack(this);
            tasks[(int)TaskType.CrackExplode] = new CrackExplode(this);
            tasks[(int)TaskType.PartialSeparator] = new PartialSeparator(this);

            PreAllocateBuffers();

            if (parameters.SFXOptions.ExplosionSoundClip)
            {
                audioSource = gameObject.GetComponent<AudioSource>();

                if (!audioSource)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }

        public void Enqueue(ExploderObject exploderObject, ExploderObject.OnExplosion callback, GameObject obj)
        {
            queue.Enqueue(exploderObject, callback, obj);
        }

        public void Enqueue(ExploderObject exploderObject, ExploderObject.OnExplosion callback, GameObject obj, Vector3 shotDir, Vector3 hitPosition, float bulletSize)
        {
            queue.EnqueuePartialExplosion(exploderObject, callback, obj, shotDir, hitPosition, bulletSize);
        }

        public void ExplodeCracked(ExploderObject.OnExplosion callback)
        {
            ExploderUtils.Assert(cracked, "Object is not cracked!");

            InitTask(TaskType.CrackExplode);
            RunTask(TaskType.CrackExplode);
            callback(tasks[(int)TaskType.CrackExplode].Watch.ElapsedMilliseconds, ExploderObject.ExplosionState.ExplosionFinished);
        }

        public void StartExplosionFromQueue(ExploderParams p)
        {
            ExploderUtils.Assert(currTaskType == TaskType.None, "Wrong task: " + currTaskType);
            this.parameters = p;
            processingFrames = 1;
            explosionWatch.Reset();
            explosionWatch.Start();

            //
            // do preprocess right away
            //
            currTaskType = TaskType.Preprocess;
            InitTask(currTaskType);
            RunTask(currTaskType);
            currTaskType = NextTask(currTaskType);
            InitTask(currTaskType);
//            RunTask(currTaskType, parameters.FrameBudget);
        }

        public void Update()
        {
            frameWatch.Reset();
            frameWatch.Start();

            if (currTaskType != TaskType.None)
            {
                while (frameWatch.ElapsedMilliseconds < parameters.FrameBudget)
                {
                    if (RunTask(currTaskType, parameters.FrameBudget))
                    {
                        currTaskType = NextTask(currTaskType);

                        if (currTaskType == TaskType.None)
                        {
                            explosionWatch.Stop();
                            queue.OnExplosionFinished(parameters.id, explosionWatch.ElapsedMilliseconds);
                            return;
                        }

                        InitTask(currTaskType);
                    }
                }

                processingFrames++;
            }
        }

#if DBG
        public void OnGUI()
        {
            GUI.Label(new Rect(10, 50, 300, 30), "Explosion time: " + explosionWatch.ElapsedMilliseconds + " [ms]");
            GUI.Label(new Rect(10, 80, 500, 30), "Processing frames: " + processingFrames);

            var y = 100;
            GUI.Label(new Rect(10, y += 20, 500, 30), string.Format("{0}: {1}[ms]: ", TaskType.Preprocess, tasks[(int)TaskType.Preprocess].Watch.ElapsedMilliseconds));
            GUI.Label(new Rect(10, y += 20, 500, 30), string.Format("{0}: {1}[ms]: ", TaskType.PartialSeparator, tasks[(int)TaskType.PartialSeparator].Watch.ElapsedMilliseconds));
            GUI.Label(new Rect(10, y += 20, 500, 30), string.Format("{0}: {1}[ms]: ", TaskType.ProcessCutter, tasks[(int)TaskType.ProcessCutter].Watch.ElapsedMilliseconds));
            GUI.Label(new Rect(10, y += 20, 500, 30), string.Format("{0}: {1}[ms]: ", TaskType.PostprocessExplode, tasks[(int)TaskType.PostprocessExplode].Watch.ElapsedMilliseconds));
            GUI.Label(new Rect(10, y += 20, 500, 30), string.Format("{0}: {1}[ms]: ", TaskType.IsolateMeshIslands, tasks[(int)TaskType.IsolateMeshIslands].Watch.ElapsedMilliseconds));
        }
#endif

        public override void OnDestroy()
        {
            base.OnDestroy();

            foreach (var task in tasks)
            {
                if (task != null)
                {
                    task.OnDestroy();
                }
            }
        }

        private void PreAllocateBuffers()
        {
            // kick memory allocator for better performance at startup
            meshSet = new HashSet<MeshObject>();

            for (int i = 0; i < 64; i++)
            {
                meshSet.Add(new MeshObject());
            }

            RunTask(TaskType.Preprocess);
            RunTask(TaskType.ProcessCutter);
        }

        [NonSerialized] public ExploderParams parameters;
        [NonSerialized] public ExploderQueue queue;
        [NonSerialized] public Stopwatch explosionWatch;
        [NonSerialized] public Stopwatch frameWatch;
        [NonSerialized] public HashSet<MeshObject> meshSet;
        [NonSerialized] public int[] targetFragments;
        [NonSerialized] public int poolIdx;
        [NonSerialized] public List<MeshObject> postList;
        [NonSerialized] public List<Fragment> pool;
        [NonSerialized] public Vector3 crackedPos;
        [NonSerialized] public Quaternion crackedRot;
        [NonSerialized] public bool splitMeshIslands;
        [NonSerialized] public bool crack;
        [NonSerialized] public bool cracked;
        [NonSerialized] public AudioSource audioSource;
        [NonSerialized] public int processingFrames;

        private ExploderTask[] tasks;
        private TaskType currTaskType;
        private bool initialized = false;

        private bool RunTask(TaskType taskType, float budget = 0.0f)
        {
            return tasks[(int) taskType].Run(budget);
        }

        private void InitTask(TaskType taskType)
        {
            tasks[(int)taskType].Init();
        }

        private TaskType NextTask(TaskType taskType)
        {
            switch (taskType)
            {
                case TaskType.Preprocess:
                    if (parameters.PartialExplosion)
                        return TaskType.PartialSeparator;
                    return TaskType.ProcessCutter;

                case TaskType.PartialSeparator:
                    return TaskType.ProcessCutter;

                case TaskType.ProcessCutter:
                {
                    if (splitMeshIslands)
                    {
                        return TaskType.IsolateMeshIslands;
                    }
                    if (crack)
                    {
                        return TaskType.PostprocessCrack;
                    }

                    return TaskType.PostprocessExplode;
                }

                case TaskType.IsolateMeshIslands:
                {
                    if (crack)
                    {
                        return TaskType.PostprocessCrack;
                    }

                    return TaskType.PostprocessExplode;
                }

                case TaskType.PostprocessExplode:
                    return TaskType.None;

                case TaskType.PostprocessCrack:
                    return TaskType.None;

                case TaskType.CrackExplode:
                    return TaskType.None;

                default:
                    ExploderUtils.Assert(false, "Invalid task type!");
                    break;
            }

            ExploderUtils.Assert(false, "Invalid task type!");
            return TaskType.None;
        }
    }
}
