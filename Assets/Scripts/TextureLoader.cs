using System.Collections.Generic;
using UnityEngine;

public class TextureLoader : MonoBehaviour
{

    [System.Serializable]
    public class CubeTexture

    {
        public string TextureName;
        public Sprite XTexture, YTexture, ZTexture;
        public FaceTextures SpecificFaceTextures;
        [System.Serializable]
        public class FaceTextures
        {
            public Sprite Up, Down;
            [Space]
            public Sprite Left, Right;
            [Space]
            public Sprite Forward, Back;
        }

        public Vector2[] GetUVsAtDirection(Vector3Int Direction)
        {
            if (Direction == Vector3Int.forward)
                return ZTexture != null ? ZTexture.uv : SpecificFaceTextures.Forward.uv;
            else if (Direction == Vector3Int.back)
                return ZTexture != null ? ZTexture.uv : SpecificFaceTextures.Back.uv;
            if (Direction == Vector3Int.right)
                return XTexture != null ? XTexture.uv : SpecificFaceTextures.Right.uv;
            else if (Direction == Vector3Int.left)
                return XTexture != null ? XTexture.uv : SpecificFaceTextures.Left.uv;
            if (Direction == Vector3Int.up)
                return YTexture != null ? YTexture.uv : SpecificFaceTextures.Up.uv;
            else if (Direction == Vector3Int.down)
                return YTexture != null ? YTexture.uv : SpecificFaceTextures.Down.uv;
            return null;
        }
    }

    [SerializeField] 
    private CubeTexture[] CubeTextures;
    public Dictionary<int, CubeTexture> Textures;
    private void Awake()
    {
        Textures = new Dictionary<int, CubeTexture>();
        for (int i = 0; i < CubeTextures.Length; i++)
        {
            Textures.Add(i + 1, CubeTextures[i]);
        }
    }
}