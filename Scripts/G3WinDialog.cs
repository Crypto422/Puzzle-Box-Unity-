using Assets.Scripts.Configs;
using Assets.Scripts.GameManager;
using DG.Tweening;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
/*
 * this class are attached to dialog that shown when level is clear
 * */
public class G3WinDialog : MonoBehaviour
{
    //all parameter will show or save when you pass the levels
	private int gameID = 3;

	private int m_level = 1;

	private int m_next;

	private int m_lv;

	private int m_award;

	private int m_exp;

	private bool isShowAward;

	public Image m_img_circle;

	public LanguageComponent m_txt_tips;

	public Transform[] m_tips_component;

	public string[] m_tips_content;

	public Text m_txt_coin;

	public int Level
	{
		get
		{
			return this.m_level;
		}
		set
		{
			this.m_level = value;
		}
	}

	public int Next
	{
		get
		{
			return this.m_next;
		}
		set
		{
			this.m_next = value;
		}
	}

	public int Lv
	{
		get
		{
			return this.m_lv;
		}
		set
		{
			this.m_lv = value;
		}
	}

	public int Award
	{
		get
		{
			return this.m_award;
		}
		set
		{
			this.m_award = value;
		}
	}

	public int Exp
	{
		get
		{
			return this.m_exp;
		}
		set
		{
			this.m_exp = value;
		}
	}

	private void Start()
	{
		GM.GetInstance().SetSavedGameID(0);
		if (this.isShowAward)
		{
			GM.GetInstance().AddDiamond(this.Award, true);
			GM.GetInstance().AddExp(this.Exp);
		}
		AudioManager.GetInstance().PlayEffect("sound_eff_popup");
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		DOTween.Kill(this.m_img_circle, false);
	}

	public void Load(int score, int next, int lv, int award = 0, int exp = 0)
	{
		this.Level = score;
		this.Next = next;
		this.Lv = lv;
		this.Award = award;
		this.Exp = exp;
		this.m_txt_coin.text = "+ " + award.ToString();
		if (score > 15)
		{
			GM.GetInstance().SetFirstFinishGame();
		}
	}

	public void OnClickNext()
	{
		UnityEngine.Debug.Log("On next lv");
		GM.GetInstance().SetSavedGameID(this.gameID);
        G3BoardGenerator.GetInstance().DestroyMap();
        G3BoardGenerator.GetInstance().StartNewGame(Configs.TG00301[this.Next.ToString()].ID);
		GlobalEventHandle.EmitDoRefreshCheckPoint(this.Lv);
		AudioManager.GetInstance().PlayEffect("sound_eff_button");
		DialogManager.GetInstance().Close(null);
		if (this.Level % 5 == 0)
		{
            //AdsManager.GetInstance().Play(AdsManager.AdType.Finish, null, null, 5, null);
            AdsControl.Instance.showAds();
		}
	}

	public void OnClickAgain()
	{
        AdsControl.Instance.ShowRewardVideo();
        GM.GetInstance().AddDiamond(5, true);
        GlobalEventHandle.EmitDoRefreshCheckPoint(this.Lv);
        this.OnClickNext();
        /*
        AdsManager.GetInstance().Play(AdsManager.AdType.MultiAwards, delegate
		{
			if (this.isShowAward)
			{
				GM.GetInstance().AddDiamond(this.Award, true);
			}
			GlobalEventHandle.EmitDoRefreshCheckPoint(this.Lv);
			this.OnClickNext();
		}, null, 5, null);
		*/
	}

	public void OnClickHome()
	{
        G3BoardGenerator.GetInstance().DestroyMap();
		GM.GetInstance().SetSavedGameID(0);
		GM.GetInstance().ResetToNewGame();
		GM.GetInstance().ResetConsumeCount();
		GlobalEventHandle.EmitClickPageButtonHandle("main", 0);
		GlobalEventHandle.EmitDoRefreshCheckPoint(this.Lv);
		AudioManager.GetInstance().PlayEffect("sound_eff_button");
		DialogManager.GetInstance().Close(null);
	}

	private void PlayTips()
	{
		int idx = new System.Random().Next(0, 1);
		this.m_img_circle.gameObject.SetActive(true);
		Sequence _sequence = DOTween.Sequence();
        _sequence.Append(this.m_img_circle.DOFade(0.3f, 1f));
        _sequence.Append(this.m_img_circle.DOFade(1f, 1f));
        _sequence.Append(this.m_img_circle.DOFade(0.3f, 1f));
        _sequence.Append(this.m_img_circle.DOFade(1f, 1f));
        _sequence.SetLoops(-1);
        _sequence.SetTarget(this.m_img_circle);
		Sequence _sequence2 = DOTween.Sequence();
        int idx1 = 0;
        _sequence2.AppendCallback(delegate
		{
			//int idx;
			this.m_img_circle.transform.localPosition = this.m_tips_component[idx1].localPosition;
			this.m_txt_tips.SetText(this.m_tips_content[idx]);
			idx1++;
			idx = idx1;
			idx1 %= 3;
		});
        _sequence2.AppendInterval(4f);
        _sequence2.SetLoops(-1);
        _sequence2.SetTarget(this.m_img_circle);
	}

	public void IsShowAward(bool isShowAward)
	{
		this.isShowAward = isShowAward;
		base.transform.Find("panel_award").gameObject.SetActive(isShowAward);
	}
}
