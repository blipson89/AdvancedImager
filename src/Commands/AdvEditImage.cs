using System;
using Sitecore.Data.Items;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Framework.Commands.WebDAV;
using Sitecore.Text;

namespace AdvancedImager.Commands
{
	[Serializable]
	public class AdvEditImage : EditImage
	{
		public override void Execute(CommandContext context)
		{
			if (context.Items.Length != 1)
				return;
			Item contextItem = context.Items[0];
			var urlString = new UrlString();
			urlString.Append("sc_content", contextItem.Database.Name);
			urlString.Append("id", contextItem.ID.ToString());
			urlString.Append("la", contextItem.Language.ToString());
			urlString.Append("vs", contextItem.Version.ToString());
			Sitecore.Shell.Framework.Windows.RunApplication("Media/AdvancedImager", urlString.ToString());
		}
	}
}