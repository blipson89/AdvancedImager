using System;
using Sitecore.Configuration;
using Sitecore.Shell;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Shell.Framework.Commands.WebDAV;

namespace AdvancedImager.Commands
{
	[Serializable]
	public class AdvancedEdit : Command
	{
		public override CommandState QueryState(CommandContext context)
		{
			Command editingCommand = GetEditingCommand();
			if (editingCommand == null)
				return CommandState.Hidden;
			CommandState commandState = editingCommand.QueryState(context);
			switch (commandState)
			{
				case CommandState.Disabled:
				case CommandState.Hidden:
					return CommandState.Disabled;
				default:
					return commandState;
			}
		}

		public override void Execute(CommandContext context)
		{
			Command editingCommand = GetEditingCommand();
			editingCommand?.Execute(context);
		}

		public virtual Command GetEditingCommand()
		{
			if (UserOptions.WebDAV.UseLocalEditor)
			{
				if (WebDAVConfiguration.IsWebDAVEnabled(true))
					return new EditMedia();
				return new AdvEditImage();
			}
			if (!IsAdvancedClient() && WebDAVConfiguration.IsWebDAVEnabled(true))
				return new EditMedia();
			return new AdvEditImage();
		}
	}
}
