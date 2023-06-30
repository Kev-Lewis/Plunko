using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mover : MonoBehaviour
{
    private float min, max;

    public float pingPongMulti;
    public float reverse;
    [SerializeField] private float maxOffset;
    [SerializeField] private Sprite originSprite;
    [SerializeField] private Sprite customBucketSprite;
    // Start is called before the first frame update
    void Start()
    {
        min = transform.position.x;
        max = transform.position.x + maxOffset;

        if(gameObject.name == "bucket"){
            if(PlayerPrefs.GetInt("customBucket") == 0){
                GetComponent<SpriteRenderer>().sprite = originSprite;
            }
            if(PlayerPrefs.GetInt("customBucket") == 1){
                GetComponent<SpriteRenderer>().sprite = customBucketSprite;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(reverse * Mathf.PingPong(Time.time*pingPongMulti, max-min)+min, transform.position.y, transform.position.z);
    }
}
