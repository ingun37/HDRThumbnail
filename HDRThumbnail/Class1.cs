
using StbImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.Runtime.InteropServices;

namespace HDRThumbnail;


public class HDRThumbnail
{
    static void _thumbnail(Stream seekableStream, int width, int height)
    {
        var image = ImageResultFloat.FromStream(seekableStream, ColorComponents.RedGreenBlue);

        using (Image<Rgb24> output = new(width, height))
        {
            for (int wi = 0; wi < width; wi++)
            {
                for (int hi = 0; hi < height; hi++)
                {
                    var pi = hi * width + wi;

                    var r = image.Data[pi * 3];
                    var g = image.Data[pi * 3 + 1];
                    var b = image.Data[pi * 3 + 2];

                    output[wi, hi] = (new Rgb24((byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0)));
                    // pixels[pi * 3] = (byte)(r * 255);
                    // pixels[pi * 3 + 1] = (byte)(g * 255);
                    // pixels[pi * 3 + 2] = (byte)(b * 255);
                }
            }
            // Do your drawing in here...
            output.SaveAsJpeg("out.jpg");

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
