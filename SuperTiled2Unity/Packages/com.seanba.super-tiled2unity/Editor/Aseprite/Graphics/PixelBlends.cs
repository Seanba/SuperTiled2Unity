using UnityEngine;
using Unity.Burst;

//namespace UnityEditor.U2D.Aseprite // Note: Code originally found in Unity's Aseprite Importer
namespace SuperTiled2Unity.Ase.Editor
{
    [BurstCompile]
    internal static class PixelBlends
    {
        [BurstCompile]
        public static void Normal(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            var inAlpha = inColor.a / 255f;

            outColor = new Color32
            {
                a = (byte)(inColor.a + prevOutColor.a * (1f - inAlpha))
            };

            var prevAlpha = (prevOutColor.a * (1f - inAlpha)) / 255f;
            var premultiplyAlpha = outColor.a > 0 ? 255f / outColor.a : 1f;
            outColor.r = (byte)((inColor.r * inAlpha + prevOutColor.r * prevAlpha) * premultiplyAlpha);
            outColor.g = (byte)((inColor.g * inAlpha + prevOutColor.g * prevAlpha) * premultiplyAlpha);
            outColor.b = (byte)((inColor.b * inAlpha + prevOutColor.b * prevAlpha) * premultiplyAlpha);
        }

        [BurstCompile]
        public static void Darken(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var darken = new Color32
            {
                r = inColor.r < prevOutColor.r ? inColor.r : prevOutColor.r,
                g = inColor.g < prevOutColor.g ? inColor.g : prevOutColor.g,
                b = inColor.b < prevOutColor.b ? inColor.b : prevOutColor.b,
                a = inColor.a
            };

            Normal(in prevOutColor, in darken, out outColor);
        }

        [BurstCompile]
        public static void Multiply(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var multiply = new Color32
            {
                r = MultiplyUnsignedByte(inColor.r, prevOutColor.r),
                g = MultiplyUnsignedByte(inColor.g, prevOutColor.g),
                b = MultiplyUnsignedByte(inColor.b, prevOutColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in multiply, out outColor);
        }

        [BurstCompile]
        public static void ColorBurn(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var burn = new Color32
            {
                r = BurnChannel(prevOutColor.r, inColor.r),
                g = BurnChannel(prevOutColor.g, inColor.g),
                b = BurnChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in burn, out outColor);
        }

        [BurstCompile]
        static byte BurnChannel(byte prevVal, byte inVal)
        {
            if (prevVal == 255)
                return prevVal;
            prevVal = (byte)(255 - prevVal);
            if (prevVal >= inVal)
                return 0;

            var div = DivideUnsignedByte(prevVal, inVal);
            return (byte)(255 - div);
        }

        [BurstCompile]
        public static void Lighten(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var lighten = new Color32
            {
                r = inColor.r > prevOutColor.r ? inColor.r : prevOutColor.r,
                g = inColor.g > prevOutColor.g ? inColor.g : prevOutColor.g,
                b = inColor.b > prevOutColor.b ? inColor.b : prevOutColor.b,
                a = inColor.a
            };

            Normal(in prevOutColor, in lighten, out outColor);
        }

        [BurstCompile]
        public static void Screen(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var multiply = new Color32
            {
                r = MultiplyUnsignedByte(inColor.r, prevOutColor.r),
                g = MultiplyUnsignedByte(inColor.g, prevOutColor.g),
                b = MultiplyUnsignedByte(inColor.b, prevOutColor.b)
            };

            var screen = new Color32
            {
                r = (byte)((inColor.r + prevOutColor.r) - multiply.r),
                g = (byte)((inColor.g + prevOutColor.g) - multiply.g),
                b = (byte)((inColor.b + prevOutColor.b) - multiply.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in screen, out outColor);
        }

        [BurstCompile]
        public static void ColorDodge(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var burn = new Color32
            {
                r = DodgeChannel(prevOutColor.r, inColor.r),
                g = DodgeChannel(prevOutColor.g, inColor.g),
                b = DodgeChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in burn, out outColor);
        }

        [BurstCompile]
        static byte DodgeChannel(byte prevVal, byte inVal)
        {
            if (prevVal == 0)
                return 0;
            inVal = (byte)(255 - inVal);
            if (prevVal >= inVal)
                return 255;

            var div = DivideUnsignedByte(prevVal, inVal);
            return div;
        }

        [BurstCompile]
        public static void Addition(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var addition = new Color32
            {
                r = (byte)Mathf.Min(inColor.r + prevOutColor.r, 255),
                g = (byte)Mathf.Min(inColor.g + prevOutColor.g, 255),
                b = (byte)Mathf.Min(inColor.b + prevOutColor.b, 255),
                a = inColor.a
            };

            Normal(in prevOutColor, in addition, out outColor);
        }

        [BurstCompile]
        public static void Overlay(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var overlay = new Color32
            {
                r = HardLightChannel(inColor.r, prevOutColor.r),
                g = HardLightChannel(inColor.g, prevOutColor.g),
                b = HardLightChannel(inColor.b, prevOutColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in overlay, out outColor);
        }

        [BurstCompile]
        static byte HardLightChannel(byte valA, byte valB)
        {
            if (valB < 128)
                return MultiplyUnsignedByte(valA, (byte)(valB << 1));
            return ScreenUnsignedByte(valA, (byte)((valB << 1) - 255));
        }

        [BurstCompile]
        public static void SoftLight(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var overlay = new Color32
            {
                r = SoftLightChannel(prevOutColor.r, inColor.r),
                g = SoftLightChannel(prevOutColor.g, inColor.g),
                b = SoftLightChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in overlay, out outColor);
        }

        [BurstCompile]
        static byte SoftLightChannel(byte inA, byte inB)
        {
            var valA = inA / 255f;
            var valB = inB / 255f;
            float valC, final;

            if (valA <= 0.25f)
                valC = ((16 * valA - 12) * valA + 4) * valA;
            else
                valC = Mathf.Sqrt(valA);

            if (valB <= 0.5f)
                final = valA - (1f - 2f * valB) * valA * (1f - valA);
            else
                final = valA + (2f * valB - 1f) * (valC - valA);

            return (byte)(final * 255f + 0.5f);
        }

        [BurstCompile]
        public static void HardLight(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var overlay = new Color32
            {
                r = HardLightChannel(prevOutColor.r, inColor.r),
                g = HardLightChannel(prevOutColor.g, inColor.g),
                b = HardLightChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in overlay, out outColor);
        }

        [BurstCompile]
        public static void Difference(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var difference = new Color32
            {
                r = DifferenceChannel(prevOutColor.r, inColor.r),
                g = DifferenceChannel(prevOutColor.g, inColor.g),
                b = DifferenceChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in difference, out outColor);
        }

        [BurstCompile]
        static byte DifferenceChannel(byte valA, byte valB)
        {
            return (byte)Mathf.Abs(valA - valB);
        }

        [BurstCompile]
        public static void Exclusion(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var exclusion = new Color32
            {
                r = ExclusionChannel(prevOutColor.r, inColor.r),
                g = ExclusionChannel(prevOutColor.g, inColor.g),
                b = ExclusionChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in exclusion, out outColor);
        }

        [BurstCompile]
        static byte ExclusionChannel(byte valA, byte valB)
        {
            var valC = MultiplyUnsignedByte(valA, valB);
            return (byte)(valA + valB - 2 * valC);
        }

        [BurstCompile]
        public static void Subtract(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var subtract = new Color32
            {
                r = (byte)Mathf.Max(prevOutColor.r - inColor.r, 0),
                g = (byte)Mathf.Max(prevOutColor.g - inColor.g, 0),
                b = (byte)Mathf.Max(prevOutColor.b - inColor.b, 0),
                a = inColor.a
            };

            Normal(in prevOutColor, in subtract, out outColor);
        }

        [BurstCompile]
        public static void Divide(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var divide = new Color32
            {
                r = DivideChannel(prevOutColor.r, inColor.r),
                g = DivideChannel(prevOutColor.g, inColor.g),
                b = DivideChannel(prevOutColor.b, inColor.b),
                a = inColor.a
            };

            Normal(in prevOutColor, in divide, out outColor);
        }

        [BurstCompile]
        static byte DivideChannel(byte valA, byte valB)
        {
            if (valA == 0)
                return 0;
            else if (valA >= valB)
                return 255;
            else
                return DivideUnsignedByte(valA, valB);
        }

        [BurstCompile]
        public static void Hue(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var r = prevOutColor.r / 255f;
            var g = prevOutColor.g / 255f;
            var b = prevOutColor.b / 255f;
            var s = GetSaturation(r, g, b);
            var l = GetLuminosity(r, g, b);

            r = inColor.r / 255f;
            g = inColor.g / 255f;
            b = inColor.b / 255f;

            SetSaturation(ref r, ref g, ref b, s);
            SetLuminosity(ref r, ref g, ref b, l);

            var hue = new Color32()
            {
                r = (byte)Mathf.RoundToInt(r * 255f),
                g = (byte)Mathf.RoundToInt(g * 255f),
                b = (byte)Mathf.RoundToInt(b * 255f),
                a = inColor.a
            };

            Normal(in prevOutColor, in hue, out outColor);
        }

        [BurstCompile]
        public static void Saturation(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var r = inColor.r / 255f;
            var g = inColor.g / 255f;
            var b = inColor.b / 255f;
            var s = GetSaturation(r, g, b);

            r = prevOutColor.r / 255f;
            g = prevOutColor.g / 255f;
            b = prevOutColor.b / 255f;
            var l = GetLuminosity(r, g, b);

            SetSaturation(ref r, ref g, ref b, s);
            SetLuminosity(ref r, ref g, ref b, l);

            var saturation = new Color32()
            {
                r = (byte)Mathf.RoundToInt(r * 255f),
                g = (byte)Mathf.RoundToInt(g * 255f),
                b = (byte)Mathf.RoundToInt(b * 255f),
                a = inColor.a
            };

            Normal(in prevOutColor, in saturation, out outColor);
        }

        [BurstCompile]
        public static void ColorBlend(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var r = prevOutColor.r / 255f;
            var g = prevOutColor.g / 255f;
            var b = prevOutColor.b / 255f;
            var l = GetLuminosity(r, g, b);

            r = inColor.r / 255f;
            g = inColor.g / 255f;
            b = inColor.b / 255f;

            SetLuminosity(ref r, ref g, ref b, l);

            var color = new Color32()
            {
                r = (byte)Mathf.RoundToInt(r * 255f),
                g = (byte)Mathf.RoundToInt(g * 255f),
                b = (byte)Mathf.RoundToInt(b * 255f),
                a = inColor.a
            };

            Normal(in prevOutColor, in color, out outColor);
        }

        [BurstCompile]
        public static void Luminosity(in Color32 prevOutColor, in Color32 inColor, out Color32 outColor)
        {
            outColor = new Color32();

            if (prevOutColor == Color.clear)
            {
                outColor = inColor;
                return;
            }

            var r = inColor.r / 255f;
            var g = inColor.g / 255f;
            var b = inColor.b / 255f;
            var l = GetLuminosity(r, g, b);

            r = prevOutColor.r / 255f;
            g = prevOutColor.g / 255f;
            b = prevOutColor.b / 255f;

            SetLuminosity(ref r, ref g, ref b, l);

            var luminosity = new Color32()
            {
                r = (byte)Mathf.RoundToInt(r * 255f),
                g = (byte)Mathf.RoundToInt(g * 255f),
                b = (byte)Mathf.RoundToInt(b * 255f),
                a = inColor.a
            };

            Normal(in prevOutColor, in luminosity, out outColor);
        }

        [BurstCompile]
        static byte MultiplyUnsignedByte(byte valA, byte valB)
        {
            const uint oneHalf = 0x80;
            const int gShift = 8;
            var valC = (int)((valA * valB) + oneHalf);
            return (byte)(((valC >> gShift) + valC) >> gShift);
        }

        [BurstCompile]
        static byte ScreenUnsignedByte(byte valA, byte valB)
        {
            return (byte)(valB + valA - MultiplyUnsignedByte(valB, valA));
        }

        [BurstCompile]
        static byte DivideUnsignedByte(byte valA, byte valB)
        {
            return (byte)((valA * 0xFF) / valB);
        }

        [BurstCompile]
        static float GetSaturation(float r, float g, float b)
        {
            return Mathf.Max(r, Mathf.Max(g, b)) - Mathf.Min(r, Mathf.Min(g, b));
        }

        [BurstCompile]
        static void SetSaturation(ref float r, ref float g, ref float b, float s)
        {
            ref var min = ref MinRef(ref r, ref MinRef(ref g, ref b));
            ref var mid = ref MidRef(ref r, ref g, ref b);
            ref var max = ref MaxRef(ref r, ref MaxRef(ref g, ref b));

            if (max > min)
            {
                mid = ((mid - min) * s) / (max - min);
                max = s;
            }
            else
                mid = max = 0;

            min = 0f;
        }

        [BurstCompile]
        static ref float MaxRef(ref float a, ref float b)
        {
            if (a > b)
                return ref a;
            return ref b;
        }

        [BurstCompile]
        static ref float MinRef(ref float a, ref float b)
        {
            if (a < b)
                return ref a;
            return ref b;
        }

        static ref float MidRef(ref float a, ref float b, ref float c)
        {
            if (a > b)
            {
                if (b > c)
                {
                    return ref b;
                }
                if (a > c)
                    return ref c;
                return ref a;
            }
            if (!(b > c))
                return ref b;
            if (c > a)
                return ref c;
            return ref a;
        }

        [BurstCompile]
        static float GetLuminosity(float r, float g, float b)
        {
            return (0.3f * r) + (0.59f * g) + (0.11f * b);
        }

        [BurstCompile]
        static void SetLuminosity(ref float r, ref float g, ref float b, float l)
        {
            var d = l - GetLuminosity(r, g, b);
            r += d;
            g += d;
            b += d;
            ClipColor(ref r, ref g, ref b);
        }

        [BurstCompile]
        static void ClipColor(ref float r, ref float g, ref float b)
        {
            var l = GetLuminosity(r, g, b);
            var n = Mathf.Min(r, Mathf.Min(g, b));
            var x = Mathf.Max(r, Mathf.Max(g, b));

            if (n < 0)
            {
                r = l + (((r - l) * l) / (l - n));
                g = l + (((g - l) * l) / (l - n));
                b = l + (((b - l) * l) / (l - n));
            }

            if (x > 1)
            {
                r = l + (((r - l) * (1 - l)) / (x - l));
                g = l + (((g - l) * (1 - l)) / (x - l));
                b = l + (((b - l) * (1 - l)) / (x - l));
            }
        }
    }
}