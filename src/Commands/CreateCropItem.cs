﻿using System;
using System.Collections.Specialized;
using AdvancedImager.Utility;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;

namespace AdvancedImager.Commands
{
	[Serializable]
	public class CreateCropItem : Command
	{
		public override void Execute(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			if (context.Items.Length != 1) return;
			var parameters = new NameValueCollection
			{
				["Items"] = SerializeItems(context.Items),
				[Constants.CropRatio] = context.Parameters[Constants.CropRatio],
				[Constants.Base64Data] = context.Parameters[Constants.Base64Data]
			};
			Context.ClientPage.Start(this, "Run", parameters);
			
		}

		protected void Run(ClientPipelineArgs args)
		{
			Item[] items = DeserializeItems(args.Parameters["Items"]);
			MediaItem parentItem = items[0];
			string cropRatio = args.Parameters[Constants.CropRatio];
			string imageData = args.Parameters[Constants.Base64Data];

			if (!args.IsPostBack && Saver.IsOverwrite(parentItem, cropRatio))
			{
				SheerResponse.Confirm("The selected crop already exists. Overwrite?");
				args.WaitForPostBack();
				return;
			}

			try
			{
				if (args.IsPostBack && args.Result != "yes") return;

				Saver.Save(parentItem, imageData, cropRatio);
				SheerResponse.Alert("The image has been saved.");
				Context.ClientPage.Modified = false;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex, this);
				SheerResponse.Alert("The image could not be saved.");
			}
		}

		

		public override CommandState QueryState(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			Item contextItem = context.Items[0];
			if (contextItem == null)
				return CommandState.Disabled;
			if (contextItem.TemplateID != TemplateIDs.VersionedImage && contextItem.TemplateID != TemplateIDs.UnversionedImage)
			{
				Template template = TemplateManager.GetTemplate(contextItem.TemplateID, contextItem.Database);
				Assert.IsNotNull(template, typeof(Template));
				if (!template.DescendsFrom(TemplateIDs.UnversionedImage) && !template.DescendsFrom(TemplateIDs.VersionedImage))
					return CommandState.Disabled;
			}
			if (contextItem.Appearance.ReadOnly || !contextItem.Access.CanRead() || !contextItem.Access.CanWrite() || !contextItem.Access.CanWriteLanguage())
				return CommandState.Disabled;
			return base.QueryState(context);
		}
	}
}
