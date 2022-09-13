using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Menu,Battle}
public class GameController : MonoBehaviour
{
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] GameObject playerController;
    [SerializeField] GameObject trainerController;
    [SerializeField] GameObject practiceController;
    [SerializeField] GameObject menu;
    [SerializeField] Camera worldCamera;

    GameState state;

    private void Awake()
    {
        ConditionsDB.Init();
    }
    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;
    }
    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        menu.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = practiceController.GetComponent<MapArea>().GetRandomWildPokemon();

        playerParty.Reset();

        battleSystem.StartBattle(playerParty, wildPokemon);
    }
    public void StartTrainerBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        menu.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainerController.GetComponent<PokemonParty>();

        playerParty.Reset();
        trainerParty.Reset();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }
    void EndBattle(bool won)
    {
        state = GameState.Menu;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        menu.SetActive(true);
    }
    private void Update()
    {
        if(state== GameState.Battle)
            battleSystem.HandleUpdate();
    }
}
