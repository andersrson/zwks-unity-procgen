using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.UIElements;
using Unity.Mathematics;
using Unity.VisualScripting;
using System;
using System.Linq;

namespace xyz.zwks.procgen.editor {

[CustomPropertyDrawer(typeof(TerrainMapGenerator))]
public class TerrainMapGeneratorPropertyDrawer : PropertyDrawer {

    TerrainMapGenerator tgt;
    
    VisualElement root;

    VisualElement terrainMapDataRoot;

    SerializedProperty terrainTypes;

    Image img;

    [SerializeField]
    Texture2D tex;
    [SerializeField]
    
    Label texFileName;
        
    bool uiInitialized = false;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        
        tgt = property.GetUnderlyingValue() as TerrainMapGenerator;

        if(tgt.terrainTypes == null) {
            tgt.terrainTypes = new System.Collections.Generic.List<TerrainType>();
        }

        root = new VisualElement();
        root.name = "zw-tgpd-root";
        
        terrainMapDataRoot = new VisualElement();
        terrainMapDataRoot.AddToClassList("zw-node-propdrawer");
        root.Add(terrainMapDataRoot);
        
        var label = new Label("Height to terrain mappings");
        terrainMapDataRoot.Add(label);

        terrainTypes = property.FindPropertyRelative("terrainTypes");

        ListView list = new ListView();
        list.name = "zw-tgpd-trntype-list";
        list.bindingPath = "terrainTypes";
        list.dataSource = tgt.terrainTypes;
        list.reorderable = false;
        list.fixedItemHeight = 30f;
        list.makeItem = () => {
                
                PropertyField fl = new PropertyField();
                fl.AddToClassList("zw-tgpd-trntype-list-item");
                fl.RegisterValueChangeCallback(terrainValueChanged);
                fl.RegisterCallback<ChangeEvent<Single>>(terrainTypesChanged);
                fl.RegisterCallback<ChangeEvent<Color>>(terrainTypesChanged);
                
                return fl;
            };

        list.bindItem = (element, index) => {
            var pf = element as PropertyField;
            pf.BindProperty(terrainTypes.GetArrayElementAtIndex(index));
        };

        list.itemsAdded += terrainTypesCountChanged;
        list.itemsRemoved += terrainTypesCountChanged;

        terrainMapDataRoot.Add(list);
        
        Foldout preview = new Foldout();
        preview.name = "zw-tgpd-preview";
        preview.text = "Preview";
        terrainMapDataRoot.Add(preview);

        img = new Image();
        preview.Add(img);
        img.AddToClassList("zw-node-preview-image");
        img.name = "zw-tgpd-preview-image";

        img.scaleMode = ScaleMode.StretchToFill;
        img.image = Texture2D.blackTexture;

        VisualElement saveControlsContainer = new VisualElement();
        root.Add(saveControlsContainer);

        VisualElement saveTexContainer = new VisualElement();
        VisualElement saveMatContainer = new VisualElement();

        saveControlsContainer.Add(saveTexContainer);
        saveControlsContainer.Add(saveMatContainer);

        saveTexContainer.name = "zw-tgpd-savetex-container";
        saveTexContainer.AddToClassList("zw-horiz-container");
        saveTexContainer.AddToClassList("zw-tgpd-save-container");
        
        UnityEngine.UIElements.Button save = new UnityEngine.UIElements.Button(saveTexButtonClicked);
        save.text = "Save as new texture..";
        saveTexContainer.Add(save);

        texFileName = new Label();
        saveTexContainer.Add(texFileName);
        
        tgt.Pipeline.PipelineDataPublish -= PipelineHasNewData;
        tgt.Pipeline.PipelineDataPublish += PipelineHasNewData;
        
        root.RegisterCallback<AttachToPanelEvent>(attached);

        return root;
    }

    void terrainTypesCountChanged(System.Collections.Generic.IEnumerable<int> list) {
        if(list == null || list.Count() < 1) 
            return;

        tgt.Invalidate();
        tgt.Generate();
    }
    
    void terrainTypesChanged(ChangeEvent<Single> evt) {
        if(evt.newValue == evt.previousValue)
            return;
        
        // This is some internal init nonsense
        if(evt.newValue == 100f && evt.previousValue == 0f)
            return;

        if(evt.newValue < 0f || evt.newValue > 1.0f) {
            PropertyField pf = (PropertyField)evt.currentTarget;
            var ff = pf.Q<UnityEngine.UIElements.FloatField>();
            ff.value = math.clamp(evt.newValue, 0f, 1f);
        }

        tgt.Invalidate();
        tgt.Generate();
    }

    void terrainValueChanged(SerializedPropertyChangeEvent evt) {
        tgt.Invalidate();
        tgt.Generate();
    }

    void terrainTypesChanged(ChangeEvent<Color> evt) {
        if(evt.newValue == evt.previousValue)
            return;
        tgt.Invalidate();
        tgt.Generate();
    }

    void saveTexButtonClicked() {
        if(tex == null)
            return;

        if(AssetDatabase.Contains(tex)) {
            EditorUtility.SetDirty(tex);
            return;
        }

        if(!AssetDatabase.IsValidFolder("Assets/data"))
            AssetDatabase.CreateFolder("Assets", "data");

        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/data/terrainmap.asset");
        AssetDatabase.CreateAsset(tex, path);
    }

    void attached(AttachToPanelEvent evt) {
        uiInitialized = true;
        updatePreview();
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        if(node != tgt) 
            return;
        
        updatePreview();
    }

    void updatePreview() {
        if(!uiInitialized)
            return;

        tex = tgt.GetTextureOutput();
        if(tex != null) {
            img.image = tex;
        }
    }
}
}