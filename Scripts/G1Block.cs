using Assets.Scripts.GameManager;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/*
 * this script will attached to block of Game 1 (Merge number game)
 */
public class G1Block : MonoBehaviour
{
    //number value of block
	private int number = 1;
    //index of block in the board
	private int index = -1;
    //save star when game over
	[SerializeField]
	public List<Sprite> boxSprite = new List<Sprite>();
    //list color of block, it depends on value of number
	[SerializeField]
	public List<Color> colors = new List<Color>();
    //background image of block
	public Image img_block;
    //number text if block
	public Text txt_number;
    //score effect appear when user get score
	public GameObject prefabScore;
    //image mask
	public Image img_mask;

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

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(int number, int idx)
	{
		this.index = idx;
		this.setNum(number);
		this.SetPosition(idx);
		base.transform.localScale = new Vector3(1f, 1f, 1f);
		base.GetComponent<Button>().onClick.AddListener(new UnityAction(this.OnClick));
		Color color = this.img_block.color;
		color.a = 1f;
		this.img_block.color = color;
		this.img_block.gameObject.SetActive(true);
		color = this.txt_number.color;
		color.a = 1f;
		this.txt_number.color = color;
		this.txt_number.gameObject.SetActive(true);
	}

	public void SetChoosParticleShow()
	{
	}

	public void PlayExplode()
	{
	}

	public void setNum(int number)
	{
		this.Number = number;
		this.txt_number.gameObject.SetActive(true);
		this.txt_number.text = number.ToString();
		this.img_mask.gameObject.SetActive(false);
		number = ((number < 0) ? (10 + number) : number);
		this.img_block.color = this.colors[Math.Abs(number % 20)];
		if (this.Number % 5 != 0)
		{
			return;
		}
		int num = this.Number / 5;
		this.img_mask.color = this.colors[Math.Abs(number % 20)];
		switch (num)
		{
		case 1:
			break;
		case 2:
			this.img_mask.sprite = this.boxSprite[0];
			this.img_mask.gameObject.SetActive(true);
			return;
		case 3:
			this.img_mask.sprite = this.boxSprite[1];
			this.img_mask.gameObject.SetActive(true);
			return;
		case 4:
			this.img_mask.sprite = this.boxSprite[2];
			this.img_mask.gameObject.SetActive(true);
			return;
		default:
			this.img_mask.sprite = this.boxSprite[3];
			this.img_mask.gameObject.SetActive(true);
			break;
		}
	}

	public Color GetCurrentColor()
	{
		return this.img_block.color;
	}

	public void ShowSymbol()
	{
		string arg = (this.Number > 0) ? "+" : "-";
		this.txt_number.text = arg + Math.Abs(this.Number);
	}

	public void SetPosition(int index)
	{
		int row = index / 5;
		int col = index % 5;
		this.SetPosition(row, col);
	}

	public void SetPosition(int row, int col)
	{
		base.transform.localPosition = new Vector3((float)(col * 120 + 60 - 300), (float)(row * 120 + 60 - 300), 0f);
	}

	public Tween Move(int index)
	{
		return base.transform.DOLocalMove(this.GetToPosition(index), 0.3f, false);
	}

	public Tween DelayMove(int index, float time)
	{
		return base.transform.DOLocalMove(this.GetToPosition(index), 0.1f, false).SetDelay(time).OnComplete(delegate
		{
            Game1DataLoader.GetInstance().FreeBlock(base.gameObject);
		});
	}

	public Sequence DoDeleteAni()
	{
		Sequence _sequence = DOTween.Sequence();
		Tween t = base.transform.DORotate(new Vector3(0f, 0f, -360f), 0.5f, RotateMode.FastBeyond360);
		Tween t2 = base.transform.DOScale(new Vector3(0f, 0f, 0.5f), 0.5f);
        _sequence.Insert(0f, t);
        _sequence.Insert(0f, t2);
		return _sequence;
    }

	public Sequence FadeOut()
	{
		this.txt_number.gameObject.SetActive(false);
		Sequence _sequence = DOTween.Sequence();
        _sequence.Insert(0f, this.img_block.DOFade(0f, 0.8f));
        _sequence.Insert(0f, this.txt_number.DOFade(0f, 0.8f));
        _sequence.SetTarget(this);
		return _sequence;
    }

	public void StopFade()
	{
		DOTween.Kill(this, false);
	}

	public void ShowScore()
	{
		GameObject obj = UnityEngine.Object.Instantiate<GameObject>(this.prefabScore);
		obj.transform.SetParent(base.transform.parent.parent, false);
		obj.transform.position = base.transform.position;
		obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		Text component = obj.transform.Find("txt").GetComponent<Text>();
		component.text = "+" + (this.Number * 10).ToString();
		Sequence _sequence = DOTween.Sequence();
        _sequence.Append(obj.transform.DOScale(1.2f, 0.2f));
        _sequence.Append(obj.transform.DOScale(1f, 0.1f));
        _sequence.Append(obj.transform.DOBlendableLocalMoveBy(new Vector3(0f, 30f, 0f), 1f, false).SetDelay(0.3f).SetEase(Ease.OutBack).OnComplete(delegate
		{
			UnityEngine.Object.Destroy(obj);
		}));
        _sequence.Insert(0.6f, component.DOFade(0f, 1f));
	}

	public void OnClick()
	{
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().HeartIndex != -1)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().IsGameOver())
		{
			return;
		}
		AudioManager.GetInstance().PlayEffect("sound_eff_click_1");
        Game1DataLoader.GetInstance().DoClickBlock(this);
	}

	public void RemoveClick()
	{
		base.GetComponent<Button>().onClick.RemoveAllListeners();
	}

	private Vector3 GetToPosition(int row, int col)
	{
		return new Vector3((float)(col * 120 + 60 - 300), (float)(row * 120 + 60 - 300), 0f);
	}

	private Vector3 GetToPosition(int _index)
	{
		int row = Game1DataLoader.GetInstance().GetRow(_index);
		int col = Game1DataLoader.GetInstance().GetCol(_index);
		return this.GetToPosition(row, col);
	}
}
