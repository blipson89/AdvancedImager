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
		protected Sitecore.Web.UI.HtmlControls.Input CropId;
		protected Sitecore.Web.UI.HtmlControls.Input CropRatio;
		private const string RibbonPath = "/sitecore/content/Applications/Media/AdvancedImager/Ribbon";
		public override void HandleMessage(Message message)
		{
			Error.AssertObject(message, "message");
			CommandContext context = GetSaveContext(message);
			message = Message.Parse(this, message.ToString().Replace("item:save", "advimager:save"));
			switch (message.Name)
			{
				case "item:save":
				case "advimager:save":
				case "advimager:createcrop":
				case "advimager:saveas":
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
			CropId.Value = message.Arguments["id"];
			UpdateRibbon();
			Item crop = Client.CoreDatabase.GetItem(CropId.Value);
			if (crop != null)
			{
				string ratio = crop.Fields[Constants.CropRatioField].Value;
				CropRatio.Value = ratio;
				SheerResponse.Eval($"setAspectRatio('{ratio}');");
			}
			else
			{
				CropRatio.Value = "none";
				SheerResponse.Eval("setAspectRatio();");
			}
		}

		protected virtual CommandContext GetSaveContext(Message message)
		{
			string language = WebUtil.GetQueryString("la");
			string version = WebUtil.GetQueryString("vs");
			MediaItem parent = GetMediaItem(ItemID, Language.Parse(language), Version.Parse(version));
			var context = new CommandContext(parent);
			context.Parameters[Constants.CropRatio] = CropRatio.Value;
			context.Parameters[Constants.CropId] = CropId.Value;
			string imageData = CropInfo.Value;
			string base64Data = Regex.Match(imageData, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
			context.Parameters[Constants.Base64Data] = base64Data;
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
			var ribbon = new Ribbon {ID = "AdvImagerRibbon"};
			Item contextItem = null;
			if (!string.IsNullOrEmpty(ItemID))
				contextItem = Context.ContentDatabase.GetItem(ItemID);
			ribbon.CommandContext = new CommandContext(contextItem);
			ribbon.ShowContextualTabs = false;
			ribbon.CommandContext.Parameters["HasFile"] = HasFile.Disabled ? "0" : "1";
			ribbon.CommandContext.Parameters[Constants.CropRatio] = CropRatio.Value;
			ribbon.CommandContext.Parameters[Constants.CropId] = CropId.Value;
			Item ribbonItem = Context.Database.GetItem(RibbonPath);
			Error.AssertItemFound(ribbonItem, RibbonPath);
			ribbon.CommandContext.RibbonSourceUri = ribbonItem.Uri;
			RibbonPanel.InnerHtml = HtmlUtil.RenderControl(ribbon);
			SheerResponse.Eval("initEventListeners();");
		}
	}
}