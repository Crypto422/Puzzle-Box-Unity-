using Assets.Scripts.Utils;
using System;
using UnityEngine;
using UnityEngine.UI;

public class LanguageComponent : MonoBehaviour
{
	private Text m_text;

	public string m_id = "TXT_NO_10001";

	public string m_suffix = "";

	public float m_linespace_ru;

	public float m_linespace_en;

	public float m_linespace_default = 0.5f;

	private void Start()
	{
		this.m_text = base.GetComponent<Text>();
		this.Set();
		Language.GetInstance().AddEvent(new Action(this.TransformLanuage));
	}

	private void OnDestroy()
	{
		Language.GetInstance().RemoveEvent(new Action(this.TransformLanuage));
	}

	public void SetText(string id)
	{
		this.m_id = id;
		this.Set();
	}

	private void Set()
	{
		if (this.m_text == null)
		{
			return;
		}
		this.m_text.text = Language.GetText(this.m_id) + this.m_suffix;
		this.m_text.font = Language.GetFont();
		SystemLanguage @int = (SystemLanguage)PlayerPrefs.GetInt("LocalData_LanguageId", (int)Application.systemLanguage);
		if (@int == SystemLanguage.Russian)
		{
			if (this.m_linespace_ru.CompareTo(0f) != 0)
			{
				this.m_text.lineSpacing = this.m_linespace_ru;
				return;
			}
		}
		else if (this.m_linespace_default.CompareTo(0f) != 0)
		{
			this.m_text.lineSpacing = this.m_linespace_default;
		}
	}

	private void TransformLanuage()
	{
		this.SetText(this.m_id);
	}
}
