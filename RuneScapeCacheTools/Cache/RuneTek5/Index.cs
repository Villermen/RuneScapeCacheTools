﻿using System;
using System.IO;

namespace Villermen.RuneScapeCacheTools.Cache.RuneTek5
{
	/// <summary>
	///   An <see cref="Index" /> points to a file inside a <see cref="FileStore" />.
	/// </summary>
	/// <author>Graham</author>
	/// <author>`Discardedx2</author>
	/// <author>Villermen</author>
	public class Index
	{
		/// <summary>
		///   Length of index data in bytes.
		/// </summary>
		public const int Length = 6;

		public Index(byte[] data)
		{
			var reader = new BinaryReader(new MemoryStream(data));
			Size = (int) reader.ReadUInt24BigEndian();
			Sector = (int) reader.ReadUInt24BigEndian();
		}

		/// <summary>
		///   The size of the file in bytes.
		/// </summary>
		public int Size { get; private set; }

		/// <summary>
		///   The number of the first sector that contains the file.
		/// </summary>
		public int Sector { get; private set; }

		public byte[] Encode()
		{
			throw new NotImplementedException();
		}
	}
}