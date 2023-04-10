using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace BrainFailProductions.PolyFew.AsImpL
{
    /// <summary>
    /// Class for loading textures files into Unity scene at run-time and in editor mode.
    /// </summary>
    /// <remarks>
    /// Derived from "Runtime OBJ Loader"
    /// (http://forum.unity3d.com/threads/free-runtime-obj-loader.365884/)
    /// and from "runtime .OBJ file loader for Unity3D"
    /// (https://github.com/hammmm/unity-obj-loader) and 
    /// (https://github.com/cmdr2/unity-remote-obj-loader)
    /// 
    /// </remarks>
    public class TextureLoader : MonoBehaviour
    {
        /// <summary>
        /// Load an image from a file into a Texture2D.
        /// </summary>
        /// <param name="url">URL of the texture image.</param>
        /// <returns>The loaded texture or null on error.</returns>
        public static Texture2D LoadTextureFromUrl(string url)
        {
            const string prefix = "file:///";
            if (url.StartsWith(prefix))
            {
                url = url.Substring(prefix.Length);
            }
            else
            {
                url = Path.GetFullPath(url);
            }
            return LoadTexture(url);
        }


        /// <summary>
        /// Load an image from a file into a Texture2D.
        /// </summary>
        /// <param name="fileName">file name of the image to load</param>
        /// <returns>The loaded texture or null on error.</returns>
        public static Texture2D LoadTexture(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".png" || ext == ".jpg")
            {
                Texture2D t2d = new Texture2D(1, 1);
                t2d.LoadImage(File.ReadAllBytes(fileName));
                return t2d;
            }
            else if (ext == ".dds")
            {
                Texture2D returnTex = LoadDDSManual(fileName);
                return returnTex;
            }
            else if (ext == ".tga")
            {
                Texture2D returnTex = LoadTGA(fileName);
                return returnTex;
            }
            else
            {
                Debug.Log("texture not supported : " + fileName);
            }
            return null;
        }


        /// <summary>
        /// Load a TGA image from a file into a Texture2D.
        /// </summary>
        /// <param name="fileName">file name of the image to load</param>
        /// <returns>The loaded texture or null on error.</returns>
        public static Texture2D LoadTGA(string fileName)
        {
            using (var imageFile = File.OpenRead(fileName))
            {
                return LoadTGA(imageFile);
            }
        }


        /// <summary>
        /// Load a DDS image from a file into a Texture2D.
        /// </summary>
        /// <param name="ddsPath">file name of the image to load</param>
        /// <returns>The loaded texture or null on error.</returns>
        public static Texture2D LoadDDSManual(string ddsPath)
        {
            try
            {

                byte[] ddsBytes = File.ReadAllBytes(ddsPath);

                byte ddsSizeCheck = ddsBytes[4];
                if (ddsSizeCheck != 124)
                    throw new System.Exception("Invalid DDS DXTn texture. Unable to read"); //this header byte should be 124 for DDS image files

                int height = ddsBytes[13] * 256 + ddsBytes[12];
                int width = ddsBytes[17] * 256 + ddsBytes[16];

                byte DXTType = ddsBytes[87];
                TextureFormat textureFormat = TextureFormat.DXT5;
                if (DXTType == 49)
                {
                    textureFormat = TextureFormat.DXT1;
                    //	Debug.Log ("DXT1");
                }

                if (DXTType == 53)
                {
                    textureFormat = TextureFormat.DXT5;
                    //	Debug.Log ("DXT5");
                }
                int DDS_HEADER_SIZE = 128;
                byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
                Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

                System.IO.FileInfo finf = new System.IO.FileInfo(ddsPath);
                Texture2D texture = new Texture2D(width, height, textureFormat, false);
                texture.LoadRawTextureData(dxtBytes);
                texture.Apply();
                texture.name = finf.Name;

                return (texture);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not load DDS: " + ex);
                return new Texture2D(8, 8);
            }
        }


        /// <summary>
        /// Load a TGA image from a stram into a Texture2D.
        /// </summary>
        /// <param name="TGAStream">input stream</param>
        /// <returns>The loaded Texture2D</returns>
        /// <remarks>
        /// This directly comes from the code submitted by aaro4130 and edited by ALLCAPS on the Unity forums (thanks to all!).
        /// https://forum.unity3d.com/threads/tga-loader-for-unity3d.172291/
        /// </remarks>
        public static Texture2D LoadTGA(Stream TGAStream)
        {
            try
            {
                using (BinaryReader r = new BinaryReader(TGAStream))
                {
                    TgaHeader th = LoadTgaHeader(r);
                    // Skip some header info we don't care about.
                    // Even if we did care, we have to move the stream seek point to the beginning,
                    // as the previous method in the workflow left it at the end.
                    //r.BaseStream.Seek( 12, SeekOrigin.Begin );

                    short width = (short)th.width;
                    short height = (short)th.height;
                    int bitDepth = th.bits;
                    //bool hflip = (th.tiDescriptor & 16) == 16;
                    bool vflip = (th.descriptor & 32) == 32;
                    //// Skip a byte of header information we don't care about.
                    //r.BaseStream.Seek( 1, SeekOrigin.Current );

                    Texture2D tex = new Texture2D(width, height);
                    Color32[] pulledColors = new Color32[width * height];
                    int length = width * height;

                    if (bitDepth == 32)
                    {
                        for (int row = 1; row <= height; row++)
                        {
                            for (int col = 0; col < width; col++)
                            {
                                byte red = r.ReadByte();
                                byte green = r.ReadByte();
                                byte blue = r.ReadByte();
                                byte alpha = r.ReadByte();

                                //					pulledColors [i] = new Color32(blue, green, red, alpha);
                                int idx;
                                if (vflip) idx = length - row * width + col;
                                else idx = length - ((height - row + 1) * width) + col;
                                pulledColors[idx] = new Color32(blue, green, red, alpha);
                            }
                        }
                    }
                    else if (bitDepth == 24)
                    {
                        for (int row = 1; row <= height; row++)
                        {
                            for (int col = 0; col < width; col++)
                            {
                                byte red = r.ReadByte();
                                byte green = r.ReadByte();
                                byte blue = r.ReadByte();

                                int idx;
                                if (vflip) idx = length - row * width + col;
                                else idx = length - ((height - row + 1) * width) + col;
                                pulledColors[idx] = new Color32(blue, green, red, 255);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("TGA texture had non 32/24 bit depth.");
                    }

                    tex.SetPixels32(pulledColors);
                    tex.Apply();
                    return tex;
                }
            }
            catch (Exception e)
            {
                // let everything else load even if this TGA loading failed
                Debug.LogWarning(e);
                return null;
            }
        }


        private static TgaHeader LoadTgaHeader(BinaryReader r)
        {
            TgaHeader th = new TgaHeader();

            r.BaseStream.Seek(0, SeekOrigin.Current);
            //  fread(&th, sizeof(gvImgTGAHeader), 1, fp);
            // due to byte alignment read the components one at a time
            th.identSize = r.ReadByte();
            th.colorMapType = r.ReadByte();
            th.imageType = r.ReadByte();
            th.colorMapStart = r.ReadUInt16();
            th.colorMapLength = r.ReadUInt16();
            th.colorMapBits = r.ReadByte();
            th.xStart = r.ReadUInt16();
            th.ySstart = r.ReadUInt16();
            th.width = r.ReadUInt16();
            th.height = r.ReadUInt16();
            th.bits = r.ReadByte();
            th.descriptor = r.ReadByte();
            Debug.LogFormat("TGA descriptor = {0}", th.descriptor);
            // check if the image contains no data
            if (th.imageType == 0) new Exception("TGA image contains no data."); // GV_IMG_ERROR_NO_DATA;

            // check for other types (compressed images)
            if (th.imageType > 10) new Exception("compressed TGA not supported."); //GV_IMG_ERROR_COMPRESSED_FILE;

            // check if the image is color indexed
            if (th.imageType == 1 || th.imageType == 9) new Exception("color indexed TGA not supported."); //GV_IMG_ERROR_INDEXED_COLOR;

            // for now we only consider 24bit rgb or rle images
            if (th.bits != 24 && th.bits != 32) throw new Exception("only 24/32 bits TGA supported."); //GV_IMG_ERROR_BITS;

            // width and height must be valid ones
            if (th.width <= 0 || th.height <= 0) throw new Exception("TGA texture has invalid size.");// GV_IMG_ERROR_SIZE;

            // skips id header
            r.BaseStream.Seek(th.identSize, SeekOrigin.Current);

            return th;
        }


        private class TgaHeader
        {
            public byte identSize;
            public byte colorMapType;
            public byte imageType;
            public ushort colorMapStart;
            public ushort colorMapLength;
            public byte colorMapBits;
            public ushort xStart;
            public ushort ySstart;
            public ushort width;
            public ushort height;
            public byte bits;
            public byte descriptor;
        }

    }
}
