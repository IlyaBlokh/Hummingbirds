using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class FlowerArea : MonoBehaviour
    {
        public const float AreaDiameter = 20f;
    
        private readonly Dictionary<Collider, Flower> nectarFlowers = new();

        public List<FlowerPlant> FlowerPlants = new();
        public List<Flower> Flowers = new();

        private void Awake()
        {
            FlowerPlants.ForEach(plant =>
            {
                plant.Flowers.ForEach(flower => nectarFlowers.Add(flower.nectarCollider, flower));
            });
        }

        public void ResetFlowers()
        {
            FlowerPlants.ForEach(plant =>
            {
                float xRotation = Random.Range(-5f, 5f);
                float yRotation = Random.Range(-180f, 180f);
                float zRotation = Random.Range(-5f, 5f);
                plant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
            });
        
            Flowers.ForEach(flower => flower.SetDefault());
        }

        public Flower GetFlowerWithCollider(Collider nectarCollider) =>
            nectarFlowers[nectarCollider];

        public void InitFlowers()
        {
            nectarFlowers.Clear();
            FlowerPlants.ForEach(plant =>
            {
                Flowers.AddRange(plant.Flowers);
                plant.Flowers.ForEach(flower => nectarFlowers.Add(flower.nectarCollider, flower));
            });
        }
    }
}
