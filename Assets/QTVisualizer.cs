using System.Collections.Generic;
using UnityEngine;

public class QTVisualizer : MonoBehaviour {

    private Quadtree<MovingQuadTreeElement> m_QuadTree;

    List<IQuadTreeElement> SearchRectObjects = new List<IQuadTreeElement>();
    public Rect SearchRect;

    private Material renderMaterial;

    public bool RenderNodes = true;
    public bool RenderAllObjects = true;
    public bool RenderPotentialObjects = true;
    public bool RenderHitObjects = true;
    public bool updateObjectPositions = true;

    public int CurrentObjectCount = 0;

    private void Start() {

        m_QuadTree = new Quadtree<MovingQuadTreeElement>();
        Rect r = m_QuadTree.rootNode.m_bounds;
        SearchRect = new Rect(r.width / 8, r.height / 8, r.width / 4, r.height / 4);
        m_QuadTree.AddObjects(GenerateElements(500));

        if (!renderMaterial) {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            renderMaterial = new Material(shader);
            renderMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

    }

    void OnPostRender() {

        GL.PushMatrix();
        renderMaterial.SetPass(0);
        GL.LoadPixelMatrix();


        //BG RESET
        GL.Begin(GL.QUADS);
        GL.Color(Color.black);

        {
            GL.Vertex(new Vector3(0, 0));
            GL.Vertex(new Vector3(Screen.width, 0));
            GL.Vertex(new Vector3(Screen.width, Screen.height));
            GL.Vertex(new Vector3(0, Screen.height));
        }

        GL.End();
        //BG RESET

        //NODES
        GL.Begin(GL.LINES);
        GL.Color(Color.white);

        if (RenderNodes) {
            foreach (var node in m_QuadTree.GetNodes()) {
                GL.Vertex(new Vector2(node.m_bounds.xMin, node.m_bounds.center.y));
                GL.Vertex(new Vector2(node.m_bounds.xMax, node.m_bounds.center.y));

                GL.Vertex(new Vector2(node.m_bounds.center.x, node.m_bounds.yMin));
                GL.Vertex(new Vector2(node.m_bounds.center.x, node.m_bounds.yMax));
            }
        }

        GL.End();
        //NODES


        //OBJECTS
        GL.Begin(GL.QUADS);
        GL.Color(Color.red);

        if (RenderAllObjects) {

            foreach (IQuadTreeElement obj in m_QuadTree.rootNode.objects) {

                Rect b = obj.GetBounds();
                GL.Vertex(new Vector3(b.x, b.y));
                GL.Vertex(new Vector3(b.x, b.y + b.width));
                GL.Vertex(new Vector3(b.x + b.height, b.y + b.width));
                GL.Vertex(new Vector3(b.x + b.height, b.y));

            }
        }

        GL.End();
        //OBJECTS





        GL.Begin(GL.QUADS);
        GL.Color(Color.yellow);

        if (RenderPotentialObjects) {

            foreach (IQuadTreeElement obj in SearchRectObjects) {

                Rect b = obj.GetBounds();
                GL.Vertex(new Vector3(b.x, b.y));
                GL.Vertex(new Vector3(b.x, b.y + b.width));
                GL.Vertex(new Vector3(b.x + b.height, b.y + b.width));
                GL.Vertex(new Vector3(b.x + b.height, b.y));

            }

        }

        GL.End();

        GL.Begin(GL.QUADS);
        GL.Color(Color.green);

        if (RenderHitObjects) {

            foreach (IQuadTreeElement obj in SearchRectObjects) {

                Rect b = obj.GetBounds();

                if (SearchRect.Overlaps(b)) {

                    GL.Vertex(new Vector3(b.x, b.y));
                    GL.Vertex(new Vector3(b.x, b.y + b.width));
                    GL.Vertex(new Vector3(b.x + b.height, b.y + b.width));
                    GL.Vertex(new Vector3(b.x + b.height, b.y));

                }

            }

        }

        GL.End();

        GL.Begin(GL.LINES);
        GL.Color(Color.cyan);

        {
            GL.Vertex(new Vector3(SearchRect.x, SearchRect.y));
            GL.Vertex(new Vector3(SearchRect.xMax, SearchRect.y));

            GL.Vertex(new Vector3(SearchRect.xMax, SearchRect.y));
            GL.Vertex(new Vector3(SearchRect.xMax, SearchRect.yMax));

            GL.Vertex(new Vector3(SearchRect.xMax, SearchRect.yMax));
            GL.Vertex(new Vector3(SearchRect.x, SearchRect.yMax));

            GL.Vertex(new Vector3(SearchRect.x, SearchRect.yMax));
            GL.Vertex(new Vector3(SearchRect.x, SearchRect.y));
        }

        GL.End();

        GL.PopMatrix();
    }

    public void Add50Objects() {
        m_QuadTree.AddObjects(GenerateElements(50));
    }

    public void SetObjectMobility(bool b) {
        updateObjectPositions = b;
    }

    public void ClearTree() {
        m_QuadTree.Clear();
    }

    private void FixedUpdate() {

        SearchRectObjects.Clear();
        SearchRectObjects.AddRange(m_QuadTree.RetrieveAdv(SearchRect));

        if (updateObjectPositions) {
            for (int i = 0; i < m_QuadTree.rootNode.objects.Count; i++) {
                MovingQuadTreeElement g = m_QuadTree.rootNode.objects[i] as MovingQuadTreeElement;
                g.UpdatePosition();
            }

            m_QuadTree.Subdivide();
        }
    }

    private void Update() {

        SearchRect.center = Input.mousePosition;
        Vector2 sizeMod = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? new Vector2(Input.mouseScrollDelta.y * 10f, 0) : new Vector2(0, Input.mouseScrollDelta.y * 10f);
        SearchRect.size += sizeMod;

    }

    private MovingQuadTreeElement[] GenerateElements(int count) {
        MovingQuadTreeElement[] objects = new MovingQuadTreeElement[count];

        for (int i = 0; i < count; i++) {
            objects[i] = new MovingQuadTreeElement();
        }

        CurrentObjectCount += count;

        return objects;
    }

}
