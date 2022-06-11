using Assets.Scripts.GameManager;
using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
/*
 * This script will show information for UI when game over (Merge Block Game)
 */
public class G1UIManager : MonoBehaviour
{
    //add 5 diamond if user get first sharing
	[Serializable]
	private sealed class CommonClass
	{
		public static readonly G1UIManager.CommonClass _commonClass = new G1UIManager.CommonClass();

		public static Action _action;

		internal void OnClickShare()
		{
			if (GM.GetInstance().isFirstShare())
			{
				GM.GetInstance().ResetFirstShare(1);
				GM.GetInstance().AddDiamond(5, true);
			}
		}
	}
    //score text
	public Text m_txt_score_value;
    //best score text
	public Text m_txt_record_value;
    //need gem text
	public Text m_txt_needGEM;
    //show star
	public Image m_img_circle;
    //tips text
	public LanguageComponent m_txt_tips;

	private double m_needGEM;

	private int gameID = 1;

	public Transform[] m_tips_component;

	public string[] m_tips_content;

	private void Start()
	{
		this.m_needGEM = 100.0 * Math.Pow(2.0, (double)GM.GetInstance().ConsumeCount);
		this.m_txt_needGEM.text = this.m_needGEM.ToString();
		GM.GetInstance().SetSavedGameID(0);
		GM.GetInstance().SaveScore(this.gameID, 0);
        Game1DataLoader.GetInstance().FinishCount = 0;
		AudioManager.GetInstance().PlayEffect("sound_eff_victory");
	}

	private void Update()
	{
	}

	private void OnDestroy()
	{
		DOTween.Kill(this.m_img_circle, false);
	}

	public void Load(int score, int maxScore)
	{
		this.m_txt_score_value.text = string.Format((score < 1000) ? "{0}" : "{0:0,00}", score);
		this.m_txt_record_value.text = string.Format((maxScore < 1000) ? "{0}" : "{0:0,00}", maxScore);
	}

	public void OnClickDiamond()
	{
		if (!GM.GetInstance().isFullGEM(Convert.ToInt32(this.m_needGEM)))
		{
			ToastManager.Show("TXT_NO_50001", true);
			return;
		}
		GM.GetInstance().SetSavedGameID(this.gameID);
		GM.GetInstance().ConsumeGEM(Convert.ToInt32(this.m_needGEM));
		GM.GetInstance().AddConsumeCount();
        Game1DataLoader.GetInstance().FillLife(false);
        Game1DataLoader.GetInstance().DoFillLife();
		DialogManager.GetInstance().Close(null);
	}

	public void OnClickAds()
	{
		AdsManager.GetInstance().Play(AdsManager.AdType.ResetLife, delegate
		{
			GM.GetInstance().SetSavedGameID(this.gameID);
            Game1DataLoader.GetInstance().FillLife(false);
            Game1DataLoader.GetInstance().DoFillLife();
			DialogManager.GetInstance().Close(null);
		}, null, 5, null);
	}

	public void OnClickHome()
	{
        Game1DataLoader.GetInstance().Score = 0;
		GM.GetInstance().SaveScore(this.gameID, 0);
		GM.GetInstance().SetSavedGameID(0);
		GM.GetInstance().ResetToNewGame();
		GM.GetInstance().ResetConsumeCount();
		DialogManager.GetInstance().Close(null);
		GlobalEventHandle.EmitClickPageButtonHandle("main", 0);
	}

	public void OnClickAgain()
	{
		GM.GetInstance().SaveScore(this.gameID, 0);
		GM.GetInstance().SetSavedGameID(0);
		GM.GetInstance().ResetToNewGame();
		GM.GetInstance().ResetConsumeCount();
        Game1DataLoader.GetInstance().Score = 0;
        Game1DataLoader.GetInstance().StartNewGame();
		DialogManager.GetInstance().Close(null);
	}

	public void OnClickShare()
	{
        Application.OpenURL("https://play.google.com/store/apps/details?id=" + Application.identifier);
    }

	private void PlayTips()
	{
		int idx = new System.Random().Next(0, 1);
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
			idx1 %= 1;
		});
        _sequence2.AppendInterval(4f);
        _sequence2.SetLoops(-1);
        _sequence2.SetTarget(this.m_img_circle);
	}
}
