using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image x;
    private PlayerFormationManager player;
    private int num;

    private void Start()
    {
        player = GetComponent<PlayerFormationManager>();
        num = player.getHealth();
    }

    private void FixedUpdate(){

        if(player != null) {

            int currentWarriors = player.getHealth();

            //If the number of remained warriors is less than initial warriors, the life bar decreases
            if (currentWarriors < num){

                x.fillAmount -= 1f / player.warriors.Length;
                num = num - 1;
            }
        }
       

    }

}