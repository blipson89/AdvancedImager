using System;
using System.Collections.Specialized;
using System.IO;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Resources.Media;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web.UI.Sheer;
using Convert = System.Convert;

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

			if (!args.IsPostBack && IsOverwrite(parentItem, cropRatio))
			{
				SheerResponse.Confirm("The selected crop already exists. Overwrite?");
				args.WaitForPostBack();
				return;
			}

			try
			{
				if (args.IsPostBack && args.Result != "yes") return;

				Save(parentItem, imageData, cropRatio);
				SheerResponse.Alert("The image has been saved.");
				Context.ClientPage.Modified = false;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex, this);
				SheerResponse.Alert("The image could not be saved.");

			}
		}

		private static bool IsOverwrite(Item parent, string cropRatio)
		{
			return parent.Database.GetItem(GetDestination(parent, cropRatio)) != null;
		}

		private static string GetDestination(Item parent, string cropRatio)
		{
			return $"{parent.Paths.FullPath}/{GetFilename(parent, cropRatio)}";
		}

		private static string GetFilename(Item parent, string cropRatio)
		{
			return $"{parent.Name}_{cropRatio.Replace(":","_")}";
		}

		protected virtual void Save(MediaItem parent, string imageData, string cropRatio)
		{
			byte[] binData = Convert.FromBase64String(imageData);
			using (var stream = new MemoryStream(binData))
			{
				
				MediaCreatorOptions options = MediaCreatorOptions.Empty;
				options.AlternateText = parent.Alt;
				options.Database = parent.Database;
				options.Destination = GetDestination(parent, cropRatio);
				options.IncludeExtensionInItemName = false;
				options.OverwriteExisting = true;

				MediaManager.Creator.CreateFromStream(stream, $"{GetFilename(parent, cropRatio)}.{parent.Extension}", options);
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
