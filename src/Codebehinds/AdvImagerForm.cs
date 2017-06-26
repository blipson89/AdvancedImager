using System;
using System.IO;
using System.Text.RegularExpressions;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.Media.Imager;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls.Ribbons;
using Version = Sitecore.Data.Version;

namespace AdvancedImager.Codebehinds
{
	public class AdvImagerForm : ImagerForm
	{
		protected Sitecore.Web.UI.HtmlControls.Input CropInfo;
		protected Sitecore.Web.UI.HtmlControls.Input CropMimeType;
		protected Sitecore.Web.UI.HtmlControls.Input CropSize;
		public override void HandleMessage(Message message)
		{
			Error.AssertObject(message, "message");
			switch (message.Name)
			{
				case "item:save":
				case "advimager:save":
					CommandContext context = GetSaveContext(message);
					message = Message.Parse(this, message.ToString().Replace("item:save", "advimager:save"));
					Dispatcher.Dispatch(message, context);
					break;
				case "advimager:openmenu":
					HandleOpenMenu();
					break;
				case "advimager:setcrop":
					HandleSetCrop(message);
					break;
				default:
					base.HandleMessage(message);
					break;
			}

		}

		private void HandleOpenMenu()
		{
			UpdateRibbon();
			SheerResponse.SetAttribute("Crops", "class", "active");
		}

		private void HandleSetCrop(Message message)
		{
			CropSize.Value = message.Arguments["id"];
			UpdateRibbon();
			Item crop = Client.CoreDatabase.GetItem(CropSize.Value);
			if (crop != null)
			{
				string ratio = crop.Fields["Crop Ratio"].Value;
				SheerResponse.Eval($"setAspectRatio('{ratio}');");
			}
			else
			{
				SheerResponse.Eval("setAspectRatio();");
			}
		}

		protected virtual CommandContext GetSaveContext(Message message)
		{
			string language = WebUtil.GetQueryString("la");
			string version = WebUtil.GetQueryString("vs");
			MediaItem parent = GetMediaItem(ItemID, Language.Parse(language), Version.Parse(version));
			var context = new CommandContext(parent);
			string imageData = CropInfo.Value;
			string base64Data = Regex.Match(imageData, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
			context.Parameters["Base64Data"] = base64Data;
			return context;
		}

		private static Item GetMediaItem(string itemId, Language language, Version version)
		{
			return Context.ContentDatabase.GetItem(ID.Parse(itemId), language, version);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CropMimeType.Value = GetMimetype();
			UpdateRibbon();
		}

		private string GetExtension()
		{
			return Path.GetExtension(File)?.Substring(1);;
		}
		private string GetMimetype()
		{
			string extension = GetExtension();
			if (extension == "jpg") extension = "jpeg";
			return $"image/{extension}";
		}


		private void UpdateRibbon()
		{
			Ribbon ribbon = new Ribbon {ID = "AdvImagerRibbon"};
			Item obj1 = null;
			if (!string.IsNullOrEmpty(ItemID))
				obj1 = Context.ContentDatabase.GetItem(ItemID);
			ribbon.CommandContext = new CommandContext(obj1);
			ribbon.ShowContextualTabs = false;
			ribbon.CommandContext.Parameters["HasFile"] = HasFile.Disabled ? "0" : "1";
			ribbon.CommandContext.Parameters["CropSize"] = CropSize.Value;
			ribbon.CommandContext.Parameters["CropID"] = CropSize.Value;
			Item obj2 = Context.Database.GetItem("/sitecore/content/Applications/Media/AdvancedImager/Ribbon");
			Error.AssertItemFound(obj2, "/sitecore/content/Applications/Media/AdvancedImager/Ribbon");
			ribbon.CommandContext.RibbonSourceUri = obj2.Uri;
			RibbonPanel.InnerHtml = HtmlUtil.RenderControl(ribbon);
			SheerResponse.Eval("initEventListeners();");
		}
	}
}