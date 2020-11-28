using UnityEngine;
using System.IO;
using Ionic.Zlib;
using System.Collections.Generic;
using Debug = EAFrameWork.Debug;

/// <summary>
/// Responsible for storing and loading files.
/// </summary>
public class FileManager
{

	public static void DeletePersistentDataPath()
	{
		DirectoryInfo dirInfo = new DirectoryInfo(Application.persistentDataPath);
		FileInfo[] infos = dirInfo.GetFiles();
		foreach (FileInfo info in infos)
		{
			info.Delete();
		}
	}

    // <summary>
    // Delete file. Limited use in current offline mode.
    // </summary>
    public static void DeleteFile(bool isOffline, string accountName, string fileName)
    {
        string filePath = MakeFilePathForAccount(fileName, accountName, isOffline);
        DeleteFile(filePath);
    }

    public static void DeleteFile(string filePath)
	{
		File.Delete(filePath);
		File.Delete(filePath + ".tmp"); // Delete if present
    }

    /// <summary>
    /// Save file.Limited use in current offline mode.Write it to tmp file for integrity, then overwrite it.
    /// </summary>
    public static void SaveText(string fileName, string text, string accountName, bool isOffline)
    {
        string filePath = MakeFilePathForAccount(fileName, accountName, isOffline);
        SaveText(filePath, text);
    }

    static string MakeFilePathForAccount(string fileName, string accountName, bool isOffline)
    {
        Debug.Assert(accountName != null);
        string offMode = isOffline ? "_offline" : string.Empty;

        return string.Format("{0}/{1}{2}_{3}", Application.persistentDataPath, accountName, offMode, fileName);
    }

    /// <summary>
    /// Save text file. Write it to tmp file for integrity, then overwrite it.
    /// </summary>
    public static void SaveText(string filePath, string text)
	{
		string dirName = Path.GetDirectoryName(filePath);
		Directory.CreateDirectory(dirName);

		string tempPath = filePath + ".tmp";

		using (StreamWriter writer = File.CreateText(tempPath))
		{
			writer.Write(text);
			writer.Close();

			File.Copy(tempPath, filePath, true);
			File.Delete(filePath + ".tmp");
		}
	}

	public static void SaveBytes(string filePath, byte[] bytes)
	{
		string dirName = Path.GetDirectoryName(filePath);
		Directory.CreateDirectory(dirName);

		string tempPath = filePath + ".tmp";

		using (FileStream fs = File.Create(tempPath))
		{
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();

			File.Copy(tempPath, filePath, true);
			File.Delete(filePath + ".tmp");
		}
	}

    /// <summary>
    /// File load. Limited use in offline mode.
    /// </summary>
    public static string LoadText(string filePath)
	{
		if (!File.Exists(filePath))
			return null;

		using (FileStream stream = File.Open(filePath, FileMode.Open))
		{
			using (StreamReader reader = new StreamReader(stream))
			{
				string s = reader.ReadToEnd();
				reader.Close();
				stream.Close();
				Debug.Log("FileManager - " + filePath + " - read string:" + s);
				return s;
			}
		}
	}

	public static byte[] LoadBytes(string filePath)
	{
		if (!File.Exists(filePath))
			return null;

		byte[] buffer;

		using (FileStream stream = File.Open(filePath, FileMode.Open))
		{
			int length = (int)stream.Length;
			buffer = new byte[length];
			int currRead;
			int totalRead = 0;
			while ((currRead = stream.Read(buffer, totalRead, length - totalRead)) > 0)
				totalRead += currRead;

			stream.Close();
		}
		return buffer;
	}


    public static string[] LoadTextLineArray(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        string[] s = File.ReadAllLines(filePath);

        Debug.Log("FileManager - " + filePath + " - read string:" + s);
        return s;
    }

    public static List<string> LoadTextLineList(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        List<string> strings = new List<string>();

        string[] s = File.ReadAllLines(filePath);

        for(int i = 0; i < s.Length; ++i)
        {
            strings.Add(s[i]);
        }

        Debug.Log("FileManager - " + filePath + " - read string:" + s);
        return strings;
    }


    static byte[] _UnZip(MemoryStream output)
	{
		var cms = new MemoryStream();
		output.Seek(0, SeekOrigin.Begin);
		using (var gz = new GZipStream(output, CompressionMode.Decompress))
		{
			var buf = new byte[1024];
			int byteCount = 0;
			while ((byteCount = gz.Read(buf, 0, buf.Length)) > 0)
			{
				cms.Write(buf, 0, byteCount);
			}
		}
		return cms.ToArray();
	}
}
