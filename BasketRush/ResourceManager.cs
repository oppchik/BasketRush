using System.Drawing;
using System.Collections.Generic;
using System.IO;

namespace BasketballGame
{
    // Кэш изображений
    public static class ResourceManager
    {
        // Словарь для изображений
        private static Dictionary<string, Image> cache = new Dictionary<string, Image>();

        // Возвращает изображение по пути
        public static Image GetImage(string path)
        {
            if (string.IsNullOrEmpty(path))
                return new Bitmap(1, 1);
            if (!cache.ContainsKey(path))
            {
                if (File.Exists(path))
                    cache[path] = Image.FromFile(path); 
                else
                    return new Bitmap(1, 1);          
            }
            return cache[path];
        }
    }
}