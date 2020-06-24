using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMDefender : MonoBehaviour {

    [Range(0f, 20f)] public float viewradius;
    [Range(0f, 20f)] public float attackrange;
    [Range(0f, 50f)] public float maximumDistanceFromDefend;

    public float reactionTime = 0f;
    public string targetTag = "PlayerSquad";

    private FSM fsm;
    private EnemyFormationManager formation;
    private Vector3 startedPosition;

    private List<GameObject> enemies = new List<GameObject>();
    private int random;

    void Start() {

        formation = GetComponent<EnemyFormationManager>();

        //States
        FSMState idle = new FSMState();
        idle.enterActions.Add(GoToCamp);
        idle.stayActions.Add(Watch);
        idle.exitActions.Add(ExitIdleState);

        FSMState chase = new FSMState();
        chase.enterActions.Add(StartChase);
        chase.stayActions.Add(Chase);
        chase.exitActions.Add(StopChase);

        FSMState attack = new FSMState();
        attack.enterActions.Add(StartAttack);
        attack.stayActions.Add(Fight);
        attack.exitActions.Add(StopAttack);

        FSMState dead = new FSMState();
        dead.enterActions.Add(Dead);

        FSMState returnInPosition = new FSMState();
        returnInPosition.enterActions.Add(ReturnToPosition);
        returnInPosition.stayActions.Add(Returning);
        returnInPosition.exitActions.Add(StopReturning);

        //Transitions
        FSMTransition t1 = new FSMTransition(ScanField);
        FSMTransition t2 = new FSMTransition(ScanFieldMoreNear);
        FSMTransition t3 = new FSMTransition(OutOfAttackRange);
        FSMTransition t4 = new FSMTransition(OutOfMaximumDistance);
        FSMTransition t5 = new FSMTransition(NoEnemiesInRange);
        FSMTransition t6 = new FSMTransition(NearEnemy);
        FSMTransition t7 = new FSMTransition(IsDead);
        FSMTransition t8 = new FSMTransition(InitialPosition);

        // Link states with transitions
        idle.AddTransition(t1, chase);
        chase.AddTransition(t2, attack);
        attack.AddTransition(t3, chase);
        chase.AddTransition(t4, returnInPosition);
        attack.AddTransition(t5, returnInPosition);
        returnInPosition.AddTransition(t6, attack);
        attack.AddTransition(t7, dead);
        returnInPosition.AddTransition(t8, idle);
       
        // Setup a FSA at initial state
        fsm = new FSM(idle);

        // Start monitoring
        StartCoroutine(Patrol());
    }

    // GIMMICS

    private void OnValidate()
    {
        Transform t = transform.Find("Range");
        if (t != null)
        {
            t.localScale = new Vector3(viewradius / transform.localScale.x, 1f, viewradius / transform.localScale.z) / 5f;
        }
    }

    // Periodic update, run forever
    public IEnumerator Patrol()
    {
        while (true)
       {
            fsm.Update();
            yield return new WaitForSeconds(reactionTime); 
        }
    }

    // CONDITIONS

    //TRANSITION from IDLE to CHASE
    public bool ScanField() {
    
        bool flagEnemy = false;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, viewradius);

        int i = 0;

        while (i < hitColliders.Length)
        {

            if (hitColliders[i].gameObject.tag == "PlayerSquad")
            {
                enemies.Add(hitColliders[i].gameObject);
                Debug.Log("Found an enemy!\n");
                flagEnemy = true;
            }

            i++;
        }

        return flagEnemy;

    }

    //TRANSITION from CHASE to ATTACK
    public bool ScanFieldMoreNear() {
    
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackrange);

        for(int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].gameObject.tag == "PlayerSquad")
            {
                return true;
            }
        }

        return false;
       
    }

    //TRANSITION from ATTACK to CHASE
    public bool OutOfAttackRange() {

        if(formation.isOnContact == true) {
            return false;
        }
        return true;
    }

    //TRANSITION from CHASE to RETURN
    public bool OutOfMaximumDistance() {

        if ((formation.defendposition.transform.position - transform.position).magnitude >= maximumDistanceFromDefend) {

            enemies.Clear();
            formation.enemy_squad = null;
            formation.hastarget = false;
            formation.isOnContact = false;
            Debug.Log("I have gone too far!\n");

            return true;
        }
        return false;

         
    }

    //TRANSITION from ATTACK to RETURN
    public bool NoEnemiesInRange() {

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackrange);

        if((formation.defendposition.transform.position - transform.position).magnitude <= maximumDistanceFromDefend){

            for (int i = 0; i < hitColliders.Length; i++)
            {
                if (hitColliders[i].tag == "PlayerSquad")
                {
                    Debug.Log("Found an enemy!\n");
                    return false;
                }
            }
        }

        enemies.Clear();
        formation.hastarget = false;
        formation.isOnContact = false;
        
        return true;

    }

    //TRANSITION from ATTACK to DEAD
    public bool IsDead() {

        if (formation.getHealth() <= 0) {

            return true;
        }
        return false;
    }

    //TRANSITION from RETURN to ATTACK
    public bool NearEnemy() {
       
       Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackrange);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].tag == "PlayerSquad")
            {
                //Debug.Log("Trovato nemico mentro tornavo al punto");
                enemies.Add(hitColliders[i].gameObject);
                formation.enemy_squad = hitColliders[i].gameObject;
                return true;
            }
        }
        formation.enemy_squad = null;
        formation.hastarget = false;
        formation.isOnContact = false;
        return false;
       
    }


    //TRANSITION from RETURN to IDLE
    public bool InitialPosition() {
    
        if ((formation.defendposition.transform.position - transform.position).magnitude < 2f) {
            //Debug.Log("Sono tornato alla postazione \n");
            return true;
        }
        return false;
    }


    // ACTIONS

    //enter action for IDLE
    public void GoToCamp()
    {
        //Debug.Log(transform + ": In position");
        formation.isDefending = true;
    }

    //stay action for IDLE
    public void Watch()
    {
        Debug.Log("Watch\n");
    }

    //exit action for IDLE
    public void ExitIdleState()
    {
        formation.isDefending = false;
    }

    //enter action for CHASE
    public void StartChase()
    {
        //Debug.Log(transform +  ": Chasing" + ", enemies count: " + enemies.Count);
        if(enemies.Count > 0)
        {
            random = Random.Range(0, enemies.Count);
            formation.enemy_squad = enemies[random];
            formation.hastarget = true;
        }
        else
        {
            Debug.Log("No more enemies\n");
        }  
    }

    //stay action for CHASE
    public void Chase() 
    {
       
       Debug.Log("Chase\n");
    }

    public void StopChase()
    {
        formation.hastarget = false;
    }
    
    //enter action for ATTACK
    public void StartAttack()
    {     
        //Debug.Log(transform + ": Started Attacking");
        formation.isOnContact = true;
    }

    //stay action for ATTACK
    public void Fight()
    {
        Debug.Log("Fight\n");
    }

    //exit action for ATTACK
    public void StopAttack()
    {
        formation.enemy_squad = null;
        formation.isOnContact = false;
    }

    //enter action in DEAD
    public void Dead()
    {
        Debug.Log(transform + ": Dead!\n");
        Destroy(gameObject);
    }

    //enter action for RETURN
    public void ReturnToPosition()
    {
        formation.hastarget = false;
        formation.isDefending = true;

    }

    //stay action for RETURN
    public void Returning()
    {
        Debug.Log("Return in position\n");
    }

    //exit action for RETURN
    public void StopReturning()
    {
        formation.isDefending = false;
    }

}
