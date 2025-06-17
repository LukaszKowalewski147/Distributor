using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private string name;
    private int hp;
    private int lemonsOwned;
    private int cactusNeedlesOwned;

    private void Start()
    {
        hp = 100;
        lemonsOwned = 0;       // TODO: change to 0
        cactusNeedlesOwned = 0; // TODO: change to 0
        UpdateHpUI();
        UpdateLemonUI();
        UpdateCactusNeedleUI();
    }

    public void SetName(string name)
    {
        this.name = name;
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
        UpdateLemonUI();
    }

    public void AddCactusNeedle()
    {
        cactusNeedlesOwned++;
        UpdateCactusNeedleUI();
    }

    public void TakeAwayLemons(int amount)
    {
        lemonsOwned -= amount;
        UpdateLemonUI();
    }

    public void TakeAwayCactusNeedles(int amount)
    {
        cactusNeedlesOwned -= amount;
        UpdateCactusNeedleUI();
    }

    public void IncreaseHpBy(int amount)
    {
        hp += amount;
        UpdateHpUI();
    }

    public void DecreaseHpBy(int amount)
    {
        hp -= amount;
        UpdateHpUI();
    }

    public void UpdateHpUI()
    {
        uiManager.UpdateHpCounter(hp);
    }

    public void UpdateLemonUI()
    {
        uiManager.UpdateLemonCounter(lemonsOwned);
    }

    public void UpdateCactusNeedleUI()
    {
        uiManager.UpdateCactusNeedlesCounter(cactusNeedlesOwned);
    }
}
