using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using Unity.VisualScripting;
using System.Linq;

namespace xyz.zwks.procgen.editor {

[CustomPropertyDrawer(typeof(MaterialGenerator))]
public class MaterialGeneratorPropertyDrawer : PropertyDrawer {

    [SerializeField]
    MaterialGenerator tgt;

    [SerializeField]
    string LastShaderName;
    
    [SerializeField]
    bool LastUseTerrainMap;

    VisualElement root;

    VisualElement materialRoot;

    Label matFileNameLabel;
    
    UnityEngine.UIElements.Button saveMat;

    bool uiInitialized = false;

    [SerializeField]
    Texture2D texture;
    [SerializeField]
    Material mat;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        tgt = property.GetUnderlyingValue() as MaterialGenerator;

        root = new VisualElement();
        root.name = "zw-tgpd-root";
        
        materialRoot = new VisualElement();
        materialRoot.AddToClassList("zw-node-propdrawer");
        root.Add(materialRoot);
        
        var label = new Label("Material generator");
        materialRoot.Add(label);
        
        var settingsContainer = new VisualElement();
        settingsContainer.name = "zw-mtpd-settings-container";
        settingsContainer.AddToClassList("zw-vertic-container");
        materialRoot.Add(settingsContainer);

        var configContainer = new VisualElement();
        configContainer.AddToClassList("zw-horiz-container");
        settingsContainer.Add(configContainer);

        Toggle useTerrainMap = new Toggle("Use terrain map as texture");
        useTerrainMap.RegisterCallback<ChangeEvent<bool>>(toggleUseTerrainMap);
        settingsContainer.Add(useTerrainMap);

        DropdownField shaderMenu = new DropdownField("Shader");

        // Binding to the property does not work, apparently.
        var shaders = tgt.LoadShaders();
        if(shaders.Count > 0) {
            foreach(var shader in shaders) {
                shaderMenu.choices.Add(shader.name);
            }
            if(shaders.Any((s) => { return s.name == LastShaderName; })) {
                shaderMenu.value = LastShaderName;
            } else {
                if(shaderMenu.index < 0)
                    shaderMenu.index = 0;
            }
        }

        shaderMenu.RegisterCallback<ChangeEvent<string>>(shaderChanged);
        
        settingsContainer.Add(shaderMenu);

        var textureContainer = new VisualElement();
        textureContainer.name = "zw-mtpd-texture-container";


        VisualElement saveControlsContainer = new VisualElement();
        root.Add(saveControlsContainer);

        VisualElement saveMatContainer = new VisualElement();

        saveControlsContainer.Add(saveMatContainer);
        
        saveMatContainer.name = "zw-mtpd-savemat-container";
        saveMatContainer.AddToClassList("zw-vertic-container");
        saveMatContainer.AddToClassList("zw-mtpd-save-container");

        UnityEngine.UIElements.Button saveMat = new UnityEngine.UIElements.Button(saveMatButtonClicked);
        saveMat.text = "Save as new material..";
        saveMatContainer.Add(saveMat);
        
        if(tgt.outputMaterial != null && AssetDatabase.Contains(tgt.outputMaterial))
            matFileNameLabel = new Label(AssetDatabase.GetAssetPath(tgt.outputMaterial));
        else
            matFileNameLabel = new Label("Not saved");
        saveMatContainer.Add(matFileNameLabel);

        tgt.Pipeline.PipelineDataPublish -= PipelineHasNewData;
        tgt.Pipeline.PipelineDataPublish += PipelineHasNewData;
        
        root.RegisterCallback<AttachToPanelEvent>(attached);

        return root;
    }

    void toggleUseTerrainMap(ChangeEvent<bool> toggledUseTerrain) {
        LastUseTerrainMap = toggledUseTerrain.newValue;
        tgt.UseTerrainMap = toggledUseTerrain.newValue;
        tgt.Invalidate();
    }

    void shaderChanged(ChangeEvent<string> changedShader) {
        var newShader = changedShader.newValue;
        if(string.IsNullOrWhiteSpace(newShader))
            return;

        if(newShader == changedShader.previousValue)
            return;

        if(newShader == tgt.SelectedShader)
            return;

        tgt.Generate();

        LastShaderName = newShader;
    }

    void saveMatButtonClicked() {
        if(tgt.outputMaterial == null)
            return;

        if(AssetDatabase.Contains(tgt.outputMaterial)) {
            EditorUtility.SetDirty(tgt.outputMaterial);
            matFileNameLabel.text = AssetDatabase.GetAssetPath(tgt.outputMaterial);
            return;
        }
        
        if(!AssetDatabase.IsValidFolder("Assets/data"))
            AssetDatabase.CreateFolder("Assets", "data");

        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/data/terrainmaterial.asset");
        AssetDatabase.CreateAsset(tgt.outputMaterial, path);
        matFileNameLabel.text = AssetDatabase.GetAssetPath(tgt.outputMaterial);
    }

    void attached(AttachToPanelEvent evt) {
        if(LastShaderName != tgt.SelectedShader || string.IsNullOrWhiteSpace(tgt.SelectedShader)) {
            var menu = root.Q<DropdownField>();
            if(menu == null)
                return;

            tgt.LoadShaders();
            
            if(menu.value != tgt.SelectedShader) {
                if(!tgt.IsShaderLoaded(menu.value))
                    tgt.AddShader(menu.value);

                tgt.SetShader(menu.value);
                LastShaderName = menu.value;
            }
        }

        uiInitialized = true;
        updatePreview();
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        
        if(node is TerrainMapGenerator) {
            var tg = (TerrainMapGenerator)node;
            var tx = tg.GetTextureOutput();
            if(tx != texture)
                texture = tx;
        }

        if(AssetDatabase.Contains(tgt.outputMaterial)) {
            EditorUtility.SetDirty(tgt.outputMaterial);
            matFileNameLabel.text = AssetDatabase.GetAssetPath(tgt.outputMaterial);
        }

        updatePreview();
    }

    void updatePreview() {
        if(!uiInitialized)
            return;

    }
}
}