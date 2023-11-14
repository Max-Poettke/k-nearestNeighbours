using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

class Point
{
    public GameObject gameObject;
    public Vector3 position;
    public Color color;
    public int ID;
    public List<Point> nearestNeighbours = new List<Point>();
    public Point(GameObject gameObject, Vector3 position, Color color, int ID)
    {
        this.gameObject = gameObject;
        this.position = position;
        this.color = color;
        this.ID = ID;
    }
}

public class Logic : MonoBehaviour
{
    //what we aim to do:
    //Randomly spawn n points of 2 different colors in a circle
    //put each grouping of points in their own list
    //Check the distance between all of the points and create a vertex at the half way point between each dot
    //Create a mesh from the vertices and color according to the color of the points
    //Watch out for intercepting points and make sure not to create a vertex if there is a point of our color
    //already closer to the point we are comparing with

    public Slider nSlider;
    public Slider kSlider;
    public TMP_InputField weightInput;
    public GameObject pointPrefab;

    private Dictionary <float, Point> points = new Dictionary<float, Point>();
    private float n = 0;
    private float k = 0;
    private Vector3 weight = new Vector3();
    private float detail = 20000;
    private List<Point> universalPoints = new List<Point>();

    // Update is called once per frame
    void Update()
    {
        n = nSlider.value;
        k = kSlider.value;
        weight = new Vector3(float.Parse(weightInput.text.Split(',')[0])
            , float.Parse(weightInput.text.Split(',')[1]));
    }

    public void SpawnPoints()
    {
        foreach (var p in points)
        {
            Destroy(p.Value.gameObject);
        }

        points = new Dictionary<float, Point>();

        for (int i = 0; i < n; i++)
        {
            //spawn a point
            GameObject newPoint = Instantiate(pointPrefab);

            //set the position of the point to a random position in a circle
            newPoint.transform.position = Random.insideUnitCircle * 5;

            //set the color of the point to one of two colors: red or blue
            if (Random.Range(0, 2) == 0)
            {
                newPoint.GetComponent<SpriteRenderer>().color = Color.magenta;
            }
            else
            {
                newPoint.GetComponent<SpriteRenderer>().color = Color.yellow;
            }

            Point point = new Point(newPoint, newPoint.transform.position,
                newPoint.GetComponent<SpriteRenderer>().color, i);
            points.Add(i, point);

        }
    }

    public void CreateMesh()
    {
        //create universal points distributed evenly within the circle that can be used to create the mesh with a detail variable
        foreach (var p in universalPoints)
        {
            Destroy(p.gameObject);
        }
        universalPoints.Clear();

        float pointsPerCircle = detail / 50;
        for (int i = 0; i < 50; i++)
        {
            for (int j = 0; j < pointsPerCircle; j++)
            {
                float distanceToCenter = 5 - (i * 0.1f);
                float angle = (j / pointsPerCircle) * Mathf.PI * 2;
                float x = Mathf.Cos(angle) * distanceToCenter;
                float y = Mathf.Sin(angle) * distanceToCenter;
                Vector3 position = new Vector3(x, y);
                Point point = new Point(null, position, Color.white, -1);
                universalPoints.Add(point);
            }
        }

        List<Point> nearestPoints = new List<Point>();
        foreach (var point in points)
        {
            nearestPoints.Add(point.Value);
        }

        foreach (var p in universalPoints)
        {
            // Clear nearestPoints for each universal point to get fresh nearest neighbors
            nearestPoints.Clear();

            // Add all points to the nearestPoints list so we can find the closest ones
            nearestPoints.AddRange(points.Values);

            // Sort nearest points by distance to universal point
            nearestPoints = nearestPoints.OrderBy(x => Vector3.Distance(p.position, x.position)).ToList();

            // Ensure k is not greater than the count of nearestPoints
            int validK = Mathf.Min(nearestPoints.Count, (int)k);

            // If there are no points, continue to the next iteration
            if (validK == 0) continue;

            // Calculate the average color of the k nearest points
            float averageRed = 0;
            float averageGreen = 0;
            float averageBlue = 0;

            for (int i = 0; i < validK; i++)
            {
                averageRed += nearestPoints[i].color.r;
                averageGreen += nearestPoints[i].color.g;
                averageBlue += nearestPoints[i].color.b;
            }

            averageRed /= validK;
            averageGreen /= validK;
            averageBlue /= validK;
            Color averageColor = new Color(averageRed, averageGreen, averageBlue, 0.3f);

            // Set the color of the universal point to the average color
            p.color = averageColor;

            // Instantiate a point Object at the universal point position with the color of the universal point
            GameObject newPoint = Instantiate(pointPrefab);
            newPoint.transform.position = p.position;
            newPoint.GetComponent<SpriteRenderer>().color = averageColor;
            
            p.gameObject = newPoint;
        }
    }
}
