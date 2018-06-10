﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KKManager.Data;

namespace KKManager
{
	public partial class formMain : Form
	{
		public formMain()
		{
			InitializeComponent();
		}

		private void formMain_Load(object sender, EventArgs e)
		{
			InitCards();
		}

		#region Cards

		private ImageList activeImageList { get; set; }

		private ConcurrentDictionary<string, Image> masterImageList { get; set; }

		private List<Size> ListViewCardSizes { get; set; } = new List<Size>
		{
			new Size(92, 128),
			new Size(183, 256)
		};


		private void InitCards()
		{
			InitCardBindings();

			masterImageList = new ConcurrentDictionary<string, Image>();
			activeImageList = new ImageList();

			activeImageList.ColorDepth = ColorDepth.Depth24Bit;
			activeImageList.ImageSize = new Size(183, 256);
			
			lsvCards.LargeImageList = activeImageList;


			lsvCards.BeginUpdate();

			foreach (string file in Directory.EnumerateFiles(@"M:\koikatu\UserData\chara\female", "*.png",
				SearchOption.AllDirectories))
			{

				string key = Path.GetFileName(file);

				using (MemoryStream mem = new MemoryStream(File.ReadAllBytes(file)))
				{
					Image image = Image.FromStream(mem);

					string itemName = key;

					if (Card.TryParseCard(() => File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read), out Card card))
					{
						itemName = $"{card.Parameter.lastname} {card.Parameter.firstname}";
					}

					masterImageList[key] = image;

					var item = lsvCards.Items.Add(key, itemName, key);

					if (card != null)
						item.Tag = card;
					else
					{
						item.ForeColor = Color.Red;
						item.Font = new Font(item.Font, FontStyle.Italic);
					}

				}
			}

			lsvCards.EndUpdate();

			cmbCardsViewSize.SelectedIndex = 0;
		}

		private void cmbCardsViewSize_SelectedIndexChanged(object sender, EventArgs e)
		{
			lsvCards.BeginUpdate();

			activeImageList.ImageSize = ListViewCardSizes[cmbCardsViewSize.SelectedIndex];

			foreach (Image image in activeImageList.Images)
			{
				image.Dispose();
			}

			activeImageList.Images.Clear();

			var arrr = masterImageList.OrderBy(x => x.Key).Select(x => x.Value).ToArray();

			activeImageList.Images.AddRange(arrr);

			for (int i = 0; i < activeImageList.Images.Count; i++)
			{
				string key = masterImageList.First(x => object.ReferenceEquals(x.Value, arrr[i])).Key;
				activeImageList.Images.SetKeyName(i, key);
				lsvCards.Items[key].ImageKey = key;
			}

			lsvCards.EndUpdate();
		}

		#region Card Databinding

		private BindingSource cardParameterSource = new BindingSource();

		private void InitCardBindings()
		{
			cardParameterSource = new BindingSource();
			cardParameterSource.DataSource = typeof(ChaFileParameter);

			txtCardFirstName.DataBindings.Add(nameof(Label.Text), cardParameterSource, nameof(ChaFileParameter.firstname));
			txtCardLastName.DataBindings.Add(nameof(Label.Text), cardParameterSource, nameof(ChaFileParameter.lastname));
			txtCardNickname.DataBindings.Add(nameof(Label.Text), cardParameterSource, nameof(ChaFileParameter.nickname));
		}

		private void SetCardDatabindings(Card card)
		{
			cardParameterSource.DataSource = card.Parameter;

			if (imgCard.Image != null)
			{
				imgCard.Image.Dispose();
				imgCard.Image = null;
			}

			if (imgCardFace.Image != null)
			{
				imgCardFace.Image.Dispose();
				imgCardFace.Image = null;
			}

			imgCard.Image = card.CardImage;
			imgCardFace.Image = card.CardFaceImage;

			lsvCardExtData.Items.Clear();

			if (card.Extended != null)
			{
				foreach (string key in card.Extended.Keys)
					lsvCardExtData.Items.Add(key);
			}
		}

		private void WriteToBindingCard()
		{
			cardParameterSource.EndEdit();
		}

		#endregion

		#endregion

		private void lsvCards_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lsvCards.SelectedIndices.Count == 0) 
				return;
			
			if (lsvCards.SelectedItems[0].Tag is Card card)
			{
				SetCardDatabindings(card);
			}
		}
	}
}