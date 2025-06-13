using System.Collections.Generic;
using UnityEngine;

namespace SuperTiled2Unity.Ase.Editor
{
    internal static class AseGraphics
    {
        public class GetPixelArgs
        {
            public ColorDepth ColorDepth { get; set; }
            public List<Color32> Palette { get; } = new List<Color32>();
            public byte[] PixelBytes { get; set; }
            public int Stride { get; set; }
        }

        public static Color32 GetPixel(int x, int y, GetPixelArgs args)
        {
            if (args.ColorDepth == ColorDepth.Indexed8)
            {
                var index = x + (y * args.Stride);
                int paletteIndex = args.PixelBytes[index];
                var color = args.Palette[paletteIndex];
                return color;
            }
            else if (args.ColorDepth == ColorDepth.Grayscale16)
            {
                var index = 2 * (x + (y * args.Stride));
                var value = args.PixelBytes[index];
                var alpha = args.PixelBytes[index + 1];
                return new Color32(value, value, value, alpha);
            }
            else if (args.ColorDepth == ColorDepth.RGBA32)
            {
                var index = 4 * (x + (y * args.Stride));
                var red = args.PixelBytes[index];
                var green = args.PixelBytes[index + 1];
                var blue = args.PixelBytes[index + 2];
                var alpha = args.PixelBytes[index + 3];
                return new Color32(red, green, blue, alpha);
            }

            // Unsupported color depth
            return Color.magenta;
        }

        public static byte CalculateOpacity(params byte[] opacities)
        {
            float opacity = 1.0f;
            foreach (var opByte in opacities)
            {
                opacity *= (float)(opByte / 255.0f);
            }

            return (byte)(opacity * 255);
        }

        public static Color32 BlendColors(BlendMode blend, Color32 prevColor, Color32 thisColor)
        {
            Color32 outColor;
            switch (blend)
            {
                case BlendMode.Darken:
                    PixelBlends.Darken(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Multiply:
                    PixelBlends.Multiply(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.ColorBurn:
                    PixelBlends.ColorBurn(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Lighten:
                    PixelBlends.Lighten(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Screen:
                    PixelBlends.Screen(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.ColorDodge:
                    PixelBlends.ColorDodge(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Addition:
                    PixelBlends.Addition(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Overlay:
                    PixelBlends.Overlay(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.SoftLight:
                    PixelBlends.SoftLight(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.HardLight:
                    PixelBlends.HardLight(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Difference:
                    PixelBlends.Difference(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Exclusion:
                    PixelBlends.Exclusion(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Subtract:
                    PixelBlends.Subtract(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Divide:
                    PixelBlends.Divide(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Hue:
                    PixelBlends.Hue(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Saturation:
                    PixelBlends.Saturation(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Color:
                    PixelBlends.ColorBlend(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Luminosity:
                    PixelBlends.Luminosity(in prevColor, in thisColor, out outColor);
                    break;
                case BlendMode.Normal:
                default:
                    PixelBlends.Normal(in prevColor, in thisColor, out outColor);
                    break;
            }

            return outColor;
        }
    }
}
