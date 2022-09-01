using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB : MonoBehaviour
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }
    public static Dictionary<ConditionId, Condition> Conditions { get; set; } = new Dictionary<ConditionId, Condition>()
    {
        {ConditionId.psn,
        new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChange.Enqueue($"{pokemon.Base.Name} hurt itself due to poison");
                }
            }
        },

        {ConditionId.brn,
        new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 16);
                    pokemon.StatusChange.Enqueue($"{pokemon.Base.Name} hurt itself due to burn");
                }
            }
        },
         {ConditionId.par,
        new Condition()
            {
                Name = "Paralize",
                StartMessage = "has been paralized",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChange.Enqueue($"{pokemon.Base.Name}'s paralized and can't move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {ConditionId.frz,
        new Condition()
            {
                Name = "Freeze",
                StartMessage = "has been frozen",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChange.Enqueue($"{pokemon.Base.Name}'s not frozen anymore");
                        return true;
                    }
                    return false;
                }
            }
        },
        {ConditionId.slp,
        new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Pokemon pokemon) =>
                {
                    //sleep for 1-4 turns
                    pokemon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"will be asleep for {pokemon.StatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {

                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChange.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }
                    pokemon.StatusTime--;
                    pokemon.StatusChange.Enqueue($"{pokemon.Base.Name} is sleeping");
                    return false;
                }
            }
        },
        //Volatile Status

        {ConditionId.confusion,
        new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Pokemon pokemon) =>
                {
                    //Fica confuso entre 1 a 4 turnos
                    pokemon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"will be confused for {pokemon.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    //Verifica quanto tempo falta para acabar a confusão
                    if(pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChange.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    //caso ainda falte tempo, reduz um turno
                    pokemon.VolatileStatusTime--;
                    pokemon.StatusChange.Enqueue($"{pokemon.Base.Name} is confused");
                    //verifica se consegue atacar
                    if(Random.Range(1, 3) == 1)
                        return true;
                    else
                    {
                        // caso não consiga
                        //se bate na confusão
                        pokemon.DecreaseHP(pokemon.MaxHp / 8);
                        pokemon.StatusChange.Enqueue($"It hurt itself due to confusion");
                        return false;
                    }
                    
                }
            }
        }
    };

    public static float GetStatusBonus(Condition condition)
    {
        if(condition == null)
            return 1f;
        else if (condition.Id == ConditionId.slp || condition.Id == ConditionId.frz)
            return 2f;
        else if (condition.Id == ConditionId.par || condition.Id == ConditionId.psn  || condition.Id == ConditionId.brn)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionId
{
    none, psn, brn, slp, par, frz,
    confusion
}