using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; //ADDED
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraExtraController : BasicGameController
{
	protected override string TaskDescriptionText
	{
		get
		{
			return "Write a headline for this photo!";
		}
	}

	protected override bool CapitalizeAnswers
	{
		get
		{
			return true;
		}
	}

	protected override IEnumerator StartState(GameController.State state)
	{
		yield return base.StartState(state);
		switch (state)
		{
		case GameController.State.Introduction:
		{
			RichPresenceManager.SetRichPresence("UYW_RP_ExtraExtra");
			this.InputContainer.SetActive(true);
			this.RevealContainer.SetActive(false);
			this.MainCamera.transform.position = new Vector3(-4.99f, -2.75f, 0f);
			this.MainCamera.orthographicSize = 2.15f;
			this._ActiveContent = (Singleton<ContentManager>.Instance.FindRandomContent(ContentManager.GameType.ExtraExtra) as ContentManager.ExtraExtraContent);
			//BEGIN MODIFICATION
			Sprite contentSprite;

			if (this._ActiveContent.ID > 999)
			{
				byte[] data = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/NewContent/ee" + this._ActiveContent.ID);
				Texture2D texture2D = new Texture2D(2, 2);
				texture2D.LoadImage(data);
				contentSprite = Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), 100f);
			}
			else
			{
				contentSprite = Resources.Load<Sprite>("Content/ExtraExtra/" + this._ActiveContent.ID + "/image");
			}
			if (contentSprite.bounds.extents.x > contentSprite.bounds.extents.y)
			{
				this._ActiveSize = this.Landscape;
			}
			//END MODIFICATION
			else
			{
				this._ActiveSize = this.Portrait;
			}
			for (int i = 0; i < this._ActiveSize.Images.Length; i++)
			{
				//BEGIN MODIFICATION
				this._ActiveSize.Images[i].sprite = contentSprite;
				this._ActiveSize.Images[i].preserveAspect = true;
			}

			Vector2 curSize = this._ActiveSize.InputContainer.GetComponent<RectTransform>().sizeDelta;
			Vector2 imgSize = this._ActiveSize.Images[0].rectTransform.sizeDelta;

			if (this._ActiveContent.ID > 999)
			{
				if (contentSprite.bounds.extents.x > contentSprite.bounds.extents.y){
					float margin = Mathf.Abs(curSize.x - imgSize.x);
					this._ActiveSize.InputContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(imgSize.x + margin, imgSize.y + margin);
				}
				else {
					float margin = Mathf.Abs(curSize.y - imgSize.y);
					this._ActiveSize.InputContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(imgSize.x + margin, imgSize.y + margin);
				}
			}
			//END MODIFICATION

			while (Singleton<LoadingScreen>.Instance.IsShowing)
			{
				yield return null;
			}
			if (!Singleton<GameManager>.Instance.NewPlayer || Singleton<GameManager>.Instance.IsFirstExtraExtra)
			{
			}
			this._ActiveSize.InputContainer.SetActive(true);
			this._ActiveSize.RevealContainer.SetActive(true);
			yield return Yielders.Seconds(1f);
			this.MainCamera.DOOrthoSize(5f, 1.5f).SetEase(Ease.InQuad);
			this.MainCamera.transform.DOMove(new Vector3(0f, 0f, -1f), 1.5f, false).SetEase(Ease.InQuad);
			base.ChangeState(GameController.State.RevealPrompt);
			break;
		}
		case GameController.State.RevealPrompt:
			if (Singleton<GameManager>.Instance.FamilyMode)
			{
				base.SetHouseAnswers(this._ActiveContent.FamilyModeHouseAnswers);
			}
			else
			{
				base.SetHouseAnswers(this._ActiveContent.HouseAnswers);
			}
			base.ChangeState(GameController.State.InputCheck);
			break;
		case GameController.State.RevealAnswers:
		{
			yield return Singleton<AudioManager>.Instance.PlayVoiceOverAndWait("12.X", Singleton<GameManager>.Instance.GetAnswerRevealClips());
			this.InputContainer.SetActive(false);
			this.RevealContainer.SetActive(true);
			this.VotingPaper.SetActive(false);
			this.NewspaperBase.transform.localScale = Vector3.zero;
			Singleton<AudioManager>.Instance.PlayMusic("EE_Theme", 1f, true);
			this.NewspaperBase.SetActive(true);
			GameController.Answer[] inputArray = this._UserInput[this._CurrentRound].ToArray();
			foreach (GameController.Answer inputPair in inputArray)
			{
				yield return this.ShowNewspaper(inputPair.Value, 5.25f);
				yield return base.ResolveInput(inputPair.User.ID);
			}
			base.ChangeState(GameController.State.VotingCheck);
			break;
		}
		case GameController.State.VotingCheck:
			this.NewspaperBase.SetActive(false);
			this.VotingPaper.SetActive(true);
			break;
		case GameController.State.RevealResults:
		{
			yield return Singleton<AudioManager>.Instance.PlayVoiceOverAndWait("36.X", Singleton<GameManager>.Instance.GetVoteRevealVO());
			this.NewspaperBase.SetActive(true);
			this.VotingPaper.SetActive(false);
			List<GameController.Answer> orderedAnswers = base.CalculateVoteOrderedInput();
			foreach (GameController.Answer inputPair2 in orderedAnswers)
			{
				yield return this.ShowNewspaper(inputPair2.Value, 1f);
				yield return base.ResolveVote(inputPair2.User.ID);
			}
			base.ChangeState(GameController.State.PostResults);
			break;
		}
		case GameController.State.EndOfGame:
			this.NewspaperBase.SetActive(false);
			break;
		}
		yield break;
	}

	private IEnumerator ShowNewspaper(string promptText, float waitDuration)
	{
		this.NewspaperBase.transform.localScale = Vector3.zero;
		this.NewspaperBase.transform.rotation = Quaternion.identity;
		this._ActiveSize.Text.text = promptText.ToUpperInvariant();
		this.NewspaperBase.transform.DOScale(1f, 1f);
		this.NewspaperBase.transform.DORotate(new Vector3(0f, 0f, 1440f), 1f, RotateMode.FastBeyond360).SetEase(Ease.Linear);
		yield return Yielders.Seconds(waitDuration + 1f);
		yield break;
	}

	public GameObject InputContainer;

	public GameObject RevealContainer;

	public GameObject NewspaperBase;

	public GameObject VotingPaper;

	public ExtraExtraController.NewspaperSize Landscape;

	public ExtraExtraController.NewspaperSize Portrait;

	public Camera MainCamera;

	private ContentManager.ExtraExtraContent _ActiveContent;

	private ExtraExtraController.NewspaperSize _ActiveSize;

	[Serializable]
	public class NewspaperSize
	{
		public GameObject InputContainer;

		public GameObject RevealContainer;

		public Image[] Images;

		public TextMeshProUGUI Text;
	}
}