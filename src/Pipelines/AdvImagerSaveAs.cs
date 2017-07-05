using System;
using System.Collections;
using AdvancedImager.Utility;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Pipelines;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;

namespace AdvancedImager.Pipelines
{
	public class AdvImagerSaveAs : CopyItems
	{
		protected override string GetDialogUrl()
		{
			return "/sitecore/shell/Applications/Dialogs/Advanced Imager Save as";
		}

		public new void GetDestination(CopyItemsArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			Item obj = GetDatabase(args).Items[new ListString(args.Parameters["items"], '|')[0]];
			var urlString = new UrlString(GetDialogUrl());
			if (obj != null)
			{
				urlString.Append("fo", obj.ID.ToString());
				urlString.Append("sc_content", obj.Database.Name);
				urlString.Append("la", args.Parameters["language"]);
			}
			Context.ClientPage.ClientResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", string.Empty, true);
			args.WaitForPostBack(false);
		}

		public override void Execute(CopyItemsArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			Item newParent = GetDatabase(args).GetItem(args.Parameters["destination"], Language.Parse(args.Parameters["language"]));
			MediaItem originalParent = GetDatabase(args).GetItem(args.Parameters["items"]);
			Assert.IsNotNull(newParent, args.Parameters["destination"]);
			Assert.IsNotNull(originalParent, args.Parameters["originalParent"]);

			string cropRatio = args.Parameters[Constants.CropRatio];
			string imageData = args.Parameters[Constants.Base64Data];

			if (!args.IsPostBack && Saver.IsOverwrite(originalParent, newParent, cropRatio))
			{
				SheerResponse.Confirm("The selected crop already exists. Overwrite?");
				args.WaitForPostBack();
				return;
			}

			try
			{
				if (args.IsPostBack && args.Result != "yes") return;

				Saver.SaveAs(originalParent, newParent, imageData, cropRatio);
				SheerResponse.Alert("The image has been saved.");
				Context.ClientPage.Modified = false;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex, this);
				SheerResponse.Alert("The image could not be saved.");
			}
		}
	}
}
