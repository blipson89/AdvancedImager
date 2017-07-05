using System;
using System.Drawing;
using System.IO;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace AdvancedImager.Utility
{
	public class Saver
	{
		public static void SaveOverwrite(MediaItem parent, string imageData)
		{
			byte[] binData = Convert.FromBase64String(imageData);
			using (var stream = new MemoryStream(binData))
			using (Image image = Image.FromStream(stream))
			{
				var media = MediaManager.GetMedia(parent) as ImageMedia;
				media?.SetImage(image);
			}
		}

		public static void SaveAs(MediaItem originalParent, MediaItem newParent, string imageData, string cropRatio)
		{
			string destination = GetDestination(originalParent, newParent, cropRatio);
			string filename = GetFilename(originalParent, cropRatio);
			byte[] binData = Convert.FromBase64String(imageData);
			using (var stream = new MemoryStream(binData))
			{

				MediaCreatorOptions options = MediaCreatorOptions.Empty;
				options.AlternateText = originalParent.Alt;
				options.Database = originalParent.Database;
				options.Destination = destination;
				options.IncludeExtensionInItemName = false;
				options.OverwriteExisting = true;

				MediaManager.Creator.CreateFromStream(stream, $"{filename}.{originalParent.Extension}", options);
			}
		}

		public static void Save(MediaItem originalParent, string imageData, string cropRatio)
		{
			SaveAs(originalParent, originalParent, imageData, cropRatio);
		}

		public static bool IsOverwrite(Item originalParent, Item newParent, string cropRatio)
		{
			return originalParent.Database.GetItem(GetDestination(originalParent, newParent, cropRatio)) != null;
		}

		public static bool IsOverwrite(Item originalParent, string cropRatio)
		{
			return originalParent.Database.GetItem(GetDestination(originalParent, originalParent, cropRatio)) != null;
		}

		private static string GetDestination(Item originalParent, Item newParent, string cropRatio)
		{
			return $"{newParent.Paths.FullPath}/{GetFilename(originalParent, cropRatio)}";
		}

		private static string GetFilename(Item parent, string cropRatio)
		{
			return $"{parent.Name}_{cropRatio.Replace(":", "_")}";
		}
	}
}
