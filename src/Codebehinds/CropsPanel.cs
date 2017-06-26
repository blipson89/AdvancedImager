using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Web.UI.WebControls;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.XmlControls;

namespace AdvancedImager.Codebehinds
{
	public class CropsPanel : CustomControl
	{
		private Scrollbox _crops;
		private Literal _selectedCropLink;
		private static IDictionary<Guid, Item> CropItems => Client.CoreDatabase.GetItem("/sitecore/content/Applications/Media/AdvancedImager/Crops").Children.ToDictionary(k => k.ID.Guid);
		public CropsPanel(Item button, CommandContext context) : base(button, context)
		{
		}
		protected override void Render(HtmlTextWriter writer)
		{
			var mainControl = ControlFactory.GetControl(Constants.CropsControlName) as XmlControl;
			Assert.IsNotNull(mainControl, "mainControl"); // ReSharper disable once PossibleNullReferenceException

			_crops = mainControl["Crops"] as Scrollbox;
			_selectedCropLink = mainControl["SelectedCrop"] as Literal;
			SetupCropLink();
			AddControl("(None)", "No aspect ratio selected", "advimager:setcrop(id=)");
			foreach (Item crop in CropItems.Values)
			{
				AddControl(crop);
			}
			using (var output = new HtmlTextWriter(new StringWriter()))
			{
				mainControl.Initialize();
				mainControl.RenderControl(output);
				writer.Write(output.InnerWriter.ToString());
			}

		}

		private void SetupCropLink()
		{
			Guid cropId;
			var text = "(None)";
			if (Guid.TryParse(CommandContext.Parameters[Constants.CropId], out cropId))
			{
				text = CropItems[cropId].GetUIDisplayName();
			}
			
			
			string str1 = HttpUtility.HtmlEncode("javascript:scForm.postEvent(this,event,'advimager:openmenu()')");
			var tag = new StringBuilder();
			tag.Append("<a ");
			tag.Append("class='scEditorHeaderVersionsLanguage scEditorHeaderButton scButton'");
			tag.Append("href='#'");
			tag.AppendFormat("onclick=\"{0}\"", str1);
			tag.Append(">");
			tag.Append(text);
			tag.Append(" <img src='/sitecore/shell/themes/standard/Images/ribbondropdown.gif' class='scEditorHeaderVersionsLanguageGlyph' alt='' border='0' />");
			tag.Append("</a>");
			_selectedCropLink.Text = tag.ToString();
		}

		private void AddControl(Item crop)
		{
			string description = crop.Fields[Constants.CropPurposeField].Value;
			AddControl(crop.GetUIDisplayName(), description, $"advimager:setcrop(id={crop.ID})");
		}

		private void AddControl(string header, string description, string clickEvent)
		{
			var control = ControlFactory.GetControl(Constants.CropsOptionsControlName) as XmlControl;
			Assert.IsNotNull(control, typeof(XmlControl)); // ReSharper disable once AssignNullToNotNullAttribute
			_crops.Controls.Add(control);

			control["Header"] = header;
			control["Description"] = description;
			control["Click"] = clickEvent;
			control["ClassName"] = "scMenuPanelItem";
		}

	}
}