using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject lightGameObject = this.gameObject;
        Light lightComp = lightGameObject.AddComponent<Light>();
        lightComp.intensity = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
