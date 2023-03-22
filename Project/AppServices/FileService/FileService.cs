using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.AppServices
{
    public enum ExtensionType { json, txt }

    class FileService
    {
        Dictionary<ExtensionType, string> extensions = new Dictionary<ExtensionType, string>()
        {
            {ExtensionType.json, ".json" },
            {ExtensionType.txt, ".txt" }
        };

        string path = AppDomain.CurrentDomain.BaseDirectory;
        string dataFolderPath;

        public FileService()
        {
            dataFolderPath = path + "Data";
        }

        public async Task SaveToFileAsync<T>(T obj, string fullName)
        {
            Type t = obj.GetType();
            switch (obj.GetType().Name)
            {
                case "string":
                case "String":
                    SaveStringToFile(obj as string, fullName);
                    break;
            }
        }

        public async Task SaveToFileAsync<T>(T obj, string fileName, ExtensionType extension)
        {
            Type t = obj.GetType();
            switch (obj.GetType().Name)
            {
                case "string":
                case "String":
                    SaveStringToFile(obj as string, fileName, extension);
                    break;
            }
        }

        public void SaveStringToFile(string data, string fullName)
        {
            if (!Directory.Exists(dataFolderPath))
                Directory.CreateDirectory(dataFolderPath);

            string combinedPath = dataFolderPath + "\\" + fullName;

            using (FileStream fs = File.Create(combinedPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(data);
                fs.Write(info, 0, info.Length);
            }
        }

        public void SaveStringToFile(string data, string fileName, ExtensionType extension)
        {
            if (!Directory.Exists(dataFolderPath))
                Directory.CreateDirectory(dataFolderPath);

            string combinedPath = dataFolderPath + "\\" + fileName + extensions[extension];

            using (FileStream fs = File.Create(combinedPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(data);
                fs.Write(info, 0, info.Length);
            }
        }

        public void LoadFromFile<T>(out T obj, string name, ExtensionType extension)
        {
            if (!Directory.Exists(dataFolderPath))
                throw new FileNotFoundException("Can't find data directory");

            obj = default(T);

            switch (extension)
            {
                case ExtensionType.json:
                    string json = LoadStringFromFile(name + extensions[extension]);
                    break;
                case ExtensionType.txt:

                    break;
            }
        }

        public string LoadStringFromFile(string fullName)
        {
            string combinedPath = dataFolderPath + "\\" + fullName;

            using (FileStream fs = File.OpenRead(combinedPath))
            {
                byte[] bytes = new byte[fs.Length];
                int numBytesToRead = (int)fs.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    int n = fs.Read(bytes, numBytesRead, numBytesToRead);

                    if (n == 0)
                        break;
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }

        public bool FileExist(string fileName)
        {
            string combinedPath = dataFolderPath + "\\" + fileName;
            return File.Exists(combinedPath);
        }

    }
}
