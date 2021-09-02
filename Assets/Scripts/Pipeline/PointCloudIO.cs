using System.IO;
using UnityEngine;
using PCToolkit.Data;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace PCToolkit.Pipeline
{
    public class PointCloudIO
    {
        const string datasetDir = "../../Datasets";
        const string pointCloudDir = "PointClouds";

        public static byte[] SerializeToBytes<T>(T item)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, item);
                stream.Seek(0, SeekOrigin.Begin);
                return stream.ToArray();
            }
        }
        public static T DeserializeFromBytes<T>(byte[] bytes)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream(bytes))
            {
                return (T)formatter.Deserialize(stream);
            }
        }

        public static void SavePointCloud(List<Point> data, string fileName)
        {
            if (data.Count <= 0)
            {
                Debug.LogError(string.Format("Error saving empty point clous {0}", fileName));
                return;
            }

            var dir = string.Format("{0}/{1}/{2}/{3}", Application.dataPath, datasetDir, pointCloudDir, fileName);
            try
            {
                Directory.CreateDirectory(dir);
                using (BinaryWriter writer = new BinaryWriter(File.Open(dir + "/" + fileName + ".points", FileMode.Create)))
                {
                    writer.Write(data.Count);
                    int size = SerializeToBytes(data[0]).Length;
                    writer.Write(size);
                    for (int i = 0; i < data.Count; i++)
                    {
                        writer.Write(SerializeToBytes(data[i]));
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Error saving point clous {0}: {1}\n{2}", fileName, e.Message, e.StackTrace));
            }
        }

        public static List<Point> LoadPointCloud(string fileName)
        {
            var filePath = string.Format("{0}/{1}/{2}/{3}/{3}.points", Application.dataPath, datasetDir, pointCloudDir, fileName);
            try
            {
                var points = new List<Point>();
                using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                {
                    var count = reader.ReadInt32();
                    var size = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        var bytes = reader.ReadBytes(size);
                        points.Add(DeserializeFromBytes<Point>(bytes));
                    }
                }

                return points;
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("Error loading {0}: {1}\n{2}", filePath, e.Message, e.StackTrace));
                return null;
            }
        }
    }
}

