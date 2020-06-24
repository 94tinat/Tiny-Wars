using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    private float speed = 30f;
    private float damage = 100f;
    private Transform target;
	

    public void setTarget(Transform t)
    {
        target = t;
    }

	// Update is called once per frame
	void Update () {
        
        if(target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        if(target!= null)
        {
            //Debug.DrawLine(transform.position, target.transform.position, Color.red);
            Vector3 dir = target.position - transform.position;

            float DistanceThisFrame = speed * Time.deltaTime;
            
            if(dir.magnitude <= DistanceThisFrame)
            {
                //target is hit
                HitTarget();
                return;
            }
            transform.Translate(dir.normalized * DistanceThisFrame, Space.World);


        }
        
	}

    public void HitTarget()
    {
        if(target != null)
        {
            target.GetComponent<Warrior>().takeDamage(damage);
            Destroy(gameObject);
        }
    }
   
}
