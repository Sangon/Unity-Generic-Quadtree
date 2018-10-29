using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IQuadTreeElement {
    Vector2 GetPosition();
    Rect GetBounds();
}

public enum NodeType {
    NW, NE, SW, SE
}

public class QNode<T> where T : IQuadTreeElement {

    public static readonly int NODE_POINT_LIMIT = 4;

    public QNode<T>[] childNodes;
    private int m_depth;


    public List<T> objects;

    public Rect m_bounds;

    public Vector2 Size {

        get {
            return new Vector2(m_bounds.width, m_bounds.height);
        }

    }

    //Extents are always half of width/height.
    public Vector2 Extents {

        get {
            return Size / 2;
        }

    }

    public QNode(float x, float y, float width, float height, T[] points, int depth) {
        objects = new List<T>();
        m_bounds = new Rect(x, y, width, height);
        m_depth = depth;
        objects.AddRange(points);
    }

    #region Constructor overloads
    public QNode(float width, float height, Vector2 pos, T[] points, int depth) : this(pos.x, pos.y, width, height, points, depth) { }
    public QNode(Vector2 size, float x, float y, T[] points, int depth) : this(x, y, size.x, size.y, points, depth) { }
    public QNode(Vector2 size, Vector2 pos, T[] points, int depth) : this(pos.x, pos.y, size.x, size.y, points, depth) { }
    public QNode(Rect rect, T[] points, int depth) : this(rect.x, rect.y, rect.width, rect.height, points, depth) { }
    #endregion

    public void Subdivide() {

        //If not over the point limit no need to subdivide.
        if ((NODE_POINT_LIMIT >= objects.Count)) {

            //If child nodes exist and we have less than the point limit then delete the child nodes.
            if (childNodes != null) {
                childNodes = null;
            }

            return;
        }

        childNodes = new QNode<T>[4];

        Rect c_r;
        IEnumerable<T> containedpoints;

        c_r = new Rect(new Vector2(m_bounds.x, m_bounds.y + Extents.y), Extents);
        containedpoints = ContainedPoints(c_r, objects);
        childNodes[(int)NodeType.NW] = new QNode<T>(c_r, containedpoints.ToArray(), m_depth + 1);
        childNodes[(int)NodeType.NW].Subdivide();

        c_r = new Rect(new Vector2(m_bounds.x + Extents.x, m_bounds.y + Extents.y), Extents);
        containedpoints = ContainedPoints(c_r, objects);
        childNodes[(int)NodeType.NE] = new QNode<T>(c_r, containedpoints.ToArray(), m_depth + 1);
        childNodes[(int)NodeType.NE].Subdivide();

        c_r = new Rect(new Vector2(m_bounds.x + Extents.x, m_bounds.y), Extents);
        containedpoints = ContainedPoints(c_r, objects);
        childNodes[(int)NodeType.SE] = new QNode<T>(c_r, containedpoints.ToArray(), m_depth + 1);
        childNodes[(int)NodeType.SE].Subdivide();

        c_r = new Rect(new Vector2(m_bounds.x, m_bounds.y), Extents);
        containedpoints = ContainedPoints(c_r, objects);
        childNodes[(int)NodeType.SW] = new QNode<T>(c_r, containedpoints.ToArray(), m_depth + 1);
        childNodes[(int)NodeType.SW].Subdivide();

    }

    public IEnumerable<T> ContainedPoints(Rect bounds, List<T> points) {
        return points.Where(p => bounds.Contains(p.GetPosition()));
    }

    public int GetDepth() {
        return m_depth;
    }

    public List<QNode<T>> GetChildren() {
        List<QNode<T>> nodes = new List<QNode<T>>();

        if (childNodes == null) {
            return nodes;
        }


        for (int i = 0; i < 4; i++) {

            if (childNodes[i] == null) {
                continue;
            }

            nodes.Add(childNodes[i]);
            nodes.AddRange(childNodes[i].GetChildren());

        }

        return nodes;
    }

    public int GetIndex(Rect pRect) {

        int index = -1;

        double verticalMidpoint = m_bounds.x + (m_bounds.width / 2);
        double horizontalMidpoint = m_bounds.y + (m_bounds.height / 2);

        bool topQuadrant = ((pRect.y + pRect.height) < horizontalMidpoint);
        bool bottomQuadrant = ((pRect.y > horizontalMidpoint) && (pRect.y + pRect.height > horizontalMidpoint));



        if (pRect.x < verticalMidpoint && pRect.x + pRect.width < verticalMidpoint) {

            if (topQuadrant) {
                index = (int)NodeType.NW;
            } else if (bottomQuadrant) {
                index = (int)NodeType.SW;
            }

        }

        if (pRect.x > verticalMidpoint) {

            if (topQuadrant) {
                index = (int)NodeType.NE;
            } else if (bottomQuadrant) {
                index = (int)NodeType.SE;
            }
        }

        return index;
    }

    public int GetIndexAdv(Rect pRect) {

        int idx = 0;

        double verticalMidpoint = m_bounds.x + (m_bounds.width / 2);
        double horizontalMidpoint = m_bounds.y + (m_bounds.height / 2);

        bool topQuadrant = ((pRect.y + pRect.height) > horizontalMidpoint);
        bool bottomQuadrant = (pRect.y < horizontalMidpoint);

        if (topQuadrant) {

            if (pRect.x < verticalMidpoint) {
                idx += 1;
            }

            if ((pRect.x + pRect.width) > verticalMidpoint) {
                idx += 2;
            }

        }

        if (bottomQuadrant) {

            if (pRect.x < verticalMidpoint) {
                idx += 4;
            }

            if ((pRect.x + pRect.width) > verticalMidpoint) {
                idx += 8;
            }

        }

        return idx;
    }

    public List<IQuadTreeElement> Retrieve(List<IQuadTreeElement> returnObjects, Rect pRect) {

        int index = GetIndex(pRect);

        if (index != -1 && childNodes != null) {
            childNodes[index].Retrieve(returnObjects, pRect);
        } else {
            returnObjects.AddRange(objects.ConvertAll(x => (IQuadTreeElement)x));
        }

        return returnObjects;
    }

    public List<T> RetrieveAdv(List<T> returnObjects, Rect pRect) {

        int index = GetIndexAdv(pRect);

        if (childNodes != null) {

            if ((index & 1) == 1) {
                childNodes[0].RetrieveAdv(returnObjects, pRect);

            }
            if ((index & 2) == 2) {
                childNodes[1].RetrieveAdv(returnObjects, pRect);

            }

            if ((index & 4) == 4) {
                childNodes[2].RetrieveAdv(returnObjects, pRect);

            }

            if ((index & 8) == 8) {
                childNodes[3].RetrieveAdv(returnObjects, pRect);

            }

        } else {
            returnObjects.AddRange(objects);
        }

        return returnObjects;
    }

}



