using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public class JSONObject
{
    public bool b;
    public long i;
    private const string INFINITY = "\"INFINITY\"";
    public List<string> keys;
    public List<JSONObject> list;
    private const int MAX_DEPTH = 100;
    private const float maxFrameTime = 0.008f;
    public float n;
    private const string NaN = "\"NaN\"";
    private const string NEGINFINITY = "\"NEGINFINITY\"";
    private static readonly Stopwatch printWatch = new Stopwatch();
    public string str;
    public Type type;
    public bool useInt;
    public static readonly char[] WHITESPACE = new char[] { ' ', '\r', '\n', '\t', '﻿', '\t' };

    public JSONObject()
    {
    }

    public JSONObject(AddJSONContents content)
    {
        content(this);
    }

    public JSONObject(Type t)
    {
        this.type = t;
        if (t != Type.OBJECT)
        {
            if (t == Type.ARRAY)
            {
                this.list = new List<JSONObject>();
            }
        }
        else
        {
            this.list = new List<JSONObject>();
            this.keys = new List<string>();
        }
    }

    public JSONObject(bool b)
    {
        this.type = Type.BOOL;
        this.b = b;
    }

    public JSONObject(Dictionary<string, JSONObject> dic)
    {
        this.type = Type.OBJECT;
        this.keys = new List<string>();
        this.list = new List<JSONObject>();
        foreach (KeyValuePair<string, JSONObject> pair in dic)
        {
            this.keys.Add(pair.Key);
            this.list.Add(pair.Value);
        }
    }

    public JSONObject(Dictionary<string, string> dic)
    {
        this.type = Type.OBJECT;
        this.keys = new List<string>();
        this.list = new List<JSONObject>();
        foreach (KeyValuePair<string, string> pair in dic)
        {
            this.keys.Add(pair.Key);
            this.list.Add(CreateStringObject(pair.Value));
        }
    }

    public JSONObject(int i)
    {
        this.type = Type.NUMBER;
        this.i = i;
        this.useInt = true;
        this.n = i;
    }

    public JSONObject(long l)
    {
        this.type = Type.NUMBER;
        this.i = l;
        this.useInt = true;
        this.n = l;
    }

    public JSONObject(float f)
    {
        this.type = Type.NUMBER;
        this.n = f;
    }

    public JSONObject(JSONObject[] objs)
    {
        this.type = Type.ARRAY;
        this.list = new List<JSONObject>(objs);
    }

    public JSONObject(string str, int maxDepth = -2, bool storeExcessLevels = false, bool strict = false)
    {
        this.Parse(str, maxDepth, storeExcessLevels, strict);
    }

    public void Absorb(JSONObject obj)
    {
        this.list.AddRange(obj.list);
        this.keys.AddRange(obj.keys);
        this.str = obj.str;
        this.n = obj.n;
        this.useInt = obj.useInt;
        this.i = obj.i;
        this.b = obj.b;
        this.type = obj.type;
    }

    public void Add(JSONObject obj)
    {
        if (obj != null)
        {
            if (this.type != Type.ARRAY)
            {
                this.type = Type.ARRAY;
                if (this.list == null)
                {
                    this.list = new List<JSONObject>();
                }
            }
            this.list.Add(obj);
        }
    }

    public void Add(AddJSONContents content)
    {
        this.Add(Create(content));
    }

    public void Add(bool val)
    {
        this.Add(Create(val));
    }

    public void Add(int val)
    {
        this.Add(Create(val));
    }

    public void Add(float val)
    {
        this.Add(Create(val));
    }

    public void Add(string str)
    {
        this.Add(CreateStringObject(str));
    }

    public void AddField(string name, JSONObject obj)
    {
        if (obj != null)
        {
            if (this.type != Type.OBJECT)
            {
                if (this.keys == null)
                {
                    this.keys = new List<string>();
                }
                if (this.type == Type.ARRAY)
                {
                    for (int i = 0; i < this.list.Count; i++)
                    {
                        this.keys.Add(i);
                    }
                }
                else if (this.list == null)
                {
                    this.list = new List<JSONObject>();
                }
                this.type = Type.OBJECT;
            }
            this.keys.Add(name);
            this.list.Add(obj);
        }
    }

    public void AddField(string name, AddJSONContents content)
    {
        this.AddField(name, Create(content));
    }

    public void AddField(string name, bool val)
    {
        this.AddField(name, Create(val));
    }

    public void AddField(string name, int val)
    {
        this.AddField(name, Create(val));
    }

    public void AddField(string name, long val)
    {
        this.AddField(name, Create(val));
    }

    public void AddField(string name, float val)
    {
        this.AddField(name, Create(val));
    }

    public void AddField(string name, string val)
    {
        this.AddField(name, CreateStringObject(val));
    }

    public void Bake()
    {
        if (this.type != Type.BAKED)
        {
            this.str = this.Print(true);
            this.type = Type.BAKED;
        }
    }

    public IEnumerable BakeAsync()
    {
        return new <BakeAsync>d__106(-2) { <>4__this = this };
    }

    public void Clear()
    {
        this.type = Type.NULL;
        if (this.list != null)
        {
            this.list.Clear();
        }
        if (this.keys != null)
        {
            this.keys.Clear();
        }
        this.str = "";
        this.n = 0f;
        this.b = false;
    }

    public JSONObject Copy()
    {
        return Create(this.Print(true), -2, false, false);
    }

    public static JSONObject Create()
    {
        return new JSONObject();
    }

    public static JSONObject Create(AddJSONContents content)
    {
        JSONObject self = Create();
        content(self);
        return self;
    }

    public static JSONObject Create(Type t)
    {
        JSONObject obj2 = Create();
        obj2.type = t;
        if (t != Type.OBJECT)
        {
            if (t == Type.ARRAY)
            {
                obj2.list = new List<JSONObject>();
            }
            return obj2;
        }
        obj2.list = new List<JSONObject>();
        obj2.keys = new List<string>();
        return obj2;
    }

    public static JSONObject Create(bool val)
    {
        JSONObject obj1 = Create();
        obj1.type = Type.BOOL;
        obj1.b = val;
        return obj1;
    }

    public static JSONObject Create(Dictionary<string, string> dic)
    {
        JSONObject obj2 = Create();
        obj2.type = Type.OBJECT;
        obj2.keys = new List<string>();
        obj2.list = new List<JSONObject>();
        foreach (KeyValuePair<string, string> pair in dic)
        {
            obj2.keys.Add(pair.Key);
            obj2.list.Add(CreateStringObject(pair.Value));
        }
        return obj2;
    }

    public static JSONObject Create(int val)
    {
        JSONObject obj1 = Create();
        obj1.type = Type.NUMBER;
        obj1.n = val;
        obj1.useInt = true;
        obj1.i = val;
        return obj1;
    }

    public static JSONObject Create(long val)
    {
        JSONObject obj1 = Create();
        obj1.type = Type.NUMBER;
        obj1.n = val;
        obj1.useInt = true;
        obj1.i = val;
        return obj1;
    }

    public static JSONObject Create(float val)
    {
        JSONObject obj1 = Create();
        obj1.type = Type.NUMBER;
        obj1.n = val;
        return obj1;
    }

    public static JSONObject Create(string val, int maxDepth = -2, bool storeExcessLevels = false, bool strict = false)
    {
        JSONObject obj1 = Create();
        obj1.Parse(val, maxDepth, storeExcessLevels, strict);
        return obj1;
    }

    public static JSONObject CreateBakedObject(string val)
    {
        JSONObject obj1 = Create();
        obj1.type = Type.BAKED;
        obj1.str = val;
        return obj1;
    }

    public static JSONObject CreateStringObject(string val)
    {
        JSONObject obj1 = Create();
        obj1.type = Type.STRING;
        obj1.str = val;
        return obj1;
    }

    public JSONObject GetField(string name)
    {
        if (this.IsObject)
        {
            for (int i = 0; i < this.keys.Count; i++)
            {
                if (this.keys[i] == name)
                {
                    return this.list[i];
                }
            }
        }
        return null;
    }

    public void GetField(string name, GetFieldResponse response, FieldNotFound fail = null)
    {
        if ((response != null) && this.IsObject)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                response(this.list[index]);
                return;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
    }

    public bool GetField(ref bool field, string name, FieldNotFound fail = null)
    {
        if (this.type == Type.OBJECT)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                field = this.list[index].b;
                return true;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
        return false;
    }

    public bool GetField(out bool field, string name, bool fallback)
    {
        field = fallback;
        return this.GetField(ref field, name, (FieldNotFound) null);
    }

    public bool GetField(ref int field, string name, FieldNotFound fail = null)
    {
        if (this.IsObject)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                field = (int) this.list[index].n;
                return true;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
        return false;
    }

    public bool GetField(out int field, string name, int fallback)
    {
        field = fallback;
        return this.GetField(ref field, name, (FieldNotFound) null);
    }

    public bool GetField(ref long field, string name, FieldNotFound fail = null)
    {
        if (this.IsObject)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                field = (long) this.list[index].n;
                return true;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
        return false;
    }

    public bool GetField(out long field, string name, long fallback)
    {
        field = fallback;
        return this.GetField(ref field, name, (FieldNotFound) null);
    }

    public bool GetField(ref float field, string name, FieldNotFound fail = null)
    {
        if (this.type == Type.OBJECT)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                field = this.list[index].n;
                return true;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
        return false;
    }

    public bool GetField(out float field, string name, float fallback)
    {
        field = fallback;
        return this.GetField(ref field, name, (FieldNotFound) null);
    }

    public bool GetField(ref string field, string name, FieldNotFound fail = null)
    {
        if (this.IsObject)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                field = this.list[index].str;
                return true;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
        return false;
    }

    public bool GetField(out string field, string name, string fallback)
    {
        field = fallback;
        return this.GetField(ref field, name, (FieldNotFound) null);
    }

    public bool GetField(ref uint field, string name, FieldNotFound fail = null)
    {
        if (this.IsObject)
        {
            int index = this.keys.IndexOf(name);
            if (index >= 0)
            {
                field = (uint) this.list[index].n;
                return true;
            }
        }
        if (fail != null)
        {
            fail(name);
        }
        return false;
    }

    public bool GetField(out uint field, string name, uint fallback)
    {
        field = fallback;
        return this.GetField(ref field, name, (FieldNotFound) null);
    }

    public bool HasField(string name)
    {
        if (this.IsObject)
        {
            for (int i = 0; i < this.keys.Count; i++)
            {
                if (this.keys[i] == name)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasFields(string[] names)
    {
        if (!this.IsObject)
        {
            return false;
        }
        for (int i = 0; i < names.Length; i++)
        {
            if (!this.keys.Contains(names[i]))
            {
                return false;
            }
        }
        return true;
    }

    public void Merge(JSONObject obj)
    {
        MergeRecur(this, obj);
    }

    private static void MergeRecur(JSONObject left, JSONObject right)
    {
        if (left.type == Type.NULL)
        {
            left.Absorb(right);
        }
        else if ((left.type == Type.OBJECT) && (right.type == Type.OBJECT))
        {
            for (int i = 0; i < right.list.Count; i++)
            {
                string name = right.keys[i];
                if (right[i].isContainer)
                {
                    if (left.HasField(name))
                    {
                        MergeRecur(left[name], right[i]);
                    }
                    else
                    {
                        left.AddField(name, right[i]);
                    }
                }
                else if (left.HasField(name))
                {
                    left.SetField(name, right[i]);
                }
                else
                {
                    left.AddField(name, right[i]);
                }
            }
        }
        else if (((left.type == Type.ARRAY) && (right.type == Type.ARRAY)) && (right.Count <= left.Count))
        {
            for (int j = 0; j < right.list.Count; j++)
            {
                if (left[j].type == right[j].type)
                {
                    if (left[j].isContainer)
                    {
                        MergeRecur(left[j], right[j]);
                    }
                    else
                    {
                        left[j] = right[j];
                    }
                }
            }
        }
    }

    public static implicit operator bool(JSONObject o)
    {
        return (o > null);
    }

    private void Parse(string str, int maxDepth = -2, bool storeExcessLevels = false, bool strict = false)
    {
        if (string.IsNullOrEmpty(str))
        {
            this.type = Type.NULL;
        }
        else
        {
            str = str.Trim(WHITESPACE);
            if ((strict && (str[0] != '[')) && (str[0] != '{'))
            {
                this.type = Type.NULL;
            }
            else if (str.Length <= 0)
            {
                this.type = Type.NULL;
            }
            else if (string.Compare(str, "true", true) == 0)
            {
                this.type = Type.BOOL;
                this.b = true;
            }
            else if (string.Compare(str, "false", true) == 0)
            {
                this.type = Type.BOOL;
                this.b = false;
            }
            else if (string.Compare(str, "null", true) == 0)
            {
                this.type = Type.NULL;
            }
            else if (str == "\"INFINITY\"")
            {
                this.type = Type.NUMBER;
                this.n = float.PositiveInfinity;
            }
            else if (str == "\"NEGINFINITY\"")
            {
                this.type = Type.NUMBER;
                this.n = float.NegativeInfinity;
            }
            else if (str == "\"NaN\"")
            {
                this.type = Type.NUMBER;
                this.n = float.NaN;
            }
            else if (str[0] == '"')
            {
                this.type = Type.STRING;
                this.str = str.Substring(1, str.Length - 2);
            }
            else
            {
                int startIndex = 1;
                int num2 = 0;
                char ch = str[num2];
                if (ch != '[')
                {
                    if (ch != '{')
                    {
                        try
                        {
                            this.n = Convert.ToSingle(str);
                            if (!str.Contains("."))
                            {
                                this.i = Convert.ToInt64(str);
                                this.useInt = true;
                            }
                            this.type = Type.NUMBER;
                        }
                        catch (FormatException)
                        {
                            this.type = Type.NULL;
                        }
                        return;
                    }
                    this.type = Type.OBJECT;
                    this.keys = new List<string>();
                    this.list = new List<JSONObject>();
                }
                else
                {
                    this.type = Type.ARRAY;
                    this.list = new List<JSONObject>();
                }
                string item = "";
                bool flag = false;
                bool flag2 = false;
                int num3 = 0;
                while (++num2 < str.Length)
                {
                    if (Array.IndexOf<char>(WHITESPACE, str[num2]) <= -1)
                    {
                        if (str[num2] == '\\')
                        {
                            num2++;
                        }
                        else
                        {
                            if (str[num2] == '"')
                            {
                                if (flag)
                                {
                                    if ((!flag2 && (num3 == 0)) && (this.type == Type.OBJECT))
                                    {
                                        item = str.Substring(startIndex + 1, (num2 - startIndex) - 1);
                                    }
                                    flag = false;
                                }
                                else
                                {
                                    if ((num3 == 0) && (this.type == Type.OBJECT))
                                    {
                                        startIndex = num2;
                                    }
                                    flag = true;
                                }
                            }
                            if (!flag)
                            {
                                if (((this.type == Type.OBJECT) && (num3 == 0)) && (str[num2] == ':'))
                                {
                                    startIndex = num2 + 1;
                                    flag2 = true;
                                }
                                if ((str[num2] == '[') || (str[num2] == '{'))
                                {
                                    num3++;
                                }
                                else if ((str[num2] == ']') || (str[num2] == '}'))
                                {
                                    num3--;
                                }
                                if (((str[num2] == ',') && (num3 == 0)) || (num3 < 0))
                                {
                                    flag2 = false;
                                    string val = str.Substring(startIndex, num2 - startIndex).Trim(WHITESPACE);
                                    if (val.Length > 0)
                                    {
                                        if (this.type == Type.OBJECT)
                                        {
                                            this.keys.Add(item);
                                        }
                                        if (maxDepth != -1)
                                        {
                                            this.list.Add(Create(val, (maxDepth < -1) ? -2 : (maxDepth - 1), false, false));
                                        }
                                        else if (storeExcessLevels)
                                        {
                                            this.list.Add(CreateBakedObject(val));
                                        }
                                    }
                                    startIndex = num2 + 1;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public string Print(bool pretty = true)
    {
        StringBuilder builder = new StringBuilder();
        this.Stringify(0, builder, pretty);
        return builder.ToString();
    }

    public IEnumerable<string> PrintAsync(bool pretty = false)
    {
        return new <PrintAsync>d__108(-2) { <>4__this = this, <>3__pretty = pretty };
    }

    public void RemoveField(string name)
    {
        if (this.keys.IndexOf(name) > -1)
        {
            this.list.RemoveAt(this.keys.IndexOf(name));
            this.keys.Remove(name);
        }
    }

    public void SetField(string name, JSONObject obj)
    {
        if (this.HasField(name))
        {
            this.list.Remove(this[name]);
            this.keys.Remove(name);
        }
        this.AddField(name, obj);
    }

    public void SetField(string name, bool val)
    {
        this.SetField(name, Create(val));
    }

    public void SetField(string name, int val)
    {
        this.SetField(name, Create(val));
    }

    public void SetField(string name, float val)
    {
        this.SetField(name, Create(val));
    }

    public void SetField(string name, string val)
    {
        this.SetField(name, CreateStringObject(val));
    }

    private void Stringify(int depth, StringBuilder builder, bool pretty = true)
    {
        if (depth++ <= 100)
        {
            switch (this.type)
            {
                case Type.NULL:
                    builder.Append("null");
                    return;

                case Type.STRING:
                    builder.AppendFormat("\"{0}\"", this.str);
                    return;

                case Type.NUMBER:
                    if (!this.useInt)
                    {
                        if (float.IsInfinity(this.n))
                        {
                            builder.Append("\"INFINITY\"");
                            return;
                        }
                        if (float.IsNegativeInfinity(this.n))
                        {
                            builder.Append("\"NEGINFINITY\"");
                            return;
                        }
                        if (float.IsNaN(this.n))
                        {
                            builder.Append("\"NaN\"");
                            return;
                        }
                        builder.Append(this.n.ToString());
                        return;
                    }
                    builder.Append(this.i.ToString());
                    return;

                case Type.OBJECT:
                    builder.Append("{");
                    if (this.list.Count > 0)
                    {
                        if (pretty)
                        {
                            builder.Append("\n");
                        }
                        for (int i = 0; i < this.list.Count; i++)
                        {
                            string str = this.keys[i];
                            JSONObject obj2 = this.list[i];
                            if (obj2 != null)
                            {
                                if (pretty)
                                {
                                    for (int j = 0; j < depth; j++)
                                    {
                                        builder.Append("\t");
                                    }
                                }
                                builder.AppendFormat("\"{0}\":", str);
                                obj2.Stringify(depth, builder, pretty);
                                builder.Append(",");
                                if (pretty)
                                {
                                    builder.Append("\n");
                                }
                            }
                        }
                        if (pretty)
                        {
                            builder.Length -= 2;
                        }
                        else
                        {
                            builder.Length--;
                        }
                    }
                    if (pretty && (this.list.Count > 0))
                    {
                        builder.Append("\n");
                        for (int k = 0; k < (depth - 1); k++)
                        {
                            builder.Append("\t");
                        }
                    }
                    builder.Append("}");
                    return;

                case Type.ARRAY:
                    builder.Append("[");
                    if (this.list.Count > 0)
                    {
                        if (pretty)
                        {
                            builder.Append("\n");
                        }
                        for (int m = 0; m < this.list.Count; m++)
                        {
                            if (this.list[m] != null)
                            {
                                if (pretty)
                                {
                                    for (int n = 0; n < depth; n++)
                                    {
                                        builder.Append("\t");
                                    }
                                }
                                this.list[m].Stringify(depth, builder, pretty);
                                builder.Append(",");
                                if (pretty)
                                {
                                    builder.Append("\n");
                                }
                            }
                        }
                        if (pretty)
                        {
                            builder.Length -= 2;
                        }
                        else
                        {
                            builder.Length--;
                        }
                    }
                    if (pretty && (this.list.Count > 0))
                    {
                        builder.Append("\n");
                        for (int num7 = 0; num7 < (depth - 1); num7++)
                        {
                            builder.Append("\t");
                        }
                    }
                    builder.Append("]");
                    return;

                case Type.BOOL:
                    if (!this.b)
                    {
                        builder.Append("false");
                        return;
                    }
                    builder.Append("true");
                    return;

                case Type.BAKED:
                    builder.Append(this.str);
                    return;
            }
        }
    }

    private IEnumerable StringifyAsync(int depth, StringBuilder builder, bool pretty = false)
    {
        return new <StringifyAsync>d__111(-2) { <>4__this = this, <>3__depth = depth, <>3__builder = builder, <>3__pretty = pretty };
    }

    public static JSONObject StringObject(string val)
    {
        return CreateStringObject(val);
    }

    public Dictionary<string, string> ToDictionary()
    {
        if (this.type != Type.OBJECT)
        {
            return null;
        }
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        for (int i = 0; i < this.list.Count; i++)
        {
            JSONObject obj2 = this.list[i];
            switch (obj2.type)
            {
                case Type.STRING:
                    dictionary.Add(this.keys[i], obj2.str);
                    break;

                case Type.NUMBER:
                    dictionary.Add(this.keys[i], obj2.n);
                    break;

                case Type.BOOL:
                    dictionary.Add(this.keys[i], obj2.b.ToString() ?? "");
                    break;
            }
        }
        return dictionary;
    }

    public override string ToString()
    {
        return this.Print(true);
    }

    public string ToString(bool pretty)
    {
        return this.Print(pretty);
    }

    public static JSONObject arr
    {
        get
        {
            return Create(Type.ARRAY);
        }
    }

    public int Count
    {
        get
        {
            if (this.list == null)
            {
                return -1;
            }
            return this.list.Count;
        }
    }

    public float f
    {
        get
        {
            return this.n;
        }
    }

    public bool IsArray
    {
        get
        {
            return (this.type == Type.ARRAY);
        }
    }

    public bool IsBool
    {
        get
        {
            return (this.type == Type.BOOL);
        }
    }

    public bool isContainer
    {
        get
        {
            if (this.type != Type.ARRAY)
            {
                return (this.type == Type.OBJECT);
            }
            return true;
        }
    }

    public bool IsNull
    {
        get
        {
            return (this.type == Type.NULL);
        }
    }

    public bool IsNumber
    {
        get
        {
            return (this.type == Type.NUMBER);
        }
    }

    public bool IsObject
    {
        get
        {
            if (this.type != Type.OBJECT)
            {
                return (this.type == Type.BAKED);
            }
            return true;
        }
    }

    public bool IsString
    {
        get
        {
            return (this.type == Type.STRING);
        }
    }

    public JSONObject this[int index]
    {
        get
        {
            if (this.list.Count > index)
            {
                return this.list[index];
            }
            return null;
        }
        set
        {
            if (this.list.Count > index)
            {
                this.list[index] = value;
            }
        }
    }

    public JSONObject this[string index]
    {
        get
        {
            return this.GetField(index);
        }
        set
        {
            this.SetField(index, value);
        }
    }

    public static JSONObject nullJO
    {
        get
        {
            return Create(Type.NULL);
        }
    }

    public static JSONObject obj
    {
        get
        {
            return Create(Type.OBJECT);
        }
    }

    [CompilerGenerated]
    private sealed class <BakeAsync>d__106 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
    {
        private int <>1__state;
        private object <>2__current;
        public JSONObject <>4__this;
        private IEnumerator<string> <>7__wrap1;
        private int <>l__initialThreadId;
        private string <s>5__1;

        [DebuggerHidden]
        public <BakeAsync>d__106(int <>1__state)
        {
            this.<>1__state = <>1__state;
            this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void <>m__Finally1()
        {
            this.<>1__state = -1;
            if (this.<>7__wrap1 != null)
            {
                this.<>7__wrap1.Dispose();
            }
        }

        private bool MoveNext()
        {
            try
            {
                int num = this.<>1__state;
                JSONObject obj2 = this.<>4__this;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    if (obj2.type != JSONObject.Type.BAKED)
                    {
                        this.<>7__wrap1 = obj2.PrintAsync(false).GetEnumerator();
                        this.<>1__state = -3;
                        while (this.<>7__wrap1.MoveNext())
                        {
                            this.<s>5__1 = this.<>7__wrap1.Current;
                            if (this.<s>5__1 != null)
                            {
                                goto Label_0085;
                            }
                            this.<>2__current = this.<s>5__1;
                            this.<>1__state = 1;
                            return true;
                        Label_007B:
                            this.<>1__state = -3;
                            goto Label_0091;
                        Label_0085:
                            obj2.str = this.<s>5__1;
                        Label_0091:
                            this.<s>5__1 = null;
                        }
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        obj2.type = JSONObject.Type.BAKED;
                    }
                    return false;
                }
                if (num != 1)
                {
                    return false;
                }
                goto Label_007B;
            }
            fault
            {
                this.System.IDisposable.Dispose();
            }
        }

        [DebuggerHidden]
        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Thread.CurrentThread.ManagedThreadId))
            {
                this.<>1__state = 0;
                return this;
            }
            return new JSONObject.<BakeAsync>d__106(0) { <>4__this = this.<>4__this };
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.System.Collections.Generic.IEnumerable<System.Object>.GetEnumerator();
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            switch (this.<>1__state)
            {
                case -3:
                case 1:
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                    break;
            }
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.<>2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.<>2__current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <PrintAsync>d__108 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
    {
        private int <>1__state;
        private string <>2__current;
        public bool <>3__pretty;
        public JSONObject <>4__this;
        private IEnumerator <>7__wrap1;
        private int <>l__initialThreadId;
        private StringBuilder <builder>5__1;
        private bool pretty;

        [DebuggerHidden]
        public <PrintAsync>d__108(int <>1__state)
        {
            this.<>1__state = <>1__state;
            this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void <>m__Finally1()
        {
            this.<>1__state = -1;
            IDisposable disposable = this.<>7__wrap1 as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private bool MoveNext()
        {
            try
            {
                int num = this.<>1__state;
                JSONObject obj2 = this.<>4__this;
                switch (num)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<builder>5__1 = new StringBuilder();
                        JSONObject.printWatch.Reset();
                        JSONObject.printWatch.Start();
                        this.<>7__wrap1 = obj2.StringifyAsync(0, this.<builder>5__1, this.pretty).GetEnumerator();
                        this.<>1__state = -3;
                        goto Label_00A0;

                    case 1:
                        this.<>1__state = -3;
                        goto Label_00A0;

                    case 2:
                        this.<>1__state = -1;
                        return false;

                    default:
                        return false;
                }
            Label_0075:
                IEnumerable enumerable1 = (IEnumerable) this.<>7__wrap1.Current;
                this.<>2__current = null;
                this.<>1__state = 1;
                return true;
            Label_00A0:
                if (this.<>7__wrap1.MoveNext())
                {
                    goto Label_0075;
                }
                this.<>m__Finally1();
                this.<>7__wrap1 = null;
                this.<>2__current = this.<builder>5__1.ToString();
                this.<>1__state = 2;
                return true;
            }
            fault
            {
                this.System.IDisposable.Dispose();
            }
        }

        [DebuggerHidden]
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            JSONObject.<PrintAsync>d__108 d__;
            if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Thread.CurrentThread.ManagedThreadId))
            {
                this.<>1__state = 0;
                d__ = this;
            }
            else
            {
                d__ = new JSONObject.<PrintAsync>d__108(0) {
                    <>4__this = this.<>4__this
                };
            }
            d__.pretty = this.<>3__pretty;
            return d__;
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            switch (this.<>1__state)
            {
                case -3:
                case 1:
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                    break;
            }
        }

        string IEnumerator<string>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.<>2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.<>2__current;
            }
        }
    }

    [CompilerGenerated]
    private sealed class <StringifyAsync>d__111 : IEnumerable<object>, IEnumerable, IEnumerator<object>, IDisposable, IEnumerator
    {
        private int <>1__state;
        private object <>2__current;
        public StringBuilder <>3__builder;
        public int <>3__depth;
        public bool <>3__pretty;
        public JSONObject <>4__this;
        private IEnumerator <>7__wrap1;
        private int <>l__initialThreadId;
        private int <i>5__1;
        private int <i>5__2;
        private StringBuilder builder;
        private int depth;
        private bool pretty;

        [DebuggerHidden]
        public <StringifyAsync>d__111(int <>1__state)
        {
            this.<>1__state = <>1__state;
            this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        private void <>m__Finally1()
        {
            this.<>1__state = -1;
            IDisposable disposable = this.<>7__wrap1 as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private void <>m__Finally2()
        {
            this.<>1__state = -1;
            IDisposable disposable = this.<>7__wrap1 as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        private bool MoveNext()
        {
            bool flag;
            try
            {
                int depth;
                int num = this.<>1__state;
                JSONObject obj2 = this.<>4__this;
                switch (num)
                {
                    case 0:
                        this.<>1__state = -1;
                        depth = this.depth;
                        this.depth = depth + 1;
                        if (depth <= 100)
                        {
                            break;
                        }
                        return false;

                    case 1:
                        goto Label_008B;

                    case 2:
                        goto Label_02B8;

                    case 3:
                        goto Label_04B6;

                    default:
                        return false;
                }
                if (JSONObject.printWatch.Elapsed.TotalSeconds <= 0.00800000037997961)
                {
                    goto Label_009C;
                }
                JSONObject.printWatch.Reset();
                this.<>2__current = null;
                this.<>1__state = 1;
                return true;
            Label_008B:
                this.<>1__state = -1;
                JSONObject.printWatch.Start();
            Label_009C:
                switch (obj2.type)
                {
                    case JSONObject.Type.NULL:
                        this.builder.Append("null");
                        goto Label_05FB;

                    case JSONObject.Type.STRING:
                        this.builder.AppendFormat("\"{0}\"", obj2.str);
                        goto Label_05FB;

                    case JSONObject.Type.NUMBER:
                        if (!obj2.useInt)
                        {
                            break;
                        }
                        this.builder.Append(obj2.i.ToString());
                        goto Label_05FB;

                    case JSONObject.Type.OBJECT:
                        this.builder.Append("{");
                        if (obj2.list.Count > 0)
                        {
                            if (this.pretty)
                            {
                                this.builder.Append("\n");
                            }
                            this.<i>5__1 = 0;
                            while (this.<i>5__1 < obj2.list.Count)
                            {
                                string str = obj2.keys[this.<i>5__1];
                                JSONObject obj3 = obj2.list[this.<i>5__1];
                                if (obj3 != null)
                                {
                                    if (this.pretty)
                                    {
                                        for (int i = 0; i < this.depth; i++)
                                        {
                                            this.builder.Append("\t");
                                        }
                                    }
                                    this.builder.AppendFormat("\"{0}\":", str);
                                    this.<>7__wrap1 = obj3.StringifyAsync(this.depth, this.builder, this.pretty).GetEnumerator();
                                    this.<>1__state = -3;
                                    while (this.<>7__wrap1.MoveNext())
                                    {
                                        IEnumerable current = (IEnumerable) this.<>7__wrap1.Current;
                                        this.<>2__current = current;
                                        this.<>1__state = 2;
                                        return true;
                                    Label_02B8:
                                        this.<>1__state = -3;
                                    }
                                    this.<>m__Finally1();
                                    this.<>7__wrap1 = null;
                                    this.builder.Append(",");
                                    if (this.pretty)
                                    {
                                        this.builder.Append("\n");
                                    }
                                }
                                depth = this.<i>5__1;
                                this.<i>5__1 = depth + 1;
                            }
                            if (this.pretty)
                            {
                                this.builder.Length -= 2;
                            }
                            else
                            {
                                this.builder.Length--;
                            }
                        }
                        if (this.pretty && (obj2.list.Count > 0))
                        {
                            this.builder.Append("\n");
                            for (int j = 0; j < (this.depth - 1); j++)
                            {
                                this.builder.Append("\t");
                            }
                        }
                        this.builder.Append("}");
                        goto Label_05FB;

                    case JSONObject.Type.ARRAY:
                        this.builder.Append("[");
                        if (obj2.list.Count > 0)
                        {
                            if (this.pretty)
                            {
                                this.builder.Append("\n");
                            }
                            this.<i>5__2 = 0;
                            while (this.<i>5__2 < obj2.list.Count)
                            {
                                if (obj2.list[this.<i>5__2] != null)
                                {
                                    if (this.pretty)
                                    {
                                        for (int k = 0; k < this.depth; k++)
                                        {
                                            this.builder.Append("\t");
                                        }
                                    }
                                    this.<>7__wrap1 = obj2.list[this.<i>5__2].StringifyAsync(this.depth, this.builder, this.pretty).GetEnumerator();
                                    this.<>1__state = -4;
                                    while (this.<>7__wrap1.MoveNext())
                                    {
                                        IEnumerable enumerable2 = (IEnumerable) this.<>7__wrap1.Current;
                                        this.<>2__current = enumerable2;
                                        this.<>1__state = 3;
                                        return true;
                                    Label_04B6:
                                        this.<>1__state = -4;
                                    }
                                    this.<>m__Finally2();
                                    this.<>7__wrap1 = null;
                                    this.builder.Append(",");
                                    if (this.pretty)
                                    {
                                        this.builder.Append("\n");
                                    }
                                }
                                depth = this.<i>5__2;
                                this.<i>5__2 = depth + 1;
                            }
                            if (this.pretty)
                            {
                                this.builder.Length -= 2;
                            }
                            else
                            {
                                this.builder.Length--;
                            }
                        }
                        if (this.pretty && (obj2.list.Count > 0))
                        {
                            this.builder.Append("\n");
                            for (int m = 0; m < (this.depth - 1); m++)
                            {
                                this.builder.Append("\t");
                            }
                        }
                        this.builder.Append("]");
                        goto Label_05FB;

                    case JSONObject.Type.BOOL:
                        if (!obj2.b)
                        {
                            goto Label_05D7;
                        }
                        this.builder.Append("true");
                        goto Label_05FB;

                    case JSONObject.Type.BAKED:
                        this.builder.Append(obj2.str);
                        goto Label_05FB;

                    default:
                        goto Label_05FB;
                }
                if (float.IsInfinity(obj2.n))
                {
                    this.builder.Append("\"INFINITY\"");
                }
                else if (float.IsNegativeInfinity(obj2.n))
                {
                    this.builder.Append("\"NEGINFINITY\"");
                }
                else if (float.IsNaN(obj2.n))
                {
                    this.builder.Append("\"NaN\"");
                }
                else
                {
                    this.builder.Append(obj2.n.ToString());
                }
                goto Label_05FB;
            Label_05D7:
                this.builder.Append("false");
            Label_05FB:
                flag = false;
            }
            fault
            {
                this.System.IDisposable.Dispose();
            }
            return flag;
        }

        [DebuggerHidden]
        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            JSONObject.<StringifyAsync>d__111 d__;
            if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Thread.CurrentThread.ManagedThreadId))
            {
                this.<>1__state = 0;
                d__ = this;
            }
            else
            {
                d__ = new JSONObject.<StringifyAsync>d__111(0) {
                    <>4__this = this.<>4__this
                };
            }
            d__.depth = this.<>3__depth;
            d__.builder = this.<>3__builder;
            d__.pretty = this.<>3__pretty;
            return d__;
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.System.Collections.Generic.IEnumerable<System.Object>.GetEnumerator();
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            int num = this.<>1__state;
            if (num <= -3)
            {
                if (num == -4)
                {
                    goto Label_002A;
                }
                if (num != -3)
                {
                    return;
                }
            }
            else if (num != 2)
            {
                if (num != 3)
                {
                    return;
                }
                goto Label_002A;
            }
            try
            {
                return;
            }
            finally
            {
                this.<>m__Finally1();
            }
        Label_002A:;
            try
            {
            }
            finally
            {
                this.<>m__Finally2();
            }
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.<>2__current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.<>2__current;
            }
        }
    }

    public delegate void AddJSONContents(JSONObject self);

    public delegate void FieldNotFound(string name);

    public delegate void GetFieldResponse(JSONObject obj);

    public enum Type
    {
        NULL,
        STRING,
        NUMBER,
        OBJECT,
        ARRAY,
        BOOL,
        BAKED
    }
}

