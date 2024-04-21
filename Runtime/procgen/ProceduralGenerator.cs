
using System.Collections.Generic;
using UnityEngine;

namespace xyz.zwks.procgen {

[CreateAssetMenu(fileName = "ProcGen", menuName = "zwks/Procedural Generator")]
public class ProceduralGenerator : ScriptableObject, IProceduralPipeline, ISerializationCallbackReceiver {

    public event System.EventHandler<IPipelineNode> PipelineDataPublish;
    
    public NoiseGenerator NoiseGen;

    public TerrainMapGenerator TerrainMapGen;

    public MeshGenerator MeshGen;

    public ScenePreview ScenePreview;

    List<IPipelineNode> _nodes;

    public void AddNode(IPipelineNode node) {
        if(_nodes == null)
            _nodes = new List<IPipelineNode>();

        if(node is NoiseGenerator) {
            NoiseGen = node as NoiseGenerator;
        } 
        if(node is TerrainMapGenerator) {
            TerrainMapGen = node as TerrainMapGenerator;
        } 
        if(node is MeshGenerator) {
            MeshGen = node as MeshGenerator;
        } 
        if(node is ScenePreview) {
            ScenePreview = node as ScenePreview;
        }

        node.SetPipeline(this);

        _nodes.Add(node);
    }

    public TPipelineNode GetNode<TPipelineNode>() where TPipelineNode : IPipelineNode {
        if(_nodes == null)
            return default(TPipelineNode);

        foreach(var n in _nodes) {
            if(n.GetType() == typeof(TPipelineNode))
                return (TPipelineNode)n;
        }
        return default(TPipelineNode);
    }

    public void NodeHasOutput(IPipelineNode node) {
        PipelineDataPublish?.Invoke(this, node);
    }

    public void OnAfterDeserialize() {
        if(_nodes == null) {
            _nodes = new List<IPipelineNode>();
            if(NoiseGen != null)
                _nodes.Add(NoiseGen);
            if(TerrainMapGen != null)
                _nodes.Add(TerrainMapGen);
            if(MeshGen != null)
                _nodes.Add(MeshGen);
            if(ScenePreview != null)
                _nodes.Add(ScenePreview);
        }
        
        NoiseGen.SetPipeline(this);
        TerrainMapGen.SetPipeline(this);
        MeshGen.SetPipeline(this);
        ScenePreview.SetPipeline(this);
    }

    public void OnBeforeSerialize() {
        
    }
}

  
}