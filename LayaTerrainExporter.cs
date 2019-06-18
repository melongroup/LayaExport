using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LayaTerrainExporter
{
    private static bool cameraCoordinateInverse = true;
    private static int CHUNK_GRID_NUM = 0x40;
    private static int LEAF_GRID_NUM = 0x20;
    private static Color[] m_alphaBlock = null;
    private static string m_alphaBlockName = "";
    private static int m_alphaIndex = 0;
    private static JSONObject m_alphamapArrayNode = null;
    private static List<KeyValuePair<string, Color[]>> m_alphamapDataList = new List<KeyValuePair<string, Color[]>>();
    private static int m_channelIndex = 0;
    private static JSONObject m_chunkInfoNode = null;
    private static JSONObject m_chunkNode = null;
    private static bool m_debug = false;
    private static JSONObject m_detailID = null;
    private static JSONObject m_detailIDArrayNode = null;
    private static string m_saveLocation;
    private static int m_splatIndex = 0;
    private static Terrain m_terrain;

    private static void AfterMergeAlphaMap(string fileName, TextureFormat format, int width, int height)
    {
        if (m_alphaBlock != null)
        {
            SaveAlphaMap(fileName, format, width, height);
        }
        m_alphaIndex = 0;
        m_alphaBlockName = "";
        m_chunkNode.AddField("alphaMap", m_alphamapArrayNode);
        m_chunkNode.AddField("detailID", m_detailIDArrayNode);
        m_chunkInfoNode.Add(m_chunkNode);
    }

    private static Vector3[] calcNormalOfTriangle(float[,] heightsData, int size, float girdSize)
    {
        Vector3[] vectorArray = new Vector3[((m_terrain.get_terrainData().get_heightmapWidth() - 1) * (m_terrain.get_terrainData().get_heightmapHeight() - 1)) * 2];
        Vector3 vector = new Vector3();
        Vector3 vector2 = new Vector3();
        Vector3 vector3 = new Vector3();
        Vector3 vector4 = new Vector3();
        float[] numArray = new float[m_terrain.get_terrainData().get_heightmapWidth() * m_terrain.get_terrainData().get_heightmapHeight()];
        float num = 65536f;
        int index = 0;
        for (int i = 0; i < m_terrain.get_terrainData().get_heightmapHeight(); i++)
        {
            for (int k = 0; k < m_terrain.get_terrainData().get_heightmapWidth(); k++)
            {
                ushort num8 = (ushort) Mathf.Clamp(Mathf.RoundToInt(heightsData[i, k] * num), 0, 0xffff);
                numArray[index] = (((num8 * 1f) / 32766f) * m_terrain.get_terrainData().get_size().y) * 0.5f;
                index++;
            }
        }
        int num3 = m_terrain.get_terrainData().get_heightmapWidth();
        int num4 = m_terrain.get_terrainData().get_heightmapHeight();
        int num5 = 0;
        Matrix4x4 matrixx = new Matrix4x4();
        matrixx.SetTRS(Vector3.get_zero(), Quaternion.AngleAxis(180f, Vector3.get_up()), Vector3.get_one());
        Matrix4x4 matrixx2 = new Matrix4x4();
        matrixx2.SetTRS(new Vector3(0f, 0f, m_terrain.get_terrainData().get_size().y), Quaternion.get_identity(), Vector3.get_one());
        Matrix4x4 matrixx3 = matrixx2 * matrixx;
        for (int j = 0; j < (num4 - 1); j++)
        {
            for (int m = 0; m < (num3 - 1); m++)
            {
                vector.x = m * girdSize;
                vector.y = numArray[((j + 1) * num3) + m];
                vector.z = (j + 1) * girdSize;
                if (cameraCoordinateInverse)
                {
                    vector = matrixx3.MultiplyPoint(vector);
                }
                vector2.x = (m + 1) * girdSize;
                vector2.y = numArray[(((j + 1) * num3) + m) + 1];
                vector2.z = (j + 1) * girdSize;
                if (cameraCoordinateInverse)
                {
                    vector2 = matrixx3.MultiplyPoint(vector2);
                }
                vector3.x = m * girdSize;
                vector3.y = numArray[(j * num3) + m];
                vector3.z = j * girdSize;
                if (cameraCoordinateInverse)
                {
                    vector3 = matrixx3.MultiplyPoint(vector3);
                }
                vector4.x = (m + 1) * girdSize;
                vector4.y = numArray[((j * num3) + m) + 1];
                vector4.z = j * girdSize;
                if (cameraCoordinateInverse)
                {
                    vector4 = matrixx3.MultiplyPoint(vector4);
                }
                Vector3 vector5 = Vector3.Cross(vector - vector3, vector4 - vector3);
                vector5.Normalize();
                vectorArray[num5] = vector5;
                num5++;
                Vector3 vector6 = Vector3.Cross(vector4 - vector2, vector - vector2);
                vector6.Normalize();
                vectorArray[num5] = vector6;
                num5++;
            }
        }
        return vectorArray;
    }

    private static Vector3 calcVertextNorml1(int x, int z, Vector3[] normalTriangle, int terrainXNum, int terrainZNum)
    {
        int num9;
        int num = z - 1;
        int num2 = z;
        int num3 = x - 1;
        int num4 = x;
        int[] numArray = new int[] { (((num * terrainXNum) + num3) * 2) + 1, ((num2 * terrainXNum) + num3) * 2, (((num2 * terrainXNum) + num3) * 2) + 1, ((num2 * terrainXNum) + num4) * 2, (((num * terrainXNum) + num4) * 2) + 1, ((num * terrainXNum) + num4) * 2 };
        if (num < 0)
        {
            numArray[5] = num9 = -1;
            numArray[0] = numArray[4] = num9;
        }
        if (num2 >= terrainZNum)
        {
            numArray[1] = -1;
            numArray[2] = -1;
            numArray[3] = -1;
        }
        if (num3 < 0)
        {
            numArray[2] = num9 = -1;
            numArray[0] = numArray[1] = num9;
        }
        if (num4 >= terrainXNum)
        {
            numArray[3] = -1;
            numArray[4] = -1;
            numArray[5] = -1;
        }
        float num5 = 0f;
        float num6 = 0f;
        float num7 = 0f;
        float num8 = 0f;
        for (int i = 0; i < numArray.Length; i++)
        {
            int index = numArray[i];
            if (numArray[i] >= 0)
            {
                num5 += normalTriangle[index].x;
                num6 += normalTriangle[index].y;
                num7 += normalTriangle[index].z;
                num8++;
            }
        }
        Vector3 vector = new Vector3(num5 / num8, num6 / num8, num7 / num8);
        vector.Normalize();
        return vector;
    }

    private static void Clean()
    {
        m_splatIndex = 0;
        m_alphaIndex = 0;
        m_channelIndex = 0;
        m_alphaBlock = null;
        m_detailID = null;
        m_alphamapDataList.Clear();
        m_terrain = null;
        m_alphamapArrayNode = null;
        m_detailIDArrayNode = null;
        m_chunkInfoNode = null;
        m_alphaBlockName = "";
    }

    public static void ExportAllTerrian(string savePath, JSONObject obj)
    {
        m_saveLocation = savePath + "/terrain";
        Terrain[] terrainArray = Terrain.get_activeTerrains();
        for (int i = 0; i < terrainArray.Length; i++)
        {
            Clean();
            m_terrain = terrainArray[i];
            obj.AddField("dataPath", "terrain/" + m_terrain.get_name().ToLower() + ".lt");
            ExportTerrain();
        }
    }

    private static void ExportAlphamap()
    {
        int length = m_terrain.get_terrainData().get_alphamapTextures().Length;
        for (int i = 0; i < length; i++)
        {
            Texture2D textured = m_terrain.get_terrainData().get_alphamapTextures()[i];
            FileStream output = File.Open(m_saveLocation + "/" + textured.get_name().ToLower() + ".png", System.IO.FileMode.Create);
            new BinaryWriter(output).Write(ImageConversion.EncodeToPNG(textured));
            output.Close();
        }
    }

    private static void ExportHeightmap16(float[,] heightsData)
    {
        byte[] buffer = new byte[(m_terrain.get_terrainData().get_heightmapWidth() * m_terrain.get_terrainData().get_heightmapHeight()) * 2];
        float num = 65536f;
        int num2 = 0;
        for (int i = 0; i < m_terrain.get_terrainData().get_heightmapHeight(); i++)
        {
            for (int j = 0; j < m_terrain.get_terrainData().get_heightmapWidth(); j++)
            {
                byte[] bytes = BitConverter.GetBytes((ushort) Mathf.Clamp(Mathf.RoundToInt(heightsData[i, j] * num), 0, 0xffff));
                buffer[num2 * 2] = bytes[0];
                buffer[(num2 * 2) + 1] = bytes[1];
                num2++;
            }
        }
        FileStream stream1 = new FileStream(m_saveLocation + "/" + m_terrain.get_name().ToLower() + "_heightmap.thdata", System.IO.FileMode.Create);
        stream1.Write(buffer, 0, buffer.Length);
        stream1.Close();
    }

    private static Texture2D ExportNormal(Vector3[] normalTriangle)
    {
        Color[] colorArray = new Color[m_terrain.get_terrainData().get_heightmapWidth() * m_terrain.get_terrainData().get_heightmapHeight()];
        int index = 0;
        for (int i = m_terrain.get_terrainData().get_heightmapHeight() - 1; i >= 0; i--)
        {
            for (int j = 0; j < m_terrain.get_terrainData().get_heightmapWidth(); j++)
            {
                Vector3 vector = calcVertextNorml1(j, i, normalTriangle, m_terrain.get_terrainData().get_heightmapWidth() - 1, m_terrain.get_terrainData().get_heightmapHeight() - 1);
                vector.x = (vector.x + 1f) * 0.5f;
                vector.y = (vector.y + 1f) * 0.5f;
                vector.z = (vector.z + 1f) * 0.5f;
                colorArray[index] = new Color(vector.x, vector.y, vector.z, 1f);
                index++;
            }
        }
        Texture2D textured = new Texture2D(m_terrain.get_terrainData().get_heightmapWidth(), m_terrain.get_terrainData().get_heightmapHeight(), 4, false);
        textured.SetPixels(colorArray);
        textured.Apply();
        textured.set_name(m_terrain.get_name().ToLower() + "_normalMap");
        File.WriteAllBytes(m_saveLocation + "/" + textured.get_name() + ".png", ImageConversion.EncodeToPNG(textured));
        return textured;
    }

    private static void ExportSplat()
    {
        int length = m_terrain.get_terrainData().get_splatPrototypes().Length;
        for (int i = 0; i < length; i++)
        {
            SplatPrototype prototype = m_terrain.get_terrainData().get_splatPrototypes()[i];
            Texture2D textured = prototype.get_texture();
            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(textured.GetInstanceID());
            UnityEditor.TextureImporter atPath = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
            atPath.isReadable = true;
            atPath.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
            UnityEditor.AssetDatabase.ImportAsset(assetPath);
            FileStream output = File.Open(m_saveLocation + "/" + textured.get_name().ToLower() + ".jpg", System.IO.FileMode.Create);
            new BinaryWriter(output).Write(ImageConversion.EncodeToJPG(textured));
            output.Close();
            if (prototype.get_normalMap() != null)
            {
                textured = prototype.get_normalMap();
                string path = UnityEditor.AssetDatabase.GetAssetPath(textured.GetInstanceID());
                UnityEditor.TextureImporter importer2 = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
                importer2.isReadable = true;
                importer2.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
                UnityEditor.AssetDatabase.ImportAsset(path);
                FileStream stream2 = File.Open(m_saveLocation + "/" + textured.get_name().ToLower() + ".jpg", System.IO.FileMode.Create);
                new BinaryWriter(stream2).Write(ImageConversion.EncodeToJPG(textured));
                stream2.Close();
            }
        }
    }

    private static void ExportTerrain()
    {
        if (!Directory.Exists(m_saveLocation))
        {
            Directory.CreateDirectory(m_saveLocation);
        }
        JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
        node.AddField("version", "1.0");
        node.AddField("cameraCoordinateInverse", cameraCoordinateInverse);
        float val = m_terrain.get_terrainData().get_size().x / ((float) (m_terrain.get_terrainData().get_heightmapWidth() - 1));
        node.AddField("gridSize", val);
        if (((m_terrain.get_terrainData().get_heightmapWidth() - 1) % CHUNK_GRID_NUM) != 0)
        {
            Debug.LogError("高度图的宽减去一必须是" + CHUNK_GRID_NUM + "的倍数");
        }
        else if (((m_terrain.get_terrainData().get_heightmapHeight() - 1) % CHUNK_GRID_NUM) != 0)
        {
            Debug.LogError("高度图的高减去一必须是" + CHUNK_GRID_NUM + "的倍数");
        }
        else
        {
            int num2 = (m_terrain.get_terrainData().get_heightmapWidth() - 1) / CHUNK_GRID_NUM;
            int num3 = (m_terrain.get_terrainData().get_heightmapHeight() - 1) / CHUNK_GRID_NUM;
            node.AddField("chunkNumX", num2);
            node.AddField("chunkNumZ", num3);
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            obj3.AddField("numX", m_terrain.get_terrainData().get_heightmapWidth());
            obj3.AddField("numZ", m_terrain.get_terrainData().get_heightmapHeight());
            obj3.AddField("bitType", 0x10);
            obj3.AddField("value", m_terrain.get_terrainData().get_size().y);
            obj3.AddField("url", m_terrain.get_name().ToLower() + "_heightmap.thdata");
            node.AddField("heightData", obj3);
            JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
            node.AddField("material", obj4);
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            obj5.Add((float) 0f);
            obj5.Add((float) 0f);
            obj5.Add((float) 0f);
            obj4.AddField("ambient", obj5);
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            obj6.Add((float) 1f);
            obj6.Add((float) 1f);
            obj6.Add((float) 1f);
            obj4.AddField("diffuse", obj6);
            JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
            obj7.Add((float) 0.2f);
            obj7.Add((float) 0.2f);
            obj7.Add((float) 0.2f);
            obj7.Add((float) 32f);
            obj4.AddField("specular", obj7);
            JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
            node.AddField("detailTexture", obj8);
            int length = m_terrain.get_terrainData().get_splatPrototypes().Length;
            for (int i = 0; i < length; i++)
            {
                JSONObject obj11 = new JSONObject(JSONObject.Type.OBJECT);
                SplatPrototype prototype = m_terrain.get_terrainData().get_splatPrototypes()[i];
                obj11.AddField("diffuse", prototype.get_texture().get_name().ToLower() + ".jpg");
                if (prototype.get_normalMap() != null)
                {
                    obj11.AddField("normal", prototype.get_normalMap().get_name().ToLower() + ".jpg");
                }
                JSONObject obj12 = new JSONObject(JSONObject.Type.ARRAY);
                obj12.Add((float) (prototype.get_tileSize().x / val));
                obj12.Add((float) (prototype.get_tileSize().y / val));
                obj11.AddField("scale", obj12);
                JSONObject obj13 = new JSONObject(JSONObject.Type.ARRAY);
                obj13.Add(prototype.get_tileOffset().x);
                obj13.Add(prototype.get_tileOffset().y);
                obj11.AddField("offset", obj13);
                obj8.Add(obj11);
            }
            m_chunkInfoNode = new JSONObject(JSONObject.Type.ARRAY);
            node.AddField("chunkInfo", m_chunkInfoNode);
            float[,] heightsData = GetHeightsData();
            Texture2D textured = ExportNormal(calcNormalOfTriangle(heightsData, (((num2 * LEAF_GRID_NUM) * num3) * LEAF_GRID_NUM) * 2, val));
            JSONObject obj9 = new JSONObject(JSONObject.Type.ARRAY);
            obj9.Add(textured.get_name().ToLower() + ".png");
            node.AddField("normalMap", obj9);
            int num5 = 0;
            int num6 = 0;
            int index = 0;
            int num8 = 0;
            int num9 = 0;
            int width = 0;
            int height = 0;
            Color[] color = null;
            TextureFormat format = 0;
            for (num5 = 0; num5 < num3; num5++)
            {
                for (num6 = 0; num6 < num2; num6++)
                {
                    length = m_terrain.get_terrainData().get_alphamapTextures().Length;
                    m_chunkNode = new JSONObject(JSONObject.Type.OBJECT);
                    m_alphamapArrayNode = new JSONObject(JSONObject.Type.ARRAY);
                    m_detailIDArrayNode = new JSONObject(JSONObject.Type.ARRAY);
                    m_splatIndex = 0;
                    for (index = 0; index < length; index++)
                    {
                        Texture2D textured2 = m_terrain.get_terrainData().get_alphamapTextures()[index];
                        if ((textured2.get_width() % num2) != 0)
                        {
                            Debug.LogError("Control Texture(alpha map) 的宽必须是" + num2 + "的倍数");
                            return;
                        }
                        if ((textured2.get_height() % num3) != 0)
                        {
                            Debug.LogError("Control Texture(alpha map) 的高必须是" + num3 + "的倍数");
                            return;
                        }
                        width = textured2.get_width() / num2;
                        height = textured2.get_height() / num3;
                        num8 = num6 * width;
                        num9 = textured2.get_height() - ((num5 + 1) * height);
                        color = textured2.GetPixels(num8, num9, width, height);
                        format = textured2.get_format();
                        MergeAlphaMap(string.Concat(new object[] { m_terrain.get_name().ToLower(), "_splatalpha{0}_", num6, "_", num5, m_debug ? ".jpg" : ".png" }), format, width, height, color);
                    }
                    AfterMergeAlphaMap(string.Concat(new object[] { m_terrain.get_name().ToLower(), "_splatalpha{0}_", num6, "_", num5, m_debug ? ".jpg" : ".png" }), format, width, height);
                    m_chunkNode.AddField("normalMap", 0);
                }
                ExportSplat();
                ExportHeightmap16(heightsData);
                ExportAlphamap();
            }
            JSONObject obj10 = new JSONObject(JSONObject.Type.ARRAY);
            for (int j = 0; j < m_alphamapDataList.Count; j++)
            {
                KeyValuePair<string, Color[]> pair = m_alphamapDataList[j];
                obj10.Add(pair.Key.ToLower());
            }
            node.AddField("alphaMap", obj10);
            saveData(node);
        }
    }

    private static int GetAlphaMapCached(Color[] color, List<KeyValuePair<string, Color[]>> alphamapDataList)
    {
        for (int i = 0; i < alphamapDataList.Count; i++)
        {
            bool flag = true;
            for (int j = 0; j < color.Length; j++)
            {
                KeyValuePair<string, Color[]> pair = alphamapDataList[i];
                if (color[j] != pair.Value[j])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return i;
            }
        }
        return -1;
    }

    private static float[,] GetHeightsData()
    {
        float[,] numArray = m_terrain.get_terrainData().GetHeights(0, 0, m_terrain.get_terrainData().get_heightmapWidth(), m_terrain.get_terrainData().get_heightmapHeight());
        float[,] numArray2 = new float[m_terrain.get_terrainData().get_heightmapWidth(), m_terrain.get_terrainData().get_heightmapHeight()];
        for (int i = m_terrain.get_terrainData().get_heightmapHeight() - 1; i >= 0; i--)
        {
            for (int j = 0; j < m_terrain.get_terrainData().get_heightmapWidth(); j++)
            {
                numArray2[(m_terrain.get_terrainData().get_heightmapHeight() - 1) - i, j] = numArray[i, j];
            }
        }
        return numArray2;
    }

    private static bool IsAlphaMapEmpty(Color[] color)
    {
        for (int i = 0; i < color.Length; i++)
        {
            if (color[i] != Color.get_clear())
            {
                return false;
            }
        }
        return true;
    }

    private static void MergeAlphaMap(string fileName, TextureFormat format, int width, int height, Color[] color)
    {
        for (int i = 0; i < 4; i++)
        {
            if (m_alphaBlock == null)
            {
                m_detailID = new JSONObject(JSONObject.Type.ARRAY);
                m_alphaBlockName = fileName;
                m_alphaBlock = new Color[color.Length];
                for (int k = 0; k < m_alphaBlock.Length; k++)
                {
                    m_alphaBlock[k] = new Color(0f, 0f, 0f, 0f);
                }
            }
            bool flag = true;
            for (int j = 0; j < color.Length; j++)
            {
                float num4 = color[j].get_Item(i);
                m_alphaBlock[j].set_Item(m_channelIndex, num4);
                if (num4 != 0f)
                {
                    flag = false;
                }
            }
            if (!flag)
            {
                m_detailID.Add(m_splatIndex);
                m_channelIndex++;
                if (m_channelIndex > 3)
                {
                    SaveAlphaMap(m_alphaBlockName, format, width, height);
                }
            }
            m_splatIndex++;
        }
    }

    private static void SaveAlphaMap(string fileName, TextureFormat format, int width, int height)
    {
        if ((m_alphaIndex <= 0) || !IsAlphaMapEmpty(m_alphaBlock))
        {
            int alphaMapCached = GetAlphaMapCached(m_alphaBlock, m_alphamapDataList);
            if (alphaMapCached == -1)
            {
                Texture2D textured = new Texture2D(width, height, format, false);
                Color[] colorArray = new Color[m_alphaBlock.Length];
                for (int i = 0; i < m_alphaBlock.Length; i++)
                {
                    float g = m_alphaBlock[i].g;
                    float b = m_alphaBlock[i].b;
                    float a = m_alphaBlock[i].a;
                    colorArray[i].r = g;
                    colorArray[i].g = b;
                    colorArray[i].b = a;
                    float num6 = ((m_alphaBlock[i].r + g) + b) + a;
                    colorArray[i].a = (num6 > 1f) ? 1f : num6;
                }
                textured.SetPixels(colorArray);
                textured.Apply();
                string key = string.Format(fileName, m_alphaIndex).ToLower();
                File.WriteAllBytes(m_saveLocation + "/" + key, m_debug ? ImageConversion.EncodeToJPG(textured) : ImageConversion.EncodeToPNG(textured));
                m_alphamapDataList.Add(new KeyValuePair<string, Color[]>(key, m_alphaBlock));
                alphaMapCached = m_alphamapDataList.Count - 1;
            }
            m_alphamapArrayNode.Add(alphaMapCached);
            m_detailIDArrayNode.Add(m_detailID);
        }
        m_alphaIndex++;
        m_alphaBlock = null;
        m_channelIndex = 0;
    }

    public static void saveData(JSONObject node)
    {
        string str = node.Print(true);
        StreamWriter writer1 = new StreamWriter(new FileStream(m_saveLocation + "/" + m_terrain.get_name().ToLower() + ".lt", System.IO.FileMode.Create, FileAccess.Write));
        writer1.Write(str);
        writer1.Close();
    }

    public static void saveLightMapData(JSONObject obj)
    {
        if ((m_terrain != null) && (m_terrain.get_lightmapIndex() > -1))
        {
            obj.AddField("lightmapIndex", m_terrain.get_lightmapIndex());
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            obj.AddField("lightmapScaleOffset", obj2);
            obj2.Add(m_terrain.get_lightmapScaleOffset().x);
            obj2.Add(m_terrain.get_lightmapScaleOffset().y);
            obj2.Add(m_terrain.get_lightmapScaleOffset().z);
            obj2.Add(-m_terrain.get_lightmapScaleOffset().w);
        }
    }
}

