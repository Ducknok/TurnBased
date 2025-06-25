using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HeroPanelHandler : MonoBehaviour
{
    public HeroStateMachine hsm;
    //For the Player Bar
    
    public Image heroHPBarFill;
    public Image heroHPBarTrail;
    public Image heroMPBarFill;
    public Image heroMPBarTrail;
    //Hero Panel;
    private HeroPanelStats stats;
    public GameObject playerPanel;
    public Transform heroPanelSpacer;
    // Start is called before the first frame update
    private void Awake()
    {
        this.hsm = this.gameObject.GetComponent<HeroStateMachine>();
        this.playerPanel = Resources.Load<GameObject>($"Prefabs/GUI/{this.hsm.baseHero.theName}Bar");
        this.heroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");
        if (heroPanelSpacer == null)
        {
            Debug.LogError("Không tìm thấy BattleCanvas!");
            return;
        }
        this.CreateHeroPanel();
    }
    protected void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected virtual void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    protected void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.hsm = this.gameObject.GetComponent<HeroStateMachine>();
        this.playerPanel = Resources.Load<GameObject>($"Prefabs/GUI/{this.hsm.baseHero.theName}Bar");
        this.heroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("Panel").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");
        if (heroPanelSpacer == null)
        {
            Debug.LogError("Không tìm thấy BattleCanvas!");
            return;
        }
        //this.CreateHeroPanel();
    }
    public void BindHeroUI(Image hpFill, Image mpFill)
    {
        heroHPBarFill = hpFill;
        heroMPBarFill = mpFill;
        //this.DescreaseMana();
    }
    //TODO: Tach toan bo ham gay st, hoi mana, mau, tao panel ra 1 class rieng xong goi lai trong class hero hoac 1 class moi 
    //Create a player panel
    public void CreateHeroPanel()
    {
        this.playerPanel = Instantiate(this.playerPanel) as GameObject;
        this.stats = this.playerPanel.GetComponent<HeroPanelStats>();
        this.InitializeStatHero();

    }
    public void InitializeStatHero()
    {
        this.stats.heroName.text = this.hsm.baseHero.theName;
        this.stats.heroHP.text = this.hsm.baseHero.curHP.ToString();
        this.stats.heroMP.text = this.hsm.baseHero.curMP.ToString();
        this.heroHPBarFill = this.stats.hpBarFill;
        this.heroHPBarTrail = this.stats.hpBarTrail;
        this.heroHPBarFill.fillAmount = 1f;
        this.heroHPBarTrail.fillAmount = 1f;
        this.heroMPBarFill = this.stats.mpBarFill;
        this.heroMPBarTrail = this.stats.mpBarTrail;
        this.heroMPBarFill.fillAmount = 1f;
        this.heroMPBarTrail.fillAmount = 1f;

        this.playerPanel.transform.SetParent(this.heroPanelSpacer, false);
    }
    //Update stats hp, mp, heal
    public void UpdateHeroPanel()
    {
        this.stats.heroHP.text = this.hsm.baseHero.curHP.ToString();
        this.stats.heroMP.text = this.hsm.baseHero.curMP.ToString();
    }
}
