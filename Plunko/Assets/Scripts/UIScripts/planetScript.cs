using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class planetScript : MonoBehaviour
{
    float randoSpeed;
    // Start is called before the first frame update
    void Start()
    {
        float randoSize = Random.Range(7f,9f);
        randoSpeed = Random.Range(.3f,.7f);
        transform.localScale = new Vector2((randoSpeed+.2f)*10,(randoSpeed+.2f)*10);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * randoSpeed * Time.deltaTime;
        if(transform.position.x > 12.5f){
            Destroy(gameObject);
        }
    }
}
