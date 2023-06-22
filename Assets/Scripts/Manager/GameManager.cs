using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public float defaultSpeed = 1000f;

    float timer = 0f;
    public float Timer { get => timer; }

    public int gold = 0;


    public int startCardNumber = 6;
    public CardDeckController cardDeckController;
    public GameSpeedController speedController;
    public WaveController waveController;
    public MapBuilder mapBuilder;
    public Canvas cameraCanvas;

    public int king_Hp = 20;
    private bool dailyIncome = true;

    private int curWave = 0; 
    public int CurWave { get => curWave; }

    public bool isInBattle = false;

    public List<Adventurer> adventurersList = new List<Adventurer>();

    public List<Trap> trapList = new List<Trap>();

    public float timeScale;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * defaultSpeed * timeScale;
        if(timer > 720f && dailyIncome)
        {
            gold += 200;

            dailyIncome = false;
        }

        if(timer > 1440f)
        {
            //재화수급
            gold += 200;
            timer = 0f;

            dailyIncome = true;

            //몬스터 웨이브 스폰
            curWave++;
            StartCoroutine(waveController.ISpawnWave(curWave));
        }
    }

    private void KingBattle()
    {

    }

    private void Start()
    {
        mapBuilder.Init();

        for (int i = 0; i < startCardNumber; i++)
        {
            cardDeckController.DrawCard();
        }

        speedController.SetSpeedZero();
        StartCoroutine(waveController.ISpawnWave(curWave));
    }
}
