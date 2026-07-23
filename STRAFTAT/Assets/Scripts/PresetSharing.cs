using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;

public class PresetSharing : MonoBehaviour
{
	public static string EncodePreset(string json)
	{
		byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
		byte[] compressed = Compress(jsonBytes);
		return Convert.ToBase64String(compressed)
			.Replace('+', '-').Replace('/', '_').TrimEnd('=');
	}

	public static string DecodePreset(string encoded)
	{
		string padded = encoded.Replace('-', '+').Replace('_', '/');
		while (padded.Length % 4 != 0) padded += "=";

		byte[] compressed = Convert.FromBase64String(padded);
		byte[] decompressed = Decompress(compressed);
		string presetJson = JsonUtility.ToJson(System.Text.Encoding.UTF8.GetString(decompressed)); 

		return presetJson;
	}

	private static byte[] Compress(byte[] data)
	{
		using var output = new MemoryStream();
		using (var gzip = new GZipStream(output, CompressionMode.Compress))
			gzip.Write(data, 0, data.Length);
		return output.ToArray();
	}

	private static byte[] Decompress(byte[] data)
	{
		using var input = new MemoryStream(data);
		using var gzip = new GZipStream(input, CompressionMode.Decompress);
		using var output = new MemoryStream();
		gzip.CopyTo(output);
		return output.ToArray();
	}
}
