
using StbImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace HDRThumbnail;
struct vec3
{
    public void mulAssign(double d)
    {
        this.X = this.X * d;
        this.Y = this.Y * d;
        this.Z = this.Z * d;
    }
    public vec3(double x, double y, double z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public static vec3 operator +(vec3 v3, double d)
    {
        return new vec3
        {
            X = v3.X + d,
            Y = v3.Y + d,
            Z = v3.Z + d,
        };
    }
    public static vec3 operator -(vec3 v3, double d)
    {
        return new vec3
        {
            X = v3.X - d,
            Y = v3.Y - d,
            Z = v3.Z - d,
        };
    }
    public static vec3 operator *(double d, vec3 v3)
    {
        return new vec3
        {
            X = v3.X * d,
            Y = v3.Y * d,
            Z = v3.Z * d,
        };
    }
    public static vec3 operator *(vec3 x, vec3 y)
    {
        return new vec3
        {
            X = x.X * y.X,
            Y = x.Y * y.Y,
            Z = x.Z * y.Z,
        };
    }
    public static vec3 operator /(vec3 x, vec3 y)
    {
        return new vec3
        {
            X = x.X / y.X,
            Y = x.Y / y.Y,
            Z = x.Z / y.Z,
        };
    }
}
struct mat3
{
    public mat3(vec3 c1, vec3 c2, vec3 c3)
    {
        this.C1 = c1; this.C2 = c2; this.C3 = c3;
    }
    vec3 C1 { get; set; }
    vec3 C2 { get; set; }
    vec3 C3 { get; set; }
    public static vec3 operator *(mat3 m, vec3 v)
    {
        return new vec3
        {
            X = v.X * m.C1.X + v.Y * m.C2.X + v.Z * m.C3.X,
            Y = v.X * m.C1.Y + v.Y * m.C2.Y + v.Z * m.C3.Y,
            Z = v.X * m.C1.Z + v.Y * m.C2.Z + v.Z * m.C3.Z,
        };
    }
}
public class InvalidHDRMetadataException : Exception
{

}

public class HDRThumbnail
{
    static double saturate(double d)
    {
        return Math.Min(1, Math.Max(0, d));
    }
    static vec3 saturate(vec3 v)
    {
        return new vec3(
            saturate(v.X), saturate(v.Y), saturate(v.Z)
        );
    }
    readonly static double toneMappingExposure = 1.0;
    static vec3 RRTAndODTFit(vec3 v)
    {
        vec3 a = v * (v + 0.0245786) - 0.000090537;
        vec3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
        return a / b;
    }
    static vec3 ACESFilmicToneMapping(vec3 color)
    {

        // sRGB => XYZ => D65_2_D60 => AP1 => RRT_SAT
        mat3 ACESInputMat = new mat3(
           new vec3(0.59719, 0.07600, 0.02840), // transposed from source
           new vec3(0.35458, 0.90834, 0.13383),
           new vec3(0.04823, 0.01566, 0.83777)
       );

        // ODT_SAT => XYZ => D60_2_D65 => sRGB
        mat3 ACESOutputMat = new mat3(
           new vec3(1.60475, -0.10208, -0.00327), // transposed from source
           new vec3(-0.53108, 1.10813, -0.07276),
           new vec3(-0.07367, -0.00605, 1.07602)
       );

        color.mulAssign(toneMappingExposure / 0.6);

        color = ACESInputMat * color;

        // Apply RRT and ODT
        color = RRTAndODTFit(color);

        color = ACESOutputMat * color;

        // Clamp to [0, 1]
        return saturate(color);
    }
    static vec3 gammaCorrection(vec3 v)
    {
        return new vec3(Math.Pow(v.X, 1.0 / 2.2), Math.Pow(v.Y, 1.0 / 2.2), Math.Pow(v.Z, 1.0 / 2.2));
    }
    static byte lin(double x)
    {
        return (byte)Math.Max(0, Math.Min(255, (x * 255.0)));
    }
    public static void thumbnail(int _width, int _height, string inputFilePath, string outputFilePath)
    {
        using (var stream = File.OpenRead(inputFilePath))
        {
            var image = ImageResultFloat.FromStream(stream);
            var meta = ImageInfo.FromStream(stream);
            if (meta == null) throw new InvalidHDRMetadataException();
            var height = meta.Value.Height * 8 / 10;
            var width = meta.Value.Height * 8 * _width / _height / 10;
            int offsetX = (meta.Value.Width - width) / 2;
            int offsetY = (meta.Value.Height - height) / 2;
            using (Image<Rgb24> output = new(width, height))
            {
                for (int wi = 0; wi < width; wi++)
                {
                    for (int hi = 0; hi < height; hi++)
                    {
                        var pi = (hi + offsetY) * meta.Value.Width + (wi + offsetX);

                        var r = image.Data[pi * 3];
                        var g = image.Data[pi * 3 + 1];
                        var b = image.Data[pi * 3 + 2];
                        var rgb = gammaCorrection(
                            // ACESFilmicToneMapping
                            (new vec3(r, g, b)));

                        output[wi, hi] = new Rgb24(lin(rgb.X), lin(rgb.Y), lin(rgb.Z));
                    }
                }

                output.Mutate(x=>x.Resize(_width, _height));
                output.SaveAsJpeg(outputFilePath);
            }

        }


    }
}
