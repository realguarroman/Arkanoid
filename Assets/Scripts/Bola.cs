using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class Bola : MonoBehaviour
{

    public Rigidbody rigidbody2;
    public float speed = 75;
    private Vector3 velocidad;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2 = GetComponent<Rigidbody>();
        velocidad.x = Random.Range(-1.0f,1.0f);
        velocidad.y = 1;
        Debug.Log(velocidad);
        //rigidbody2.AddForce(velocidad * speed);
        rigidbody2.AddForce(Vector3.up * speed);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
