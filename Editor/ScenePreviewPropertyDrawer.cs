using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.UIElements;
using Unity.VisualScripting;

namespace xyz.zwks.procgen.editor {

[CustomPropertyDrawer(typeof(ScenePreview))]
public class ScenePreviewPropertyDrawer : PropertyDrawer {
    
    ScenePreview previewGenerator;
    VisualElement root;

    VisualElement previewRoot;

    ObjectField sceneObjField;

    [SerializeField]
    GameObject previewObject;
    [SerializeField]
    MeshFilter previewMF;
    [SerializeField]
    MeshRenderer previewMR;

    Mesh mesh;
    Texture2D texture;
    Material mat;

    bool uiInitialized = false;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        
        previewGenerator = property.GetUnderlyingValue() as ScenePreview;

        root = new VisualElement();
        root.name = "zw-sppd-root";
        
        previewRoot = new VisualElement();
        previewRoot.AddToClassList("zw-node-propdrawer");
        root.Add(previewRoot);
        
        var label = new Label("Scene Preview");
        previewRoot.Add(label);

        Toggle showPreview = new Toggle("Enable scene preview");
        showPreview.RegisterCallback<ChangeEvent<bool>>(scenePreviewToggle);
        showPreview.BindProperty(property.FindPropertyRelative("EnableInScenePreview"));
        previewRoot.Add(showPreview);

        Vector3Field spawnAtField = new Vector3Field("Spawn location");
        spawnAtField.BindProperty(property.FindPropertyRelative("spawnAt"));

        ObjectField prefab = new ObjectField("Prefab");
        prefab.name = "zw-sppd-prefabfield";
        prefab.BindProperty(property.FindPropertyRelative("prefab"));

        previewRoot.Add(prefab);

        sceneObjField = new ObjectField("Preview");
        sceneObjField.name = "zw-sppd-sceneobjectfield";
        sceneObjField.BindProperty(property.FindPropertyRelative("scenePreviewObject"));
        sceneObjField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(previewObjectChanged);
        sceneObjField.allowSceneObjects = true;
        sceneObjField.objectType = typeof(ProcGenPreview);

        previewRoot.Add(sceneObjField);

        previewGenerator.Pipeline.PipelineDataPublish -= PipelineHasNewData;
        previewGenerator.Pipeline.PipelineDataPublish += PipelineHasNewData;
        
        root.RegisterCallback<AttachToPanelEvent>(attached);

        return root;
    }

    void previewObjectChanged(ChangeEvent<UnityEngine.Object> evt) {
        if(previewGenerator != null) {
            if(evt.newValue == null) {
                //Debug.Log("Object field cleared");
                previewObject = null;
            } else if(evt.newValue is ProcGenPreview) {
                //Debug.Log("Object field got GameObject");
                previewObject = previewGenerator.scenePreviewObject;
                
                string label = "ProcGenPreview-d";
                if(previewObject != null) 
                    label = previewObject.name;
                else if(previewGenerator.scenePreviewObject != null)
                    label = previewGenerator.scenePreviewObject.name;
                else if(evt.newValue != null)
                    label = evt.newValue.name;

                var labelVE = sceneObjField.Query(className: "unity-object-field-display").Children<Label>().First();
                if(labelVE != null && previewObject != null) {
                    ((Label)labelVE).text = label;
                }
            }

            checkRegeneratePreviewObject();
            refreshScenePreview();
            updatePreview();
        }
    }

    void scenePreviewToggle(ChangeEvent<bool> evt) {
        if(evt.newValue) {
            checkRegeneratePreviewObject();
            refreshScenePreview();
            updatePreview();
        } else {
            previewGenerator.scenePreviewObject = null;
            previewObject = null;
        }
    }

    void attached(AttachToPanelEvent evt) {
        uiInitialized = true;
        checkRegeneratePreviewObject();
        refreshScenePreview();
        updatePreview();
    }

    void refreshScenePreview() {
        if(!previewGenerator.EnableInScenePreview)
            return;

        var tmpObj = previewGenerator.refreshSceneObject();
        if(tmpObj != previewObject) {
            previewObject = tmpObj;
        }
        if(previewObject != null && previewMF == null) 
            previewMF = previewObject.GetComponent<MeshFilter>();
        if(previewObject != null && previewMR == null)
            previewMR = previewObject.GetComponent<MeshRenderer>();
    }

    void checkRegeneratePreviewObject() {
        if( previewGenerator.EnableInScenePreview 
            && previewGenerator.scenePreviewObject == null
            && mesh != null)
            previewGenerator.Generate();
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        if(node == null)
            return;

        if(node == previewGenerator) {
            var sp = (ScenePreview)node;
            previewObject = sp.scenePreviewObject;
        }
        if(node is MeshGenerator) {
            var mg = (MeshGenerator)node;
            mesh = mg.GetMeshOutput();
        }
        if(node is TerrainMapGenerator) {
            var tg = (TerrainMapGenerator)node;
            texture = tg.GetTextureOutput();
        }
        
        if(node != previewGenerator)
            checkRegeneratePreviewObject();

        refreshScenePreview();    
        
        updatePreview();
    }

    void updatePreview() {
        if(!uiInitialized)
            return;

        if(!previewGenerator.EnableInScenePreview)
            return;

        if(previewObject == null)
            return;
        if(previewMF == null)
            return;
        if(previewMR == null)
            return;

        if(mesh == null)
            return;

        previewMF.sharedMesh = mesh;
        
        if(texture != null && previewMR != null) {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if(shader == null)
                shader = Shader.Find("Lit");
            if(shader == null)
                shader = Shader.Find("Standard");

            if(mat == null)
                mat = new Material(shader);
            
            mat.mainTexture = texture;
            previewMR.sharedMaterial = mat;
        }
    }
}
}
