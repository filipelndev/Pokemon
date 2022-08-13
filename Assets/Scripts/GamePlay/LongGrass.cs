using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTrigger
{
    public int BattleDificult;

    public void OnPlayerTriggered(PlayerController player)
    {
        if(UnityEngine.Random.Range(1, 101) <= BattleDificult)
            {
                player.Character.Animator.IsMoving = false;
                GameController.Instance.StartBattle();
            }
    }
}
