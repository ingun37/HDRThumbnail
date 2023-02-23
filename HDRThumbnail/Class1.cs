
using StbImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Runtime.InteropServices;

namespace HDRThumbnail;

public class InvalidHDRMetadataException : Exception
{

}

public class HDRThumbnail
{
    static byte lin(double x)
    {
        return (byte)Math.Max(0, Math.Min(255, (Math.Pow(x, 1.0 / 2.2) * 255.0)));
    }
    static void _thumbnail(Stream seekableStream, int width, int height)
    {
        var image = ImageResultFloat.FromStream(seekableStream);
        var meta = ImageInfo.FromStream(seekableStream);
        if (meta == null) throw new InvalidHDRMetadataException();
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

                    output[wi, hi] = new Rgb24(lin(r), lin(g), lin(b));
                    // pixels[pi * 3] = (byte)(r * 255);
                    // pixels[pi * 3 + 1] = (byte)(g * 255);
                    // pixels[pi * 3 + 2] = (byte)(b * 255);
                }
            }
            // Do your drawing in here...
            output.SaveAsBmp("out.bmp");

        }


    }
    public static void thumbnail(Stream stream, int width, int height)
    {
        if (stream.CanSeek) _thumbnail(stream, width, height);
        else
        {
            using (var copy = new MemoryStream())
            {
                _thumbnail(copy, width, height);
            }
        }
    }
}
