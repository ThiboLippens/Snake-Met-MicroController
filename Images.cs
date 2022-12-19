using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Project_6_12_2022
{
    public static class Images
    {
        public readonly static ImageSource Empty = LoadImage("Empty.png");
        public readonly static ImageSource Body = LoadImage("Body.png");
        public readonly static ImageSource Head = LoadImage("Head.png");
        public readonly static ImageSource Food = LoadImage("Food.png");
        public readonly static ImageSource DeadBody = LoadImage("DeadBody.png");
        public readonly static ImageSource DeadHead = LoadImage("DeadHead.png");

        private static ImageSource LoadImage(string fileName)
        {
            return new BitmapImage(new Uri($"Afbeeldingen/{fileName}", UriKind.Relative));
        }

    }
}
