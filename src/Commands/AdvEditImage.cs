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
			Item obj = context.Items[0];
			var urlString = new UrlString();
			urlString.Append("sc_content", obj.Database.Name);
			urlString.Append("id", obj.ID.ToString());
			urlString.Append("la", obj.Language.ToString());
			urlString.Append("vs", obj.Version.ToString());
			if (!string.IsNullOrEmpty(context.Parameters["frameName"]))
				urlString.Add("pfn", context.Parameters["frameName"]);
			Sitecore.Shell.Framework.Windows.RunApplication("Media/AdvancedImager", urlString.ToString());
		}
	}
}