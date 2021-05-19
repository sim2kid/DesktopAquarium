using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace DesktopAquarium
{
    /// <summary>
    /// Creates a Tank using the position and size of a <see cref="Camera"/>.
    /// </summary>
    public class TankGenerator : MonoBehaviour
    {
        /// <summary>
        /// The camera that is monitored by this class.
        /// </summary>
        [SerializeField]
        private Camera camToMonitor;
        /// <summary>
        /// The Z position of a tank.
        /// </summary>
        [SerializeField]
        private float tankFront = 10;
        /// <summary>
        /// The outter depth of the tank.
        /// </summary>
        [SerializeField]
        private float tankDepth = 20;
        /// <summary>
        /// The material used for the edges of the tank.
        /// </summary>
        [SerializeField]
        private Material edgeMat;
        /// <summary>
        /// The material used for the bottom of the tank.
        /// </summary>
        [SerializeField]
        private Material bottomMat;
        /// <summary>
        /// The material used for the top of the tank.
        /// </summary>
        [SerializeField]
        private Material topMat;
        /// <summary>
        /// The size of the cube that represents the edges.
        /// </summary>
        [SerializeField]
        private float edgeSize = 0.1f;

        /// <summary>
        /// The bottom left position of the tank in Game Space
        /// </summary>
        private Vector2 bottomLeft;
        /// <summary>
        /// The top right position of the tank in Game Space
        /// </summary>
        private Vector2 topRight;

        /// <summary>
        /// The top/bottom <see cref="GameObject"/> for the tank.
        /// </summary>
        private GameObject tankTop, tankBottom;
        /// <summary>
        /// The edge of the tank.
        /// </summary>
        private GameObject tankEdge;

        private void Start()
        {
            if(camToMonitor == null)
                camToMonitor = Camera.main;

            getGameSpacePositions();
            generateEdgeMesh();
            tankEdge.transform.position = camToMonitor.transform.position;
        }

        void Update()
        {

        }

        #region Get/Set
        /// <summary>
        /// Assings a camera to map a tank's size to.
        /// </summary>
        /// <param name="toAssign">The camera to monitor.</param>
        public void AssignCamera(Camera toAssign) 
        {
            camToMonitor = toAssign;
        }
        #endregion



        #region Tank Creation
        private void getGameSpacePositions() 
        {
            Vector3 botLeft = camToMonitor.ScreenToWorldPoint(new Vector3(0, 0, tankFront));
            Vector3 topRight = camToMonitor.ScreenToWorldPoint(new Vector3(camToMonitor.pixelWidth, camToMonitor.pixelHeight, tankFront));

            bottomLeft = botLeft;
            this.topRight = topRight;
        }

        /// <summary>
        /// Uses <see cref="ProBuilderMesh"/> to generate the tank's mesh from scratch.
        /// </summary>
        private void generateEdgeMesh() 
        {
            Vector3[] points = {
            /*00 Vector3 FrameFrontBottomLeft */ new Vector3(bottomLeft.x, bottomLeft.y, tankFront),
            /*01 Vector3 FrameFrontTopRight */ new Vector3(topRight.x, topRight.y, tankFront),
            /*02 Vector3 FrameFrontTopLeft */ new Vector3(bottomLeft.x, topRight.y, tankFront),
            /*03 Vector3 FrameFrontBottomRight */ new Vector3(topRight.x, bottomLeft.y, tankFront),

            /*04 Vector3 FrameBackBottomLeft */ new Vector3(bottomLeft.x, bottomLeft.y, tankDepth),
            /*05 Vector3 FrameBackTopRight */ new Vector3(topRight.x, topRight.y, tankDepth),
            /*06 Vector3 FrameBackTopLeft */ new Vector3(bottomLeft.x, topRight.y, tankDepth),
            /*07 Vector3 FrameBackBottomRight */ new Vector3(topRight.x, bottomLeft.y, tankDepth),

            /*08 Vector3 innerFrameFrontBottomLeft */ new Vector3(bottomLeft.x + edgeSize, bottomLeft.y + edgeSize, tankFront + edgeSize),
            /*09 Vector3 innerFrameFrontTopRight */ new Vector3(topRight.x - edgeSize, topRight.y - edgeSize, tankFront + edgeSize),
            /*10 Vector3 innerFrameFrontTopLeft */ new Vector3(bottomLeft.x + edgeSize, topRight.y - edgeSize, tankFront + edgeSize),
            /*11 Vector3 innerFrameFrontBottomRight */ new Vector3(topRight.x - edgeSize, bottomLeft.y + edgeSize, tankFront + edgeSize),

            /*12 Vector3 innerFrameBackBottomLeft */ new Vector3(bottomLeft.x + edgeSize, bottomLeft.y + edgeSize, tankDepth - edgeSize),
            /*13 Vector3 innerFrameBackTopRight */ new Vector3(topRight.x - edgeSize, topRight.y - edgeSize, tankDepth - edgeSize),
            /*14 Vector3 innerFrameBackTopLeft */ new Vector3(bottomLeft.x + edgeSize, topRight.y - edgeSize, tankDepth - edgeSize),
            /*15 Vector3 innerFrameBackBottomRight */ new Vector3(topRight.x - edgeSize, bottomLeft.y + edgeSize, tankDepth - edgeSize),

            /*16 Vector3 GlassFrontBottomLeft */ new Vector3(bottomLeft.x + edgeSize, bottomLeft.y + edgeSize, tankFront),
            /*17 Vector3 GlassFrontTopRight */ new Vector3(topRight.x - edgeSize, topRight.y - edgeSize, tankFront),
            /*18 Vector3 GlassFrontTopLeft */ new Vector3(bottomLeft.x + edgeSize, topRight.y - edgeSize, tankFront),
            /*19 Vector3 GlassFrontBottomRight */ new Vector3(topRight.x - edgeSize, bottomLeft.y + edgeSize, tankFront),

            /*20 Vector3 GlassBackBottomLeft */ new Vector3(bottomLeft.x + edgeSize, bottomLeft.y + edgeSize, tankDepth),
            /*21 Vector3 GlassBackTopRight */ new Vector3(topRight.x - edgeSize, topRight.y - edgeSize, tankDepth),
            /*22 Vector3 GlassBackTopLeft */ new Vector3(bottomLeft.x + edgeSize, topRight.y - edgeSize, tankDepth),
            /*23 Vector3 GlassBackBottomRight */ new Vector3(topRight.x - edgeSize, bottomLeft.y + edgeSize, tankDepth),

            /*24 Vector3 GlassLeftBottomBack */ new Vector3(bottomLeft.x, bottomLeft.y + edgeSize, tankDepth - edgeSize),
            /*25 Vector3 GlassLeftTopBack */ new Vector3(bottomLeft.x, topRight.y - edgeSize, tankDepth - edgeSize),
            /*26 Vector3 GlassLeftTopFront */ new Vector3(bottomLeft.x, topRight.y - edgeSize, tankFront + edgeSize),
            /*27 Vector3 GlassLeftBottomFront */ new Vector3(bottomLeft.x, bottomLeft.y + edgeSize, tankFront + edgeSize),

            /*28 Vector3 GlassRightBottomFront */ new Vector3(topRight.x, bottomLeft.y + edgeSize, tankDepth - edgeSize),
            /*29 Vector3 GlassRightTopFront */ new Vector3(topRight.x, topRight.y - edgeSize, tankDepth - edgeSize),
            /*30 Vector3 GlassRightTopBack */ new Vector3(topRight.x, topRight.y - edgeSize, tankFront + edgeSize),
            /*31 Vector3 GlassRightBottomBack */ new Vector3(topRight.x, bottomLeft.y + edgeSize, tankFront + edgeSize),

            /*32 Vector3 GlassTopFrontLeft */ new Vector3(bottomLeft.x + edgeSize, topRight.y, tankFront + edgeSize),
            /*33 Vector3 GlassTopBackLeft */ new Vector3(bottomLeft.x + edgeSize, topRight.y, tankDepth - edgeSize),
            /*34 Vector3 GlassTopBackRight */ new Vector3(topRight.x - edgeSize, topRight.y, tankDepth - edgeSize),
            /*35 Vector3 GlassTopFrontRight */ new Vector3(topRight.x - edgeSize, topRight.y, tankFront + edgeSize),

            /*36 Vector3 GlassBottomFrontLeft */ new Vector3(bottomLeft.x + edgeSize, bottomLeft.y, tankFront + edgeSize),
            /*37 Vector3 GlassBottomBackLeft */ new Vector3(bottomLeft.x + edgeSize, bottomLeft.y, tankDepth - edgeSize),
            /*38 Vector3 GlassBottomBackRight */ new Vector3(topRight.x - edgeSize, bottomLeft.y, tankDepth - edgeSize),
            /*39 Vector3 GlassBottomFrontRight */ new Vector3(topRight.x - edgeSize, bottomLeft.y, tankFront + edgeSize)
            };

            Face[] faces = { 
                new Face(new int[] {2,1,17}),
                new Face(new int[] {2,17,18}),
                new Face(new int[] {1,3,17}),
                new Face(new int[] {17,3,19}),
                new Face(new int[] {19,3,0}),
                new Face(new int[] {0,16,19}),
                new Face(new int[] {2,18,16}),
                new Face(new int[] {16,0,2}),

                new Face(new int[] {16,8,11}),
                new Face(new int[] {16,11,19}),
                new Face(new int[] {16,18,10}),
                new Face(new int[] {16,10,8})
            };

            ProBuilderMesh tankMesh = ProBuilderMesh.Create(points, faces);
            tankEdge = tankMesh.gameObject;
            tankEdge.GetComponent<Renderer>().material = edgeMat;
        }
        #endregion
    }
}