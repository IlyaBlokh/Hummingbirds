using System.Collections.Generic;
using UnityEngine;

public class FlowerArea : MonoBehaviour
{
    public const float AreaDiameter = 20f;

    private List<GameObject> flowerPlants = new();
    private Dictionary<Collider, Flower> nectarFlowers = new();
    
    public List<Flower> Flowers { get; private set; }

    public void ResetFlowers()
    {
        flowerPlants.ForEach(plant =>
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
}
