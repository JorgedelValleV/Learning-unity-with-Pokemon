﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public PokemonBase Base
    {
        get { return _base; }
    }
    public int Level
    {
        get { return level; }
    }

    public int HP { get; set; }
    public List<Move> Moves { get; set; }
    public Move CurrentMove { get; set; }
    public Dictionary<Stat,int> Stats { get; private set; }
    public Dictionary<Stat, int> StatBoost { get; private set; }
    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }

    public Queue<string> StatusChanges { get; private set; } = new Queue<string>();
    public bool HpChanged { get; set; }
    public event System.Action OnStatusChanged;
    public void Init()
    {
        //Generate Moves
        Moves = new List<Move>();
        foreach( var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
            if (Moves.Count >= 4)
                break;
        }
        CalculateStats();
        HP = MaxHp;

        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
    }
    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f)+ Level + 10;
    }
    public void ResetStatBoost()
    {
        StatBoost = new Dictionary<Stat, int>()
        {
            {Stat.Attack,0 },
            {Stat.Defense,0 },
            {Stat.SpAttack,0 },
            {Stat.SpDefense,0 },
            {Stat.Speed,0 },
            {Stat.Accuracy,0 },
            {Stat.Evasion,0 }
        };
    }
    int GetStat(Stat stat)
    {
        int statval = Stats[stat];
        // apply boost
        int boost = StatBoost[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4 };

        if (boost >= 0)
            statval = Mathf.FloorToInt(statval * boostValues[boost]);
        else
            statval = Mathf.FloorToInt(statval / boostValues[-boost]);

        return statval;
    }
    public void ApplyBoost(List<StatBoost> statBoosts)
    {
        foreach(var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoost[stat] = Mathf.Clamp(StatBoost[stat] + boost,-6,6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose");
            else
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell");

            Debug.Log($"{stat} has been boosted to {StatBoost[stat]}");
        }
    }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }
    public int MaxHp { get; private set; }

    public DamageDetails TakeDamage(Move move,Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 1.5f;
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1)* TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        float stab = (move.Base.Type == attacker.Base.Type1 || move.Base.Type == attacker.Base.Type2) ? 1.5f : 1f;
        
        float attack= (move.Base.Category==MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f)*type*critical*stab;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }
    public void UpdateHP(int damage)
    {
        HP= Mathf.Clamp(HP-damage,0,MaxHp);
        HpChanged = true;
    }
    public void SetStatus(ConditionID conditionId)
    {
        if (Status != null)
        {
            StatusChanges.Enqueue($"{Base.Name} is already {Status.Name}");
            return;
        }
        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }
    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }
    public void SetVolatileStatus(ConditionID conditionId)
    {
        if (VolatileStatus != null)
        {
            StatusChanges.Enqueue($"{Base.Name} is already {VolatileStatus.Name}");
            return;
        }
        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }
    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }
    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }
    public Move GetRandomMove(Pokemon defender)
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        var ret = movesWithPP[0];
        int totalDamage = 0;
        int[] acum = new int[movesWithPP.Count];
        for (int i = 0; i < movesWithPP.Count; ++i)
        {
            int damage = defender.CalculateDamage(movesWithPP[i], this);
            totalDamage += damage;
            acum[i] = totalDamage;
        }
        //int r = Random.Range(0, totalDamage);
        int r = Mathf.RoundToInt(Random.Range(0,(float)totalDamage));
        for (int i = 0; i < movesWithPP.Count; ++i)
        {
            if (r <= acum[i])
            {
                ret = movesWithPP[i];
                break;
            }
        }
        return ret;
    }
    public int CalculateDamage(Move move, Pokemon attacker)
    {
        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense; ;

        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);
        float modifiers = Random.Range(0.85f, 1f)*type;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        return damage;
    }
    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
            if (!Status.OnBeforeMove(this))
                canPerformMove = false;

        if (VolatileStatus?.OnBeforeMove != null)
            if (!VolatileStatus.OnBeforeMove(this))
                canPerformMove = false;

        return canPerformMove;
    }
    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }
    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }
}
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
