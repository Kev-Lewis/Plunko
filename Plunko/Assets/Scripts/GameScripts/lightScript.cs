using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightScript : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private float intensity = 1f;

    private Light lightComponent;

    private void Start() {
        lightComponent = GetComponent<Light>();

        if (lightComponent == null) {
            lightComponent = gameObject.AddComponent<Light>();
        }

        lightComponent.intensity = intensity;
    }
}