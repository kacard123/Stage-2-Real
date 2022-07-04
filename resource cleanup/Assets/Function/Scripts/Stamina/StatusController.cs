using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusController : MonoBehaviour
{
    // 필요한 이미지
    [SerializeField]
    private Image staminaImage;

     // 스태미나
    [SerializeField]
    private float maxS;  // 최대 스태미나. 유니티 에디터 슬롯에서 지정할 것.
    private float currentS;

    // 스태미나 증가량
    [SerializeField]
    private float spIncreaseSpeed;

    // 스태미나 재회복 딜레이 시간
    [SerializeField]
    private float spRechargeTime;  
    private float currentSpRechargeTime;

    // 스태미나 감소 여부
    private bool spUsed;

    [SerializeField]private float canHealStamina;

    void Start()
    {
        currentS = maxS;
    }

    void Update()
    {
        SPRechargeTime();
        SPRecover();
        GaugeUpdate();
    }

    private void GaugeUpdate()
    {
        staminaImage.fillAmount = (float)currentS / maxS;
    }

     public void DecreaseStamina(float count)
    {
        spUsed = true;
        currentSpRechargeTime = 0f;

        if (currentS - count > 0f)
        {
            currentS -= count;
        }
        else
            currentS = 0f;
    }

    private void SPRechargeTime()
    {
        if (spUsed)
        {
            if (currentSpRechargeTime < spRechargeTime)
                currentSpRechargeTime++;
            else
                spUsed = false;
        }
    }

    private void SPRecover()
    {
        if (!spUsed && currentS < maxS)
        {
            currentS += spIncreaseSpeed;
        }
    }

    public float GetCurrentSP()
    {
        return currentS;
    }

    public void FullStamina()
    {
        if(currentS < maxS)
        {
            currentS += canHealStamina;
        }
    }
}
