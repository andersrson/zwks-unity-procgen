using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.UIElements;
using Unity.VisualScripting;
using System;

namespace xyz.zwks.procgen.editor {

[CustomPropertyDrawer(typeof(MeshGenerator))]
public class MeshGeneratorPropertyDrawer : PropertyDrawer {

    MeshGenerator meshGen;
    
    VisualElement root;

    VisualElement meshRoot;

    Image img;
    Mesh mesh = null;
    MeshPreview mp;
    Foldout preview;

    [SerializeField]
    GameObject previewObject;
    [SerializeField]
    MeshFilter previewMF;
    [SerializeField]
    MeshRenderer previewMR;

    bool uiInitialized = false;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        
        meshGen = property.GetUnderlyingValue() as MeshGenerator;

        root = new VisualElement();
        root.name = "zw-mgpd-root";
        
        meshRoot = new VisualElement();
        meshRoot.AddToClassList("zw-node-propdrawer");
        root.Add(meshRoot);
        
        var label = new Label("Mesh Generator");
        meshRoot.Add(label);

        var heightInfluence = new UnityEngine.UIElements.FloatField("Height influence");
        heightInfluence.BindProperty(property.FindPropertyRelative("HeightMapInfluence"));
        heightInfluence.RegisterCallback<ChangeEvent<float>>(heightInfluenceUpdated);

        meshRoot.Add(heightInfluence);

        UnityEngine.UIElements.Button save = new UnityEngine.UIElements.Button(buttonClicked);
        save.text = "Save mesh";
        meshRoot.Add(save);

        meshGen.Pipeline.PipelineDataPublish -= PipelineHasNewData;
        meshGen.Pipeline.PipelineDataPublish += PipelineHasNewData;
        
        root.RegisterCallback<AttachToPanelEvent>(attached);

        return root;
    }

    void previewObjectSelected(ChangeEvent<UnityEngine.Object> evt) {
        Debug.Log("preview object selected");
        
        try {
            previewObject = (GameObject) evt.newValue;
        } catch (Exception) {
            Debug.LogError("Object is not a gameobject");
            return;
        }

        previewObject.TryGetComponent<MeshFilter>(out previewMF);
        previewObject.TryGetComponent<MeshRenderer>(out previewMR);

        if(previewMF == null)
            previewMF = previewObject.AddComponent<MeshFilter>();
        if(previewMR == null)
            previewMR = previewObject.AddComponent<MeshRenderer>();
    }

    void heightInfluenceUpdated(ChangeEvent<float> evt) {
        if(meshGen != null) {
            meshGen.Invalidate();
            meshGen.Generate();
        }
    }

    void buttonClicked() {
        if(mesh == null)
            return;

        if(!AssetDatabase.IsValidFolder("Assets/data"))
            AssetDatabase.CreateFolder("Assets", "data");

        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/data/mesh.asset");
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
    }

    void attached(AttachToPanelEvent evt) {
        uiInitialized = true;
        updatePreview();
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        if(node != meshGen) 
            return;
        
        updatePreview();
    }

    void updatePreview() {
        if(!uiInitialized)
            return;

        mesh = meshGen.GetMeshOutput();
        if(mesh != null && previewMF != null && previewMR != null) {
            previewMF.sharedMesh = mesh;
        }
    }
}
}
