using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Framework.Pipelines;
using Sitecore.Web.UI.Sheer;

namespace AdvancedImager.Commands
{
	[Serializable]
	public class SaveAs : Command
	{
		public override void Execute(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			Item[] items = context.Items;
			Assert.IsNotNull(items, "items");

			if (items.Length <= 0)
				return;
			var args = new CopyItemsArgs {Parameters = context.Parameters};

			Start("uiAdvImagerSaveAs", args, items[0].Database, items);
		}

		private static void Start(string pipelineName, ClientPipelineArgs args, Database database, IReadOnlyList<Item> items)
		{
			Assert.ArgumentNotNull(pipelineName, "pipelineName");
			Assert.ArgumentNotNull(args, "args");
			Assert.ArgumentNotNull(database, "database");
			Assert.ArgumentNotNull(items, "items");
			Assert.ArgumentNotNullOrEmpty(pipelineName, "pipelineName");
			string itemIds = string.Join("|", items.Select(item => item.ID));
			string langauge = items[0].Language.ToString();
			NameValueCollection parameters = args.Parameters;
			parameters["database"] = database.Name;
			parameters["items"] = itemIds;
			parameters["language"] = langauge;
			args.Parameters = parameters;

			Context.ClientPage.Start(pipelineName, args);
		}
	}
}
