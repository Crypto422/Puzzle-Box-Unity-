using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/*
 * this class will show infomation for level item in level selector
 */
public class G3LevelItem : MonoBehaviour
{
    //level text (1,2,3..etc..)
	public Text m_text;
    //image of level item
	public Image m_image;
    //color of level item
	public Color[] m_colors;
    //number of level
	private int number;
    //index of level in data
	private int index;

	private string key = "";

	private bool activity;

	public int Number
	{
		get
		{
			return this.number;
		}
		set
		{
			this.number = value;
		}
	}

	public int Index
	{
		get
		{
			return this.index;
		}
		set
		{
			this.index = value;
		}
	}

	public string Key
	{
		get
		{
			return this.key;
		}
		set
		{
			this.key = value;
		}
	}

	private void Start()
	{
		base.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnClick));
	}

	private void Update()
	{
	}

	public void Init(int num, int idx, string key)
	{
		this.Number = num;
		this.Index = idx;
		this.Key = key;
		this.SetNumber(num);
	}

	public void SetCheckpointStatus(bool activity)
	{
		this.activity = activity;
		this.UpdateUI();
	}

	public void SetNumber(int num)
	{
		this.number = num;
		this.UpdateUI();
	}

	public void UpdateUI()
	{
		this.m_text.text = this.number.ToString();
		if (this.activity)
		{
			this.m_image.color = this.m_colors[0];
			return;
		}
		this.m_image.color = this.m_colors[1];
	}

	public void OnClick()
	{
		if (this.activity)
		{
            G3MapData.GetInstance().DoClickCheckPointHandler(this);
		}
	}
    //set position for item in level selector by index
	public void SetPosition(int index)
	{
		this.SetPosition(G3MapData.GetInstance().GetRow(index), G3MapData.GetInstance().GetCol(index));
	}
    //set position for item in level selector by row and col
    public void SetPosition(int row, int col)
	{
		base.transform.localPosition = new Vector3((float)(col * 150 + 75 - 300), (float)(G3MapData.GetInstance().GetGameBoxHeight(0) / 2 - row * 150 - 75 + 30), 0f);
	}
}
