using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionSquad : MonoBehaviour {

    public List<GameObject> squads;
   
    void Update () {

        try
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {

                switch (EventSystem.current.currentSelectedGameObject.name)
                {

                    case "NameSquad1":

                        squads[0].GetComponent<PlayerFormationManager>().isSelected = true;
                        squads[0].GetComponent<PlayerFormationManager>().showSelectionSprites(true);

                        deselectOtherSquads(0);

                        break;

                    case "NameSquad2":

                        squads[1].GetComponent<PlayerFormationManager>().isSelected = true;
                        squads[1].GetComponent<PlayerFormationManager>().showSelectionSprites(true);

                        deselectOtherSquads(1);

                        break;

                    case "NameSquad3":

                        squads[2].GetComponent<PlayerFormationManager>().isSelected = true;
                        squads[2].GetComponent<PlayerFormationManager>().showSelectionSprites(true);

                        deselectOtherSquads(2);

                        break;

                    case "NameSquad4":

                        squads[3].GetComponent<PlayerFormationManager>().isSelected = true;
                        squads[3].GetComponent<PlayerFormationManager>().showSelectionSprites(true);

                        deselectOtherSquads(3);

                        break;

                }

            }
        }
        catch(NullReferenceException ex)
        {
            Debug.Log("Click on the squad name to select it");
        }


    }

    //Deselect other squads, while select current squad
    private void deselectOtherSquads(int currentSquad) {

        for (int i = 0; i < squads.Count; i++) {

            if (squads[i] != null && i != currentSquad) {
                squads[i].GetComponent<PlayerFormationManager>().isSelected = false;
                squads[i].GetComponent<PlayerFormationManager>().showSelectionSprites(false);
            }
        }

    }
}
