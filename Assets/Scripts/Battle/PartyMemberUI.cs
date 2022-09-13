using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Sprite aliveSprite;
    [SerializeField] Sprite deadSprite;

    [SerializeField] Color highlightedColor;

    Pokemon _pokemon;

    Image image;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        image = GetComponent<Image>();

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / (float)pokemon.MaxHp);

        if(pokemon.HP>0)
            image.sprite = aliveSprite;
        else
            image.sprite = deadSprite;
    }
    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = highlightedColor;
        else
            nameText.color = Color.black;
    }
}
