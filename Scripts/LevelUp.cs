using Assets.Scripts.Configs;
using Assets.Scripts.GameManager;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
	public GameObject m_img_cirlce;

	public Text m_txt_lv_value;

	public Text m_txt_awards;

	private void Start()
	{
		this.InitUI();
	}

	private void Update()
	{
	}

	public void OnClickOK()
	{
		if (Configs.TPlayers.ContainsKey(GM.GetInstance().Lv.ToString()))
		{
			TPlayer tPlayer = Configs.TPlayers[GM.GetInstance().Lv.ToString()];
			GM.GetInstance().AddDiamond(tPlayer.Item, true);
		}
		if (GM.GetInstance().GameId == 2)
		{
            G2BoardGenerator.GetInstance().IsPuase = false;
		}
		DialogManager.GetInstance().Close(null);
	}

	private void InitUI()
	{
		this.m_txt_lv_value.text = string.Format("Level {0}", GM.GetInstance().Lv);
		if (Configs.TPlayers.ContainsKey(GM.GetInstance().Lv.ToString()))
		{
			TPlayer tPlayer = Configs.TPlayers[GM.GetInstance().Lv.ToString()];
			this.m_txt_awards.text = string.Format("+{0}", tPlayer.Item);
		}
		DOTween.Kill(this.m_img_cirlce, false);
		Sequence _sequence = DOTween.Sequence();
		Image component = this.m_img_cirlce.GetComponent<Image>();
        _sequence.Append(component.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0f));
        _sequence.Append(component.DOFade(1f, 0f));
        _sequence.Append(component.DOFade(0f, 1f));
        _sequence.Insert(0f, component.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 1f));
        _sequence.AppendInterval(1f);
        _sequence.SetLoops(-1);
	}
}
