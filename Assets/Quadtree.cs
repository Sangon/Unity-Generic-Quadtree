using System.Collections.Generic;
using UnityEngine;

public class Quadtree<T> where T : IQuadTreeElement {

    public QNode<T> rootNode;

    public Quadtree() {
        rootNode = new QNode<T>(0, 0, Screen.width, Screen.height, new T[] { }, 0);
    }

    public void Subdivide() {
        rootNode.Subdivide();
    }

    public void AddObject(T @object) {
        rootNode.objects.Add(@object);
        Subdivide();
    }

    //TODO: Do an insertion instead of calculating the whole thing.
    public void AddObjects(T[] objects) {
        rootNode.objects.AddRange(objects);
        Subdivide();
    }

    public void Clear() {
        rootNode.objects.Clear();
        Subdivide();
    }

    public List<QNode<T>> GetNodes() {
        List<QNode<T>> nodes = new List<QNode<T>>();
        nodes.Add(rootNode);
        nodes.AddRange(rootNode.GetChildren());
        return nodes;
    }

    public List<IQuadTreeElement> Retrieve(List<IQuadTreeElement> returnObjects, Rect pRect) {

        int index = rootNode.GetIndex(pRect);

        if (index != -1) {
            rootNode.childNodes[index].Retrieve(returnObjects, pRect);
        } else {
            returnObjects.AddRange(rootNode.objects.ConvertAll(x => (IQuadTreeElement)x));
        }

        return returnObjects;
    }

    public List<IQuadTreeElement> RetrieveAdv(Rect pRect) {

        var returnObjects = new List<T>();
        int index = rootNode.GetIndexAdv(pRect);

        if (index == 0 || rootNode.childNodes == null) {
            return rootNode.objects.ConvertAll(x => (IQuadTreeElement)x);
        }

        if ((index & 1) == 1) {
            rootNode.childNodes[0].RetrieveAdv(returnObjects, pRect);

        }
        if ((index & 2) == 2) {
            rootNode.childNodes[1].RetrieveAdv(returnObjects, pRect);

        }

        if ((index & 4) == 4) {
            rootNode.childNodes[2].RetrieveAdv(returnObjects, pRect);

        }

        if ((index & 8) == 8) {
            rootNode.childNodes[3].RetrieveAdv(returnObjects, pRect);

        }


        return returnObjects.ConvertAll(x => (IQuadTreeElement)x);

    }

}


