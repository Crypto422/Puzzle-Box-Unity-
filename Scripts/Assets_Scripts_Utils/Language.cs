using Assets.Scripts.Configs;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	internal class Language
	{
		private SystemLanguage m_id = SystemLanguage.English;

		private Font m_fonts;

		private static Language m_instance;

		private Action m_transformHandle;

		//private event Action m_transformHandle;

		public SystemLanguage Id
		{
			get
			{
				return this.m_id;
			}
			set
			{
				this.m_id = value;
			}
		}

		public Language()
		{
			this.Init();
		}

		public static Language GetInstance()
		{
			if (Language.m_instance == null)
			{
				Language.m_instance = new Language();
			}
			return Language.m_instance;
		}

		public static string GetText(string id)
		{
			if (!Configs.Configs.TLanguages.ContainsKey(id))
			{
				return "ERROR NO " + id;
			}
			TLanguage tLanguage = Configs.Configs.TLanguages[id];
			SystemLanguage id2 = Language.GetInstance().Id;
			if (id2 <= SystemLanguage.Japanese)
			{
				if (id2 != SystemLanguage.Chinese)
				{
					if (id2 == SystemLanguage.Indonesian)
					{
						return tLanguage.L_ID;
					}
					if (id2 != SystemLanguage.Japanese)
					{
						goto IL_8C;
					}
					return tLanguage.L_JP;
				}
			}
			else if (id2 <= SystemLanguage.Russian)
			{
				if (id2 == SystemLanguage.Portuguese)
				{
					return tLanguage.L_PT;
				}
				if (id2 != SystemLanguage.Russian)
				{
					goto IL_8C;
				}
				return tLanguage.L_RU;
			}
			else if (id2 != SystemLanguage.ChineseSimplified)
			{
				if (id2 != SystemLanguage.ChineseTraditional)
				{
					goto IL_8C;
				}
				return tLanguage.L_ZH_CN;
			}
			return tLanguage.L_ZH_CN;
			IL_8C:
			return tLanguage.L_EN;
		}

		public static Font GetFont()
		{
			return Language.m_instance.m_fonts;
		}

		public void Set(SystemLanguage id)
		{
			this.Id = id;
			this.m_fonts = this.LoadFont();
			PlayerPrefs.SetInt("LocalData_LanguageId", (int)this.Id);
			Action expr_29 = this.m_transformHandle;
			if (expr_29 == null)
			{
				return;
			}
			expr_29();
		}

		public void AddEvent(Action e)
		{
			this.m_transformHandle += e;
		}

		public void RemoveEvent(Action e)
		{
			this.m_transformHandle -= e;
		}

		private void Init()
		{
			this.Id = (SystemLanguage)PlayerPrefs.GetInt("LocalData_LanguageId", (int)Application.systemLanguage);
			SystemLanguage id = this.Id;
			if (id <= SystemLanguage.Japanese)
			{
				if (id != SystemLanguage.Chinese)
				{
					if (id != SystemLanguage.Japanese)
					{
						goto IL_54;
					}
					this.Id = SystemLanguage.Japanese;
					goto IL_5C;
				}
			}
			else
			{
				if (id == SystemLanguage.Russian)
				{
					this.Id = SystemLanguage.Russian;
					goto IL_5C;
				}
				if (id != SystemLanguage.ChineseSimplified)
				{
					goto IL_54;
				}
			}
			this.Id = SystemLanguage.ChineseSimplified;
			goto IL_5C;
			IL_54:
			this.Id = SystemLanguage.English;
			IL_5C:
			this.m_fonts = this.LoadFont();
		}

		private Font LoadFont()
		{
			SystemLanguage id = this.Id;
			string path;
			if (id == SystemLanguage.Russian)
			{
				path = "font/font_ru";
			}
			else
			{
				path = "font/font_common";
			}
			return Resources.Load(path) as Font;
		}
	}
}
