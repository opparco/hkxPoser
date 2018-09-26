using System;
using System.Collections.Generic;
using SharpDX.Direct3D11;

namespace MiniCube
{
    public class TextureResource
    {
        public Texture2D texture2d;
        public ShaderResourceView textureView;

        public TextureResource(Texture2D texture2d, ShaderResourceView textureView)
        {
            this.texture2d = texture2d;
            this.textureView = textureView;
        }

        public void Dispose()
        {
            textureView.Dispose();
            texture2d.Dispose();
        }
    }

    public class TextureCollection
    {
        Device device;
        TextureLoader textureLoader;
        Dictionary<string, TextureResource> resource_map = new Dictionary<string, TextureResource>();

        public TextureCollection(Device device, TextureLoader textureLoader)
        {
            this.device = device;
            this.textureLoader = textureLoader;
        }

        public void LoadTexture(string path)
        {
            if (resource_map.ContainsKey(path))
                return;
            Console.WriteLine("TextureCollection.LoadTexture path:{0}", path);

            var texture2d = textureLoader.CreateTexture2DFromPath(device, path);
            if (texture2d != null)
            {
                var textureView = new ShaderResourceView(device, texture2d);
                resource_map[path] = new TextureResource(texture2d, textureView);
            }
        }

        public Texture2D GetTexture2DByPath(string path)
        {
            TextureResource resource;

            if (resource_map.TryGetValue(path, out resource))
                return resource.texture2d;
            else
                return null;
        }

        public ShaderResourceView GetTextureViewByPath(string path)
        {
            TextureResource resource;

            if (resource_map.TryGetValue(path, out resource))
                return resource.textureView;
            else
                return null;
        }

        public void Dispose()
        {
            foreach (var resource in resource_map.Values)
            {
                resource.Dispose();
            }
        }
    }
}
