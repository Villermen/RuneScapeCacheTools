﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Villermen.RuneScapeCacheTools.Cache
{
    /// <summary>
    /// Base class for 
    /// </summary>
	public abstract class Cache
	{
		protected Cache()
		{
			CacheDirectory = DefaultCacheDirectory;
			TemporaryDirectory = Path.GetTempPath() + "rsct/";
		}

		protected Cache(IDataProcessor dataProcessor) : this()
		{
			DataProcessor = dataProcessor;
		}

		/// <summary>
		///   The directory that is the default location for this type of cache.
		/// </summary>
		public abstract string DefaultCacheDirectory { get; }

		/// <summary>
		///   The location where the cache is located.
		/// </summary>
		public string CacheDirectory { get; set; }

		/// <summary>
		///   The location where the processed cache will be stored.
		/// </summary>
		public string OutputDirectory { get; set; }

		/// <summary>
		///   Temporary files used while processing will be stored here.
		/// </summary>
		public string TemporaryDirectory { get; set; }

		/// <summary>
		///   Processor used on obtained data.
		/// </summary>
		public IDataProcessor DataProcessor { get; set; } = new ExtendableDataProcessor();

		public abstract IEnumerable<int> GetArchiveIds();

		public abstract IEnumerable<int> GetFileIds(int indexId);

		/// <summary>
		///   Extracts every file in every archive.
		/// </summary>
		/// <returns></returns>
		public async Task ExtractAllAsync()
		{
			var archiveIds = GetArchiveIds();

			await Task.Run(() => { Parallel.ForEach(archiveIds, archiveId => { ExtractArchiveAsync(archiveId).Wait(); }); });
		}

		/// <summary>
		///   Extracts every file in the given archive.
		/// </summary>
		/// <param name="archiveId"></param>
		/// <returns></returns>
		public async Task ExtractArchiveAsync(int archiveId)
		{
			var fileIds = GetFileIds(archiveId);
			await Task.Run(() => { Parallel.ForEach(fileIds, fileId => { ExtractFile(archiveId, fileId); }); });
		}

		/// <summary>
		///   Extracts and processes the given file in the given index.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <returns></returns>
		public void ExtractFile(int indexId, int fileId)
		{
			var fileData = GetFileData(indexId, fileId);

			if (fileData == null)
			{
				return;
			}

			DataProcessor.Process(ref fileData);
			var extension = DataProcessor.GuessExtension(ref fileData);

			WriteFile(indexId, fileId, fileData, extension);
		}

		/// <summary>
		/// </summary>
		/// <param name="indexId"></param>
		/// <returns>The path to the directory of the given archive, or null if it does not exist.</returns>
		public virtual string GetIndexOutputPath(int indexId)
		{
			string archivePath = $"{OutputDirectory}cache/{indexId}/";

			if (Directory.Exists(archivePath))
			{
				return archivePath;
			}

			return null;
		}

		/// <summary>
		///   Finds the path for the given extracted file.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <param name="extractIfMissing">Try to extract the file if it hasn't been extracted yet.</param>
		/// <returns>Returns the path to the obtained file, or null if it does not exist.</returns>
		public virtual string GetFileOutputPath(int indexId, int fileId, bool extractIfMissing = false)
		{
			try
			{
				var path = Directory
					.EnumerateFiles($"{OutputDirectory}cache/{indexId}/", $"{fileId}*")
					.FirstOrDefault(file => Regex.IsMatch(file, $@"(/|\\){fileId}(\..+)?$"));

				if (!string.IsNullOrWhiteSpace(path))
				{
					return path;
				}

				if (extractIfMissing)
				{
					ExtractFile(indexId, fileId);
					return GetFileOutputPath(indexId, fileId);
				}

				return null;
			}
			catch (DirectoryNotFoundException)
			{
				return null;
			}
		}

		/// <summary>
		///   Write out the given data to the specified file.
		///   Deletes a previous version of the file (regardless of extension) if it exists.
		/// </summary>
		/// <param name="indexId"></param>
		/// <param name="fileId"></param>
		/// <param name="data"></param>
		/// <param name="extension">File extension, without the dot.</param>
		protected void WriteFile(int indexId, int fileId, byte[] data, string extension = null)
		{
			// Throw an exception if the output directory is not yet set or does not exist
			if (string.IsNullOrWhiteSpace(OutputDirectory) || !Directory.Exists(OutputDirectory))
			{
				throw new DirectoryNotFoundException("Output directory does not exist.");
			}

			// Delete existing file
			var existingFilePath = GetFileOutputPath(indexId, fileId);
			if (!string.IsNullOrWhiteSpace(existingFilePath))
			{
				File.Delete(existingFilePath);
			}

			// Construct new path
			string newFilePath = $"{OutputDirectory}cache/{indexId}/{fileId}";
			if (!string.IsNullOrWhiteSpace(extension))
			{
				newFilePath += $".{extension}";
			}

			// Create directories where necessary, before writing to file
			Directory.CreateDirectory(Path.GetDirectoryName(newFilePath));
			File.WriteAllBytes(newFilePath, data);
		}

        /// <summary>
        ///   Returns the raw data for the given file.
        /// </summary>
        /// <param name="indexId"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public abstract byte[] GetFileData(int indexId, int fileId);

        public byte[][] GetArchiveFiles(int indexId, int archiveId)
        {
            
        }

        public byte[] GetArchiveFile(int indexId, int archiveId, int fileId);

        // TODO: caching
	    public abstract ReferenceTable GetReferenceTable(int indexId);
	}
}