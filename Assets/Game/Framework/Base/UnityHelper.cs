using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace MOYV
{
    public class UnityHelper : MonoBehaviour
    {
        #region GameObjects

        public static T FindComponent<T>(GameObject go, string name) where T : Component
        {
            go = FindGameObjectByNameInAllChildren(go, name);
            return go == null ? null : go.GetComponent<T>();
        }

        public static GameObject FindGameObjectByName(GameObject parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;

            Transform transform = parent.transform;
            foreach (Transform t in transform)
            {
                if (t.gameObject.name == name)
                    return t.gameObject;
            }

            return null;
        }

        public static GameObject FindGameObjectByNameInAllChildren(GameObject parent, string name)
        {
            if (parent == null)
                return null;

            GameObject g = FindGameObjectByName(parent, name);
            if (g != null)
                return g;

            Transform transform = parent.transform;
            foreach (Transform t in transform)
            {
                g = FindGameObjectByNameInAllChildren(t.gameObject, name);
                if (g != null)
                    return g;
            }

            return null;
        }

        public static GameObject InstantiatePrefab(string prefabFullpath)
        {
            return GameObject.Instantiate(Resources.Load(prefabFullpath)) as GameObject;
        }

        public static GameObject InstantiatePrefab(string prefabFullpath, GameObject parent)
        {
            return GameObject.Instantiate(Resources.Load(prefabFullpath), parent.transform) as GameObject;
        }

        public static GameObject InstantiatePrefab(string prefabFullpath, Vector3 pos, Quaternion rotation,
            GameObject parent)
        {
            return GameObject.Instantiate(Resources.Load(prefabFullpath), pos, rotation,
                parent.transform) as GameObject;
        }

        public static GameObject AddChild(GameObject parent, GameObject child, bool layerFollowParent = true)
        {
            if (layerFollowParent)
                child.layer = parent.layer;

            child.transform.parent = parent.transform;
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        #endregion


        #region Math

        /// <summary>
        /// 点到线断最近的一个点位置和距离
        /// </summary>
        /// <param name="linePt1"> 线段起始点</param>
        /// <param name="linePt2"> 线段终点</param>
        /// <param name="point">任意一点</param>
        /// <param name="retPoint">out 相交点</param>
        /// <param name="d">out 点到线段的最近距离</param>
        /// <returns>是否有垂线与线段相交</returns>
        public static bool ClosestPointOnLine(Vector2 linePt1, Vector2 linePt2, Vector2 point, out Vector2 retPoint,
            out float d)
        {
            Matrix4x4 mat;
            Matrix4x4 mat_inv;
            Vector2 p2;
            mat = Matrix4x4.TRS(linePt1,
                Quaternion.Euler(0, 0, CalcIncludedAngle(Vector2.right, linePt2 - linePt1)),
                Vector3.one);
            mat_inv = mat.inverse;
            point = mat_inv.MultiplyPoint(point);
            p2 = mat_inv.MultiplyPoint(linePt2);

            bool ret;
            ret = (point.x > 0) != (point.x > p2.x);

            if (ret)
            {
                d = Mathf.Abs(point.y);
                retPoint = mat.MultiplyPoint(new Vector3(point.x, 0, 0));
            }
            else
            {
                float d1, d2;
                d1 = point.magnitude;
                d2 = (point - p2).magnitude;
                d = Mathf.Min(d1, d2);
                retPoint = d1 < d2 ? linePt1 : linePt2;
            }

            return ret;
        }

        public static bool IsPointInPoly(Vector2 testPoint, Vector2[] poly)
        {
            bool ret = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if ((poly[i].y > testPoint.y) != (poly[j].y > testPoint.y) &&
                    testPoint.x > poly[i].x +
                    (poly[j].x - poly[i].x) * (poly[i].y - testPoint.y) / (poly[i].y - poly[j].y))
                {
                    ret = !ret;
                }
            }

            return ret;
        }

        //实例化对象并配置UILabel文本(UILabel 的名字要以@开头)
        public static GameObject InstantiateDump(string prefabFullPath, GameObject parent = null)
        {
            GameObject go;
            go = InstantiatePrefab(prefabFullPath, parent);
            return go;
        }

        /// <summary>
        /// 判断点是否在直线上
        /// </summary>
        /// <param name="point">任意点</param>
        /// <param name="start">直线起点</param>
        /// <param name="end">直线终点</param>
        /// <returns>返回值越接近0就是表示点越靠近反之越远。当为0时，点完全在线上</returns>
        public static float IsPointOnLine(Vector2 p, Vector2 start, Vector2 end)
        {
            return (start.x - p.x) * (end.y - p.y) - (end.x - p.x) * (start.y - p.y);
        }

        public static Vector2 CalcLineIntersection(Vector2 p1, Vector2 p2, //第一根直线
            Vector2 p3, Vector2 p4) //第二根直线
        {
            Vector2 result = new Vector2();
            float left, right;

            left = (p2.y - p1.y) * (p4.x - p3.x) - (p4.y - p3.y) * (p2.x - p1.x);
            right = (p3.y - p1.y) * (p2.x - p1.x) * (p4.x - p3.x) + (p2.y - p1.y) * (p4.x - p3.x) * p1.x -
                    (p4.y - p3.y) * (p2.x - p1.x) * p3.x;
            result.x = right / left;

            left = (p2.x - p1.x) * (p4.y - p3.y) - (p4.x - p3.x) * (p2.y - p1.y);
            right = (p3.x - p1.x) * (p2.y - p1.y) * (p4.y - p3.y) + p1.y * (p2.x - p1.x) * (p4.y - p3.y) -
                    p3.y * (p4.x - p3.x) * (p2.y - p1.y);
            result.y = right / left;
            return result;
        }

        protected class Vector2Node
        {
            public Vector2 point = Vector2.zero;
            public Vector2Node next = null;
        }

        public static void OptimizePolygon(Vector2[] inpoints, out Vector2[] outpoints, float optimization = 1.0f)
        {
            Vector2Node head, now;

            now = new Vector2Node();
            now.point = inpoints[0];
            head = now;

            for (int i = 1; i < inpoints.Length; ++i)
            {
                now.next = new Vector2Node();
                now = now.next;
                now.point = inpoints[i];
            }

            now.next = head;

            now = head;
            do
            {
                if (Mathf.Abs(IsPointOnLine(now.next.point, now.point, now.next.next.point)) < optimization)
                {
                    if (now.next == head)
                    {
                        now.next = now.next.next;
                        break;
                    }

                    now.next = now.next.next;
                }
                else
                {
                    now = now.next;
                }
            } while (head != now);

            List<Vector2> plist;
            plist = new List<Vector2>(8);
            head = now;
            do
            {
                plist.Add(now.point);
                now = now.next;
            } while (head != now);

            outpoints = (plist.Count > 0) ? plist.ToArray() : null;
        }

        // -180 ---- 180.
        public static float CalcIncludedAngle(Vector3 from, Vector3 to)
        {
            Vector2 v1, v2;
            @from.y = @from.z;
            to.y = to.z;

            v1 = @from;
            v2 = to;

            return CalcIncludedAngle(v1, v2);
        }

        public static float CalcIncludedAngle(Vector2 from, Vector2 to)
        {
            Vector3 v3;
            v3 = Vector3.Cross(@from, to);
            return v3.z > 0 ? Vector2.Angle(@from, to) : -Vector2.Angle(@from, to);
        }

        public static Bounds CalcGamaObjectBoundsInWorld(GameObject go)
        {
            Bounds bounds;
            MeshFilter mf;
            Vector3 min, max;
            Transform trans = go.transform;
            Vector3[] point;

            max = Vector3.one * float.MinValue;
            min = Vector3.one * float.MaxValue;
            bounds = new Bounds();
            point = new Vector3[8];

            if (!go.activeInHierarchy)
                return bounds;

            mf = go.GetComponent<MeshFilter>();
            if (mf && mf.sharedMesh)
            {
                mf.sharedMesh.RecalculateBounds();
                bounds = mf.sharedMesh.bounds;
            }
            else
            {
                SkinnedMeshRenderer smr;
                smr = go.GetComponent<SkinnedMeshRenderer>();
                if (smr && smr.rootBone)
                {
                    trans = smr.rootBone;
                    bounds = smr.localBounds;
                }
            }

            Vector3 e = bounds.extents;
            point[0] = bounds.center + new Vector3(-e.x, e.y, e.z);
            point[1] = bounds.center + new Vector3(-e.x, e.y, -e.z);
            point[2] = bounds.center + new Vector3(e.x, e.y, e.z);
            point[3] = bounds.center + new Vector3(e.x, e.y, -e.z);

            point[4] = bounds.center + new Vector3(-e.x, -e.y, e.z);
            point[5] = bounds.center + new Vector3(-e.x, -e.y, -e.z);
            point[6] = bounds.center + new Vector3(e.x, -e.y, e.z);
            point[7] = bounds.center + new Vector3(e.x, -e.y, -e.z);

            for (int i = 0; i < point.Length; ++i)
                point[i] = trans.localToWorldMatrix.MultiplyPoint(point[i]);

            for (int i = 0; i < point.Length; ++i)
            {
                min.x = Mathf.Min(point[i].x, min.x);
                min.y = Mathf.Min(point[i].y, min.y);
                min.z = Mathf.Min(point[i].z, min.z);

                max.x = Mathf.Max(point[i].x, max.x);
                max.y = Mathf.Max(point[i].y, max.y);
                max.z = Mathf.Max(point[i].z, max.z);
            }

            bounds.SetMinMax(min, max);

            for (int i = 0; i < go.transform.childCount; ++i)
            {
                bounds.Encapsulate(CalcGamaObjectBoundsInWorld(go.transform.GetChild(i).gameObject));
            }

            return bounds;
        }

        public static float Clamp(float value, float min, float max, float bufferFactor, float damping = 2.0f)
        {
            float result = 0.0f;
            result = Mathf.Clamp(value, min, max);
            if (Mathf.Abs(value - result) > 0.001f)
            {
                float overDistance;
                float distance;
                float ratio;

                overDistance = Mathf.Abs(value - result);
                distance = Mathf.Abs(max - min);

                ratio = overDistance / distance;
                ratio = Mathf.Min(1.0f, ratio / damping);
                ratio = Mathf.Sin(ratio * Mathf.PI / 2.0f);

                float buffer;
                buffer = Mathf.Abs(bufferFactor) * Mathf.Abs(max - min);
                buffer *= ratio;

                result += (value - result > 0) ? buffer : -buffer;
            }

            return result;
        }

        private static float[] s_sin_values =
        {
            0.00000f, 0.01745f, 0.03490f, 0.05234f, 0.06976f, 0.08716f, 0.10453f, 0.12187f, 0.13917f, 0.15643f,
            0.17365f, 0.19081f, 0.20791f, 0.22495f, 0.24192f, 0.25882f, 0.27564f, 0.29237f, 0.30902f, 0.32557f,
            0.34202f, 0.35837f, 0.37461f, 0.39073f, 0.40674f, 0.42262f, 0.43837f, 0.45399f, 0.46947f, 0.48481f,
            0.50000f, 0.51504f, 0.52992f, 0.54464f, 0.55919f, 0.57358f, 0.58779f, 0.60182f, 0.61566f, 0.62932f,
            0.64279f, 0.65606f, 0.66913f, 0.68200f, 0.69466f, 0.70711f, 0.71934f, 0.73135f, 0.74314f, 0.75471f,
            0.76604f, 0.77715f, 0.78801f, 0.79864f, 0.80902f, 0.81915f, 0.82904f, 0.83867f, 0.84805f, 0.85717f,
            0.86603f, 0.87462f, 0.88295f, 0.89101f, 0.89879f, 0.90631f, 0.91355f, 0.92050f, 0.92718f, 0.93358f,
            0.93969f, 0.94552f, 0.95106f, 0.95630f, 0.96126f, 0.96593f, 0.97030f, 0.97437f, 0.97815f, 0.98163f,
            0.98481f, 0.98769f, 0.99027f, 0.99255f, 0.99452f, 0.99619f, 0.99756f, 0.99863f, 0.99939f, 0.99985f,
            1.00000f, 0.99985f, 0.99939f, 0.99863f, 0.99756f, 0.99619f, 0.99452f, 0.99255f, 0.99027f, 0.98769f,
            0.98481f, 0.98163f, 0.97815f, 0.97437f, 0.97030f, 0.96593f, 0.96126f, 0.95630f, 0.95106f, 0.94552f,
            0.93969f, 0.93358f, 0.92718f, 0.92050f, 0.91355f, 0.90631f, 0.89879f, 0.89101f, 0.88295f, 0.87462f,
            0.86603f, 0.85717f, 0.84805f, 0.83867f, 0.82904f, 0.81915f, 0.80902f, 0.79864f, 0.78801f, 0.77715f,
            0.76604f, 0.75471f, 0.74314f, 0.73135f, 0.71934f, 0.70711f, 0.69466f, 0.68200f, 0.66913f, 0.65606f,
            0.64279f, 0.62932f, 0.61566f, 0.60182f, 0.58779f, 0.57358f, 0.55919f, 0.54464f, 0.52992f, 0.51504f,
            0.50000f, 0.48481f, 0.46947f, 0.45399f, 0.43837f, 0.42262f, 0.40674f, 0.39073f, 0.37461f, 0.35837f,
            0.34202f, 0.32557f, 0.30902f, 0.29237f, 0.27564f, 0.25882f, 0.24192f, 0.22495f, 0.20791f, 0.19081f,
            0.17365f, 0.15643f, 0.13917f, 0.12187f, 0.10453f, 0.08716f, 0.06976f, 0.05234f, 0.03490f, 0.01745f,
            0.00000f, -0.01745f, -0.03490f, -0.05234f, -0.06976f, -0.08716f, -0.10453f, -0.12187f, -0.13917f, -0.15643f,
            -0.17365f, -0.19081f, -0.20791f, -0.22495f, -0.24192f, -0.25882f, -0.27564f, -0.29237f, -0.30902f,
            -0.32557f,
            -0.34202f, -0.35837f, -0.37461f, -0.39073f, -0.40674f, -0.42262f, -0.43837f, -0.45399f, -0.46947f,
            -0.48481f, -0.50000f, -0.51504f, -0.52992f, -0.54464f, -0.55919f, -0.57358f, -0.58779f, -0.60182f,
            -0.61566f, -0.62932f,
            -0.64279f, -0.65606f, -0.66913f, -0.68200f, -0.69466f, -0.70711f, -0.71934f, -0.73135f, -0.74314f,
            -0.75471f, -0.76604f, -0.77715f, -0.78801f, -0.79864f, -0.80902f, -0.81915f, -0.82904f, -0.83867f,
            -0.84805f, -0.85717f,
            -0.86603f, -0.87462f, -0.88295f, -0.89101f, -0.89879f, -0.90631f, -0.91355f, -0.92050f, -0.92718f,
            -0.93358f, -0.93969f, -0.94552f, -0.95106f, -0.95630f, -0.96126f, -0.96593f, -0.97030f, -0.97437f,
            -0.97815f, -0.98163f,
            -0.98481f, -0.98769f, -0.99027f, -0.99255f, -0.99452f, -0.99619f, -0.99756f, -0.99863f, -0.99939f,
            -0.99985f, -1.00000f, -0.99985f, -0.99939f, -0.99863f, -0.99756f, -0.99619f, -0.99452f, -0.99255f,
            -0.99027f, -0.98769f,
            -0.98481f, -0.98163f, -0.97815f, -0.97437f, -0.97030f, -0.96593f, -0.96126f, -0.95630f, -0.95106f,
            -0.94552f, -0.93969f, -0.93358f, -0.92718f, -0.92050f, -0.91355f, -0.90631f, -0.89879f, -0.89101f,
            -0.88295f, -0.87462f,
            -0.86603f, -0.85717f, -0.84805f, -0.83867f, -0.82904f, -0.81915f, -0.80902f, -0.79864f, -0.78801f,
            -0.77715f, -0.76604f, -0.75471f, -0.74314f, -0.73135f, -0.71934f, -0.70711f, -0.69466f, -0.68200f,
            -0.66913f, -0.65606f,
            -0.64279f, -0.62932f, -0.61566f, -0.60182f, -0.58779f, -0.57358f, -0.55919f, -0.54464f, -0.52992f,
            -0.51504f, -0.50000f, -0.48481f, -0.46947f, -0.45399f, -0.43837f, -0.42262f, -0.40674f, -0.39073f,
            -0.37461f, -0.35837f,
            -0.34202f, -0.32557f, -0.30902f, -0.29237f, -0.27564f, -0.25882f, -0.24192f, -0.22495f, -0.20791f,
            -0.19081f, -0.17365f, -0.15643f, -0.13917f, -0.12187f, -0.10453f, -0.08716f, -0.06976f, -0.05234f,
            -0.03490f, -0.01745f,
        };

        public static float HighPerformanceSin(float angle)
        {
            int index;
            index = (int) angle % 360;
            if (index < 0)
            {
                return -s_sin_values[-index];
            }

            return s_sin_values[index];
        }

        public static float HighPerformanceCos(float angle)
        {
            return HighPerformanceSin(angle + 90);
        }


        // Carmack's Sqrt.
        // C++ Source Code
        //float Q_rsqrt(float number)
        //{
        //    long i;
        //    float x2, y;
        //    const float threehalfs = 1.5F;

        //    x2 = number * 0.5F;
        //    y = number;
        //    i = *(long*)&y;// evil floating point bit level hacking
        //    i = 0x5f3759df - (i >> 1);// what the fuck?
        //    y = *(float*)&i;
        //    y = y * (threehalfs - (x2 * y * y));   // 1st iteration 
        //    //      y  = y * ( threehalfs - ( x2 * y * y ) );   // 2nd iteration, this can be removed
        //    return y * number;
        //}

        // To C#.
        [StructLayout(LayoutKind.Explicit)]
        private struct FloatIntUnion
        {
            [FieldOffset(0)] public float f;

            [FieldOffset(0)] public int i;
        }

        public static float Sqrt(float number)
        {
            if (number == 0) return 0;
            FloatIntUnion u;
            float x2 = 0.5f * number;
            u.i = 0;
            u.f = number;
            u.i = 0x5f375a86 - (u.i >> 1); // - -~~ 照着翻译（无法理解呀）,呵呵，神奇的 【0x5f375a86】
            u.f = u.f * (1.5f - x2 * u.f * u.f);
            return u.f * number;
        }

        // Hue To RGB [0.0f - 1.0f]
        public static Color Hue2RGB(int hue, float alpha = 1.0f)
        {
            int region;
            float modF;
            float raiseValue;
            float fallValue;

            hue = Math.Abs(hue) % 360;
            modF = (hue % 60) / 60.0f;
            region = hue / 60;

            raiseValue = modF;
            fallValue = 1.0f - modF;

            float r, g, b;
            r = 0;
            g = 0;
            b = 0;
            switch (region)
            {
                case 0:
                    r = 1;
                    g = raiseValue;
                    b = 0;
                    break;
                case 1:
                    r = fallValue;
                    g = 1;
                    b = 0;
                    break;
                case 2:
                    r = 0;
                    g = 1;
                    b = raiseValue;
                    break;
                case 3:
                    r = 0;
                    g = fallValue;
                    b = 1;
                    break;
                case 4:
                    r = raiseValue;
                    g = 0;
                    b = 1;
                    break;
                case 5:
                    r = 1;
                    g = 0;
                    b = fallValue;
                    break;
            }

            return new Color(r, g, b, alpha);
        }

        public static float RGB2Hue(Color color)
        {
            float r, g, b;

            r = color.r;
            g = color.g;
            b = color.b;

            float max = Mathf.Max(r, g, b);
            float min = Mathf.Min(r, g, b);

            float hue;
            hue = 0.0f;
            if (max == r && g >= b)
            {
                if (max - min == 0) hue = 0.0f;
                else hue = 60 * (g - b) / (max - min);
            }
            else if (max == r && g < b)
            {
                hue = 60 * (g - b) / (max - min) + 360;
            }
            else if (max == g)
            {
                hue = 60 * (b - r) / (max - min) + 120;
            }
            else if (max == b)
            {
                hue = 60 * (r - g) / (max - min) + 240;
            }

            return hue;
        }

        #endregion

        #region Unity GameObject

        /// <summary>
        /// 根据名字计算layer
        /// </summary>
        public static int CalcLayer(params string[] layers)
        {
            int layer = 0;
            for (int i = 0; i < layers.Length; ++i)
                layer |= 1 << LayerMask.NameToLayer(layers[i]);
            return layer;
        }

        public static void SetAllRenderersLayer(GameObject go, string layerName)
        {
            Renderer[] renderers;
            renderers = go.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i)
                renderers[i].gameObject.layer = LayerMask.NameToLayer(layerName);
        }

        public static void SetShader(GameObject go, Shader shader)
        {
            if (go.GetComponent<Renderer>() && go.GetComponent<Renderer>().material)
                go.GetComponent<Renderer>().material.shader = shader;
            for (int i = 0; i < go.transform.childCount; ++i)
                SetShader(go.transform.GetChild(i).gameObject, shader);
        }

        public static MeshCollider CombineMeshCollidersToTarget(GameObject target, bool destroyRigidbody = true)
        {
            MeshCollider[] meshColliders = target.GetComponentsInChildren<MeshCollider>();
            CombineInstance[] combine = new CombineInstance[meshColliders.Length];
            int i = 0;
            while (i < meshColliders.Length)
            {
                combine[i].mesh = meshColliders[i].sharedMesh;
                combine[i].transform =
                    target.transform.worldToLocalMatrix * meshColliders[i].transform.localToWorldMatrix;
                GameObject.Destroy(meshColliders[i]);
                if (meshColliders[i].GetComponent<Rigidbody>() && destroyRigidbody)
                    GameObject.Destroy(meshColliders[i].GetComponent<Rigidbody>());
                i++;
            }

            MeshCollider mc;
            mc = target.AddComponent<MeshCollider>();
            mc.sharedMesh = new Mesh();
            mc.sharedMesh.CombineMeshes(combine);
            return mc;
        }

        #endregion


        #region Debug

#if UNITY_EDITOR

        public static void DebugDrawBounds(Bounds bounds)
        {
            DebugDrawBounds(bounds, Color.white);
        }

        public static void DebugDrawBounds(Bounds bounds, Color color)
        {
            Vector3 e = bounds.extents;

            // top surface
            Vector3 p1 = bounds.center + new Vector3(-e.x, e.y, e.z); // left top
            Vector3 p2 = bounds.center + new Vector3(-e.x, e.y, -e.z); // left bottom
            Vector3 p3 = bounds.center + new Vector3(e.x, e.y, e.z);
            Vector3 p4 = bounds.center + new Vector3(e.x, e.y, -e.z);

            // bottom surface
            Vector3 p5 = bounds.center + new Vector3(-e.x, -e.y, e.z); // left top
            Vector3 p6 = bounds.center + new Vector3(-e.x, -e.y, -e.z); // left bottom
            Vector3 p7 = bounds.center + new Vector3(e.x, -e.y, e.z);
            Vector3 p8 = bounds.center + new Vector3(e.x, -e.y, -e.z);

            // draw
            Debug.DrawLine(p1, p2, color);
            Debug.DrawLine(p2, p4, color);
            Debug.DrawLine(p4, p3, color);
            Debug.DrawLine(p3, p1, color);

            Debug.DrawLine(p5, p6, color);
            Debug.DrawLine(p6, p8, color);
            Debug.DrawLine(p8, p7, color);
            Debug.DrawLine(p7, p5, color);

            Debug.DrawLine(p1, p5, color);
            Debug.DrawLine(p2, p6, color);
            Debug.DrawLine(p3, p7, color);
            Debug.DrawLine(p4, p8, color);
        }
#endif

        #endregion
    }
}