using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows;

namespace Circles {

    /// <summary>
    /// System.Windows.Vector cannot be used in WinForms :/
    /// </summary>
    struct Vector {
        public Vector(float x, float y) {
            X = x;
            Y = y;
        }

        public Vector(Vector v) {
            X = v.X;
            Y = v.Y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public float this[int index] {
            get {
                if (index == 0)
                    return X;
                if (index == 1)
                    return Y;
                throw new InvalidOperationException();
            }
            set {
                if (index == 0)
                    X = value;
                if (index == 1)
                    Y = value;
                throw new InvalidOperationException();
            }
        }

        public float Length => (float)Math.Sqrt((double)(X * X + Y * Y));

        public static Vector operator +(Vector a, Vector b) => Add(a, b);
        public static Vector operator -(Vector a, Vector b) => Sub(a, b);
        public static Vector operator *(Vector a, Vector b) => Mult(a, b);
        public static Vector operator /(Vector a, Vector b) => Div(a, b);
        public static Vector operator +(Vector a, float b) => new Vector(a.X + b, a.Y + b);
        public static Vector operator -(Vector a, float b) => new Vector(a.X - b, a.Y - b);
        public static Vector operator *(Vector a, float b) => new Vector(a.X * b, a.Y * b);
        public static Vector operator /(Vector a, float b) => new Vector(a.X / b, a.Y / b);

        public static Vector Add(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
        public static Vector Sub(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);
        public static Vector Mult(Vector a, Vector b) => new Vector(a.X * b.X, a.Y * b.Y);
        public static Vector Div(Vector a, Vector b) => new Vector(a.X / b.X, a.Y / b.Y);
    }

    class Circle {
        public Vector Centre { get; set; }
        public float Radius { get; set; }

        public List<Circle> Neighbours { get; set; } = new List<Circle>();

        public Circle(Vector centre, float radius) {
            Centre = centre;
            Radius = radius;
        }

        private float MmToPixels(float mm, float DPI) => mm * 0.0393700787f * DPI;

        /// <summary>
        /// Draw the Circle on the form
        /// </summary>
        /// <param name="g"></param>
        public void DrawItself(Graphics g) {
            RectangleF rectangle = new RectangleF(MmToPixels(Centre[0] - Radius, g.DpiX), MmToPixels(Centre[1] - Radius, g.DpiX),
                MmToPixels(Radius, g.DpiX) * 2, MmToPixels(Radius, g.DpiX) * 2);
            g.DrawEllipse(new Pen(Color.FromName("SlateBlue")), rectangle);
        }


        /// <summary>
        /// Draw the Circle on the form with the specified colour
        /// </summary>
        /// <param name="g"></param>
        public void DrawItself(Graphics g, Color color) {
            RectangleF rectangle = new RectangleF(MmToPixels(Centre[0] - Radius, g.DpiX), MmToPixels(Centre[1] - Radius, g.DpiX),
                MmToPixels(Radius, g.DpiX) * 2, MmToPixels(Radius, g.DpiX) * 2);
            g.DrawEllipse(new Pen(color), rectangle);
        }
    }

    class Parser : IDisposable {
        TextReader reader;

        public Parser(TextReader tr) {
            reader = tr;
        }

        public void Dispose() {
            if (reader != null)
                reader.Dispose();
        }

        /// <summary>
        /// Read and process the input txt file
        /// </summary>
        /// <returns>List of the defined circles (cables)</returns>
        public List<Circle> ReadCircles() {
            string line = reader.ReadLine();
            List<Circle> circles = new List<Circle>();

            while (line != null) {
                if (line[0] != '#')
                    circles.Add(new Circle(new Vector(0f, 0f), float.TryParse(line, out float result) ? result : throw new InvalidDataException()));
                line = reader.ReadLine();
            }
            return circles;
        }
    }
}
