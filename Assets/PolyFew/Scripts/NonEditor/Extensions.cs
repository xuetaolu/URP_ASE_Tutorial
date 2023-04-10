//////////////////////////////////////////////////////
// Copyright (c) BrainFailProductions
//////////////////////////////////////////////////////

#if UNITY_EDITOR


using UnityEngine;
using System.Reflection;
using System;
using UnityEditor;



namespace BrainFailProductions.PolyFew
{


    public static class Extensions
    {
        public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var localToWorldMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            return localToWorldMatrix.MultiplyPoint3x4(position);
        }

        public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
        {
            var worldToLocalMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one).inverse;
            return worldToLocalMatrix.MultiplyPoint3x4(position);
        }



        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match

            BindingFlags flags = PrivateValueAccessor.Flags;
            PropertyInfo[] pinfos = type.GetProperties(flags);

            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }




        public static T AddComponentClone<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }




        public static T MakeSimilarTo<T>(this UnityEngine.Object copyTo, T copyFrom) where T : UnityEngine.Object
        {
            Type type = copyTo.GetType();
            if (type != copyFrom.GetType()) return null; // type mis-match

            BindingFlags flags = PrivateValueAccessor.Flags;
            PropertyInfo[] pinfos = type.GetProperties(flags);

            foreach (var pinfo in pinfos)
            {

                if (pinfo.CanWrite)
                {
                    //Debug.Log("Propery Name:  " + pinfo.Name);
                    try
                    {
                        pinfo.SetValue(copyTo, pinfo.GetValue(copyFrom, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown.
                }
            }


            FieldInfo[] finfos = type.GetFields(flags);

            foreach (var finfo in finfos)
            {
                //Debug.Log("Field Name:  " + finfo.Name);

                finfo.SetValue(copyTo, finfo.GetValue(copyFrom));
            }
            return copyTo as T;
        }




        public static T MakeSimilarToOtherMesh<T>(this Mesh copyTo, T copyFrom) where T : UnityEngine.Object
        {
            Type type = copyTo.GetType();
            if (type != copyFrom.GetType()) return null; // type mis-match

#pragma warning disable

            Mesh mesh = new Mesh();

            BindingFlags flags = PrivateValueAccessor.Flags;

            PropertyInfo[] pinfos = type.GetProperties(flags);

#pragma warning disable

            PropertyInfo trianglesProp = type.GetProperty("triangles", flags);
            PropertyInfo verticesProp = type.GetProperty("vertices", flags);
            PropertyInfo bonesProp = type.GetProperty("boneWeights", flags);


            bool vertsFirst = copyTo.triangles.Length <= (copyFrom as Mesh).triangles.Length;

            if (!vertsFirst) { trianglesProp.SetValue(copyTo, trianglesProp.GetValue(copyFrom, null), null); }

            //bonesProp.SetValue(copyTo, bonesProp.GetValue(copyFrom, null), null);
            //mesh.vertices = new Vector3[(copyFrom as Mesh).vertices.Length];
            //mesh.triangles = new int[(copyFrom as Mesh).triangles.Length];
            //mesh.boneWeights = new BoneWeight[(copyFrom as Mesh).vertices.Length];


            for (int a = 0; a < pinfos.Length; a++)
            {
                PropertyInfo pinfo = pinfos[a];

                if (!vertsFirst && pinfo.Name == "triangles") { continue; }
                if (pinfo.Name == "boneWeights") { continue; }

                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(copyTo, pinfo.GetValue(copyFrom, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown.
                }
            }

            bonesProp.SetValue(copyTo, bonesProp.GetValue(copyFrom, null), null);


            FieldInfo[] finfos = type.GetFields(flags);

            foreach (var finfo in finfos)
            {
                finfo.SetValue(copyTo, finfo.GetValue(copyFrom));
            }

            return copyTo as T;
        }



        public static void RemoveNullObjects(this SerializedProperty prop)
        {
            for (int i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetAt(i).GetValue() == null)
                {
                    prop.RemoveAt(i);
                    i--;
                }
            }
        }
        public static bool Contains(this SerializedProperty prop, System.Object value)
        {
            for (int i = 0, size = prop.arraySize; i < size; i++)
            {
                if (prop.GetAt(i).GetValue() == value)
                    return true;
            }
            return false;
        }
        public static void MoveUp(this SerializedProperty prop, int fromIndex)
        {
            AssertArray(prop);
            AssertNotEmpty(prop);
            int previous = fromIndex - 1;
            if (previous < 0) previous = prop.arraySize - 1;
            prop.Swap(fromIndex, previous);
        }
        public static void MoveDown(this SerializedProperty prop, int fromIndex)
        {
            AssertArray(prop);
            AssertNotEmpty(prop);
            int next = (fromIndex + 1) % prop.arraySize;
            prop.Swap(fromIndex, next);
        }
        public static void Swap(this SerializedProperty prop, int i, int j)
        {
            AssertArray(prop);
            AssertNotEmpty(prop);
            var value1 = prop.GetObjectValueAt(i);
            var value2 = prop.GetObjectValueAt(j);
            System.Object temp = value1;
            prop.SetObjectValueAt(i, value2);
            prop.SetObjectValueAt(j, temp);
        }
        public static bool ContainsReferenceTypes(this SerializedProperty prop)
        {
            AssertArray(prop);
            AssertNotEmpty(prop);
            return prop.GetFirst().IsReferenceType();
        }
        public static bool IsReferenceType(this SerializedProperty prop)
        {
            return prop.propertyType == SerializedPropertyType.ObjectReference;
        }
        public static void Add(this SerializedProperty prop, UnityEngine.Object value)
        {
            AssertArray(prop);
            prop.arraySize++;
            prop.GetAt(prop.arraySize - 1).objectReferenceValue = value;
        }
        public static void Add(this SerializedProperty prop)
        {
            AssertArray(prop);
            prop.arraySize++;
            prop.GetLast().SetToDefault();
        }
        public static void SetToDefault(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = default(int);
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = default(float);
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = default(bool);
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = default(Color);
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = default(Bounds);
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = default(Rect);
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = default(Vector2);
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = default(Vector3);
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = null;
                    break;
            }
        }
        public static SerializedProperty GetLast(this SerializedProperty prop)
        {
            AssertArray(prop);
            return prop.GetAt(prop.arraySize - 1);
        }
        public static SerializedProperty GetFirst(this SerializedProperty prop)
        {
            AssertArray(prop);
            return prop.GetAt(0);
        }
        public static void AssertArray(this SerializedProperty prop)
        {
            if (!prop.isArray)
                throw new UnityException("SerializedProperty `" + prop.name + "` is not an array. Yet you're trying to index it!");
        }
        public static void RemoveAt(this SerializedProperty prop, int atIndex)
        {
            AssertArray(prop);
            AssertNotEmpty(prop);

            for (int i = atIndex, size = prop.arraySize; i < size - 1; i++)
            {
                prop.SetObjectValueAt(i, prop.GetObjectValueAt(i + 1));
            }
            prop.arraySize--;
        }
        public static void AssertNotEmpty(this SerializedProperty prop)
        {
            if (prop.arraySize <= 0)
                throw new UnityException("Array `" + prop.name + "` is empty. You can't do anything with it!");
        }
        public static System.Object GetObjectValueAt(this SerializedProperty prop, int i)
        {
            //AssertArray(prop);
            //AssertNotEmpty(prop);
            return prop.GetAt(i).GetValue();
        }
        public static void SetObjectValueAt(this SerializedProperty prop, int i, System.Object toValue)
        {
            AssertArray(prop);
            AssertNotEmpty(prop);
            prop.GetAt(i).SetObjectValue(toValue);
        }
        public static void SetObjectValue(this SerializedProperty prop, System.Object toValue)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (bool)toValue;
                    break;
                case SerializedPropertyType.Bounds:
                    prop.boundsValue = (Bounds)toValue;
                    break;
                case SerializedPropertyType.Color:
                    prop.colorValue = (Color)toValue;
                    break;
                case SerializedPropertyType.Float:
                    prop.floatValue = (float)toValue;
                    break;
                case SerializedPropertyType.Integer:
                    prop.intValue = (int)toValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.objectReferenceValue = toValue as UnityEngine.Object;
                    break;
                case SerializedPropertyType.Rect:
                    prop.rectValue = (Rect)toValue;
                    break;
                case SerializedPropertyType.String:
                    prop.stringValue = (string)toValue;
                    break;
                case SerializedPropertyType.Vector2:
                    prop.vector2Value = (Vector2)toValue;
                    break;
                case SerializedPropertyType.Vector3:
                    prop.vector3Value = (Vector3)toValue;
                    break;
            }
        }
        public static System.Object GetValue(this SerializedProperty prop)
        {

#pragma warning disable

            switch (prop.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                    break;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                    break;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                    break;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                    break;
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                    break;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                    break;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                    break;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                default: return null;
            }
        }
        public static T GetValue<T>(this SerializedProperty prop)
        {
            return (T)prop.GetValue();
        }
        public static SerializedProperty GetAt(this SerializedProperty prop, int i)
        {
            //AssertArray(prop);
            return prop.GetArrayElementAtIndex(i);
        }


    }

}


#endif