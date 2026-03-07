using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Random = UnityEngine.Random;

[Serializable]
public struct EffectIconMapping
{
    public BaseAttack.Effect effect;
    public Sprite iconSprite;
}

[Serializable]
public class EnemyUI : DucMonobehaviour
{
    private EnemyStateMachine esm;
    public Transform attacktypesIconPosition;
    public Transform timerIconPosition;
    public GameObject timerIcon;
    private GameObject timerIconPrefab;

    // Dictionary lưu trữ danh sách icon theo loại hệ
    private Dictionary<BaseAttack.Effect, List<GameObject>> attackTypeIcons = new Dictionary<BaseAttack.Effect, List<GameObject>>();
    public List<GameObject> iconsToDestroy = new List<GameObject>();
    public List<GameObject> timerIconToDestroy = new List<GameObject>();

    [Header("Icon Database Setup")]
    public GameObject baseIconPrefab;
    public List<EffectIconMapping> iconDatabase;
    private Dictionary<BaseAttack.Effect, Sprite> spriteDictionary = new Dictionary<BaseAttack.Effect, Sprite>();

    protected override void Start()
    {
        this.timerIconPrefab = Resources.Load<GameObject>($"TimerIcon/Timer");
        this.esm = this.transform.GetComponent<EnemyStateMachine>();

        foreach (var item in iconDatabase)
        {
            if (item.effect == BaseAttack.Effect.None) continue;
            if (!spriteDictionary.ContainsKey(item.effect) && item.iconSprite != null)
            {
                spriteDictionary.Add(item.effect, item.iconSprite);
            }
        }
    }

    protected override void Update()
    {
        foreach (var icon in this.timerIconToDestroy)
        {
            if (icon != null && this.esm != null)
            {
                icon.transform.GetComponentInChildren<TextMeshProUGUI>().text = this.esm.timer.ToString();
            }
        }
    }

    // =====================================================================
    // HÀM MỚI: TẠO LOCK VÀ ĐĂNG KÝ VÀO DICTIONARY NGAY LẬP TỨC
    // =====================================================================
    public void SetAttackTypes(List<BaseAttack.Effect> attackTypes)
    {
        // 1. NGỪNG CÁC COROUTINE ĐANG CHẠY (Nếu quái đang xoay icon cũ thì dừng ngay)
        StopAllCoroutines();

        // 2. DỌN DẸP TRIỆT ĐỂ ĐỒ CŨ
        foreach (var icon in iconsToDestroy) if (icon != null) Destroy(icon);
        this.iconsToDestroy.Clear();
        this.attackTypeIcons.Clear();

        if (spriteDictionary.Count == 0 || baseIconPrefab == null) return;

        float spacing = 1.03f;
        float startX = -(attackTypes.Count - 1) * spacing / 2f;

        // 3. TẠO ICON VÀ GÁN VÀO DICTIONARY LUÔN (KHÔNG ĐỢI XOAY XONG)
        for (int i = 0; i < attackTypes.Count; i++)
        {
            BaseAttack.Effect type = attackTypes[i];
            GameObject iconObj = Instantiate(baseIconPrefab, attacktypesIconPosition.position, Quaternion.identity);
            iconObj.transform.SetParent(attacktypesIconPosition, true);
            iconObj.transform.localPosition += new Vector3(startX + i * spacing, 0, 0);

            // Đăng ký vào Dictionary ngay để logic chém trúng là nhận diện được liền
            if (!attackTypeIcons.ContainsKey(type)) attackTypeIcons[type] = new List<GameObject>();
            attackTypeIcons[type].Add(iconObj);

            iconsToDestroy.Add(iconObj);

            // Tắt Animator tạm thời để chạy hiệu ứng xoay bằng script
            Animator anim = iconObj.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;
        }

        // 4. CHẠY HIỆU ỨNG XOAY (ROULETTE) SAU KHI ĐÃ ĐĂNG KÝ XONG DỮ LIỆU
        StartCoroutine(RollAttackTypesCoroutine(attackTypes));
    }

    private IEnumerator RollAttackTypesCoroutine(List<BaseAttack.Effect> finalTypes)
    {
        float rollDuration = 1.0f;
        float rollSpeed = 0.1f;
        float elapsedTime = 0f;
        List<BaseAttack.Effect> allEffects = new List<BaseAttack.Effect>(spriteDictionary.Keys);

        // Xoay hình ảnh liên tục cho đẹp mắt
        while (elapsedTime < rollDuration)
        {
            for (int i = 0; i < iconsToDestroy.Count; i++)
            {
                BaseAttack.Effect randomEffect = allEffects[Random.Range(0, allEffects.Count)];
                Transform iconChild = iconsToDestroy[i].transform.Find("AttackTypeIcon");
                if (iconChild != null) iconChild.GetComponent<SpriteRenderer>().sprite = spriteDictionary[randomEffect];
            }
            elapsedTime += rollSpeed;
            yield return new WaitForSeconds(rollSpeed);
        }

        // Chốt kết quả hình ảnh cuối cùng khớp với dữ liệu trong Dictionary
        for (int i = 0; i < iconsToDestroy.Count; i++)
        {
            BaseAttack.Effect finalEffect = finalTypes[i];
            Transform iconChild = iconsToDestroy[i].transform.Find("AttackTypeIcon");
            if (iconChild != null) iconChild.GetComponent<SpriteRenderer>().sprite = spriteDictionary[finalEffect];

            iconsToDestroy[i].transform.DOShakeScale(0.2f, 0.2f);
        }
    }

    public void GrayOutAttackType(BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        HandleGrayOut(attackType1);
        if (attackType2 != attackType1) HandleGrayOut(attackType2);
    }

    private void HandleGrayOut(BaseAttack.Effect type)
    {
        if (type == BaseAttack.Effect.None) return;

        if (attackTypeIcons.ContainsKey(type))
        {
            List<GameObject> icons = attackTypeIcons[type];
            if (icons.Count > 0)
            {
                GameObject icon = icons[0];
                icons.RemoveAt(0);

                Transform iconSpriteObj = icon.transform.Find("AttackTypeIcon");
                if (iconSpriteObj != null)
                {
                    SpriteRenderer sr = iconSpriteObj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.gray;
                }

                Transform iconBreak = icon.transform.Find("Break");
                if (iconBreak != null) iconBreak.gameObject.SetActive(true);

                icon.transform.DOShakePosition(0.2f, 0.1f);
            }
        }
    }

    public IEnumerator ClearAllAttackTypeIcons()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (var icon in iconsToDestroy) if (icon != null) Destroy(icon);
        this.iconsToDestroy.Clear();
        this.attackTypeIcons.Clear();
    }

    public void SetTimerIcon(int timer)
    {
        if (timerIconPrefab != null)
        {
            this.timerIcon = Instantiate(timerIconPrefab, timerIconPosition.position, Quaternion.identity);
            timerIcon.transform.GetComponentInChildren<TextMeshProUGUI>().text = timer.ToString();
            timerIcon.transform.SetParent(timerIconPosition, true);
            this.timerIconToDestroy.Add(timerIcon);
        }
    }

    public IEnumerator ClearTimerIcon()
    {
        foreach (var icon in timerIconToDestroy) if (icon != null) Destroy(icon);
        this.timerIconToDestroy.Clear();
        yield return null;
    }
}