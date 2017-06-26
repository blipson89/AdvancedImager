using System;
using System.Drawing;
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
	public class AdvSave : Command
	{
		public override void Execute(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			if (context.Items.Length != 1) return;
			MediaItem parentItem = context.Items[0];
			try
			{
				Save(parentItem, context.Parameters[Constants.Base64Data]);
				SheerResponse.Alert("The image has been saved.");
				Context.ClientPage.Modified = false;

			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, ex, this);
				SheerResponse.Alert("The image could not be saved.");

			}
		}

		protected virtual void Save(MediaItem parent, string imageData)
		{
			byte[] binData = Convert.FromBase64String(imageData);
			using (var stream = new MemoryStream(binData))
			using (Image image = Image.FromStream(stream))
			{
				var media = MediaManager.GetMedia(parent) as ImageMedia;
				media?.SetImage(image);
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
