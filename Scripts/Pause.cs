using Assets.Scripts.GameManager;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
	public Text m_lb_title;

	public Text m_lb_score;

	public Text m_lb_score_value;

	public Button m_btn_home;

	public Button m_btn_refresh;

	public Button m_btn_continue;

	public Action OnClickHomeHandle;

	public Action OnClickRefreshHandle;

	public Action OnClickContinueHandle;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetScore(int score)
	{
		this.m_lb_score_value.text = string.Format((score < 1000) ? "{0}" : "{0:0,00}", score);
	}

	public void SetTitle(string id)
	{
		this.m_lb_score.GetComponent<LanguageComponent>().SetText(id);
	}

	public void OnClickHome()
	{
		Action _action = this.OnClickHomeHandle;
		if (_action != null)
		{
            _action();
		}
		DialogManager.GetInstance().Close(null);
	}

	public void OnClickRefresh()
	{
		//if (GM.GetInstance().IsRandomStatus(50))
		//{
		//	AdsManager.GetInstance().Play(AdsManager.AdType.Refresh, delegate
		//	{
		//		Action _action = this.OnClickRefreshHandle;
		//		if (_action == null)
		//		{
		//			return;
		//		}
		//              _action();
		//	}, null, 5, null);
		//}
		//else
		//{
		//	Action _action2 = this.OnClickRefreshHandle;
		//	if (_action2 != null)
		//	{
		//              _action2();
		//	}
		//}
		Action _action = this.OnClickRefreshHandle;
		if (_action != null)
		{
			_action();
		}
		DialogManager.GetInstance().Close(null);
	}

	public void OnClickContinue()
	{
		//if (GM.GetInstance().IsRandomStatus(50))
		//{
		//	AdsManager.GetInstance().Play(AdsManager.AdType.Continue, delegate
		//	{
		//		Action _action = this.OnClickContinueHandle;
		//		if (_action == null)
		//		{
		//			return;
		//		}
		//              _action();
		//	}, null, 5, null);
		//}
		//else
		//{
		//	Action _action2 = this.OnClickContinueHandle;
		//	if (_action2 != null)
		//	{
		//              _action2();
		//	}
		//}
		
		Action _action = this.OnClickContinueHandle;
		if (_action != null)
		{
			_action();
		}
		DialogManager.GetInstance().Close(null);
	}
}
