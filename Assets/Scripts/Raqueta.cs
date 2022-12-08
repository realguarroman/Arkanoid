using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raqueta : MonoBehaviour
{

    public Rigidbody rigidbody;
    private float entrada;
    public float velocidad = 250;
    private Vector3 direccion;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        entrada = Input.GetAxisRaw("Horizontal");
        if (entrada == 1) direccion = Vector3.right;
        if (entrada == -1) direccion = Vector3.left;
        if (entrada == 0) direccion = Vector3.zero;
        rigidbody.AddForce(direccion * velocidad * Time.deltaTime);
    }
}
