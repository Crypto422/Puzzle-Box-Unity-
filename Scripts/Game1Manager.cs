using Assets.Scripts.GameManager;
using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/*
 * This script will controll all logical of game play
 */
public class Game1Manager : MonoBehaviour
{
    //root transform of board
	private struct TransformControl
	{
		public Transform parent;

		public Transform self;

		public TransformControl(Transform parent, Transform self)
		{
			this.parent = parent;
			this.self = self;
		}
	}
    /*
     * Find path function, it find all blocks have same number
     */
	private struct PathRDM
	{
		public List<Node> paths;

		public int index;
	}
	[Serializable]
	private sealed class FindPathClass
	{
		public static readonly Game1Manager.FindPathClass _findPathFunc = new Game1Manager.FindPathClass();

		public static TweenCallback _callback;

		public static Comparison<Game1Manager.PathRDM> camparisonFunc;

		internal void UseProps()
		{
            Game1DataLoader.GetInstance().Down();
            Game1DataLoader.GetInstance().CurPropId = 0;
		}

		internal int FindPathFunc(Game1Manager.PathRDM a, Game1Manager.PathRDM b)
		{
			return -1 * a.paths.Count.CompareTo(b.paths.Count);
		}
	}
    //object hold the game board
	public GameObject gameBox;
    //list of blocks
	private List<G1Block> blocks = new List<G1Block>();
    //the block will be drag to board
	public GameObject bloodBox;
    //list of blood contain 5 blocks
	[SerializeField]
	public List<GameObject> bloodPosList = new List<GameObject>();

	private List<GameObject> bloodList = new List<GameObject>
	{
		null,
		null,
		null,
		null,
		null
	};

	public GameObject m_pannel_props;

	private List<GameObject> m_tranformObjs = new List<GameObject>();

	private GameObject bgPs;

	public GameObject txt_score;

	public Image m_img_double;

	public Transform[] m_img_props;

	public GameObject m_line;

	public GameObject m_figner;

	public GameObject m_mask;

	public GameObject m_img_heart;

	private int m_dobleTotal;

	private List<Game1Manager.TransformControl> m_transformList = new List<Game1Manager.TransformControl>();

	private Vector3 m_fingerRect2 = new Vector3(100f, -70f);

	private float m_tips_time;

	private bool m_markTips = true;

	private int m_guideStatus;

	private void Start()
	{
		this.LoadUI();
		this.LoadBlock();
		this.InitEvent();
		this.runGuide();
	}

	private void Update()
	{
		if (Game1DataLoader.GetInstance().BloodList[1] == 0 && this.m_markTips)
		{
			this.m_tips_time += Time.deltaTime;
			if (this.m_tips_time >= 30f)
			{
				this.m_tips_time = 0f;
				this.m_markTips = false;
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00106") as GameObject);
                G1Dialog _dialog = gameObject.GetComponent< G1Dialog>();
                _dialog.OnClickAdsHandle = delegate
				{
					this.OnClickProp(1);
					this.m_markTips = false;
				};
                _dialog.OnClickCloseHandle = delegate
				{
					this.m_markTips = true;
				};
				DialogManager.GetInstance().show(gameObject, false);
			}
		}
	}

	private void OnDestroy()
	{
		this.RemoveEvent();
	}

	private void InitEvent()
	{
        Game1DataLoader.GetInstance().DoRefreshHandle += new Action(this.RefreshMap);
        Game1DataLoader.GetInstance().DoDeleteHandle += new Action<List<int>, int>(this.Delete);
        Game1DataLoader.GetInstance().DoDropHandle += new Action<List<sDropData>, List<int>>(this.Drop);
        Game1DataLoader _data1 = Game1DataLoader.GetInstance();
        _data1.DoClickBlock = (Action<G1Block>)Delegate.Combine(_data1.DoClickBlock, new Action<G1Block>(this.OnClickBlock));
        Game1DataLoader _data2 = Game1DataLoader.GetInstance();
        _data2.DoFillLife = (Action)Delegate.Combine(_data2.DoFillLife, new Action(this.InitLife));
        Game1DataLoader.GetInstance().DoCompMaxNumber += new Action<int>(this.PlayNewNumber);
        Game1DataLoader.GetInstance().OnClickReturnHandle = new Action<GameList>(this.OnClickReturn);
        Game1DataLoader.GetInstance().OnRandomHeartHandle = new Action(this.OnRandomHeart);
        Game1DataLoader.GetInstance().OnUseHeartHandle = new Action<int>(this.UseHeart);
	}

	private void RemoveEvent()
	{
	}

	public void OnClickProp(int type)
	{
		this.m_markTips = true;
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().HeartIndex != -1)
		{
			return;
		}
		
		int value = Constant.COMMON_CONFIG_PROP[type - 1];
		if (!GM.GetInstance().isFullGEM(value))
		{
			GlobalEventHandle.EmitClickPageButtonHandle("shop", 0);
			return;
		}
		if (Game1DataLoader.GetInstance().CurPropId != 0)
		{
            Game1DataLoader.GetInstance().CurPropId = 0;
			this.ControlPropsPannel(true);
			return;
		}
        Game1DataLoader.GetInstance().CurPropId = type;
		this.ControlPropsPannel(false);
	}

	public void StopAllParticle()
	{
	}

	public void BeginAllParticle()
	{
	}

	public void OnClickReturn(GameList obj)
	{
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		this.m_markTips = false;
		Utils.ShowPause(Game1DataLoader.GetInstance().Score, delegate
		{
            Game1DataLoader.GetInstance().Score = 0;
			GM.GetInstance().SaveScore(1, 0);
			GM.GetInstance().SetSavedGameID(0);
			GM.GetInstance().ResetToNewGame();
			GM.GetInstance().ResetConsumeCount();
			GlobalEventHandle.EmitDoGoHome();
			GlobalEventHandle.EmitClickPageButtonHandle("main", 0);
			obj.HideTopBtn();
		}, delegate
		{
			GM.GetInstance().SaveScore(1, 0);
			GM.GetInstance().SetSavedGameID(0);
			GM.GetInstance().ResetToNewGame();
			GM.GetInstance().ResetConsumeCount();
            Game1DataLoader.GetInstance().Score = 0;
            Game1DataLoader.GetInstance().StartNewGame();
			this.m_tips_time = 0f;
			this.m_markTips = true;
		}, delegate
		{
			this.m_markTips = true;
		});
	}

	private void LoadUI()
	{
		this.RefreshScore(false);
		this.InitLife();
		this.m_img_heart.SetActive(false);
	}

	private void RefreshScore(bool isAni = true)
	{
		if (isAni)
		{
			this.txt_score.GetComponent<OverlayNumber>().setNum(Game1DataLoader.GetInstance().Score);
			return;
		}
		this.txt_score.GetComponent<OverlayNumber>().Reset();
		this.txt_score.GetComponent<OverlayNumber>().setNum(Game1DataLoader.GetInstance().Score);
		this.txt_score.GetComponent<Text>().text = string.Format((Game1DataLoader.GetInstance().Score < 1000) ? "{0}" : "{0:0,00}", Game1DataLoader.GetInstance().Score);
	}

	private void LoadBlock()
	{
		for (int i = 0; i < 5; i++)
		{
			for (int j = 0; j < 5; j++)
			{
				int number = Game1DataLoader.GetInstance().GetNumber(i, j);
				int index = Game1DataLoader.GetInstance().GetIndex(i, j);
                G1Block item = Game1DataLoader.GetInstance().CreateBlock(number, index, this.gameBox);
				this.blocks.Add(item);
			}
		}
	}

	private void InitLife()
	{
		for (int i = 0; i < 5; i++)
		{
			int num = Game1DataLoader.GetInstance().BloodList[i];
			if (num != 0)
			{
                G1BlockEvent g = this.CreateNewLife(num, this.bloodBox, i);
				g.transform.localPosition = this.bloodPosList[i].transform.localPosition;
				g.transform.localScale = this.bloodPosList[i].transform.localScale;
				if (i == 0)
				{
					g.DisableDrag = false;
				}
			}
		}
		this.m_markTips = true;
	}

	private void RefreshMap()
	{
		foreach (G1Block current in this.blocks)
		{
			if (!(current == null))
			{
                Game1DataLoader.GetInstance().FreeBlock(current.gameObject);
			}
		}
		this.blocks.Clear();
		for (int i = 0; i < this.bloodList.Count; i++)
		{
			GameObject gameObject = this.bloodList[i];
			if (!(gameObject == null))
			{
				this.bloodList[i] = null;
				UnityEngine.Object.Destroy(gameObject);
			}
		}
		this.LoadBlock();
		this.LoadUI();
	}

	private void OnClickBlock(G1Block block)
	{
		if (Game1DataLoader.GetInstance().CurPropId != 0)
		{
			this.m_dobleTotal = 0;
			ParticlesControl.GetInstance().StopChooseAllEffic();
			if (Game1DataLoader.GetInstance().CurPropId == 1)
			{
               
                if(AdsControl.Instance.GetRewardAvailable())
                {

                    AdsControl.Instance.PlayDelegateRewardVideo(delegate
                    {
                        Game1DataLoader.GetInstance().IsPlaying = true;
                        this.ControlPropsPannel(true);
                        this.UseProps(block);
                        this.m_markTips = true;
                    });
                }

            }
            else
			{
                Game1DataLoader.GetInstance().IsPlaying = true;
				this.ControlPropsPannel(true);
				this.UseProps(block);
			}
		}
		else
		{
			this.MoveBloodToMap(block);
		}
		this.BeginAllParticle();
	}

	private void MoveBloodToMap(G1Block toObj)
	{
		if (this.m_guideStatus == 0)
		{
			this.m_guideStatus = 1;
			PlayerPrefs.SetInt("LocalData_guide_game0102", 1);
			DOTween.Kill(this.m_figner, false);
			this.ToMask(this.m_transformList, "", true, Vector3.zero);
		}
		int blood = Game1DataLoader.GetInstance().GetBlood();
        Game1DataLoader.GetInstance().AddNumber(toObj.Index, blood);
		int number = Game1DataLoader.GetInstance().GetNumber(toObj.Index);
		GameObject obj = this.bloodList[0];
		obj.transform.SetParent(base.transform);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(obj.transform.DOMove(toObj.transform.position, 0.2f, false));
		sequence.InsertCallback(0.2f, delegate
		{
			if (number <= 0)
			{
                Game1DataLoader.GetInstance().FreeBlock(toObj.gameObject);
			}
			else
			{
				toObj.setNum(number);
			}
			this.m_dobleTotal = 0;
            Game1DataLoader.GetInstance().IsPlaying = false;
            Game1DataLoader.GetInstance().Delete(toObj.Index);
			TaskData.GetInstance().Add(100102, 1, true);
			this.GameOver();
		});
		if (number > 0)
		{
			sequence.AppendCallback(delegate
			{
				obj.GetComponent<G1BlockEvent>().FadeOut(toObj.GetCurrentColor());
			});
			sequence.Append(obj.transform.DOScale(1.5f, 0.5f));
		}
		sequence.OnComplete(delegate
		{
			UnityEngine.Object.Destroy(obj);
		});
		TweenCallback _tween = null;
		for (int i = 0; i < 5; i++)
		{
			if (i + 1 < 5)
			{
				this.bloodList[i] = this.bloodList[i + 1];
				if (this.bloodList[i + 1] != null)
				{
					this.bloodList[i + 1].transform.DOKill(false);
					this.bloodList[i + 1].transform.DOLocalMove(this.bloodPosList[i].transform.localPosition, 0.2f, false);
					if (i == 0)
					{
						Tweener arg_241_0 = this.bloodList[i + 1].transform.DOScale(this.bloodPosList[i].transform.localScale, 0.2f);
						TweenCallback arg_241_1;
						if ((arg_241_1 = _tween) == null)
						{
							arg_241_1 = (_tween = delegate
							{
								this.bloodList[0].GetComponent<G1BlockEvent>().DisableDrag = false;
							});
						}
						arg_241_0.OnComplete(arg_241_1);
					}
					else
					{
						this.bloodList[i + 1].transform.DOScale(this.bloodPosList[i].transform.localScale, 0.2f);
					}
				}
			}
			else
			{
				this.bloodList[i] = null;
			}
		}
        Game1DataLoader.GetInstance().IsPlaying = true;
	}

	private void onBegainDragLife(GameObject obj, PointerEventData eventData)
	{
		if (obj != this.bloodList[0])
		{
			return;
		}
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().HeartIndex != -1)
		{
			return;
		}
		obj.transform.DOKill(false);
		obj.transform.DOScale(1f, 0.1f);
	}

	private void OnDragLife(GameObject obj, PointerEventData eventData)
	{
		if (obj != this.bloodList[0])
		{
			return;
		}
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().HeartIndex != -1)
		{
			return;
		}
		this.m_markTips = false;
		obj.transform.DOKill(false);
		obj.transform.DOScale(1f, 0.1f);
		if (this.m_guideStatus != 0 && obj.transform.parent != base.transform)
		{
			obj.transform.SetParent(base.transform);
		}
		Vector2 a;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out a);
		obj.transform.localPosition = a + new Vector2(0f, 100f);
	}

	private void OnEndDragLife(GameObject obj, PointerEventData eventData)
	{
		if (obj != this.bloodList[0])
		{
			return;
		}
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().HeartIndex != -1)
		{
			return;
		}
		this.m_markTips = true;
		if (!eventData.dragging)
		{
			obj.GetComponent<G1BlockEvent>().DisableDrag = true;
			obj.transform.DOKill(false);
			obj.transform.DOScale(this.bloodPosList[0].transform.localScale, 0.1f).OnComplete(delegate
			{
				obj.GetComponent<G1BlockEvent>().DisableDrag = false;
			});
			return;
		}
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, obj.transform.position);
		if (!RectTransformUtility.RectangleContainsScreenPoint(this.gameBox.GetComponent<RectTransform>(), screenPoint, eventData.pressEventCamera))
		{
			obj.GetComponent<G1BlockEvent>().DisableDrag = true;
			obj.transform.DOKill(false);
			obj.transform.DOLocalMove(base.transform.InverseTransformPoint(this.bloodPosList[0].transform.position), 0.1f, false);
			obj.transform.DOScale(this.bloodPosList[0].transform.localScale, 0.1f).OnComplete(delegate
			{
				obj.GetComponent<G1BlockEvent>().DisableDrag = false;
			});
			return;
		}
		Vector2 vector = this.gameBox.GetComponent<RectTransform>().InverseTransformPoint(obj.transform.position);
		Vector2 vector2 = this.gameBox.GetComponent<RectTransform>().sizeDelta;
		float num = vector2.x / 2f + vector.x;
		double arg_15B_0 = (double)(vector2.y / 2f + vector.y);
		int num2 = (int)Math.Floor((double)(num / 120f));
		int num3 = (int)Math.Floor(arg_15B_0 / (double)120f);
		if (num2 < 0 || num2 >= 5)
		{
			return;
		}
		if (num3 < 0 || num3 >= 5)
		{
			return;
		}
		int index = Game1DataLoader.GetInstance().GetIndex(num3, num2);
        G1Block component = this.blocks[index].GetComponent<G1Block>();
		if (this.m_guideStatus == 0)
		{
			if (index == 12)
			{
				this.MoveBloodToMap(component);
			}
			else
			{
				obj.GetComponent<G1BlockEvent>().DisableDrag = true;
				obj.transform.DOKill(false);
				obj.transform.DOLocalMove(base.transform.InverseTransformPoint(this.bloodPosList[0].transform.position), 0.1f, false);
				obj.transform.DOScale(this.bloodPosList[0].transform.localScale, 0.1f).OnComplete(delegate
				{
					obj.GetComponent<G1BlockEvent>().DisableDrag = false;
				});
			}
		}
		else
		{
			this.MoveBloodToMap(component);
		}
		AudioManager.GetInstance().PlayEffect("sound_eff_click_1");
	}

	private void UseProps(G1Block block)
	{
		this.PlayUseProp(block, Game1DataLoader.GetInstance().CurPropId, delegate
		{
			foreach (int current in Game1DataLoader.GetInstance().Use(block.Index))
			{
				if (current < this.blocks.Count)
				{
                    G1Block g = this.blocks[current];
					if (!(g == null))
					{
						ParticlesControl.GetInstance().PlayExplodeEffic(g.transform.position, g.GetCurrentColor());
                        Game1DataLoader.GetInstance().FreeBlock(g.gameObject);
					}
				}
			}
			Sequence _sequence = DOTween.Sequence();
            _sequence.AppendInterval(0.5f);
			TweenCallback _tween;
			if ((_tween = Game1Manager.FindPathClass._callback) == null)
			{
                _tween = (Game1Manager.FindPathClass._callback = new TweenCallback(Game1Manager.FindPathClass._findPathFunc.UseProps));
			}
            _sequence.AppendCallback(_tween);
		});
		
	}

	private void Delete(List<int> list, int index)
	{
		AudioManager.GetInstance().PlayEffect("sound_eff_clear_1");
		List<Game1Manager.PathRDM> list2 = this.FindPath(list, index);
		if (list2.Count < 0)
		{
			return;
		}
		this.m_dobleTotal++;
		int count = list2[0].paths.Count;
		Sequence sequence = DOTween.Sequence();
		foreach (Game1Manager.PathRDM current in list2)
		{
            G1Block g = this.blocks[current.index];
			g.ShowScore();
			this.blocks[current.index] = null;
			int row = Game1DataLoader.GetInstance().GetRow(current.index);
			int col = Game1DataLoader.GetInstance().GetCol(current.index);
			if (current.paths.Count >= 2)
			{
				Vector2 b = new Vector2((float)current.paths[0].x, (float)current.paths[0].y);
				Vector2 vector = new Vector2((float)current.paths[1].x, (float)current.paths[1].y) - b;
				int index2 = Game1DataLoader.GetInstance().GetIndex(row + (int)vector.y, col + (int)vector.x);
				Tween t = g.DelayMove(index2, (float)(count - current.paths.Count) * 0.1f);
				sequence.Insert(0f, t);
			}
		}
		sequence.AppendInterval(0.1f);
		sequence.OnComplete(delegate
		{
			this.PlayDoubleAni();
			this.blocks[index].setNum(Game1DataLoader.GetInstance().GetNumber(index));
			this.AddNewLife(this.blocks[index]);
            Game1DataLoader.GetInstance().Down();
		});
        Game1DataLoader.GetInstance().IsPlaying = true;
		this.RefreshScore(true);
	}

	private List<Game1Manager.PathRDM> FindPath(List<int> list, int index)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 5;
		int num4 = 5;
		foreach (int current in list)
		{
			int row = Game1DataLoader.GetInstance().GetRow(current);
			int col = Game1DataLoader.GetInstance().GetCol(current);
			if (row > num)
			{
				num = row;
			}
			if (row < num3)
			{
				num3 = row;
			}
			if (col > num2)
			{
				num2 = col;
			}
			if (col < num4)
			{
				num4 = col;
			}
		}
		int row2 = Math.Abs(num - num3) + 1;
        Assets.Scripts.Utils.Grid grid = new Assets.Scripts.Utils.Grid(Math.Abs(num2 - num4) + 1, row2);
		List<sGridRMD> list2 = new List<sGridRMD>();
		int py = 0;
		int px = 0;
		int i = num3;
		int num5 = 0;
		while (i < num + 1)
		{
			int j = num4;
			int num6 = 0;
			while (j < num2 + 1)
			{
				int index2 = Game1DataLoader.GetInstance().GetIndex(i, j);
				grid.getNode(num6, num5).isWalk = list.Contains(index2);
				if (index2 == index)
				{
					py = num5;
					px = num6;
				}
				else if (list.Contains(index2))
				{
					list2.Add(new sGridRMD(i, j, num5, num6));
				}
				j++;
				num6++;
			}
			i++;
			num5++;
		}
		AStar aStar = new AStar(grid, DiagonalMovement.NEVER, HeuristicType.MANHATTAN);
		List<Game1Manager.PathRDM> list3 = new List<Game1Manager.PathRDM>();
		foreach (sGridRMD current2 in list2)
		{
			int index3 = Game1DataLoader.GetInstance().GetIndex(current2.gameRow, current2.gameCol);
			if (index3 != index)
			{
                Game1DataLoader.GetInstance().GetRow(index3);
                Game1DataLoader.GetInstance().GetCol(index3);
				list3.Add(new Game1Manager.PathRDM
				{
					paths = aStar.Find(new AVec2(current2.gridCol, current2.gridRow), new AVec2(px, py)),
					index = index3
				});
			}
		}
		List<Game1Manager.PathRDM> _pathList = list3;
		Comparison<Game1Manager.PathRDM> _com;
		if ((_com = Game1Manager.FindPathClass.camparisonFunc) == null)
		{
            _com = (Game1Manager.FindPathClass.camparisonFunc = new Comparison<Game1Manager.PathRDM>(Game1Manager.FindPathClass._findPathFunc.FindPathFunc));
		}
        _pathList.Sort(_com);
		return list3;
	}

	private void AddNewLife(G1Block block)
	{
		int num = Game1DataLoader.GetInstance().AddLife();
		if (num == -1)
		{
			return;
		}
		int number = Game1DataLoader.GetInstance().BloodList[num];
        G1BlockEvent _even = this.CreateNewLife(number, this.bloodBox, num);
        _even.transform.position = this.bloodPosList[num].transform.position;
        _even.transform.localScale = new Vector3(1f, 1f, 1f);
        _even.transform.DOScale(this.bloodPosList[num].transform.localScale, 0.3f);
	}

	private void Drop(List<sDropData> dropList, List<int> newList)
	{
		Sequence sequence = DOTween.Sequence();
		foreach (sDropData current in dropList)
		{
            G1Block g = this.blocks[current.srcIdx];
			this.blocks[current.srcIdx] = null;
			this.blocks[current.dstIdx] = g;
			g.Index = current.dstIdx;
			Tween t = g.Move(current.dstIdx);
			sequence.Insert(0f, t);
		}
		for (int i = 0; i < 5; i++)
		{
			int num = 0;
			foreach (int current2 in newList)
			{
				int col = Game1DataLoader.GetInstance().GetCol(current2);
				if (i == col)
				{
                    Game1DataLoader.GetInstance().GetRow(current2);
					int number = Game1DataLoader.GetInstance().GetNumber(current2);
                    G1Block g2 = Game1DataLoader.GetInstance().CreateBlock(number, current2, this.gameBox);
					g2.SetPosition(5 + num, i);
					this.blocks[current2] = g2;
					Tween t2 = g2.Move(current2);
					sequence.Insert(0f, t2);
					num++;
				}
			}
		}
		sequence.AppendInterval(0.2f);
		sequence.OnComplete(delegate
		{
            Game1DataLoader.GetInstance().IsPlaying = false;
            Game1DataLoader.GetInstance().AutoDelete();
			this.GameOver();
		});
        Game1DataLoader.GetInstance().IsPlaying = true;
	}

	private void PlayDoubleAni()
	{
		if (this.m_dobleTotal < 2)
		{
			return;
		}
		this.m_img_double.transform.Find("txt").GetComponent<Text>().text = string.Format("{0}", this.m_dobleTotal);
		DOTween.Kill(this.m_img_double, false);
		Sequence _sequence = DOTween.Sequence();
		this.m_img_double.gameObject.SetActive(true);
		this.m_img_double.DOKill(false);
		this.m_img_double.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        _sequence.Append(this.m_img_double.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
        _sequence.AppendInterval(0.5f);
        _sequence.OnComplete(delegate
		{
			this.m_img_double.gameObject.SetActive(false);
		});
		UnityEngine.Debug.Log("Double:" + this.m_dobleTotal);
	}

	private void PlayUseProp(G1Block toBlock, int propID, Action callfunc)
	{
		if (propID > this.m_img_props.Length)
		{
			return;
		}
		Transform image = this.m_img_props[propID - 1];
		Vector3 vector = base.GetComponent<RectTransform>().InverseTransformPoint(toBlock.transform.position);
		Vector3 vector2 = new Vector3(vector.x, 700f, 0f);
		image.localPosition = vector2;
		image.gameObject.SetActive(true);
		DOTween.Kill(image, false);
		float num = 800f;
		Sequence sequence = DOTween.Sequence();
		switch (propID)
		{
		case 1:
		{
			float num2 = Math.Abs(vector.y - vector2.y) / num;
			sequence.Append(image.transform.DOLocalMove(vector, num2, false).SetEase(Ease.OutBounce));
			break;
		}
		case 2:
		{
			vector = new Vector3(vector.x + 80f, vector.y, vector.z);
			float num2 = Math.Abs(vector.y - vector2.y) / num;
			image.transform.localRotation = default(Quaternion);
			sequence.Append(image.transform.DOLocalMove(vector, num2, false).SetEase(Ease.OutBounce));
			sequence.AppendInterval(0.2f);
			sequence.Append(image.transform.DOLocalRotate(new Vector3(0f, 0f, -50f), 0.2f, RotateMode.LocalAxisAdd));
			sequence.Append(image.transform.DOLocalRotate(new Vector3(0f, 0f, 90f), 0.1f, RotateMode.LocalAxisAdd));
			break;
		}
		case 3:
		{
			vector2 = new Vector3(0f, vector2.y, vector2.y);
			vector = this.gameBox.transform.localPosition;
			float num2 = Math.Abs(vector.y - vector2.y) / num;
			image.transform.localScale = Vector3.one;
			image.GetComponent<Image>().color = Color.white;
			sequence.Append(image.transform.DOLocalMove(vector, num2, false).SetEase(Ease.OutBounce));
			sequence.AppendInterval(0.2f);
			sequence.Append(image.transform.DOScale(15f, 0.5f));
			sequence.Insert(num2 + 0.2f, image.GetComponent<Image>().DOFade(0f, 0.5f));
			break;
		}
		}
		sequence.OnComplete(delegate
		{
			image.gameObject.SetActive(false);
			Action expr_17 = callfunc;
			if (expr_17 == null)
			{
				return;
			}
			expr_17();
		});
	}

	private void OnRandomHeart()
	{
		DOTween.Kill(this.m_img_heart, false);
		this.m_img_heart.transform.DOKill(false);
		this.m_img_heart.SetActive(true);
		this.m_img_heart.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
		Sequence _sequence = DOTween.Sequence();
        _sequence.Append(this.m_img_heart.transform.DOScale(1.1f, 0.5f));
        _sequence.Append(this.m_img_heart.transform.DOScale(1f, 0.3f));
        _sequence.OnComplete(delegate
		{
			Sequence _sequence2 = DOTween.Sequence();
            _sequence2.Append(this.m_img_heart.transform.DOScale(1.1f, 0.5f));
            _sequence2.Append(this.m_img_heart.transform.DOScale(1f, 0.5f));
            _sequence2.SetLoops(-1);
            _sequence2.SetTarget(this.m_img_heart);
		});
        _sequence.SetTarget(this.m_img_heart);
		int num = Game1DataLoader.GetInstance().HeartIndex / 5;
		int num2 = Game1DataLoader.GetInstance().HeartIndex % 5;
		Vector3 position = new Vector3((float)(num2 * 120 + 60 - 300), (float)(num * 120 + 60 - 300), 0f);
		this.m_img_heart.transform.position = this.gameBox.transform.TransformPoint(position);
	}

	public void UseHeart(int idx)
	{
		DOTween.Kill(this.m_img_heart, false);
		int num = idx / 5;
		int num2 = idx % 5;
		Vector3 position = new Vector3((float)(num2 * 120 + 60 - 300), (float)(num * 120 + 60 - 300), 0f);
		Vector3 endValue = this.gameBox.transform.TransformPoint(position);
		this.m_img_heart.transform.localScale = new Vector3(1f, 1f, 1f);
		this.m_img_heart.transform.DOJump(endValue, 0.1f, 1, 0.5f, false).OnComplete(delegate
		{
			this.blocks[idx].setNum(Game1DataLoader.GetInstance().GetNumber(idx));
			this.m_img_heart.SetActive(false);
			GameObject obj = UnityEngine.Object.Instantiate<GameObject>(this.blocks[idx].gameObject);
            G1Block _sequence3 = obj.GetComponent<G1Block>();
            _sequence3.transform.DOScale(1.5f, 0.5f);
            _sequence3.FadeOut().OnComplete(delegate
			{
                Game1DataLoader.GetInstance().HeartIndex = -1;
                Game1DataLoader.GetInstance().AutoDelete();
				UnityEngine.Object.Destroy(obj);
			});
			obj.transform.SetParent(this.transform, false);
			obj.transform.position = this.blocks[idx].transform.position;
			AudioManager.GetInstance().PlayEffect("sound_eff_click_1");
			this.GameOver();
		});
	}

	private void PlayNewNumber(int number)
	{
		GameObject original = Resources.Load("Prefabs/G00104") as GameObject;
		GameObject node = UnityEngine.Object.Instantiate<GameObject>(original, base.transform, false);
		node.transform.Find("prefab_max").GetComponent<G1Block>().setNum(number);
		node.transform.localScale = new Vector3(1f, 0f, 0f);
		Sequence _sequence4 = DOTween.Sequence();
        _sequence4.Append(node.transform.DOScaleY(1f, 0.1f));
        _sequence4.AppendInterval(1f);
        _sequence4.OnComplete(delegate
		{
			UnityEngine.Object.Destroy(node);
		});
		AudioManager.GetInstance().PlayEffect("sound_eff_newNum");
	}

	private void GameOver()
	{
		if (Game1DataLoader.GetInstance().IsPlaying)
		{
			return;
		}
		if (Game1DataLoader.GetInstance().HeartIndex != -1)
		{
			return;
		}
		if (!Game1DataLoader.GetInstance().IsGameOver())
		{
			return;
		}
		if (Game1DataLoader.GetInstance().FinishCount < 1000)
		{
            Game1DataLoader.GetInstance().FinishCount++;
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/finish") as GameObject);
			gameObject.GetComponent<Finish>().Load(1, Game1DataLoader.GetInstance().GetMapMaxNumber());
			DialogManager.GetInstance().show(gameObject, true);
		}
		else
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00102") as GameObject);
			gameObject2.GetComponent<G1UIManager>().Load(Game1DataLoader.GetInstance().Score, Game1DataLoader.GetInstance().MaxScore);
			DialogManager.GetInstance().show(gameObject2, true);
		}
		this.m_markTips = false;
	}

	private void ControlPropsPannel(bool isDel)
	{
		if (isDel)
		{
			for (int i = 0; i < this.m_tranformObjs.Count; i++)
			{
				if (i < this.m_tranformObjs.Count - 1)
				{
					this.m_tranformObjs[i].transform.SetParent(this.gameBox.transform, true);
				}
				else
				{
					this.m_tranformObjs[i].transform.SetParent(this.m_pannel_props.transform, true);
					this.m_tranformObjs[i].transform.Find("img01").gameObject.SetActive(false);
				}
			}
			this.m_tranformObjs.Clear();
			GlobalEventHandle.EmitDoUseProps(true, this.m_tranformObjs);
			return;
		}
		foreach (G1Block current in this.blocks)
		{
			this.m_tranformObjs.Add(current.gameObject);
		}
		GameObject gameObject = base.transform.Find(string.Format("img_pros/item{0}", Game1DataLoader.GetInstance().CurPropId)).gameObject;
		gameObject.transform.Find("img01").gameObject.SetActive(true);
		this.m_tranformObjs.Add(gameObject);
		GlobalEventHandle.EmitDoUseProps(false, this.m_tranformObjs);
	}

	private G1BlockEvent CreateNewLife(int number, GameObject parent, int idx)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00105") as GameObject);
		gameObject.transform.SetParent(parent.transform, false);
        G1BlockEvent _event = gameObject.GetComponent<G1BlockEvent>();
        _event.Init(number, idx);
        _event.OnDownHandle = new Action<GameObject, PointerEventData>(this.onBegainDragLife);
        _event.OnDragHandle = new Action<GameObject, PointerEventData>(this.OnDragLife);
        _event.OnUpHandle = new Action<GameObject, PointerEventData>(this.OnEndDragLife);
		this.bloodList[idx] = gameObject.gameObject;
        return _event;
	}

	private void runGuide()
	{
		this.m_guideStatus = PlayerPrefs.GetInt("LocalData_guide_game0102", 0);
		if (this.m_guideStatus != 0)
		{
			this.m_mask.SetActive(false);
			return;
		}
		this.m_transformList.Add(new Game1Manager.TransformControl(this.gameBox.transform, this.blocks[11].transform));
		this.m_transformList.Add(new Game1Manager.TransformControl(this.gameBox.transform, this.blocks[12].transform));
		this.m_transformList.Add(new Game1Manager.TransformControl(this.gameBox.transform, this.blocks[13].transform));
		this.m_transformList.Add(new Game1Manager.TransformControl(this.bloodBox.transform, this.bloodList[0].transform));
		this.ToMask(this.m_transformList, "", false, Vector3.zero);
		DOTween.Kill(this.m_figner, false);
		Sequence _sequence = DOTween.Sequence();
        _sequence.AppendCallback(delegate
		{
			this.m_figner.transform.localPosition = this.m_mask.transform.InverseTransformPoint(this.blocks[12].transform.position) + this.m_fingerRect2;
			this.m_figner.gameObject.SetActive(true);
		});
        _sequence.Append(this.m_figner.transform.DOBlendableLocalMoveBy(new Vector3(0f, -10f, 0f), 0.5f, false));
        _sequence.Append(this.m_figner.transform.DOBlendableLocalMoveBy(new Vector3(0f, 10f, 0f), 0.5f, false));
        _sequence.SetLoops(-1);
        _sequence.SetTarget(this.m_figner);
		Canvas _canvas = this.bloodList[0].AddComponent<Canvas>();
        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 13;
		this.bloodList[0].AddComponent<GraphicRaycaster>().enabled = true;
	}

	private void ToMask(List<Game1Manager.TransformControl> list, string txt, bool isOut, Vector3 tipsPos)
	{
		this.m_mask.SetActive(!isOut);
		if (isOut)
		{
			foreach (Game1Manager.TransformControl current in list)
			{
				current.self.SetParent(current.parent, true);
			}
			this.m_transformList.Clear();
			return;
		}
		using (List<Game1Manager.TransformControl>.Enumerator enumerator = list.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.self.SetParent(this.m_mask.transform, true);
			}
		}
	}
}
