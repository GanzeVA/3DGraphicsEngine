﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.WinForms
{
    public class Edge
    {
        public int Ymax { get; set; }
        public float Xmin { get; set; }
        public float Move { get; set; }
        public float Z { get; set; }
        public float ZMove { get; set; }

        public Edge(int ymax, float xmin, float move, float z, float zMove)
        {
            Ymax = ymax;
            Xmin = xmin;
            Move = move;
            Z = z;
            ZMove = zMove;
        }
    }

    class Face
    {
        public int Count { get; set; }
        public int[] VerticesInd { get; set; }
        public Color FaceColor { get; set; }

        public Face(int count, int[] verticesInd, Color faceColor)
        {
            Count = count;
            VerticesInd = verticesInd.ToArray();
            FaceColor = faceColor;
        }

        public void Draw(ref Bitmap b, PointF[] points, ref float[,] zLevel, float[] pointsZLevel)
        {
            int maxy = 0;
            int miny = int.MaxValue;
            Point textureStart = new Point(0, 0);
            List<Line> lines = new List<Line>();
            for (int i = 0; i < points.Length - 1; i++)
            {
                lines.Add(new Line(points[i], points[i + 1], pointsZLevel[i], pointsZLevel[i + 1]));
            }
            lines.Add(new Line(points[points.Length - 1], points[0], pointsZLevel[points.Length - 1], pointsZLevel[0]));
            foreach (Line l in lines)
            {
                if (l.Point1.Y > maxy)
                {
                    maxy = l.Point1.Y;
                }
                if (l.Point1.Y < miny || (l.Point1.Y == miny && l.Point1.X < textureStart.X))
                {
                    miny = l.Point1.Y;
                    textureStart = l.Point1;
                }
            }

            List<Edge>[] ET = new List<Edge>[maxy - miny + 1];
            int scanline = maxy - miny + 1;
            int ymin, ymax, xmin;
            float move, z, zMove;
            foreach (Line l in lines)
            {
                if (l.Point1.Y <= l.Point2.Y)
                {
                    ymin = l.Point1.Y;
                    xmin = l.Point1.X;
                    ymax = l.Point2.Y;
                    move = ((float)l.Point1.X - (float)l.Point2.X) / ((float)l.Point1.Y - (float)l.Point2.Y);
                }
                else
                {
                    ymin = l.Point2.Y;
                    xmin = l.Point2.X;
                    ymax = l.Point1.Y;
                    move = ((float)l.Point2.X - (float)l.Point1.X) / ((float)l.Point2.Y - (float)l.Point1.Y);
                }
                z = l.Point1Z;
                zMove = ((float)l.Point1Z - (float)l.Point2Z) / ((float)l.Point1Z - (float)l.Point2Z);
                if (ymin - miny < scanline)
                    scanline = ymin - miny;
                Edge ne = new Edge(ymax - miny, xmin, move, z, zMove);

                if (ET[ymin - miny] == null)
                {
                    ET[ymin - miny] = new List<Edge>() { ne };
                }
                else
                {
                    ET[ymin - miny].Add(ne);
                }
            }

            List<Edge> AET = new List<Edge>();
            while (AET.Count > 0 || scanline <= maxy - miny)
            {
                if (scanline <= maxy - miny && ET[scanline] != null)
                {
                    AET.AddRange(ET[scanline]);
                }

                for (int i = 0; i < AET.Count; i++)
                {
                    if (AET[i].Ymax == scanline)
                    {
                        AET.RemoveAt(i);
                        i--;
                    }
                }

                AET = AET.OrderBy(x => x.Xmin).ToList();

                for (int i = 0; i < AET.Count - 1; i += 2)
                {
                    for (int j = (int)AET[i].Xmin; j <= (int)AET[i + 1].Xmin; j++)
                    {
                        if (j >= 0 && scanline + miny >= 0 && scanline + miny < b.Height && j < b.Width)// && AET[i].Z > zLevel[j, scanline + miny])
                        {
                            b.SetPixel(j, scanline + miny, FaceColor);
                            zLevel[j, scanline + miny] = AET[i].Z;
                        }
                    }
                }

                scanline++;

                for (int i = 0; i < AET.Count; i++)
                {
                    AET[i].Xmin = (float)AET[i].Xmin + AET[i].Move;
                    AET[i].Z = (float)AET[i].Z + AET[i].ZMove;
                }
            }
        }
    }
}