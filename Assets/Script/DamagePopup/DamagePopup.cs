using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamagePopup : DucMonobehaviour
{
    public static DamagePopup Create(Vector3 position, float damageAmout, bool isCriticalHit, bool isNormalAttack)
    {
        Transform damagePopupTransform = Instantiate(GameAssets.Instance.popupText,position,Quaternion.identity);
        DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup.SetUp(damageAmout, isCriticalHit, isNormalAttack);
        return damagePopup;
    }

    private static int sortingOrder;
    private const float DISAPPEAR_TIMER_MAX = 1f;
    private TextMeshPro textMesh;
    private Color textColor;
    private float disappearTimer;
    private Vector3 moveVector;

    protected override void Awake()
    {
        this.textMesh = this.transform.GetComponent<TextMeshPro>();
    }

    public void SetUp(float damageAmount, bool isCriticalHit, bool isNormalAttack)
    {
        this.textMesh.SetText(damageAmount.ToString());
        if (!isCriticalHit)
        {
            this.textMesh.fontSize = 10f;
            this.textColor = Color.white;
        }
        else
        {
            this.textMesh.fontSize = 15f;
            this.textColor = Color.yellow;

        }
        if (isNormalAttack)
        {
            this.textMesh.fontSize = 10;
            this.textColor = Color.blue;
        }
        textMesh.color = this.textColor;
        this.disappearTimer = DISAPPEAR_TIMER_MAX;
        sortingOrder++;
        this.textMesh.sortingOrder = sortingOrder;
        this.moveVector = new Vector3(0f, 0.5f) * 10f;
    }

    protected override void Update()
    {
        this.CheckState();
    }
    public override void CheckState()
    {
        this.transform.position += this.moveVector * Time.deltaTime;
        this.moveVector -= this.moveVector * 2f * Time.deltaTime;
        if (this.disappearTimer > DISAPPEAR_TIMER_MAX * 5f)
        {
            float increaseScaleAmount = 1f;
            this.transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        }
        else
        {
            float decreaseScaleAmount = 1f;
            this.transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }
        this.disappearTimer -= Time.deltaTime;
        if (this.disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            this.textColor.a -= disappearSpeed * Time.deltaTime;
            this.textMesh.color = this.textColor;
            if (textColor.a < 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
