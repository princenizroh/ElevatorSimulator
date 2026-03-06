using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace KProject.SaveSystem
{
    public static class SaveSerializer
    {
        public static byte[] Serialize(SaveData data)
        {
            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
            formatter.Serialize(stream, data);
#pragma warning restore SYSLIB0011
            return stream.ToArray();
        }

        public static SaveData Deserialize(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            var formatter = new BinaryFormatter();
#pragma warning disable SYSLIB0011
            return (SaveData)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
        }

        public static SaveResult WriteToDisk(string path, byte[] bytes)
        {
            try
            {
                long availableSpace = GetAvailableDiskSpace(path);
                if (availableSpace != -1 && bytes.Length > availableSpace)
                    return SaveResult.NotEnoughSpace;

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllBytes(path, bytes);
                return SaveResult.Success;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSerializer] Write failed: {e.Message}");
                return SaveResult.Failed;
            }
        }

        public static (SaveResult, SaveData) ReadFromDisk(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return (SaveResult.SlotEmpty, null);

                byte[] bytes = File.ReadAllBytes(path);
                SaveData data = Deserialize(bytes);
                return (SaveResult.Success, data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSerializer] Read failed: {e.Message}");
                return (SaveResult.FileCorrupted, null);
            }
        }

        private static long GetAvailableDiskSpace(string path)
        {
            try
            {
                string root = Path.GetPathRoot(path);
                var drive = new DriveInfo(root);
                return drive.AvailableFreeSpace;
            }
            catch
            {
                return -1;
            }
        }
    }
}
