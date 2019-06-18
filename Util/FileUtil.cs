namespace Util
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public class FileUtil
    {
        public static string getRelativePath(string path1, string path2)
        {
            string str = "";
            char[] separator = new char[] { '/' };
            string[] strArray = path1.Split(separator);
            char[] chArray2 = new char[] { '/' };
            string[] strArray2 = path2.Split(chArray2);
            int num = 0;
            for (int i = 0; i < (strArray.Length - 1); i++)
            {
                if (strArray[i] != strArray2[i])
                {
                    break;
                }
                num++;
            }
            for (int j = 0; j < ((strArray.Length - num) - 1); j++)
            {
                str = str + "../";
            }
            for (int k = num; k < strArray2.Length; k++)
            {
                str = str + strArray2[k];
                if (k < (strArray2.Length - 1))
                {
                    str = str + "/";
                }
            }
            return str;
        }

        public static FileStream saveFile(string fileName, JSONObject node = null)
        {
            string directoryName = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            if (node != null)
            {
                string str2 = node.Print(true);
                StreamWriter writer1 = new StreamWriter(stream);
                writer1.Write(str2);
                writer1.Close();
            }
            return stream;
        }

        public static void WriteData(FileStream fs, params bool[] datas)
        {
            bool[] flagArray = datas;
            for (int i = 0; i < flagArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(flagArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params byte[] datas)
        {
            foreach (byte num2 in datas)
            {
                fs.WriteByte(num2);
            }
        }

        public static void WriteData(FileStream fs, params double[] datas)
        {
            double[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params short[] datas)
        {
            short[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params int[] datas)
        {
            int[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params long[] datas)
        {
            long[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params sbyte[] datas)
        {
            BinaryWriter writer = new BinaryWriter(fs);
            foreach (sbyte num2 in datas)
            {
                writer.Write(num2);
            }
        }

        public static void WriteData(FileStream fs, params float[] datas)
        {
            float[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params ushort[] datas)
        {
            ushort[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, params uint[] datas)
        {
            uint[] numArray = datas;
            for (int i = 0; i < numArray.Length; i++)
            {
                byte[] bytes = BitConverter.GetBytes(numArray[i]);
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public static void WriteData(FileStream fs, string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            short length = (short) bytes.Length;
            short[] datas = new short[] { length };
            WriteData(fs, datas);
            fs.Write(bytes, 0, bytes.Length);
        }
    }
}

