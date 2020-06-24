using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerFormationManager : MonoBehaviour {

    [Header("Attack Target")]
    public GameObject enemy_squad;

    //states
    [Header("States")]
    public bool isSelected = false;
    public bool shouldmove = false;
    public bool isChasing = false;
    public bool isOnContact = false;
    public bool isAttacking;
    

    [Header("Recovery Point")]
    public GameObject recovery;

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

    public void Start () {

        formation_destination = Vector3.zero;
        terrain = GameObject.FindWithTag("terrain");
        agent = GetComponent<NavMeshAgent>();
        positions = new List<Vector3>();
        empties = new List<GameObject>();
        patrolpoints = new List<GameObject>();
        
        //initialize formation positions
        for (int i = 1; i < transform.childCount; i++) {
            empties.Add(transform.GetChild(i).gameObject);
        }
    }

    public int getHealth()
    {
        int res = 0;
        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null) { res++; }
        }

        return res;
    }


    //flees to a specific point to recover health.
    public void Flee(){
        if(recovery!=null){
            agent.destination = recovery.transform.position;
            moveSquad();
        }
    }

    //basic comand to move the squad.
    void moveSquad() {
        for (int i = 0; i < warriors.Length; i++) {
            if (warriors[i] != null) {
                //devo accedere allo script di movimento della singola unita. 
                Warrior w = warriors[i].GetComponent<Warrior>();
                w.shouldMove = true;
                w.moveTo(empties[i].transform.position);
            }
        }
    }

    //chases a selected Enemy Squad
    public void ChaseEnemy(GameObject enemy){
        agent.destination = enemy.transform.position;
        moveSquad();
    }


    //attacks the target squad --> DEPRECATED (delete later)
    public void Attack(GameObject target) {
        //eseguire solo una volta
        GameObject[] enemy_units = target.GetComponent<EnemyFormationManager>().warriors;

        foreach (GameObject soldatino in warriors)
        {
            if (soldatino != null)
            {
                soldatino.GetComponent<Warrior>().is_attacking = true;
                soldatino.GetComponent<Warrior>().enemy_squad = enemy_units;           
            }
        }
    }


    //Engage a specific squad: each unit in this squad will attack a specific unit in "enemy squad"
    public void EngageSquad()
    { 
        for(int i = 0; i < warriors.Length; i ++)
        {
            if(warriors[i] != null)
            {
                Warrior guerriero = warriors[i].GetComponent<Warrior>();
                
                //if guerriero does not have a target to attack
                if(guerriero.attacktarget == null && guerriero.has_already_target == false)
                {
                    //randomic choice of a target inside enemysquad.warriors
                    int random = UnityEngine.Random.Range(0, enemy_squad.GetComponent<EnemyFormationManager>().warriors.Length);
                    if(deadenemies.Count == enemy_squad.GetComponent<EnemyFormationManager>().warriors.Length)
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
                        if (!deadenemies.Contains(random) && enemy_squad.GetComponent<EnemyFormationManager>().warriors[random]!=null)
                        {
                            guerriero.has_already_target = true;
                            guerriero.attacktarget = enemy_squad.GetComponent<EnemyFormationManager>().warriors[random];
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
                        //se ha gia un target si muove e lo attacca
                        if ((guerriero.transform.position - guerriero.attacktarget.transform.position).magnitude < 1.5)
                        {
                            guerriero.shouldMove = false;
                            //è a contatto con il nemico
                            if (guerriero.attacktarget.GetComponent<Warrior>().life > 0 && guerriero.is_on_contact == false)
                            {
                                //entra una volta qui.
                                guerriero.is_on_contact = true;
                                guerriero.is_fighting = true;
                                StartCoroutine(guerriero.AttackEnemy());
                            }

                        }
                        else
                        {
                            //è uscito dal contact, deve inseguire.
                            guerriero.is_on_contact = false;
                            guerriero.is_fighting = false;
                            guerriero.shouldMove = true; 
                            guerriero.moveTo(guerriero.attacktarget.transform.position);
                        }
                    }
                    else
                    {
                        //vuol dire che è morto
                        guerriero.attacktarget = null;
                        guerriero.has_already_target = false;
                    }
                }
            } //end if guerriero!=null
        } // end for
    }//end Engage Squad



    //Patrols between the patrol points
    public void Patrol() {

        /*
        //initialize formation positions
        for (int i = 1; i < transform.childCount; i++)
        {
            empties.Add(transform.GetChild(i).gameObject);
        }
        */

        //AI PATROL
        if (patrolpoints.Count > 0 && patrolpoints != null)
        {
            Vector3 verticalAdj = new Vector3(patrolpoints[counter_patrol].transform.position.x, transform.position.y, patrolpoints[counter_patrol].transform.position.z); //prevents sinking
            Vector3 toDest = verticalAdj - transform.position; //distance between me and destination

            if (toDest.magnitude > 1)
            {
                //go to patrol point

                agent.destination = patrolpoints[counter_patrol].transform.position;
                //moveSquad();
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

    //when a squad is selected shows sprites (yellow circles) around its units
    public void showSelectionSprites(bool isSelected)
    {
        if (this.isSelected)
        {
            for (int i = 0; i < warriors.Length; i++)
            {
                if (this.warriors[i] != null)
                {
                    this.warriors[i].transform.GetChild(0).gameObject.SetActive(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < this.warriors.Length; i++)
            {
                if (this.warriors[i] != null)
                {
                    this.warriors[i].transform.GetChild(0).gameObject.SetActive(false);
                }
            }
        }

    }

    //cancel an attack order
    private void stopAttacking()
    {
        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] != null)
            {
                Warrior w = warriors[i].GetComponent<Warrior>();
                w.cancelAttack();
            }
        }
    }


    private bool hasAliveWarriors()
    {
        bool alive = true;
        int deathcounter = 0;
        for (int i = 0; i < warriors.Length; i++)
        {
            if (warriors[i] == null)
            {
                deathcounter += 1;
            }
        }

        if (deathcounter == warriors.Length)
        {
            alive = false;
        }

        return alive;
    }



    void Update () {   
        //SELECTION OF THE SQUAD
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
                //pop up circle on the squad
                if (hit.transform.gameObject == transform.gameObject)
                {
                    isSelected = true;
                    showSelectionSprites(isSelected);
                }
                else {
                    isSelected = false;
                    showSelectionSprites(isSelected);
                }
            }
        }

        //MOVEMENT ORDERS with mouse
        RaycastHit hit1;
        if (Input.GetMouseButtonDown(1))
        {
            if (isSelected == true) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit1, Mathf.Infinity))
                {
                    if (hit1.transform.tag.Equals("EnemySquad") )
                    {
                        //attacks enemy
                        enemy_squad = hit1.transform.gameObject;
                        //Debug.Log("squad attacca" + enemy_squad.transform);
                        isChasing = true;
                        deadenemies.Clear();
                    }
                    else
                    {
                        //moves to the position
                        shouldmove = true;
                        isChasing = false;
                        isAttacking = false;
                        isOnContact = false;
                        formation_destination = hit1.point;
                        stopAttacking();
                    }           
                }

            }
        }

    }

    private void FixedUpdate()
    {
        if (hasAliveWarriors())
        {
            //MOVEMENT 
            if (shouldmove == true && isChasing == false && isAttacking == false && isOnContact == false)
            {
                agent.destination = formation_destination;
                moveSquad();
            }

            //CHASES a target
            if (isChasing == true)
            {
                if (Vector3.Distance(enemy_squad.transform.position, transform.position) > 5.0f)
                {
                    //Debug.Log("sto inseguendo");
                    isChasing = true;
                    ChaseEnemy(enemy_squad);
                }
                else
                {
                    //Debug.Log("Sono a contatto");
                    isChasing = false;
                    isOnContact = true;
                }
            }

            //ATTACKS
            if (isOnContact == true && enemy_squad != null)
            {
                if (Vector3.Distance(enemy_squad.transform.position, transform.position) < 5.0f)
                {
                    EngageSquad();
                }
                else
                {
                    //Debug.Log("Uscita Attack!");
                    isChasing = true;
                    isOnContact = false;
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
