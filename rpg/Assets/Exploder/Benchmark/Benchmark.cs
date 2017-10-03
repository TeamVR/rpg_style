#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX

using System.Collections.Generic;
using Exploder;
using Exploder.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Benchmark : MonoBehaviour
{
    public GameObject testObjects;
    private MeshRenderer[] objects;
    private int index = 0;
    private int rounds = 5;
    private int batchIndex = 0;
    private int[] batches = {30, 60, 100};

    class Report
    {
        public string name;
        public float ms;
        public int frames;
        public int count;
    }

    private readonly Dictionary<string, Report> report = new Dictionary<string,Report>();

    void AddReport(Report r)
    {
//        Debug.LogFormat("{0}: {1}[ms] {2}[frames]", r.name, r.ms, r.frames);

        if (report.ContainsKey(r.name))
        {
            report[r.name].ms += r.ms;
            report[r.name].count++;
            report[r.name].frames += r.frames;
        }
        else
        {
            report.Add(r.name, r);
        }
    }

    void PrintReport()
    {
        Debug.Log("PrintReportTotal");
        var content = "Report:\n\n";

        foreach (var r in report.Values)
        {
            var line = string.Format("{0}: {1}[ms] {2}[frames]", r.name, r.ms/r.count, r.frames/r.count);
//            Debug.LogFormat(line);
            content += line + "\n";
        }

//        System.IO.File.WriteAllText("c:\\Dev\\Exploder\\exploder-git\\benchmark.txt", content);
        System.IO.File.WriteAllText("c:\\Development\\Unity\\AssetStore\\exploder-git\\benchmark.txt", content);
    }

    void Start()
    {
#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
#endif

	    objects = testObjects.GetComponentsInChildren<MeshRenderer>(true);
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 50), "Start"))
        {
            ExplodeObject();
        }
    }

    void ExplodeObject()
    {
        if (rounds == 0)
        {
            batchIndex ++;
            rounds = 5;

            if (batchIndex >= batches.Length)
            {
                PrintReport();
                return;
            }
        }

        var targetFragments = batches[batchIndex];
        ExploderSingleton.ExploderInstance.TargetFragments = targetFragments;

        if (index >= objects.Length)
        {
            FragmentPool.Instance.DeactivateFragments();
            rounds --;
            index = 0;
        }

        objects[index].gameObject.SetActive(true);
        ExploderSingleton.ExploderInstance.ExplodeObject(objects[index].gameObject, (ms, state) =>
        {
            if (state == ExploderObject.ExplosionState.ExplosionFinished)
            {
                var frames = ExploderSingleton.ExploderInstance.ProcessingFrames;
                AddReport(new Report {name = objects[index].gameObject.name+"["+targetFragments+"]", ms = ms, frames = frames, count = 1});
                index++;
                ExplodeObject();
            }
        });
    }
}
#endif
