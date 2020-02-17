using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace Circles {
    class CircleComparer : IComparer<Circle> {
        public int Compare(Circle x, Circle y) => x.Radius.CompareTo(y.Radius);
    }
    
    /// <summary>
    /// Diameter algorithm
    /// The alg. places the circles from the largest one to the smallest one in the bundle.
    /// It starts with the two largest circles and a smaller bundle radius. To place a circle c in the bundle,
    /// two circles are selected and the circle c is placed in the bundle as their neighbour. The two 
    /// circles must be chosen so that no two circles overlap and all circles are within the bundle.
    /// If no such two circles can be selected, the bundle radius is somewhat increased.
    /// 
    /// After any addition of a circle to the bundle, the center of the bundle is changed so that it is 
    /// the center of circles in the bundle and bundle radius is changed to be the minimal possible with
    /// invariants still holding.
    /// </summary>
    class Algorithm {
        public List<Circle> Circles { get; }
        Vector Centre { get; set; }
        List<Circle> Current { get; set; } = new List<Circle>();
        float Radius { get; set; } = 0;
        Random rnd = new Random();
        Graphics gg;
        public Algorithm(List<Circle> circles, Vector centre) {
            Circles = circles;
            Centre = centre;
        }

        private void Init() {
            Circles.Sort(new CircleComparer());
            Circles.Reverse();
        }

        private void EnlargeRadius(float scale) {
            Radius *= scale;
        }

        /// <summary>
        /// Place two largest circles in the circular bundle
        /// </summary>
        /// <returns></returns>
        private bool AddFirstTwo() {
            if (Circles.Count > 1) {
                Current.Add(Circles[0]);
                Circles[0].Centre = Centre;
                Current.Add(Circles[1]);
                Circles[1].Centre = new Vector( Circles[0].Centre[0] + Circles[0].Radius + Circles[1].Radius, Circles[0].Centre[1]);
                Circles[0].Neighbours.Add(Circles[1]);
                Circles[1].Neighbours.Add(Circles[0]);
                Radius = Circles[0].Radius * 1.2f;
                return true;
            }

            if (Circles.Count == 1) {
                Radius = Circles[0].Radius * 1.2f;
                return false;
            }
            Radius = 0;
            return false;
        }

        private bool Conflict(Circle c1, Circle c2) => (c1.Centre - c2.Centre).Length < c1.Radius + c2.Radius;

        /// <summary>
        /// Choose a neighbour of the first chosen circle to be the other circle used for placing next circle
        /// </summary>
        /// <param name="first"></param>
        /// <param name="c">Circle to be placed in the bundle</param>
        /// <returns></returns>
        private bool TrySecond(Circle first, Circle c) {
            int start = rnd.Next(0, first.Neighbours.Count);
            for (int i = 0; i < first.Neighbours.Count; i++) {
                int index = (i + start) % first.Neighbours.Count;
                if (TryAddCircle(first, first.Neighbours[index], c))
                    return true;
            }
            return false;
            foreach(var second in first.Neighbours) {
                if (TryAddCircle(first, second, c))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Try to place circle c in the bundle given first, second circles
        /// c is to be their neighbour
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="c"></param>
        /// <returns> true if operation was a success. Else false </returns>
        private bool TryAddCircle(Circle first, Circle second, Circle c) {
            Circle c1 = new Circle(first.Centre, first.Radius + c.Radius);
            Circle c2 = new Circle(second.Centre, second.Radius + c.Radius);

            float cDist = (second.Centre - first.Centre).Length;
            Vector v = new Vector((second.Centre - first.Centre) / (second.Centre - first.Centre).Length);
            Vector n = new Vector(-v.Y, v.X);
            float a = (cDist * cDist + c1.Radius * c1.Radius - c2.Radius * c2.Radius) / (2 * cDist);
            float h = (float)Math.Sqrt((float)(c1.Radius * c1.Radius - a * a));

            Vector i1 = c1.Centre + v * a;
            Vector i2 = i1 + n * h;
            Vector i3 = i1 - n * h;

            c.Centre = i2;
            //c.DrawItself(gg);
            if (ValidAddition(c)) {
                AddToCurrent(c, first, second);
                return true;
            }
            
            c.Centre = i3;
            //c.DrawItself(gg);
            if (ValidAddition(c)) {
                AddToCurrent(c, first, second);
                return true;
            }

            return false;
        }

        private void AddToCurrent(Circle c, Circle n1, Circle n2) {
            Current.Add(c);
            c.Neighbours.Add(n1);
            c.Neighbours.Add(n2);
            n1.Neighbours.Add(c);
            n2.Neighbours.Add(c);
        }

        /// <summary>
        /// Choose first of the two circles that determine the position of next circle c in the bundle
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool AddCircle(Circle c) {
            int start = rnd.Next(0, Current.Count);
            int index;
            for (int i = 0; i < Current.Count; i++) {
                index = (i + start) % Current.Count;
                if (TrySecond(Current[index], c)) 
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Check if circle c is within the bundle and if it does not overlap with another circle
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool ValidAddition(Circle c) {
            float eps = 0.00001f;
            if ((Centre - c.Centre).Length + c.Radius + eps > Radius)
                return false;

            // Can be improved by using VP-trees, R-trees...
            if (Current.TrueForAll(x => (c.Centre - x.Centre).Length + eps >= c.Radius + x.Radius)) {
                return true;
            }
            return false;        
        }

        /// <summary>
        /// Count distance to the furthest point of all circles from the bundle center
        /// </summary>
        /// <returns></returns>
        private float FindDistanceToFurthestPoint() {
            float max = 0f;
            foreach (var c in Current) {
                var len = (Centre - c.Centre).Length + c.Radius;
                if (len > max)
                    max = len;
            }
            return max;
        }

        private Vector FindCenterOfMass() {
            Vector tmp = new Vector(0f, 0f);
            float div = 0f;
            foreach (var c in Current) {
                tmp += c.Centre * c.Radius * c.Radius;
                div += c.Radius * c.Radius;
            }
            tmp = tmp / div;
            return tmp;
        }

        /// <summary>
        /// Should make circles move toward the bundle center - not working yet
        /// </summary>
        private void Jiggle() {
            for (int k = 0; k < 50; k++) {
                float forceToCentre = 0.5f;
                float repulsivePairForce = 0.5f;
                for (int i = 0; i < Current.Count; i++) {
                    Vector v = Centre - Current[i].Centre;
                    Current[i].Centre += v * forceToCentre;
                    for (int j = 0; j < Current.Count; j++) {
                        if (i == j) continue;
                        Vector u = Current[j].Centre - Current[i].Centre;
                        Current[i].Centre += repulsivePairForce / u.Length;
                        if (Conflict(Current[i], Current[j])) {
                            Vector w = Current[i].Centre - Current[j].Centre;
                            float error = Current[i].Radius + Current[j].Radius - w.Length;
                            Current[i].Centre += (w / w.Length) * error / 2f;
                            Current[j].Centre -= (w / w.Length) * error / 2f;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// The main method that implements the Diameter algorithm.
        /// </summary>
        /// <param name="g"></param>
        /// <returns>the minimal bundle diameter</returns>
        public float Run(Graphics g) {
            Init();
            gg = g;
            if (!AddFirstTwo())
                return Radius;
            int i = 2;
            while (Circles.Count != Current.Count) {
                Current.ForEach(x => x.DrawItself(g));
                //new Circle(Centre, Radius).DrawItself(g, Color.DarkKhaki);
                if (AddCircle(Circles[i])) {
                    ++i;
                    Centre = FindCenterOfMass();
                    Radius = FindDistanceToFurthestPoint();
                }
                else {
                    EnlargeRadius(1.2f);
                }
            }
            //Jiggle();
            //gg.Clear(Color.White);
            Current.ForEach(x => x.DrawItself(g));
            new Circle(Centre, 0.5f).DrawItself(g, Color.DarkRed);
            new Circle(Centre, Radius).DrawItself(g, Color.DarkKhaki);
            return Radius * 2f;
        }
    }
}
