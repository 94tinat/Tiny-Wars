using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFormationManager : MonoBehaviour {

    [Header("Warrior prefab")]
    public GameObject prefab;

    [Header("Attack Target")]
    public GameObject enemy_squad;
    public bool hastarget = false; 
    public bool isOnContact = false;

    //states
    [Header("States")]
    public bool isDefending = false;
    public bool isPatrolling = false; 
    public bool isFleeing = false;
    public bool isRecharging = false;
    public bool isOnRecoveryPoint = false;
    public bool isbusy; 
    public bool isAttacking;
    public bool isSelected = false;
    bool isMoving;

    [Header("Recovery Point")]
    public GameObject recovery;

    [Header("Defend Point")]
    public GameObject defendposition;

    [Header("Patrol Points")]
    public List<GameObject> patrolpoints;
    int counter_patrol = 0;

    [Header("Squad Units")]
    public GameObject[] warriors = new GameObject[9];

    //attack 
    private List<int> deadenemies = new List<int>();

    NavMeshAgent agent;
    List<Vector3> positions;
    List<GameObject> empties;
    GameObject terrain;
    Vector3 formation_destination;

    public void Start()
    {
        terrain = GameObject.FindWithTag("terrain");
        formation_destination = Vector3.zero;
        agent = GetComponent<NavMeshAgent>();
        positions = new List<Vector3>();
        empties = new List<GameObject>();

        isAttacking = false;
        isMoving = false;
        isbusy = false;

        //initialize formation positions
        for (int i = 1; i < transform.childCount; i++)
        {
            empties.Add(transform.GetChild(i).gameObject);
        }

    }


    public int getHealth()
    {
        int res = 0;
        for(int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null) { res++; }
        }

        return res;
    }


    //returns to its assigned defend position
    public void ReturnToDefendPoint()
    {
        if(defendposition != null)
        {
            agent.destination = defendposition.transform.position;
            moveSquad();
        }
        
    }


    //flees to a specific point to recover health.
    public void Flee()
    {
        if (recovery != null)
        {
            agent.destination = recovery.transform.position;
            moveSquad();
        }
    }


    //basic comand to move the squad.
    void moveSquad()
    {
        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null)
            {
                //devo accedere allo script di movimento della singola unita. 
                Warrior w = warriors[i].GetComponent<Warrior>();
                w.cancelAttack();
                w.shouldMove = true;
                w.moveTo(empties[i].transform.position);
            }
        }
    }

    //chases a selected Enemy Squad
    public void ChaseEnemy(GameObject enemy)
    {
        agent.destination = enemy.transform.position;
        moveSquad();
    }

    /*
    //attacks the target squad
    public void Attack(GameObject[] target)
    {
        //eseguire solo una volta
        foreach (GameObject soldatino in warriors)
        {
            if (soldatino != null)
            {
                soldatino.GetComponent<Warrior>().enemy_squad = target;
                soldatino.GetComponent<Warrior>().is_attacking = true;
            }

        }
        //in futuro: passero l'array dei target a ciascuno dei soldatini, cosi poi sara lui a scegliere chi attaccare.
    }
    */

    //Patrols between the patrol points
    public void Patrol()
    {
        //AI PATROL
        if (patrolpoints.Count > 0 && patrolpoints != null)
        {
            Vector3 verticalAdj = new Vector3(patrolpoints[counter_patrol].transform.position.x, transform.position.y, patrolpoints[counter_patrol].transform.position.z); //prevents sinking
            Vector3 toDest = verticalAdj - transform.position; //distance between me and destination
            if (toDest.magnitude > 1)
            {
                //go to patrol point
                agent.destination = patrolpoints[counter_patrol].transform.position;
                moveSquad();
            }
            else
            {
                //he reached the patrol point 
                if (counter_patrol >= patrolpoints.Count - 1)
                {
                    //restart from first patrol point
                    counter_patrol = 0;
                }
                else
                {
                    //go to next point
                    counter_patrol++;
                }

            }
        }
        else
        {
            Debug.Log("NO patrol points set!");
        }
    }

    //ENGAGES in combat with a target squad
    public void EngageSquad()
    {
        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null)
            {
                Warrior guerriero = warriors[i].GetComponent<Warrior>();

                //if guerriero does not have a target to attack
                if (guerriero.attacktarget == null && guerriero.has_already_target == false)
                {
                    //randomic choice of a target inside enemysquad.warriors
                    int random = UnityEngine.Random.Range(0, enemy_squad.GetComponent<PlayerFormationManager>().warriors.Length);
                    if (deadenemies.Count == enemy_squad.GetComponent<PlayerFormationManager>().warriors.Length)
                    {
                        //all warriors in enemy squad are dead
                        Debug.Log(guerriero.transform + "Everyone is dead!");
                        guerriero.is_attacking = false;
                        guerriero.is_on_contact = false;
                        guerriero.is_fighting = false;
                    }
                    else
                    {
                        //otherwise it chooses a target
                        if (!deadenemies.Contains(random) && enemy_squad.GetComponent<PlayerFormationManager>().warriors[random] != null)
                        {
                            guerriero.has_already_target = true;
                            guerriero.attacktarget = enemy_squad.GetComponent<PlayerFormationManager>().warriors[random];
                            //deadenemies.Add(random);                            
                        }
                        else
                        {
                            //if the enemy choosen is already dead then it must choose another one.
                            guerriero.attacktarget = null;
                            guerriero.has_already_target = false;
                        }
                    }
                }
                else
                {
                    //guerriero has already a target to attack
                    if (guerriero.attacktarget != null)
                    {
                        //moves to it and attacks
                        if ((guerriero.transform.position - guerriero.attacktarget.transform.position).magnitude < 1.5)
                        {
                            guerriero.shouldMove = false;
                            //on contact with enemy
                            if (guerriero.attacktarget.GetComponent<Warrior>().life > 0 && guerriero.is_on_contact == false)
                            {
                                //enter once here
                                guerriero.is_on_contact = true;
                                guerriero.is_fighting = true;
                                StartCoroutine(guerriero.AttackEnemy());
                            }

                        }
                        else
                        {
                            //not on contact anymore, must chase
                            guerriero.is_on_contact = false;
                            guerriero.is_fighting = false;
                            guerriero.shouldMove = true;
                            guerriero.moveTo(guerriero.attacktarget.transform.position);
                        }
                    }
                    else
                    {
                        //target is dead
                        guerriero.attacktarget = null;
                        guerriero.has_already_target = false;
                    }
                }
            } //end if guerriero!=null
        } // end for
    }


    void Update()
    {
    }

    private void FixedUpdate()
    {
        //PATROL
        if(isPatrolling == true)
        {
            Patrol();
        }

        //CHASE
        if (hastarget == true)
        {
            if (enemy_squad != null && Vector3.Distance(enemy_squad.transform.position, transform.position) > 5.0f )
            {
                ChaseEnemy(enemy_squad);
            }
        }

        //ATTACK
        if (isOnContact == true && enemy_squad!=null)
        {
            if (Vector3.Distance(enemy_squad.transform.position, transform.position) < 5.0f)
            {
                EngageSquad();
            }
        }
        

        //FLEE to recovery point
        if(isFleeing == true)
        {
            Flee();
        }

        //RECHARGE HEALTH
        if(isRecharging == true)
        {
            isRecharging = false; //enters once here
            rechargeWarriorsHealth();
            StartCoroutine(spawnCoroutine());
        }

        //DEFENSIVE
        if(defendposition!=null && isDefending == true)
        {
            ReturnToDefendPoint();   
        }
        
    }


    //Recharges the life of alive warriors
    public void rechargeWarriorsHealth()
    {
        for(int i = 0; i < warriors.Length; i ++)
        {
            if(warriors[i] != null)
            {
                StartCoroutine(rechargeHealth(warriors[i]));
            }
        }

    }

    //returns true if life of all warriors is full
    public bool areFullHealthWarriors()
    {
        int count = 0;
        for(int i = 0; i < warriors.Length; i++)
        {
            if(warriors[i] != null && warriors[i].GetComponent<Warrior>().life == warriors[i].GetComponent<Warrior>().maxhealth)
            {
                count++;
            }
            
        }
        
        if(count == getHealth())
        {
            return true;
        }
        else {
            return false;
        }
        
    }

    //Spawns new warriors 
    public IEnumerator spawnCoroutine()
    {
        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] == null && isOnRecoveryPoint == true)
            {
                yield return new WaitForSeconds(0.5f);
                //istanzia un warrior.
                Vector3 spawnposition = empties[i].transform.position;
                spawnposition.y += 1;
                GameObject newWarrior = Instantiate(prefab, spawnposition, Quaternion.identity);
                warriors[i] = newWarrior;
                yield return new WaitForSeconds(5);
            }
        }
    }



    //coroutine to recharge health
    private IEnumerator rechargeHealth(GameObject warrior)
    {
        while(warrior.GetComponent<Warrior>().life <= warrior.GetComponent<Warrior>().maxhealth && isOnRecoveryPoint == true && warrior != null)
        {

            if(warrior.GetComponent<Warrior>().life + 10 > warrior.GetComponent<Warrior>().maxhealth)
            {
                warrior.GetComponent<Warrior>().life = warrior.GetComponent<Warrior>().maxhealth;
            }
            else
            {
                warrior.GetComponent<Warrior>().life += 10;
            }

            yield return new WaitForSeconds(1);
        }

        yield return null; //esce dalla coroutine
    }

}
