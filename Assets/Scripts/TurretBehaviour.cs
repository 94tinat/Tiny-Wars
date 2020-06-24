using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour {

    [Header("Arrow prefab")]
    public GameObject arrow_prefab;
    public Transform SpawnArrowPoint;

    [Header("Turret Stats")]
    [Range(5f, 50f)] public float attackRange;


    private bool alerted = false;
    private bool attacking = false;
    private Vector3 onGround; 

	// Use this for initialization
	void Start () {

        onGround = new Vector3(transform.position.x, 0f, transform.position.z);
        //InvokeRepeating("ShootArrow", 0f, 2f);
    }

	
	// Update is called once per frame
    /*
	void Update () {
        if (Input.GetKey(KeyCode.M))
        {
            Debug.Log("Stops shooting turret");
            CancelInvoke("ShootArrow");
        }
	}
    */

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 fromGround = new Vector3(transform.position.x, 0f, transform.position.z);
        Gizmos.DrawWireSphere(fromGround, attackRange);
    }

    //yellow light On/Off
    public void isAlerted(bool val )
    {
        transform.GetChild(0).gameObject.SetActive(val);
    }

    //red light On/Off
    public void isAlertedAndAttacking(bool val)
    {
        transform.GetChild(1).gameObject.SetActive(val);
    }


    //invokes the Attack method
    public void startAttack()
    {
        InvokeRepeating("ShootArrow", 0f, 2f);
    }

    //stops the Invoke
    public void stopAttack()
    {
        CancelInvoke("ShootArrow");
    }


    //ATTACK
    public void ShootArrow()
    {
        Collider[] colliders = Physics.OverlapSphere(onGround, attackRange);
        List<GameObject> enemies = new List<GameObject>();
        foreach (Collider collider in colliders)
        {
            if(collider.tag == "PlayerUnit") {
                enemies.Add(collider.gameObject);
            }
        }
        if(enemies.Count > 0)
        {
            int random = UnityEngine.Random.Range(0, enemies.Count);
            Debug.Log("nemico scelto: " + enemies[random].transform);

            GameObject freccia = Instantiate(arrow_prefab, SpawnArrowPoint.position, SpawnArrowPoint.rotation);
            Arrow a = freccia.GetComponent<Arrow>();
            if(a!= null)
            {
                a.setTarget(enemies[random].gameObject.transform);
            }    
        }
    }
}
