namespace LayaExport
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Util;

    public class DataManager
    {
        public static bool BatchMade;
        public static bool ConvertLightMap;
        public static bool ConvertNonPNGAndJPG;
        private static List<string> ConvertOriginalTextureTypeList;
        public static bool ConvertOriginJPG;
        public static bool ConvertOriginPNG;
        public static float ConvertQuality;
        public static bool ConvertTerrainToMesh;
        public static bool ConvertToJPG;
        public static bool ConvertToPNG;
        public static bool CoverOriginalFile;
        private static bool curNodeHasLegalChild;
        private static bool curNodeHasLocalParticleChild;
        private static bool curNodeHasNotLegalChild;
        public static bool CustomizeDirectory;
        public static string CustomizeDirectoryName;
        private static int directionalLightCurCount = 0;
        private static int directionalLightTotalCount = 1;
        public static bool IgnoreColor;
        public static bool IgnoreNotActiveGameObject;
        public static bool IgnoreNullGameObject;
        public static bool IgnoreTangent;
        public static bool LayaAuto = false;
        private static List<Dictionary<GameObject, string>> layaAutoGameObjectsList = new List<Dictionary<GameObject, string>>();
        private static int LayaAutoGOListIndex = 0;
        private static int MaxBoneCount = 0x18;
        public static bool OptimizeGameObject;
        public static bool OptimizeMeshName;
        public static GameObject pathFindGameObject;
        private static float precision = 0.01f;
        public static string SAVEPATH;
        public static float ScaleFactor;
        private static string sceneName;
        public static bool SimplifyBone;
        public static int TerrainToMeshResolution;
        public static int Type;
        public static string VERSION = "1.7.16 beta";
        private static int[] VertexStructure = new int[7];

        public static bool checkChildBoneIsLegal(Transform root, Transform bone, Transform skinBone, int count)
        {
            if (root == bone)
            {
                return true;
            }
            for (int i = 0; i < count; i++)
            {
                if (skinBone == root)
                {
                    return false;
                }
                if (bone == skinBone)
                {
                    return true;
                }
                skinBone = skinBone.get_parent();
            }
            return false;
        }

        public static void checkChildHasLocalParticle(GameObject gameObject, bool isTopNode)
        {
            if (isTopNode)
            {
                curNodeHasLocalParticleChild = false;
            }
            if (gameObject.get_transform().get_childCount() > 0)
            {
                for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                {
                    GameObject obj2 = gameObject.get_transform().GetChild(i).get_gameObject();
                    if ((componentsOnGameObject(obj2).IndexOf(ComponentType.ParticleSystem) != -1) && (obj2.GetComponent<ParticleSystem>().get_main().get_scalingMode().ToString() == "Local"))
                    {
                        curNodeHasLocalParticleChild = true;
                    }
                    checkChildHasLocalParticle(obj2, false);
                }
            }
        }

        public static void checkChildIsLegal(GameObject gameObject, bool isTopNode)
        {
            if (isTopNode)
            {
                curNodeHasLegalChild = false;
            }
            if (gameObject.get_transform().get_childCount() > 0)
            {
                for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                {
                    GameObject obj1 = gameObject.get_transform().GetChild(i).get_gameObject();
                    if (componentsOnGameObject(obj1).Count > 1)
                    {
                        curNodeHasLegalChild = true;
                    }
                    checkChildIsLegal(obj1, false);
                }
            }
        }

        public static void checkChildIsNotLegal(GameObject gameObject, bool isTopNode)
        {
            if (isTopNode)
            {
                curNodeHasNotLegalChild = false;
            }
            if (componentsOnGameObject(gameObject).Count <= 1)
            {
                curNodeHasNotLegalChild = true;
            }
            if (gameObject.get_transform().get_childCount() > 0)
            {
                for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                {
                    GameObject obj1 = gameObject.get_transform().GetChild(i).get_gameObject();
                    if (componentsOnGameObject(obj1).Count <= 1)
                    {
                        curNodeHasNotLegalChild = true;
                    }
                    checkChildIsNotLegal(obj1, false);
                }
            }
        }

        public static string cleanIllegalChar(string str, bool heightLevel)
        {
            str = str.Replace("<", "_");
            str = str.Replace(">", "_");
            str = str.Replace("\"", "_");
            str = str.Replace("|", "_");
            str = str.Replace("?", "_");
            str = str.Replace("*", "_");
            str = str.Replace("#", "_");
            if (heightLevel)
            {
                str = str.Replace("/", "_");
                str = str.Replace(":", "_");
            }
            return str;
        }

        public static List<ComponentType> componentsOnGameObject(GameObject gameObject)
        {
            gameObject.GetComponent<Transform>();
            List<ComponentType> list = new List<ComponentType>();
            Camera component = gameObject.GetComponent<Camera>();
            Light light = gameObject.GetComponent<Light>();
            MeshFilter filter = gameObject.GetComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
            SkinnedMeshRenderer renderer2 = gameObject.GetComponent<SkinnedMeshRenderer>();
            Animation animation = gameObject.GetComponent<Animation>();
            ParticleSystem system = gameObject.GetComponent<ParticleSystem>();
            Terrain terrain = gameObject.GetComponent<Terrain>();
            SphereCollider collider = gameObject.GetComponent<SphereCollider>();
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            list.Add(ComponentType.Transform);
            if (gameObject.GetComponent<TrailRenderer>() != null)
            {
                list.Add(ComponentType.TrailRenderer);
            }
            if (gameObject.GetComponent<BoxCollider>() != null)
            {
                list.Add(ComponentType.BoxCollider);
            }
            if (collider != null)
            {
                list.Add(ComponentType.SphereCollider);
            }
            if (rigidbody != null)
            {
                list.Add(ComponentType.Rigidbody);
            }
            if (animation != null)
            {
                list.Add(ComponentType.Animation);
            }
            if (gameObject.GetComponent<Animator>() != null)
            {
                if (animation == null)
                {
                    list.Add(ComponentType.Animator);
                }
                else if (animation != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " Animation and Animator can't exist at the same time !");
                }
            }
            if (component != null)
            {
                list.Add(ComponentType.Camera);
            }
            if ((light != null) && (light.get_type() == 1))
            {
                list.Add(ComponentType.DirectionalLight);
            }
            if (filter != null)
            {
                if (component == null)
                {
                    list.Add(ComponentType.MeshFilter);
                    if (renderer == null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " need a MeshRenderer ComponentType !");
                    }
                }
                else if (component != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " Camera and MeshFilter can't exist at the same time !");
                }
            }
            if (renderer != null)
            {
                if (component == null)
                {
                    list.Add(ComponentType.MeshRenderer);
                    if (filter == null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " need a meshFilter ComponentType !");
                    }
                }
                else if (component != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " Camera and MeshRenderer can't exist at the same time !");
                }
            }
            if (renderer2 != null)
            {
                if (((component == null) && (filter == null)) && (renderer == null))
                {
                    list.Add(ComponentType.SkinnedMeshRenderer);
                }
                else
                {
                    if (component != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " Camera and SkinnedMeshRenderer can't exist at the same time !");
                    }
                    if (filter != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " MeshFilter and SkinnedMeshRenderer can't exist at the same time !");
                    }
                    if (renderer != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " MeshRenderer and SkinnedMeshRenderer can't exist at the same time !");
                    }
                }
            }
            if (system != null)
            {
                if (((component == null) && (filter == null)) && ((renderer == null) && (renderer2 == null)))
                {
                    list.Add(ComponentType.ParticleSystem);
                }
                else
                {
                    if (component != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " Camera and ParticleSystem can't exist at the same time !");
                    }
                    if (filter != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " MeshFilter and ParticleSystem can't exist at the same time !");
                    }
                    if (renderer != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " MeshRenderer and ParticleSystem can't exist at the same time !");
                    }
                    if (renderer2 != null)
                    {
                        Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " SkinnedMeshRenderer and ParticleSystem can't exist at the same time !");
                    }
                }
            }
            if (terrain != null)
            {
                if ((((component == null) && (filter == null)) && ((renderer == null) && (renderer2 == null))) && (system == null))
                {
                    list.Add(ComponentType.Terrain);
                    return list;
                }
                if (component != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " Camera and Terrain can't exist at the same time !");
                }
                if (filter != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " MeshFilter and Terrain can't exist at the same time !");
                }
                if (renderer != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " MeshRenderer and Terrain can't exist at the same time !");
                }
                if (renderer2 != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " SkinnedMeshRenderer and Terrain can't exist at the same time !");
                }
                if (system != null)
                {
                    Debug.LogWarning("LayaAir3D : " + gameObject.get_name() + " ParticleSystem and Terrain can't exist at the same time !");
                }
            }
            return list;
        }

        public static bool findStrsInCurString(string curString, List<string> strs)
        {
            int num = curString.Length - 4;
            for (int i = 0; i < strs.Count; i++)
            {
                if (curString.IndexOf(strs[i]) == num)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool frameDataIsEquals(FrameData f1, FrameData f2)
        {
            Vector3 localPosition = f1.localPosition;
            Quaternion localRotation = f1.localRotation;
            Vector3 localScale = f1.localScale;
            Vector3 vector3 = f2.localPosition;
            Quaternion quaternion2 = f2.localRotation;
            Vector3 vector4 = f2.localScale;
            return ((((MathUtil.isSimilar(localPosition.x, vector3.x) && MathUtil.isSimilar(localPosition.y, vector3.y)) && (MathUtil.isSimilar(localPosition.z, vector3.z) && MathUtil.isSimilar(localRotation.x, quaternion2.x))) && ((MathUtil.isSimilar(localRotation.y, quaternion2.y) && MathUtil.isSimilar(localRotation.z, quaternion2.z)) && (MathUtil.isSimilar(localRotation.w, quaternion2.w) && MathUtil.isSimilar(localScale.x, vector4.x)))) && (MathUtil.isSimilar(localScale.y, vector4.y) && MathUtil.isSimilar(localScale.z, vector4.z)));
        }

        public static void getAnimatorComponentData(GameObject gameObject, JSONObject component, List<string> linkSprite)
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj5 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
            Avatar avatar = gameObject.GetComponent<Animator>().get_avatar();
            if (avatar != null)
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(avatar.GetInstanceID());
                string[] textArray1 = new string[6];
                char[] separator = new char[] { '.' };
                textArray1[0] = cleanIllegalChar(assetPath.Split(separator)[0], false);
                textArray1[1] = "-";
                textArray1[2] = cleanIllegalChar(gameObject.get_name(), true);
                textArray1[3] = "-";
                textArray1[4] = avatar.get_name();
                textArray1[5] = ".lav";
                string val = string.Concat(textArray1);
                string path = SAVEPATH + "/" + val;
                if (File.Exists(path) && !CoverOriginalFile)
                {
                    return;
                }
                JSONObject parentsChildNodes = new JSONObject(JSONObject.Type.ARRAY);
                getLavData(gameObject, parentsChildNodes, gameObject);
                node.AddField("version", "LAYAAVATAR:01");
                node.AddField("rootNode", parentsChildNodes[0]);
                Util.FileUtil.saveFile(path, node);
                obj2.AddField("avatar", obj4);
                obj4.AddField("path", val);
                obj4.AddField("linkSprites", obj5);
                for (int i = 0; i < linkSprite.Count; i++)
                {
                    JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
                    obj8.Add(linkSprite[i]);
                    obj5.AddField(linkSprite[i], obj8);
                }
            }
            saveLaniData(gameObject, obj3);
            obj2.AddField("clipPaths", obj3);
            obj2.AddField("playOnWake", true);
            component.AddField("Animator", obj2);
        }

        public static void getBoxColliderComponentData(GameObject gameObject, JSONObject component)
        {
            foreach (BoxCollider collider in gameObject.GetComponents<BoxCollider>())
            {
                JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
                JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
                obj2.AddField("isTrigger", collider.get_isTrigger());
                Vector3 vector = collider.get_center();
                obj3.Add(vector.x);
                obj3.Add(vector.y);
                obj3.Add(vector.z);
                obj2.AddField("center", obj3);
                Vector3 vector2 = collider.get_size();
                obj4.Add(vector2.x);
                obj4.Add(vector2.y);
                obj4.Add(vector2.z);
                obj2.AddField("size", obj4);
                component.AddField("BoxCollider", obj2);
            }
        }

        public static void getCameraComponentData(GameObject gameObject, JSONObject props, JSONObject customProps)
        {
            Camera component = gameObject.GetComponent<Camera>();
            if (component.get_clearFlags() == 1)
            {
                props.AddField("clearFlag", 1);
            }
            else if ((component.get_clearFlags() == 2) || (component.get_clearFlags() == 2))
            {
                props.AddField("clearFlag", 0);
            }
            else if (component.get_clearFlags() == 3)
            {
                props.AddField("clearFlag", 2);
            }
            else
            {
                props.AddField("clearFlag", 3);
            }
            props.AddField("orthographic", component.get_orthographic());
            if (component.get_orthographic())
            {
                props.AddField("orthographicVerticalSize", (float) (component.get_orthographicSize() * 2f));
            }
            else
            {
                props.AddField("fieldOfView", component.get_fieldOfView());
            }
            props.AddField("nearPlane", component.get_nearClipPlane());
            props.AddField("farPlane", component.get_farClipPlane());
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            Rect rect = component.get_rect();
            obj2.Add(rect.get_x());
            obj2.Add(rect.get_y());
            obj2.Add(rect.get_width());
            obj2.Add(rect.get_height());
            customProps.AddField("viewport", obj2);
            JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
            Color color = component.get_backgroundColor();
            obj3.Add(color.r);
            obj3.Add(color.g);
            obj3.Add(color.b);
            obj3.Add(color.a);
            customProps.AddField("clearColor", obj3);
            Skybox[] components = gameObject.GetComponents<Skybox>();
            if (components.Length != 0)
            {
                JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("skyBox", obj4);
                JSONObject obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj4.AddField("sharedMaterial", obj5);
                for (int i = 0; i < components.Length; i++)
                {
                    Skybox skybox = components[i];
                    if (skybox.get_enabled())
                    {
                        Material material = skybox.get_material();
                        if (material != null)
                        {
                            char[] separator = new char[] { '.' };
                            string val = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".lmat";
                            string path = SAVEPATH + "/" + val;
                            if (material.get_shader().get_name() == "Skybox/6 Sided")
                            {
                                obj5.AddField("type", "Laya.SkyBoxMaterial");
                                obj5.AddField("path", val);
                                if (File.Exists(path) && !CoverOriginalFile)
                                {
                                    break;
                                }
                                saveLayaSkyBoxData(material, path);
                                return;
                            }
                        }
                    }
                }
            }
        }

        public static void getComponentsData(GameObject gameObject, JSONObject node, JSONObject child, Vector3 position, Quaternion rotation, Vector3 scale, ref string goPath)
        {
            List<ComponentType> list = componentsOnGameObject(gameObject);
            JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
            node.AddField("type", "");
            if (list.IndexOf(ComponentType.Transform) != -1)
            {
                node.SetField("type", "Sprite3D");
            }
            if (list.IndexOf(ComponentType.BoxCollider) != -1)
            {
                node.SetField("type", "Sprite3D");
            }
            if (list.IndexOf(ComponentType.SphereCollider) != -1)
            {
                node.SetField("type", "Sprite3D");
            }
            if (list.IndexOf(ComponentType.Rigidbody) != -1)
            {
                node.SetField("type", "Sprite3D");
            }
            if (list.IndexOf(ComponentType.Animation) != -1)
            {
                node.SetField("type", "Sprite3D");
            }
            if (list.IndexOf(ComponentType.Animator) != -1)
            {
                node.SetField("type", "Sprite3D");
            }
            if (list.IndexOf(ComponentType.DirectionalLight) != -1)
            {
                node.SetField("type", "DirectionLight");
            }
            if (list.IndexOf(ComponentType.Camera) != -1)
            {
                node.SetField("type", "Camera");
            }
            if (list.IndexOf(ComponentType.MeshFilter) != -1)
            {
                node.SetField("type", "MeshSprite3D");
            }
            if (list.IndexOf(ComponentType.MeshRenderer) != -1)
            {
                node.SetField("type", "MeshSprite3D");
            }
            if (list.IndexOf(ComponentType.SkinnedMeshRenderer) != -1)
            {
                if (selectParentbyType(gameObject, ComponentType.Animation) != null)
                {
                    node.SetField("type", "MeshSprite3D");
                }
                else
                {
                    node.SetField("type", "SkinnedMeshSprite3D");
                }
            }
            if (list.IndexOf(ComponentType.ParticleSystem) != -1)
            {
                node.SetField("type", "ShuriKenParticle3D");
            }
            if (list.IndexOf(ComponentType.Terrain) != -1)
            {
                if (ConvertTerrainToMesh)
                {
                    node.SetField("type", "MeshSprite3D");
                }
                else
                {
                    node.SetField("type", "Terrain");
                }
            }
            if (list.IndexOf(ComponentType.TrailRenderer) != -1)
            {
                node.SetField("type", "TrailSprite3D");
            }
            node.AddField("props", obj2);
            obj2.AddField("isStatic", gameObject.get_isStatic());
            obj2.AddField("name", gameObject.get_name());
            goPath = goPath + "/" + gameObject.get_name();
            node.AddField("customProps", obj3);
            if (gameObject.get_layer() == 0x1f)
            {
                Debug.LogWarning("LayaUnityPlugin : layer must less than 31 !");
            }
            else
            {
                obj3.AddField("layer", gameObject.get_layer());
            }
            node.AddField("components", obj4);
            if (IsLayaAutoGameObjects(gameObject))
            {
                layaAutoGameObjectsList[LayaAutoGOListIndex].Add(gameObject, goPath);
            }
            if (list.IndexOf(ComponentType.Transform) != -1)
            {
                getTransformComponentData(gameObject, obj3, position, rotation, scale);
            }
            if (list.IndexOf(ComponentType.BoxCollider) != -1)
            {
                getBoxColliderComponentData(gameObject, obj4);
            }
            if (list.IndexOf(ComponentType.SphereCollider) != -1)
            {
                getSphereColliderComponentData(gameObject, obj4);
            }
            if (list.IndexOf(ComponentType.Rigidbody) != -1)
            {
                getRigidbodyComponentData(gameObject, obj4);
            }
            if (list.IndexOf(ComponentType.Animation) != -1)
            {
                saveLsaniData(gameObject);
                for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                {
                    getGameObjectData(gameObject.get_transform().GetChild(i).get_gameObject(), goPath, child, false);
                }
            }
            if (list.IndexOf(ComponentType.Animator) != -1)
            {
                List<string> linkSprite = new List<string>();
                if (gameObject.GetComponent<Animator>().get_avatar() != null)
                {
                    getSkinAniGameObjectData(gameObject, goPath, child, linkSprite);
                }
                else
                {
                    for (int j = 0; j < gameObject.get_transform().get_childCount(); j++)
                    {
                        getGameObjectData(gameObject.get_transform().GetChild(j).get_gameObject(), goPath, child, false);
                    }
                }
                getAnimatorComponentData(gameObject, obj4, linkSprite);
            }
            if (list.IndexOf(ComponentType.DirectionalLight) != -1)
            {
                getDirectionalLightComponentData(gameObject, obj2, obj3);
            }
            if (list.IndexOf(ComponentType.Camera) != -1)
            {
                getCameraComponentData(gameObject, obj2, obj3);
            }
            if (list.IndexOf(ComponentType.MeshFilter) != -1)
            {
                getMeshFilterComponentData(gameObject, obj3);
            }
            if (list.IndexOf(ComponentType.MeshRenderer) != -1)
            {
                getMeshRendererComponentData(gameObject, obj3);
            }
            if (list.IndexOf(ComponentType.SkinnedMeshRenderer) != -1)
            {
                getSkinnedMeshRendererComponentData(gameObject, obj3);
            }
            if (list.IndexOf(ComponentType.ParticleSystem) != -1)
            {
                getParticleSystemComponentData(gameObject, obj3);
            }
            if (list.IndexOf(ComponentType.Terrain) != -1)
            {
                getTerrainComponentData(gameObject, obj3);
            }
            if (list.IndexOf(ComponentType.TrailRenderer) != -1)
            {
                getTrailRendererComponentData(gameObject, obj2, obj3);
            }
        }

        public static void getData()
        {
            sceneName = SceneManager.GetActiveScene().get_name();
            sceneName = cleanIllegalChar(sceneName, true);
            if (sceneName == "")
            {
                sceneName = "layaScene";
            }
            string str = "";
            if (CustomizeDirectory && (CustomizeDirectoryName != ""))
            {
                CustomizeDirectoryName = cleanIllegalChar(CustomizeDirectoryName, true);
                str = "/" + CustomizeDirectoryName;
            }
            else
            {
                str = "/LayaScene_" + sceneName;
            }
            SAVEPATH = SAVEPATH + str;
            ConvertOriginalTextureTypeList = new List<string>();
            if (ConvertNonPNGAndJPG)
            {
                ConvertOriginalTextureTypeList.Add(".tga");
                ConvertOriginalTextureTypeList.Add(".TGA");
                ConvertOriginalTextureTypeList.Add(".psd");
                ConvertOriginalTextureTypeList.Add(".PSD");
                ConvertOriginalTextureTypeList.Add(".gif");
                ConvertOriginalTextureTypeList.Add(".GIF");
                ConvertOriginalTextureTypeList.Add(".tif");
                ConvertOriginalTextureTypeList.Add(".TIF");
                ConvertOriginalTextureTypeList.Add(".bmp");
                ConvertOriginalTextureTypeList.Add(".BMP");
                ConvertOriginalTextureTypeList.Add(".exr");
                ConvertOriginalTextureTypeList.Add(".EXR");
            }
            if (ConvertOriginPNG)
            {
                ConvertOriginalTextureTypeList.Add(".png");
                ConvertOriginalTextureTypeList.Add(".PNG");
            }
            if (ConvertOriginJPG)
            {
                ConvertOriginalTextureTypeList.Add(".jpg");
                ConvertOriginalTextureTypeList.Add(".JPG");
            }
            directionalLightCurCount = 0;
            recodeLayaJSFile(str + "/" + sceneName);
            if (LayaAuto)
            {
                saveLayaAutoData();
            }
            else
            {
                saveData();
            }
        }

        public static void getDirectionalLightComponentData(GameObject gameObject, JSONObject props, JSONObject customProps)
        {
            Light component = gameObject.GetComponent<Light>();
            props.AddField("intensity", component.get_intensity());
            switch (component.get_lightmapBakeType())
            {
                case 1:
                    props.AddField("lightmapBakedType", 1);
                    break;

                case 2:
                    props.AddField("lightmapBakedType", 2);
                    break;

                case 4:
                    props.AddField("lightmapBakedType", 0);
                    break;

                default:
                    props.AddField("lightmapBakedType", 0);
                    break;
            }
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            Color color = component.get_color();
            obj2.Add(color.r);
            obj2.Add(color.g);
            obj2.Add(color.b);
            customProps.AddField("color", obj2);
            directionalLightCurCount++;
        }

        public static void getGameObjectData(GameObject gameObject, string gameObjectPath, JSONObject parentsChildNodes, bool ignoreNullChild = false)
        {
            List<ComponentType> list = componentsOnGameObject(gameObject);
            checkChildIsLegal(gameObject, true);
            if (((((gameObject.get_activeInHierarchy() || !IgnoreNotActiveGameObject) && (!LayaAuto || !IsCustomGameObject(gameObject))) && ((!OptimizeGameObject && !IgnoreNullGameObject) || ((list.Count > 1) || curNodeHasLegalChild))) && !((list.Count <= 1) & ignoreNullChild)) && ((list.IndexOf(ComponentType.DirectionalLight) == -1) || (directionalLightCurCount < directionalLightTotalCount)))
            {
                JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject child = new JSONObject(JSONObject.Type.ARRAY);
                Vector3 position = gameObject.get_transform().get_localPosition();
                Quaternion rotation = gameObject.get_transform().get_localRotation();
                Vector3 scale = gameObject.get_transform().get_localScale();
                string goPath = gameObjectPath;
                getComponentsData(gameObject, node, child, position, rotation, scale, ref goPath);
                checkChildHasLocalParticle(gameObject, true);
                if (((gameObject.get_transform().get_childCount() > 0) && (list.IndexOf(ComponentType.Animation) == -1)) && (list.IndexOf(ComponentType.Animator) == -1))
                {
                    if ((OptimizeGameObject && (selectParentbyType(gameObject, ComponentType.Animator) == null)) && ((selectParentbyType(gameObject, ComponentType.Animation) == null) && !curNodeHasLocalParticleChild))
                    {
                        getSimpleGameObjectData(gameObject, goPath, child);
                    }
                    else
                    {
                        for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                        {
                            getGameObjectData(gameObject.get_transform().GetChild(i).get_gameObject(), goPath, child, false);
                        }
                    }
                }
                node.AddField("child", child);
                parentsChildNodes.Add(node);
            }
        }

        public static void getLavData(GameObject gameObject, JSONObject parentsChildNodes, GameObject animatorGameObject)
        {
            checkChildIsNotLegal(gameObject, true);
            if (((gameObject.get_activeInHierarchy() || !IgnoreNotActiveGameObject) && (((selectParentbyType(gameObject, ComponentType.Animator) == animatorGameObject) && (componentsOnGameObject(gameObject).IndexOf(ComponentType.Animator) == -1)) || (gameObject == animatorGameObject))) && curNodeHasNotLegalChild)
            {
                JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
                JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
                JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
                Vector3 vector = gameObject.get_transform().get_localPosition();
                Quaternion quaternion = gameObject.get_transform().get_localRotation();
                Vector3 vector2 = gameObject.get_transform().get_localScale();
                obj3.AddField("name", gameObject.get_name());
                obj2.AddField("props", obj3);
                obj4 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("customProps", obj4);
                obj5 = new JSONObject(JSONObject.Type.ARRAY);
                obj4.AddField("translate", obj5);
                obj5.Add((float) (vector.x * -1f));
                obj5.Add(vector.y);
                obj5.Add(vector.z);
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj4.AddField("rotation", obj6);
                obj6.Add((float) (quaternion.x * -1f));
                obj6.Add(quaternion.y);
                obj6.Add(quaternion.z);
                obj6.Add((float) (quaternion.w * -1f));
                obj7 = new JSONObject(JSONObject.Type.ARRAY);
                obj4.AddField("scale", obj7);
                obj7.Add(vector2.x);
                obj7.Add(vector2.y);
                obj7.Add(vector2.z);
                if (gameObject.get_transform().get_childCount() > 0)
                {
                    JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
                    obj2.AddField("child", obj8);
                    for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                    {
                        getLavData(gameObject.get_transform().GetChild(i).get_gameObject(), obj8, animatorGameObject);
                    }
                }
                else
                {
                    obj2.AddField("child", new JSONObject(JSONObject.Type.ARRAY));
                }
                parentsChildNodes.Add(obj2);
            }
        }

        public static JSONObject getLayaAutoSceneNode()
        {
            JSONObject obj2 = getSceneNode();
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            obj2.GetField("customProps").AddField("gameObjects", obj3);
            foreach (KeyValuePair<GameObject, string> pair in layaAutoGameObjectsList[0])
            {
                JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                obj4.AddField("name", pair.Key.get_name());
                obj4.AddField("renderSprite", pair.Value);
                obj3.Add(obj4);
            }
            return obj2;
        }

        public static Dictionary<string, JSONObject> getLayaAutoSpriteNode()
        {
            Dictionary<string, JSONObject> dictionary = new Dictionary<string, JSONObject>();
            int num = 0;
            foreach (KeyValuePair<string, JSONObject> pair in saveSpriteNode())
            {
                JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("renderTree", pair.Value);
                JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("gameObjects", obj3);
                foreach (KeyValuePair<GameObject, string> pair2 in layaAutoGameObjectsList[num++])
                {
                    JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                    obj4.AddField("name", pair2.Key.get_name());
                    obj4.AddField("renderSprite", pair2.Value);
                    obj3.Add(obj4);
                }
                dictionary.Add(pair.Key, obj2);
            }
            return dictionary;
        }

        public static void getMeshFilterComponentData(GameObject gameObject, JSONObject customProps)
        {
            Mesh mesh = gameObject.GetComponent<MeshFilter>().get_sharedMesh();
            if (mesh != null)
            {
                string str = cleanIllegalChar(mesh.get_name(), true);
                char[] separator = new char[] { '.' };
                string val = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(mesh.GetInstanceID()).Split(separator)[0], false) + "-" + str;
                if (!OptimizeMeshName)
                {
                    object[] objArray1 = new object[] { val, "[", mesh.GetInstanceID(), "].lm" };
                    val = string.Concat(objArray1);
                }
                else
                {
                    val = val + ".lm";
                }
                string path = SAVEPATH + "/" + val;
                customProps.AddField("meshPath", val);
                if (!File.Exists(path) || CoverOriginalFile)
                {
                    saveLmFile(mesh, path);
                }
            }
            else
            {
                Debug.LogWarning("LayaUnityPlugin : " + gameObject.get_name() + "'s Mesh data can't be null!");
            }
        }

        public static void getMeshRendererComponentData(GameObject gameObject, JSONObject customProps)
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            if (Type == 0)
            {
                MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
                if (component.get_lightmapIndex() > -1)
                {
                    customProps.AddField("lightmapIndex", component.get_lightmapIndex());
                    customProps.AddField("lightmapScaleOffset", obj2);
                    obj2.Add(component.get_lightmapScaleOffset().x);
                    obj2.Add(component.get_lightmapScaleOffset().y);
                    obj2.Add(component.get_lightmapScaleOffset().z);
                    obj2.Add(-component.get_lightmapScaleOffset().w);
                }
            }
            Material[] materialArray = gameObject.GetComponent<MeshRenderer>().get_sharedMaterials();
            JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("materials", obj3);
            for (int i = 0; i < materialArray.Length; i++)
            {
                Material material = materialArray[i];
                if (material != null)
                {
                    char[] separator = new char[] { '.' };
                    string val = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".lmat";
                    string path = SAVEPATH + "/" + val;
                    string str3 = material.get_shader().get_name();
                    char[] chArray2 = new char[] { '/' };
                    if (str3.Split(chArray2)[0] == "LayaAir3D")
                    {
                        JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                        char[] chArray3 = new char[] { '/' };
                        if (str3.Split(chArray3)[1] == "BlinnPhong")
                        {
                            obj4.AddField("type", "Laya.BlinnPhongMaterial");
                        }
                        else
                        {
                            char[] chArray4 = new char[] { '/' };
                            if (str3.Split(chArray4)[1] == "PBR(Standard)")
                            {
                                obj4.AddField("type", "Laya.PBRStandardMaterial");
                            }
                            else
                            {
                                char[] chArray5 = new char[] { '/' };
                                if (str3.Split(chArray5)[1] == "PBR(Specular)")
                                {
                                    obj4.AddField("type", "Laya.PBRSpecularMaterial");
                                }
                            }
                        }
                        obj4.AddField("path", val);
                        obj3.Add(obj4);
                        if (!File.Exists(path) || CoverOriginalFile)
                        {
                            saveLayaLmatData(material, path);
                        }
                    }
                    else
                    {
                        JSONObject obj5 = new JSONObject(JSONObject.Type.OBJECT);
                        obj5.AddField("type", "Laya.StandardMaterial");
                        obj5.AddField("path", val);
                        obj3.Add(obj5);
                        if (!File.Exists(path) || CoverOriginalFile)
                        {
                            saveLmatFile(material, path, ComponentType.MeshRenderer);
                        }
                    }
                }
            }
        }

        public static void getParticleSystemComponentData(GameObject gameObject, JSONObject customProps)
        {
            JSONObject obj2;
            JSONObject obj6;
            int num;
            ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = gameObject.GetComponent<ParticleSystemRenderer>();
            int val = 0;
            customProps.AddField("isPerformanceMode", true);
            customProps.AddField("duration", component.get_main().get_duration());
            customProps.AddField("looping", component.get_main().get_loop());
            customProps.AddField("prewarm", false);
            if (component.get_main().get_startDelay().get_mode().ToString() == "Constant")
            {
                val = 0;
            }
            else if (component.get_main().get_startDelay().get_mode().ToString() == "TwoConstants")
            {
                val = 1;
            }
            customProps.AddField("startDelayType", val);
            customProps.AddField("startDelay", component.get_main().get_startDelay().get_constant());
            customProps.AddField("startDelayMin", component.get_main().get_startDelay().get_constantMin());
            customProps.AddField("startDelayMax", component.get_main().get_startDelay().get_constantMax());
            if (component.get_main().get_startLifetime().get_mode().ToString() == "Constant")
            {
                val = 0;
            }
            else if (component.get_main().get_startLifetime().get_mode().ToString() == "Curves")
            {
                val = 1;
            }
            else if (component.get_main().get_startLifetime().get_mode().ToString() == "TwoConstants")
            {
                val = 2;
            }
            else if (component.get_main().get_startLifetime().get_mode().ToString() == "MinMaxCurves")
            {
                val = 3;
            }
            customProps.AddField("startLifetimeType", val);
            customProps.AddField("startLifetimeConstant", component.get_main().get_startLifetime().get_constant());
            customProps.AddField("startLifetimeConstantMin", component.get_main().get_startLifetime().get_constantMin());
            customProps.AddField("startLifetimeConstantMax", component.get_main().get_startLifetime().get_constantMax());
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("startLifetimeGradient", obj5);
            JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
            if (component.get_main().get_startLifetime().get_curve() != null)
            {
                for (num = 0; num < component.get_main().get_startLifetime().get_curve().get_length(); num++)
                {
                    obj4.Clear();
                    obj3.Add(obj4);
                    obj4.AddField("key", component.get_main().get_startLifetime().get_curve().get_Item(num).get_time());
                    obj4.AddField("value", component.get_main().get_startLifetime().get_curve().get_Item(num).get_value());
                }
            }
            obj5.AddField("startLifetimes", obj3);
            obj5 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("startLifetimeGradientMin", obj5);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj4 = new JSONObject(JSONObject.Type.OBJECT);
            if (component.get_main().get_startLifetime().get_curveMin() != null)
            {
                for (num = 0; num < component.get_main().get_startLifetime().get_curveMin().get_length(); num++)
                {
                    obj4.Clear();
                    obj3.Add(obj4);
                    obj4.AddField("key", component.get_main().get_startLifetime().get_curveMin().get_Item(num).get_time());
                    obj4.AddField("value", component.get_main().get_startLifetime().get_curveMin().get_Item(num).get_value());
                }
            }
            obj5.AddField("startLifetimes", obj3);
            obj5 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("startLifetimeGradientMax", obj5);
            if (component.get_main().get_startLifetime().get_curveMax() != null)
            {
                for (num = 0; num < component.get_main().get_startLifetime().get_curveMax().get_length(); num++)
                {
                    obj4.Clear();
                    obj3.Add(obj4);
                    obj4.AddField("key", component.get_main().get_startLifetime().get_curveMax().get_Item(num).get_time());
                    obj4.AddField("value", component.get_main().get_startLifetime().get_curveMax().get_Item(num).get_value());
                }
            }
            obj5.AddField("startLifetimes", obj3);
            if (component.get_main().get_startSpeed().get_mode().ToString() == "Constant")
            {
                val = 0;
            }
            else if (component.get_main().get_startSpeed().get_mode().ToString() == "Curve")
            {
                val = 1;
            }
            else if (component.get_main().get_startSpeed().get_mode().ToString() == "TwoConstants")
            {
                val = 2;
            }
            else if (component.get_main().get_startSpeed().get_mode().ToString() == "TwoCurves")
            {
                val = 3;
            }
            customProps.AddField("startSpeedType", val);
            customProps.AddField("startSpeedConstant", component.get_main().get_startSpeed().get_constant());
            customProps.AddField("startSpeedConstantMin", component.get_main().get_startSpeed().get_constantMin());
            customProps.AddField("startSpeedConstantMax", component.get_main().get_startSpeed().get_constantMax());
            if (component.get_main().get_startSizeX().get_mode().ToString() == "Constant")
            {
                val = 0;
            }
            else if (component.get_main().get_startSizeX().get_mode().ToString() == "Curve")
            {
                val = 1;
            }
            else if (component.get_main().get_startSizeX().get_mode().ToString() == "TwoConstants")
            {
                val = 2;
            }
            else if (component.get_main().get_startSizeX().get_mode().ToString() == "TwoCurves")
            {
                val = 3;
            }
            customProps.AddField("threeDStartSize", component.get_main().get_startSize3D());
            customProps.AddField("startSizeType", val);
            customProps.AddField("startSizeConstant", component.get_main().get_startSize().get_constant());
            customProps.AddField("startSizeConstantMin", component.get_main().get_startSize().get_constantMin());
            customProps.AddField("startSizeConstantMax", component.get_main().get_startSize().get_constantMax());
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(component.get_main().get_startSizeX().get_constant());
            obj3.Add(component.get_main().get_startSizeY().get_constant());
            obj3.Add(component.get_main().get_startSizeZ().get_constant());
            customProps.AddField("startSizeConstantSeparate", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(component.get_main().get_startSizeX().get_constantMin());
            obj3.Add(component.get_main().get_startSizeY().get_constantMin());
            obj3.Add(component.get_main().get_startSizeZ().get_constantMin());
            customProps.AddField("startSizeConstantMinSeparate", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(component.get_main().get_startSizeX().get_constantMax());
            obj3.Add(component.get_main().get_startSizeY().get_constantMax());
            obj3.Add(component.get_main().get_startSizeZ().get_constantMax());
            customProps.AddField("startSizeConstantMaxSeparate", obj3);
            customProps.AddField("threeDStartRotation", component.get_main().get_startRotation3D());
            if (component.get_main().get_startRotationX().get_mode().ToString() == "Constant")
            {
                val = 0;
            }
            else if (component.get_main().get_startRotationX().get_mode().ToString() == "Curve")
            {
                val = 1;
            }
            else if (component.get_main().get_startRotationX().get_mode().ToString() == "TwoConstants")
            {
                val = 2;
            }
            else if (component.get_main().get_startRotationX().get_mode().ToString() == "TwoCurves")
            {
                val = 3;
            }
            customProps.AddField("startRotationType", val);
            customProps.AddField("startRotationConstant", (float) ((component.get_main().get_startRotation().get_constant() * 180f) / 3.141593f));
            customProps.AddField("startRotationConstantMin", (float) ((component.get_main().get_startRotation().get_constantMin() * 180f) / 3.141593f));
            customProps.AddField("startRotationConstantMax", (float) ((component.get_main().get_startRotation().get_constantMax() * 180f) / 3.141593f));
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add((float) ((component.get_main().get_startRotationX().get_constant() * 180f) / 3.141593f));
            obj3.Add((float) (-1f * ((component.get_main().get_startRotationY().get_constant() * 180f) / 3.141593f)));
            obj3.Add((float) (-1f * ((component.get_main().get_startRotationZ().get_constant() * 180f) / 3.141593f)));
            customProps.AddField("startRotationConstantSeparate", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add((float) ((component.get_main().get_startRotationX().get_constantMin() * 180f) / 3.141593f));
            obj3.Add((float) (-1f * ((component.get_main().get_startRotationY().get_constantMin() * 180f) / 3.141593f)));
            obj3.Add((float) (-1f * ((component.get_main().get_startRotationZ().get_constantMin() * 180f) / 3.141593f)));
            customProps.AddField("startRotationConstantMinSeparate", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add((float) ((component.get_main().get_startRotationX().get_constantMax() * 180f) / 3.141593f));
            obj3.Add((float) (-1f * ((component.get_main().get_startRotationY().get_constantMax() * 180f) / 3.141593f)));
            obj3.Add((float) (-1f * ((component.get_main().get_startRotationZ().get_constantMax() * 180f) / 3.141593f)));
            customProps.AddField("startRotationConstantMaxSeparate", obj3);
            customProps.AddField("randomizeRotationDirection", (float) ((component.get_main().get_flipRotation() * 180f) / 3.141593f));
            if (component.get_main().get_startColor().get_mode().ToString() == "Color")
            {
                val = 0;
            }
            else if (component.get_main().get_startColor().get_mode().ToString() == "Gradient")
            {
                val = 1;
            }
            else if (component.get_main().get_startColor().get_mode().ToString() == "TwoColors")
            {
                val = 2;
            }
            else if (component.get_main().get_startColor().get_mode().ToString() == "TwoGradients")
            {
                val = 3;
            }
            else if (component.get_main().get_startColor().get_mode().ToString() == "RandomColor")
            {
                val = 4;
            }
            customProps.AddField("startColorType", val);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(component.get_main().get_startColor().get_color().r);
            obj3.Add(component.get_main().get_startColor().get_color().g);
            obj3.Add(component.get_main().get_startColor().get_color().b);
            obj3.Add(component.get_main().get_startColor().get_color().a);
            customProps.AddField("startColorConstant", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(component.get_main().get_startColor().get_colorMin().r);
            obj3.Add(component.get_main().get_startColor().get_colorMin().g);
            obj3.Add(component.get_main().get_startColor().get_colorMin().b);
            obj3.Add(component.get_main().get_startColor().get_colorMin().a);
            customProps.AddField("startColorConstantMin", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(component.get_main().get_startColor().get_colorMax().r);
            obj3.Add(component.get_main().get_startColor().get_colorMax().g);
            obj3.Add(component.get_main().get_startColor().get_colorMax().b);
            obj3.Add(component.get_main().get_startColor().get_colorMax().a);
            customProps.AddField("startColorConstantMax", obj3);
            obj3 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.Add(Physics.get_gravity().x);
            obj3.Add(Physics.get_gravity().y);
            obj3.Add(Physics.get_gravity().z);
            customProps.AddField("gravity", obj3);
            customProps.AddField("gravityModifier", component.get_main().get_gravityModifier().get_constant());
            if (component.get_main().get_simulationSpace().ToString() == "World")
            {
                val = 0;
            }
            else if (component.get_main().get_simulationSpace().ToString() == "Local")
            {
                val = 1;
            }
            customProps.AddField("simulationSpace", val);
            if (component.get_main().get_scalingMode().ToString() == "Hierarchy")
            {
                val = 0;
            }
            else if (component.get_main().get_scalingMode().ToString() == "Local")
            {
                val = 1;
            }
            else if (component.get_main().get_scalingMode().ToString() == "Shape")
            {
                val = 2;
            }
            customProps.AddField("scaleMode", val);
            customProps.AddField("playOnAwake", component.get_main().get_playOnAwake());
            customProps.AddField("maxParticles", component.get_main().get_maxParticles());
            customProps.AddField("autoRandomSeed", component.get_useAutoRandomSeed());
            customProps.AddField("randomSeed", (long) component.get_randomSeed());
            if (component.get_emission().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("emission", obj2);
                obj2.AddField("enable", component.get_emission().get_enabled());
                if (component.get_emission().get_rateOverTime().get_mode().ToString() == "Constant")
                {
                    val = 0;
                }
                else if (component.get_emission().get_rateOverTime().get_mode().ToString() == "Curve")
                {
                    val = 1;
                }
                else if (component.get_emission().get_rateOverTime().get_mode().ToString() == "TwoConstants")
                {
                    val = 2;
                }
                else if (component.get_emission().get_rateOverTime().get_mode().ToString() == "TwoCurves")
                {
                    val = 3;
                }
                obj2.AddField("emissionRate", component.get_emission().get_rateOverTime().get_constant());
                obj2.AddField("emissionRateTip", "Time");
                obj3 = new JSONObject(JSONObject.Type.ARRAY);
                ParticleSystem.Burst[] burstArray = new ParticleSystem.Burst[component.get_emission().get_burstCount()];
                component.get_emission().GetBursts(burstArray);
                for (num = 0; num < burstArray.Length; num++)
                {
                    obj4 = new JSONObject(JSONObject.Type.OBJECT);
                    obj4.AddField("time", burstArray[num].get_time());
                    obj4.AddField("min", (int) burstArray[num].get_minCount());
                    obj4.AddField("max", (int) burstArray[num].get_maxCount());
                    obj3.Add(obj4);
                }
                obj2.AddField("bursts", obj3);
            }
            if (component.get_shape().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("shape", obj2);
                obj2.AddField("enable", component.get_shape().get_enabled());
                if ((component.get_shape().get_shapeType().ToString() == "Sphere") || (component.get_shape().get_shapeType().ToString() == "SphereShell"))
                {
                    val = 0;
                }
                else if ((component.get_shape().get_shapeType().ToString() == "Hemisphere") || (component.get_shape().get_shapeType().ToString() == "HemisphereShell"))
                {
                    val = 1;
                }
                else if ((((component.get_shape().get_shapeType().ToString() == "Cone") || (component.get_shape().get_shapeType().ToString() == "ConeShell")) || ((component.get_shape().get_shapeType().ToString() == "ConeBase") || (component.get_shape().get_shapeType().ToString() == "ConeBaseShell"))) || ((component.get_shape().get_shapeType().ToString() == "ConeVolume") || (component.get_shape().get_shapeType().ToString() == "ConeVolumeShell")))
                {
                    val = 2;
                }
                else if (component.get_shape().get_shapeType().ToString() == "Box")
                {
                    val = 3;
                }
                else if (component.get_shape().get_shapeType().ToString() == "Mesh")
                {
                    val = 4;
                }
                else if (component.get_shape().get_shapeType().ToString() == "MeshRenderer")
                {
                    val = 5;
                }
                else if (component.get_shape().get_shapeType().ToString() == "SkinnedMeshRenderer")
                {
                    val = 6;
                }
                else if ((component.get_shape().get_shapeType().ToString() == "Circle") || (component.get_shape().get_shapeType().ToString() == "CircleEdge"))
                {
                    val = 7;
                }
                else if (component.get_shape().get_shapeType().ToString() == "SingleSidedEdge")
                {
                    val = 8;
                }
                obj2.AddField("shapeType", val);
                obj2.AddField("sphereRadius", component.get_shape().get_radius());
                if (component.get_shape().get_shapeType().ToString() == "SphereShell")
                {
                    obj2.AddField("sphereEmitFromShell", true);
                }
                else
                {
                    obj2.AddField("sphereEmitFromShell", false);
                }
                obj2.AddField("sphereRandomDirection", component.get_shape().get_randomDirectionAmount());
                obj2.AddField("hemiSphereRadius", component.get_shape().get_radius());
                if (component.get_shape().get_shapeType().ToString() == "HemisphereShell")
                {
                    obj2.AddField("hemiSphereEmitFromShell", true);
                }
                else
                {
                    obj2.AddField("hemiSphereEmitFromShell", false);
                }
                obj2.AddField("hemiSphereRandomDirection", component.get_shape().get_randomDirectionAmount());
                obj2.AddField("coneAngle", component.get_shape().get_angle());
                obj2.AddField("coneRadius", component.get_shape().get_radius());
                obj2.AddField("coneLength", component.get_shape().get_length());
                if (component.get_shape().get_shapeType().ToString() == "Cone")
                {
                    val = 0;
                }
                else if (component.get_shape().get_shapeType().ToString() == "ConeShell")
                {
                    val = 1;
                }
                else if (component.get_shape().get_shapeType().ToString() == "ConeVolume")
                {
                    val = 2;
                }
                else if (component.get_shape().get_shapeType().ToString() == "ConeVolumeShell")
                {
                    val = 3;
                }
                obj2.AddField("coneEmitType", val);
                obj2.AddField("coneRandomDirection", component.get_shape().get_randomDirectionAmount());
                obj2.AddField("boxX", component.get_shape().get_scale().x);
                obj2.AddField("boxY", component.get_shape().get_scale().y);
                obj2.AddField("boxZ", component.get_shape().get_scale().z);
                obj2.AddField("boxRandomDirection", component.get_shape().get_randomDirectionAmount());
                obj2.AddField("circleRadius", component.get_shape().get_radius());
                obj2.AddField("circleArc", component.get_shape().get_arc());
                if (component.get_shape().get_shapeType().ToString() == "CircleEdge")
                {
                    obj2.AddField("circleEmitFromEdge", true);
                }
                else
                {
                    obj2.AddField("circleEmitFromEdge", false);
                }
                obj2.AddField("circleRandomDirection", component.get_shape().get_randomDirectionAmount());
            }
            if (component.get_velocityOverLifetime().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("velocityOverLifetime", obj2);
                obj2.AddField("enable", component.get_velocityOverLifetime().get_enabled());
                if (component.get_velocityOverLifetime().get_space().ToString() == "Local")
                {
                    val = 0;
                }
                else if (component.get_velocityOverLifetime().get_space().ToString() == "World")
                {
                    val = 1;
                }
                obj2.AddField("space", val);
                obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("velocity", obj5);
                if (component.get_velocityOverLifetime().get_x().get_mode().ToString() == "Constant")
                {
                    val = 0;
                }
                else if (component.get_velocityOverLifetime().get_x().get_mode().ToString() == "Curve")
                {
                    val = 1;
                }
                else if (component.get_velocityOverLifetime().get_x().get_mode().ToString() == "TwoConstants")
                {
                    val = 2;
                }
                else if (component.get_velocityOverLifetime().get_x().get_mode().ToString() == "TwoCurves")
                {
                    val = 3;
                }
                obj5.AddField("type", val);
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add((float) (component.get_velocityOverLifetime().get_x().get_constant() * -1f));
                obj6.Add(component.get_velocityOverLifetime().get_y().get_constant());
                obj6.Add(component.get_velocityOverLifetime().get_z().get_constant());
                obj5.AddField("constant", obj6);
                saveParticleFrameData(component.get_velocityOverLifetime().get_x(), obj5, "gradientX", "velocitys", 0, component.get_velocityOverLifetime().get_x().get_curveMultiplier(), -1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_y(), obj5, "gradientY", "velocitys", 0, component.get_velocityOverLifetime().get_y().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_z(), obj5, "gradientZ", "velocitys", 0, component.get_velocityOverLifetime().get_z().get_curveMultiplier(), 1f);
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add((float) (component.get_velocityOverLifetime().get_x().get_constantMin() * -1f));
                obj6.Add(component.get_velocityOverLifetime().get_y().get_constantMin());
                obj6.Add(component.get_velocityOverLifetime().get_z().get_constantMin());
                obj5.AddField("constantMin", obj6);
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add((float) (component.get_velocityOverLifetime().get_x().get_constantMax() * -1f));
                obj6.Add(component.get_velocityOverLifetime().get_y().get_constantMax());
                obj6.Add(component.get_velocityOverLifetime().get_z().get_constantMax());
                obj5.AddField("constantMax", obj6);
                saveParticleFrameData(component.get_velocityOverLifetime().get_x(), obj5, "gradientXMin", "velocitys", -1, component.get_velocityOverLifetime().get_x().get_curveMultiplier(), -1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_x(), obj5, "gradientXMax", "velocitys", 1, component.get_velocityOverLifetime().get_x().get_curveMultiplier(), -1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_y(), obj5, "gradientYMin", "velocitys", -1, component.get_velocityOverLifetime().get_y().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_y(), obj5, "gradientYMax", "velocitys", 1, component.get_velocityOverLifetime().get_y().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_z(), obj5, "gradientZMin", "velocitys", -1, component.get_velocityOverLifetime().get_z().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_velocityOverLifetime().get_z(), obj5, "gradientZMax", "velocitys", 1, component.get_velocityOverLifetime().get_z().get_curveMultiplier(), 1f);
            }
            if (component.get_colorOverLifetime().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("colorOverLifetime", obj2);
                obj2.AddField("enable", component.get_colorOverLifetime().get_enabled());
                obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("color", obj5);
                if (component.get_colorOverLifetime().get_color().get_mode().ToString() == "Gradient")
                {
                    val = 1;
                }
                else if (component.get_colorOverLifetime().get_color().get_mode().ToString() == "TwoGradients")
                {
                    val = 3;
                }
                obj5.AddField("type", val);
                obj3 = new JSONObject(JSONObject.Type.ARRAY);
                obj3.Add(component.get_colorOverLifetime().get_color().get_color().r);
                obj3.Add(component.get_colorOverLifetime().get_color().get_color().g);
                obj3.Add(component.get_colorOverLifetime().get_color().get_color().b);
                obj3.Add(component.get_colorOverLifetime().get_color().get_color().a);
                obj5.AddField("constant", obj3);
                saveParticleFrameData(component.get_colorOverLifetime().get_color().get_gradient(), obj5, "gradient");
                obj3 = new JSONObject(JSONObject.Type.ARRAY);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMin().r);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMin().g);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMin().b);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMin().a);
                obj5.AddField("constantMin", obj3);
                obj3 = new JSONObject(JSONObject.Type.ARRAY);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMax().r);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMax().g);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMax().b);
                obj3.Add(component.get_colorOverLifetime().get_color().get_colorMax().a);
                obj5.AddField("constantMax", obj3);
                saveParticleFrameData(component.get_colorOverLifetime().get_color().get_gradientMin(), obj5, "gradientMin");
                saveParticleFrameData(component.get_colorOverLifetime().get_color().get_gradientMax(), obj5, "gradientMax");
            }
            if (component.get_sizeOverLifetime().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("sizeOverLifetime", obj2);
                obj2.AddField("enable", component.get_sizeOverLifetime().get_enabled());
                obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("size", obj5);
                if (component.get_sizeOverLifetime().get_size().get_mode().ToString() == "Curve")
                {
                    val = 0;
                }
                else if (component.get_sizeOverLifetime().get_size().get_mode().ToString() == "TwoConstants")
                {
                    val = 1;
                }
                else if (component.get_sizeOverLifetime().get_size().get_mode().ToString() == "TwoCurves")
                {
                    val = 2;
                }
                obj5.AddField("type", val);
                obj5.AddField("separateAxes", component.get_sizeOverLifetime().get_separateAxes());
                saveParticleFrameData(component.get_sizeOverLifetime().get_size(), obj5, "gradient", "sizes", 0, component.get_sizeOverLifetime().get_size().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_x(), obj5, "gradientX", "sizes", 0, component.get_sizeOverLifetime().get_x().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_y(), obj5, "gradientY", "sizes", 0, component.get_sizeOverLifetime().get_y().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_z(), obj5, "gradientZ", "sizes", 0, component.get_sizeOverLifetime().get_z().get_curveMultiplier(), 1f);
                obj5.AddField("constantMin", component.get_sizeOverLifetime().get_size().get_constantMin());
                obj5.AddField("constantMax", component.get_sizeOverLifetime().get_size().get_constantMax());
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add(component.get_sizeOverLifetime().get_x().get_constantMin());
                obj6.Add(component.get_sizeOverLifetime().get_y().get_constantMin());
                obj6.Add(component.get_sizeOverLifetime().get_z().get_constantMin());
                obj5.AddField("constantMinSeparate", obj6);
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add(component.get_sizeOverLifetime().get_x().get_constantMax());
                obj6.Add(component.get_sizeOverLifetime().get_y().get_constantMax());
                obj6.Add(component.get_sizeOverLifetime().get_z().get_constantMax());
                obj5.AddField("constantMaxSeparate", obj6);
                saveParticleFrameData(component.get_sizeOverLifetime().get_size(), obj5, "gradientMin", "sizes", -1, component.get_sizeOverLifetime().get_size().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_size(), obj5, "gradientMax", "sizes", 1, component.get_sizeOverLifetime().get_size().get_curveMultiplier(), 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_x(), obj5, "gradientXMin", "sizes", -1, 1f, 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_x(), obj5, "gradientXMax", "sizes", 1, 1f, 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_y(), obj5, "gradientYMin", "sizes", -1, 1f, 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_y(), obj5, "gradientYMax", "sizes", 1, 1f, 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_z(), obj5, "gradientZMin", "sizes", -1, 1f, 1f);
                saveParticleFrameData(component.get_sizeOverLifetime().get_z(), obj5, "gradientZMax", "sizes", 1, 1f, 1f);
            }
            if (component.get_rotationOverLifetime().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("rotationOverLifetime", obj2);
                obj2.AddField("enable", component.get_rotationOverLifetime().get_enabled());
                obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("angularVelocity", obj5);
                if (component.get_rotationOverLifetime().get_z().get_mode().ToString() == "Constant")
                {
                    val = 0;
                }
                else if (component.get_rotationOverLifetime().get_z().get_mode().ToString() == "Curve")
                {
                    val = 1;
                }
                else if (component.get_rotationOverLifetime().get_z().get_mode().ToString() == "TwoConstants")
                {
                    val = 2;
                }
                else if (component.get_rotationOverLifetime().get_z().get_mode().ToString() == "TwoCurves")
                {
                    val = 3;
                }
                obj5.AddField("type", val);
                obj5.AddField("separateAxes", component.get_rotationOverLifetime().get_separateAxes());
                obj5.AddField("constant", (float) ((component.get_rotationOverLifetime().get_z().get_constant() * 180f) / 3.141593f));
                obj5.AddField("constantMin", (float) ((component.get_rotationOverLifetime().get_z().get_constantMin() * 180f) / 3.141593f));
                obj5.AddField("constantMax", (float) ((component.get_rotationOverLifetime().get_z().get_constantMax() * 180f) / 3.141593f));
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add((float) ((component.get_rotationOverLifetime().get_x().get_constantMin() * 180f) / 3.141593f));
                obj6.Add((float) ((component.get_rotationOverLifetime().get_y().get_constantMin() * 180f) / 3.141593f));
                obj6.Add((float) ((component.get_rotationOverLifetime().get_z().get_constantMin() * 180f) / 3.141593f));
                obj5.AddField("constantMinSeparate", obj6);
                obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add((float) ((component.get_rotationOverLifetime().get_x().get_constantMax() * 180f) / 3.141593f));
                obj6.Add((float) ((component.get_rotationOverLifetime().get_y().get_constantMax() * 180f) / 3.141593f));
                obj6.Add((float) ((component.get_rotationOverLifetime().get_z().get_constantMax() * 180f) / 3.141593f));
                obj5.AddField("constantMaxSeparate", obj6);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradient", "angularVelocitys", 0, (component.get_rotationOverLifetime().get_z().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientX", "angularVelocitys", 0, (component.get_rotationOverLifetime().get_x().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientY", "angularVelocitys", 0, (component.get_rotationOverLifetime().get_y().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientZ", "angularVelocitys", 0, (component.get_rotationOverLifetime().get_z().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientMin", "angularVelocitys", -1, (component.get_rotationOverLifetime().get_z().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientMax", "angularVelocitys", 1, (component.get_rotationOverLifetime().get_z().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientXMin", "angularVelocitys", -1, (component.get_rotationOverLifetime().get_x().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientXMax", "angularVelocitys", 1, (component.get_rotationOverLifetime().get_x().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientYMin", "angularVelocitys", -1, (component.get_rotationOverLifetime().get_y().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientYMax", "angularVelocitys", 1, (component.get_rotationOverLifetime().get_y().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientZMin", "angularVelocitys", -1, (component.get_rotationOverLifetime().get_z().get_curveMultiplier() * 180f) / 3.141593f, 1f);
                saveParticleFrameData(component.get_rotationOverLifetime().get_z(), obj5, "gradientZMax", "angularVelocitys", 1, (component.get_rotationOverLifetime().get_z().get_curveMultiplier() * 180f) / 3.141593f, 1f);
            }
            if (component.get_textureSheetAnimation().get_enabled())
            {
                obj2 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("textureSheetAnimation", obj2);
                obj2.AddField("enable", component.get_textureSheetAnimation().get_enabled());
                obj3 = new JSONObject(JSONObject.Type.ARRAY);
                obj2.AddField("tiles", obj3);
                obj3.Add(component.get_textureSheetAnimation().get_numTilesX());
                obj3.Add(component.get_textureSheetAnimation().get_numTilesY());
                float num4 = 0f;
                ParticleSystemAnimationType type2 = component.get_textureSheetAnimation().get_animation();
                if (type2 != null)
                {
                    if (type2 != 1)
                    {
                        Debug.LogWarning("unknown type.");
                    }
                    else
                    {
                        val = 1;
                        num4 = component.get_textureSheetAnimation().get_numTilesX();
                    }
                }
                else
                {
                    val = 0;
                    num4 = component.get_textureSheetAnimation().get_numTilesX() * component.get_textureSheetAnimation().get_numTilesY();
                }
                float curveMultiplier = num4 * component.get_textureSheetAnimation().get_frameOverTime().get_curveMultiplier();
                obj2.AddField("type", val);
                obj2.AddField("randomRow", component.get_textureSheetAnimation().get_useRandomRow());
                obj2.AddField("rowIndex", component.get_textureSheetAnimation().get_rowIndex());
                obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("frame", obj5);
                if (component.get_textureSheetAnimation().get_frameOverTime().get_mode().ToString() == "Constant")
                {
                    val = 0;
                }
                else if (component.get_textureSheetAnimation().get_frameOverTime().get_mode().ToString() == "Curve")
                {
                    val = 1;
                }
                else if (component.get_textureSheetAnimation().get_frameOverTime().get_mode().ToString() == "TwoConstants")
                {
                    val = 2;
                }
                else if (component.get_textureSheetAnimation().get_frameOverTime().get_mode().ToString() == "TwoCurves")
                {
                    val = 3;
                }
                obj5.AddField("type", val);
                obj5.AddField("constant", (float) (component.get_textureSheetAnimation().get_frameOverTime().get_constant() * num4));
                saveParticleFrameData(component.get_textureSheetAnimation().get_frameOverTime(), obj5, "overTime", "frames", 0, curveMultiplier, 1f);
                obj5.AddField("constantMin", (float) (component.get_textureSheetAnimation().get_frameOverTime().get_constantMin() * num4));
                obj5.AddField("constantMax", (float) (component.get_textureSheetAnimation().get_frameOverTime().get_constantMax() * num4));
                saveParticleFrameData(component.get_textureSheetAnimation().get_frameOverTime(), obj5, "overTimeMin", "frames", -1, curveMultiplier, 1f);
                saveParticleFrameData(component.get_textureSheetAnimation().get_frameOverTime(), obj5, "overTimeMax", "frames", 1, curveMultiplier, 1f);
                obj5 = new JSONObject(JSONObject.Type.OBJECT);
                obj2.AddField("startFrame", obj5);
                obj5.AddField("type", 0);
                obj5.AddField("constant", (float) (component.get_textureSheetAnimation().get_startFrame().get_constant() * ((component.get_textureSheetAnimation().get_numTilesX() * component.get_textureSheetAnimation().get_numTilesY()) - 1)));
                obj5.AddField("constantMin", (float) (component.get_textureSheetAnimation().get_startFrame().get_constantMin() * ((component.get_textureSheetAnimation().get_numTilesX() * component.get_textureSheetAnimation().get_numTilesY()) - 1)));
                obj5.AddField("constantMax", (float) (component.get_textureSheetAnimation().get_startFrame().get_constantMax() * ((component.get_textureSheetAnimation().get_numTilesX() * component.get_textureSheetAnimation().get_numTilesY()) - 1)));
                obj2.AddField("cycles", component.get_textureSheetAnimation().get_cycleCount());
                if (component.get_textureSheetAnimation().get_enabled())
                {
                    obj2.AddField("enableUVChannels", 1);
                }
                else
                {
                    obj2.AddField("enableUVChannels", 0);
                }
                obj2.AddField("enableUVChannelsTip", component.get_textureSheetAnimation().get_uvChannelMask().ToString());
            }
            int num3 = 0;
            if (renderer.get_renderMode().ToString() == "Billboard")
            {
                num3 = 0;
            }
            else if (renderer.get_renderMode().ToString() == "Stretch")
            {
                num3 = 1;
            }
            else if (renderer.get_renderMode().ToString() == "HorizontalBillboard")
            {
                num3 = 2;
            }
            else if (renderer.get_renderMode().ToString() == "VerticalBillboard")
            {
                num3 = 3;
            }
            else if (renderer.get_renderMode().ToString() == "Mesh")
            {
                num3 = 4;
            }
            customProps.AddField("renderMode", num3);
            customProps.AddField("stretchedBillboardCameraSpeedScale", renderer.get_cameraVelocityScale());
            customProps.AddField("stretchedBillboardSpeedScale", renderer.get_velocityScale());
            customProps.AddField("stretchedBillboardLengthScale", renderer.get_lengthScale());
            customProps.AddField("sortingFudge", renderer.get_sortingFudge());
            Material material = gameObject.GetComponent<Renderer>().get_sharedMaterial();
            JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
            if (material != null)
            {
                char[] separator = new char[] { '.' };
                string str = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".lmat";
                string path = SAVEPATH + "/" + str;
                JSONObject obj8 = new JSONObject(JSONObject.Type.OBJECT);
                obj8.AddField("type", "Laya.ShurikenParticleMaterial");
                obj8.AddField("path", str);
                obj7.Add(obj8);
                customProps.AddField("material", obj8);
                if (!File.Exists(path) || CoverOriginalFile)
                {
                    if (material.get_shader().get_name() == "LayaAir3D/ShurikenParticle")
                    {
                        saveLayaParticleLmatData(material, path);
                    }
                    else
                    {
                        saveLmatFile(material, path, ComponentType.ParticleSystem);
                    }
                }
            }
            if (num3 == 4)
            {
                Mesh mesh = gameObject.GetComponent<ParticleSystemRenderer>().get_mesh();
                if (mesh != null)
                {
                    string str3 = cleanIllegalChar(mesh.get_name(), true);
                    char[] chArray2 = new char[] { '.' };
                    string str4 = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(mesh.GetInstanceID()).Split(chArray2)[0], false) + "-" + str3 + ".lm";
                    string str5 = SAVEPATH + "/" + str4;
                    customProps.AddField("meshPath", str4);
                    if (!File.Exists(str5) || CoverOriginalFile)
                    {
                        saveLmFile(mesh, str5);
                    }
                }
                else
                {
                    customProps.AddField("meshPath", "");
                    Debug.LogWarning("LayaUnityPlugin : " + gameObject.get_name() + "'s Render Mesh can't be null!");
                }
            }
        }

        public static void getRigidbodyComponentData(GameObject gameObject, JSONObject component)
        {
            gameObject.GetComponent<Rigidbody>();
            JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
            component.AddField("Rigidbody", obj2);
        }

        public static JSONObject getSceneNode()
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
            if (Type == 0)
            {
                obj2.AddField("type", "Scene");
            }
            else if (Type == 1)
            {
                obj2.AddField("type", "Sprite3D");
            }
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            obj3.AddField("name", DataManager.sceneName);
            obj2.AddField("props", obj3);
            JSONObject customProps = new JSONObject(JSONObject.Type.OBJECT);
            if (Type == 0)
            {
                JSONObject obj5 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("skyBox", obj5);
                Material material = RenderSettings.get_skybox();
                if (material != null)
                {
                    char[] separator = new char[] { '.' };
                    string val = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".lmat";
                    string path = SAVEPATH + "/" + val;
                    if (material.get_shader().get_name() == "Skybox/6 Sided")
                    {
                        obj5.AddField("ltcPath", val);
                        if (!File.Exists(path) || CoverOriginalFile)
                        {
                            saveLayaSkyBoxData(material, path);
                        }
                    }
                }
                saveLightMapFile(customProps);
                Color color = RenderSettings.get_ambientLight();
                JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
                obj6.Add(color.r);
                obj6.Add(color.g);
                obj6.Add(color.b);
                customProps.AddField("ambientColor", obj6);
                obj3.AddField("enableFog", RenderSettings.get_fog());
                JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
                Color color2 = RenderSettings.get_fogColor();
                obj7.Add(color2.r);
                obj7.Add(color2.g);
                obj7.Add(color2.b);
                customProps.AddField("fogColor", obj7);
                obj3.AddField("fogStart", RenderSettings.get_fogStartDistance());
                obj3.AddField("fogRange", (float) (RenderSettings.get_fogEndDistance() - RenderSettings.get_fogStartDistance()));
            }
            else if (Type == 1)
            {
                Vector3 vector = new Vector3(0f, 0f, 0f);
                Quaternion quaternion = new Quaternion(0f, 0f, 0f, -1f);
                Vector3 vector2 = new Vector3(1f, 1f, 1f);
                JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
                obj8.Add(vector.x);
                obj8.Add(vector.y);
                obj8.Add(vector.z);
                customProps.AddField("translate", obj8);
                JSONObject obj9 = new JSONObject(JSONObject.Type.ARRAY);
                obj9.Add(quaternion.x);
                obj9.Add(quaternion.y);
                obj9.Add(quaternion.z);
                obj9.Add(quaternion.w);
                customProps.AddField("rotation", obj9);
                JSONObject obj10 = new JSONObject(JSONObject.Type.ARRAY);
                obj10.Add(vector2.x);
                obj10.Add(vector2.y);
                obj10.Add(vector2.z);
                customProps.AddField("scale", obj10);
            }
            obj2.AddField("customProps", customProps);
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            if (rootGameObjects.Length != 0)
            {
                Dictionary<GameObject, string> dictionary;
                JSONObject obj11 = new JSONObject(JSONObject.Type.ARRAY);
                obj2.AddField("child", obj11);
                string sceneName = DataManager.sceneName;
                dictionary = new Dictionary<GameObject, string> {
                    dictionary
                };
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    getGameObjectData(rootGameObjects[i].get_gameObject(), sceneName, obj11, false);
                }
                return obj2;
            }
            obj2.AddField("child", new JSONObject(JSONObject.Type.ARRAY));
            return obj2;
        }

        public static void getSimpleGameObjectData(GameObject gameObject, string gameObjectPath, JSONObject parentsChildNodes)
        {
            List<ComponentType> list = componentsOnGameObject(gameObject);
            Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject child = new JSONObject(JSONObject.Type.ARRAY);
                GameObject obj4 = componentsInChildren[i].get_gameObject();
                List<ComponentType> list2 = componentsOnGameObject(obj4);
                checkChildIsLegal(obj4, true);
                if (i == (componentsInChildren.Length - 1))
                {
                    new List<string>();
                }
                if ((((obj4 != gameObject) && (obj4.get_activeInHierarchy() || !IgnoreNotActiveGameObject)) && ((selectParentbyType(obj4, ComponentType.Animator) == null) && ((list.IndexOf(ComponentType.DirectionalLight) == -1) || (directionalLightCurCount < directionalLightTotalCount)))) && ((!IgnoreNullGameObject || (list2.Count > 1)) || curNodeHasLegalChild))
                {
                    Matrix4x4 matrixx = gameObject.get_transform().get_worldToLocalMatrix() * obj4.get_transform().get_localToWorldMatrix();
                    Vector3 column = matrixx.GetColumn(3);
                    Vector4 introduced13 = matrixx.GetColumn(2);
                    Quaternion rotation = Quaternion.LookRotation(introduced13, matrixx.GetColumn(1));
                    float introduced14 = matrixx.GetColumn(0).get_magnitude();
                    float introduced15 = matrixx.GetColumn(1).get_magnitude();
                    Vector3 scale = new Vector3(introduced14, introduced15, matrixx.GetColumn(2).get_magnitude());
                    MathUtil.Decompose(matrixx.get_transpose(), out scale, out rotation, out column);
                    string goPath = gameObjectPath;
                    getComponentsData(obj4, node, child, column, rotation, scale, ref goPath);
                    node.AddField("child", child);
                    parentsChildNodes.Add(node);
                }
            }
        }

        public static void getSkinAniGameObjectData(GameObject gameObject, string gameObjectPath, JSONObject parentsChildNodes, List<string> linkSprite = null)
        {
            componentsOnGameObject(gameObject);
            Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
                GameObject obj4 = componentsInChildren[i].get_gameObject();
                List<ComponentType> list = componentsOnGameObject(obj4);
                checkChildIsLegal(obj4, true);
                if (((obj4 != gameObject) && ((list.Count <= 1) || (obj4.get_transform().get_parent().get_gameObject() == gameObject))) && ((selectParentbyType(obj4, ComponentType.Animator) == gameObject) && ((list.Count > 1) || isHasLegalChild(obj4))))
                {
                    if (isHasLegalChild(obj4))
                    {
                        for (int j = 0; j < obj4.get_transform().get_childCount(); j++)
                        {
                            getGameObjectData(obj4.get_transform().GetChild(j).get_gameObject(), gameObjectPath, obj3, true);
                            if ((linkSprite != null) && (linkSprite.IndexOf(obj4.get_name()) == -1))
                            {
                                linkSprite.Add(obj4.get_name());
                            }
                        }
                    }
                    Matrix4x4 matrixx = gameObject.get_transform().get_worldToLocalMatrix() * obj4.get_transform().get_localToWorldMatrix();
                    Vector3 column = matrixx.GetColumn(3);
                    Vector4 introduced13 = matrixx.GetColumn(2);
                    Quaternion rotation = Quaternion.LookRotation(introduced13, matrixx.GetColumn(1));
                    float introduced14 = matrixx.GetColumn(0).get_magnitude();
                    float introduced15 = matrixx.GetColumn(1).get_magnitude();
                    Vector3 scale = new Vector3(introduced14, introduced15, matrixx.GetColumn(2).get_magnitude());
                    MathUtil.Decompose(matrixx.get_transpose(), out scale, out rotation, out column);
                    string goPath = gameObjectPath;
                    getComponentsData(obj4, node, obj3, column, rotation, scale, ref goPath);
                    node.AddField("child", obj3);
                    parentsChildNodes.Add(node);
                }
            }
        }

        public static void getSkinnedMeshRendererComponentData(GameObject gameObject, JSONObject customProps)
        {
            SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
            if (selectParentbyType(gameObject, ComponentType.Animation) == null)
            {
                customProps.AddField("rootBone", (component.get_rootBone() != null) ? component.get_rootBone().get_name() : "");
                Bounds bounds = component.get_localBounds();
                Vector3 vector = bounds.get_center();
                Vector3 vector2 = new Vector3(-vector.x, vector.y, vector.z);
                Vector3 vector3 = bounds.get_extents();
                Vector3 vector4 = vector2 - vector3;
                Vector3 vector5 = vector2 + vector3;
                float val = Vector3.Distance(vector4, vector5) / 2f;
                JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("boundBox", obj3);
                JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
                obj4.Add(vector4.x);
                obj4.Add(vector4.y);
                obj4.Add(vector4.z);
                obj3.AddField("min", obj4);
                JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
                obj5.Add(vector5.x);
                obj5.Add(vector5.y);
                obj5.Add(vector5.z);
                obj3.AddField("max", obj5);
                JSONObject obj6 = new JSONObject(JSONObject.Type.OBJECT);
                customProps.AddField("boundSphere", obj6);
                JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
                obj7.Add(vector2.x);
                obj7.Add(vector2.y);
                obj7.Add(vector2.z);
                obj6.AddField("center", obj7);
                obj6.AddField("radius", val);
            }
            Material[] materialArray = component.get_sharedMaterials();
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("materials", obj2);
            for (int i = 0; i < materialArray.Length; i++)
            {
                Material material = materialArray[i];
                if (material != null)
                {
                    char[] separator = new char[] { '.' };
                    string str = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".lmat";
                    string path = SAVEPATH + "/" + str;
                    string str3 = material.get_shader().get_name();
                    char[] chArray2 = new char[] { '/' };
                    if (str3.Split(chArray2)[0] == "LayaAir3D")
                    {
                        JSONObject obj8 = new JSONObject(JSONObject.Type.OBJECT);
                        char[] chArray3 = new char[] { '/' };
                        if (str3.Split(chArray3)[1] == "BlinnPhong")
                        {
                            obj8.AddField("type", "Laya.BlinnPhongMaterial");
                        }
                        else
                        {
                            char[] chArray4 = new char[] { '/' };
                            if (str3.Split(chArray4)[1] == "PBR(Standard)")
                            {
                                obj8.AddField("type", "Laya.PBRStandardMaterial");
                            }
                            else
                            {
                                char[] chArray5 = new char[] { '/' };
                                if (str3.Split(chArray5)[1] == "PBR(Specular)")
                                {
                                    obj8.AddField("type", "Laya.PBRSpecularMaterial");
                                }
                            }
                        }
                        obj8.AddField("path", str);
                        obj2.Add(obj8);
                        if (!File.Exists(path) || CoverOriginalFile)
                        {
                            saveLayaLmatData(material, path);
                        }
                    }
                    else
                    {
                        JSONObject obj9 = new JSONObject(JSONObject.Type.OBJECT);
                        obj9.AddField("type", "Laya.StandardMaterial");
                        obj9.AddField("path", str);
                        obj2.Add(obj9);
                        if (!File.Exists(path) || CoverOriginalFile)
                        {
                            saveLmatFile(material, path, ComponentType.MeshRenderer);
                        }
                    }
                }
            }
            Mesh mesh = component.get_sharedMesh();
            if (mesh != null)
            {
                string str4 = cleanIllegalChar(mesh.get_name(), true);
                char[] chArray6 = new char[] { '.' };
                string str5 = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(mesh.GetInstanceID()).Split(chArray6)[0], false) + "-" + str4;
                if (!OptimizeMeshName)
                {
                    object[] objArray1 = new object[] { str5, "[", mesh.GetInstanceID(), "].lm" };
                    str5 = string.Concat(objArray1);
                }
                else
                {
                    str5 = str5 + ".lm";
                }
                string str6 = SAVEPATH + "/" + str5;
                customProps.AddField("meshPath", str5);
                if (!File.Exists(str6) || CoverOriginalFile)
                {
                    saveSkinLmFile(component, str6);
                }
            }
            else
            {
                Debug.LogWarning("LayaUnityPlugin : " + gameObject.get_name() + "'s Mesh data can't be null!");
            }
        }

        public static void getSphereColliderComponentData(GameObject gameObject, JSONObject component)
        {
            foreach (SphereCollider collider in gameObject.GetComponents<SphereCollider>())
            {
                JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
                obj2.AddField("isTrigger", collider.get_isTrigger());
                Vector3 vector = collider.get_center();
                obj3.Add(vector.x);
                obj3.Add(vector.y);
                obj3.Add(vector.z);
                obj2.AddField("center", obj3);
                obj2.AddField("radius", collider.get_radius());
                component.AddField("SphereCollider", obj2);
            }
        }

        public static void getTerrainComponentData(GameObject gameObject, JSONObject customProps)
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            Terrain component = gameObject.GetComponent<Terrain>();
            if (ConvertTerrainToMesh)
            {
                saveTerrainLmFile(gameObject, customProps, 3);
                saveTerrainLmatData(gameObject, customProps);
            }
            else
            {
                saveTerrainData(SAVEPATH, customProps, null);
            }
            if (component.get_lightmapIndex() > -1)
            {
                customProps.AddField("lightmapIndex", component.get_lightmapIndex());
                customProps.AddField("lightmapScaleOffset", obj2);
                obj2.Add(component.get_lightmapScaleOffset().x);
                obj2.Add(component.get_lightmapScaleOffset().y);
                obj2.Add(component.get_lightmapScaleOffset().z);
                obj2.Add(-component.get_lightmapScaleOffset().w);
            }
        }

        public static void getTerrainGameObjectData(GameObject gameObject, JSONObject parentsChildNodes)
        {
            TerrainData data1 = gameObject.GetComponent<Terrain>().get_terrainData();
            int num = data1.get_heightmapWidth();
            int num2 = data1.get_heightmapHeight();
            Vector3 vector = data1.get_size();
            int num3 = 0x40;
            vector = new Vector3((vector.x / ((float) (num - 1))) * num3, vector.y, (vector.z / ((float) (num2 - 1))) * num3);
            Vector2 vector2 = new Vector2(1f / ((float) (num - 1)), 1f / ((float) (num2 - 1)));
            float[,] numArray = data1.GetHeights(0, 0, num, num2);
            num = ((num - 1) / num3) + 1;
            num2 = ((num2 - 1) / num3) + 1;
            int num4 = 2;
            List<List<Vector3>> list = new List<List<Vector3>>();
            List<List<Vector3>> list2 = new List<List<Vector3>>();
            List<List<Vector2>> list3 = new List<List<Vector2>>();
            for (int i = 0; i < (num4 * num4); i++)
            {
                list.Add(new List<Vector3>());
                list2.Add(new List<Vector3>());
                list3.Add(new List<Vector2>());
            }
            for (int j = 0; j < num2; j++)
            {
                for (int n = 0; n < num; n++)
                {
                    Vector3 item = Quaternion.Euler(0f, 0f, 0f) * Vector3.Scale(vector, new Vector3((float) j, numArray[n * num3, j * num3], (float) n));
                    Vector2 vector4 = Vector2.Scale(new Vector2((float) (n * num3), (float) (1 - (j * num3))), vector2);
                    vector4 = new Vector2((vector4.x * Mathf.Cos(1.570796f)) - (vector4.y * Mathf.Sin(1.570796f)), (vector4.x * Mathf.Sin(1.570796f)) + (vector4.y * Mathf.Cos(1.570796f)));
                    if ((j <= ((num2 - 1) / 2)) && (n <= ((num - 1) / 2)))
                    {
                        list[0].Add(item);
                        list3[0].Add(vector4);
                    }
                    if ((j <= ((num2 - 1) / 2)) && (n >= ((num - 1) / 2)))
                    {
                        list[1].Add(item);
                        list3[1].Add(vector4);
                    }
                    if ((j >= ((num2 - 1) / 2)) && (n <= ((num - 1) / 2)))
                    {
                        list[2].Add(item);
                        list3[2].Add(vector4);
                    }
                    if ((j >= ((num2 - 1) / 2)) && (n >= ((num - 1) / 2)))
                    {
                        list[3].Add(item);
                        list3[3].Add(vector4);
                    }
                }
            }
            num = (num - 1) / num4;
            num2 = (num2 - 1) / num4;
            int[] numArray2 = new int[(num * num2) * 6];
            int num5 = 0;
            for (int k = 0; k < num2; k++)
            {
                for (int num16 = 0; num16 < num; num16++)
                {
                    numArray2[num5++] = ((k + 1) * (num + 1)) + num16;
                    numArray2[num5++] = (k * (num + 1)) + num16;
                    numArray2[num5++] = (((k + 1) * (num + 1)) + num16) + 1;
                    numArray2[num5++] = (k * (num + 1)) + num16;
                    numArray2[num5++] = ((k * (num + 1)) + num16) + 1;
                    numArray2[num5++] = (((k + 1) * (num + 1)) + num16) + 1;
                }
            }
            for (int m = 0; m < list.Count; m++)
            {
                for (int num18 = 0; num18 < list[m].Count; num18++)
                {
                    List<Vector3> list4 = new List<Vector3>();
                    for (int num19 = 0; num19 < numArray2.Length; num19 += 3)
                    {
                        if (((numArray2[num19] == num18) || (numArray2[num19 + 1] == num18)) || (numArray2[num19 + 2] == num18))
                        {
                            list4.Add(list[m][numArray2[num19]]);
                            list4.Add(list[m][numArray2[num19 + 1]]);
                            list4.Add(list[m][numArray2[num19 + 2]]);
                        }
                    }
                    float num11 = 0f;
                    List<float> list6 = new List<float>();
                    List<Vector3> list5 = new List<Vector3>();
                    for (int num20 = 0; num20 < list4.Count; num20 += 3)
                    {
                        Vector3 vector5 = list4[num20] - list4[num20 + 1];
                        Vector3 vector6 = list4[num20] - list4[num20 + 2];
                        float num6 = Mathf.Sqrt((Mathf.Pow(list4[num20].x - list4[num20 + 1].x, 2f) + Mathf.Pow(list4[num20].y - list4[num20 + 1].y, 2f)) + Mathf.Pow(list4[num20].z - list4[num20 + 1].z, 2f));
                        float num7 = Mathf.Sqrt((Mathf.Pow(list4[num20].x - list4[num20 + 2].x, 2f) + Mathf.Pow(list4[num20].y - list4[num20 + 2].y, 2f)) + Mathf.Pow(list4[num20].z - list4[num20 + 2].z, 2f));
                        float num8 = Mathf.Sqrt((Mathf.Pow(list4[num20 + 2].x - list4[num20 + 1].x, 2f) + Mathf.Pow(list4[num20 + 2].y - list4[num20 + 1].y, 2f)) + Mathf.Pow(list4[num20 + 2].z - list4[num20 + 1].z, 2f));
                        float num9 = ((num6 + num7) + num8) / 2f;
                        float num10 = Mathf.Sqrt(((num9 * (num9 - num6)) * (num9 - num7)) * (num9 - num8));
                        list6.Add(num10);
                        num11 += num10;
                        list5.Add(Vector3.Cross(vector5, vector6).get_normalized());
                    }
                    Vector3 vector7 = new Vector3();
                    for (int num21 = 0; num21 < list5.Count; num21++)
                    {
                        vector7 += (Vector3) ((list5[num21] * list6[num21]) / num11);
                    }
                    list2[m].Add(vector7.get_normalized());
                }
            }
        }

        public static long GetTimeStamp()
        {
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - new DateTime(0x7b2, 1, 1, 0, 0, 0, 0));
            return Convert.ToInt64(span.TotalMilliseconds);
        }

        public static void getTrailRendererComponentData(GameObject gameObject, JSONObject props, JSONObject customProps)
        {
            TrailRenderer component = gameObject.GetComponent<TrailRenderer>();
            props.AddField("time", component.get_time());
            props.AddField("minVertexDistance", component.get_minVertexDistance());
            props.AddField("widthMultiplier", component.get_widthMultiplier());
            if (component.get_textureMode() == null)
            {
                props.AddField("textureMode", 0);
            }
            else
            {
                props.AddField("textureMode", 1);
            }
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("widthCurve", obj2);
            Keyframe[] keyframeArray = component.get_widthCurve().get_keys();
            for (int i = 0; i < keyframeArray.Length; i++)
            {
                JSONObject obj7;
                Keyframe keyframe = keyframeArray[i];
                if ((i == 0) && (keyframe.get_time() != 0f))
                {
                    obj7 = new JSONObject(JSONObject.Type.OBJECT);
                    obj7.AddField("time", 0);
                    obj7.AddField("inTangent", 0);
                    obj7.AddField("outTangent", 0);
                    obj7.AddField("value", keyframe.get_value());
                    obj2.Add(obj7);
                }
                obj7 = new JSONObject(JSONObject.Type.OBJECT);
                obj7.AddField("time", keyframe.get_time());
                obj7.AddField("inTangent", keyframe.get_inTangent());
                obj7.AddField("outTangent", keyframe.get_inTangent());
                obj7.AddField("value", keyframe.get_value());
                obj2.Add(obj7);
                if ((i == (keyframeArray.Length - 1)) && (keyframe.get_time() != 1f))
                {
                    obj7 = new JSONObject(JSONObject.Type.OBJECT);
                    obj7.AddField("time", 1);
                    obj7.AddField("inTangent", 0);
                    obj7.AddField("outTangent", 0);
                    obj7.AddField("value", keyframe.get_value());
                    obj2.Add(obj7);
                }
            }
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            customProps.AddField("colorGradient", obj3);
            Gradient gradient = component.get_colorGradient();
            if (gradient.get_mode() == null)
            {
                obj3.AddField("mode", 0);
            }
            else
            {
                obj3.AddField("mode", 1);
            }
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.AddField("colorKeys", obj4);
            GradientColorKey[] keyArray = gradient.get_colorKeys();
            for (int j = 0; j < keyArray.Length; j++)
            {
                JSONObject obj8;
                JSONObject obj9;
                Color color;
                GradientColorKey key = keyArray[j];
                if ((j == 0) && (key.time != 0f))
                {
                    obj8 = new JSONObject(JSONObject.Type.OBJECT);
                    obj8.AddField("time", 0);
                    obj9 = new JSONObject(JSONObject.Type.ARRAY);
                    obj8.AddField("value", obj9);
                    color = key.color;
                    obj9.Add(color.r);
                    obj9.Add(color.g);
                    obj9.Add(color.b);
                    obj4.Add(obj8);
                }
                obj8 = new JSONObject(JSONObject.Type.OBJECT);
                obj8.AddField("time", key.time);
                obj9 = new JSONObject(JSONObject.Type.ARRAY);
                obj8.AddField("value", obj9);
                color = key.color;
                obj9.Add(color.r);
                obj9.Add(color.g);
                obj9.Add(color.b);
                obj4.Add(obj8);
                if ((j == (keyArray.Length - 1)) && (key.time != 1f))
                {
                    obj8 = new JSONObject(JSONObject.Type.OBJECT);
                    obj8.AddField("time", 1);
                    obj9 = new JSONObject(JSONObject.Type.ARRAY);
                    obj8.AddField("value", obj9);
                    color = key.color;
                    obj9.Add(color.r);
                    obj9.Add(color.g);
                    obj9.Add(color.b);
                    obj4.Add(obj8);
                }
            }
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.AddField("alphaKeys", obj5);
            GradientAlphaKey[] keyArray2 = gradient.get_alphaKeys();
            for (int k = 0; k < keyArray2.Length; k++)
            {
                JSONObject obj10;
                GradientAlphaKey key2 = keyArray2[k];
                if ((k == 0) && (key2.time != 0f))
                {
                    obj10 = new JSONObject(JSONObject.Type.OBJECT);
                    obj10.AddField("time", 0);
                    obj10.AddField("value", key2.alpha);
                    obj5.Add(obj10);
                }
                obj10 = new JSONObject(JSONObject.Type.OBJECT);
                obj10.AddField("time", key2.time);
                obj10.AddField("value", key2.alpha);
                obj5.Add(obj10);
                if ((k == (keyArray2.Length - 1)) && (key2.time != 1f))
                {
                    obj10 = new JSONObject(JSONObject.Type.OBJECT);
                    obj10.AddField("time", 1);
                    obj10.AddField("value", key2.alpha);
                    obj5.Add(obj10);
                }
            }
            Material[] materialArray = component.get_sharedMaterials();
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("materials", obj6);
            for (int m = 0; m < materialArray.Length; m++)
            {
                Material material = materialArray[m];
                if (material != null)
                {
                    char[] separator = new char[] { '.' };
                    string val = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".lmat";
                    string path = SAVEPATH + "/" + val;
                    string str3 = material.get_shader().get_name();
                    char[] chArray2 = new char[] { '/' };
                    if (str3.Split(chArray2)[0] == "LayaAir3D")
                    {
                        char[] chArray3 = new char[] { '/' };
                        if (str3.Split(chArray3)[1] == "Trail")
                        {
                            JSONObject obj11 = new JSONObject(JSONObject.Type.OBJECT);
                            obj11.AddField("type", "Laya.TrailMaterial");
                            obj11.AddField("path", val);
                            obj6.Add(obj11);
                            if (!File.Exists(path) || CoverOriginalFile)
                            {
                                saveLayaParticleLmatData(material, path);
                            }
                        }
                    }
                }
            }
        }

        public static void getTransformComponentData(GameObject gameObject, JSONObject customProps, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("translate", obj2);
            customProps.AddField("rotation", obj3);
            customProps.AddField("scale", obj4);
            List<ComponentType> list = componentsOnGameObject(gameObject);
            obj2.Add((float) (position.x * -1f));
            obj2.Add(position.y);
            obj2.Add(position.z);
            if (list.IndexOf(ComponentType.Terrain) == -1)
            {
                if ((list.IndexOf(ComponentType.Camera) != -1) || (list.IndexOf(ComponentType.DirectionalLight) != -1))
                {
                    rotation *= new Quaternion(0f, 1f, 0f, 0f);
                }
                obj3.Add((float) (rotation.x * -1f));
                obj3.Add(rotation.y);
                obj3.Add(rotation.z);
                obj3.Add((float) (rotation.w * -1f));
                obj4.Add(scale.x);
                obj4.Add(scale.y);
                obj4.Add(scale.z);
            }
            else
            {
                obj3.Add(0);
                obj3.Add(0);
                obj3.Add(0);
                obj3.Add(-1);
                obj4.Add(1);
                obj4.Add(1);
                obj4.Add(1);
            }
        }

        public static VertexData getVertexData(Mesh mesh, int index)
        {
            VertexData data;
            data.index = index;
            data.vertice = mesh.get_vertices()[index];
            if (VertexStructure[1] == 1)
            {
                data.normal = mesh.get_normals()[index];
            }
            else
            {
                data.normal = new Vector3();
            }
            if (VertexStructure[2] == 1)
            {
                data.color = mesh.get_colors()[index];
            }
            else
            {
                data.color = new Color();
            }
            if (VertexStructure[3] == 1)
            {
                data.uv = mesh.get_uv()[index];
            }
            else
            {
                data.uv = new Vector2();
            }
            if (VertexStructure[4] == 1)
            {
                data.uv2 = mesh.get_uv2()[index];
            }
            else
            {
                data.uv2 = new Vector2();
            }
            if (VertexStructure[5] == 1)
            {
                BoneWeight weight = mesh.get_boneWeights()[index];
                data.boneWeight.x = weight.get_weight0();
                data.boneWeight.y = weight.get_weight1();
                data.boneWeight.z = weight.get_weight2();
                data.boneWeight.w = weight.get_weight3();
                data.boneIndex.x = weight.get_boneIndex0();
                data.boneIndex.y = weight.get_boneIndex1();
                data.boneIndex.z = weight.get_boneIndex2();
                data.boneIndex.w = weight.get_boneIndex3();
            }
            else
            {
                data.boneWeight = new Vector4();
                data.boneIndex = new Vector4();
            }
            if (VertexStructure[6] == 1)
            {
                data.tangent = mesh.get_tangents()[index];
                return data;
            }
            data.tangent = new Vector4();
            return data;
        }

        public static bool IsCustomGameObject(GameObject gameObject)
        {
            bool flag = false;
            if ((gameObject.get_transform().get_parent() == null) && (gameObject.get_name() == "pathFind"))
            {
                pathFindGameObject = gameObject;
                flag = true;
            }
            return flag;
        }

        public static bool isHasChildByType(GameObject gameObject, ComponentType type, bool onlySon, bool isCheckParent)
        {
            GameObject obj2 = gameObject;
            if (isCheckParent)
            {
                obj2 = gameObject.get_transform().get_parent().get_gameObject();
            }
            List<GameObject> selectGameObjects = new List<GameObject>();
            selectChildByType(obj2, type, selectGameObjects, onlySon);
            return (selectGameObjects.Count > 0);
        }

        public static bool isHasLegalChild(GameObject gameObject)
        {
            for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
            {
                GameObject obj2 = gameObject.get_transform().GetChild(i).get_gameObject();
                if ((componentsOnGameObject(obj2).Count > 1) && obj2.get_activeInHierarchy())
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLayaAutoGameObjects(GameObject gameObject)
        {
            return ((gameObject.get_tag() == "Laya3D") && !layaAutoGameObjectsList[LayaAutoGOListIndex].ContainsKey(gameObject));
        }

        public static void onChangeLayaBlinnPhong(Material material)
        {
            switch (material.GetInt("_Mode"))
            {
                case 0:
                    material.SetInt("_AlphaTest", 0);
                    material.SetInt("_AlphaBlend", 0);
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_ZTest", 2);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("EnableAlphaCutoff");
                    material.set_renderQueue(0x7d0);
                    material.SetInt("_RenderQueue", 0);
                    return;

                case 1:
                    material.SetInt("_AlphaTest", 1);
                    material.SetInt("_AlphaBlend", 0);
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.SetInt("_ZTest", 2);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("EnableAlphaCutoff");
                    material.set_renderQueue(0x992);
                    material.SetInt("_RenderQueue", 1);
                    return;

                case 2:
                    material.SetInt("_AlphaTest", 0);
                    material.SetInt("_AlphaBlend", 1);
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_ZTest", 2);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("EnableAlphaCutoff");
                    material.set_renderQueue(0xbb8);
                    material.SetInt("_RenderQueue", 2);
                    return;

                case 3:
                    material.SetInt("_AlphaTest", 0);
                    material.SetInt("_AlphaBlend", 1);
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 1);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_ZTest", 2);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("EnableAlphaCutoff");
                    material.set_renderQueue(0xbb8);
                    material.SetInt("_RenderQueue", 2);
                    return;
            }
            material.SetInt("_AlphaTest", 0);
            material.SetInt("_AlphaBlend", 0);
            material.SetInt("_SrcBlend", 1);
            material.SetInt("_DstBlend", 0);
            material.SetInt("_ZWrite", 1);
            material.SetInt("_ZTest", 2);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("EnableAlphaCutoff");
            material.set_renderQueue(0x7d0);
            material.SetInt("_RenderQueue", 0);
        }

        public static void onChangeLayaParticle(Material material)
        {
            switch (material.GetInt("_Mode"))
            {
                case 0:
                    material.SetInt("_AlphaTest", 0);
                    material.SetInt("_AlphaBlend", 1);
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 1);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_ZTest", 2);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.set_renderQueue(0xbb8);
                    material.SetInt("_RenderQueue", 2);
                    return;

                case 1:
                    material.SetInt("_AlphaTest", 0);
                    material.SetInt("_AlphaBlend", 1);
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.SetInt("_ZTest", 2);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.set_renderQueue(0xbb8);
                    material.SetInt("_RenderQueue", 2);
                    return;
            }
            material.SetInt("_AlphaTest", 0);
            material.SetInt("_AlphaBlend", 1);
            material.SetInt("_SrcBlend", 5);
            material.SetInt("_DstBlend", 10);
            material.SetInt("_ZWrite", 0);
            material.SetInt("_ZTest", 2);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.set_renderQueue(0xbb8);
            material.SetInt("_RenderQueue", 2);
        }

        public static void recodeLayaJSFile(string pathName)
        {
            FileStream stream = new FileStream(Application.get_dataPath() + "/WebPlayerTemplates/LayaDemo/LayaAir3DSample.js", System.IO.FileMode.Create, FileAccess.ReadWrite);
            StreamWriter writer = new StreamWriter(stream);
            writer.WriteLine("Laya3D.init(0, 0, true);");
            writer.WriteLine("Laya.stage.scaleMode = Laya.Stage.SCALE_FULL;");
            writer.WriteLine("Laya.stage.screenMode = Laya.Stage.SCREEN_NONE;");
            writer.WriteLine("Laya.Stat.show();");
            if (Type == 0)
            {
                writer.WriteLine("var scene = Laya.stage.addChild(Laya.Scene.load('res" + pathName + ".ls'));");
            }
            else
            {
                writer.WriteLine("var scene = Laya.stage.addChild(new Laya.Scene());");
                writer.WriteLine("var sprite3D = scene.addChild(Laya.Sprite3D.load('res" + pathName + ".lh'));");
            }
            writer.Close();
            stream.Close();
        }

        public static void saveData()
        {
            if (BatchMade && (Type == 1))
            {
                foreach (KeyValuePair<string, JSONObject> pair in saveSpriteNode())
                {
                    string path = SAVEPATH + "/" + cleanIllegalChar(pair.Key, true) + ".lh";
                    if (!File.Exists(path) || CoverOriginalFile)
                    {
                        Util.FileUtil.saveFile(path, pair.Value);
                    }
                }
            }
            else
            {
                string str2 = "";
                if (Type == 0)
                {
                    str2 = SAVEPATH + "/" + sceneName + ".ls";
                }
                else if (Type == 1)
                {
                    str2 = SAVEPATH + "/" + sceneName + ".lh";
                }
                if (File.Exists(str2) && !CoverOriginalFile)
                {
                    return;
                }
                Util.FileUtil.saveFile(str2, getSceneNode());
            }
            Debug.Log(" -- Exporting Data is Finished -- ");
        }

        public static void saveLaniData(GameObject gameObject, JSONObject obj)
        {
            List<ComponentType> list = componentsOnGameObject(gameObject);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("UnityEngine.Transform", "transform");
            dictionary.Add("UnityEngine.MeshRenderer", "meshRenderer");
            dictionary.Add("UnityEngine.SkinnedMeshRenderer", "skinnedMeshRender");
            dictionary.Add("UnityEngine.ParticleSystemRenderer", "particleRender");
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
            dictionary2.Add("m_LocalPosition", "localPosition");
            dictionary2.Add("m_LocalRotation", "localRotation");
            dictionary2.Add("m_LocalScale", "localScale");
            dictionary2.Add("localEulerAnglesRaw", "localRotationEuler");
            dictionary2.Add("material._MainTex_ST", "meshRender.sharedMaterial.tilingOffset|skinnedMeshRender.sharedMaterial.tilingOffset|particleRender.sharedMaterial.tilingOffset");
            dictionary2.Add("material._TintColor", "meshRender.sharedMaterial.albedoColor|skinnedMeshRender.sharedMaterial.albedoColor|particleRender.sharedMaterial.tintColor");
            dictionary2.Add("material._Color", "meshRender.sharedMaterial.albedoColor|skinnedMeshRender.sharedMaterial.albedoColor|particleRender.sharedMaterial.tintColor");
            dictionary2.Add("material._SpecularColor", "meshRender.sharedMaterial.specularColor|skinnedMeshRender.sharedMaterial.specularColor");
            Dictionary<string, byte> dictionary3 = new Dictionary<string, byte>();
            dictionary3.Add("m_LocalPosition", 12);
            dictionary3.Add("m_LocalRotation", 0x10);
            dictionary3.Add("m_LocalScale", 12);
            dictionary3.Add("localEulerAnglesRaw", 12);
            dictionary3.Add("material._MainTex_ST", 0x10);
            dictionary3.Add("material._TintColor", 0x10);
            dictionary3.Add("material._Color", 0x10);
            dictionary3.Add("material._SpecularColor", 0x10);
            Dictionary<string, int> dictionary4 = new Dictionary<string, int>();
            dictionary4.Add("m_LocalPosition", 3);
            dictionary4.Add("m_LocalRotation", 4);
            dictionary4.Add("m_LocalScale", 3);
            dictionary4.Add("localEulerAnglesRaw", 3);
            dictionary4.Add("material._MainTex_ST", 4);
            dictionary4.Add("material._TintColor", 4);
            dictionary4.Add("material._Color", 4);
            dictionary4.Add("material._SpecularColor", 4);
            List<string> list2 = new List<string> { "x", "y", "z" };
            List<string> list3 = new List<string> { "x", "y", "z", "w" };
            List<string> list4 = new List<string> { "r", "g", "b", "a" };
            Dictionary<string, List<string>> dictionary5 = new Dictionary<string, List<string>>();
            dictionary5.Add("m_LocalPosition", list2);
            dictionary5.Add("m_LocalRotation", list3);
            dictionary5.Add("m_LocalScale", list2);
            dictionary5.Add("localEulerAnglesRaw", list2);
            dictionary5.Add("material._MainTex_ST", list3);
            dictionary5.Add("material._TintColor", list4);
            dictionary5.Add("material._Color", list4);
            dictionary5.Add("material._SpecularColor", list4);
            List<ushort> list5 = new List<ushort> { 12, 0x10 };
            RuntimeAnimatorController controller = gameObject.GetComponent<Animator>().get_runtimeAnimatorController();
            if (controller == null)
            {
                Debug.LogWarning("LayaUnityPlugin : " + gameObject.get_name() + "'s Animator Compoment must have a Controller!");
            }
            else
            {
                foreach (AnimationClip clip in controller.get_animationClips())
                {
                    List<double> list6 = new List<double>();
                    List<string> list7 = new List<string> { "ANIMATIONS" };
                    if (clip != null)
                    {
                        gameObject.get_name();
                        int num2 = (int) clip.get_frameRate();
                        string item = cleanIllegalChar(clip.get_name(), true);
                        list7.Add(item);
                        char[] separator = new char[] { '.' };
                        string str = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(clip.GetInstanceID()).Split(separator)[0], false) + "-" + item + ".lani";
                        string path = SAVEPATH + "/" + str;
                        if (!File.Exists(path) || CoverOriginalFile)
                        {
                            AniNodeData data3;
                            obj.Add(str);
                            UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);
                            UnityEditor.AnimationClipCurveData[] dataArray = new UnityEditor.AnimationClipCurveData[curveBindings.Length];
                            for (int i = 0; i < curveBindings.Length; i++)
                            {
                                dataArray[i] = new UnityEditor.AnimationClipCurveData(curveBindings[i]);
                                dataArray[i].curve = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBindings[i]);
                                for (int num18 = 0; num18 < dataArray[i].curve.get_keys().Length; num18++)
                                {
                                    UnityEditor.AnimationUtility.SetKeyLeftTangentMode(dataArray[i].curve, num18, UnityEditor.AnimationUtility.TangentMode.Linear);
                                }
                            }
                            for (int j = 0; j < dataArray.Length; j++)
                            {
                                Keyframe[] keyframeArray = dataArray[j].curve.get_keys();
                                for (int num20 = 0; num20 < keyframeArray.Length; num20++)
                                {
                                    double num3 = Math.Round((double) keyframeArray[num20].get_time(), 3);
                                    if (list6.IndexOf(num3) == -1)
                                    {
                                        list6.Add(num3);
                                    }
                                }
                            }
                            list6.Sort();
                            List<string> list8 = new List<string>();
                            List<CustomAnimationClipCurveData> list9 = new List<CustomAnimationClipCurveData>();
                            for (int k = 0; k < dataArray.Length; k++)
                            {
                                CustomAnimationClipCurveData data;
                                CustomAnimationCurve curve;
                                UnityEditor.AnimationClipCurveData data4 = dataArray[k];
                                curve.keys = data4.curve.get_keys();
                                data.curve = curve;
                                data.path = data4.path;
                                data.propertyName = data4.propertyName;
                                data.type = data4.type;
                                list9.Add(data);
                            }
                            List<CustomAnimationClipCurveData> list10 = new List<CustomAnimationClipCurveData>();
                            List<CustomAnimationClipCurveData> list11 = new List<CustomAnimationClipCurveData>();
                            for (int m = 0; m < dataArray.Length; m++)
                            {
                                CustomAnimationClipCurveData data2;
                                CustomAnimationCurve curve2;
                                UnityEditor.AnimationClipCurveData data5 = dataArray[m];
                                curve2.keys = data5.curve.get_keys();
                                data2.curve = curve2;
                                data2.path = data5.path;
                                data2.propertyName = data5.propertyName;
                                data2.type = data5.type;
                                if (data2.propertyName.IndexOf(".") != -1)
                                {
                                    string str7 = data2.propertyName.Substring(0, data2.propertyName.LastIndexOf('.'));
                                    string str8 = data2.path;
                                    string str9 = str7 + "|" + str8;
                                    if (list8.IndexOf(str9) == -1)
                                    {
                                        list8.Add(str9);
                                        list10 = new List<CustomAnimationClipCurveData>();
                                        for (int num23 = 0; num23 < dictionary5[str7].Count; num23++)
                                        {
                                            string str10 = str7 + "." + dictionary5[str7][num23];
                                            for (int num24 = 0; num24 < list9.Count; num24++)
                                            {
                                                if ((list9[num24].propertyName == str10) && (list9[num24].path == str8))
                                                {
                                                    list10.Add(list9[num24]);
                                                    list9.RemoveAt(list9.IndexOf(list9[num24]));
                                                }
                                            }
                                        }
                                        if (dictionary5[str7].Count != list10.Count)
                                        {
                                            List<CustomAnimationClipCurveData> list15 = new List<CustomAnimationClipCurveData>();
                                            for (int num25 = 0; num25 < dictionary5[str7].Count; num25++)
                                            {
                                                string str11 = str7 + "." + dictionary5[str7][num25];
                                                bool flag = false;
                                                for (int num26 = 0; num26 < list10.Count; num26++)
                                                {
                                                    if (list10[num26].propertyName == str11)
                                                    {
                                                        flag = true;
                                                        list15.Add(list10[num26]);
                                                    }
                                                }
                                                if (!flag)
                                                {
                                                    CustomAnimationClipCurveData data6;
                                                    CustomAnimationCurve curve3;
                                                    curve3.keys = new Keyframe[0];
                                                    data6.path = list10[0].path;
                                                    data6.propertyName = str11;
                                                    data6.type = list10[0].type;
                                                    data6.curve = curve3;
                                                    list15.Add(data6);
                                                }
                                            }
                                            list10 = list15;
                                        }
                                        List<double> list13 = new List<double>();
                                        for (int num27 = 0; num27 < list10.Count; num27++)
                                        {
                                            Keyframe[] keys = list10[num27].curve.keys;
                                            for (int num28 = 0; num28 < keys.Length; num28++)
                                            {
                                                bool flag2 = false;
                                                for (int num29 = 0; num29 < list13.Count; num29++)
                                                {
                                                    if (Math.Round(list13[num29], 3) == Math.Round((double) keys[num28].get_time(), 3))
                                                    {
                                                        flag2 = true;
                                                    }
                                                }
                                                if (!flag2)
                                                {
                                                    list13.Add((double) keys[num28].get_time());
                                                }
                                            }
                                        }
                                        list13.Sort();
                                        List<Keyframe> list14 = new List<Keyframe>();
                                        for (int num30 = 0; num30 < list13.Count; num30++)
                                        {
                                            Keyframe keyframe = new Keyframe();
                                            keyframe.set_inTangent(float.NaN);
                                            keyframe.set_outTangent(float.NaN);
                                            keyframe.set_time((float) list13[num30]);
                                            keyframe.set_value(float.NaN);
                                            list14.Add(keyframe);
                                        }
                                        for (int num31 = 0; num31 < list10.Count; num31++)
                                        {
                                            CustomAnimationClipCurveData data7;
                                            CustomAnimationCurve curve4;
                                            Keyframe keyframe2;
                                            List<Keyframe> list16 = list10[num31].curve.keys.ToList<Keyframe>();
                                            List<Keyframe> list17 = new List<Keyframe>();
                                            for (int num32 = 0; num32 < list13.Count; num32++)
                                            {
                                                bool flag3 = false;
                                                for (int num33 = 0; num33 < list16.Count; num33++)
                                                {
                                                    keyframe2 = list16[num33];
                                                    if (Math.Round((double) keyframe2.get_time(), 3) == Math.Round(list13[num32], 3))
                                                    {
                                                        flag3 = true;
                                                        list17.Add(list16[num33]);
                                                    }
                                                }
                                                if (!flag3)
                                                {
                                                    list17.Add(list14[num32]);
                                                }
                                            }
                                            for (int num34 = 0; num34 < list13.Count; num34++)
                                            {
                                                keyframe2 = list17[num34];
                                                if (float.IsNaN(keyframe2.get_value()))
                                                {
                                                    float num43;
                                                    bool flag4 = false;
                                                    bool flag5 = false;
                                                    int num35 = -1;
                                                    int num36 = -1;
                                                    for (int num37 = num34 - 1; num37 >= 0; num37--)
                                                    {
                                                        keyframe2 = list17[num37];
                                                        if (!float.IsNaN(keyframe2.get_value()))
                                                        {
                                                            flag4 = true;
                                                            num35 = num37;
                                                            break;
                                                        }
                                                    }
                                                    for (int num38 = num34 + 1; num38 < list13.Count; num38++)
                                                    {
                                                        keyframe2 = list17[num38];
                                                        if (!float.IsNaN(keyframe2.get_value()))
                                                        {
                                                            flag5 = true;
                                                            num36 = num38;
                                                            break;
                                                        }
                                                    }
                                                    if (flag4 & flag5)
                                                    {
                                                        float num39;
                                                        keyframe2 = list17[num36];
                                                        keyframe2 = list17[num35];
                                                        float introduced143 = keyframe2.get_time();
                                                        float num40 = introduced143 - keyframe2.get_time();
                                                        float t = (float) ((list13[num34] - list13[num35]) / (list13[num36] - list13[num35]));
                                                        keyframe2 = list17[num35];
                                                        keyframe2 = list17[num36];
                                                        float start = keyframe2.get_value();
                                                        keyframe2 = list17[num35];
                                                        float end = keyframe2.get_value();
                                                        keyframe2 = list17[num36];
                                                        float num42 = MathUtil.Interpolate((float) list13[num35], (float) list13[num36], start, end, keyframe2.get_outTangent() * num40, keyframe2.get_inTangent() * num40, t, out num39);
                                                        Keyframe keyframe3 = new Keyframe();
                                                        keyframe3.set_outTangent(num43 = num39);
                                                        keyframe3.set_inTangent(num43);
                                                        keyframe3.set_value(num42);
                                                        keyframe3.set_time((float) list13[num34]);
                                                        list17[num34] = keyframe3;
                                                    }
                                                    else if (flag4 && !flag5)
                                                    {
                                                        Keyframe keyframe4 = new Keyframe();
                                                        keyframe4.set_outTangent(num43 = 0f);
                                                        keyframe4.set_inTangent(num43);
                                                        keyframe2 = list17[num35];
                                                        keyframe4.set_value(keyframe2.get_value());
                                                        keyframe4.set_time((float) list13[num34]);
                                                        list17[num34] = keyframe4;
                                                    }
                                                    else if (!flag4 & flag5)
                                                    {
                                                        Keyframe keyframe5 = new Keyframe();
                                                        keyframe5.set_outTangent(num43 = 0f);
                                                        keyframe5.set_inTangent(num43);
                                                        keyframe5.set_value(list17[num36].get_value());
                                                        keyframe5.set_time((float) list13[num34]);
                                                        list17[num34] = keyframe5;
                                                    }
                                                    else
                                                    {
                                                        Debug.LogWarning(gameObject.get_name() + "'s Animator " + gameObject.get_name() + "/" + list10[num31].path + " " + list10[num31].propertyName + " keyFrame data can't be null!");
                                                    }
                                                }
                                            }
                                            curve4.keys = list17.ToArray();
                                            data7.curve = curve4;
                                            data7.path = list10[num31].path;
                                            data7.propertyName = list10[num31].propertyName;
                                            data7.type = list10[num31].type;
                                            list10[num31] = data7;
                                        }
                                        for (int num44 = 0; num44 < list10.Count; num44++)
                                        {
                                            list11.Add(list10[num44]);
                                        }
                                    }
                                }
                            }
                            List<AniNodeData> list12 = new List<AniNodeData>();
                            int num4 = 0;
                            string str4 = "";
                            short num5 = -1;
                            string str5 = "";
                            ushort index = 0;
                            byte num7 = 0;
                            int num8 = 0;
                            for (int n = 0; n < list11.Count; n += num4)
                            {
                                CustomAnimationClipCurveData data8 = list11[n];
                                num8 = 0;
                                List<ushort> list18 = new List<ushort>();
                                char[] chArray2 = new char[] { '/' };
                                string[] strArray = data8.path.Split(chArray2);
                                for (int num47 = 0; num47 < strArray.Length; num47++)
                                {
                                    if (list7.IndexOf(strArray[num47]) == -1)
                                    {
                                        list7.Add(strArray[num47]);
                                    }
                                    list18.Add((ushort) list7.IndexOf(strArray[num47]));
                                }
                                data3.pathLength = (ushort) list18.Count;
                                data3.pathIndex = list18;
                                str4 = dictionary[data8.type.ToString()];
                                data3.conpomentTypeIndex = num5;
                                str5 = data8.propertyName.Substring(0, data8.propertyName.LastIndexOf('.'));
                                num4 = dictionary4[str5];
                                num7 = dictionary3[str5];
                                if (str4 == "meshRenderer")
                                {
                                    char[] chArray3 = new char[] { '|' };
                                    str5 = dictionary2[str5].Split(chArray3)[0];
                                }
                                else if (str4 == "skinnedMeshRender")
                                {
                                    char[] chArray4 = new char[] { '|' };
                                    str5 = dictionary2[str5].Split(chArray4)[1];
                                }
                                else if (str4 == "particleRender")
                                {
                                    char[] chArray5 = new char[] { '|' };
                                    str5 = dictionary2[str5].Split(chArray5)[2];
                                }
                                else
                                {
                                    str5 = dictionary2[str5];
                                }
                                if (list7.IndexOf(str5) == -1)
                                {
                                    list7.Add(str5);
                                }
                                index = (ushort) list7.IndexOf(str5);
                                data3.propertyNameIndex = index;
                                data3.frameDataLengthIndex = (byte) list5.IndexOf(num7);
                                List<AniNodeFrameData> list19 = new List<AniNodeFrameData>();
                                Keyframe[] keyframeArray3 = data8.curve.keys;
                                float num46 = 0f;
                                for (int num48 = 0; num48 < keyframeArray3.Length; num48++)
                                {
                                    AniNodeFrameData data9;
                                    num46 = keyframeArray3[num48].get_time();
                                    data9.startTimeIndex = (ushort) list6.IndexOf(Math.Round((double) num46, 3));
                                    List<float> list20 = new List<float>();
                                    List<float> list21 = new List<float>();
                                    List<float> list22 = new List<float>();
                                    int num49 = 0;
                                    for (int num50 = n; num50 < (n + num4); num50++)
                                    {
                                        Keyframe keyframe6 = list11[num50].curve.keys[num48];
                                        switch (str5)
                                        {
                                            case "localPosition":
                                                if (num49 == 0)
                                                {
                                                    list20.Add(keyframe6.get_value() * -1f);
                                                    list21.Add(keyframe6.get_inTangent() * -1f);
                                                    list22.Add(keyframe6.get_outTangent() * -1f);
                                                }
                                                else
                                                {
                                                    list20.Add(keyframe6.get_value());
                                                    list21.Add(keyframe6.get_inTangent());
                                                    list22.Add(keyframe6.get_outTangent());
                                                }
                                                break;

                                            case "localRotation":
                                                switch (num49)
                                                {
                                                    case 0:
                                                    case 3:
                                                        list20.Add(keyframe6.get_value() * -1f);
                                                        list21.Add(keyframe6.get_inTangent() * -1f);
                                                        list22.Add(keyframe6.get_outTangent() * -1f);
                                                        goto Label_12CB;
                                                }
                                                list20.Add(keyframe6.get_value());
                                                list21.Add(keyframe6.get_inTangent());
                                                list22.Add(keyframe6.get_outTangent());
                                                break;

                                            case "localRotationEuler":
                                                if (list.IndexOf(ComponentType.Camera) != -1)
                                                {
                                                    switch (num49)
                                                    {
                                                        case 0:
                                                            list20.Add(keyframe6.get_value() * -1f);
                                                            list21.Add(keyframe6.get_inTangent() * -1f);
                                                            list22.Add(keyframe6.get_outTangent() * -1f);
                                                            goto Label_12CB;

                                                        case 1:
                                                            list20.Add(180f - keyframe6.get_value());
                                                            list21.Add(keyframe6.get_inTangent() * -1f);
                                                            list22.Add(keyframe6.get_outTangent() * -1f);
                                                            goto Label_12CB;
                                                    }
                                                    list20.Add(keyframe6.get_value());
                                                    list21.Add(keyframe6.get_inTangent());
                                                    list22.Add(keyframe6.get_outTangent());
                                                }
                                                else if ((num49 == 1) || (num49 == 2))
                                                {
                                                    list20.Add(keyframe6.get_value() * -1f);
                                                    list21.Add(keyframe6.get_inTangent() * -1f);
                                                    list22.Add(keyframe6.get_outTangent() * -1f);
                                                }
                                                else
                                                {
                                                    list20.Add(keyframe6.get_value());
                                                    list21.Add(keyframe6.get_inTangent());
                                                    list22.Add(keyframe6.get_outTangent());
                                                }
                                                break;

                                            case "meshRender.sharedMaterial.tilingOffset":
                                            case "skinnedMeshRender.sharedMaterial.tilingOffset":
                                            case "particleRender.sharedMaterial.tilingOffset":
                                                if (num49 == 3)
                                                {
                                                    list20.Add(keyframe6.get_value() * -1f);
                                                    list21.Add(keyframe6.get_inTangent() * -1f);
                                                    list22.Add(keyframe6.get_outTangent() * -1f);
                                                }
                                                else
                                                {
                                                    list20.Add(keyframe6.get_value());
                                                    list21.Add(keyframe6.get_inTangent());
                                                    list22.Add(keyframe6.get_outTangent());
                                                }
                                                break;

                                            case "meshRender.sharedMaterial.albedoColor":
                                            case "skinnedMeshRender.sharedMaterial.albedoColor":
                                                list20.Add(keyframe6.get_value() * ScaleFactor);
                                                list21.Add(keyframe6.get_inTangent());
                                                list22.Add(keyframe6.get_outTangent());
                                                break;

                                            default:
                                                list20.Add(keyframe6.get_value());
                                                list21.Add(keyframe6.get_inTangent());
                                                list22.Add(keyframe6.get_outTangent());
                                                break;
                                        }
                                    Label_12CB:
                                        num49++;
                                    }
                                    data9.valueNumbers = list20;
                                    data9.inTangentNumbers = list21;
                                    data9.outTangentNumbers = list22;
                                    list19.Add(data9);
                                }
                                data3.keyFrameCount = (ushort) (keyframeArray3.Length + num8);
                                data3.aniNodeFrameDatas = list19;
                                list12.Add(data3);
                            }
                            FileStream fs = Util.FileUtil.saveFile(path, null);
                            long position = 0L;
                            long num10 = 0L;
                            long num11 = 0L;
                            long num12 = 0L;
                            long num13 = 0L;
                            long num14 = 0L;
                            string str6 = "LAYAANIMATION:02";
                            Util.FileUtil.WriteData(fs, str6);
                            position = fs.Position;
                            Util.FileUtil.WriteData(fs, new uint[1]);
                            Util.FileUtil.WriteData(fs, new uint[1]);
                            num10 = fs.Position;
                            int num15 = 1;
                            ushort[] datas = new ushort[] { (ushort) num15 };
                            Util.FileUtil.WriteData(fs, datas);
                            for (int num51 = 0; num51 < num15; num51++)
                            {
                                Util.FileUtil.WriteData(fs, new uint[1]);
                                Util.FileUtil.WriteData(fs, new uint[1]);
                            }
                            num11 = fs.Position;
                            Util.FileUtil.WriteData(fs, new uint[1]);
                            Util.FileUtil.WriteData(fs, new ushort[1]);
                            num12 = fs.Position;
                            ushort[] numArray2 = new ushort[] { (ushort) list7.IndexOf("ANIMATIONS") };
                            Util.FileUtil.WriteData(fs, numArray2);
                            byte[] buffer1 = new byte[] { (byte) list5.Count };
                            Util.FileUtil.WriteData(fs, buffer1);
                            for (int num52 = 0; num52 < list5.Count; num52++)
                            {
                                ushort[] numArray3 = new ushort[] { list5[num52] };
                                Util.FileUtil.WriteData(fs, numArray3);
                            }
                            ushort[] numArray4 = new ushort[] { (ushort) list6.Count };
                            Util.FileUtil.WriteData(fs, numArray4);
                            for (int num53 = 0; num53 < list6.Count; num53++)
                            {
                                float[] singleArray1 = new float[] { (float) list6[num53] };
                                Util.FileUtil.WriteData(fs, singleArray1);
                            }
                            ushort[] numArray5 = new ushort[] { (ushort) list7.IndexOf(item) };
                            Util.FileUtil.WriteData(fs, numArray5);
                            float[] singleArray2 = new float[] { (float) list6[list6.Count - 1] };
                            Util.FileUtil.WriteData(fs, singleArray2);
                            bool[] flagArray1 = new bool[] { clip.get_isLooping() };
                            Util.FileUtil.WriteData(fs, flagArray1);
                            ushort[] numArray6 = new ushort[] { (ushort) num2 };
                            Util.FileUtil.WriteData(fs, numArray6);
                            ushort[] numArray7 = new ushort[] { (ushort) list12.Count };
                            Util.FileUtil.WriteData(fs, numArray7);
                            for (int num54 = 0; num54 < list12.Count; num54++)
                            {
                                data3 = list12[num54];
                                ushort[] numArray8 = new ushort[] { data3.pathLength };
                                Util.FileUtil.WriteData(fs, numArray8);
                                for (int num55 = 0; num55 < data3.pathLength; num55++)
                                {
                                    ushort[] numArray9 = new ushort[] { data3.pathIndex[num55] };
                                    Util.FileUtil.WriteData(fs, numArray9);
                                }
                                short[] numArray10 = new short[] { data3.conpomentTypeIndex };
                                Util.FileUtil.WriteData(fs, numArray10);
                                ushort[] numArray11 = new ushort[] { data3.propertyNameIndex };
                                Util.FileUtil.WriteData(fs, numArray11);
                                byte[] buffer2 = new byte[] { data3.frameDataLengthIndex };
                                Util.FileUtil.WriteData(fs, buffer2);
                                ushort[] numArray12 = new ushort[] { data3.keyFrameCount };
                                Util.FileUtil.WriteData(fs, numArray12);
                                for (int num56 = 0; num56 < data3.keyFrameCount; num56++)
                                {
                                    ushort[] numArray13 = new ushort[] { data3.aniNodeFrameDatas[num56].startTimeIndex };
                                    Util.FileUtil.WriteData(fs, numArray13);
                                    List<float> valueNumbers = data3.aniNodeFrameDatas[num56].valueNumbers;
                                    List<float> inTangentNumbers = data3.aniNodeFrameDatas[num56].inTangentNumbers;
                                    List<float> outTangentNumbers = data3.aniNodeFrameDatas[num56].outTangentNumbers;
                                    for (int num57 = 0; num57 < inTangentNumbers.Count; num57++)
                                    {
                                        float[] singleArray3 = new float[] { inTangentNumbers[num57] };
                                        Util.FileUtil.WriteData(fs, singleArray3);
                                    }
                                    for (int num58 = 0; num58 < outTangentNumbers.Count; num58++)
                                    {
                                        float[] singleArray4 = new float[] { outTangentNumbers[num58] };
                                        Util.FileUtil.WriteData(fs, singleArray4);
                                    }
                                    for (int num59 = 0; num59 < valueNumbers.Count; num59++)
                                    {
                                        float[] singleArray5 = new float[] { valueNumbers[num59] };
                                        Util.FileUtil.WriteData(fs, singleArray5);
                                    }
                                }
                            }
                            AnimationEvent[] eventArray = clip.get_events();
                            int length = eventArray.Length;
                            short[] numArray14 = new short[] { (short) length };
                            Util.FileUtil.WriteData(fs, numArray14);
                            for (int num60 = 0; num60 < length; num60++)
                            {
                                AnimationEvent event2 = eventArray[num60];
                                float[] singleArray6 = new float[] { event2.get_time() };
                                Util.FileUtil.WriteData(fs, singleArray6);
                                string str12 = event2.get_functionName();
                                if (list7.IndexOf(str12) == -1)
                                {
                                    list7.Add(str12);
                                }
                                ushort[] numArray15 = new ushort[] { (ushort) list7.IndexOf(str12) };
                                Util.FileUtil.WriteData(fs, numArray15);
                                ushort num61 = 3;
                                ushort[] numArray16 = new ushort[] { num61 };
                                Util.FileUtil.WriteData(fs, numArray16);
                                for (int num62 = 0; num62 < 1; num62++)
                                {
                                    byte[] buffer3 = new byte[] { 2 };
                                    Util.FileUtil.WriteData(fs, buffer3);
                                    float[] singleArray7 = new float[] { event2.get_floatParameter() };
                                    Util.FileUtil.WriteData(fs, singleArray7);
                                    byte[] buffer4 = new byte[] { 1 };
                                    Util.FileUtil.WriteData(fs, buffer4);
                                    int[] numArray17 = new int[] { event2.get_intParameter() };
                                    Util.FileUtil.WriteData(fs, numArray17);
                                    byte[] buffer5 = new byte[] { 3 };
                                    Util.FileUtil.WriteData(fs, buffer5);
                                    string str13 = event2.get_stringParameter();
                                    if (list7.IndexOf(str13) == -1)
                                    {
                                        list7.Add(str13);
                                    }
                                    ushort[] numArray18 = new ushort[] { (ushort) list7.IndexOf(str13) };
                                    Util.FileUtil.WriteData(fs, numArray18);
                                }
                            }
                            num13 = fs.Position;
                            for (int num63 = 0; num63 < list7.Count; num63++)
                            {
                                Util.FileUtil.WriteData(fs, list7[num63]);
                            }
                            num14 = fs.Position;
                            fs.Position = num11 + 4L;
                            ushort[] numArray19 = new ushort[] { (ushort) list7.Count };
                            Util.FileUtil.WriteData(fs, numArray19);
                            fs.Position = (num10 + 2L) + 4L;
                            uint[] numArray20 = new uint[] { (uint) (num13 - num12) };
                            Util.FileUtil.WriteData(fs, numArray20);
                            fs.Position = position;
                            uint[] numArray21 = new uint[] { (uint) num13 };
                            Util.FileUtil.WriteData(fs, numArray21);
                            uint[] numArray22 = new uint[] { (uint) (num14 - num13) };
                            Util.FileUtil.WriteData(fs, numArray22);
                            fs.Close();
                        }
                    }
                }
            }
        }

        public static void saveLayaAutoData()
        {
            layaAutoGameObjectsList.Clear();
            if (BatchMade && (Type == 1))
            {
                foreach (KeyValuePair<string, JSONObject> pair in getLayaAutoSpriteNode())
                {
                    string path = SAVEPATH + "/" + cleanIllegalChar(pair.Key, true) + ".lh";
                    if (!File.Exists(path) || CoverOriginalFile)
                    {
                        Util.FileUtil.saveFile(path, pair.Value);
                    }
                }
            }
            else
            {
                string str2 = "";
                if (Type == 0)
                {
                    str2 = SAVEPATH + "/" + sceneName + ".ls";
                }
                else if (Type == 1)
                {
                    str2 = SAVEPATH + "/" + sceneName + ".lh";
                }
                if (File.Exists(str2) && !CoverOriginalFile)
                {
                    return;
                }
                Util.FileUtil.saveFile(str2, getLayaAutoSceneNode());
            }
            Debug.Log(" -- Exporting Data is Finished -- ");
        }

        public static void saveLayaLmatData(Material material, string savePath)
        {
            JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            node.AddField("version", "LAYAMATERIAL:01");
            node.AddField("props", obj3);
            List<string> list = material.get_shaderKeywords().ToList<string>();
            string val = material.get_name();
            obj3.AddField("name", val);
            obj3.AddField("cull", material.GetInt("_Cull"));
            if (list.IndexOf("_ALPHABLEND_ON") != -1)
            {
                obj3.AddField("blend", 1);
            }
            else
            {
                obj3.AddField("blend", 0);
            }
            switch (material.GetInt("_SrcBlend"))
            {
                case 0:
                    obj3.AddField("srcBlend", 0);
                    break;

                case 1:
                    obj3.AddField("srcBlend", 1);
                    break;

                case 2:
                    obj3.AddField("srcBlend", 0x306);
                    break;

                case 3:
                    obj3.AddField("srcBlend", 0x300);
                    break;

                case 4:
                    obj3.AddField("srcBlend", 0x307);
                    break;

                case 5:
                    obj3.AddField("srcBlend", 770);
                    break;

                case 6:
                    obj3.AddField("srcBlend", 0x301);
                    break;

                case 7:
                    obj3.AddField("srcBlend", 0x304);
                    break;

                case 8:
                    obj3.AddField("srcBlend", 0x305);
                    break;

                case 9:
                    obj3.AddField("srcBlend", 0x308);
                    break;

                case 10:
                    obj3.AddField("srcBlend", 0x303);
                    break;

                default:
                    obj3.AddField("srcBlend", 1);
                    break;
            }
            switch (material.GetInt("_DstBlend"))
            {
                case 0:
                    obj3.AddField("dstBlend", 0);
                    break;

                case 1:
                    obj3.AddField("dstBlend", 1);
                    break;

                case 2:
                    obj3.AddField("dstBlend", 0x306);
                    break;

                case 3:
                    obj3.AddField("dstBlend", 0x300);
                    break;

                case 4:
                    obj3.AddField("dstBlend", 0x307);
                    break;

                case 5:
                    obj3.AddField("dstBlend", 770);
                    break;

                case 6:
                    obj3.AddField("dstBlend", 0x301);
                    break;

                case 7:
                    obj3.AddField("dstBlend", 0x304);
                    break;

                case 8:
                    obj3.AddField("dstBlend", 0x305);
                    break;

                case 9:
                    obj3.AddField("dstBlend", 0x308);
                    break;

                case 10:
                    obj3.AddField("dstBlend", 0x303);
                    break;

                default:
                    obj3.AddField("dstBlend", 0);
                    break;
            }
            if (list.IndexOf("_ALPHATEST_ON") != -1)
            {
                obj3.AddField("alphaTest", true);
            }
            else
            {
                obj3.AddField("alphaTest", false);
            }
            obj3.AddField("alphaTestValue", material.GetFloat("_Cutoff"));
            if (material.GetInt("_ZWrite") == 1)
            {
                obj3.AddField("depthWrite", true);
            }
            else
            {
                obj3.AddField("depthWrite", false);
            }
            switch (material.GetInt("_ZTest"))
            {
                case 0:
                    obj3.AddField("depthTest", 0);
                    break;

                case 1:
                    obj3.AddField("depthTest", 0x200);
                    break;

                case 2:
                    obj3.AddField("depthTest", 0x201);
                    break;

                case 3:
                    obj3.AddField("depthTest", 0x202);
                    break;

                case 4:
                    obj3.AddField("depthTest", 0x203);
                    break;

                case 5:
                    obj3.AddField("depthTest", 0x204);
                    break;

                case 6:
                    obj3.AddField("depthTest", 0x205);
                    break;

                case 7:
                    obj3.AddField("depthTest", 0x206);
                    break;

                case 8:
                    obj3.AddField("depthTest", 0x207);
                    break;

                default:
                    obj3.AddField("depthTest", 0);
                    break;
            }
            if (list.IndexOf("_ALPHABLEND_ON") != -1)
            {
                obj3.AddField("renderQueue", 2);
            }
            else
            {
                obj3.AddField("renderQueue", 1);
            }
            if (material.HasProperty("_AlbedoIntensity"))
            {
                obj3.AddField("albedoIntensity", material.GetFloat("_AlbedoIntensity"));
            }
            if (material.HasProperty("_Metallic"))
            {
                obj3.AddField("metallic", material.GetFloat("_Metallic"));
            }
            if (material.HasProperty("_Glossiness"))
            {
                obj3.AddField("smoothness", material.GetFloat("_Glossiness"));
            }
            if (material.HasProperty("_GlossMapScale"))
            {
                obj3.AddField("smoothnessTextureScale", material.GetFloat("_GlossMapScale"));
            }
            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                obj3.AddField("smoothnessSource", material.GetFloat("_SmoothnessTextureChannel"));
            }
            if (material.HasProperty("_BumpScale"))
            {
                obj3.AddField("normalTextureScale", material.GetFloat("_BumpScale"));
            }
            if (material.HasProperty("_Parallax"))
            {
                obj3.AddField("parallaxTextureScale", material.GetFloat("_Parallax"));
            }
            if (material.HasProperty("_OcclusionStrength"))
            {
                obj3.AddField("occlusionTextureStrength", material.GetFloat("_OcclusionStrength"));
            }
            if (material.HasProperty("_Emission"))
            {
                if (material.GetFloat("_Emission") == 1.0)
                {
                    obj3.AddField("enableEmission", true);
                }
                else
                {
                    obj3.AddField("enableEmission", false);
                }
            }
            if (material.HasProperty("_MainTex"))
            {
                Texture2D texture = material.GetTexture("_MainTex");
                if (texture != null)
                {
                    JSONObject obj13 = new JSONObject(JSONObject.Type.OBJECT);
                    obj13.AddField("name", "albedoTexture");
                    saveTextureFile(obj13, texture, savePath, val, "path");
                    obj4.Add(obj13);
                }
                JSONObject obj11 = new JSONObject(JSONObject.Type.OBJECT);
                obj11.AddField("name", "tilingOffset");
                JSONObject obj12 = new JSONObject(JSONObject.Type.ARRAY);
                Vector2 textureScale = material.GetTextureScale("_MainTex");
                Vector2 textureOffset = material.GetTextureOffset("_MainTex");
                obj12.Add(textureScale.x);
                obj12.Add(textureScale.y);
                obj12.Add(textureOffset.x);
                obj12.Add((float) (textureOffset.y * -1f));
                obj11.AddField("value", obj12);
                obj5.Add(obj11);
            }
            if (material.HasProperty("_MetallicGlossMap"))
            {
                Texture2D textured2 = material.GetTexture("_MetallicGlossMap");
                if (textured2 != null)
                {
                    JSONObject obj14 = new JSONObject(JSONObject.Type.OBJECT);
                    obj14.AddField("name", "metallicGlossTexture");
                    saveTextureFile(obj14, textured2, savePath, val, "path");
                    obj4.Add(obj14);
                }
            }
            if (material.HasProperty("_Lighting"))
            {
                if (material.GetFloat("_Lighting") == 0.0)
                {
                    obj3.AddField("enableLighting", true);
                }
                else
                {
                    obj3.AddField("enableLighting", false);
                }
            }
            if (!material.HasProperty("_Lighting") || (material.HasProperty("_Lighting") && (material.GetFloat("_Lighting") == 0.0)))
            {
                if (material.HasProperty("_Shininess"))
                {
                    obj3.AddField("shininess", material.GetFloat("_Shininess"));
                }
                if (material.HasProperty("_SpecGlossMap"))
                {
                    Texture2D textured3 = material.GetTexture("_SpecGlossMap");
                    if (textured3 != null)
                    {
                        JSONObject obj17 = new JSONObject(JSONObject.Type.OBJECT);
                        obj17.AddField("name", "specularTexture");
                        saveTextureFile(obj17, textured3, savePath, val, "path");
                        obj4.Add(obj17);
                    }
                }
                if (material.HasProperty("_BumpMap"))
                {
                    Texture2D textured4 = material.GetTexture("_BumpMap");
                    if (textured4 != null)
                    {
                        JSONObject obj18 = new JSONObject(JSONObject.Type.OBJECT);
                        obj18.AddField("name", "normalTexture");
                        saveTextureFile(obj18, textured4, savePath, val, "path");
                        obj4.Add(obj18);
                    }
                }
                JSONObject obj15 = new JSONObject(JSONObject.Type.OBJECT);
                obj15.AddField("name", "specularColor");
                JSONObject obj16 = new JSONObject(JSONObject.Type.ARRAY);
                if (material.HasProperty("_SpecColor"))
                {
                    Color color = material.GetColor("_SpecColor");
                    obj16.Add(color.r);
                    obj16.Add(color.g);
                    obj16.Add(color.b);
                    obj15.AddField("value", obj16);
                    obj5.Add(obj15);
                }
            }
            if (material.HasProperty("_ParallaxMap"))
            {
                Texture2D textured5 = material.GetTexture("_ParallaxMap");
                if (textured5 != null)
                {
                    JSONObject obj19 = new JSONObject(JSONObject.Type.OBJECT);
                    obj19.AddField("name", "parallaxTexture");
                    saveTextureFile(obj19, textured5, savePath, val, "path");
                    obj4.Add(obj19);
                }
            }
            if (material.HasProperty("_OcclusionMap"))
            {
                Texture2D textured6 = material.GetTexture("_OcclusionMap");
                if (textured6 != null)
                {
                    JSONObject obj20 = new JSONObject(JSONObject.Type.OBJECT);
                    obj20.AddField("name", "occlusionTexture");
                    saveTextureFile(obj20, textured6, savePath, val, "path");
                    obj4.Add(obj20);
                }
            }
            if (material.HasProperty("_EmissionMap"))
            {
                Texture2D textured7 = material.GetTexture("_EmissionMap");
                if (textured7 != null)
                {
                    JSONObject obj21 = new JSONObject(JSONObject.Type.OBJECT);
                    obj21.AddField("name", "emissionTexture");
                    saveTextureFile(obj21, textured7, savePath, val, "path");
                    obj4.Add(obj21);
                }
            }
            JSONObject obj7 = new JSONObject(JSONObject.Type.OBJECT);
            obj7.AddField("name", "albedoColor");
            JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
            if (material.HasProperty("_Color"))
            {
                Color color2 = material.GetColor("_Color");
                obj8.Add(color2.r);
                obj8.Add(color2.g);
                obj8.Add(color2.b);
                obj8.Add(color2.a);
                obj7.AddField("value", obj8);
                obj5.Add(obj7);
            }
            JSONObject obj9 = new JSONObject(JSONObject.Type.OBJECT);
            obj9.AddField("name", "emissionColor");
            JSONObject obj10 = new JSONObject(JSONObject.Type.ARRAY);
            if (material.HasProperty("_EmissionColor"))
            {
                Color color3 = material.GetColor("_EmissionColor");
                obj10.Add(color3.r);
                obj10.Add(color3.g);
                obj10.Add(color3.b);
                obj10.Add(color3.a);
                obj9.AddField("value", obj10);
                obj5.Add(obj9);
            }
            if ((material.GetInt("_Mode") == 2) || (material.GetInt("_Mode") == 3))
            {
                obj6.Add("ADDTIVEFOG");
            }
            obj3.AddField("textures", obj4);
            obj3.AddField("vectors", obj5);
            obj3.AddField("defines", obj6);
            Util.FileUtil.saveFile(savePath, node);
        }

        public static void saveLayaParticleLmatData(Material material, string savePath)
        {
            JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            node.AddField("version", "LAYAMATERIAL:01");
            node.AddField("props", obj3);
            List<string> list = material.get_shaderKeywords().ToList<string>();
            string val = material.get_name();
            obj3.AddField("name", val);
            obj3.AddField("cull", material.GetInt("_Cull"));
            if (list.IndexOf("_ALPHABLEND_ON") != -1)
            {
                obj3.AddField("blend", 1);
            }
            else
            {
                obj3.AddField("blend", 0);
            }
            switch (material.GetInt("_SrcBlend"))
            {
                case 0:
                    obj3.AddField("srcBlend", 0);
                    break;

                case 1:
                    obj3.AddField("srcBlend", 1);
                    break;

                case 2:
                    obj3.AddField("srcBlend", 0x306);
                    break;

                case 3:
                    obj3.AddField("srcBlend", 0x300);
                    break;

                case 4:
                    obj3.AddField("srcBlend", 0x307);
                    break;

                case 5:
                    obj3.AddField("srcBlend", 770);
                    break;

                case 6:
                    obj3.AddField("srcBlend", 0x301);
                    break;

                case 7:
                    obj3.AddField("srcBlend", 0x304);
                    break;

                case 8:
                    obj3.AddField("srcBlend", 0x305);
                    break;

                case 9:
                    obj3.AddField("srcBlend", 0x308);
                    break;

                case 10:
                    obj3.AddField("srcBlend", 0x303);
                    break;

                default:
                    obj3.AddField("srcBlend", 1);
                    break;
            }
            switch (material.GetInt("_DstBlend"))
            {
                case 0:
                    obj3.AddField("dstBlend", 0);
                    break;

                case 1:
                    obj3.AddField("dstBlend", 1);
                    break;

                case 2:
                    obj3.AddField("dstBlend", 0x306);
                    break;

                case 3:
                    obj3.AddField("dstBlend", 0x300);
                    break;

                case 4:
                    obj3.AddField("dstBlend", 0x307);
                    break;

                case 5:
                    obj3.AddField("dstBlend", 770);
                    break;

                case 6:
                    obj3.AddField("dstBlend", 0x301);
                    break;

                case 7:
                    obj3.AddField("dstBlend", 0x304);
                    break;

                case 8:
                    obj3.AddField("dstBlend", 0x305);
                    break;

                case 9:
                    obj3.AddField("dstBlend", 0x308);
                    break;

                case 10:
                    obj3.AddField("dstBlend", 0x303);
                    break;

                default:
                    obj3.AddField("dstBlend", 0);
                    break;
            }
            if (list.IndexOf("_ALPHATEST_ON") != -1)
            {
                obj3.AddField("alphaTest", true);
            }
            else
            {
                obj3.AddField("alphaTest", false);
            }
            if (material.GetInt("_ZWrite") == 1)
            {
                obj3.AddField("depthWrite", true);
            }
            else
            {
                obj3.AddField("depthWrite", false);
            }
            switch (material.GetInt("_ZTest"))
            {
                case 0:
                    obj3.AddField("depthTest", 0);
                    break;

                case 1:
                    obj3.AddField("depthTest", 0x200);
                    break;

                case 2:
                    obj3.AddField("depthTest", 0x201);
                    break;

                case 3:
                    obj3.AddField("depthTest", 0x202);
                    break;

                case 4:
                    obj3.AddField("depthTest", 0x203);
                    break;

                case 5:
                    obj3.AddField("depthTest", 0x204);
                    break;

                case 6:
                    obj3.AddField("depthTest", 0x205);
                    break;

                case 7:
                    obj3.AddField("depthTest", 0x206);
                    break;

                case 8:
                    obj3.AddField("depthTest", 0x207);
                    break;

                default:
                    obj3.AddField("depthTest", 0);
                    break;
            }
            if (list.IndexOf("_ALPHABLEND_ON") != -1)
            {
                obj3.AddField("renderQueue", 2);
            }
            else
            {
                obj3.AddField("renderQueue", 1);
            }
            JSONObject obj7 = new JSONObject(JSONObject.Type.OBJECT);
            obj7.AddField("name", "diffuseTexture");
            JSONObject obj8 = new JSONObject(JSONObject.Type.OBJECT);
            obj8.AddField("name", "tilingOffset");
            if (material.HasProperty("_MainTex"))
            {
                Texture2D texture = material.GetTexture("_MainTex");
                saveTextureFile(obj7, texture, savePath, val, "path");
                obj4.Add(obj7);
                JSONObject obj11 = new JSONObject(JSONObject.Type.ARRAY);
                Vector2 textureScale = material.GetTextureScale("_MainTex");
                Vector2 textureOffset = material.GetTextureOffset("_MainTex");
                obj11.Add(textureScale.x);
                obj11.Add(textureScale.y);
                obj11.Add(textureOffset.x);
                obj11.Add((float) (textureOffset.y * -1f));
                obj8.AddField("value", obj11);
                obj5.Add(obj8);
            }
            JSONObject obj9 = new JSONObject(JSONObject.Type.OBJECT);
            obj9.AddField("name", "tintColor");
            JSONObject obj10 = new JSONObject(JSONObject.Type.ARRAY);
            if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color");
                obj10.Add(color.r);
                obj10.Add(color.g);
                obj10.Add(color.b);
                obj10.Add(color.a);
                obj9.AddField("value", obj10);
                obj5.Add(obj9);
            }
            if ((material.GetInt("_Mode") == 2) || (material.GetInt("_Mode") == 3))
            {
                obj6.Add("ADDTIVEFOG");
            }
            obj3.AddField("textures", obj4);
            obj3.AddField("vectors", obj5);
            obj3.AddField("defines", obj6);
            Util.FileUtil.saveFile(savePath, node);
        }

        public static void saveLayaSkyBoxData(Material material, string savePath)
        {
            JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
            node.AddField("version", "LAYAMATERIAL:01");
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            node.AddField("props", obj3);
            if (material.HasProperty("_Exposure"))
            {
                obj3.AddField("exposure", material.GetFloat("_Exposure"));
            }
            if (material.HasProperty("_Rotation"))
            {
                obj3.AddField("rotation", material.GetFloat("_Rotation"));
            }
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.AddField("vectors", obj4);
            JSONObject obj5 = new JSONObject(JSONObject.Type.OBJECT);
            obj5.AddField("name", "tintColor");
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            if (material.HasProperty("_Tint"))
            {
                Color color = material.GetColor("_Tint");
                obj6.Add(color.r);
                obj6.Add(color.g);
                obj6.Add(color.b);
                obj6.Add(color.a);
                obj5.AddField("value", obj6);
                obj4.Add(obj5);
            }
            JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
            obj3.AddField("textures", obj7);
            JSONObject obj8 = new JSONObject(JSONObject.Type.OBJECT);
            obj7.Add(obj8);
            new JSONObject(JSONObject.Type.OBJECT);
            obj8.AddField("name", "textureCube");
            char[] separator = new char[] { '.' };
            string str = cleanIllegalChar(UnityEditor.AssetDatabase.GetAssetPath(material.GetInstanceID()).Split(separator)[0], false) + ".ltc";
            string path = SAVEPATH + "/" + str;
            if (!File.Exists(path) || CoverOriginalFile)
            {
                saveTextureCubeFile(material, obj8, path);
            }
            Util.FileUtil.saveFile(savePath, node);
        }

        public static void saveLightMapFile(JSONObject customProps)
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            customProps.AddField("lightmaps", obj2);
            LightmapData[] dataArray = LightmapSettings.get_lightmaps();
            if ((dataArray != null) && (dataArray.Length != 0))
            {
                for (int i = 0; i < dataArray.Length; i++)
                {
                    Texture2D textured = dataArray[i].get_lightmapColor();
                    if (textured != null)
                    {
                        string assetPath = UnityEditor.AssetDatabase.GetAssetPath(textured.GetInstanceID());
                        string path = SAVEPATH + "/" + Path.GetDirectoryName(assetPath);
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        string str = SAVEPATH + "/" + assetPath;
                        if (ConvertLightMap)
                        {
                            if (ConvertToPNG)
                            {
                                char[] separator = new char[] { '.' };
                                str = str.Split(separator)[0] + ".png";
                            }
                            else if (ConvertToJPG)
                            {
                                char[] chArray2 = new char[] { '.' };
                                str = str.Split(chArray2)[0] + ".jpg";
                            }
                            str = cleanIllegalChar(str, false);
                            if (!File.Exists(str) || CoverOriginalFile)
                            {
                                UnityEditor.TextureImporter atPath = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                                atPath.isReadable = true;
                                atPath.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
                                UnityEditor.AssetDatabase.ImportAsset(assetPath);
                                FileStream output = File.Open(str, System.IO.FileMode.Create, FileAccess.ReadWrite);
                                BinaryWriter writer = new BinaryWriter(output);
                                if (ConvertToPNG)
                                {
                                    writer.Write(ImageConversion.EncodeToPNG(textured));
                                }
                                else if (ConvertToJPG)
                                {
                                    writer.Write(ImageConversion.EncodeToJPG(textured, (int) ConvertQuality));
                                }
                                else
                                {
                                    writer.Write(ImageConversion.EncodeToPNG(textured));
                                }
                                output.Close();
                            }
                            if (ConvertToPNG)
                            {
                                char[] chArray3 = new char[] { '.' };
                                obj2.Add(assetPath.Split(chArray3)[0] + ".png");
                            }
                            else if (ConvertToJPG)
                            {
                                char[] chArray4 = new char[] { '.' };
                                obj2.Add(assetPath.Split(chArray4)[0] + ".jpg");
                            }
                        }
                        else
                        {
                            str = cleanIllegalChar(SAVEPATH + "/" + assetPath, false);
                            if (!File.Exists(str) || CoverOriginalFile)
                            {
                                File.Copy(assetPath, str, true);
                            }
                            obj2.Add(assetPath);
                        }
                    }
                }
            }
        }

        public static void saveLmatFile(Material material, string savePath, ComponentType type)
        {
            JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            node.AddField("version", "LAYAMATERIAL:01");
            node.AddField("props", obj3);
            string val = material.get_name();
            obj3.AddField("name", val);
            string str2 = material.get_shader().get_name();
            string str3 = "Legacy Shaders/Transparent/Cutout/";
            int num = 1;
            switch (str2)
            {
                case "Particles/Additive":
                case "Mobile/Particles/Additive":
                case "Particles/Additive (Soft)":
                    num = 8;
                    break;

                case "Particles/Alpha Blended":
                case "Mobile/Particles/Alpha Blended":
                    num = 6;
                    break;

                case "Standard":
                case "Standard (Specular setup)":
                    if (material.get_shaderKeywords().Length == 0)
                    {
                        num = 1;
                    }
                    else if (((material.get_shaderKeywords()[0] == "_EMISSION") && (material.GetInt("_SrcBlend") == 1)) && ((material.GetInt("_DstBlend") == 0) && (material.GetInt("_ZWrite") == 1)))
                    {
                        num = 1;
                    }
                    else if (((material.get_shaderKeywords()[0] == "_ALPHATEST_ON") && (material.GetInt("_SrcBlend") == 1)) && ((material.GetInt("_DstBlend") == 0) && (material.GetInt("_ZWrite") == 1)))
                    {
                        num = 3;
                    }
                    else if (((material.get_shaderKeywords()[0] == "_ALPHABLEND_ON") && (material.GetInt("_SrcBlend") == 5)) && ((material.GetInt("_DstBlend") == 10) && (material.GetInt("_ZWrite") == 0)))
                    {
                        num = 5;
                    }
                    else if (((material.get_shaderKeywords()[0] == "_ALPHAPREMULTIPLY_ON") && (material.GetInt("_SrcBlend") == 1)) && ((material.GetInt("_DstBlend") == 10) && (material.GetInt("_ZWrite") == 0)))
                    {
                        num = 13;
                    }
                    break;

                default:
                    if (str2.IndexOf(str3) > -1)
                    {
                        num = 3;
                    }
                    else if ((str2 == "Mobile/Diffuse") || (str2 == "Legacy Shaders/Diffuse"))
                    {
                        num = 1;
                    }
                    else
                    {
                        num = 1;
                    }
                    break;
            }
            switch (num)
            {
                case 1:
                    obj3.AddField("cull", 2);
                    obj3.AddField("blend", 0);
                    obj3.AddField("srcBlend", 1);
                    obj3.AddField("dstBlend", 0);
                    obj3.AddField("alphaTest", false);
                    obj3.AddField("depthWrite", true);
                    obj3.AddField("renderQueue", 1);
                    break;

                case 3:
                    obj3.AddField("cull", 0);
                    obj3.AddField("blend", 0);
                    obj3.AddField("srcBlend", 1);
                    obj3.AddField("dstBlend", 0);
                    obj3.AddField("alphaTest", true);
                    obj3.AddField("depthWrite", true);
                    obj3.AddField("renderQueue", 1);
                    break;

                case 5:
                    obj3.AddField("cull", 0);
                    obj3.AddField("blend", 1);
                    obj3.AddField("srcBlend", 770);
                    obj3.AddField("dstBlend", 0x303);
                    obj3.AddField("alphaTest", false);
                    obj3.AddField("depthWrite", false);
                    obj3.AddField("renderQueue", 2);
                    break;

                case 6:
                    obj3.AddField("cull", 0);
                    obj3.AddField("blend", 1);
                    obj3.AddField("srcBlend", 770);
                    obj3.AddField("dstBlend", 0x303);
                    obj3.AddField("alphaTest", false);
                    obj3.AddField("depthWrite", false);
                    obj3.AddField("renderQueue", 2);
                    break;

                case 8:
                    obj3.AddField("cull", 0);
                    obj3.AddField("blend", 1);
                    obj3.AddField("srcBlend", 770);
                    obj3.AddField("dstBlend", 1);
                    obj3.AddField("alphaTest", false);
                    obj3.AddField("depthWrite", false);
                    obj3.AddField("renderQueue", 2);
                    obj6.Add("ADDTIVEFOG");
                    break;

                case 13:
                    obj3.AddField("cull", 2);
                    obj3.AddField("blend", 1);
                    obj3.AddField("srcBlend", 770);
                    obj3.AddField("dstBlend", 0x303);
                    obj3.AddField("alphaTest", false);
                    obj3.AddField("depthWrite", true);
                    obj3.AddField("renderQueue", 2);
                    break;

                default:
                    obj3.AddField("cull", 2);
                    obj3.AddField("blend", 0);
                    obj3.AddField("srcBlend", 1);
                    obj3.AddField("dstBlend", 0);
                    obj3.AddField("alphaTest", false);
                    obj3.AddField("depthWrite", true);
                    obj3.AddField("renderQueue", 1);
                    break;
            }
            obj3.AddField("textures", obj4);
            obj3.AddField("vectors", obj5);
            obj3.AddField("defines", obj6);
            JSONObject obj7 = new JSONObject(JSONObject.Type.OBJECT);
            obj7.AddField("name", "diffuseTexture");
            if (material.HasProperty("_MainTex"))
            {
                Texture2D texture = material.GetTexture("_MainTex");
                saveTextureFile(obj7, texture, savePath, val, "path");
            }
            else
            {
                obj7.AddField("path", "");
            }
            obj4.Add(obj7);
            if (type == ComponentType.ParticleSystem)
            {
                JSONObject obj8 = new JSONObject(JSONObject.Type.OBJECT);
                obj8.AddField("name", "tintColor");
                JSONObject obj9 = new JSONObject(JSONObject.Type.ARRAY);
                if (material.HasProperty("_TintColor"))
                {
                    Color color = material.GetColor("_TintColor");
                    obj9.Add(color.r);
                    obj9.Add(color.g);
                    obj9.Add(color.b);
                    obj9.Add(color.a);
                }
                else
                {
                    obj9.Add((float) 0.5f);
                    obj9.Add((float) 0.5f);
                    obj9.Add((float) 0.5f);
                    obj9.Add((float) 0.5f);
                }
                obj8.AddField("value", obj9);
                obj5.Add(obj8);
            }
            else
            {
                JSONObject obj10 = new JSONObject(JSONObject.Type.OBJECT);
                obj10.AddField("name", "normalTexture");
                if (material.HasProperty("_BumpMap"))
                {
                    Texture2D textured2 = material.GetTexture("_BumpMap");
                    saveTextureFile(obj10, textured2, savePath, val, "path");
                }
                else
                {
                    obj10.AddField("path", "");
                }
                obj4.Add(obj10);
                JSONObject obj11 = new JSONObject(JSONObject.Type.OBJECT);
                obj11.AddField("name", "specularTexture");
                if (material.HasProperty("_SpecGlossMap"))
                {
                    Texture2D textured3 = material.GetTexture("_SpecGlossMap");
                    saveTextureFile(obj11, textured3, savePath, val, "path");
                }
                else
                {
                    obj11.AddField("path", "");
                }
                obj4.Add(obj11);
                JSONObject obj12 = new JSONObject(JSONObject.Type.OBJECT);
                obj12.AddField("name", "emissiveTexture");
                if (material.HasProperty("_EmissionMap"))
                {
                    Texture2D textured4 = material.GetTexture("_EmissionMap");
                    saveTextureFile(obj12, textured4, savePath, val, "path");
                }
                else
                {
                    obj12.AddField("path", "");
                }
                obj4.Add(obj12);
                JSONObject obj13 = new JSONObject(JSONObject.Type.OBJECT);
                obj13.AddField("name", "ambientColor");
                JSONObject obj14 = new JSONObject(JSONObject.Type.ARRAY);
                obj14.Add((float) 0f);
                obj14.Add((float) 0f);
                obj14.Add((float) 0f);
                obj13.AddField("value", obj14);
                obj5.Add(obj13);
                JSONObject obj15 = new JSONObject(JSONObject.Type.OBJECT);
                obj15.AddField("name", "albedo");
                JSONObject obj16 = new JSONObject(JSONObject.Type.ARRAY);
                if (material.HasProperty("_Color"))
                {
                    Color color2 = material.GetColor("_Color");
                    obj16.Add(color2.r);
                    obj16.Add(color2.g);
                    obj16.Add(color2.b);
                    obj16.Add(color2.a);
                }
                else if (material.HasProperty("_TintColor"))
                {
                    Color color3 = material.GetColor("_TintColor");
                    obj16.Add((float) (color3.r * 2f));
                    obj16.Add((float) (color3.g * 2f));
                    obj16.Add((float) (color3.b * 2f));
                    obj16.Add((float) (color3.a * 2f));
                }
                else
                {
                    obj16.Add((float) 1f);
                    obj16.Add((float) 1f);
                    obj16.Add((float) 1f);
                    obj16.Add((float) 1f);
                }
                obj15.AddField("value", obj16);
                obj5.Add(obj15);
                JSONObject obj17 = new JSONObject(JSONObject.Type.OBJECT);
                obj17.AddField("name", "diffuseColor");
                JSONObject obj18 = new JSONObject(JSONObject.Type.ARRAY);
                if (material.HasProperty("_DiffuseColor"))
                {
                    Color color4 = material.GetColor("_DiffuseColor");
                    obj18.Add(color4.r);
                    obj18.Add(color4.g);
                    obj18.Add(color4.b);
                }
                else
                {
                    obj18.Add((float) 1f);
                    obj18.Add((float) 1f);
                    obj18.Add((float) 1f);
                }
                obj17.AddField("value", obj18);
                obj5.Add(obj17);
                JSONObject obj19 = new JSONObject(JSONObject.Type.OBJECT);
                obj19.AddField("name", "specularColor");
                JSONObject obj20 = new JSONObject(JSONObject.Type.ARRAY);
                if (material.HasProperty("_SpecColor"))
                {
                    Color color5 = material.GetColor("_SpecColor");
                    obj20.Add(color5.r);
                    obj20.Add(color5.g);
                    obj20.Add(color5.b);
                    obj20.Add(color5.a);
                }
                else
                {
                    obj20.Add((float) 1f);
                    obj20.Add((float) 1f);
                    obj20.Add((float) 1f);
                    obj20.Add((float) 8f);
                }
                obj19.AddField("value", obj20);
                obj5.Add(obj19);
                JSONObject obj21 = new JSONObject(JSONObject.Type.OBJECT);
                obj21.AddField("name", "emissionColor");
                JSONObject obj22 = new JSONObject(JSONObject.Type.ARRAY);
                if (material.HasProperty("_EmissionColor"))
                {
                    Color color6 = material.GetColor("_EmissionColor");
                    obj22.Add(color6.r);
                    obj22.Add(color6.g);
                    obj22.Add(color6.b);
                }
                else
                {
                    obj22.Add((float) 0f);
                    obj22.Add((float) 0f);
                    obj22.Add((float) 0f);
                }
                obj21.AddField("value", obj22);
                obj5.Add(obj21);
            }
            Util.FileUtil.saveFile(savePath, node);
        }

        public static void saveLmFile(Mesh mesh, string savePath)
        {
            int num;
            int num2;
            string item = cleanIllegalChar(mesh.get_name(), true);
            ushort num3 = (ushort) mesh.get_subMeshCount();
            int num4 = num3 + 1;
            FileStream fs = Util.FileUtil.saveFile(savePath, null);
            ushort num5 = 0;
            string str2 = "";
            for (num = 0; num < VertexStructure.Length; num++)
            {
                VertexStructure[num] = 0;
            }
            Vector3[] vectorArray = mesh.get_vertices();
            Vector3[] vectorArray2 = mesh.get_normals();
            Color[] colorArray = mesh.get_colors();
            Vector2[] vectorArray3 = mesh.get_uv();
            Vector2[] vectorArray4 = mesh.get_uv2();
            Vector4[] vectorArray5 = mesh.get_tangents();
            if ((vectorArray != null) && (vectorArray.Length != 0))
            {
                VertexStructure[0] = 1;
                str2 = str2 + "POSITION";
                num5 = (ushort) (num5 + 12);
            }
            if ((vectorArray2 != null) && (vectorArray2.Length != 0))
            {
                VertexStructure[1] = 1;
                str2 = str2 + ",NORMAL";
                num5 = (ushort) (num5 + 12);
            }
            if (((colorArray != null) && (colorArray.Length != 0)) && !IgnoreColor)
            {
                VertexStructure[2] = 1;
                str2 = str2 + ",COLOR";
                num5 = (ushort) (num5 + 0x10);
            }
            if ((vectorArray3 != null) && (vectorArray3.Length != 0))
            {
                VertexStructure[3] = 1;
                str2 = str2 + ",UV";
                num5 = (ushort) (num5 + 8);
            }
            if ((vectorArray4 != null) && (vectorArray4.Length != 0))
            {
                VertexStructure[4] = 1;
                str2 = str2 + ",UV1";
                num5 = (ushort) (num5 + 8);
            }
            if (((vectorArray5 != null) && (vectorArray5.Length != 0)) && !IgnoreTangent)
            {
                VertexStructure[6] = 1;
                str2 = str2 + ",TANGENT";
                num5 = (ushort) (num5 + 0x10);
            }
            int[] numArray = new int[num3];
            int[] numArray2 = new int[num3];
            for (num = 0; num < num3; num++)
            {
                int[] indices = mesh.GetIndices(num);
                numArray[num] = indices[0];
                numArray2[num] = indices.Length;
            }
            long num6 = 0L;
            long num7 = 0L;
            long num8 = 0L;
            long num9 = 0L;
            long num10 = 0L;
            long num11 = 0L;
            long num12 = 0L;
            long num13 = 0L;
            long num14 = 0L;
            long num15 = 0L;
            long num16 = 0L;
            long num17 = 0L;
            long num18 = 0L;
            long[] numArray3 = new long[num3];
            long[] numArray4 = new long[num3];
            long[] numArray5 = new long[num3];
            List<string> list = new List<string> { "MESH", "SUBMESH" };
            string data = "LAYAMODEL:0301";
            Util.FileUtil.WriteData(fs, data);
            long position = fs.Position;
            num6 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            num11 = fs.Position;
            ushort[] datas = new ushort[] { (ushort) num4 };
            Util.FileUtil.WriteData(fs, datas);
            for (num = 0; num < num4; num++)
            {
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
            }
            num12 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new ushort[1]);
            num7 = fs.Position;
            ushort[] numArray8 = new ushort[] { (ushort) list.IndexOf("MESH") };
            Util.FileUtil.WriteData(fs, numArray8);
            list.Add(item);
            ushort[] numArray9 = new ushort[] { (ushort) list.IndexOf(item) };
            Util.FileUtil.WriteData(fs, numArray9);
            ushort[] numArray10 = new ushort[] { 1 };
            Util.FileUtil.WriteData(fs, numArray10);
            num9 = fs.Position;
            for (num = 0; num < 1; num++)
            {
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                list.Add(str2);
                ushort[] numArray11 = new ushort[] { (ushort) list.IndexOf(str2) };
                Util.FileUtil.WriteData(fs, numArray11);
            }
            num10 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            long num24 = fs.Position;
            Util.FileUtil.WriteData(fs, new ushort[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            num8 = fs.Position - num7;
            for (num = 0; num < num3; num++)
            {
                numArray3[num] = fs.Position;
                ushort[] numArray12 = new ushort[] { (ushort) list.IndexOf("SUBMESH") };
                Util.FileUtil.WriteData(fs, numArray12);
                Util.FileUtil.WriteData(fs, new ushort[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                ushort[] numArray13 = new ushort[] { 1 };
                Util.FileUtil.WriteData(fs, numArray13);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                numArray4[num] = fs.Position;
                numArray5[num] = numArray4[num] - numArray3[num];
            }
            num13 = fs.Position;
            for (num = 0; num < list.Count; num++)
            {
                Util.FileUtil.WriteData(fs, list[num]);
            }
            num14 = fs.Position - num13;
            num15 = fs.Position;
            for (num2 = 0; num2 < mesh.get_vertexCount(); num2++)
            {
                Vector3 vector = vectorArray[num2];
                float[] singleArray1 = new float[] { vector.x * -1f, vector.y, vector.z };
                Util.FileUtil.WriteData(fs, singleArray1);
                if (VertexStructure[1] == 1)
                {
                    Vector3 vector2 = vectorArray2[num2];
                    float[] singleArray2 = new float[] { vector2.x * -1f, vector2.y, vector2.z };
                    Util.FileUtil.WriteData(fs, singleArray2);
                }
                if (VertexStructure[2] == 1)
                {
                    Color color = colorArray[num2];
                    float[] singleArray3 = new float[] { color.r, color.g, color.b, color.a };
                    Util.FileUtil.WriteData(fs, singleArray3);
                }
                if (VertexStructure[3] == 1)
                {
                    Vector2 vector3 = vectorArray3[num2];
                    float[] singleArray4 = new float[] { vector3.x, (vector3.y * -1f) + 1f };
                    Util.FileUtil.WriteData(fs, singleArray4);
                }
                if (VertexStructure[4] == 1)
                {
                    Vector2 vector4 = vectorArray4[num2];
                    float[] singleArray5 = new float[] { vector4.x, vector4.y * -1f };
                    Util.FileUtil.WriteData(fs, singleArray5);
                }
                if (VertexStructure[6] == 1)
                {
                    Vector4 vector5 = vectorArray5[num2];
                    float[] singleArray6 = new float[] { vector5.x * -1f, vector5.y, vector5.z, vector5.w };
                    Util.FileUtil.WriteData(fs, singleArray6);
                }
            }
            num16 = fs.Position - num15;
            num17 = fs.Position;
            int[] numArray6 = mesh.get_triangles();
            for (num2 = 0; num2 < numArray6.Length; num2++)
            {
                ushort[] numArray14 = new ushort[] { (ushort) numArray6[num2] };
                Util.FileUtil.WriteData(fs, numArray14);
            }
            num18 = fs.Position - num17;
            uint num19 = 0;
            uint num20 = 0;
            uint num21 = 0;
            uint num22 = 0;
            uint num23 = 0;
            for (num = 0; num < num3; num++)
            {
                fs.Position = numArray3[num] + 4L;
                if (num3 == 1)
                {
                    num19 = 0;
                    num20 = (uint) (num16 / ((ulong) num5));
                    num21 = 0;
                    num22 = (uint) (num18 / 2L);
                }
                else if (num == 0)
                {
                    num19 = 0;
                    num20 = (uint) numArray[num + 1];
                    num21 = num23;
                    num22 = (uint) numArray2[num];
                }
                else if (num < (num3 - 1))
                {
                    num19 = (uint) numArray[num];
                    num20 = (uint) (numArray[num + 1] - numArray[num]);
                    num21 = num23;
                    num22 = (uint) numArray2[num];
                }
                else
                {
                    num19 = (uint) numArray[num];
                    num20 = (uint) ((num16 / ((ulong) num5)) - numArray[num]);
                    num21 = num23;
                    num22 = (uint) numArray2[num];
                }
                uint[] numArray15 = new uint[] { num19 };
                Util.FileUtil.WriteData(fs, numArray15);
                uint[] numArray16 = new uint[] { num20 };
                Util.FileUtil.WriteData(fs, numArray16);
                uint[] numArray17 = new uint[] { num21 };
                Util.FileUtil.WriteData(fs, numArray17);
                uint[] numArray18 = new uint[] { num22 };
                Util.FileUtil.WriteData(fs, numArray18);
                num23 += num22;
                fs.Position += 2L;
                uint[] numArray19 = new uint[] { num21 };
                Util.FileUtil.WriteData(fs, numArray19);
                uint[] numArray20 = new uint[] { num22 };
                Util.FileUtil.WriteData(fs, numArray20);
            }
            fs.Position = num9;
            uint[] numArray21 = new uint[] { (uint) (num15 - num13) };
            Util.FileUtil.WriteData(fs, numArray21);
            uint[] numArray22 = new uint[] { (uint) num16 };
            Util.FileUtil.WriteData(fs, numArray22);
            fs.Position = num10;
            uint[] numArray23 = new uint[] { (uint) (num17 - num13) };
            Util.FileUtil.WriteData(fs, numArray23);
            uint[] numArray24 = new uint[] { (uint) num18 };
            Util.FileUtil.WriteData(fs, numArray24);
            fs.Position = num12;
            Util.FileUtil.WriteData(fs, new uint[1]);
            ushort[] numArray25 = new ushort[] { (ushort) list.Count };
            Util.FileUtil.WriteData(fs, numArray25);
            long num25 = fs.Position;
            fs.Position = num11 + 2L;
            uint[] numArray26 = new uint[] { (uint) num7 };
            Util.FileUtil.WriteData(fs, numArray26);
            uint[] numArray27 = new uint[] { (uint) num8 };
            Util.FileUtil.WriteData(fs, numArray27);
            for (num = 0; num < num3; num++)
            {
                uint[] numArray28 = new uint[] { (uint) numArray3[num] };
                Util.FileUtil.WriteData(fs, numArray28);
                uint[] numArray29 = new uint[] { (uint) numArray5[num] };
                Util.FileUtil.WriteData(fs, numArray29);
            }
            fs.Position = num6;
            uint[] numArray30 = new uint[] { (uint) num13 };
            Util.FileUtil.WriteData(fs, numArray30);
            uint[] numArray31 = new uint[] { (uint) ((((num13 + num14) + num16) + num18) + numArray5[0]) };
            Util.FileUtil.WriteData(fs, numArray31);
            fs.Close();
        }

        public static void saveLsaniData(GameObject gameObject)
        {
            Transform transform = gameObject.get_transform();
            Animation component = gameObject.GetComponent<Animation>();
            List<Transform> list = new List<Transform> {
                transform
            };
            List<Transform> list2 = new List<Transform> {
                transform
            };
            foreach (Transform transform2 in component.get_gameObject().GetComponentsInChildren<Transform>())
            {
                if ((list.IndexOf(transform2) == -1) && (componentsOnGameObject(transform2.get_gameObject()).Count <= 1))
                {
                    list.Add(transform2);
                    list2.Add(transform2);
                }
            }
            if (SimplifyBone)
            {
                List<Transform> list4 = new List<Transform>();
                foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    for (int m = 0; m < renderer.get_bones().Length; m++)
                    {
                        Transform item = renderer.get_bones()[m];
                        if (list4.IndexOf(item) == -1)
                        {
                            list4.Add(item);
                        }
                    }
                }
                List<Transform> list5 = new List<Transform>();
                for (int j = 0; j < list.Count; j++)
                {
                    list5.Add(list[j]);
                }
                for (int k = 0; k < list5.Count; k++)
                {
                    Transform bone = list5[k];
                    for (int n = 0; n < list4.Count; n++)
                    {
                        Transform skinBone = list4[n];
                        if (checkChildBoneIsLegal(gameObject.get_transform(), bone, skinBone, list5.Count))
                        {
                            break;
                        }
                        if (n == (list4.Count - 1))
                        {
                            list.Remove(bone);
                        }
                    }
                }
            }
            List<int> list3 = new List<int> { -1 };
            for (int i = 1; i < list.Count; i++)
            {
                list3.Add(list.IndexOf(list[i].get_parent().get_transform()));
            }
            foreach (AnimationState state in component)
            {
                List<string> list6 = new List<string> { "ANIMATIONS" };
                for (int num8 = 0; num8 < list.Count; num8++)
                {
                    list6.Add(list[num8].get_name());
                }
                AnimationClip clip = state.get_clip();
                if (clip != null)
                {
                    gameObject.get_name();
                    int num9 = (int) state.get_clip().get_frameRate();
                    string str = clip.get_name();
                    list6.Add(str);
                    string assetPath = UnityEditor.AssetDatabase.GetAssetPath(clip.GetInstanceID());
                    string[] textArray1 = new string[6];
                    textArray1[0] = SAVEPATH;
                    textArray1[1] = "/";
                    char[] separator = new char[] { '.' };
                    textArray1[2] = assetPath.Split(separator)[0];
                    textArray1[3] = "-";
                    textArray1[4] = str;
                    textArray1[5] = ".lsani";
                    string path = string.Concat(textArray1);
                    if (!File.Exists(path) || CoverOriginalFile)
                    {
                        List<int> list7 = new List<int>();
                        UnityEditor.EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);
                        AnimationCurve[] curveArray = new AnimationCurve[curveBindings.Length];
                        for (int num22 = 0; num22 < curveBindings.Length; num22++)
                        {
                            curveArray[num22] = UnityEditor.AnimationUtility.GetEditorCurve(clip, curveBindings[num22]);
                        }
                        AnimationCurve[] curveArray2 = curveArray;
                        for (int num23 = 0; num23 < curveArray2.Length; num23++)
                        {
                            foreach (Keyframe keyframe in curveArray2[num23].get_keys())
                            {
                                int num25 = (int) Math.Floor((double) (keyframe.get_time() * num9));
                                if (list7.IndexOf(num25) == -1)
                                {
                                    list7.Add(num25);
                                }
                            }
                        }
                        list7.Sort();
                        Dictionary<int, List<FrameData>> dictionary = new Dictionary<int, List<FrameData>>();
                        Dictionary<int, List<float>> dictionary2 = new Dictionary<int, List<float>>();
                        state.set_enabled(true);
                        state.set_weight(1f);
                        state.set_speed(1f);
                        int num10 = list7[list7.Count - 1];
                        float num11 = (num10 * 1000f) / 30f;
                        float num12 = 33.33333f;
                        Matrix4x4 matrixx = new Matrix4x4();
                        for (int num26 = 0; num26 < list7.Count; num26++)
                        {
                            int num27 = list7[num26];
                            state.set_normalizedTime(((float) num27) / ((float) num10));
                            component.Sample();
                            foreach (Transform transform6 in list2.ToArray())
                            {
                                if (list.IndexOf(transform6) != -1)
                                {
                                    FrameData data;
                                    matrixx.SetTRS(new Vector3(0f, 0f, 0f), transform6.get_localRotation(), new Vector3(1f, 1f, 1f));
                                    matrixx = matrixx.get_inverse();
                                    Vector4 column = matrixx.GetColumn(2);
                                    Quaternion quaternion = Quaternion.LookRotation(column, matrixx.GetColumn(1));
                                    data.localPosition = quaternion * transform6.get_localPosition();
                                    data.localRotation = transform6.get_localRotation();
                                    data.localScale = transform6.get_localScale();
                                    if (num26 == 0)
                                    {
                                        List<FrameData> list8 = new List<FrameData>();
                                        List<float> list9 = new List<float> {
                                            data,
                                            0f
                                        };
                                        dictionary.Add(list.IndexOf(transform6), list8);
                                        dictionary2.Add(list.IndexOf(transform6), list9);
                                    }
                                    foreach (KeyValuePair<int, List<FrameData>> pair in dictionary)
                                    {
                                        if (pair.Key == list.IndexOf(transform6))
                                        {
                                            pair.Value.Add(data);
                                            dictionary2[list.IndexOf(transform6)].Add(num27 * num12);
                                        }
                                    }
                                    if (num26 == (list7.Count - 1))
                                    {
                                        foreach (KeyValuePair<int, List<FrameData>> pair2 in dictionary)
                                        {
                                            if (pair2.Key == list.IndexOf(transform6))
                                            {
                                                List<FrameData> list10 = pair2.Value;
                                                List<float> list11 = dictionary2[list.IndexOf(transform6)];
                                                if (num11 != list11[list11.Count - 1])
                                                {
                                                    list10.Add(data);
                                                    list11.Add(num11);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        foreach (KeyValuePair<int, List<FrameData>> pair3 in dictionary)
                        {
                            int key = pair3.Key;
                            List<FrameData> list12 = pair3.Value;
                            List<float> list13 = dictionary2[key];
                            for (int num30 = 1; num30 < (list12.Count - 1); num30++)
                            {
                                if (frameDataIsEquals(list12[num30], list12[num30 - 1]) && frameDataIsEquals(list12[num30], list12[num30 + 1]))
                                {
                                    list12.RemoveAt(num30);
                                    list13.RemoveAt(num30);
                                }
                            }
                        }
                        state.set_enabled(false);
                        state.set_normalizedTime(0f);
                        component.Sample();
                        FileStream fs = Util.FileUtil.saveFile(path, null);
                        long position = 0L;
                        long num14 = 0L;
                        long num15 = 0L;
                        long num16 = 0L;
                        long num17 = 0L;
                        long num18 = 0L;
                        long num19 = 0L;
                        string str4 = "LAYAANIMATION:02";
                        Util.FileUtil.WriteData(fs, str4);
                        position = fs.Position;
                        Util.FileUtil.WriteData(fs, new uint[1]);
                        Util.FileUtil.WriteData(fs, new uint[1]);
                        num14 = fs.Position;
                        int num20 = 1;
                        ushort[] datas = new ushort[] { (ushort) num20 };
                        Util.FileUtil.WriteData(fs, datas);
                        for (int num31 = 0; num31 < num20; num31++)
                        {
                            Util.FileUtil.WriteData(fs, new uint[1]);
                            Util.FileUtil.WriteData(fs, new uint[1]);
                        }
                        long num1 = fs.Position;
                        Util.FileUtil.WriteData(fs, new uint[1]);
                        ushort[] numArray2 = new ushort[] { (ushort) list6.Count };
                        Util.FileUtil.WriteData(fs, numArray2);
                        num15 = fs.Position;
                        ushort[] numArray3 = new ushort[] { (ushort) list6.IndexOf("ANIMATIONS") };
                        Util.FileUtil.WriteData(fs, numArray3);
                        ushort[] numArray4 = new ushort[] { 10 };
                        Util.FileUtil.WriteData(fs, numArray4);
                        Util.FileUtil.WriteData(fs, new sbyte[1]);
                        Util.FileUtil.WriteData(fs, new sbyte[1]);
                        Util.FileUtil.WriteData(fs, new sbyte[1]);
                        sbyte[] numArray5 = new sbyte[] { 1 };
                        Util.FileUtil.WriteData(fs, numArray5);
                        sbyte[] numArray6 = new sbyte[] { 1 };
                        Util.FileUtil.WriteData(fs, numArray6);
                        sbyte[] numArray7 = new sbyte[] { 1 };
                        Util.FileUtil.WriteData(fs, numArray7);
                        sbyte[] numArray8 = new sbyte[] { 1 };
                        Util.FileUtil.WriteData(fs, numArray8);
                        Util.FileUtil.WriteData(fs, new sbyte[1]);
                        Util.FileUtil.WriteData(fs, new sbyte[1]);
                        Util.FileUtil.WriteData(fs, new sbyte[1]);
                        byte[] buffer1 = new byte[] { 1 };
                        Util.FileUtil.WriteData(fs, buffer1);
                        ushort[] numArray9 = new ushort[] { (ushort) list6.IndexOf(str) };
                        Util.FileUtil.WriteData(fs, numArray9);
                        float[] singleArray1 = new float[] { (num10 * 1000f) / 30f };
                        Util.FileUtil.WriteData(fs, singleArray1);
                        short[] numArray10 = new short[] { (short) list.Count };
                        Util.FileUtil.WriteData(fs, numArray10);
                        num16 = fs.Position;
                        for (int num32 = 0; num32 < list.Count; num32++)
                        {
                            List<float> list14 = dictionary2[num32];
                            ushort[] numArray11 = new ushort[] { (ushort) list6.IndexOf(list[num32].get_name()) };
                            Util.FileUtil.WriteData(fs, numArray11);
                            short[] numArray12 = new short[] { (short) list3[num32] };
                            Util.FileUtil.WriteData(fs, numArray12);
                            ushort[] numArray13 = new ushort[] { (ushort) list14.Count };
                            Util.FileUtil.WriteData(fs, numArray13);
                            for (int num33 = 0; num33 < list14.Count; num33++)
                            {
                                float[] singleArray2 = new float[] { list14[num33] };
                                Util.FileUtil.WriteData(fs, singleArray2);
                                Util.FileUtil.WriteData(fs, new uint[1]);
                            }
                        }
                        num17 = fs.Position;
                        for (int num34 = 0; num34 < list6.Count; num34++)
                        {
                            Util.FileUtil.WriteData(fs, list6[num34]);
                        }
                        num18 = fs.Position - num17;
                        long num39 = fs.Position;
                        for (int num35 = 0; num35 < list.Count; num35++)
                        {
                            List<FrameData> list15 = dictionary[num35];
                            for (int num36 = 0; num36 < list15.Count; num36++)
                            {
                                float[] singleArray3 = new float[] { list15[num36].localPosition.x * -1f };
                                Util.FileUtil.WriteData(fs, singleArray3);
                                float[] singleArray4 = new float[] { list15[num36].localPosition.y };
                                Util.FileUtil.WriteData(fs, singleArray4);
                                float[] singleArray5 = new float[] { list15[num36].localPosition.z };
                                Util.FileUtil.WriteData(fs, singleArray5);
                                float[] singleArray6 = new float[] { list15[num36].localRotation.x * -1f };
                                Util.FileUtil.WriteData(fs, singleArray6);
                                float[] singleArray7 = new float[] { list15[num36].localRotation.y };
                                Util.FileUtil.WriteData(fs, singleArray7);
                                float[] singleArray8 = new float[] { list15[num36].localRotation.z };
                                Util.FileUtil.WriteData(fs, singleArray8);
                                float[] singleArray9 = new float[] { list15[num36].localRotation.w * -1f };
                                Util.FileUtil.WriteData(fs, singleArray9);
                                float[] singleArray10 = new float[] { list15[num36].localScale.x };
                                Util.FileUtil.WriteData(fs, singleArray10);
                                float[] singleArray11 = new float[] { list15[num36].localScale.y };
                                Util.FileUtil.WriteData(fs, singleArray11);
                                float[] singleArray12 = new float[] { list15[num36].localScale.z };
                                Util.FileUtil.WriteData(fs, singleArray12);
                            }
                        }
                        num19 = fs.Position;
                        long num21 = num18;
                        fs.Position = num16;
                        for (int num37 = 0; num37 < list.Count; num37++)
                        {
                            fs.Position += 6L;
                            List<FrameData> list16 = dictionary[num37];
                            for (int num38 = 0; num38 < list16.Count; num38++)
                            {
                                fs.Position += 4L;
                                uint[] numArray14 = new uint[] { (uint) num21 };
                                Util.FileUtil.WriteData(fs, numArray14);
                                num21 += 40L;
                            }
                        }
                        fs.Position = (num14 + 2L) + 4L;
                        uint[] numArray15 = new uint[] { (uint) (num17 - num15) };
                        Util.FileUtil.WriteData(fs, numArray15);
                        fs.Position = position;
                        uint[] numArray16 = new uint[] { (uint) num17 };
                        Util.FileUtil.WriteData(fs, numArray16);
                        uint[] numArray17 = new uint[] { (uint) (num19 - num17) };
                        Util.FileUtil.WriteData(fs, numArray17);
                        fs.Close();
                    }
                }
            }
        }

        public static void saveParticleFrameData(Gradient gradient, JSONObject obj, string str)
        {
            if (gradient != null)
            {
                GradientAlphaKey[] keyArray = gradient.get_alphaKeys();
                GradientColorKey[] keyArray2 = gradient.get_colorKeys();
                JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                obj.AddField(str, obj2);
                JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
                JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                if ((keyArray != null) && (keyArray.Length != 0))
                {
                    int length = keyArray.Length;
                    for (int i = 0; i < length; i++)
                    {
                        GradientAlphaKey key = keyArray[i];
                        if (i == 0)
                        {
                            obj4 = new JSONObject(JSONObject.Type.OBJECT);
                            obj4.AddField("key", (float) 0f);
                            obj4.AddField("value", key.alpha);
                            obj3.Add(obj4);
                            if (((key.time - precision) > 0f) && (key.time < 0.5))
                            {
                                obj4 = new JSONObject(JSONObject.Type.OBJECT);
                                obj4.AddField("key", key.time);
                                obj4.AddField("value", key.alpha);
                                obj3.Add(obj4);
                            }
                        }
                        if ((i != 0) && (i != (length - 1)))
                        {
                            obj4 = new JSONObject(JSONObject.Type.OBJECT);
                            obj4.AddField("key", key.time);
                            obj4.AddField("value", key.alpha);
                            obj3.Add(obj4);
                        }
                        if (i == (length - 1))
                        {
                            if (((key.time + precision) < 1f) && (key.time >= 0.5))
                            {
                                obj4 = new JSONObject(JSONObject.Type.OBJECT);
                                obj4.AddField("key", key.time);
                                obj4.AddField("value", key.alpha);
                                obj3.Add(obj4);
                            }
                            obj4 = new JSONObject(JSONObject.Type.OBJECT);
                            obj4.AddField("key", (float) 1f);
                            obj4.AddField("value", key.alpha);
                            obj3.Add(obj4);
                        }
                    }
                }
                obj2.AddField("alphas", obj3);
                JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
                JSONObject obj6 = new JSONObject(JSONObject.Type.OBJECT);
                JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
                if ((keyArray2 != null) && (keyArray2.Length != 0))
                {
                    int num3 = keyArray2.Length;
                    for (int j = 0; j < num3; j++)
                    {
                        GradientColorKey key2 = keyArray2[j];
                        if (j == 0)
                        {
                            obj6 = new JSONObject(JSONObject.Type.OBJECT);
                            obj6.AddField("key", (float) 0f);
                            obj7 = new JSONObject(JSONObject.Type.ARRAY);
                            obj7.Add(key2.color.r);
                            obj7.Add(key2.color.g);
                            obj7.Add(key2.color.b);
                            obj6.AddField("value", obj7);
                            obj5.Add(obj6);
                            if (((key2.time - precision) > 0f) && (key2.time < 0.5))
                            {
                                obj6 = new JSONObject(JSONObject.Type.OBJECT);
                                obj6.AddField("key", key2.time);
                                obj7 = new JSONObject(JSONObject.Type.ARRAY);
                                obj7.Add(key2.color.r);
                                obj7.Add(key2.color.g);
                                obj7.Add(key2.color.b);
                                obj6.AddField("value", obj7);
                                obj5.Add(obj6);
                            }
                        }
                        if ((j != 0) && (j != (num3 - 1)))
                        {
                            obj6 = new JSONObject(JSONObject.Type.OBJECT);
                            obj6.AddField("key", key2.time);
                            obj7 = new JSONObject(JSONObject.Type.ARRAY);
                            obj7.Add(key2.color.r);
                            obj7.Add(key2.color.g);
                            obj7.Add(key2.color.b);
                            obj6.AddField("value", obj7);
                            obj5.Add(obj6);
                        }
                        if (j == (num3 - 1))
                        {
                            if (((key2.time + precision) < 1f) && (key2.time >= 0.5))
                            {
                                obj6 = new JSONObject(JSONObject.Type.OBJECT);
                                obj6.AddField("key", key2.time);
                                obj7 = new JSONObject(JSONObject.Type.ARRAY);
                                obj7.Add(key2.color.r);
                                obj7.Add(key2.color.g);
                                obj7.Add(key2.color.b);
                                obj6.AddField("value", obj7);
                                obj5.Add(obj6);
                            }
                            obj6 = new JSONObject(JSONObject.Type.OBJECT);
                            obj6.AddField("key", (float) 1f);
                            obj7 = new JSONObject(JSONObject.Type.ARRAY);
                            obj7.Add(key2.color.r);
                            obj7.Add(key2.color.g);
                            obj7.Add(key2.color.b);
                            obj6.AddField("value", obj7);
                            obj5.Add(obj6);
                        }
                    }
                }
                obj2.AddField("rgbs", obj5);
            }
        }

        public static void saveParticleFrameData(ParticleSystem.MinMaxCurve minMaxCurve, JSONObject obj, string str1, string str2, int type, float curveMultiplier, float convert)
        {
            AnimationCurve curve;
            if (type == -1)
            {
                curve = minMaxCurve.get_curveMin();
            }
            else if (type == 1)
            {
                curve = minMaxCurve.get_curveMax();
            }
            else
            {
                curve = minMaxCurve.get_curve();
            }
            JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
            obj.AddField(str1, obj2);
            JSONObject obj3 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
            if ((curve != null) && (curve.get_length() != 0))
            {
                int num = curve.get_length();
                for (int i = 0; i < num; i++)
                {
                    Keyframe keyframe = curve.get_Item(i);
                    if (i == 0)
                    {
                        obj4 = new JSONObject(JSONObject.Type.OBJECT);
                        obj4.AddField("key", (float) 0f);
                        obj4.AddField("value", (float) ((keyframe.get_value() * curveMultiplier) * convert));
                        obj3.Add(obj4);
                        if (((keyframe.get_time() - precision) > 0f) && (keyframe.get_time() < 0.5))
                        {
                            obj4 = new JSONObject(JSONObject.Type.OBJECT);
                            obj4.AddField("key", keyframe.get_time());
                            obj4.AddField("value", (float) ((keyframe.get_value() * curveMultiplier) * convert));
                            obj3.Add(obj4);
                        }
                    }
                    if ((i != 0) && (i != (num - 1)))
                    {
                        obj4 = new JSONObject(JSONObject.Type.OBJECT);
                        obj4.AddField("key", keyframe.get_time());
                        obj4.AddField("value", (float) ((keyframe.get_value() * curveMultiplier) * convert));
                        obj3.Add(obj4);
                    }
                    if (i == (num - 1))
                    {
                        if (((keyframe.get_time() + precision) < 1f) && (keyframe.get_time() >= 0.5))
                        {
                            obj4 = new JSONObject(JSONObject.Type.OBJECT);
                            obj4.AddField("key", keyframe.get_time());
                            obj4.AddField("value", (float) ((keyframe.get_value() * curveMultiplier) * convert));
                            obj3.Add(obj4);
                        }
                        obj4 = new JSONObject(JSONObject.Type.OBJECT);
                        obj4.AddField("key", (float) 1f);
                        obj4.AddField("value", (float) ((keyframe.get_value() * curveMultiplier) * convert));
                        obj3.Add(obj4);
                    }
                }
            }
            obj2.AddField(str2, obj3);
        }

        public static void saveSkinLmFile(SkinnedMeshRenderer skinnedMeshRenderer, string savePath)
        {
            int num;
            int num3;
            VertexData data;
            Mesh mesh = skinnedMeshRenderer.get_sharedMesh();
            ushort num6 = 1;
            ushort num7 = (ushort) mesh.get_subMeshCount();
            ushort num8 = 0;
            string item = "";
            string str2 = cleanIllegalChar(mesh.get_name(), true);
            for (num = 0; num < VertexStructure.Length; num++)
            {
                VertexStructure[num] = 0;
            }
            if ((mesh.get_vertices() != null) && (mesh.get_vertices().Length != 0))
            {
                VertexStructure[0] = 1;
                item = item + "POSITION";
                num8 = (ushort) (num8 + 12);
            }
            if ((mesh.get_normals() != null) && (mesh.get_normals().Length != 0))
            {
                VertexStructure[1] = 1;
                item = item + ",NORMAL";
                num8 = (ushort) (num8 + 12);
            }
            if (((mesh.get_colors() != null) && (mesh.get_colors().Length != 0)) && !IgnoreColor)
            {
                VertexStructure[2] = 1;
                item = item + ",COLOR";
                num8 = (ushort) (num8 + 0x10);
            }
            if ((mesh.get_uv() != null) && (mesh.get_uv().Length != 0))
            {
                VertexStructure[3] = 1;
                item = item + ",UV";
                num8 = (ushort) (num8 + 8);
            }
            if ((mesh.get_uv2() != null) && (mesh.get_uv2().Length != 0))
            {
                VertexStructure[4] = 1;
                item = item + ",UV1";
                num8 = (ushort) (num8 + 8);
            }
            if ((mesh.get_boneWeights() != null) && (mesh.get_boneWeights().Length != 0))
            {
                VertexStructure[5] = 1;
                item = item + ",BLENDWEIGHT,BLENDINDICES";
                num8 = (ushort) (num8 + 0x20);
            }
            if (((mesh.get_tangents() != null) && (mesh.get_tangents().Length != 0)) && !IgnoreTangent)
            {
                VertexStructure[6] = 1;
                item = item + ",TANGENT";
                num8 = (ushort) (num8 + 0x10);
            }
            List<Transform> list = new List<Transform>();
            int index = 0;
            while (index < skinnedMeshRenderer.get_bones().Length)
            {
                Transform transform = skinnedMeshRenderer.get_bones()[index];
                if (list.IndexOf(transform) == -1)
                {
                    list.Add(transform);
                }
                index++;
            }
            List<VertexData> list2 = new List<VertexData>();
            List<VertexData> list3 = new List<VertexData>();
            List<VertexData> list4 = new List<VertexData>();
            int[] numArray = new int[3];
            List<int> list5 = new List<int>();
            List<List<int>>[] listArray = new List<List<int>>[num7];
            List<int> list6 = new List<int>();
            List<int> list7 = new List<int>();
            List<int>[] listArray2 = new List<int>[num7];
            for (num = 0; num < num7; num++)
            {
                int[] indices = mesh.GetIndices(num);
                listArray[num] = new List<List<int>>();
                listArray2[num] = new List<int>();
                index = 0;
                while (index < indices.Length)
                {
                    int num4;
                    int num5;
                    num3 = 0;
                    while (num3 < 3)
                    {
                        int num9 = index + num3;
                        int num10 = indices[num9];
                        numArray[num3] = -1;
                        num4 = 0;
                        while (num4 < list3.Count)
                        {
                            if (list3[num4].index == num10)
                            {
                                numArray[num3] = num4;
                                break;
                            }
                            num4++;
                        }
                        if (numArray[num3] == -1)
                        {
                            data = getVertexData(mesh, num10);
                            list4.Add(data);
                            num4 = 0;
                            while (num4 < 4)
                            {
                                float num12 = data.boneIndex.get_Item(num4);
                                if ((list6.IndexOf((int) num12) == -1) && (list7.IndexOf((int) num12) == -1))
                                {
                                    list7.Add((int) num12);
                                }
                                num4++;
                            }
                        }
                        num3++;
                    }
                    if ((list6.Count + list7.Count) <= MaxBoneCount)
                    {
                        num5 = 0;
                        while (num5 < list7.Count)
                        {
                            list6.Add(list7[num5]);
                            num5++;
                        }
                        int num11 = 1;
                        num5 = 0;
                        while (num5 < 3)
                        {
                            if (numArray[num5] == -1)
                            {
                                list5.Add(((list3.Count - 1) + num11++) + list2.Count);
                            }
                            else
                            {
                                list5.Add(numArray[num5] + list2.Count);
                            }
                            num5++;
                        }
                        num5 = 0;
                        while (num5 < list4.Count)
                        {
                            list3.Add(list4[num5]);
                            num5++;
                        }
                    }
                    else
                    {
                        listArray2[num].Add(index);
                        listArray[num].Add(list6);
                        num5 = 0;
                        while (num5 < list3.Count)
                        {
                            data = list3[num5];
                            num4 = 0;
                            while (num4 < 4)
                            {
                                data.boneIndex.set_Item(num4, (float) list6.IndexOf((int) data.boneIndex.get_Item(num4)));
                                num4++;
                            }
                            list2.Add(data);
                            num5++;
                        }
                        index -= 3;
                        list3 = new List<VertexData>();
                        list6 = new List<int>();
                    }
                    if ((index + 3) == indices.Length)
                    {
                        listArray2[num].Add(indices.Length);
                        listArray[num].Add(list6);
                        for (num5 = 0; num5 < list3.Count; num5++)
                        {
                            data = list3[num5];
                            for (num4 = 0; num4 < 4; num4++)
                            {
                                data.boneIndex.set_Item(num4, (float) list6.IndexOf((int) data.boneIndex.get_Item(num4)));
                            }
                            list2.Add(data);
                        }
                        list3 = new List<VertexData>();
                        list6 = new List<int>();
                    }
                    list7 = new List<int>();
                    list4 = new List<VertexData>();
                    index += 3;
                }
            }
            int[] numArray2 = new int[num7];
            int[] numArray3 = new int[num7];
            int num13 = 0;
            for (num = 0; num < num7; num++)
            {
                int[] numArray8 = mesh.GetIndices(num);
                numArray2[num] = list5[num13];
                numArray3[num] = numArray8.Length;
                num13 += numArray8.Length;
            }
            long num14 = 0L;
            long num15 = 0L;
            long num16 = 0L;
            long num17 = 0L;
            long num18 = 0L;
            long num19 = 0L;
            long num20 = 0L;
            long num21 = 0L;
            long num22 = 0L;
            long num23 = 0L;
            long num24 = 0L;
            long num25 = 0L;
            long num26 = 0L;
            long num27 = 0L;
            long num28 = 0L;
            long num29 = 0L;
            long num30 = 0L;
            long[] numArray4 = new long[num7];
            long[] numArray5 = new long[num7];
            long[] numArray6 = new long[num7];
            List<string> list8 = new List<string> { "MESH", "SUBMESH" };
            FileStream fs = Util.FileUtil.saveFile(savePath, null);
            string str3 = "LAYAMODEL:0301";
            Util.FileUtil.WriteData(fs, str3);
            long position = fs.Position;
            num14 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            num20 = fs.Position;
            int num31 = num7 + 1;
            ushort[] datas = new ushort[] { (ushort) num31 };
            Util.FileUtil.WriteData(fs, datas);
            for (num = 0; num < num31; num++)
            {
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
            }
            num21 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new ushort[1]);
            num15 = fs.Position;
            ushort[] numArray9 = new ushort[] { (ushort) list8.IndexOf("MESH") };
            Util.FileUtil.WriteData(fs, numArray9);
            list8.Add(str2);
            ushort[] numArray10 = new ushort[] { (ushort) list8.IndexOf(str2) };
            Util.FileUtil.WriteData(fs, numArray10);
            ushort[] numArray11 = new ushort[] { num6 };
            Util.FileUtil.WriteData(fs, numArray11);
            num17 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            list8.Add(item);
            ushort[] numArray12 = new ushort[] { (ushort) list8.IndexOf(item) };
            Util.FileUtil.WriteData(fs, numArray12);
            num18 = fs.Position;
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            num19 = fs.Position;
            ushort[] numArray13 = new ushort[] { (ushort) list.Count };
            Util.FileUtil.WriteData(fs, numArray13);
            for (num = 0; num < list.Count; num++)
            {
                list8.Add(list[num].get_name());
                ushort[] numArray14 = new ushort[] { (ushort) list8.IndexOf(list[num].get_name()) };
                Util.FileUtil.WriteData(fs, numArray14);
            }
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            Util.FileUtil.WriteData(fs, new uint[1]);
            num16 = fs.Position - num15;
            for (num = 0; num < num7; num++)
            {
                numArray4[num] = fs.Position;
                ushort[] numArray15 = new ushort[] { (ushort) list8.IndexOf("SUBMESH") };
                Util.FileUtil.WriteData(fs, numArray15);
                Util.FileUtil.WriteData(fs, new ushort[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                ushort[] numArray16 = new ushort[] { (ushort) listArray[num].Count };
                Util.FileUtil.WriteData(fs, numArray16);
                index = 0;
                while (index < listArray[num].Count)
                {
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    index++;
                }
                numArray5[num] = fs.Position;
                numArray6[num] = numArray5[num] - numArray4[num];
            }
            num22 = fs.Position;
            for (num = 0; num < list8.Count; num++)
            {
                Util.FileUtil.WriteData(fs, list8[num]);
            }
            num23 = fs.Position - num22;
            num24 = fs.Position;
            for (index = 0; index < list2.Count; index++)
            {
                data = list2[index];
                Vector3 vertice = data.vertice;
                float[] singleArray1 = new float[] { vertice.x * -1f, vertice.y, vertice.z };
                Util.FileUtil.WriteData(fs, singleArray1);
                if (VertexStructure[1] == 1)
                {
                    Vector3 normal = data.normal;
                    float[] singleArray2 = new float[] { normal.x * -1f, normal.y, normal.z };
                    Util.FileUtil.WriteData(fs, singleArray2);
                }
                if (VertexStructure[2] == 1)
                {
                    Color color = data.color;
                    float[] singleArray3 = new float[] { color.r, color.g, color.b, color.a };
                    Util.FileUtil.WriteData(fs, singleArray3);
                }
                if (VertexStructure[3] == 1)
                {
                    Vector2 uv = data.uv;
                    float[] singleArray4 = new float[] { uv.x, (uv.y * -1f) + 1f };
                    Util.FileUtil.WriteData(fs, singleArray4);
                }
                if (VertexStructure[4] == 1)
                {
                    Vector2 vector4 = data.uv2;
                    float[] singleArray5 = new float[] { vector4.x, vector4.y * -1f };
                    Util.FileUtil.WriteData(fs, singleArray5);
                }
                if (VertexStructure[5] == 1)
                {
                    Vector4 boneWeight = data.boneWeight;
                    Vector4 boneIndex = data.boneIndex;
                    float[] singleArray6 = new float[] { boneWeight.x, boneWeight.y, boneWeight.z, boneWeight.w };
                    Util.FileUtil.WriteData(fs, singleArray6);
                    float[] singleArray7 = new float[] { boneIndex.x, boneIndex.y, boneIndex.z, boneIndex.w };
                    Util.FileUtil.WriteData(fs, singleArray7);
                }
                if (VertexStructure[6] == 1)
                {
                    Vector4 tangent = data.tangent;
                    float[] singleArray8 = new float[] { tangent.x * -1f, tangent.y, tangent.z, tangent.w };
                    Util.FileUtil.WriteData(fs, singleArray8);
                }
            }
            num25 = fs.Position - num24;
            num26 = fs.Position;
            index = 0;
            while (index < list5.Count)
            {
                ushort[] numArray17 = new ushort[] { (ushort) list5[index] };
                Util.FileUtil.WriteData(fs, numArray17);
                index++;
            }
            num27 = fs.Position - num26;
            if ((mesh.get_bindposes() != null) && (mesh.get_bindposes().Length != 0))
            {
                Matrix4x4[] matrixxArray = new Matrix4x4[mesh.get_bindposes().Length];
                for (num = 0; num < mesh.get_bindposes().Length; num++)
                {
                    Vector3 vector8;
                    Quaternion quaternion;
                    Vector3 vector9;
                    matrixxArray[num] = mesh.get_bindposes()[num];
                    matrixxArray[num] = matrixxArray[num].get_inverse();
                    MathUtil.Decompose(matrixxArray[num].get_transpose(), out vector9, out quaternion, out vector8);
                    vector8.x *= -1f;
                    quaternion.x *= -1f;
                    quaternion.w *= -1f;
                    matrixxArray[num] = Matrix4x4.TRS(vector8, quaternion, vector9);
                }
                num28 = fs.Position;
                for (num = 0; num < mesh.get_bindposes().Length; num++)
                {
                    Matrix4x4 matrixx = matrixxArray[num];
                    index = 0;
                    while (index < 0x10)
                    {
                        float[] singleArray9 = new float[] { matrixx.get_Item(index) };
                        Util.FileUtil.WriteData(fs, singleArray9);
                        index++;
                    }
                }
                num29 = fs.Position;
                for (num = 0; num < mesh.get_bindposes().Length; num++)
                {
                    Matrix4x4 matrixx2 = matrixxArray[num].get_inverse();
                    index = 0;
                    while (index < 0x10)
                    {
                        float[] singleArray10 = new float[] { matrixx2.get_Item(index) };
                        Util.FileUtil.WriteData(fs, singleArray10);
                        index++;
                    }
                }
                num30 = fs.Position;
                for (num = 0; num < num7; num++)
                {
                    index = 0;
                    while (index < listArray[num].Count)
                    {
                        for (num3 = 0; num3 < listArray[num][index].Count; num3++)
                        {
                            byte[] buffer1 = new byte[] { (byte) listArray[num][index][num3] };
                            Util.FileUtil.WriteData(fs, buffer1);
                        }
                        index++;
                    }
                }
                long num39 = fs.Position;
            }
            uint num32 = 0;
            uint num33 = 0;
            uint num34 = 0;
            uint num35 = 0;
            uint num36 = 0;
            long num37 = num30 - num22;
            for (num = 0; num < num7; num++)
            {
                fs.Position = numArray4[num] + 4L;
                if (num7 == 1)
                {
                    num32 = 0;
                    num33 = (uint) (num25 / ((ulong) num8));
                    num34 = 0;
                    num35 = (uint) (num27 / 2L);
                }
                else if (num == 0)
                {
                    num32 = 0;
                    num33 = (uint) numArray2[num + 1];
                    num34 = num36;
                    num35 = (uint) numArray3[num];
                }
                else if (num < (num7 - 1))
                {
                    num32 = (uint) numArray2[num];
                    num33 = (uint) (numArray2[num + 1] - numArray2[num]);
                    num34 = num36;
                    num35 = (uint) numArray3[num];
                }
                else
                {
                    num32 = (uint) numArray2[num];
                    num33 = (uint) ((num25 / ((ulong) num8)) - numArray2[num]);
                    num34 = num36;
                    num35 = (uint) numArray3[num];
                }
                uint[] numArray18 = new uint[] { num32 };
                Util.FileUtil.WriteData(fs, numArray18);
                uint[] numArray19 = new uint[] { num33 };
                Util.FileUtil.WriteData(fs, numArray19);
                uint[] numArray20 = new uint[] { num34 };
                Util.FileUtil.WriteData(fs, numArray20);
                uint[] numArray21 = new uint[] { num35 };
                Util.FileUtil.WriteData(fs, numArray21);
                num36 += num35;
                fs.Position += 2L;
                int num38 = 0;
                for (index = 0; index < listArray[num].Count; index++)
                {
                    uint[] numArray22 = new uint[] { num38 + num34 };
                    Util.FileUtil.WriteData(fs, numArray22);
                    uint[] numArray23 = new uint[] { listArray2[num][index] - num38 };
                    Util.FileUtil.WriteData(fs, numArray23);
                    num38 = listArray2[num][index];
                    uint[] numArray24 = new uint[] { (uint) num37 };
                    Util.FileUtil.WriteData(fs, numArray24);
                    uint[] numArray25 = new uint[] { listArray[num][index].Count };
                    Util.FileUtil.WriteData(fs, numArray25);
                    num37 += listArray[num][index].Count;
                }
            }
            fs.Position = num17;
            uint[] numArray26 = new uint[] { (uint) (num24 - num22) };
            Util.FileUtil.WriteData(fs, numArray26);
            uint[] numArray27 = new uint[] { (uint) num25 };
            Util.FileUtil.WriteData(fs, numArray27);
            fs.Position = num18;
            uint[] numArray28 = new uint[] { (uint) (num26 - num22) };
            Util.FileUtil.WriteData(fs, numArray28);
            uint[] numArray29 = new uint[] { (uint) num27 };
            Util.FileUtil.WriteData(fs, numArray29);
            fs.Position = num19 + ((list.Count + 1) * 2);
            uint[] numArray30 = new uint[] { (uint) (num28 - num22) };
            Util.FileUtil.WriteData(fs, numArray30);
            uint[] numArray31 = new uint[] { (uint) (num29 - num28) };
            Util.FileUtil.WriteData(fs, numArray31);
            uint[] numArray32 = new uint[] { (uint) (num29 - num22) };
            Util.FileUtil.WriteData(fs, numArray32);
            uint[] numArray33 = new uint[] { (uint) (num30 - num29) };
            Util.FileUtil.WriteData(fs, numArray33);
            fs.Position = num21;
            Util.FileUtil.WriteData(fs, new uint[1]);
            ushort[] numArray34 = new ushort[] { (ushort) list8.Count };
            Util.FileUtil.WriteData(fs, numArray34);
            long num40 = fs.Position;
            fs.Position = num20 + 2L;
            uint[] numArray35 = new uint[] { (uint) num15 };
            Util.FileUtil.WriteData(fs, numArray35);
            uint[] numArray36 = new uint[] { (uint) num16 };
            Util.FileUtil.WriteData(fs, numArray36);
            for (num = 0; num < num7; num++)
            {
                uint[] numArray37 = new uint[] { (uint) numArray4[num] };
                Util.FileUtil.WriteData(fs, numArray37);
                uint[] numArray38 = new uint[] { (uint) numArray6[num] };
                Util.FileUtil.WriteData(fs, numArray38);
            }
            fs.Position = num14;
            uint[] numArray39 = new uint[] { (uint) num22 };
            Util.FileUtil.WriteData(fs, numArray39);
            uint[] numArray40 = new uint[] { (uint) ((((num22 + num23) + num25) + num27) + numArray6[0]) };
            Util.FileUtil.WriteData(fs, numArray40);
            fs.Close();
        }

        public static Dictionary<string, JSONObject> saveSpriteNode()
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            Dictionary<string, JSONObject> dictionary = new Dictionary<string, JSONObject>();
            if (rootGameObjects.Length != 0)
            {
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    Dictionary<GameObject, string> dictionary2;
                    LayaAutoGOListIndex = i;
                    dictionary2 = new Dictionary<GameObject, string> {
                        dictionary2
                    };
                    List<ComponentType> list = componentsOnGameObject(rootGameObjects[i]);
                    checkChildIsLegal(rootGameObjects[i], true);
                    if ((rootGameObjects[i].get_activeInHierarchy() || !IgnoreNotActiveGameObject) && ((!OptimizeGameObject && !IgnoreNullGameObject) || ((list.Count > 1) || curNodeHasLegalChild)))
                    {
                        JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                        obj2.AddField("type", "Sprite3D");
                        JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
                        obj3.AddField("name", SceneManager.GetActiveScene().get_name());
                        obj2.AddField("props", obj3);
                        JSONObject obj4 = new JSONObject(JSONObject.Type.OBJECT);
                        Vector3 vector = new Vector3(0f, 0f, 0f);
                        Quaternion quaternion = new Quaternion(0f, 0f, 0f, -1f);
                        Vector3 vector2 = new Vector3(1f, 1f, 1f);
                        JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
                        obj5.Add(vector.x);
                        obj5.Add(vector.y);
                        obj5.Add(vector.z);
                        obj4.AddField("translate", obj5);
                        JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
                        obj6.Add(quaternion.x);
                        obj6.Add(quaternion.y);
                        obj6.Add(quaternion.z);
                        obj6.Add(quaternion.w);
                        obj4.AddField("rotation", obj6);
                        JSONObject obj7 = new JSONObject(JSONObject.Type.ARRAY);
                        obj7.Add(vector2.x);
                        obj7.Add(vector2.y);
                        obj7.Add(vector2.z);
                        obj4.AddField("scale", obj7);
                        obj2.AddField("customProps", obj4);
                        JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
                        obj2.AddField("child", obj8);
                        string sceneName = DataManager.sceneName;
                        getGameObjectData(rootGameObjects[i].get_gameObject(), sceneName, obj8, false);
                        dictionary.Add(rootGameObjects[i].get_name(), obj2);
                    }
                }
            }
            return dictionary;
        }

        public static void saveTerrainData(string savePath, JSONObject obj, GameObject gameObject = null)
        {
            LayaTerrainExporter.ExportAllTerrian(savePath, obj);
        }

        public static void saveTerrainLmatData(GameObject gameObject, JSONObject obj)
        {
            TerrainData data = gameObject.GetComponent<Terrain>().get_terrainData();
            string val = cleanIllegalChar("terrain_" + gameObject.get_name(), true);
            string str2 = "terrain/" + val + ".lmat";
            string str = SAVEPATH + "/" + str2;
            JSONObject node = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            JSONObject obj4 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj5 = new JSONObject(JSONObject.Type.ARRAY);
            JSONObject obj6 = new JSONObject(JSONObject.Type.ARRAY);
            node.AddField("version", "LAYAMATERIAL:01");
            node.AddField("props", obj3);
            obj3.AddField("name", val);
            obj3.AddField("cull", 2);
            obj3.AddField("blend", 0);
            obj3.AddField("srcBlend", 1);
            obj3.AddField("dstBlend", 0);
            obj3.AddField("alphaTest", false);
            obj3.AddField("depthWrite", true);
            obj3.AddField("renderQueue", 1);
            obj3.AddField("textures", obj4);
            obj3.AddField("vectors", obj5);
            obj3.AddField("defines", obj6);
            if (data.get_alphamapTextures().Length > 0)
            {
                for (int j = 0; j < 1; j++)
                {
                    JSONObject obj15 = new JSONObject(JSONObject.Type.OBJECT);
                    obj15.AddField("name", "splatAlphaTexture");
                    Color[] pixels = data.get_alphamapTextures()[j].GetPixels();
                    int num3 = pixels.Length;
                    int num1 = (int) Mathf.Sqrt((float) num3);
                    Texture2D textured = new Texture2D(num1, num1);
                    Color[] colorArray2 = new Color[num3];
                    for (int k = 0; k < num3; k++)
                    {
                        colorArray2[k] = pixels[k];
                        if (colorArray2[k].a == 0f)
                        {
                            colorArray2[k].a = 0.03125f;
                        }
                    }
                    textured.SetPixels(colorArray2);
                    textured.Apply();
                    FileStream output = File.Open(SAVEPATH + "/terrain/splatAlphaTexture.png", System.IO.FileMode.Create);
                    new BinaryWriter(output).Write(ImageConversion.EncodeToPNG(textured));
                    output.Close();
                    obj15.AddField("path", "splatAlphaTexture.png");
                    obj4.Add(obj15);
                }
            }
            int length = data.get_splatPrototypes().Length;
            for (int i = 0; i < length; i++)
            {
                SplatPrototype prototype = data.get_splatPrototypes()[i];
                JSONObject obj16 = new JSONObject(JSONObject.Type.OBJECT);
                obj16.AddField("name", "diffuseTexture" + (i + 1));
                saveTextureFile(obj16, prototype.get_texture(), cleanIllegalChar(str, false), null, "path");
                obj4.Add(obj16);
                JSONObject obj17 = new JSONObject(JSONObject.Type.OBJECT);
                obj17.AddField("name", "diffuseScaleOffset" + (i + 1));
                JSONObject obj18 = new JSONObject(JSONObject.Type.ARRAY);
                obj18.Add((float) (data.get_size().x / prototype.get_tileSize().x));
                obj18.Add((float) (data.get_size().z / prototype.get_tileSize().y));
                obj18.Add(prototype.get_tileOffset().x);
                obj18.Add(prototype.get_tileOffset().y);
                obj17.AddField("value", obj18);
                obj5.Add(obj17);
            }
            JSONObject obj7 = new JSONObject(JSONObject.Type.OBJECT);
            obj7.AddField("name", "albedo");
            JSONObject obj8 = new JSONObject(JSONObject.Type.ARRAY);
            obj8.Add((float) 1f);
            obj8.Add((float) 1f);
            obj8.Add((float) 1f);
            obj8.Add((float) 1f);
            obj7.AddField("value", obj8);
            obj5.Add(obj7);
            JSONObject obj9 = new JSONObject(JSONObject.Type.OBJECT);
            obj9.AddField("name", "ambientColor");
            JSONObject obj10 = new JSONObject(JSONObject.Type.ARRAY);
            obj10.Add((float) 0f);
            obj10.Add((float) 0f);
            obj10.Add((float) 0f);
            obj9.AddField("value", obj10);
            obj5.Add(obj9);
            JSONObject obj11 = new JSONObject(JSONObject.Type.OBJECT);
            obj11.AddField("name", "diffuseColor");
            JSONObject obj12 = new JSONObject(JSONObject.Type.ARRAY);
            obj12.Add((float) 1f);
            obj12.Add((float) 1f);
            obj12.Add((float) 1f);
            obj11.AddField("value", obj12);
            obj5.Add(obj11);
            JSONObject obj13 = new JSONObject(JSONObject.Type.OBJECT);
            obj13.AddField("name", "specularColor");
            JSONObject obj14 = new JSONObject(JSONObject.Type.ARRAY);
            obj14.Add((float) 1f);
            obj14.Add((float) 1f);
            obj14.Add((float) 1f);
            obj14.Add((float) 8f);
            obj13.AddField("value", obj14);
            obj5.Add(obj13);
            Util.FileUtil.saveFile(str, node);
        }

        public static void saveTerrainLmFile(GameObject gameObject, JSONObject obj, int gameObjectType)
        {
            List<TerrainVertexData> list5;
            List<int> list7;
            TerrainData data = gameObject.GetComponent<Terrain>().get_terrainData();
            int num = data.get_heightmapWidth();
            int num2 = data.get_heightmapHeight();
            Vector3 vector = data.get_size();
            int terrainToMeshResolution = TerrainToMeshResolution;
            Vector3 vector2 = new Vector3((vector.x / ((float) (num - 1))) * terrainToMeshResolution, vector.y, (vector.z / ((float) (num2 - 1))) * terrainToMeshResolution);
            Vector2 vector3 = new Vector2(1f / ((float) (num - 1)), 1f / ((float) (num2 - 1)));
            float[,] numArray = data.GetHeights(0, 0, num, num2);
            num = ((num - 1) / terrainToMeshResolution) + 1;
            num2 = ((num2 - 1) / terrainToMeshResolution) + 1;
            Vector3[] vectorArray = new Vector3[num * num2];
            Vector3[] vectorArray2 = new Vector3[num * num2];
            Vector2[] vectorArray3 = new Vector2[num * num2];
            int[] numArray2 = new int[((num - 1) * (num2 - 1)) * 6];
            for (int i = 0; i < num2; i++)
            {
                for (int num36 = 0; num36 < num; num36++)
                {
                    vectorArray[(i * num) + num36] = Vector3.Scale(new Vector3((float) i, numArray[num36 * terrainToMeshResolution, i * terrainToMeshResolution], (float) num36), vector2);
                    vectorArray3[(i * num) + num36] = Vector2.Scale(new Vector2((float) (num36 * terrainToMeshResolution), 1f - (i * terrainToMeshResolution)), vector3) - new Vector2(0f, 1f / ((float) (data.get_heightmapHeight() - 1)));
                    float x = vectorArray3[(i * num) + num36].x;
                    float y = vectorArray3[(i * num) + num36].y;
                    vectorArray3[(i * num) + num36] = new Vector2((x * Mathf.Cos(1.570796f)) - (y * Mathf.Sin(1.570796f)), (x * Mathf.Sin(1.570796f)) + (y * Mathf.Cos(1.570796f)));
                }
            }
            int num4 = 0;
            for (int j = 0; j < (num2 - 1); j++)
            {
                for (int num40 = 0; num40 < (num - 1); num40++)
                {
                    numArray2[num4++] = (j * num) + num40;
                    numArray2[num4++] = ((j * num) + num40) + 1;
                    numArray2[num4++] = ((j + 1) * num) + num40;
                    numArray2[num4++] = ((j + 1) * num) + num40;
                    numArray2[num4++] = ((j * num) + num40) + 1;
                    numArray2[num4++] = (((j + 1) * num) + num40) + 1;
                }
            }
            for (int k = 0; k < vectorArray.Length; k++)
            {
                List<Vector3> list = new List<Vector3>();
                for (int num42 = 0; num42 < numArray2.Length; num42 += 3)
                {
                    if (((numArray2[num42] == k) || (numArray2[num42 + 1] == k)) || (numArray2[num42 + 2] == k))
                    {
                        list.Add(vectorArray[numArray2[num42]]);
                        list.Add(vectorArray[numArray2[num42 + 1]]);
                        list.Add(vectorArray[numArray2[num42 + 2]]);
                    }
                }
                float num10 = 0f;
                List<float> list3 = new List<float>();
                List<Vector3> list2 = new List<Vector3>();
                for (int num43 = 0; num43 < list.Count; num43 += 3)
                {
                    Vector3 vector4 = list[num43] - list[num43 + 1];
                    Vector3 vector5 = list[num43] - list[num43 + 2];
                    float num5 = Mathf.Sqrt((Mathf.Pow(list[num43].x - list[num43 + 1].x, 2f) + Mathf.Pow(list[num43].y - list[num43 + 1].y, 2f)) + Mathf.Pow(list[num43].z - list[num43 + 1].z, 2f));
                    float num6 = Mathf.Sqrt((Mathf.Pow(list[num43].x - list[num43 + 2].x, 2f) + Mathf.Pow(list[num43].y - list[num43 + 2].y, 2f)) + Mathf.Pow(list[num43].z - list[num43 + 2].z, 2f));
                    float num7 = Mathf.Sqrt((Mathf.Pow(list[num43 + 2].x - list[num43 + 1].x, 2f) + Mathf.Pow(list[num43 + 2].y - list[num43 + 1].y, 2f)) + Mathf.Pow(list[num43 + 2].z - list[num43 + 1].z, 2f));
                    float num8 = ((num5 + num6) + num7) / 2f;
                    float num9 = Mathf.Sqrt(((num8 * (num8 - num5)) * (num8 - num6)) * (num8 - num7));
                    list3.Add(num9);
                    num10 += num9;
                    list2.Add(Vector3.Cross(vector4, vector5).get_normalized());
                }
                Vector3 vector6 = new Vector3();
                for (int num44 = 0; num44 < list2.Count; num44++)
                {
                    vector6 += (Vector3) ((list2[num44] * list3[num44]) / num10);
                }
                vectorArray2[k] = vector6.get_normalized();
            }
            int num11 = 0xfffe;
            List<List<TerrainVertexData>> list4 = new List<List<TerrainVertexData>>();
            list5 = new List<TerrainVertexData> {
                list5
            };
            List<List<int>> list6 = new List<List<int>>();
            list7 = new List<int> {
                list7
            };
            for (int m = 0; m < numArray2.Length; m++)
            {
                TerrainVertexData data2;
                if (list5.Count == num11)
                {
                    list5 = new List<TerrainVertexData> {
                        list5
                    };
                    list7 = new List<int> {
                        list7
                    };
                }
                int index = numArray2[m];
                data2.vertice = vectorArray[index];
                data2.normal = vectorArray2[index];
                data2.uv = vectorArray3[index];
                int num47 = list5.IndexOf(data2);
                if (num47 == -1)
                {
                    list5.Add(data2);
                    list7.Add(list5.Count - 1);
                }
                else
                {
                    list7.Add(num47);
                }
            }
            int count = list4.Count;
            string item = cleanIllegalChar("terrain_" + gameObject.get_name(), true);
            string val = "terrain/" + item + ".lm";
            obj.AddField("meshPath", val);
            JSONObject obj2 = new JSONObject(JSONObject.Type.ARRAY);
            obj.AddField("materials", obj2);
            string str3 = cleanIllegalChar("terrain_" + gameObject.get_name(), true);
            string str4 = "terrain/" + str3 + ".lmat";
            JSONObject obj3 = new JSONObject(JSONObject.Type.OBJECT);
            obj3.AddField("type", "Laya.ExtendTerrainMaterial");
            obj3.AddField("path", str4);
            for (int n = 0; n < count; n++)
            {
                obj2.Add(obj3);
            }
            string path = SAVEPATH + "/" + val;
            int num13 = 1 + count;
            ushort num14 = 0x20;
            string str6 = "POSITION,NORMAL,UV";
            if (!File.Exists(path) || CoverOriginalFile)
            {
                int num16;
                long num17 = 0L;
                long num18 = 0L;
                long num19 = 0L;
                long[] numArray3 = new long[count];
                long num20 = 0L;
                long num21 = 0L;
                long num22 = 0L;
                long num23 = 0L;
                long num24 = 0L;
                long[] numArray4 = new long[count];
                long num25 = 0L;
                long num26 = 0L;
                long num27 = 0L;
                long num28 = 0L;
                long[] numArray5 = new long[count];
                long[] numArray6 = new long[count];
                long[] numArray7 = new long[count];
                List<string> list8 = new List<string> { "MESH", "SUBMESH" };
                FileStream fs = Util.FileUtil.saveFile(path, null);
                string str7 = "LAYAMODEL:0301";
                Util.FileUtil.WriteData(fs, str7);
                long position = fs.Position;
                num17 = fs.Position;
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                num21 = fs.Position;
                ushort[] datas = new ushort[] { (ushort) num13 };
                Util.FileUtil.WriteData(fs, datas);
                int num15 = 0;
                while (num15 < num13)
                {
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    num15++;
                }
                num22 = fs.Position;
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new ushort[1]);
                num18 = fs.Position;
                ushort[] numArray8 = new ushort[] { (ushort) list8.IndexOf("MESH") };
                Util.FileUtil.WriteData(fs, numArray8);
                list8.Add(item);
                ushort[] numArray9 = new ushort[] { (ushort) list8.IndexOf(item) };
                Util.FileUtil.WriteData(fs, numArray9);
                ushort[] numArray10 = new ushort[] { (ushort) list4.Count };
                Util.FileUtil.WriteData(fs, numArray10);
                list8.Add(str6);
                num15 = 0;
                while (num15 < list4.Count)
                {
                    numArray3[num15] = fs.Position;
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    ushort[] numArray11 = new ushort[] { (ushort) list8.IndexOf(str6) };
                    Util.FileUtil.WriteData(fs, numArray11);
                    num15++;
                }
                num20 = fs.Position;
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                long num49 = fs.Position;
                Util.FileUtil.WriteData(fs, new ushort[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                Util.FileUtil.WriteData(fs, new uint[1]);
                num19 = fs.Position - num18;
                num15 = 0;
                while (num15 < count)
                {
                    numArray5[num15] = fs.Position;
                    ushort[] numArray12 = new ushort[] { (ushort) list8.IndexOf("SUBMESH") };
                    Util.FileUtil.WriteData(fs, numArray12);
                    Util.FileUtil.WriteData(fs, new ushort[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    ushort[] numArray13 = new ushort[] { 1 };
                    Util.FileUtil.WriteData(fs, numArray13);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    Util.FileUtil.WriteData(fs, new uint[1]);
                    numArray6[num15] = fs.Position;
                    numArray7[num15] = numArray6[num15] - numArray5[num15];
                    num15++;
                }
                num23 = fs.Position;
                num15 = 0;
                while (num15 < list8.Count)
                {
                    Util.FileUtil.WriteData(fs, list8[num15]);
                    num15++;
                }
                num24 = fs.Position - num23;
                num25 = fs.Position;
                num15 = 0;
                while (num15 < list4.Count)
                {
                    numArray4[num15] = fs.Position;
                    List<TerrainVertexData> list9 = list4[num15];
                    num16 = 0;
                    while (num16 < list9.Count)
                    {
                        TerrainVertexData data3 = list9[num16];
                        Vector3 vertice = data3.vertice;
                        float[] singleArray1 = new float[] { vertice.x * -1f, vertice.y, vertice.z };
                        Util.FileUtil.WriteData(fs, singleArray1);
                        Vector3 normal = data3.normal;
                        float[] singleArray2 = new float[] { normal.x * -1f, normal.y, normal.z };
                        Util.FileUtil.WriteData(fs, singleArray2);
                        Vector2 uv = data3.uv;
                        float[] singleArray3 = new float[] { uv.x, (uv.y * -1f) + 1f };
                        Util.FileUtil.WriteData(fs, singleArray3);
                        num16++;
                    }
                    num15++;
                }
                num26 = fs.Position - num25;
                num27 = fs.Position;
                num15 = 0;
                while (num15 < list6.Count)
                {
                    List<int> list10 = list6[num15];
                    for (num16 = 0; num16 < list10.Count; num16++)
                    {
                        ushort[] numArray14 = new ushort[] { (ushort) list10[num16] };
                        Util.FileUtil.WriteData(fs, numArray14);
                    }
                    num15++;
                }
                num28 = fs.Position - num27;
                uint num29 = 0;
                uint num30 = 0;
                uint num31 = 0;
                uint num32 = 0;
                uint num33 = 0;
                uint num34 = 0;
                num15 = 0;
                while (num15 < count)
                {
                    fs.Position = numArray5[num15] + 2L;
                    ushort[] numArray15 = new ushort[] { (ushort) num15 };
                    Util.FileUtil.WriteData(fs, numArray15);
                    num29 = num31;
                    num30 = (uint) list4[num15].Count;
                    num32 = num34;
                    num33 = (uint) list6[num15].Count;
                    uint[] numArray16 = new uint[] { num29 };
                    Util.FileUtil.WriteData(fs, numArray16);
                    uint[] numArray17 = new uint[] { num30 };
                    Util.FileUtil.WriteData(fs, numArray17);
                    uint[] numArray18 = new uint[] { num32 };
                    Util.FileUtil.WriteData(fs, numArray18);
                    uint[] numArray19 = new uint[] { num33 };
                    Util.FileUtil.WriteData(fs, numArray19);
                    num31 += num30;
                    num34 += num33;
                    fs.Position += 2L;
                    uint[] numArray20 = new uint[] { num32 };
                    Util.FileUtil.WriteData(fs, numArray20);
                    uint[] numArray21 = new uint[] { num33 };
                    Util.FileUtil.WriteData(fs, numArray21);
                    num15++;
                }
                num15 = 0;
                while (num15 < list4.Count)
                {
                    fs.Position = numArray3[num15];
                    uint[] numArray22 = new uint[] { (uint) (numArray4[num15] - num23) };
                    Util.FileUtil.WriteData(fs, numArray22);
                    uint[] numArray23 = new uint[] { list4[num15].Count * num14 };
                    Util.FileUtil.WriteData(fs, numArray23);
                    num15++;
                }
                fs.Position = num20;
                uint[] numArray24 = new uint[] { (uint) (num27 - num23) };
                Util.FileUtil.WriteData(fs, numArray24);
                uint[] numArray25 = new uint[] { (uint) num28 };
                Util.FileUtil.WriteData(fs, numArray25);
                fs.Position = num22;
                Util.FileUtil.WriteData(fs, new uint[1]);
                ushort[] numArray26 = new ushort[] { (ushort) list8.Count };
                Util.FileUtil.WriteData(fs, numArray26);
                long num50 = fs.Position;
                fs.Position = num21 + 2L;
                uint[] numArray27 = new uint[] { (uint) num18 };
                Util.FileUtil.WriteData(fs, numArray27);
                uint[] numArray28 = new uint[] { (uint) num19 };
                Util.FileUtil.WriteData(fs, numArray28);
                for (num15 = 0; num15 < count; num15++)
                {
                    uint[] numArray29 = new uint[] { (uint) numArray5[num15] };
                    Util.FileUtil.WriteData(fs, numArray29);
                    uint[] numArray30 = new uint[] { (uint) numArray7[num15] };
                    Util.FileUtil.WriteData(fs, numArray30);
                }
                fs.Position = num17;
                uint[] numArray31 = new uint[] { (uint) num23 };
                Util.FileUtil.WriteData(fs, numArray31);
                uint[] numArray32 = new uint[] { (uint) ((((num23 + num24) + num26) + num28) + numArray7[0]) };
                Util.FileUtil.WriteData(fs, numArray32);
                fs.Close();
            }
        }

        public static void saveTextureCubeFile(Material material, JSONObject obj, string savePath)
        {
            JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
            string materialName = material.get_name();
            if (material.HasProperty("_FrontTex"))
            {
                Texture2D texture = material.GetTexture("_FrontTex");
                saveTextureFile(obj2, texture, savePath, materialName, "front");
            }
            if (material.HasProperty("_BackTex"))
            {
                Texture2D textured2 = material.GetTexture("_BackTex");
                saveTextureFile(obj2, textured2, savePath, materialName, "back");
            }
            if (material.HasProperty("_LeftTex"))
            {
                Texture2D textured3 = material.GetTexture("_LeftTex");
                saveTextureFile(obj2, textured3, savePath, materialName, "left");
            }
            if (material.HasProperty("_RightTex"))
            {
                Texture2D textured4 = material.GetTexture("_RightTex");
                saveTextureFile(obj2, textured4, savePath, materialName, "right");
            }
            if (material.HasProperty("_UpTex"))
            {
                Texture2D textured5 = material.GetTexture("_UpTex");
                saveTextureFile(obj2, textured5, savePath, materialName, "up");
            }
            if (material.HasProperty("_DownTex"))
            {
                Texture2D textured6 = material.GetTexture("_DownTex");
                saveTextureFile(obj2, textured6, savePath, materialName, "down");
            }
            string val = Util.FileUtil.getRelativePath(savePath, savePath);
            obj.AddField("path", val);
            Util.FileUtil.saveFile(savePath, obj2);
        }

        public static void saveTextureFile(JSONObject obj, Texture2D texture, string MaterialPath = null, string materialName = null, string nodeName = "path")
        {
            if (texture != null)
            {
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(texture.GetInstanceID());
                string path = cleanIllegalChar(SAVEPATH + "/" + Path.GetDirectoryName(assetPath), false);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                UnityEditor.TextureImporter atPath = UnityEditor.AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                string str = "";
                char[] separator = new char[] { '.' };
                string[] strArray = (SAVEPATH + "/" + assetPath).Split(separator);
                for (int i = 0; i < (strArray.Length - 1); i++)
                {
                    str = str + strArray[i];
                    if (i < (strArray.Length - 2))
                    {
                        str = str + ".";
                    }
                }
                if (findStrsInCurString(assetPath, ConvertOriginalTextureTypeList))
                {
                    if (ConvertToPNG)
                    {
                        str = str + ".png";
                    }
                    else if (ConvertToJPG)
                    {
                        str = str + ".jpg";
                    }
                    str = cleanIllegalChar(str, false);
                    if (!File.Exists(str) || CoverOriginalFile)
                    {
                        atPath.isReadable = true;
                        atPath.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
                        UnityEditor.AssetDatabase.ImportAsset(assetPath);
                        FileStream output = File.Open(str, System.IO.FileMode.Create, FileAccess.ReadWrite);
                        BinaryWriter writer = new BinaryWriter(output);
                        if (ConvertToPNG)
                        {
                            writer.Write(ImageConversion.EncodeToPNG(texture));
                        }
                        else if (ConvertToJPG)
                        {
                            writer.Write(ImageConversion.EncodeToJPG(texture, (int) ConvertQuality));
                        }
                        else
                        {
                            writer.Write(ImageConversion.EncodeToPNG(texture));
                        }
                        output.Close();
                    }
                }
                else
                {
                    str = cleanIllegalChar(SAVEPATH + "/" + assetPath, false);
                    if (!File.Exists(str) || CoverOriginalFile)
                    {
                        if (File.Exists(assetPath))
                        {
                            File.Copy(assetPath, str, true);
                        }
                        else
                        {
                            Debug.LogWarning("LayaAir3D : " + materialName + "has texture can't find!");
                        }
                    }
                }
                if (File.Exists(assetPath))
                {
                    string val = Util.FileUtil.getRelativePath(MaterialPath, str);
                    obj.AddField(nodeName, val);
                    JSONObject obj2 = new JSONObject(JSONObject.Type.OBJECT);
                    obj.AddField("params", obj2);
                    if (atPath != null)
                    {
                        obj2.AddField("mipmap", atPath.mipmapEnabled);
                    }
                    else
                    {
                        obj2.AddField("mipmap", false);
                    }
                    if (texture.get_filterMode() == null)
                    {
                        obj2.AddField("filterMode", 0);
                    }
                    else if (texture.get_filterMode() == 1)
                    {
                        obj2.AddField("filterMode", 1);
                    }
                    else if (texture.get_filterMode() == 2)
                    {
                        obj2.AddField("filterMode", 2);
                    }
                    else
                    {
                        obj2.AddField("filterMode", 1);
                    }
                    if (texture.get_wrapMode() == null)
                    {
                        obj2.AddField("wrapModeU", 0);
                        obj2.AddField("wrapModeV", 0);
                    }
                    else if (texture.get_wrapMode() == 1)
                    {
                        obj2.AddField("wrapModeU", 1);
                        obj2.AddField("wrapModeV", 1);
                    }
                    else
                    {
                        obj2.AddField("wrapModeU", 0);
                        obj2.AddField("wrapModeV", 0);
                    }
                    if (atPath != null)
                    {
                        obj2.AddField("anisoLevel", texture.get_anisoLevel());
                    }
                    else
                    {
                        obj2.AddField("anisoLevel", 0);
                    }
                    if (texture.get_format().ToString() == "DXT1")
                    {
                        obj2.AddField("format", 0);
                    }
                    else if (texture.get_format().ToString() == "DXT5")
                    {
                        obj2.AddField("format", 1);
                    }
                    else
                    {
                        obj2.AddField("format", 1);
                    }
                }
                else
                {
                    obj.AddField(nodeName, "");
                }
            }
            else
            {
                obj.AddField(nodeName, "");
            }
        }

        public static void selectChildByType(GameObject gameObject, ComponentType type, List<GameObject> selectGameObjects, bool onlySon)
        {
            if (gameObject.get_transform().get_childCount() > 0)
            {
                for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                {
                    GameObject obj2 = gameObject.get_transform().GetChild(i).get_gameObject();
                    if (componentsOnGameObject(obj2).IndexOf(type) != -1)
                    {
                        selectGameObjects.Add(obj2);
                    }
                    if (!onlySon)
                    {
                        selectChildByType(obj2, type, selectGameObjects, onlySon);
                    }
                }
            }
        }

        public static GameObject selectParentbyType(GameObject gameObject, ComponentType type)
        {
            if (gameObject.get_transform().get_parent() == null)
            {
                return null;
            }
            GameObject obj2 = gameObject.get_transform().get_parent().get_gameObject();
            if (componentsOnGameObject(obj2).IndexOf(type) != -1)
            {
                return obj2;
            }
            return selectParentbyType(obj2, type);
        }

        public static void SwitchToLayaShader()
        {
            GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            if (rootGameObjects.Length != 0)
            {
                for (int i = 0; i < rootGameObjects.Length; i++)
                {
                    SwitchToLayaShader(rootGameObjects[i].get_gameObject());
                }
            }
        }

        public static void SwitchToLayaShader(GameObject gameObject)
        {
            List<ComponentType> list = componentsOnGameObject(gameObject);
            Shader shader = Shader.Find("LayaAir3D/BlinnPhong");
            Shader shader2 = Shader.Find("LayaAir3D/ShurikenParticle");
            if (list.IndexOf(ComponentType.MeshRenderer) != -1)
            {
                foreach (Material material in gameObject.GetComponent<MeshRenderer>().get_sharedMaterials())
                {
                    if (material != null)
                    {
                        material.set_shader(shader);
                        onChangeLayaBlinnPhong(material);
                    }
                }
            }
            if (list.IndexOf(ComponentType.SkinnedMeshRenderer) != -1)
            {
                foreach (Material material2 in gameObject.GetComponent<SkinnedMeshRenderer>().get_sharedMaterials())
                {
                    if (material2 != null)
                    {
                        material2.set_shader(shader);
                        onChangeLayaBlinnPhong(material2);
                    }
                }
            }
            if (list.IndexOf(ComponentType.ParticleSystem) != -1)
            {
                Material material3 = gameObject.GetComponent<Renderer>().get_sharedMaterial();
                if (material3 != null)
                {
                    material3.set_shader(shader2);
                    onChangeLayaParticle(material3);
                }
            }
            if (gameObject.get_transform().get_childCount() > 0)
            {
                for (int i = 0; i < gameObject.get_transform().get_childCount(); i++)
                {
                    SwitchToLayaShader(gameObject.get_transform().GetChild(i).get_gameObject());
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AniNodeData
        {
            public ushort pathLength;
            public List<ushort> pathIndex;
            public short conpomentTypeIndex;
            public ushort propertyNameIndex;
            public byte frameDataLengthIndex;
            public ushort keyFrameCount;
            public List<DataManager.AniNodeFrameData> aniNodeFrameDatas;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AniNodeFrameData
        {
            public ushort startTimeIndex;
            public List<float> inTangentNumbers;
            public List<float> outTangentNumbers;
            public List<float> valueNumbers;
        }

        public enum ComponentType
        {
            Transform,
            Camera,
            DirectionalLight,
            MeshFilter,
            MeshRenderer,
            SkinnedMeshRenderer,
            Animation,
            Animator,
            ParticleSystem,
            Terrain,
            BoxCollider,
            SphereCollider,
            Rigidbody,
            TrailRenderer
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CustomAnimationClipCurveData
        {
            public DataManager.CustomAnimationCurve curve;
            public string path;
            public string propertyName;
            public System.Type type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CustomAnimationCurve
        {
            public Keyframe[] keys;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FrameData
        {
            public Vector3 localPosition;
            public Quaternion localRotation;
            public Vector3 localScale;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TerrainVertexData
        {
            public Vector3 vertice;
            public Vector3 normal;
            public Vector2 uv;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertexData
        {
            public int index;
            public Vector3 vertice;
            public Vector3 normal;
            public Color color;
            public Vector2 uv;
            public Vector2 uv2;
            public Vector4 boneWeight;
            public Vector4 boneIndex;
            public Vector4 tangent;
        }
    }
}

