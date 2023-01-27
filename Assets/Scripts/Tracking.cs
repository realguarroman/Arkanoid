using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracking : MonoBehaviour
{
    [SerializeField]
    float range = 5f;
    [SerializeField]
    float speed = 2;
    [SerializeField]
    float track = 25;

    Vector3 target;

    float WRight = 10f;
    float WLeft = -10f;



    // Start is called before the first frame update
    void Start()
    {
        SetTarget();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, target) < range)
        {
            SetTarget();
        }
    }

    void SetTarget()
    {
        target = new Vector3(Random.Range(WLeft, WRight), track, 0);
    }
}
