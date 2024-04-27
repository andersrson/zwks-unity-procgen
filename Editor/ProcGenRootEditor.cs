using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.UIElements;

namespace xyz.zwks.procgen.editor {

[CustomEditor(typeof(ProceduralGenerator))]
public class ProcGenRootEditor : Editor {

    public VisualTreeAsset uxmlNoiseGen;
    public VisualTreeAsset uxmlTerrainMapGen;


    VisualElement root;
    VisualElement noiseGenRoot;

    VisualElement noiseGenPropertyFieldRoot;

    VisualElement terrainMapRoot;

    VisualElement terrainMapPropertyFieldRoot;

    VisualElement meshGenRoot;
    VisualElement meshGenPropertyFieldRoot;

    VisualElement materialRoot;
    VisualElement materialPropertyFieldRoot;
    
    VisualElement previewRoot;
    VisualElement previewPropertyFieldRoot;

    ProceduralGenerator tgt;
    
    public override VisualElement CreateInspectorGUI() {

        tgt = serializedObject.targetObject as ProceduralGenerator;

        if(tgt.GetNode<NoiseGenerator>() == null) {
            tgt.AddNode(new NoiseGenerator());
            EditorUtility.SetDirty(tgt);
        }

        if(tgt.GetNode<TerrainMapGenerator>() == null) {
            tgt.AddNode(new TerrainMapGenerator());
            EditorUtility.SetDirty(tgt);
        }

        if(tgt.GetNode<MeshGenerator>() == null) {
            tgt.AddNode(new MeshGenerator());
            EditorUtility.SetDirty(tgt);
        }

        if(tgt.GetNode<MaterialGenerator>() == null) {
            tgt.AddNode(new MaterialGenerator());
            EditorUtility.SetDirty(tgt);
        }

        if(tgt.GetNode<ScenePreview>() == null) {
            tgt.AddNode(new ScenePreview());
            EditorUtility.SetDirty(tgt);
        }

        root = new VisualElement();
        noiseGenRoot = new VisualElement();
        terrainMapRoot = new VisualElement();
        meshGenRoot = new VisualElement();
        materialRoot = new VisualElement();
        previewRoot = new VisualElement();

        var horizCont = new VisualElement();
        horizCont.AddToClassList("zw-horiz-container");

        root.Add(horizCont);

        var leftVert = new VisualElement();
        leftVert.AddToClassList("zw-vertic-container");
        var midVert = new VisualElement();
        midVert.AddToClassList("zw-vertic-container");
        var rightVert = new VisualElement();
        rightVert.AddToClassList("zw-vertic-container");

        horizCont.Add(leftVert);
        horizCont.Add(midVert);
        horizCont.Add(rightVert);

        leftVert.Add(noiseGenRoot);
        midVert.Add(terrainMapRoot);
        rightVert.Add(meshGenRoot);
        rightVert.Add(materialRoot);
        rightVert.Add(previewRoot);

        noiseGenRoot.name = "zw-pged-ng-root-container";
        noiseGenRoot.AddToClassList("zw-border-container");
        noiseGenRoot.AddToClassList("zw-pged-root-container");
        
        noiseGenPropertyFieldRoot = new VisualElement();
        noiseGenRoot.Add(noiseGenPropertyFieldRoot);

        PropertyField npf = new PropertyField();
        npf.bindingPath = "NoiseGen";
        noiseGenPropertyFieldRoot.Add(npf);

        terrainMapRoot.name = "zw-pged-tg-root-container";
        terrainMapRoot.AddToClassList("zw-border-container");
        terrainMapRoot.AddToClassList("zw-pged-root-container");
        
        terrainMapPropertyFieldRoot = new VisualElement();
        terrainMapRoot.Add(terrainMapPropertyFieldRoot);

        PropertyField tpf = new PropertyField();
        tpf.bindingPath = "TerrainMapGen";
        terrainMapPropertyFieldRoot.Add(tpf);
        
        meshGenRoot.name = "zw-pged-mg-root-container";
        meshGenRoot.AddToClassList("zw-border-container");
        meshGenRoot.AddToClassList("zw-pged-root-container");
        
        meshGenPropertyFieldRoot = new VisualElement();
        meshGenRoot.Add(meshGenPropertyFieldRoot);

        PropertyField meshf = new PropertyField();
        meshf.bindingPath = "MeshGen";
        meshGenPropertyFieldRoot.Add(meshf);

        materialRoot.name = "zw-pged-matg-root-container";
        materialRoot.AddToClassList("zw-border-container");
        materialRoot.AddToClassList("zw-pged-root-container");

        materialPropertyFieldRoot = new VisualElement();
        materialRoot.Add(materialPropertyFieldRoot);

        PropertyField matf = new PropertyField();
        matf.bindingPath = "MatGen";
        materialPropertyFieldRoot.Add(matf);

        previewRoot.name = "zw-pged-sp-root-container";
        previewRoot.AddToClassList("zw-border-container");
        previewRoot.AddToClassList("zw-pged-root-container");
        
        previewPropertyFieldRoot = new VisualElement();
        previewRoot.Add(previewPropertyFieldRoot);

        PropertyField preview = new PropertyField();
        preview.bindingPath = "ScenePreview";
        previewPropertyFieldRoot.Add(preview);

        var so = new SerializedObject(tgt);
        root.Bind(so);
        return root;
    }

}
}