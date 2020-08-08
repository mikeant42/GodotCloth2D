using Godot;
using Godot.Collections;
using Cloth;
using System.Collections.Generic;
using System;

namespace Cloth
{
    // Dynamic 2d cloth simulation using verlet integration based off this python project: https://code.google.com/archive/p/pythoncloth/
    // TODO 1. COLLISIONS
    public class Cloth : Polygon2D
    {

        private Particle[,] grid;
        private List<Vector2> drawPoints;
        private List<Vector2> drawUvs;

        [Export]
        public int rows = 4;
        [Export]
        public int columns = 8;

        private Timer timer;

        [Export]
        public float refreshRate = 0.01f;
        [Export]
        public float timeStep = 1.5f;
        [Export]
        public float gravity = 0.005f;
        [Export]
        public bool debugFollowMouse = true;
        [Export]
        public float scaling = 15;


        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            drawPoints = new List<Vector2>();
            drawUvs = new List<Vector2>();
            CreateGrid();

            timer = new Timer();
            timer.Connect("timeout", this, "_OnTimeout");
            timer.WaitTime = refreshRate;
            AddChild(timer);
            timer.Start();
            
            
        }

        public void _OnTimeout()
        {
            Verlet();
            SatisfyConstraints();
            SatisfyConstraints();
        }

        public override void _Process(float delta)
        {
            Update();
            if (debugFollowMouse)
                FollowMouse();

        }

        private void FollowMouse()
        {
            Vector2 mousePos = GetLocalMousePosition();
            float distance = 300;

            float x = Mathf.Lerp(Transform.origin.x, mousePos.x, 0.2f);
            float y = Mathf.Lerp(Transform.origin.y, mousePos.y, 0.2f);

            grid[0,0].currentPos = new Vector2(mousePos.x/scaling,(mousePos.y)/scaling);
            grid[0,0].previousPos = grid[0,0].currentPos;
            
             grid[rows/2,0].currentPos = new Vector2((mousePos.x+(distance/2))/scaling,(mousePos.y)/scaling);
             grid[rows/2,0].previousPos = grid[4,0].currentPos;

            grid[rows-1,0].currentPos = new Vector2((mousePos.x+distance)/scaling,(mousePos.y)/scaling);
            grid[rows-1,0].previousPos = grid[rows-1,0].previousPos;
        }
        

        public override void _Draw()
        {
            SortGridForDrawing();
            CreateMesh();   

        }

        private void CreateMesh()
        {
  
            IList<Point> points = new List<Point>();
            foreach (var dot in drawPoints)
            {
                points.Add(new Point(dot.x, dot.y));
            }

            IList<Point> hull = ConvexHull.MakeHull(points);
            List<Vector2> polygon = new List<Vector2>();

            List<Vector2> uvs = new List<Vector2>();

            foreach (var point in hull)
            {
                polygon.Add(new Vector2((float)point.x,(float)point.y));
                uvs.Add(new Vector2((float)point.x / 10, (float)point.y / 10));
            }
            Polygon = (polygon.ToArray());
            //Uv = uvs.ToArray();


            drawPoints.Clear();
            drawUvs.Clear();


            
           
        }


        private void SortGridForDrawing()
        {
            Color lineColor = new Color(1, 0, 0);
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Particle particle = grid[i,j];
                    
                    grid[i,j].pixelPosition = new Vector2 (particle.currentPos.x*scaling, particle.currentPos.y*scaling);
                    List<Vector2> neighbors = particle.neighbors;

                    foreach (var neighbor in particle.neighbors)
                    {
                        Particle point = grid[(int)neighbor.x, (int)neighbor.y]; //!!! (int)
                        if (point == null)
                            return;
                        
                        Vector2 toLine = new Vector2(point.currentPos.x*scaling, point.currentPos.y*scaling);

                        //DrawLine(particle.pixelPosition, toLine, new Color(1,0,0));
                        drawPoints.Add(particle.pixelPosition);
                        //drawUvs.Add(particle.pixelPosition / 10);
                        drawUvs.Add(toLine);       
                        
                    }
                    Particle prevParticle = particle;
                }
            }
            // grid[rows-1,0].stuck = true;
            // grid[0,0].stuck = true;

            
        }

        private void CreateGrid()
        {
            grid = new Particle[rows,columns];

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                
                // Filling out the array
                //grid[x] = new Particle[columns];

                for (int y = 0; y < columns; y++)
                {
                    //grid[x,y] = new Particle();
                    Vector2 currentPos = new Vector2(x,y);
                    Particle particle = new Particle();
                    particle.currentPos = currentPos;
                    particle.previousPos = currentPos;
                    particle.forces = new Vector2(0, gravity);
                    particle.restLength = 1;
                    particle.listPosition = new Vector2(x,y);
                    particle.stuck = false;
                    particle.pixelPosition = new Vector2(0,0);
                    particle.neighbors = new List<Vector2>();

                    grid[x,y] = particle;

                }
            }

            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    List<Vector2> neighbors = findNeighbors(grid[x,y].listPosition, grid);
                    grid[x,y].neighbors = neighbors;
                }
            }
        }

        private List<Vector2> findNeighbors(Vector2 pointPosition, Particle[,] grid)
        {
            int columnLimit = grid.GetLength(1);
            int rowLimit = grid.GetLength(0);

            List<Vector2> possNeighbors = new List<Vector2>();
            possNeighbors.Add(new Vector2(pointPosition.x-1, pointPosition.y));
            possNeighbors.Add(new Vector2(pointPosition.x, pointPosition.y-1));
            possNeighbors.Add(new Vector2(pointPosition.x+1, pointPosition.y));
            possNeighbors.Add(new Vector2(pointPosition.x, pointPosition.y+1));


            List<Vector2> neig = new List<Vector2>();

            // Loop and find the qualified potential neighbors
            foreach (var coord in possNeighbors)
            {
                if (coord.x < 0 | (coord.x > rowLimit-1))
                {
                    
                } else if (coord.y < 0 | (coord.y > columnLimit - 1))
                {

                } 
                else
                {
                    neig.Add(coord);
                }
            }

            List<Vector2> finalNeighbors = new List<Vector2>();

            foreach (var point in neig)
            {
                finalNeighbors.Add(new Vector2(point.x, point.y));
            }

            return finalNeighbors;
        }


        private void Verlet()
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    Particle particle = grid[x,y];
                    if (particle.stuck)
                        particle.currentPos = particle.previousPos;
                    else
                    {
                        Vector2 c_p = particle.currentPos;
                        Vector2 temp = c_p;
                        Vector2 p_p = particle.previousPos;
                        Vector2 f = particle.forces;

                        var fmultbytime = new Vector2(f.x*timeStep*timeStep, f.y*timeStep*timeStep);
                        var tempminusp_p = new Vector2(c_p.x-p_p.x, c_p.y-p_p.y);
                        var together = new Vector2(fmultbytime.x+tempminusp_p.x, fmultbytime.y+tempminusp_p.y);
                        c_p = new Vector2(c_p.x+together.x, c_p.y+together.y);

                        particle.currentPos = c_p;
                        particle.previousPos = temp;
                    }
                }
            }
        }

        private void SatisfyConstraints()
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    Particle particle = grid[x,y];
                    if (particle.stuck)
                        particle.currentPos = particle.previousPos;
                    else
                    {
                        foreach(var constraint in particle.neighbors)
                        {
                            var c2 = grid[(int)constraint.x,(int)constraint.y].currentPos; //!! (int)
                            var c1 = particle.currentPos;
                            Vector2 delta = new Vector2(c2.x-c1.x, c2.y-c1.y);
                            float deltaLength = (float)Math.Sqrt(Math.Pow((c2.x-c1.x),2) + Math.Pow((c2.y-c1.y),2));
                            float diff = (deltaLength - 1.0f) / deltaLength;

                            var dtemp = new Vector2(delta.x*0.5f*diff, delta.y*0.5f*diff);

                            c1.x += dtemp.x;
                            c1.y += dtemp.y;
                            c2.x -= dtemp.x;
                            c2.y -= dtemp.y;

                            particle.currentPos = c1;
                            grid[(int)constraint.x,(int)constraint.y].currentPos = c2; // !! (int)
                        }
                        
                    }
                }
            }
        }



    }

}
