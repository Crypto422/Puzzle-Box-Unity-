using Assets.Scripts.GameManager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 * This class will set empty block for game board
 */
public class G3Block : MonoBehaviour
{
    //image of block
	public Image img_block;
    //normal color of block
	[SerializeField]
	public List<Color> colors = new List<Color>();
    //color of block if this the lines are connected
	public List<Color> colors2 = new List<Color>();
    //index of block
	private int index;
    //row index of block
	private int row;
    //col index of block
	private int col;
    //color index of block
	private int color;
    //width of block (pixel)
	private float width;
    //heigh of block(pixle)
	private float height;

    /*
     * get and set for parameter
     */

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

	public int Row
	{
		get
		{
			return this.row;
		}
		set
		{
			this.row = value;
		}
	}

	public int Col
	{
		get
		{
			return this.col;
		}
		set
		{
			this.col = value;
		}
	}

	public int Color
	{
		get
		{
			return this.color;
		}
		set
		{
			this.color = value;
		}
	}

	public float Width
	{
		get
		{
			return this.width;
		}
		set
		{
			this.width = value;
		}
	}

	public float Height
	{
		get
		{
			return this.height;
		}
		set
		{
			this.height = value;
		}
	}

	public G3Block(int idx, int color)
	{
		this.index = idx;
		this.color = color;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(int idx, int color)
	{
		this.index = idx;
		this.color = color;
		this.SetPosition(idx);
		this.SetColor(color);
	}

	public void SetContentSize(float x, float y)
	{
		this.SetContentSize(new Vector2(x, y));
	}
    /*
     * set size for this block  
     */  
	public void SetContentSize(Vector2 v)
	{
		this.Width = v.x;
		this.Height = v.y;
		this.img_block.GetComponent<RectTransform>().sizeDelta = v;
	}
    /*
     * set position for this block in the board
     */
	public void SetPosition(int index)
	{
		this.index = index;
		this.SetPosition(G3BoardGenerator.GetInstance().GetRow(index), G3BoardGenerator.GetInstance().GetCol(index));
	}

	public void SetPosition(int row, int col)
	{
		this.row = row;
		this.col = col;
		base.transform.localPosition = new Vector3((float)col * G3BoardGenerator.GetInstance().Cell_height + G3BoardGenerator.GetInstance().Cell_height / 2f - 282f
        , 281f - (float)row * G3BoardGenerator.GetInstance().Cell_width - G3BoardGenerator.GetInstance().Cell_width / 2f, 0f);
	}
    /*
     * set color for this block
     */
	public void SetColor(int color)
	{
		int skinID = GM.GetInstance().SkinID;
		List<Color> list;
		if (skinID != 1)
		{
			if (skinID != 2)
			{
				list = this.colors;
			}
			else
			{
				list = this.colors2;
			}
		}
		else
		{
			list = this.colors;
		}
		Color arg_37_0 = list[color];
		this.Color = color;
		this.img_block.color = list[color];
	}
}
