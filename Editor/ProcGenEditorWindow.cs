using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace xyz.zwks.procgen.editor {

[Serializable]
public class ProcGenEditorWindow : EditorWindow {

    internal static readonly string zwksAssetsRoot = "Assets/zwks";
    internal static readonly string zwksAssetsData = "Assets/zwks/data";
    internal static readonly string settingsAssetPath = "Assets/zwks/data/procgensettings.asset";
    internal static readonly string defaultMeshGenAssetPath = "Assets/zwks/data/noisegendefaultsettings.asset";

    [SerializeField]
    VisualTreeAsset windowRoot;
    
    [SerializeField]
    StyleSheet styles;

    ProceduralGenerator generator;
    
    ProcGenSharedSettings sharedSettings;

    [MenuItem("Tools/zwks Procedural Generator")]
    public static void ShowEditor() {
        ProcGenEditorWindow w = GetWindow<ProcGenEditorWindow>();
        w.titleContent = new UnityEngine.GUIContent("zwks Procedural Generator");
    }

    public void CreateGUI() {
        ProceduralGenerator rt = null;
        string latest = LoadMostRecentAssetPath();
        var needsSave = false;

        if(!string.IsNullOrWhiteSpace(latest) && !AssetDatabase.AssetPathExists(latest)) {
            latest = null;
        }

        if(string.IsNullOrWhiteSpace(latest)) {
            rt = ScriptableObject.CreateInstance<ProceduralGenerator>();
            rt.AddNode(new NoiseGenerator());
            rt.AddNode(new TerrainMapGenerator());
            rt.AddNode(new MeshGenerator());
            rt.AddNode(new ScenePreview());
            needsSave = true;
        } else {
            rt = AssetDatabase.LoadAssetAtPath<ProceduralGenerator>(latest);
        }
        
        if(rt.GetNode<NoiseGenerator>() == null) {
            rt.AddNode(new NoiseGenerator());
            needsSave = true;
        }

        if(rt.GetNode<TerrainMapGenerator>() == null) {    
            rt.AddNode(new TerrainMapGenerator());
            needsSave = true;
        }

        if(rt.GetNode<MeshGenerator>() == null) {    
            rt.AddNode(new MeshGenerator());
            needsSave = true;
        }

        if(rt.GetNode<ScenePreview>() == null) {    
            rt.AddNode(new ScenePreview());
            needsSave = true;
        }

        if(needsSave) {
            if(!AssetDatabase.Contains(rt)) {
                var savePath = EditorUtility.SaveFilePanelInProject("Save generator asset", "ProcGen", "asset", "Save new ProcGen asset");
                savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);

                if(!string.IsNullOrWhiteSpace(savePath)) {
                    AssetDatabase.CreateAsset(rt, savePath);
                    SaveMostRecentAssetPath(savePath);
                }

                EditorUtility.SetDirty(rt);
                AssetDatabase.SaveAssets();
            }
        }

        rootVisualElement.styleSheets.Add(styles);
        
        var objField = rootVisualElement.Q<ObjectField>();
        if(objField != null) {
            objField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(generatorObjectChanged);
        } else {
            VisualElement objFieldContainer = new VisualElement();
            objFieldContainer.name = "zw-procgen-object-field";
            rootVisualElement.Add(objFieldContainer);

            objField = new ObjectField("Generator asset");
            objField.bindingPath = "generator";
            objField.objectType = typeof(xyz.zwks.procgen.ProceduralGenerator);
            objFieldContainer.Add(objField);

            objField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(generatorObjectChanged);
        }

        var uxmlRoot = windowRoot.CloneTree();        
        rootVisualElement.Add(uxmlRoot);

        updateGenerator(rt);
    }

    ProcGenSharedSettings EnsureSettingsAssetExists() {
        if(AssetDatabase.AssetPathExists(settingsAssetPath)) {
            return AssetDatabase.LoadAssetAtPath<ProcGenSharedSettings>(settingsAssetPath);
        }

        if(!AssetDatabase.AssetPathExists(zwksAssetsRoot))
            AssetDatabase.CreateFolder("Assets", "zwks");
        if(!AssetDatabase.AssetPathExists(zwksAssetsData))
            AssetDatabase.CreateFolder(zwksAssetsRoot, "data");
        
        ProcGenSharedSettings st = ScriptableObject.CreateInstance<ProcGenSharedSettings>();
        AssetDatabase.CreateAsset(st, settingsAssetPath);
        AssetDatabase.SaveAssets();

        return st;
    }

    void SaveMostRecentAssetPath(string latest) {
        var st = EnsureSettingsAssetExists();
        st.lastOpenedAssetPath = latest;
        EditorUtility.SetDirty(st);
        AssetDatabase.SaveAssets();
    }

    string LoadMostRecentAssetPath() {
        ProcGenSharedSettings st = EnsureSettingsAssetExists();
        return st.lastOpenedAssetPath;
    }

    void updateGenerator(ProceduralGenerator newGenerator) {
        generator = newGenerator;

        var so = new SerializedObject(newGenerator);
        var inspector = rootVisualElement.Q<InspectorElement>();
        if(inspector != null) {
            inspector.Bind(so);
        } else {
            inspector = new InspectorElement(so);
            rootVisualElement.Add(inspector);        
        }

        var objField = rootVisualElement.Q<ObjectField>();
        if(objField != null) {
            objField.SetValueWithoutNotify(newGenerator);
        }
    }

    void generatorObjectChanged(ChangeEvent<UnityEngine.Object> evt) {
        if(evt.newValue == null || evt.newValue is not ProceduralGenerator) {
            updateGenerator(ScriptableObject.CreateInstance<ProceduralGenerator>());
            SaveMostRecentAssetPath(null);
            return;
        }

        ProceduralGenerator obj = (ProceduralGenerator)evt.newValue;
        var path = AssetDatabase.GetAssetPath(obj);
        SaveMostRecentAssetPath(path);
        
        updateGenerator(obj);
    }

}
}