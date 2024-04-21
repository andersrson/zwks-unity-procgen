using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace xyz.zwks.procgen.editor {

[CustomPropertyDrawer(typeof(TerrainType))]
public class TerrainTypePropertyDrawer : PropertyDrawer {

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var terrainType = (TerrainType)property.GetUnderlyingValue();

        VisualElement root = new VisualElement();
        root.AddToClassList("zw-horiz-container");

        var height = new FloatField("Height");
        height.BindProperty(property.FindPropertyRelative("MinHeight"));
        height.AddToClassList("zw-ttpd-height-propfield");
        root.Add(height);

        var color = new ColorField("Color");
        color.BindProperty(property.FindPropertyRelative("Color"));
        color.AddToClassList("zw-ttpd-color-propfield");
        root.Add(color);

        return root;
    }
}

}