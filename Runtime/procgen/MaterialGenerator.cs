
using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.WorkspaceServer.DataStore.WkTree;
using Unity.Collections;
using UnityEngine;
using UnityEngine.TerrainUtils;

namespace xyz.zwks.procgen {

[Serializable]
public class MaterialGenerator : IPipelineNode, IDisposable {
    
    public Material outputMaterial;
    public string SelectedShader;

    public bool UseTerrainMap;

    public List<Shader> loadedShaders;
    List<string> shaderNames;

    public Shader shader;
    Shader lastUsedShader;
    
    bool invalidated = true;

    float maxHeight = 0f;

    // Shader property IDs
    int maxHeightID = -1;
    int minHeightID = -1;
    int levelCountID = -1;
    
    int[] colIDs = new int[8];
    int[] heightIDs = new int[7];

    MeshGenerator meshGen;
    TerrainMapGenerator tmGen;

    public void Invalidate() {
        invalidated = true;
    }

    public List<Shader> LoadShaders() {
        if(shaderNames == null) {
            shaderNames = new List<string>() {
                "Custom/zwks-terrain-shader", 
                "Universal Render Pipeline/Baked Lit",
                "Universal Render Pipeline/Complex Lit",
                "Universal Render Pipeline/Lit",
                "Universal Render Pipeline/Simple Lit",
                "Universal Render Pipeline/Unlit", 
                "Lit", 
                "Unlit/Color", 
                "Unlit/Texture", 
                "Unlit/Transparent", 
                "Standard"
            };
        }

        if(loadedShaders == null)
            loadedShaders = new List<Shader>();

        foreach(var shaderName in shaderNames) {
            var shader = Shader.Find(shaderName);
            if(shader != null && !loadedShaders.Contains(shader))
                loadedShaders.Add(shader);
        }
        return loadedShaders;
    }

    public Shader AddShader(string shaderName) {
        if(loadedShaders == null)
            LoadShaders();
        
        var sh = Shader.Find(shaderName);
        if(sh != null) {
            if(!shaderNames.Contains(shaderName))
                shaderNames.Add(shaderName);
            if(!loadedShaders.Contains(sh))
                loadedShaders.Add(sh);

            return sh;
        }
        return null;
    }

    public Shader GetLoadedShader(string shaderName) {
        if(loadedShaders == null)
            return null;
        
        if(shaderNames == null)
            return null;

        if(!shaderNames.Contains(shaderName))
            return null;

        return loadedShaders.Where((s) => {return s.name == shaderName;}).FirstOrDefault();
    }

    public bool IsShaderLoaded(string shaderName) {
        if(string.IsNullOrWhiteSpace(shaderName))
            return false;
        if(loadedShaders == null)
            return false;
        if(shaderNames == null)
            return false;
        
        return shaderNames.Contains(shaderName);
    }

    /**
        Finds shader by name and sets it on the material. Will try to load the shader if not already loaded.
        Returns true and sets Invalidated = true if change was made. 
        Returns false if:
            shaderName is null or whitespace
            shaderName is the name of the current shader
            no shader named shaderName can be loaded
    */
    public bool SetShader(string shaderName) {

        if(string.IsNullOrWhiteSpace(shaderName))
            return false;
        if(shaderName == SelectedShader)
            return false;

        Shader sh = null;
        if(!IsShaderLoaded(shaderName))
            sh = AddShader(shaderName);
        else 
            sh = GetLoadedShader(shaderName);

        if(sh == null)
            return false;

        shader = sh;
        SelectedShader = shaderName;

        invalidated = true;
        return true;
    }

    public void Generate() {

        if(!(invalidated || outputMaterial == null))
            return;

        if(lastUsedShader == null || shader != lastUsedShader) {
            LoadShaders();
            if(shader == null) {
                if(string.IsNullOrWhiteSpace(SelectedShader)) 
                    shader = loadedShaders.FirstOrDefault();
                else {
                    if(IsShaderLoaded(SelectedShader))
                        shader = GetLoadedShader(SelectedShader);
                     else
                        shader = AddShader(SelectedShader);
                }
            }
            
        }
        if(outputMaterial == null) {
            outputMaterial = new Material(shader);
        } else if(shader != lastUsedShader) {
            outputMaterial.shader = shader;
        }
            
        lastUsedShader = shader;
        invalidated = false;
    }

    public Material GetMaterialOutput() {
        return outputMaterial;
    }

    IProceduralPipeline _pipeline;

    public IProceduralPipeline Pipeline { get {return _pipeline; } }
    public void SetPipeline(IProceduralPipeline pipeline) {
        _pipeline = pipeline;
        _pipeline.PipelineDataPublish -= PipelineHasNewData;
        _pipeline.PipelineDataPublish += PipelineHasNewData;
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        if(node == this)
            return;

        if(node is MeshGenerator) {
            meshGen = (MeshGenerator)node;
            if(meshGen.HeightMapInfluence != maxHeight) {
                maxHeight = meshGen.HeightMapInfluence;
                invalidated = true;
            }
        } else if(node is TerrainMapGenerator) {
            tmGen = (TerrainMapGenerator)node;
            invalidated = true;
        } else 
            return;

        if(tmGen == null || tmGen.terrainTypes == null || tmGen.terrainTypes.Count == 0)
            return;

        UpdateMaterial();
    }

    void UpdateMaterial() {
        if(outputMaterial == null || invalidated) {
            Generate();

            if(meshGen != null)
                UpdateMaterialProperties(meshGen);
            if(tmGen != null)
                UpdateMaterialProperties(tmGen);

            _pipeline.NodeHasOutput(this);
        }
    }

    void UpdateMaterialProperties(MeshGenerator mg) {
        maxHeight = mg.HeightMapInfluence;

        if(maxHeightID < 1) {
            maxHeightID = Shader.PropertyToID("_MaxHeight");
            minHeightID = Shader.PropertyToID("_MinHeight");
        }

        outputMaterial.SetFloat(maxHeightID, maxHeight);
        outputMaterial.SetFloat(minHeightID, 0f);
        outputMaterial.SetInt(levelCountID, tmGen.terrainTypes.Count);
    }

    void UpdateMaterialProperties(TerrainMapGenerator tmg) {
        if(SelectedShader == "Custom/zwks-terrain-shader") {
            if(levelCountID < 1) {
                maxHeightID = Shader.PropertyToID("_MaxHeight");
                minHeightID = Shader.PropertyToID("_MinHeight");
                levelCountID = Shader.PropertyToID("_LevelsUsed");
                colIDs[0] = Shader.PropertyToID("_ColorBase");
                colIDs[1] = Shader.PropertyToID("_Color1");
                colIDs[2] = Shader.PropertyToID("_Color2");
                colIDs[3] = Shader.PropertyToID("_Color3");
                colIDs[4] = Shader.PropertyToID("_Color4");
                colIDs[5] = Shader.PropertyToID("_Color5");
                colIDs[6] = Shader.PropertyToID("_Color6");
                colIDs[7] = Shader.PropertyToID("_Color7");
                heightIDs[0] = Shader.PropertyToID("_Height1");
                heightIDs[1] = Shader.PropertyToID("_Height2");
                heightIDs[2] = Shader.PropertyToID("_Height3");
                heightIDs[3] = Shader.PropertyToID("_Height4");
                heightIDs[4] = Shader.PropertyToID("_Height5");
                heightIDs[5] = Shader.PropertyToID("_Height6");
                heightIDs[6] = Shader.PropertyToID("_Height7");    
            }
        
            outputMaterial.SetFloat(maxHeightID, maxHeight);
            outputMaterial.SetFloat(minHeightID, 0f);
            outputMaterial.SetInt(levelCountID, tmGen.terrainTypes.Count);

            for (int i = 0; i < tmGen.terrainTypes.Count && i < colIDs.Length; i++)
                outputMaterial.SetColor(colIDs[i], tmGen.terrainTypes[i].Color);

            for(int i = 0; i < tmGen.terrainTypes.Count && i < heightIDs.Length; i++)
                outputMaterial.SetFloat(heightIDs[i], tmGen.terrainTypes[i].MinHeight);            
        }
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