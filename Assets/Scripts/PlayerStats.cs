using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private int hp;
    private int lemonsOwned;
    private int cactusNeedlesOwned;

    private void Start()
    {
        hp = 100;
        lemonsOwned = 0;
        cactusNeedlesOwned = 0;
        UpdateHp();
        UpdateLemons();
        UpdateCactusNeedles();
    }

    public int GetHp()
    {
        return hp;
    }

    public int GetLemonsOwned()
    {
        return lemonsOwned;
    }

    public int GetCactusNeedlesOwned()
    {
        return cactusNeedlesOwned;
    }

    public void AddLemon()
    {
        lemonsOwned++;
        UpdateLemons();
    }

    public void AddCactusNeedle()
    {
        cactusNeedlesOwned++;
        UpdateCactusNeedles();
    }

    public void TakeAwayLemons(int amount)
    {
        lemonsOwned -= amount;
        UpdateLemons();
    }

    public void TakeAwayCactusNeedles(int amount)
    {
        cactusNeedlesOwned -= amount;
        UpdateCactusNeedles();
    }

    public void IncreaseHpBy(int amount)
    {
        hp += amount;
        UpdateHp();
    }

    public void DecreaseHpBy(int amount)
    {
        hp -= amount;
        UpdateHp();
    }

    private void UpdateHp()
    {
        UpdateHpUI();
        UpdateHpServer();
    }

    private void UpdateLemons()
    {
        UpdateLemonUI();
        UpdateLemonServer();
    }

    private void UpdateCactusNeedles()
    {
        UpdateCactusNeedleUI();
        UpdateCactusNeedleServer();
    }

    private void UpdateHpUI()
    {
        uiManager.UpdateHpCounter(hp);
    }

    private void UpdateLemonUI()
    {
        uiManager.UpdateLemonCounter(lemonsOwned);
    }

    private void UpdateCactusNeedleUI()
    {
        uiManager.UpdateCactusNeedlesCounter(cactusNeedlesOwned);
    }

    private void UpdateHpServer()
    {
        PlayerData.hp = hp;
    }

    private void UpdateLemonServer()
    {
        PlayerData.lemonsCount = lemonsOwned;
    }

    private void UpdateCactusNeedleServer()
    {
        PlayerData.cactusNeedlesCount = cactusNeedlesOwned;
    }
}
