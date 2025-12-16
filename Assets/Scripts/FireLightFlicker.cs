using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireLightFlicker : MonoBehaviour
{
    public float baseIntensity = 2f;
    public float flickerAmount = 0.5f;
    public float speed = 5f;

    Light l;

    void Awake()
    {
        l = GetComponent<Light>();
    }

    void Update()
    {
        if (l == null) return;

        float noise = Mathf.PerlinNoise(Time.time * speed, 0f);
        l.intensity = baseIntensity + (noise - 0.5f) * flickerAmount;
    }
}
