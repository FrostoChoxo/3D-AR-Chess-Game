using System.Collections;
using UnityEngine;

public class HandleUI : MonoBehaviour
{
    private GameObject mainMenu;
    private GameObject winScreen;
    private GameObject drawScreen;
    private GameObject aiLevelMenu;
    private GameObject aiColorMenu;
    private GameObject pvpMenu;
    public float delay = 0.05f; // Delay of 0.05 seconds to allow menu audio to play

    public GameObject loader;
    public GameObject loadText;

    private bool isPVP = false;

    void Start()
    {
        mainMenu = gameObject.transform.GetChild(0).gameObject;
        winScreen = gameObject.transform.GetChild(1).gameObject;
        drawScreen = gameObject.transform.GetChild(2).gameObject;
        aiLevelMenu = gameObject.transform.GetChild(3).gameObject;
        aiColorMenu = gameObject.transform.GetChild(4).gameObject;
        pvpMenu = gameObject.transform.GetChild(5).gameObject;
        SetMainMenuActive();
    }

    public void SetMainMenuActive()
    {
        StartCoroutine(ActivateMainMenu());
    }

    public void SetAiLevelActive()
    {
        StartCoroutine(ActivateAiLevel());
    }

    public void SetAiColorActive()
    {
        StartCoroutine(ActivateAiColor());
    }

    public void SetWinScreenActive()
    {
        StartCoroutine(ActivateWinScreen());
    }

    public void SetDrawScreenActive()
    {
        StartCoroutine(ActivateDrawScreen());
    }

    public void SetPvpMenuActive()
    {
        StartCoroutine(ActivatePvpMenu());
    }

    public void setAllInactive()
    {
        StartCoroutine(SetAllInactive());
    }

    private IEnumerator SetAllInactive()
    {
        yield return new WaitForSeconds(delay);
        mainMenu.SetActive(false);
        winScreen.SetActive(false);
        drawScreen.SetActive(false);
        aiLevelMenu.SetActive(false);
        aiColorMenu.SetActive(false);
        pvpMenu.SetActive(false);
        if (isPVP)
        {
            loader.SetActive(true);
            loadText.SetActive(true);
        }
    }

    private IEnumerator ActivateMainMenu()
    {
        yield return new WaitForSeconds(delay);
        mainMenu.SetActive(true);
        winScreen.SetActive(false);
        drawScreen.SetActive(false);
        aiLevelMenu.SetActive(false);
        aiColorMenu.SetActive(false);
        pvpMenu.SetActive(false);
    }

    private IEnumerator ActivateAiLevel()
    {
        yield return new WaitForSeconds(delay);
        mainMenu.SetActive(false);
        winScreen.SetActive(false);
        drawScreen.SetActive(false);
        aiLevelMenu.SetActive(true);
        aiColorMenu.SetActive(false);
        pvpMenu.SetActive(false);
    }

    private IEnumerator ActivateAiColor()
    {
        yield return new WaitForSeconds(delay);
        mainMenu.SetActive(false);
        winScreen.SetActive(false);
        drawScreen.SetActive(false);
        aiLevelMenu.SetActive(false);
        aiColorMenu.SetActive(true);
        pvpMenu.SetActive(false);
    }

    private IEnumerator ActivateWinScreen()
    {
        yield return new WaitForSeconds(delay);
        mainMenu.SetActive(false);
        winScreen.SetActive(true);
        drawScreen.SetActive(false);
        aiLevelMenu.SetActive(false);
        aiColorMenu.SetActive(false);
        pvpMenu.SetActive(false);
    }

    private IEnumerator ActivateDrawScreen()
    {
        yield return new WaitForSeconds(delay);
        mainMenu.SetActive(false);
        winScreen.SetActive(false);
        drawScreen.SetActive(true);
        aiLevelMenu.SetActive(false);
        aiColorMenu.SetActive(false);
        pvpMenu.SetActive(false);
    }

    private IEnumerator ActivatePvpMenu()
    {
        yield return new WaitForSeconds(delay);
        isPVP=true;
        mainMenu.SetActive(false);
        winScreen.SetActive(false);
        drawScreen.SetActive(false);
        aiLevelMenu.SetActive(false);
        aiColorMenu.SetActive(false);
        pvpMenu.SetActive(true);
    }
}
