using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FlowerArea))]
    public class FlowerAreaEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var flowerArea = (FlowerArea)target;

            if (GUILayout.Button("Grab flowers"))
            {
                flowerArea.FlowerPlants.Clear();
                flowerArea.Flowers.Clear();
                
                flowerArea.FlowerPlants = FindObjectsOfType<FlowerPlant>().ToList();
                flowerArea.InitFlowers();
            }
            EditorUtility.SetDirty(target);
        }
    }
}