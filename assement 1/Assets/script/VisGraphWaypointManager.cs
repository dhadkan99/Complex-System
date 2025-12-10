using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class VisGraphWaypointManager : MonoBehaviour
{
    // Allow you to set the waypoint text colour.
    private enum WaypointTextColour { Blue, Cyan, Yellow }

    [SerializeField]
    private WaypointTextColour waypointTextColour = WaypointTextColour.Blue;

    // List of all connections from this node.
    [SerializeField]
    private List<VisGraphConnection> connections = new List<VisGraphConnection>();
    public List<VisGraphConnection> Connections => connections;

    // Allow you to set a waypoint as a start or goal.
    private enum WaypointPropsList { Standard, Start, Goal }

    [SerializeField]
    private WaypointPropsList waypointType = WaypointPropsList.Standard;

    // Controls if the node type is displayed in the Unity editor.
    private const bool displayType = false;

    // Used to determine if the waypoint is selected.
    private bool objectSelected = false;

    // Text displayed above the node.
    private const bool displayText = true;
    private string infoText = "";
    private Color infoTextColor;

    void Start()
    {
        // Initialization logic if needed
    }

    void Update()
    {
        // Update logic if needed
    }

    // Draws debug objects in the editor and during play.
    void OnDrawGizmos()
    {
        infoText = "";

        if (displayType)
        {
            infoText = "Type: " + waypointType.ToString() + " / ";
        }

        infoText += gameObject.name + "\nConnections: " + Connections.Count;

        switch (waypointTextColour)
        {
            case WaypointTextColour.Blue:
                infoTextColor = Color.blue;
                break;
            case WaypointTextColour.Cyan:
                infoTextColor = Color.cyan;
                break;
            case WaypointTextColour.Yellow:
                infoTextColor = Color.yellow;
                break;
        }

        DrawWaypointAndConnections(objectSelected);

        if (displayText)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = infoTextColor;
            Handles.Label(transform.position + Vector3.up, infoText, style);
        }

        objectSelected = false;
    }

    // Draws debug objects when an object is selected.
    void OnDrawGizmosSelected()
    {
        objectSelected = true;
    }

    // Draws debug objects for the waypoint and its connections.
    private void DrawWaypointAndConnections(bool selected)
    {
        Color waypointColor = selected ? Color.red : Color.yellow;
        Color arrowHeadColor = selected ? Color.magenta : Color.blue;

        // Draw the waypoint sphere.
        Gizmos.color = waypointColor;
        Gizmos.DrawSphere(transform.position, 0.2f);

        // Draw all connections.
        for (int i = 0; i < Connections.Count; i++)
        {
            var connection = Connections[i];

            if (connection.ToNode != null)
            {
                if (connection.ToNode.Equals(gameObject))
                {
                    infoText = "WARNING - Connection to SELF at element: " + i;
                    infoTextColor = Color.red;
                }

                Vector3 direction = connection.ToNode.transform.position - transform.position;
                DrawConnection(transform.position, direction, arrowHeadColor);

                if (selected)
                {
                    // Draw spheres along the line
                    Gizmos.color = arrowHeadColor;
                    float dist = direction.magnitude;

                    for (int j = 1; j <= 3; j++)
                    {
                        float pos = dist * (j * 0.1f);
                        Gizmos.DrawSphere(transform.position + (direction.normalized * pos), 0.3f);
                    }
                }
            }
            else
            {
                infoText = "WARNING - Connection is missing at element: " + i;
                infoTextColor = Color.red;
            }
        }
    }

    // Draw a line to the connected node.
    private void DrawConnection(Vector3 from, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(from, from + direction);
    }
}
