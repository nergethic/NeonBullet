using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = new Vector3(Mathf.Sin(2.0f*Time.time), transform.position.y, transform.position.z);
        // transform.position = transform.position - target.transform.position;
        Vector3 hitDirection = target.transform.position - transform.position;
        hitDirection.Normalize();

        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(hitDirection), out hit, Mathf.Infinity))
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            Debug.DrawRay(transform.position, transform.TransformDirection(hitDirection) * hit.distance, Color.yellow);
            // Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(hitDirection) * 1000, Color.white);
            //Debug.Log("Did not Hit");
        }
    }
}
