using UnityEngine;

public class Flower : MonoBehaviour
{
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private Collider flowerCollider;
    private Material flowerMaterial;
    
    [HideInInspector]
    public Collider nectarCollider;

    public Color filledColor = new Color(1f, 0, 0.3f);
    public Color emptyColor = new Color(0.5f, 0, 1f);

    public Vector3 FlowerUpVector => nectarCollider.transform.up;
    public Vector3 FlowerCenterPosition => nectarCollider.transform.position;
    public float NectarAmount { get; private set; }
    public bool HasNectar => NectarAmount > 0f;

    public float Feed(float amount)
    {
        float amountToTake = Mathf.Clamp(amount, 0f, NectarAmount);
        NectarAmount -= amountToTake;
        if (!HasNectar)
        {
            flowerCollider.enabled = false;
            nectarCollider.enabled = false;
            flowerMaterial.SetColor(BaseColor, emptyColor);
        }

        return amountToTake;
    }
}
