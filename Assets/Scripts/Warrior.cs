using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : MonoBehaviour {

    [Header("Movement")]
    public bool shouldMove;
    public float max_speed;
    public float rotation_speed;
    public float stopAt;
    private Rigidbody body;


    [Header("Object Avoidance")]
    public LayerMask LayerMask;
    public float sightRange;
    public float steer;

    //character attributes
    [Header("Stats")]
    public float maxhealth;
    public float life = 100.0f;
    public float attack = 10.0f;
    

    [Header("Attack behaviour")]
    //states
    public bool is_attacking = false;
    public bool has_already_target = false;
    public bool is_on_contact = false;
    public bool is_fighting = false;

    //attack
    public GameObject attacktarget;
    public GameObject[]enemy_squad;
    private List<int> deadenemies = new List<int>();
 
    Vector3 dir;

    private void Start()
    {
        shouldMove = true;
        attacktarget = null;
        body = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(0,9);
    }


    public void cancelAttack()
    {
        for (int i = 0; i < enemy_squad.Length; i++) {
            enemy_squad = null;
        }
        attacktarget = null;
        has_already_target = false;
        is_on_contact = false;
        is_fighting = false;
        //deadenemies.Clear();
    }

    //prende danno
    public void takeDamage(float dmg) {
        life = life - dmg;
        if (life <= 0) {
            //Debug.Log(transform + "Morto");
            Destroy(gameObject); 
        }
    }

    public IEnumerator AttackEnemy() {
        while (attacktarget != null && attacktarget.GetComponent<Warrior>().life > 0)
        {
            yield return new WaitForSeconds(1);
            if (is_on_contact == true && attacktarget!= null)
            {
                try
                {
                    attacktarget.GetComponent<Warrior>().takeDamage(attack);
                }
                catch(NullReferenceException e)
                {
                    //Debug.Log("choosen enemy is already dead..");
                }
                catch (MissingReferenceException ee)
                {
                    //Debug.Log("gameobject not found");
                }
                yield return new WaitForSeconds(1);
            }
            else
            {
                yield return null;
            }
            
        }

    }
    public void moveTo(Vector3 dest)
    {
        Vector3 verticalAdj = new Vector3(dest.x, transform.position.y, dest.z); //prevents sinking
        Vector3 toDest = verticalAdj - transform.position; //distance between me and destination
        Vector3 direction = (verticalAdj - transform.position).normalized;
        
        RaycastHit ray;
        if (toDest.magnitude > stopAt && shouldMove == true)
        {
            //if to far from its slot
            if (toDest.magnitude > 4) { max_speed = 8; } else { max_speed = 6; }

            //Object avoidance
            //straight
            if (Physics.Raycast(transform.position, transform.forward, out ray, sightRange, LayerMask))
            {
                if (ray.transform != transform && ray.transform.tag != "NPC" && ray.transform.tag != "PlayerUnit")
                {
                    direction += ray.normal * steer;
                    
                }
            }
            //left 45°
            if (Physics.Raycast(transform.position, (transform.forward - transform.right).normalized, out ray, sightRange, LayerMask))
            {
                if (ray.transform != transform && ray.transform.tag != "NPC" && ray.transform.tag != "PlayerUnit")
                {
                    direction += ray.normal * steer;
                }
            }

            //right 45°
            if (Physics.Raycast(transform.position, (transform.forward + transform.right).normalized, out ray, sightRange, LayerMask))
            {
                if (ray.transform != transform && ray.transform.tag != "NPC" && ray.transform.tag != "PlayerUnit")
                {
                    direction += ray.normal * steer;
                }
            }


            //debug for object avoidance vectors
            Debug.DrawRay(transform.position, transform.forward, Color.green);
            Debug.DrawRay(transform.position, (transform.forward - transform.right), Color.green);
            Debug.DrawRay(transform.position, (transform.forward + transform.right), Color.green);

            //look at destination
            var rot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotation_speed);   
            //move
            body.MovePosition(transform.position + transform.forward * max_speed * Time.deltaTime);
        }
        else
        {
            shouldMove = false;
            
        }

    }


}
