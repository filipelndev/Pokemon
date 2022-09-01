using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Recovery Item")]
public class RecoveryItem : ItemBase
{   [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;
    
    [Header("STATUS")]
    [SerializeField] ConditionId status;
    [SerializeField] bool recoverAllStatus;
    
    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon pokemon)
    {
        if(hpAmount > 0)
        {
            if(pokemon.HP == pokemon.MaxHp)
                return false;
            
            pokemon.IncreaseHP(hpAmount);
        }

        return true;
    }
}
