using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniGame.Core.Rendering
{
    public class TextureLoader : MonoBehaviour
    {

        [System.Serializable]
        public class CubeTexture
        {
            public string TextureName;
            public Sprite XTexture, YTexture, ZTexture;

            [HideInInspector]
            public Vector2[] XTextureT, YTextureT, ZTextureT;

            public FaceTextures SpecificFaceTextures;

            [System.Serializable]
            public class FaceTextures
            {
                public Sprite Up, Down;
                [Space]
                public Sprite Left, Right;
                [Space]
                public Sprite Forward, Back;

                [HideInInspector]
                public Vector2[] UpT, DownT;
                [Space]
                [HideInInspector]
                public Vector2[] LeftT, RightT;
                [Space]
                [HideInInspector]
                public Vector2[] ForwardT, BackT;

                public void InitThreadSafeData()
                {
                    UpT = Up != null ? Up.uv : null;
                    DownT = Down != null ? Down.uv : null;

                    LeftT = Left != null ? Left.uv : null;
                    RightT = Right != null ? Right.uv : null;

                    ForwardT = Forward != null ? Forward.uv : null;
                    BackT = Back != null ? Back.uv : null;
                }
            }

            public void InitThreadSafeData()
            {
                if (XTexture != null)
                    XTextureT = XTexture.uv;

                if (YTexture != null)
                    YTextureT = YTexture.uv;

                if (ZTexture != null)
                    ZTextureT = ZTexture.uv;

                SpecificFaceTextures.InitThreadSafeData();
            }

            //This function is thread safe
            public Vector2[] GetUVsAtDirectionT(Vector3Int Direction)
            {
                if (Direction == Vector3Int.forward)
                    return ZTextureT.Length > 0 ? ZTextureT : SpecificFaceTextures.ForwardT;
                else if (Direction == Vector3Int.back)
                    return ZTextureT.Length > 0 ? ZTextureT : SpecificFaceTextures.BackT;

                if (Direction == Vector3Int.right)
                    return XTextureT.Length > 0 ? XTextureT : SpecificFaceTextures.RightT;
                else if (Direction == Vector3Int.left)
                    return XTextureT.Length > 0 ? XTextureT : SpecificFaceTextures.LeftT;

                if (Direction == Vector3Int.up)
                    return YTextureT.Length > 0 ? YTextureT : SpecificFaceTextures.UpT;
                else if (Direction == Vector3Int.down)
                    return YTextureT.Length > 0 ? YTextureT : SpecificFaceTextures.DownT;

                Debug.Log("Nil");
                return null;
            }
        }

        [SerializeField] private CubeTexture[] CubeTextures;
        public Dictionary<int, CubeTexture> Textures;


        private void Awake()
        {
            Textures = new Dictionary<int, CubeTexture>();

            for (int i = 0; i < CubeTextures.Length; i++)
            {
                CubeTextures[i].InitThreadSafeData();
                Textures.Add(i + 1, CubeTextures[i]);
            }
        }
    }
}