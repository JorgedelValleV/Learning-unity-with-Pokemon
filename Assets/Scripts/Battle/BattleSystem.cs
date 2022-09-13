using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start,ActionSelection,MoveSelection,RunningTurn,Busy,PartyScreen,BattleOver}
public enum BattleAction { Move,SwitchPokemon,UseItem,Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetUpBattle());
    }
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;

        player = playerParty.GetComponentInParent<PlayerController>();
        trainer=trainerParty.GetComponentInParent<TrainerController>();

        StartCoroutine(SetUpBattle());
    }
    public IEnumerator SetUpBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();
        if (!isTrainerBattle)
        {
            // Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared.");
        }
        else
        {
            // Trainer Battle
            //Show trainer and player sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            // Send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);

            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyPokemon.Base.Name}");

            // Send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);

            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        }

        partyScreen.Init();
        ActionSelection();
    }
    IEnumerator BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        if (isTrainerBattle)
        {
            PlayEndTrainer();
            if (won)
                yield return dialogBox.TypeDialog($"Congrats!! You won the battle.");
            else
                yield return dialogBox.TypeDialog($"{trainer.Name} won the battle.");
        }
        else
        {
            if (won)
                yield return dialogBox.TypeDialog($"Congrats!! You won the battle.");
            else
                yield return dialogBox.TypeDialog($"You run without problem.");
        }
        yield return new WaitForSeconds(1f);
        playerUnit.gameObject.SetActive(true);
        enemyUnit.gameObject.SetActive(true);
        playerImage.gameObject.SetActive(false);
        trainerImage.gameObject.SetActive(false);
        OnBattleOver(won);
    }
    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog($"Choose an action.");
        dialogBox.EnableActionSelector(true);
    }
    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        prevState = state;
        if (playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove(playerUnit.Pokemon);

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //check who goes first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if(enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;
            var secondPokemon = secondUnit.Pokemon;

            //First Unit
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            if (state == BattleState.BattleOver) yield break;

            if (secondPokemon.HP > 0)
            {
                //Second Unit
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                if (state == BattleState.BattleOver) yield break;
                yield return RunAfterTurn(secondUnit);
            }
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;
        }
        else //Switch 
        {
            if(playerAction == BattleAction.SwitchPokemon)
            {
                var selectedMember = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                currentMember = 0;
                yield return SwitchPokemon(selectedMember);
            }
            //enemy turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove(playerUnit.Pokemon);
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            if (state == BattleState.BattleOver) yield break;
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
            yield return RunAfterTurn(playerUnit);
        }
        if (state != BattleState.BattleOver)
            ActionSelection();
    }
    IEnumerator RunMove(BattleUnit sourceUnit,BattleUnit targetUnit,Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}.");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon)){
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(0.5f);
            targetUnit.PlayHitAnimation();

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Secondaries!=null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    if(UnityEngine.Random.Range(1, 101) <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon,secondary.Target);
                }
            }
            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted.");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name.ToUpper()}'s attack missed.");
        }
    }
    IEnumerator RunMoveEffects(MoveEffects effects,Pokemon source,Pokemon target,MoveTarget moveTarget)
    {
        // Stat Boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoost(effects.Boosts);
            else
                target.ApplyBoost(effects.Boosts);
        }
        //Status condition
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }
        //Volatile Status condition
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }
    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        // Statuses like brn or psn will hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted.");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }
    bool CheckIfMoveHits(Move move,Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits) return true;

        float moveAccuraccy = move.Base.Accuracy;

        int accuracy = source.StatBoost[Stat.Accuracy];
        int evasion = source.StatBoost[Stat.Evasion];


        var boostValues = new float[] { 1f, 4f/3, 5f/3, 2f, 7f/3, 8f/3, 3f };
        if (accuracy >= 0)
            moveAccuraccy *=boostValues[accuracy];
        else
            moveAccuraccy /= boostValues[-accuracy];

        if (evasion >= 0)
            moveAccuraccy /= boostValues[evasion];
        else
            moveAccuraccy *= boostValues[evasion];

        return UnityEngine.Random.Range(0, 101) <= moveAccuraccy;

    }
    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                StartCoroutine(BattleOver(false));
        }
        else
        {
            if (!isTrainerBattle)
                StartCoroutine(BattleOver(true));
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(SendNextTrainerPokemon(nextPokemon));
                }
                else
                    StartCoroutine(BattleOver(true));
            }
        }
            

    }
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical>1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not effective!");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
            HandleActionSelection();
        else if (state == BattleState.MoveSelection)
            HandleMoveSelection();
        else if (state == BattleState.PartyScreen)
            HandlePartySelection();
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 2)
                currentAction += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 1)
                currentAction -= 2;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentAction % 2 == 1)
                --currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentAction % 2 == 0)
                ++currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
            }
            else if (currentAction == 2)
            {
                //Party
                prevState = state;
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                currentAction = 0;
                StartCoroutine(BattleOver(false));
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1 && currentMove % 2 == 0)
                ++currentMove;

        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove % 2 == 1)
                --currentMove;

        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }

        if (currentMove < playerUnit.Pokemon.Moves.Count)
            dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSwitch();
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (prevState != BattleState.RunningTurn)
            {
                prevState = null;
                partyScreen.gameObject.SetActive(false);
                ActionSelection();
            }
        }
    }
    void HandleSwitch()
    {
        var selectedMember = playerParty.Pokemons[currentMember];
        if (selectedMember.HP <= 0)
        {
            partyScreen.SetMessageText("You can't send out a fainted Pokemon");
            return;
        }
        else if (selectedMember == playerUnit.Pokemon)
        {
            partyScreen.SetMessageText("You can't switch with the same Pokemon");
            return;
        }
        partyScreen.gameObject.SetActive(false);
        if (prevState == BattleState.ActionSelection)
        {
            prevState = null;
            StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
        }
        else
        {
            state = BattleState.Busy;
            currentMember = 0;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
    }
    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        playerUnit.Pokemon.ResetStatBoost();
        var swtich = playerUnit.Pokemon.HP > 0;
        if (swtich)
        {
            currentAction = 0;
            currentMove = 0;
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1f);
        }
        
        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        state = BattleState.RunningTurn;
    }
    IEnumerator SendNextTrainerPokemon(Pokemon nextPokemon)
    {
        state = BattleState.Busy;

        playerUnit.Pokemon.ResetStatBoost();

        var trainerOriginalPos = trainerImage.transform.localPosition;
        var sequence = DOTween.Sequence();
        sequence.Append(trainerImage.transform.DOLocalMoveX(trainerOriginalPos.x+500f, 0.25f));
        yield return new WaitForSeconds(0.25f);
        trainerImage.gameObject.SetActive(true);
        enemyUnit.gameObject.SetActive(false);

        sequence.Append(trainerImage.transform.DOLocalMoveX(trainerOriginalPos.x, 0.4f));

        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextPokemon.Base.Name}!");
        enemyUnit.Setup(nextPokemon);

        sequence.Append(trainerImage.transform.DOLocalMoveX(trainerOriginalPos.x + 500f, 0.4f));
        yield return new WaitForSeconds(0.25f);
        trainerImage.gameObject.SetActive(false);
        enemyUnit.gameObject.SetActive(true);
        sequence.Append(trainerImage.transform.DOLocalMoveX(trainerOriginalPos.x, 0.25f));

        state = BattleState.RunningTurn;
    }
    void PlayEndTrainer()
    {
        currentAction = 0;
        currentMove = 0;
        playerUnit.Clear();
        enemyUnit.Clear();
        trainerImage.gameObject.SetActive(true);
        enemyUnit.gameObject.SetActive(false);
        playerImage.gameObject.SetActive(true);
        playerUnit.gameObject.SetActive(false);

        var trainerOriginalPos = trainerImage.transform.localPosition;
        var playerOriginalPos = playerImage.transform.localPosition;
        trainerImage.transform.localPosition = new Vector3(500f, trainerOriginalPos.y);
        playerImage.transform.localPosition = new Vector3(-500f, playerOriginalPos.y);
        var sequence = DOTween.Sequence();
        sequence.Append(trainerImage.transform.DOLocalMoveX(trainerOriginalPos.x, 0.5f));
        sequence.Join(playerImage.transform.DOLocalMoveX(playerOriginalPos.x, 0.5f));

        isTrainerBattle = false;
    }
}
