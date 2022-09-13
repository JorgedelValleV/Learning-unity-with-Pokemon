using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Pokemon",menuName = "Pokemon/Create a new pokemon")]

public class PokemonBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] PokemonNature nature;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get { return name; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public string Description
    {
        get { return description; }
    }

    public int MaxHp
    {
        get { return maxHp* (int)NatureChart.GetHPChange(Nature); }
    }

    public int Attack
    {
        get { return attack * (int)NatureChart.GetATChange(Nature); }
    }

    public int SpAttack
    {
        get { return spAttack * (int)NatureChart.GetSAChange(Nature); }
    }

    public int Defense
    {
        get { return defense * (int)NatureChart.GetDFChange(Nature); }
    }

    public int SpDefense
    {
        get { return spDefense * (int)NatureChart.GetSDChange(Nature); }
    }

    public int Speed
    {
        get { return speed * (int)NatureChart.GetSPChange(Nature); }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public PokemonNature Nature
    {
        get { return nature; }
    }
    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}
[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base{
        get { return moveBase; }
    }
    public int Level{
        get { return level; }
    }
}
public enum Stat
{
    Attack,Defense,SpAttack,SpDefense,Speed,
    //these 2 are not actual stats, they are used to boost the moveAccuracy
    Accuracy,Evasion
}

public enum PokemonType
{
    None,Normal,Fire,Water,Electric,Grass,Ice,Fighting,Poison,Ground,Flying, Psychic,Bug,Rock,Ghost,Dragon,Dark,Steel,Fairy
}
public class TypeChart
{
    static float[][] chart =
    {
        //Has to be same order as PokemonType class
        //                       Nor   Fir   Wat   Ele   Gra   Ice   Fig   Poi   Gro   Fly   Psy   Bug   Roc   Gho   Dra   Dar  Ste    Fai
        /*Normal*/  new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 0,    1f,   1f,   0.5f, 1f},
        /*Fire*/    new float[] {1f,   0.5f, 0.5f, 1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 1f,   2f,   1f},
        /*Water*/   new float[] {1f,   2f,   0.5f, 1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   1f,   1f},
        /*Electric*/new float[] {1f,   1f,   2f,   0.5f, 0.5f, 1f,   1f,   1f,   0f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f},
        /*Grass*/   new float[] {1f,   0.5f, 2f,   1f,   0.5f, 1f,   1f,   0.5f, 2f,   0.5f, 1f,   0.5f, 2f,   1f,   0.5f, 1f,   0.5f, 1f},
        /*Ice*/     new float[] {1f,   0.5f, 0.5f, 1f,   2f,   0.5f, 1f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f},
        /*Fighting*/new float[] {2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f, 0.5f, 0.5f, 2f,   0f,   1f,   2f,   2f,   0.5f},
        /*Poison*/  new float[] {1f,   1f,   1f,   1f,   2f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   0f,   2f},
        /*Ground*/  new float[] {1f,   2f,   1f,   2f,   0.5f, 1f,   1f,   2f,   1f,   0f,   1f,   0.5f, 2f,   1f,   1f,   1f,   2f,   1f},
        /*Flying*/  new float[] {1f,   1f,   1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 1f},
        /*Psychic*/ new float[] {1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f,   0.5f, 1f,   1f,   1f,   1f,   0f,   0.5f, 1f},
        /*Bug*/     new float[] {1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 0.5f, 1f,   0.5f, 2f,   1f,   1f,   0.5f, 1f,   2f,   0.5f, 0.5f},
        /*Rock*/    new float[] {1f,   2f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f},
        /*Ghost*/   new float[] {0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 1f,   1f},
        /*Dragon*/  new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 0f},
        /*Dark*/    new float[] {1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f},
        /*Steel*/   new float[] {1f,   0.5f, 0.5f, 0.5f, 1f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 2f},
        /*Fairy*/   new float[] {1f,   0.5f, 1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   0.5f, 1f }
    };
    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;
        int row = (int)attackType;
        int col = (int)defenseType;
        return chart[--row][--col];
    }
}
public enum PokemonNature
{
    Quirky,Hardy, Lonely, Brave, Adamant, Naughty, Bold, Docile, Relaxed, Impish, Lax, Timid, Hasty, Serious, Jolly, Naive, Modest, Mild, Quiet, Bashful, Rash, Calm, Gentle, Sassy, Careful
 };

public class NatureChart
{
    static float[][] chart =
    {
        //Has to be same order as PokemonType class
        //                       Hp     Attk    Dfs     SpAt    SpDf    Spd   
        /*Quirky*/ new float[] {1,     1,      1,      1,      1,      1},
        /*Hardy*/  new float[] {1,     1,      1,      1,      1,      1},
        /*Lonely*/ new float[] {1,     1.1f,   0.9f,   1,      1,      1},
        /*Brave*/  new float[] {1,     1.1f,   1,      1,      1,      0.9f},
        /*Adamant*/new float[] {1,     1.1f,   1,      0.9f,   1,      1},
        /*Naughty*/new float[] {1,     1.1f,   1,      1,      0.9f,   1},
        /*Bold*/   new float[] {1,     0.9f,   1.1f,   1,      1,      1},
        /*Docile*/ new float[] {1,     1,      1,      1,      1,      1},
        /*Relaxed*/new float[] {1,     1,      1.1f,   1,      1,      0.9f},
        /*Impish*/ new float[] {1,     1,      1.1f,   0.9f,   1,      1},
        /*Lax*/    new float[] {1,     1,      1.1f,   1,      0.9f,   1},
        /*Timid*/  new float[] {1,     0.9f,   1,      1,      1,      1.1f},
        /*Hasty*/  new float[] {1,     1,      0.9f,   1,      1,      1.1f},
        /*Serious*/new float[] {1,     1,      1,      1,      1,      1},
        /*Jolly*/  new float[] {1,     1,      1,      0.9f,   1,      1.1f},
        /*Naive*/  new float[] {1,     1,      1,      1,      0.9f,   1.1f},
        /*Modest*/ new float[] {1,     0.9f,   1,      1.1f,   1,      1},
        /*Mild*/   new float[] {1,     1,      0.9f,   1.1f,   1,      1},
        /*Quiet*/  new float[] {1,     1,      1,      1.1f,   1,      0.9f},
        /*Bashful*/new float[] {1,     1,      1,      1,      1,      1},
        /*Rash*/   new float[] {1,     1,      1,      1.1f,   0.9f,   1},
        /*Calm*/   new float[] {1,     0.9f,   1,      1,      1.1f,   1},
        /*Gentle*/ new float[] {1,     1,      0.9f,   1,      1.1f,   1},
        /*Sassy*/  new float[] {1,     1,      1,      1,      1.1f,   0.9f},
        /*Careful*/new float[] {1,     1,      1,      0.9f,   1.1f,   1}
    };
    public static float GetHPChange(PokemonNature nature)
    {
        int row = (int)nature;
        return chart[row][0];
    }
    public static float GetATChange(PokemonNature nature)
    {
        int row = (int)nature;
        return chart[row][1];
    }
    public static float GetDFChange(PokemonNature nature)
    {
        int row = (int)nature;
        return chart[row][2];
    }
    public static float GetSAChange(PokemonNature nature)
    {
        int row = (int)nature;
        return chart[row][3];
    }
    public static float GetSDChange(PokemonNature nature)
    {
        int row = (int)nature;
        return chart[row][4];
    }
    public static float GetSPChange(PokemonNature nature)
    {
        int row = (int)nature;
        return chart[row][5];
    }
}