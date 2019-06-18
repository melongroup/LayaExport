namespace Util
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal class MathUtil
    {
        public static bool Decompose(Matrix4x4 matrix, out Vector3 scale, out Matrix4x4 rotation, out Vector3 translation)
        {
            translation.x = matrix.m30;
            translation.y = matrix.m31;
            translation.z = matrix.m32;
            scale.x = (float) Math.Sqrt((double) (((matrix.m00 * matrix.m00) + (matrix.m01 * matrix.m01)) + (matrix.m02 * matrix.m02)));
            scale.y = (float) Math.Sqrt((double) (((matrix.m10 * matrix.m10) + (matrix.m11 * matrix.m11)) + (matrix.m12 * matrix.m12)));
            scale.z = (float) Math.Sqrt((double) (((matrix.m20 * matrix.m20) + (matrix.m21 * matrix.m21)) + (matrix.m22 * matrix.m22)));
            if ((IsZero(scale.x) || IsZero(scale.y)) || IsZero(scale.z))
            {
                rotation = Matrix4x4.get_identity();
                return false;
            }
            Vector3 vector = new Vector3(matrix.m20 / scale.z, matrix.m21 / scale.z, matrix.m22 / scale.z);
            Vector3 vector2 = Vector3.Cross(vector, new Vector3(matrix.m00 / scale.x, matrix.m01 / scale.x, matrix.m02 / scale.x));
            Vector3 vector3 = Vector3.Cross(vector2, vector);
            rotation = Matrix4x4.get_identity();
            rotation.m00 = vector3.x;
            rotation.m01 = vector3.y;
            rotation.m02 = vector3.z;
            rotation.m10 = vector2.x;
            rotation.m11 = vector2.y;
            rotation.m12 = vector2.z;
            rotation.m20 = vector.x;
            rotation.m21 = vector.y;
            rotation.m22 = vector.z;
            scale.x = (Vector3.Dot(vector3, new Vector3(matrix.m00, matrix.m01, matrix.m02)) > 0f) ? scale.x : -scale.x;
            scale.y = (Vector3.Dot(vector2, new Vector3(matrix.m10, matrix.m11, matrix.m12)) > 0f) ? scale.y : -scale.y;
            scale.z = (Vector3.Dot(vector, new Vector3(matrix.m20, matrix.m21, matrix.m22)) > 0f) ? scale.z : -scale.z;
            return true;
        }

        public static bool Decompose(Matrix4x4 matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation)
        {
            Matrix4x4 matrixx;
            Decompose(matrix, out scale, out matrixx, out translation);
            RotationMatrix(matrixx, out rotation);
            return true;
        }

        public static float Interpolate(float startX, float endX, float start, float end, float tanPoint1, float tanPoint2, float t, out float tangent)
        {
            tangent = ((((((6f * t) * t) - (6f * t)) * start) + (((((3f * t) * t) - (4f * t)) + 1f) * tanPoint1)) + ((((-6f * t) * t) + (6f * t)) * end)) + ((((3f * t) * t) - (2f * t)) * tanPoint2);
            tangent /= endX - startX;
            return (((((((((2f * t) * t) * t) - ((3f * t) * t)) + 1f) * start) + (((((t * t) * t) - ((2f * t) * t)) + t) * tanPoint1)) + (((((-2f * t) * t) * t) + ((3f * t) * t)) * end)) + ((((t * t) * t) - (t * t)) * tanPoint2));
        }

        public static bool isSimilar(float a, float b)
        {
            return ((a - b) <= 0.001);
        }

        public static bool IsZero(float a)
        {
            return (Math.Abs(a) < 1E-06f);
        }

        public static void RotationMatrix(Matrix4x4 matrix, out Quaternion result)
        {
            float num;
            float num3 = (matrix.m00 + matrix.m11) + matrix.m22;
            if (num3 > 0f)
            {
                num = (float) Math.Sqrt((double) (num3 + 1f));
                result.w = num * 0.5f;
                num = 0.5f / num;
                result.x = (matrix.m12 - matrix.m21) * num;
                result.y = (matrix.m20 - matrix.m02) * num;
                result.z = (matrix.m01 - matrix.m10) * num;
            }
            else
            {
                float num2;
                if ((matrix.m00 >= matrix.m11) && (matrix.m00 >= matrix.m22))
                {
                    num = (float) Math.Sqrt((double) (((1f + matrix.m00) - matrix.m11) - matrix.m22));
                    num2 = 0.5f / num;
                    result.x = 0.5f * num;
                    result.y = (matrix.m01 + matrix.m10) * num2;
                    result.z = (matrix.m02 + matrix.m20) * num2;
                    result.w = (matrix.m12 - matrix.m21) * num2;
                }
                else if (matrix.m11 > matrix.m22)
                {
                    num = (float) Math.Sqrt((double) (((1f + matrix.m11) - matrix.m00) - matrix.m22));
                    num2 = 0.5f / num;
                    result.x = (matrix.m10 + matrix.m01) * num2;
                    result.y = 0.5f * num;
                    result.z = (matrix.m21 + matrix.m12) * num2;
                    result.w = (matrix.m20 - matrix.m02) * num2;
                }
                else
                {
                    num = (float) Math.Sqrt((double) (((1f + matrix.m22) - matrix.m00) - matrix.m11));
                    num2 = 0.5f / num;
                    result.x = (matrix.m20 + matrix.m02) * num2;
                    result.y = (matrix.m21 + matrix.m12) * num2;
                    result.z = 0.5f * num;
                    result.w = (matrix.m01 - matrix.m10) * num2;
                }
            }
        }
    }
}

