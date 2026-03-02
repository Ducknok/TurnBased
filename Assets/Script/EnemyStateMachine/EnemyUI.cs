using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyUI : DucMonobehaviour
{
    private EnemyStateMachine esm;
    public Transform attacktypesIconPosition; // Vị trí trên đầu enemy
    public Transform timerIconPosition;
    public GameObject timerIcon;
    private GameObject timerIconPrefab;
    private Dictionary<BaseAttack.Effect, List<GameObject>> attackTypeIcons = new Dictionary<BaseAttack.Effect, List<GameObject>>();
    public List<GameObject> iconsToDestroy = new List<GameObject>();
    public List<GameObject> timerIconToDestroy = new List<GameObject>();



    protected override void Start()
    {
        this.timerIconPrefab = Resources.Load<GameObject>($"TimerIcon/Timer");
        this.esm = this.transform.GetComponent<EnemyStateMachine>();
    }
    protected override void Update()
    {
        foreach(var icon in this.timerIconToDestroy)
        {
            icon.transform.GetComponentInChildren<TextMeshProUGUI>().text = this.esm.timer.ToString();
        }
    }

    //-------------------------Attack Types---------------------------
    public void SetAttackTypes(List<BaseAttack.Effect> attackTypes)
    {
        // Xóa icon cũ trước khi cập nhật mới
        foreach (var icon in iconsToDestroy)
        {
            Destroy(icon);
        }
        this.attackTypeIcons.Clear();
        this.iconsToDestroy.Clear();

        float spacing = 1.03f;
        float startX = -(attackTypes.Count - 1) * spacing / 2f;

        for (int i = 0; i < attackTypes.Count; i++)
        {
            BaseAttack.Effect attackType = attackTypes[i];

            GameObject attackPrefab = Resources.Load<GameObject>($"AttackTypeIcon/{attackType}Icon");

            if (attackPrefab != null)
            {
                GameObject attackIcon = Instantiate(attackPrefab, attacktypesIconPosition.position, Quaternion.identity);
                attackIcon.transform.SetParent(attacktypesIconPosition, true);
                attackIcon.transform.localPosition += new Vector3(startX + i * spacing, 0, 0);

                // **Thay Dictionary thành List để lưu nhiều icon**
                if (!attackTypeIcons.ContainsKey(attackType))
                {
                    attackTypeIcons[attackType] = new List<GameObject>();
                }
                attackTypeIcons[attackType].Add(attackIcon); // Lưu nhiều icon cho cùng AttackType
                iconsToDestroy.Add(attackIcon);
            }
            else return;
        }
    }

    public void GrayOutAttackType(BaseAttack.Effect attackType1, BaseAttack.Effect attackType2)
    {
        //Debug.LogWarning(attackType1 + " va " + attackType2);

        if (attackTypeIcons.ContainsKey(attackType1))
        {
            //Debug.LogWarning(attackType);

            List<GameObject> icons = attackTypeIcons[attackType1];
            if (icons.Count > 0)
            {
                GameObject icon = icons[0]; // Lấy icon đầu tiên chưa bị phá
                icons.RemoveAt(0); // Xóa icon khỏi List để đảm bảo lần sau gọi sẽ ảnh hưởng icon tiếp theo

                SpriteRenderer spriteRenderer = icon.GetComponent<SpriteRenderer>();
                Transform iconBreak = icon.transform.Find("Break");

                if (iconBreak != null)
                {
                    iconBreak.gameObject.SetActive(true); // Hiện hiệu ứng "Break"
                }

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.gray; // Chuyển thành màu xám
                }
            }
        }
        if (attackType2 != attackType1 && attackTypeIcons.ContainsKey(attackType2))
        {
            //Debug.LogWarning(attackType);

            List<GameObject> icons = attackTypeIcons[attackType2];
            if (icons.Count > 0)
            {
                GameObject icon = icons[0]; // Lấy icon đầu tiên chưa bị phá
                icons.RemoveAt(0); // Xóa icon khỏi List để đảm bảo lần sau gọi sẽ ảnh hưởng icon tiếp theo

                SpriteRenderer spriteRenderer = icon.GetComponent<SpriteRenderer>();
                Transform iconBreak = icon.transform.Find("Break");

                if (iconBreak != null)
                {
                    iconBreak.gameObject.SetActive(true); // Hiện hiệu ứng "Break"
                }

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.gray; // Chuyển thành màu xám
                }
            }
        }
    }
    public IEnumerator ClearAllAttackTypeIcons()
    {
        yield return new WaitForSeconds(1.3f);
        foreach (var icon in iconsToDestroy)
        {
            //Debug.LogError("destroy lock");
            Destroy(icon);
        }

        // Cuối cùng, xóa Dictionary
        this.iconsToDestroy.Clear();
        this.attackTypeIcons.Clear();
    }

    //-------------------------Timer---------------------------
    public void SetTimerIcon(int timer)
    {
        if (timerIconPrefab != null)
        {
           
            this.timerIcon = Instantiate(timerIconPrefab, timerIconPosition.position, Quaternion.identity);
            timerIcon.transform.GetComponentInChildren<TextMeshProUGUI>().text = timer.ToString();
            timerIcon.transform.SetParent(timerIconPosition, true);
            this.timerIconToDestroy.Add(timerIcon);
        }
        else return;
    }

    public IEnumerator ClearTimerIcon()
    {
        yield return new WaitForSeconds(1f);
        foreach (var icon in timerIconToDestroy)
        {
            Destroy(icon);
        }
        this.timerIconToDestroy.Clear();
        
    }

}
