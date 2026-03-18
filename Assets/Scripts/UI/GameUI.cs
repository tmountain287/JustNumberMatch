using Common.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class GameUI : BaseUI
{
    [SerializeField] private Image bg = null;
    [SerializeField] private List<Sprite> bgSpriteList = null;
    [SerializeField] private Text stageText = null; 

    [SerializeField] private Character character = null;

    [SerializeField] private GameObject block = null;

    [SerializeField] private Button exitButton = null;

    [SerializeField] private ItemStateBox goldStateBox = null;
    [SerializeField] private List<SkillItemButton> skillItemButtonList = null;

    [SerializeField] private GameTopUI gameTopUI = null;
    [SerializeField] private GameObject congrats = null;

    [SerializeField] private StageTextUI stageTextUI = null;

    [SerializeField] private List<AudioClip> bgmList = null;

    [SerializeField] private BgmPlayer bgmPlayer = null;

    [SerializeField] private ObjectPool objectPool;

    public GameTopUI GameTopUI { get => gameTopUI; }
    public StageTextUI StageTextUI { get => stageTextUI; }
    public ItemStateBox GoldStateBox { get => goldStateBox; }
    public List<SkillItemButton> SkillItemButtonList { get => skillItemButtonList; }

    public void InitState()
    {
        SetBlock(false);      
    }

    public void SetStage(GameModeType gameModeType, DifficultyType _difficultyType)
    {
        InitState();
        bg.sprite = bgSpriteList[(int)_difficultyType];
        bgmPlayer.InBgm = bgmList[(int)gameModeType];
        bgmPlayer.PlayBgm();
        //stageText.text = $"Statge {_stage}";
    }

    public void SetCharacter(ValidationResultType validationResultType, string segment = "")
    {
        character.SetConversation( validationResultType, segment);
    } 

    public void SetBlock(bool _flag)
    {
        block.SetActive(_flag);
    }

    public void OnCongrats()
    {
        congrats.SetActive(true);
    }
}