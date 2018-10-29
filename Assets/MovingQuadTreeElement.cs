using UnityEngine;

public class MovingQuadTreeElement : IQuadTreeElement {

    private Rect m_bounds;
    private Vector2 m_direction;

    public MovingQuadTreeElement() {
        m_bounds = new Rect(new Vector2(Screen.width * Random.value, Screen.height * Random.value),new Vector2(10f,10f));
        m_direction = new Vector2((Random.value * 4) - 1, (Random.value * 4) - 1);
    }

    public void UpdatePosition() {

        //Move the element in a direction.
        m_bounds.position += m_direction;

        //If the new position is outside the screen then flip the sign.
        if (m_bounds.position.x > Screen.width || m_bounds.position.x < 0) {
            m_direction.x = m_direction.x * -1;
        }

        if (m_bounds.position.y > Screen.height || m_bounds.position.y < 0) {
            m_direction.y = m_direction.y * -1;
        }

    }

    public Vector2 GetPosition() {
        return m_bounds.position;
    }

    public Rect GetBounds() {
        return m_bounds;
    }
}
