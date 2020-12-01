using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Threading;
using System.Drawing.Drawing2D;

namespace WindowsFormsApp13
{
    public partial class Form1 : Form
    {
        //Use W and S to zoom in and out

        //The distance between each point on the graph. Make this value lower if you want better looking lines
        float lineRes = 7;

        //Line thickness, 
        float lineThickness = 5;

        //Bounds for functions
        float startT;
        float endT;

        //UI
        int pointHeld = -1;

        //Functions

        // ( t, cost)
        static float x1(float t)
        {
            return (float) (t);
        }

        static float y1(float t)
        {
            return (float)Math.Cos(t);
        }

        //(sint, sin1.1t)
        static float x2(float t)
        {
            return (float)Math.Sin(t);
        }

        static float y2(float t)
        {
            return (float)Math.Sin(1.1 * t);
        }

        //(t, 1/x)
        static float x3(float t)
        {
            return (float) t;
        }

        static float y3(float t)
        {
            return (float) 1/t;
        }

        //Function that passes through (1,1) (2,3) (3,1) (4,2) This may cause a bit of lag
        /*
        static float x4(float t)
        {
            return (float)t;
        }

        static float y4(float t)
        {
            float[,] points = new float[,]{ { 1, 1 }, { 2, 3 }, { 3, 1 }, { 4, 2 } };

            float[,] system = toSystem(points);
            
            float[] coefficients = solveGuass(Guass(system));

            float answer = 0;
            
            for(int i = 0; i < coefficients.GetLength(0); i++)
            {
                answer += (float) ( coefficients[i] * Math.Pow(t, i) );
            }

            return answer;
        }
        */

        //Array of functions, if you want to add a function put it in this array
        Func<float, float>[,] functions = { {x1, y1 }, {x2, y2 }, {x3, y3}};

        //Bounds, if you want it to be set automatically. Set a value to null. 
        //Only works for functions, as I'm not sure how to optimize parametrics
        float?[,] bounds = { {null , null }, {0, (float) Math.PI*10 }, {null, null}};

        //Colors
        Color[] colors = {Color.Black, Color.Red, Color.Green};








        //Other variables for my code
        String text = "";
        float textX = -15;
        float textY = -15;

        float precision = 0.1f;

        static float offsetX = 100;
        static float offsetY = 0;

        float zoom = 500;

        int mX = -1;
        int mY = -1;

        float ooX = offsetX;
        float ooY = offsetY;

        float incrementX = 0.1f;
        float incrementY = 0.1f;

        bool mouseDown = false;


        public Form1()
        {
            InitializeComponent();
            this.Width = 1000;
            this.Height = 1000;
        }

        void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.FromArgb(200, Color.Red));
            Brush brush = new SolidBrush(Color.Red);
            Brush bruhmoment = new SolidBrush(Color.Gray);

            pen.Width = 2;

            //X and Y number line increments
            incrementX = 100 / zoom;
            incrementY = 100 / zoom;

            //Auto-Bound a function such that it only loads whatever is on-screen
            startT = -(offsetX / zoom);
            endT = -(offsetX / zoom) + this.Width / zoom;

            //The distance between the dots drawn on the functions.
            precision = 2f / zoom;

            //g.Clear(Color.White);

            //For each function contained in the array
            for (int f = 0; f < functions.GetLength(0); f++)
            {
                //Check in the bound array if the bound corresponding to the function is "null". If it is null, use auto-bound feature.
                float bound1;
                float bound2;

                if (bounds[f, 0] == null)
                {
                    bound1 = startT;
                    bound2 = endT;
                }

                else
                {
                    bound1 = (float)bounds[f, 0];
                    bound2 = (float)bounds[f, 1];
                }

                //Call draw function with the paramaters.
                drawFunction(functions[f, 0], functions[f, 1], bound1, bound2, offsetX, offsetY, zoom, precision, f, colors[f]);
            }

            //Draw grid
            drawGrid(offsetX, offsetY, incrementX, incrementY, zoom, Color.FromArgb(150, Color.Black), Color.FromArgb(128, Color.Black), Color.Black);

            //The text that is drawn when a user is seeing what a value is on a function.
            g.FillRectangle(bruhmoment, new Rectangle((int)textX, (int)textY + 15, (int)(text.Length * 9.5), (int)20));
            g.DrawString(text, this.Font, brush, textX + 10, textY - 20);
            g.DrawEllipse(pen, new Rectangle((int)textX - 7, (int)textY - 7, 20, 20));


            void drawFunction(Func<float, float> xt, Func<float, float> yt, float start, float end, float oX, float oY, float z, float p, int index, Color lineColor)
            {
                //Pen lc = new Pen(lineColor);
                Brush circles = new SolidBrush(lineColor);

                //lc.Width = 5;

                float cX = Cursor.Position.X - this.Location.X;
                float cY = Cursor.Position.Y - this.Location.Y;

                //Draw line
                for (float t = start; t <= end; t += p)
                {
                    float x = xt(t) * z + oX;
                    float y = this.Height - (yt(t) * z + oY);

                    float x2 = xt(t + p) * z + oX;
                    float y2 = this.Height - (yt(t + p) * z + oY);

                    float dist = (float)Math.Sqrt(Math.Pow(x2 - x, 2) + Math.Pow(y2 - y, 2));

                    if ((y2 < 0 || y2 > this.Height) || (x2 < 0 || x2 > this.Width)) continue;

                    g.FillEllipse(circles, new Rectangle((int)x, (int)y, (int)lineThickness, (int)lineThickness));

                    p *= lineRes / dist;

                    if (Math.Abs(y - (cY - 15)) < 20 && Math.Abs(x - (cX - 15)) < 20 && (pointHeld == -1 || pointHeld == index) && mouseDown == true)
                    {
                        pointHeld = index;

                        text = xt(t) + " , " + yt(t) + "  t: " + t;
                        textX = x;
                        textY = y;
                    }

                }
            }

            void drawGrid(float oX, float oY, float ix, float iy, float z, Color gridColor, Color gridPattern, Color numberColor)
            {
                Pen gc = new Pen(gridColor);
                Pen gp = new Pen(gridPattern);
                Brush b = new SolidBrush(numberColor);

                gc.Width = 3;

                //Draw y axis
                g.DrawLine(gc, oX, 0, oX, this.Height);

                //Draw the top
                for (float t = z * iy; t <= this.Height - oY; t += z * iy)
                {
                    g.DrawLine(gc, oX - 10, this.Height - (oY + t), oX + 10, this.Height - (oY + t));
                    g.DrawLine(gp, -this.Width, this.Height - (oY + t), this.Width, this.Height - (oY + t));

                    g.DrawString(t / z + "", this.Font, b, oX - 20, this.Height - (oY + t + z * iy * 0.5f));
                }

                //Draw the bottom
                for (float t = -(z * iy); t >= -this.Height - oY; t -= z * iy)
                {
                    g.DrawLine(gc, oX - 10, this.Height - (oY + t), oX + 10, this.Height - (oY + t));
                    g.DrawLine(gp, -this.Width, this.Height - (oY + t), this.Width, this.Height - (oY + t));

                    g.DrawString(t / z + "", this.Font, b, oX - 20, this.Height - (oY + t + z * iy * 0.5f));
                }


                //Draw x axis
                g.DrawLine(gc, 0, this.Height - (oY), this.Width, this.Height - (oY));

                //Draw the right side
                for (float t = z * iy; t <= this.Width - oX; t += z * ix)
                {
                    g.DrawLine(gc, oX + t, this.Height - (oY + 10), oX + t, this.Height - (oY - 10));
                    g.DrawLine(gp, oX + t, -this.Height, oX + t, this.Height);

                    g.DrawString(t / z + "", this.Font, b, oX + t, this.Height - (oY + 20));
                }


                //Draw the left side
                for (float t = -(z * iy); t >= -this.Width - oX; t -= z * ix)
                {
                    g.DrawLine(gc, oX + t, this.Height - (oY + 10), oX + t, this.Height - (oY - 10));
                    g.DrawLine(gp, oX + t, -this.Height, oX + t, this.Height);

                    g.DrawString(t / z + "", this.Font, b, oX + t, this.Height - (oY + 20));
                }
            }

        }

        float[] cTp(float[] point)
        {
            float[] newPoint = new float[2];
            newPoint[0] = point[0] * zoom + offsetX;
            newPoint[1] = this.Height - (point[1] * zoom + offsetY);
            return newPoint;
        }

        private void rePaint()
        {
            //Clear the board, and call the onPaint method
            Rectangle r = new Rectangle(0, 0, this.Width, this.Height);
            this.Invalidate(r);
        }

        private void reText()
        {
            text = "";

            textX = -20;
            textY = -20;
            rePaint();
        }



        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //See what there cursor coordinates were the moment they clicked.
            mX = Cursor.Position.X - this.Location.X;
            mY = Cursor.Position.Y - this.Location.Y;

            mouseDown = true;
            rePaint();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            float cX = Cursor.Position.X - this.Location.X;
            float cY = Cursor.Position.Y - this.Location.Y;

            if (mouseDown == true)
            {
                rePaint();
                if (pointHeld != -1) return;
                offsetX = ooX - (mX - cX);
                offsetY = ooY + (mY - cY);
            }

            //If they are holding onto a function, don't drag the screen.

        }

        //When they move their mouse up
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //Wait a bit
            Thread.Sleep(50);
            //Tell the code they are not holding onto any function
            pointHeld = -1;
            //Tell the code their mouse is not down
            mouseDown = false;
            reText();

            ooX = offsetX;
            ooY = offsetY;
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Some code I made algebraically so that when the user zooms in, it zooms in onto the cursor and not the origin
            if (e.KeyChar == 'w')
            {
                zoom *= (float)2;

                offsetX -= ((Cursor.Position.X - this.Location.X) - offsetX);
                offsetY += ((Cursor.Position.Y - this.Location.Y - 20) - (this.Height - offsetY));

                ooX = offsetX;
                ooY = offsetY;
                reText();
                rePaint();
            }

            if (e.KeyChar == 's')
            {


                zoom *= (float)0.5f;

                offsetX += ((Cursor.Position.X - this.Location.X) - offsetX) / 2;
                offsetY -= ((Cursor.Position.Y - this.Location.Y - 20) - (this.Height - offsetY)) / 2;

                ooX = offsetX;
                ooY = offsetY;

                reText();
                rePaint();
            }

            //When they press R, it resets the zoom and makes the origin at the center
            if (e.KeyChar == 'r')
            {
                zoom = 100;

                offsetX = this.Width / 2;
                offsetY = this.Height / 2;

                ooX = offsetX;
                ooY = offsetY;

                reText();
                rePaint();
            }

        }

        //When the user resizes the screen, repaint everything
        private void Form1_Resize(object sender, EventArgs e)
        {
            rePaint();
        }

        //Some code I made that solves linear systems of equations.

        //If you have a lot of points (1,1) (3,1) (6,2) (7,2)
        //You put them into the toSystem, then take that and put it into the Guass function
        //Then into the solveGuass function, and what comes out are coefficients for a, b, c, d... for a polynomial.
        static float[,] toSystem(float[,] points)
        {
            float[,] system = new float[points.GetLength(0), points.GetLength(0) + 1];
            for (int a = 0; a < system.GetLength(0); a++)
            {
                for (int b = 0; b < system.GetLength(0); b++)
                {
                    system[a, b] = (float)Math.Pow(points[a, 0], b);
                }
                system[a, system.GetLength(0)] = points[a, 1];
            }
            return system;
        }

        static float[,] Guass(float[,] system)
        {

            for(int a=0; a<system.GetLength(0)-1; a++)
            {
                for(int b=a+1; b< system.GetLength(0); b++)
                {
                    float m = system[b, a] / system[a, a];
                    for (int c=0; c<system.GetLength(1); c++)
                    {
                        system[b, c] -= system[a, c] * m;
                    }
                }
            }

            return system;

        }

        static float[] solveGuass(float [,] system)
        {
            float[] solutions = new float[system.GetLength(0)];

            for(int a = system.GetLength(0)-1; a>=1; a--)
            {
                for(int b = a-1; b>=0; b--)
                {
                    system[b, system.GetLength(1)-1] -= (system[a, system.GetLength(1)-1] / system[a, a]) * system[b, a];
                }
            }

            for(int a = 0; a<system.GetLength(0); a++)
            {
                solutions[a] = system[a, system.GetLength(1)-1] / system[a, a];
            }

            return solutions;
        }
    }
}
