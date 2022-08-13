using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;
    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //Basic Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if(growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if( growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if(growthRate == GrowthRate.MediumSlow)
        {
            return (6 / 5 * (level * level * level)) - 15 * (level * level) + 100 * level - 140;
        }
        else if(growthRate == GrowthRate.Slow)
        {
            return (5 * (level * level * level)) / 4;
        }
        return -1;
    }

    public string GetName()
{
    return name;
}

    public int MaxHp
    {
        get { return maxHp;}
    }
    public int Attack
    {
        get { return attack;}
    }
    public int Defense
    {
        get { return defense;}
    }
    public int SpAttack
    {
        get { return spAttack;}
    }
    public int SpDefense
    {
        get { return spDefense;}
    }
    public int Speed
    {
        get { return speed;}
    }
    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }
    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

     public int CatchRate => catchRate;
    
    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate;

}
    [System.Serializable]
    public class LearnableMove
    {
        [SerializeField] MoveBase moveBase;
        [SerializeField] int level;

        public MoveBase Base {
            get { return moveBase; }
        }
        public int Level {
            get { return level; }
        }
    }

    public enum PokemonType
    {
        None,
        Normal,
        Fire,
        Water,
        Eletric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dragon
    }

    public enum GrowthRate
    {
        Fast,
        MediumFast,
        MediumSlow,
        Slow
    }

    public enum Stat
    {
        Attack,
        Defense,
        SpAttack,
        SpDefense,
        Speed,

        Accuracy,
        Evasion
    }

    public class TypeChart
    {
        static float[][] chart = 
        {
                        //              NOR FIR WAT ELE GRA ICE FIG POI GRO FLY PSY BUG ROC  GHO DRA DAR  STE   FAI
            /*Normal*/  new float[]    {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f, 1f, 1f, 0.5f, 1f},

                       //              NOR  FIR   WAT   ELE GRA ICE FIG POI GRO FLY PSY BUG  ROC  GHO   DRA DAR STE FAI
            /*Fire*/  new float[]      {1f, 0.5f, 0.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f,  0.5f, 1f, 2f, 1f},

                         //            NOR FIR  WAT  ELE  GRA  ICE FIG POI GRO FLY PSY BUG ROC  GHO DRA  DAR STE FAI
            /*Water*/  new float[]     {1f, 2f, 0.5f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f, 1f},
            
                       //              NOR FIR WAT  ELE   GRA  ICE FIG POI GRO FLY PSY BUG ROC  GHO  DRA DAR  STE FAI
            /*Eletric*/  new float[]   {1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f},

                        //             NOR   FIR WAT ELE  GRA  ICE FIG   POI GRO   FLY PSY   BUG ROC GHO  DRA  DAR  STE   FAI
            /*Grass*/  new float[]     {1f, 0.5f, 1f, 1f, 0.5f, 1f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, 1f, 0.5f, 1f, 0.5f, 1f},

                         //            NOR   FIR   WAT ELE GRA   ICE FIG POI GRO FLY PSY BUG ROC  GHO DRA DAR  STE FAI
            /*Ice*/  new float[]       {1f, 0.5f, 0.5f, 1f, 2f, 0.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f},

                       //              NOR FIR WAT ELE GRA ICE FIG   POI GRO   FLY   PSY  BUG  ROC GHO DRA DAR STE  FAI
            /*Fighting*/  new float[]  {2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0f, 1f, 2f, 2f, 0.5f},

                       //              NOR FIR WAT ELE GRA ICE FIG   POI   GRO FLY PSY BUG   ROC  GHO  DRA DAR  STE FAI
            /*Poison*/  new float[]    {1f, 1f, 1f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 0f, 2f},

                       //              NOR FIR WAT ELE  GRA  ICE FIG POI GRO FLY PSY  BUG  ROC  GHO DRA DAR STE FAI
            /*Ground*/  new float[]    {1f, 2f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 0f, 1f, 0.5f, 2f, 1f, 1f, 1f, 2f, 1f},

                       //              NOR FIR WAT   ELE GRA ICE FIG POI GRO FLY PSY BUG  ROC  GHO DRA DAR  STE  FAI
            /*Flying*/  new float[]    {1f, 1f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 1f},

                       //              NOR FIR WAT ELE GRA ICE FIG POI GRO FLY  PSY BUG ROC  GHO DRA DAR  STE   FAI
            /*Psychic*/  new float[]   {1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 0f, 0.5f, 1f},

                       //              NOR   FIR WAT ELE GRA ICE   FIG   POI GRO   FLY PSY BUG ROC  GHO  DRA DAR  STE   FAI
            /*Bug*/  new float[]       {1f, 0.5f, 1f, 1f, 2f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 0.5f, 1f, 2f, 0.5f, 0.5f},

                       //              NOR FIR WAT ELE GRA ICE   FIG POI   GRO FLY PSY BUG ROC  GHO DRA DAR  STE FAI
            /*Rock*/  new float[]      {1f, 2f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f},

                       //              NOR FIR WAT ELE GRA ICE FIG POI GRO FLY PSY BUG ROC GHO DRA  DAR  STE FAI
            /*Ghost*/  new float[]     {0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f},

                       //              NOR FIR WAT ELE GRA ICE FIG POI GRO FLY PSY BUG ROC  GHO DRA DAR STE  FAI
            /*Dragon*/  new float[]    {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 0f},

                       //              NOR FIR WAT ELE GRA ICE   FIG POI GRO FLY PSY BUG ROC  GHO DRA DAR STE  FAI
            /*Dark*/  new float[]      {1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f},

                       //              NOR   FIR   WAT   ELE GRA ICE FIG POI GRO FLY PSY BUG ROC  GHO DRA DAR STE  FAI
            /*Steel*/  new float[]     {1f, 0.5f, 0.5f, 0.5f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 2f},

                       //              NOR   FIR WAT ELE GRA ICE FIG   POI GRO FLY PSY BUG ROC  GHO DRA DAR STE  FAI
            /*Fairy*/  new float[]     {1f, 0.5f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 0.5f, 1f}
        };
        public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
        {
            if(attackType == PokemonType.None || defenseType == PokemonType.None)
                return 1;

                int row = (int)attackType - 1;
                int col = (int)defenseType - 1;

                return chart[row][col];
        }
    }