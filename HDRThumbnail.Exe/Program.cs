using HDRThumbnail;
using System.Net;

using (var stream = File.OpenRead("venice_sunset_1k.hdr"))
{
    // See https://aka.ms/new-console-template for more information
    // var url = "http://127.0.0.1:7890/venice_sunset_1k.hdr";
    // using WebClient client = new WebClient();
    HDRThumbnail.HDRThumbnail.thumbnail( 1200, 1000,"venice_sunset_1k.hdr", "out.jpg");

}
