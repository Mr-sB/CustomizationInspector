using System;

namespace UnityEngine.Extension
{
    /// <summary>
    ///   <para>Representation of 4D vectors and points using integers.</para>
    /// </summary>
    [Serializable]
    public struct Vector4Int : IEquatable<Vector4Int>
    {
        private static readonly Vector4Int s_Zero = new Vector4Int(0, 0, 0, 0);
        private static readonly Vector4Int s_One = new Vector4Int(1, 1, 1, 1);
        [SerializeField] private int m_X;
        [SerializeField] private int m_Y;
        [SerializeField] private int m_Z;
        [SerializeField] private int m_W;

        public Vector4Int(int x) : this(x, 0, 0, 0)
        {
        }

        public Vector4Int(int x, int y) : this(x, y, 0, 0)
        {
        }

        public Vector4Int(int x, int y, int z) : this(x, y, z, 0)
        {
        }

        public Vector4Int(int x, int y, int z, int w)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
            m_W = w;
        }

        /// <summary>
        ///   <para>X component of the vector.</para>
        /// </summary>
        public int x
        {
            get { return m_X; }
            set { m_X = value; }
        }

        /// <summary>
        ///   <para>Y component of the vector.</para>
        /// </summary>
        public int y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        /// <summary>
        ///   <para>Z component of the vector.</para>
        /// </summary>
        public int z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }

        /// <summary>
        ///   <para>W component of the vector.</para>
        /// </summary>
        public int w
        {
            get { return m_W; }
            set { m_W = value; }
        }

        /// <summary>
        ///   <para>Set x, y, z and w components of an existing Vector4Int.</para>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="w"></param>
        public void Set(int x, int y, int z, int w)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
            m_W = w;
        }

        public int this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    case 3:
                        return w;
                    default:
                        throw new IndexOutOfRangeException(
                            string.Format("Invalid Vector4Int index addressed: {0}!", index));
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        x = value;
                        break;
                    case 1:
                        y = value;
                        break;
                    case 2:
                        z = value;
                        break;
                    case 3:
                        w = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException(
                            string.Format("Invalid Vector4Int index addressed: {0}!", index));
                }
            }
        }

        /// <summary>
        ///   <para>Returns the length of this vector (Read Only).</para>
        /// </summary>
        public float magnitude
        {
            get { return Mathf.Sqrt(sqrMagnitude); }
        }

        /// <summary>
        ///   <para>Returns the squared length of this vector (Read Only).</para>
        /// </summary>
        public int sqrMagnitude
        {
            get { return Dot(this, this); }
        }

        public static int Dot(Vector4Int a, Vector4Int b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
        }

        /// <summary>
        ///   <para>Returns the distance between a and b.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static float Distance(Vector4Int a, Vector4Int b)
        {
            return (a - b).magnitude;
        }

        /// <summary>
        ///   <para>Returns a vector that is made from the smallest components of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static Vector4Int Min(Vector4Int lhs, Vector4Int rhs)
        {
            return new Vector4Int(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z),
                Mathf.Min(lhs.w, rhs.w));
        }

        /// <summary>
        ///   <para>Returns a vector that is made from the largest components of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        public static Vector4Int Max(Vector4Int lhs, Vector4Int rhs)
        {
            return new Vector4Int(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z),
                Mathf.Max(lhs.w, rhs.w));
        }

        /// <summary>
        ///   <para>Multiplies two vectors component-wise.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static Vector4Int Scale(Vector4Int a, Vector4Int b)
        {
            return new Vector4Int(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        /// <summary>
        ///   <para>Multiplies every component of this vector by the same component of scale.</para>
        /// </summary>
        /// <param name="scale"></param>
        public void Scale(Vector4Int scale)
        {
            x *= scale.x;
            y *= scale.y;
            z *= scale.z;
            w *= scale.w;
        }

        /// <summary>
        ///   <para>Clamps the Vector4Int to the bounds given by min and max.</para>
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void Clamp(Vector4Int min, Vector4Int max)
        {
            x = Math.Max(min.x, x);
            x = Math.Min(max.x, x);
            y = Math.Max(min.y, y);
            y = Math.Min(max.y, y);
            z = Math.Max(min.z, z);
            z = Math.Min(max.z, z);
            w = Math.Max(min.w, w);
            w = Math.Min(max.w, w);
        }

        public static implicit operator Vector4(Vector4Int v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }

        public static explicit operator Vector3Int(Vector4Int v)
        {
            return new Vector3Int(v.x, v.y, v.z);
        }

        /// <summary>
        ///   <para>Converts a Vector4 to a Vector4Int by doing a Floor to each value.</para>
        /// </summary>
        /// <param name="v"></param>
        public static Vector4Int FloorToInt(Vector4 v)
        {
            return new Vector4Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z),
                Mathf.FloorToInt(v.w));
        }

        /// <summary>
        ///   <para>Converts a Vector4 to a Vector4Int by doing a Ceiling to each value.</para>
        /// </summary>
        /// <param name="v"></param>
        public static Vector4Int CeilToInt(Vector4 v)
        {
            return new Vector4Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z),
                Mathf.CeilToInt(v.w));
        }

        /// <summary>
        ///   <para>Converts a Vector4 to a Vector4Int by doing a Round to each value.</para>
        /// </summary>
        /// <param name="v"></param>
        public static Vector4Int RoundToInt(Vector4 v)
        {
            return new Vector4Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z),
                Mathf.RoundToInt(v.w));
        }

        public static Vector4Int operator +(Vector4Int a, Vector4Int b)
        {
            return new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }

        public static Vector4Int operator -(Vector4Int a, Vector4Int b)
        {
            return new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }

        public static Vector4Int operator *(Vector4Int a, Vector4Int b)
        {
            return new Vector4Int(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        }

        public static Vector4Int operator -(Vector4Int a)
        {
            return new Vector4Int(-a.x, -a.y, -a.z, -a.w);
        }

        public static Vector4Int operator *(Vector4Int a, int b)
        {
            return new Vector4Int(a.x * b, a.y * b, a.z * b, a.w * b);
        }

        public static Vector4Int operator *(int a, Vector4Int b)
        {
            return new Vector4Int(a * b.x, a * b.y, a * b.z, a * b.w);
        }

        public static Vector4Int operator /(Vector4Int a, int b)
        {
            return new Vector4Int(a.x / b, a.y / b, a.z / b, a.w / b);
        }

        public static bool operator ==(Vector4Int lhs, Vector4Int rhs)
        {
            return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z && lhs.w == rhs.w;
        }

        public static bool operator !=(Vector4Int lhs, Vector4Int rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        ///   <para>Returns true if the objects are equal.</para>
        /// </summary>
        /// <param name="other"></param>
        public override bool Equals(object other)
        {
            return other is Vector4Int other1 && Equals(other1);
        }

        public bool Equals(Vector4Int other)
        {
            return this == other;
        }

        /// <summary>
        ///   <para>Gets the hash code for the Vector4Int.</para>
        /// </summary>
        /// <returns>
        ///   <para>The hash code of the Vector4Int.</para>
        /// </returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2 ^ w.GetHashCode() >> 1;
        }

        /// <summary>
        ///   <para>Returns a nicely formatted string for this vector.</para>
        /// </summary>
        /// <param name="format"></param>
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", x, y, z, w);
        }

        /// <summary>
        ///   <para>Returns a nicely formatted string for this vector.</para>
        /// </summary>
        /// <param name="format"></param>
        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2}, {3})", x.ToString(format), y.ToString(format), z.ToString(format),
                w.ToString(format));
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector4Int (0, 0, 0, 0).</para>
        /// </summary>
        public static Vector4Int zero
        {
            get { return s_Zero; }
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector4Int (1, 1, 1, 1).</para>
        /// </summary>
        public static Vector4Int one
        {
            get { return s_One; }
        }
    }
}