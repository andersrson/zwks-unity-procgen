using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.UIElements;
using Unity.VisualScripting;
using System;

namespace xyz.zwks.procgen.editor {

[CustomPropertyDrawer(typeof(NoiseGenerator))]
public class NoiseGeneratorPropDrawer : PropertyDrawer {

    NoiseGenerator ng = null;
    Texture2D tex = null;
    Image img = null;

    bool constrainDimensions = true;

    bool uiInitialized = false;

    UnityEngine.UIElements.Toggle constrain;

    IntegerField seed;
    IntegerField width;
    IntegerField height;
    
    FloatField scale;
    FloatField offsetX;
    FloatField offsetY;

    IntegerField octaves;
    FloatField lacunarity;
    FloatField persistence;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {

        VisualElement root = new VisualElement();
        root.name = "zw-ngpd-root";

        VisualElement noiseNodeRoot = new VisualElement();
        noiseNodeRoot.AddToClassList("zw-node-propdrawer");
        root.Add(noiseNodeRoot);

        ng = (NoiseGenerator)property.GetUnderlyingValue();

        //seed = new PropertyField(property.FindPropertyRelative("Seed"));
        seed = new IntegerField("Seed");
        seed.BindProperty(property.FindPropertyRelative("Seed"));
        seed.AddToClassList("zw-field-property");

        noiseNodeRoot.Add(seed);
        
        Foldout dims = new Foldout();
        dims.text = "Dimension properties";
        noiseNodeRoot.Add(dims);

        constrain = new UnityEngine.UIElements.Toggle("Constrain w/h");
        constrain.SetValueWithoutNotify(constrainDimensions);
        constrain.AddToClassList("zw-field-property");
        //constrain.RegisterCallback<ChangeEvent<bool>>(constrainChanged);

        width = new IntegerField("Width");
        width.BindProperty(property.FindPropertyRelative("Width"));
        width.AddToClassList("zw-field-property");
        
        height = new IntegerField("Height");
        height.BindProperty(property.FindPropertyRelative("Height"));
        height.AddToClassList("zw-field-property");

        scale = new FloatField("Scale");
        scale.BindProperty(property.FindPropertyRelative("Scale"));
        scale.AddToClassList("zw-field-property");

        offsetX = new FloatField("Offset X");
        offsetX.BindProperty(property.FindPropertyRelative("OffsetX"));
        offsetX.AddToClassList("zw-field-property");
        offsetY = new FloatField("Offset Y");
        offsetY.BindProperty(property.FindPropertyRelative("OffsetY"));
        offsetY.AddToClassList("zw-field-property");

        dims.Add(constrain);
        dims.Add(width);
        dims.Add(height);
        dims.Add(scale);
        dims.Add(offsetX);
        dims.Add(offsetY);

        Foldout noiseProps = new Foldout();
        noiseProps.text = "Noise settings";
        noiseNodeRoot.Add(noiseProps);

        octaves = new IntegerField("Octaves");
        octaves.BindProperty(property.FindPropertyRelative("Octaves"));
        octaves.AddToClassList("zw-field-property");

        lacunarity = new FloatField("Lacunarity");
        lacunarity.BindProperty(property.FindPropertyRelative("Lacunarity"));
        lacunarity.AddToClassList("zw-field-property");
        persistence = new FloatField("Persistence");
        persistence.BindProperty(property.FindPropertyRelative("Persistence"));
        persistence.AddToClassList("zw-field-property");
        
        noiseProps.Add(octaves);
        noiseProps.Add(lacunarity);
        noiseProps.Add(persistence);

        Foldout preview = new Foldout();
        preview.text = "Preview";
        noiseNodeRoot.Add(preview);

        img = new Image();
        preview.Add(img);
        img.scaleMode = ScaleMode.StretchToFill;
        img.image = Texture2D.blackTexture;
        img.AddToClassList("zw-node-preview-image");
        img.name = "zw-ngpd-preview-image";
        
        Button save = new Button(buttonClicked);
        save.text = "Save texture";
        noiseNodeRoot.Add(save);

        if(ng.Seed == 0)
            ng.Seed = (uint)new System.Random().Next(1, int.MaxValue / 2);
            
        root.RegisterCallback<AttachToPanelEvent>(attached);

        return root;
    }

#region Event capture

    void buttonClicked() {
        if(tex == null)
            return;

        if(!AssetDatabase.IsValidFolder("Assets/data"))
            AssetDatabase.CreateFolder("Assets", "data");

        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/data/noisemap.asset");
        AssetDatabase.CreateAsset(tex, path);
        AssetDatabase.SaveAssets();
    }

    void attached(AttachToPanelEvent evt) {
        uiInitialized = true;
        if(ng.Validate())
            UpdatePreview();

        constrain.RegisterCallback<ChangeEvent<bool>>(constrainChanged);

        seed.RegisterCallback<ChangeEvent<int>>(noiseParameterIntChanged);
        
        width.RegisterCallback<ChangeEvent<int>>(dimensionsChanged);
        height.RegisterCallback<ChangeEvent<int>>(dimensionsChanged);
        
        scale.RegisterCallback<ChangeEvent<float>>(noiseParameterFloatChanged);
        offsetX.RegisterCallback<ChangeEvent<float>>(noiseParameterFloatChanged);
        offsetY.RegisterCallback<ChangeEvent<float>>(noiseParameterFloatChanged);
        
        octaves.RegisterCallback<ChangeEvent<int>>(noiseParameterIntChanged);
        lacunarity.RegisterCallback<ChangeEvent<float>>(noiseParameterFloatChanged);
        persistence.RegisterCallback<ChangeEvent<float>>(noiseParameterFloatChanged);
        
    }

    void constrainChanged(ChangeEvent<bool> update) {
        constrainDimensions = update.newValue;
    }
#endregion

#region Update validation
    void dimensionsChanged(ChangeEvent<int> change) {
        
        if(constrainDimensions) {
            ng.Width = change.newValue;
            ng.Height = ng.Width;
        }

        if(ng.Width < 1)
            ng.Width = 1;

        if(ng.Height < 1)
            ng.Height = 1;

        if(ng.Width * ng.Height > UInt16.MaxValue) {
            if(constrainDimensions) {
                ng.Height = 255;
                ng.Width = 255;
            }
            if(change.target == height)
                ng.Height = change.previousValue;
            if(change.target == width)
                ng.Width = change.previousValue;
        }

        ng.InvalidateDimensions();

        noiseParameterUpdated();
    }

    void noiseParameterIntChanged(ChangeEvent<int> change) {
        
        if(ng.Seed == 0)
            ng.Seed = (uint)new System.Random().Next(1, int.MaxValue / 2);

        if(ng.Octaves < 0)
            ng.Octaves = 0;
    
        noiseParameterUpdated();
    }

    void noiseParameterFloatChanged(ChangeEvent<float> update) {
        if(ng.Scale < 0f)
            ng.Scale = 0f;
        if(ng.Lacunarity < 0f)
            ng.Lacunarity = 0f;
        if(ng.Persistence < 0f)
            ng.Persistence = 0f;

        noiseParameterUpdated();
    }

    void noiseParameterUpdated() {
        ng.InvalidateOutput();

        if(!uiInitialized)
            return;

        if(ng.Validate())
            UpdatePreview();
        else 
            ClearPreview();
    
    }

#endregion

    void ClearPreview() {
        tex = Texture2D.blackTexture;
        img.image = tex;
    }

    void UpdatePreview() {
        if(!uiInitialized)
            return;

        ng.Generate();
        tex = ng.GetTextureOutput();
        img.image = tex;
    }
}
}