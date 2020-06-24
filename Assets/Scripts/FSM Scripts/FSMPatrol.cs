using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMPatrol : MonoBehaviour {

    [Range(0f, 20f)] public float sight_range;
    [Range(0f, 20f)] public float attack_range;
    [Range(0f, 50f)] public float farFromPosition;

    [Range(0f, 20f)] public float rangeFromRecovery;

    public float reactionTime = 0f;

    private FSM fsm;
    private Vector3 chasingPoint;
    private EnemyFormationManager formation;
    private int random;

    private List<GameObject> enemies = new List<GameObject>();

    void Start(){

        formation = GetComponent<EnemyFormationManager>();

        //States
        FSMState patrol = new FSMState();
        patrol.enterActions.Add(SetGoal);
        patrol.stayActions.Add(Walk);
        patrol.exitActions.Add(StopPatrol);

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

        FSMState flee = new FSMState();
        flee.enterActions.Add(StartFleeing);
        flee.stayActions.Add(Flee);

        FSMState recoverHP = new FSMState();
        recoverHP.enterActions.Add(StartRecovery);
        recoverHP.stayActions.Add(RechargeHP);
        recoverHP.exitActions.Add(StopRecovery);

        //Transitions
        FSMTransition t1 = new FSMTransition(ScanField);
        FSMTransition t2 = new FSMTransition(ScanFieldMoreNear);
        FSMTransition t3 = new FSMTransition(OutOfLargeRange);
        FSMTransition t4 = new FSMTransition(OutOfRange);
        FSMTransition t5 = new FSMTransition(CompletedBattle);
        FSMTransition t6 = new FSMTransition(LowHp);
        FSMTransition t7 = new FSMTransition(IsDead);
        FSMTransition t8 = new FSMTransition(DeadDuringEscape);
        FSMTransition t9 = new FSMTransition(DistanceFromRecovery);
        FSMTransition t10 = new FSMTransition(CompletedRecharge);
        FSMTransition t11 = new FSMTransition(ChaseDuringRecharge);

        // Link states with transitions
        patrol.AddTransition(t1, chase);
        chase.AddTransition(t2, attack);
        chase.AddTransition(t3, patrol);
        attack.AddTransition(t4, chase);
        attack.AddTransition(t5, patrol);
        attack.AddTransition(t6, flee);
        attack.AddTransition(t7, dead);
        flee.AddTransition(t8, dead);
        flee.AddTransition(t9, recoverHP);
        recoverHP.AddTransition(t10, patrol);
        recoverHP.AddTransition(t11, chase );

        // Setup a FSA at initial state
        fsm = new FSM(patrol);

        // Start monitoring
        StartCoroutine(Patrol());
    }

    // GIMMICS
    private void OnValidate()
    {
        Transform t = transform.Find("Range");
        if (t != null)
        {
            t.localScale = new Vector3(sight_range / transform.localScale.x, 1f, sight_range / transform.localScale.z) / 5f;
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

    //TRANSITION from PATROL to CHASE
    public bool ScanField() {

        bool flagEnemy = false;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, sight_range);

        int i = 0;

        while(i < hitColliders.Length){

            if(hitColliders[i].gameObject.tag == "PlayerSquad"){

                enemies.Add(hitColliders[i].gameObject);
                chasingPoint = transform.position;

                Debug.Log("Found an enemy!\n");
                flagEnemy = true;
            }

            i++;
        }

        return flagEnemy;       
    }

    //TRANSITION from CHASE to ATTACK
    public bool ScanFieldMoreNear() {
    
        if(formation.enemy_squad!= null && (formation.enemy_squad.transform.position - transform.position).magnitude <= attack_range)
        {
            return true;
        }
        return false;

    }

    //TRANSITION from CHASE to PATROL
    public bool OutOfLargeRange() {
        
        if ((chasingPoint - transform.position).magnitude >= farFromPosition) {

            Debug.Log("I have gone too far: " + (chasingPoint - transform.position).magnitude+ "\n");
            formation.hastarget = false;
            formation.isOnContact = false;
            enemies.Clear();
            return true;
        }
        return false;
    }

    //TRANSITION from ATTACK to CHASE
    public bool OutOfRange() {
        
        if(formation.enemy_squad!= null && (formation.enemy_squad.transform.position - transform.position).magnitude > attack_range)
        {
            Debug.Log("Out of attack range\n");
            formation.isOnContact = false;
            return true;
        }

        return false;
    }

    //TRANSITION from ATTACK to PATROL
    public bool CompletedBattle(){

        if (formation.enemy_squad == null)
        {
            Debug.Log("Enemy defeated!\n");
            enemies.Clear();
            formation.isOnContact = false;
            formation.hastarget = false;
            return true;
        }
        return false;
    }

    //TRANSITION from ATTACK to FLEE
    public bool LowHp() {
        if(formation.getHealth() <= 3 && (formation.recovery.transform.position - transform.position).magnitude > rangeFromRecovery) {
            return true;
        }
       
        return false;
    }

    //TRANSITION from ATTACK to DEAD
    public bool IsDead(){
    
        if (formation.getHealth() <= 0) {        
            return true;
        }

        return false;

    }

    //TRANSITION from FLEE to DEAD
    public bool DeadDuringEscape() {
        if (formation.getHealth() <= 0) {
            return true;
        }
        return false;
    }

    //TRANSITION from FLEE to RECOVER
    public bool DistanceFromRecovery(){

        //Debug.Log("distance from recovery: " + (formation.recovery.transform.position - transform.position).magnitude);

        if ((formation.recovery.transform.position - transform.position).magnitude <= rangeFromRecovery) {

            Debug.Log("Arrived at recovery point\n");
            return true;
        }

        return false;
    }

    //TRANSITION from RECOVER to PATROL
    public bool CompletedRecharge() {

        if (formation.areFullHealthWarriors() && formation.getHealth() == formation.warriors.Length) {

            Debug.Log("Recharge completed!\n");
            formation.isFleeing = false;
            return true;
        }

        return false;
    }

    //TRANSITION from RECOVER to CHASE  
    public bool ChaseDuringRecharge() {

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attack_range);

        for(int i = 0; i < hitColliders.Length; i++) {

            if (hitColliders[i].transform.tag == "PlayerSquad") {
                formation.isFleeing = false;
                formation.isOnRecoveryPoint = false;
                enemies.Add(hitColliders[i].gameObject);
                return true;
            }
        }
        return false;
    }

    // ACTION

    //enter action for PATROL
    public void SetGoal() {
        //Debug.Log("Set goal, patrolling time!\n");
        formation.isPatrolling = true;
    }


    //stay for PATROL
    public void Walk() {
        Debug.Log("Walk\n");
    }

    //exit action of patrol
    public void StopPatrol()
    {
        formation.isPatrolling = false;
    }
    
    //enter action for CHASE
    //decide chi inseguire
    public void StartChase() {
        
        if(formation.enemy_squad == null)
        {
            random = Random.Range(0, enemies.Count);
            //Debug.Log("Start chase: " + enemies[random].name);
            formation.enemy_squad = enemies[random];
            chasingPoint = transform.position;
        }

        formation.hastarget = true;
    }

    //stay action for Chase
    public void Chase() {
        Debug.Log("Chase\n");
    }

    //exit action for Chase
    public void StopChase()
    {
        if(enemies.Count == 0)
        {
            formation.enemy_squad = null;
            formation.hastarget = false;
        }
    }


    //enter action for ATTACK
    public void StartAttack() {
        //Debug.Log("Start attack\n");
        formation.isOnContact = true;
    }

    //stay action for ATTACK
    public void Fight() {
        Debug.Log("Fight\n");   
    }

    //exit action for ATTACK
    public void StopAttack()
    {
        if(formation.enemy_squad == null)
        {
            enemies.Clear();
            formation.hastarget = false;
            formation.enemy_squad = null;
        }
        formation.isOnContact = false;
    }

    //enter action for Dead
    public void Dead() {

        Debug.Log(transform + ": Dead!\n");
        Destroy(gameObject);
    }

    //enter action of FLEE
    public void StartFleeing() {

        formation.hastarget = false;
        formation.isOnContact = false;
        formation.isFleeing = true;
    }

    //stay action of FLEE
    public void Flee()
    {
        Debug.Log("Fleeing from battlefield");
    }

    //enter action for RECOVERY
    public void StartRecovery() {

        formation.isOnRecoveryPoint = true;
        formation.isRecharging = true;
    }

    //stay action for RECOVERY
    public void RechargeHP() {
        Debug.Log("Recharging in progress");
    }

    //exit action from Recovery
    public void StopRecovery()
    {
        formation.isOnRecoveryPoint = false;
    }
}