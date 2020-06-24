using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMTurret : MonoBehaviour {

    [Header("Attack Range")]
    [Range(0f, 20f)] public float attackRange;

    [Header("Alarm Position")]
    public Transform alarmPosition;

    [Header("Alarm range")]
    [Range(4f, 12f)] public float alarmRange;

    public float reactionTime;

    private Vector3 viewFromGround; 

    private FSM fsm;
    private TurretBehaviour turret;
    private List<GameObject> enemies = new List<GameObject>();

    void Start(){

        turret = GetComponent<TurretBehaviour>();
        viewFromGround = new Vector3(transform.position.x, 0f, transform.position.z); //for overlap spheres

        //States
        FSMState idle = new FSMState();
        idle.enterActions.Add(Wait);

        FSMState active = new FSMState();
        active.enterActions.Add(Alerted);
        active.exitActions.Add(StopAlert);

        FSMState attack = new FSMState();
        attack.enterActions.Add(StartAttack);
        attack.stayActions.Add(Attack);
        attack.exitActions.Add(StopAttack);

        //Transitions
        FSMTransition t1 = new FSMTransition(PatrolInCamp);
        FSMTransition t2 = new FSMTransition(NobodyOnAlarm);
        FSMTransition t3 = new FSMTransition(EnemiesInRange);
        FSMTransition t4 = new FSMTransition(NoEnemiesInRange);

        // Link states with transitions
        idle.AddTransition(t1, active);
        active.AddTransition(t2, idle);
        active.AddTransition(t3, attack);
        attack.AddTransition(t4, active);

        //Setup a FSA at initial state
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
            t.localScale = new Vector3(attackRange / transform.localScale.x, 1f, attackRange / transform.localScale.z) / 5f;
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

    //CONDITIONS

    //TRANSITION from IDLE to ACTIVE
    public bool PatrolInCamp(){

        Collider[] hitColliders = Physics.OverlapSphere(alarmPosition.position, alarmRange);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject.tag == "NPC"){  
                return true;   
            }
            i++;
        }
        return false;
    }


    //TRANSITION from ACTIVE to IDLE
    public bool NobodyOnAlarm(){

        Collider[] hitColliders = Physics.OverlapSphere(alarmPosition.position, alarmRange);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject.tag == "NPC"){
                return false;
            }

            i++;
        }
        return true;
    }


    //TRANSITION from ACTIVE to ATTACK
    public bool EnemiesInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(viewFromGround, attackRange);

        foreach(Collider col in hitColliders)
        {
            if(col.tag == "PlayerSquad")
            {
                return true;
            }
        }
        return false;
    }

    //TRANSITION from ATTACK to ACTIVE
    public bool NoEnemiesInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(viewFromGround, attackRange);

        foreach (Collider col in hitColliders)
        {
            if (col.tag == "PlayerSquad")
            {
                return false;
            }
        }
        return true;
    }


    //ACTIONS

    //enter action for IDLE
    public void Wait()
    {
        turret.isAlerted(false);
        turret.isAlertedAndAttacking(false);
    }

    //enter action for ACTIVE
    public void Alerted()
    {
        turret.isAlerted(true);
    }

    //exit action for ACTIVE
    public void StopAlert()
    {
        turret.isAlerted(false);
    }

    //enter action for ATTACK
    public void StartAttack()
    {
        turret.isAlertedAndAttacking(true);
        turret.startAttack();
    }

    //stay action for ATTACK
    public void Attack()
    {
        Debug.Log("Attacking the enemy squad");
    }

    //exit action for ATTACK
    public void StopAttack()
    {
        turret.isAlertedAndAttacking(false);
        turret.stopAttack();
    }
}
