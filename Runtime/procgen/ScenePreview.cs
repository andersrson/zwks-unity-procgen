
using System;
using UnityEditor;
using UnityEngine;

namespace xyz.zwks.procgen {

[Serializable]
public class ScenePreview : IPipelineNode, IDisposable {

    public bool EnableInScenePreview;
    public GameObject prefab;
    public Vector3 spawnAt = Vector3.zero;

    public GameObject scenePreviewObject;

    public GameObject refreshSceneObject() {
        ProcGenPreview prevObj = null;
        if (scenePreviewObject == null) {
            prevObj = GameObject.FindAnyObjectByType<ProcGenPreview>();
            if(prevObj != null) {
                scenePreviewObject = prevObj.gameObject;
            }
        }
        return scenePreviewObject;
    }

    public void Generate() {

        ProcGenPreview prevObj = null;
        GameObject tmpGo = null;
        if (scenePreviewObject == null) {
            prevObj = GameObject.FindAnyObjectByType<ProcGenPreview>();
        }
        if(prevObj == null) {
            if(prefab != null) {
                var tmpObj = PrefabUtility.InstantiatePrefab(prefab);
                tmpGo = (GameObject)tmpObj;
                tmpGo.transform.position = spawnAt;
                tmpGo.transform.rotation = Quaternion.identity;
                tmpGo.name = "Procedural preview";
                
                if(tmpGo.GetComponentInChildren<ProcGenPreview>() == null)
                    tmpGo.AddComponent<ProcGenPreview>();
                if(tmpGo.GetComponentInChildren<MeshFilter>() == null)
                    tmpGo.AddComponent<MeshFilter>();
                if(tmpGo.GetComponentInChildren<MeshRenderer>() == null)
                    tmpGo.AddComponent<MeshRenderer>();

            } else {
                tmpGo = new GameObject("ProcGenPreview", typeof(ProcGenPreview), typeof(MeshFilter), typeof(MeshRenderer));
            }
            prevObj = GameObject.FindAnyObjectByType<ProcGenPreview>();
        }
        if(prevObj != null && prevObj.gameObject != scenePreviewObject) {
            scenePreviewObject = prevObj.gameObject;
            _pipeline.NodeHasOutput(this);
        }
    }

    IProceduralPipeline _pipeline;

    public IProceduralPipeline Pipeline { get {return _pipeline; } }
    public void SetPipeline(IProceduralPipeline pipeline) {
        _pipeline = pipeline;
        //_pipeline.PipelineDataPublish -= PipelineHasNewData;
        //_pipeline.PipelineDataPublish += PipelineHasNewData;
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        if(node == this)
            return;
        if(node is ScenePreview)
            return;
        if(node is NoiseGenerator)
            return;
        
        if(!EnableInScenePreview)
            return;

            
    }

#region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) { }
            
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~NoiseGenerator()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
#endregion
}

}