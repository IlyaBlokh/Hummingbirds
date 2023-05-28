using System.Collections.Generic;
using System.Linq;
using Gameplay;
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
                Debug.Log("Flower area initialized");
            }

            if (GUILayout.Button("Init hummingbirds"))
            {
                List<HummingbirdAgent> birds = FindObjectsOfType<HummingbirdAgent>().ToList();
                birds.ForEach(bird => bird.SetArea(flowerArea));
                Debug.Log("Flower area set for hummingbird agents");
            }
            
            EditorUtility.SetDirty(target);
        }
    }
}