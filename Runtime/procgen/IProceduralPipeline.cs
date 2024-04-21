
using System;
using System.Collections.Generic;

namespace xyz.zwks.procgen {

public interface IPipelineNode {
    IProceduralPipeline Pipeline { get; }
    void SetPipeline(IProceduralPipeline pipeline);
}

public interface IProceduralPipeline {
    
    event System.EventHandler<IPipelineNode> PipelineDataPublish;

    //void AddNode<TPipelineNode>(TPipelineNode node) where TPipelineNode: IPipelineNode;

    void AddNode(IPipelineNode node);

    TPipelineNode GetNode<TPipelineNode>() where TPipelineNode: IPipelineNode;

    void NodeHasOutput(IPipelineNode node);

}

}