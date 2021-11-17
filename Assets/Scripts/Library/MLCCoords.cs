using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MLC
{
    public class Vec : IEquatable<Vec>
    {
        // Private Members
        private float[] array = new float[] { 0.0f, 0.0f, 0.0f };
        const float EqualityThreshold = 0.000001f;

        // Constructors
        public Vec(float x, float y, float z)
        {
            array[0] = x;
            array[1] = y;
            array[2] = z;
        }

        // Public Members
        public float x
        {
            get { return array[0]; }
            set { array[0] = value; }
        }
        public float y
        {
            get { return array[1]; }
            set { array[1] = value; }
        }
        public float z
        {
            get { return array[2]; }
            set { array[2] = value; }
        }

        // Conversions to and from unity vector
        public static Vec UnityToMLC (Vector3 UnityVector) => new Vec(UnityVector.x, UnityVector.z, UnityVector.y);

        public static Vector3 MLCToUnity (Vec MLCVector) => new Vector3(MLCVector.x, MLCVector.z, MLCVector.y);

        // Comparisons

        public override bool Equals(object obj)
        {
            return Equals(obj as Vec);
        }
        public bool Equals(Vec b)
        {
            if (ReferenceEquals(this, b)) // same object
            {
                return true;
            }
            else if (b is null)
            {
                return false;
            }
            else
            {
                if((this - b).sqrMag < EqualityThreshold) // values are very very close
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
        }

        // Operators
        public static Vec operator -(Vec a) => new Vec(-a.x, -a.y, -a.z);
        public static Vec operator +(Vec a, Vec b) => new Vec(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vec operator -(Vec a, Vec b) => new Vec(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vec operator *(float a, Vec b) => new Vec(a * b.x, a * b.y, a * b.z);
        public static Vec operator *(Vec b, float a) => new Vec(a * b.x, a * b.y, a * b.z);
        public static float operator *(Vec a, Vec b) => a.x * b.x + a.y * b.y + a.z * b.z; // Inner Product
        public static Vec operator /(Vec a, float b) => new Vec(a.x / b, a.y / b, a.z / b);
        public static bool operator ==(Vec a, Vec b)
        {
            if(a is null)
            {
                if(b is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return a.Equals(b);
            }
        }

        public static bool operator !=(Vec a, Vec b)
        {
            return !(a == b);
        }

        // Properties
        public float sqrMag
        {
            get { return array[0] * array[0] + array[1] * array[1] + array[2] * array[2]; }
        }

        public float mag
        {
            get { return Mathf.Sqrt(sqrMag); }
        }

        public float sqrXYMag
        {
            get { return array[0] * array[0] + array[1] * array[1]; }
        }

        public float XYmag
        {
            get { return Mathf.Sqrt(sqrXYMag); }
        }

        public Vec normalized // magnitude 1.0f -- not to confuse with the normal TO a vector
        {
            get
            {
                float m = sqrMag;
                if (m < EqualityThreshold)
                {
                    return new Vec(0.0f, 0.0f, 0.0f); // returns zero vector if input is zero vector
                }
                else
                {
                    m = Mathf.Sqrt(m);
                    return new Vec(x / m, y / m, z / m);
                }
            }
        }
        public Vec XYnormalized
        {
            get
            {
                float m = sqrXYMag;
                if (m < EqualityThreshold)
                {
                    return new Vec(0.0f, 0.0f, z);
                }
                else
                {
                    m = Mathf.Sqrt(m);
                    return new Vec(x / m, y / m, z);
                }
            }
        }

    }
}