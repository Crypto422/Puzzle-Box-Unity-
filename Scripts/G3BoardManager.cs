using Assets.Scripts.Configs;
using Assets.Scripts.GameManager;
using Assets.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
/*
 * this function will help you to generate board from level data
 */
public class G3BoardManager : MonoBehaviour
{
    //On Return Function (back to home)
	[Serializable]
	private sealed class CommonClass
	{
		public static readonly G3BoardManager.CommonClass _commonClass = new G3BoardManager.CommonClass();

		public static Action _action;

		internal void OnClickReturn()
		{
		}
	}
   //finger shown when you play guide in first time
	public GameObject guide_finger;

	private Vector2 mousePosition = new Vector2(0f, 0f);

	private Vector2 mouseClickPosition = new Vector2(0f, 0f);

	public GameObject gameBox;

	private bool m_isCanClick;
    //list block of background
	private List<G3Block> m_boards = new List<G3Block>();
    //list line or level block in map
	private List<G3Line> m_maps = new List<G3Line>();

	private Queue m_recovery = new Queue();
    //list line or level block in current game play
    private List<G3Line> m_blocks = new List<G3Line>();

	private G3BoardGenerator Model;

	private int isDragTarget;

	private void Awake()
	{
        //load board from level data
		this.Model = G3BoardGenerator.GetInstance();
		this.InitEvent();
	}

	private void Start()
	{
		this.m_isCanClick = true;
		this.LoadUI();
	}

	private void Update()
	{
		if (this.Model.Mask_isVictory)
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			this.mouseClickPosition = this.InputPosTransformToLocalPos(UnityEngine.Input.mousePosition, this.gameBox);
			this.CheckMouseClick(this.mouseClickPosition);
		}
		if (Input.GetMouseButton(0))
		{
			this.mousePosition = this.InputPosTransformToLocalPos(UnityEngine.Input.mousePosition, this.gameBox);
			this.CheckMouseDrop(this.mousePosition);
		}
		if (Input.GetMouseButtonUp(0))
		{
			Vector2 pos = this.InputPosTransformToLocalPos(UnityEngine.Input.mousePosition, this.gameBox);
			this.CheckMouseUp(pos);
			this.RefreshBoard();
			this.isDragTarget = 0;
		}
	}

	private void CheckMouseClick(Vector2 pos)
	{
		if (this.m_isCanClick)
		{
			this.isDragTarget = this.Model.CheckClickPos(pos);
			return;
		}
		this.isDragTarget = -1;
	}

	private void CheckMouseDrop(Vector2 pos)
	{
		if (this.isDragTarget != -1)
		{
			int num = this.Model.CheckDropPos(this.isDragTarget, pos);
			if (num != -1)
			{
				this.isDragTarget = num;
			}
		}
	}

	private void CheckMouseUp(Vector2 pos)
	{
		this.Model.CheckUpPos(pos);
	}

	public void RefreshBoard()
	{
		for (int i = 0; i < this.Model.m_map_row; i++)
		{
			for (int j = 0; j < this.Model.m_map_col; j++)
			{
				Color_Node node = this.Model.GetNode(i, j);
				if (node.Type == G3BoardGenerator.Node_type.TARGET)
				{
					if (node.Next != node.Index || node.Per != node.Index)
					{
						this.m_boards[node.Index].SetColor(node.Color);
					}
					else
					{
						this.m_boards[node.Index].SetColor(0);
					}
				}
				else
				{
					this.m_boards[node.Index].SetColor(node.Color);
				}
			}
		}
		foreach (G3Line current in this.m_maps)
		{
			Color_Node node2 = this.Model.GetNode(current.Index);
			if (node2.Type == G3BoardGenerator.Node_type.TARGET)
			{
				if (this.Model.CheckIsConnected(node2))
				{
					current.ShowStar();
				}
				else
				{
					current.HideStar();
				}
			}
		}
	}

	public void InitEvent()
	{
		this.Model.DoReloadMapsHandle += new Action(this.ReloadMaps);
		this.Model.DoRefreshHandle += new Action(this.RefreshMaps);
		this.Model.DoAddBlockHandle += new Action<int>(this.AddBlock);
		this.Model.DoRemoveBlockHandle += new Action<int>(this.RemoveBlock);
		this.Model.OnClickReturnHandle = new Action<GameList>(this.OnClickReturn);
	}

	private void OnDestroy()
	{
		this.RemoveEvent();
	}
    //return levels
	public void OnClickReturn(GameList obj)
	{
		int arg = Configs.TG00301[this.Model.Map_config.ID.ToString()].Level;
		Action action1 = delegate
		{
            G3BoardGenerator.GetInstance().DestroyMap();
			GlobalEventHandle.EmitDoRefreshCheckPoint(this.Model.Map_config.G003);
			GM.GetInstance().SetSavedGameID(0);
			GM.GetInstance().ResetToNewGame();
			GM.GetInstance().ResetConsumeCount();
			GlobalEventHandle.EmitDoGoHome();
			GlobalEventHandle.EmitClickPageButtonHandle("main", 0);
			obj.HideTopBtn();
		};
		Action action2 = delegate
		{
			GM.GetInstance().SetSavedGameID(3);
            G3BoardGenerator.GetInstance().DestroyMap();
            G3BoardGenerator.GetInstance().StartNewGame(Configs.TG00301[this.Model.Map_config.ID.ToString()].ID);
		};
		Action action3;
		if ((action3 = G3BoardManager.CommonClass._action) == null)
		{
			action3 = (G3BoardManager.CommonClass._action = new Action(G3BoardManager.CommonClass._commonClass.OnClickReturn));
		}
		Utils.ShowPause(arg, action1, action2, action3).SetTitle("TXT_NO_10007");
	}
    //reload current map
	public void ReloadMaps()
	{
		this.LoadBoard();
		for (int i = 0; i < this.Model.m_map_row; i++)
		{
			for (int j = 0; j < this.Model.m_map_col; j++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00302") as GameObject);
				gameObject.transform.SetParent(this.gameBox.transform.Find("game_block"), false);
				gameObject.SetActive(true);
				Color_Node node = this.Model.GetNode(i, j);
				gameObject.GetComponent<G3Line>().SetContentSize(this.Model.Cell_width, this.Model.Cell_height);
				gameObject.GetComponent<G3Line>().Init(node.Index, node.Color, node.Type);
				this.m_maps.Add(gameObject.GetComponent<G3Line>());
			}
		}
	}

	public G3Line CreateBlock()
	{
		GameObject gameObject;
		if (this.m_recovery.Count > 0)
		{
			gameObject = (this.m_recovery.Dequeue() as GameObject);
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00302") as GameObject);
		}
		gameObject.transform.SetParent(this.gameBox.transform.Find("game_line"), false);
		gameObject.SetActive(true);
		return gameObject.GetComponent<G3Line>();
	}

	public void RecoveryBlock(GameObject block)
	{
		block.SetActive(false);
		block.transform.DOKill(false);
		this.m_recovery.Enqueue(block);
	}

	public int ExistInBlocks(int index)
	{
		int num = 0;
		using (List<G3Line>.Enumerator enumerator = this.m_blocks.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Index == index)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public void AddBlock(int index)
	{
		if (this.ExistInBlocks(index) == -1)
		{
            G3Line g = this.CreateBlock();
			Color_Node node = this.Model.GetNode(index);
			g.SetContentSize(new Vector2(this.Model.Cell_width, this.Model.Cell_height));
			g.Init(node.Index, node.Color, G3BoardGenerator.Node_type.SEGMENT);
			this.m_blocks.Add(g);
			AudioManager.GetInstance().PlayEffect("sound_eff_click_2");
		}
	}

	public void RemoveBlock(int index)
	{
		int num = this.ExistInBlocks(index);
		if (num != -1)
		{
            G3Line g = this.m_blocks[num];
			this.m_blocks.RemoveAt(num);
			this.RecoveryBlock(g.gameObject);
		}
	}

	public void LoadUI()
	{
	}
    //load board from level data
	public void LoadBoard()
	{
		int num = 0;
		for (int i = 0; i < this.Model.m_map_row; i++)
		{
			for (int j = 0; j < this.Model.m_map_col; j++)
			{
				GameObject expr_21 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00304") as GameObject);
				expr_21.transform.SetParent(this.gameBox.transform.Find("game_board"), false);
				expr_21.SetActive(true);
                G3Block component = expr_21.GetComponent<G3Block>();
				component.SetContentSize(this.Model.Cell_width, this.Model.Cell_height);
				component.Init(num++, 0);
				this.m_boards.Add(component);
			}
		}
	}
    //show victory dialog
	public void ShowVictory()
	{
		this.m_isCanClick = false;
		AudioManager.GetInstance().PlayEffect("sound_eff_star_2");
		DOTween.Kill(this, false);
		Sequence sequence = DOTween.Sequence();
		for (int i = 0; i < this.Model.m_map_row; i++)
		{
			for (int j = 0; j < this.Model.m_map_col; j++)
			{
				Color_Node node = this.Model.GetNode(i, j);
				float num = (float)(i + j) / 10f;
				sequence.Insert(num, this.m_boards[node.Index].transform.DOScale(0.95f, 0.04f));
				sequence.Insert(num + 0.2f, this.m_boards[node.Index].transform.DOScale(1.05f, 0.04f));
				sequence.Insert(num + 0.4f, this.m_boards[node.Index].transform.DOScale(1f, 0.04f));
			}
		}
		sequence.AppendCallback(delegate
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load("Prefabs/G00303") as GameObject);
			gameObject.GetComponent<G3WinDialog>().Load(this.Model.Map_config.ID, this.Model.Map_config.Next, this.Model.Map_config.G003, this.Model.Map_config.Award, this.Model.Map_config.Target * 10);
			gameObject.GetComponent<G3WinDialog>().IsShowAward(GM.GetInstance().GetScoreRecord(3) + 1 <= this.Model.Map_config.ID);
			this.Model.SaveScore();
			this.Model.Mask_isVictory = true;
			DialogManager.GetInstance().show(gameObject, true);
		});
		sequence.SetTarget(this);
	}

	public void ResetDirection(G3Line block, Color_Node node)
	{
		if (node.Per == node.Index - this.Model.m_map_col)
		{
			block.SetDirection(G3BoardGenerator.Direction.DOWN);
			return;
		}
		if (node.Per == node.Index + 1)
		{
			block.SetDirection(G3BoardGenerator.Direction.LEFT);
			return;
		}
		if (node.Per == node.Index + this.Model.m_map_col)
		{
			block.SetDirection(G3BoardGenerator.Direction.UP);
			return;
		}
		if (node.Per == node.Index - 1)
		{
			block.SetDirection(G3BoardGenerator.Direction.RIGHT);
			return;
		}
		block.SetDirection(G3BoardGenerator.Direction.NONE);
	}

	private void RefreshMaps()
	{
		foreach (G3Line current in this.m_blocks)
		{
			Color_Node node = this.Model.GetNode(current.Index);
			this.ResetDirection(current, node);
			current.SetColor(node.Color);
		}
	}

	private void RemoveEvent()
	{
	}

	public void OnClickPrompted()
	{
		if (this.m_isCanClick)
		{
			this.Model.GetPrompted();
		}
	}

	public Vector2 InputPosTransformToLocalPos(Vector2 mousePosition, GameObject obj)
	{
		mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
		mousePosition = obj.transform.InverseTransformPoint(mousePosition);
		return mousePosition;
	}

	public void DestroyMap()
	{
		foreach (UnityEngine.Object arg_23_0 in this.m_recovery)
		{
			MonoBehaviour.print("Clear the recovery");
			UnityEngine.Object.Destroy(arg_23_0);
		}
		this.m_recovery.Clear();
		foreach (Component arg_6D_0 in this.m_blocks)
		{
			MonoBehaviour.print("Clear the block");
			UnityEngine.Object.Destroy(arg_6D_0.gameObject);
		}
		this.m_blocks.Clear();
		foreach (Component arg_BA_0 in this.m_maps)
		{
			MonoBehaviour.print("Clear the maps");
			UnityEngine.Object.Destroy(arg_BA_0.gameObject);
		}
		this.m_maps.Clear();
		foreach (Component arg_107_0 in this.m_boards)
		{
			MonoBehaviour.print("Clear the board");
			UnityEngine.Object.Destroy(arg_107_0.gameObject);
		}
		this.m_boards.Clear();
		this.m_isCanClick = true;
	}

	public Vector3 GetBlockPosition(int index)
	{
		return this.m_maps[index].GetPosition();
	}

	public void PlayHandAnimation(int idx = 0)
	{
		this.guide_finger.SetActive(true);
		Sequence sequence = DOTween.Sequence();
		Vector3 pos1 = new Vector3(0f, 0f, 0f);
		Vector3 blockPosition = new Vector3(0f, 0f, 0f);
		Vector3 blockPosition2 = new Vector3(0f, 0f, 0f);
		Vector3 blockPosition3 = new Vector3(0f, 0f, 0f);
		Vector3 blockPosition4 = new Vector3(0f, 0f, 0f);
		switch (idx)
		{
		case 0:
			pos1 = this.GetBlockPosition(19);
			blockPosition = this.GetBlockPosition(24);
			blockPosition2 = this.GetBlockPosition(20);
			blockPosition3 = this.GetBlockPosition(15);
			sequence.AppendCallback(delegate
			{
				this.guide_finger.transform.localPosition = pos1;
			});
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition, 0.4f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition2, 1.4f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition3, 0.3f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition3, 0.2f, false));
			sequence.SetLoops(-1);
			return;
		case 1:
			pos1 = this.GetBlockPosition(16);
			blockPosition = this.GetBlockPosition(6);
			blockPosition2 = this.GetBlockPosition(8);
			sequence.AppendCallback(delegate
			{
				this.guide_finger.transform.localPosition = pos1;
			});
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition, 0.5f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition2, 0.5f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition2, 0.2f, false));
			sequence.SetLoops(-1);
			return;
		case 2:
			pos1 = this.GetBlockPosition(13);
			blockPosition = this.GetBlockPosition(14);
			blockPosition2 = this.GetBlockPosition(4);
			blockPosition3 = this.GetBlockPosition(0);
			blockPosition4 = this.GetBlockPosition(10);
			sequence.AppendCallback(delegate
			{
				this.guide_finger.transform.localPosition = pos1;
			});
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition, 0.4f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition2, 0.8f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition3, 1.3f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition4, 0.6f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition4, 0.2f, false));
			sequence.SetLoops(-1);
			return;
		case 3:
			pos1 = this.GetBlockPosition(12);
			blockPosition = this.GetBlockPosition(17);
			blockPosition2 = this.GetBlockPosition(18);
			sequence.AppendCallback(delegate
			{
				this.guide_finger.transform.localPosition = pos1;
			});
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition, 0.4f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition2, 0.5f, false));
			sequence.Append(this.guide_finger.transform.DOLocalMove(blockPosition2, 0.2f, false));
			sequence.SetLoops(-1);
			return;
		default:
			this.StopHandAnimation();
			return;
		}
	}

	public void StopHandAnimation()
	{
		this.guide_finger.transform.DOKill(false);
		this.guide_finger.SetActive(false);
	}
}
