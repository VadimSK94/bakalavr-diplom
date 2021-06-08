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

namespace UTTTWithGUI
{
    public partial class Gameplay : Form
    {
        private Graphics g;
        Game game;
        int player = -1;
        public Gameplay()
        {
            InitializeComponent();
            //game = new Game();
        }
        void NewGame()
        {
            StartGame dialog = new StartGame();
            dialog.ShowDialog();
            player = dialog.player;
            if (dialog.player==1)
            {
                game = new Game(1);
                clearField();
                game.Search(-1, -9999, 9999, game.maxDepth, 0);
                game.Move(game.BestMicroMove, game.MicroGame);
            }
            else if (dialog.player == -1)
            {
                game = new Game(-1);
                clearField();
            }
        }     
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            coord coord = transformFromCoord(e.X, e.Y);//Переводим координаты панели в номер клетки
            
            if((((game.userSquare==-1)||((game.userSquare==coord.X)&&(game.MacroGame[game.userSquare]==0)))&&(game.MicroGame[coord.X,coord.Y]==0)))
            {
                game.MicroGame[coord.X, coord.Y] = player;//Сходили
                
                if (game.EndMicroGame(player, coord.X))
                {
                    game.MacroGame[coord.X] = player;
                    game.StepsToLeft -= 9;
                }
                else game.StepsToLeft -= 1;
                if (!game.GameOver())
                {
                    if (game.StepsToLeft > game.StepsToFullView)//Если размер поля приблизился к размеру, обрабатываемому полным перебором, то включаем его
                    {
                        label1.Text = game.StepsToLeft.ToString();
                    }
                    else
                    {
                        game.maxDepth = game.StepsToFullView;
                        label1.Text = "supermode";
                    }
                    game.Search(-player, -9999, 9999, game.maxDepth, coord.Y);
                    game.Move(game.BestMicroMove, game.MicroGame);
                    game.Move(game.BestMacroMove, game.MacroGame);
                    game.Evaluated.AddFirst(game.evaluate(-player) - game.evaluate(player));
                    game.CountStepsToLeft();
                    field.Invalidate();
                }   
                else
                {
                    if (game.EndMacroGame(1)) MessageBox.Show("Нолики выиграли", "Game over");
                    if (game.EndMacroGame(-1)) MessageBox.Show("Крестики выиграли", "Game over");
                    clearField();
                }
            }
        }
    
        private void onPaint(object sender, PaintEventArgs e)
        {
            g = e.Graphics;
            showGame(g);
        }
        void drawCross(Graphics g, int X, int Y)
        {
            Pen cross = new Pen(Color.Red, 3);
            g.DrawLine(cross, X, Y, X + 70, Y + 70);
            g.DrawLine(cross, X + 70, Y, X, Y + 70);
        }//Рисует крестик по координатам левого верхнего угла
        void drawNull(Graphics g, int X, int Y)
        {
            Pen nul = new Pen(Color.Blue, 3);
            g.DrawEllipse(nul, X, Y, 70, 70);
        }//Рисует нолик по координатам левого верхнего угла
        void drawSign(Graphics g, int X, int Y, int sign)
        {
            if (sign == 1) drawNull(g, X, Y);
            if (sign == -1) drawCross(g, X, Y);
        }//Рисует знак
        void drawField(Graphics g)
        {
            Pen cells = new Pen(Color.Black, 5);
            g.DrawLine(cells, 230, 10, 230, 680);
            g.DrawLine(cells, 460, 10, 460, 680);
            g.DrawLine(cells, 10, 230, 680, 230);
            g.DrawLine(cells, 10, 460, 680, 460);
            cells.Width = 3;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    g.DrawLine(cells, 80 + 230 * i, 10 + 230 * j, 80 + 230 * i, 220 + 230 * j);
                    g.DrawLine(cells, 150 + 230 * i, 10 + 230 * j, 150 + 230 * i, 220 + 230 * j);
                    g.DrawLine(cells, 10 + 230 * i, 80 + 230 * j, 220 + 230 * i, 80 + 230 * j);
                    g.DrawLine(cells, 10 + 230 * i, 150 + 230 * j, 220 + 230 * i, 150 + 230 * j);
                }
            }
        }//Рисует поле
        void drawMacroSign(Graphics g, int nMG, int player)
        {
            coord coord;
            coord = transformFromNumber(nMG, 0);
            if (player == 1)
            {
                Pen nul = new Pen(Color.Blue, 5);
                g.DrawEllipse(nul, coord.X, coord.Y, 210, 210);
            }
            if (player == -1)
            {
                Pen cross = new Pen(Color.Red, 5);
                g.DrawLine(cross, coord.X, coord.Y, coord.X + 210, coord.Y + 210);
                g.DrawLine(cross, coord.X + 210, coord.Y, coord.X, coord.Y + 210);
            }
        }//Рисует знак на большой доске
        void drawLimitation(Graphics g, int nMG)
        {
            Pen lim = new Pen(Color.Green, 3);
            coord coord = transformFromNumber(nMG, 0);
            g.DrawRectangle(lim, coord.X, coord.Y, 210, 210);
        }//Рисует ограничивающий квадрат
        coord transformFromCoord(int X, int Y)
        {
            coord result;
            int numI = 0, numJ = 0;
            if (((X > 10) && (X < 80)) && ((Y > 10) && (Y < 80))) { numI = 0; numJ = 0; }
            if (((X > 80) && (X < 150)) && ((Y > 10) && (Y < 70))) { numI = 0; numJ = 1; }
            if (((X > 150) && (X < 220)) && ((Y > 10) && (Y < 70))) { numI = 0; numJ = 2; }
            if (((X > 240) && (X < 310)) && ((Y > 10) && (Y < 70))) { numI = 1; numJ = 0; }
            if (((X > 310) && (X < 380)) && ((Y > 10) && (Y < 70))) { numI = 1; numJ = 1; }
            if (((X > 380) && (X < 450)) && ((Y > 10) && (Y < 70))) { numI = 1; numJ = 2; }
            if (((X > 470) && (X < 540)) && ((Y > 10) && (Y < 70))) { numI = 2; numJ = 0; }
            if (((X > 540) && (X < 610)) && ((Y > 10) && (Y < 70))) { numI = 2; numJ = 1; }
            if (((X > 610) && (X < 680)) && ((Y > 10) && (Y < 70))) { numI = 2; numJ = 2; }

            if (((X > 10) && (X < 80)) && ((Y > 80) && (Y < 150))) { numI = 0; numJ = 3; }
            if (((X > 80) && (X < 150)) && ((Y > 80) && (Y < 150))) { numI = 0; numJ = 4; }
            if (((X > 150) && (X < 220)) && ((Y > 80) && (Y < 150))) { numI = 0; numJ = 5; }
            if (((X > 240) && (X < 310)) && ((Y > 80) && (Y < 150))) { numI = 1; numJ = 3; }
            if (((X > 310) && (X < 380)) && ((Y > 80) && (Y < 150))) { numI = 1; numJ = 4; }
            if (((X > 380) && (X < 450)) && ((Y > 80) && (Y < 150))) { numI = 1; numJ = 5; }
            if (((X > 470) && (X < 540)) && ((Y > 80) && (Y < 150))) { numI = 2; numJ = 3; }
            if (((X > 540) && (X < 610)) && ((Y > 80) && (Y < 150))) { numI = 2; numJ = 4; }
            if (((X > 610) && (X < 680)) && ((Y > 80) && (Y < 150))) { numI = 2; numJ = 5; }

            if (((X > 10) && (X < 80)) && ((Y > 150) && (Y < 220))) { numI = 0; numJ = 6; }
            if (((X > 80) && (X < 150)) && ((Y > 150) && (Y < 220))) { numI = 0; numJ = 7; }
            if (((X > 150) && (X < 220)) && ((Y > 150) && (Y < 220))) { numI = 0; numJ = 8; }
            if (((X > 240) && (X < 310)) && ((Y > 150) && (Y < 220))) { numI = 1; numJ = 6; }
            if (((X > 310) && (X < 380)) && ((Y > 150) && (Y < 220))) { numI = 1; numJ = 7; }
            if (((X > 380) && (X < 450)) && ((Y > 150) && (Y < 220))) { numI = 1; numJ = 8; }
            if (((X > 470) && (X < 540)) && ((Y > 150) && (Y < 220))) { numI = 2; numJ = 6; }
            if (((X > 540) && (X < 610)) && ((Y > 150) && (Y < 220))) { numI = 2; numJ = 7; }
            if (((X > 610) && (X < 680)) && ((Y > 150) && (Y < 220))) { numI = 2; numJ = 8; }

            if (((X > 10) && (X < 80)) && ((Y > 240) && (Y < 310))) { numI = 3; numJ = 0; }
            if (((X > 80) && (X < 150)) && ((Y > 240) && (Y < 310))) { numI = 3; numJ = 1; }
            if (((X > 150) && (X < 220)) && ((Y > 240) && (Y < 310))) { numI = 3; numJ = 2; }
            if (((X > 240) && (X < 310)) && ((Y > 240) && (Y < 310))) { numI = 4; numJ = 0; }
            if (((X > 310) && (X < 380)) && ((Y > 240) && (Y < 310))) { numI = 4; numJ = 1; }
            if (((X > 380) && (X < 450)) && ((Y > 240) && (Y < 310))) { numI = 4; numJ = 2; }
            if (((X > 470) && (X < 540)) && ((Y > 240) && (Y < 310))) { numI = 5; numJ = 0; }
            if (((X > 540) && (X < 610)) && ((Y > 240) && (Y < 310))) { numI = 5; numJ = 1; }
            if (((X > 610) && (X < 680)) && ((Y > 240) && (Y < 310))) { numI = 5; numJ = 2; }

            if (((X > 10) && (X < 80)) && ((Y > 310) && (Y < 380))) { numI = 3; numJ = 3; }
            if (((X > 80) && (X < 150)) && ((Y > 310) && (Y < 380))) { numI = 3; numJ = 4; }
            if (((X > 150) && (X < 220)) && ((Y > 310) && (Y < 380))) { numI = 3; numJ = 5; }
            if (((X > 240) && (X < 310)) && ((Y > 310) && (Y < 380))) { numI = 4; numJ = 3; }
            if (((X > 310) && (X < 380)) && ((Y > 310) && (Y < 380))) { numI = 4; numJ = 4; }
            if (((X > 380) && (X < 450)) && ((Y > 310) && (Y < 380))) { numI = 4; numJ = 5; }
            if (((X > 470) && (X < 540)) && ((Y > 310) && (Y < 380))) { numI = 5; numJ = 3; }
            if (((X > 540) && (X < 610)) && ((Y > 310) && (Y < 380))) { numI = 5; numJ = 4; }
            if (((X > 610) && (X < 680)) && ((Y > 310) && (Y < 380))) { numI = 5; numJ = 5; }

            if (((X > 10) && (X < 80)) && ((Y > 380) && (Y < 450))) { numI = 3; numJ = 6; }
            if (((X > 80) && (X < 150)) && ((Y > 380) && (Y < 450))) { numI = 3; numJ = 7; }
            if (((X > 150) && (X < 220)) && ((Y > 380) && (Y < 450))) { numI = 3; numJ = 8; }
            if (((X > 240) && (X < 310)) && ((Y > 380) && (Y < 450))) { numI = 4; numJ = 6; }
            if (((X > 310) && (X < 380)) && ((Y > 380) && (Y < 450))) { numI = 4; numJ = 7; }
            if (((X > 380) && (X < 450)) && ((Y > 380) && (Y < 450))) { numI = 4; numJ = 8; }
            if (((X > 470) && (X < 540)) && ((Y > 380) && (Y < 450))) { numI = 5; numJ = 6; }
            if (((X > 540) && (X < 610)) && ((Y > 380) && (Y < 450))) { numI = 5; numJ = 7; }
            if (((X > 610) && (X < 680)) && ((Y > 380) && (Y < 450))) { numI = 5; numJ = 8; }

            if (((X > 10) && (X < 80)) && ((Y > 470) && (Y < 540))) { numI = 6; numJ = 0; }
            if (((X > 80) && (X < 150)) && ((Y > 470) && (Y < 540))) { numI = 6; numJ = 1; }
            if (((X > 150) && (X < 220)) && ((Y > 470) && (Y < 540))) { numI = 6; numJ = 2; }
            if (((X > 240) && (X < 310)) && ((Y > 470) && (Y < 540))) { numI = 7; numJ = 0; }
            if (((X > 310) && (X < 380)) && ((Y > 470) && (Y < 540))) { numI = 7; numJ = 1; }
            if (((X > 380) && (X < 450)) && ((Y > 470) && (Y < 540))) { numI = 7; numJ = 2; }
            if (((X > 470) && (X < 540)) && ((Y > 470) && (Y < 540))) { numI = 8; numJ = 0; }
            if (((X > 540) && (X < 610)) && ((Y > 470) && (Y < 540))) { numI = 8; numJ = 1; }
            if (((X > 610) && (X < 680)) && ((Y > 470) && (Y < 540))) { numI = 8; numJ = 2; }

            if (((X > 10) && (X < 80)) && ((Y > 540) && (Y < 610))) { numI = 6; numJ = 3; }
            if (((X > 80) && (X < 150)) && ((Y > 540) && (Y < 610))) { numI = 6; numJ = 4; }
            if (((X > 150) && (X < 220)) && ((Y > 540) && (Y < 610))) { numI = 6; numJ = 5; }
            if (((X > 240) && (X < 310)) && ((Y > 540) && (Y < 610))) { numI = 7; numJ = 3; }
            if (((X > 310) && (X < 380)) && ((Y > 540) && (Y < 610))) { numI = 7; numJ = 4; }
            if (((X > 380) && (X < 450)) && ((Y > 540) && (Y < 610))) { numI = 7; numJ = 5; }
            if (((X > 470) && (X < 540)) && ((Y > 540) && (Y < 610))) { numI = 8; numJ = 3; }
            if (((X > 540) && (X < 610)) && ((Y > 540) && (Y < 610))) { numI = 8; numJ = 4; }
            if (((X > 610) && (X < 680)) && ((Y > 540) && (Y < 610))) { numI = 8; numJ = 5; }

            if (((X > 10) && (X < 80)) && ((Y > 610) && (Y < 680))) { numI = 6; numJ = 6; }
            if (((X > 80) && (X < 150)) && ((Y > 610) && (Y < 680))) { numI = 6; numJ = 7; }
            if (((X > 150) && (X < 220)) && ((Y > 610) && (Y < 680))) { numI = 6; numJ = 8; }
            if (((X > 240) && (X < 310)) && ((Y > 610) && (Y < 680))) { numI = 7; numJ = 6; }
            if (((X > 310) && (X < 380)) && ((Y > 610) && (Y < 680))) { numI = 7; numJ = 7; }
            if (((X > 380) && (X < 450)) && ((Y > 610) && (Y < 680))) { numI = 7; numJ = 8; }
            if (((X > 470) && (X < 540)) && ((Y > 610) && (Y < 680))) { numI = 8; numJ = 6; }
            if (((X > 540) && (X < 610)) && ((Y > 610) && (Y < 680))) { numI = 8; numJ = 7; }
            if (((X > 610) && (X < 680)) && ((Y > 610) && (Y < 680))) { numI = 8; numJ = 8; }
            result.X = numI; result.Y = numJ;
            return result;
        }//Переводит координаты поля в номер квадрата
        coord transformFromNumber(int i, int j)
        {
            int numX = 0, numY = 0;
            coord result;
            if ((i == 0) && (j == 0)) { numX = 10; numY = 10; }
            if ((i == 0) && (j == 1)) { numX = 80; numY = 10; }
            if ((i == 0) && (j == 2)) { numX = 150; numY = 10; }
            if ((i == 1) && (j == 0)) { numX = 240; numY = 10; }
            if ((i == 1) && (j == 1)) { numX = 310; numY = 10; }
            if ((i == 1) && (j == 2)) { numX = 380; numY = 10; }
            if ((i == 2) && (j == 0)) { numX = 470; numY = 10; }
            if ((i == 2) && (j == 1)) { numX = 540; numY = 10; }
            if ((i == 2) && (j == 2)) { numX = 610; numY = 10; }

            if ((i == 0) && (j == 3)) { numX = 10; numY = 80; }
            if ((i == 0) && (j == 4)) { numX = 80; numY = 80; }
            if ((i == 0) && (j == 5)) { numX = 150; numY = 80; }
            if ((i == 1) && (j == 3)) { numX = 240; numY = 80; }
            if ((i == 1) && (j == 4)) { numX = 310; numY = 80; }
            if ((i == 1) && (j == 5)) { numX = 380; numY = 80; }
            if ((i == 2) && (j == 3)) { numX = 470; numY = 80; }
            if ((i == 2) && (j == 4)) { numX = 540; numY = 80; }
            if ((i == 2) && (j == 5)) { numX = 610; numY = 80; }

            if ((i == 0) && (j == 6)) { numX = 10; numY = 150; }
            if ((i == 0) && (j == 7)) { numX = 80; numY = 150; }
            if ((i == 0) && (j == 8)) { numX = 150; numY = 150; }
            if ((i == 1) && (j == 6)) { numX = 240; numY = 150; }
            if ((i == 1) && (j == 7)) { numX = 310; numY = 150; }
            if ((i == 1) && (j == 8)) { numX = 380; numY = 150; }
            if ((i == 2) && (j == 6)) { numX = 470; numY = 150; }
            if ((i == 2) && (j == 7)) { numX = 540; numY = 150; }
            if ((i == 2) && (j == 8)) { numX = 610; numY = 150; }

            if ((i == 3) && (j == 0)) { numX = 10; numY = 240; }
            if ((i == 3) && (j == 1)) { numX = 80; numY = 240; }
            if ((i == 3) && (j == 2)) { numX = 150; numY = 240; }
            if ((i == 4) && (j == 0)) { numX = 240; numY = 240; }
            if ((i == 4) && (j == 1)) { numX = 310; numY = 240; }
            if ((i == 4) && (j == 2)) { numX = 380; numY = 240; }
            if ((i == 5) && (j == 0)) { numX = 470; numY = 240; }
            if ((i == 5) && (j == 1)) { numX = 540; numY = 240; }
            if ((i == 5) && (j == 2)) { numX = 610; numY = 240; }

            if ((i == 3) && (j == 3)) { numX = 10; numY = 310; }
            if ((i == 3) && (j == 4)) { numX = 80; numY = 310; }
            if ((i == 3) && (j == 5)) { numX = 150; numY = 310; }
            if ((i == 4) && (j == 3)) { numX = 240; numY = 310; }
            if ((i == 4) && (j == 4)) { numX = 310; numY = 310; }
            if ((i == 4) && (j == 5)) { numX = 380; numY = 310; }
            if ((i == 5) && (j == 3)) { numX = 470; numY = 310; }
            if ((i == 5) && (j == 4)) { numX = 540; numY = 310; }
            if ((i == 5) && (j == 5)) { numX = 610; numY = 310; }

            if ((i == 3) && (j == 6)) { numX = 10; numY = 380; }
            if ((i == 3) && (j == 7)) { numX = 80; numY = 380; }
            if ((i == 3) && (j == 8)) { numX = 150; numY = 380; }
            if ((i == 4) && (j == 6)) { numX = 240; numY = 380; }
            if ((i == 4) && (j == 7)) { numX = 310; numY = 380; }
            if ((i == 4) && (j == 8)) { numX = 380; numY = 380; }
            if ((i == 5) && (j == 6)) { numX = 470; numY = 380; }
            if ((i == 5) && (j == 7)) { numX = 540; numY = 380; }
            if ((i == 5) && (j == 8)) { numX = 610; numY = 380; }

            if ((i == 6) && (j == 0)) { numX = 10; numY = 470; }
            if ((i == 6) && (j == 1)) { numX = 80; numY = 470; }
            if ((i == 6) && (j == 2)) { numX = 150; numY = 470; }
            if ((i == 7) && (j == 0)) { numX = 240; numY = 470; }
            if ((i == 7) && (j == 1)) { numX = 310; numY = 470; }
            if ((i == 7) && (j == 2)) { numX = 380; numY = 470; }
            if ((i == 8) && (j == 0)) { numX = 470; numY = 470; }
            if ((i == 8) && (j == 1)) { numX = 540; numY = 470; }
            if ((i == 8) && (j == 2)) { numX = 610; numY = 470; }

            if ((i == 6) && (j == 3)) { numX = 10; numY = 540; }
            if ((i == 6) && (j == 4)) { numX = 80; numY = 540; }
            if ((i == 6) && (j == 5)) { numX = 150; numY = 540; }
            if ((i == 7) && (j == 3)) { numX = 240; numY = 540; }
            if ((i == 7) && (j == 4)) { numX = 310; numY = 540; }
            if ((i == 7) && (j == 5)) { numX = 380; numY = 540; }
            if ((i == 8) && (j == 3)) { numX = 470; numY = 540; }
            if ((i == 8) && (j == 4)) { numX = 540; numY = 540; }
            if ((i == 8) && (j == 5)) { numX = 610; numY = 540; }

            if ((i == 6) && (j == 6)) { numX = 10; numY = 610; }
            if ((i == 6) && (j == 7)) { numX = 80; numY = 610; }
            if ((i == 6) && (j == 8)) { numX = 150; numY = 610; }
            if ((i == 7) && (j == 6)) { numX = 240; numY = 610; }
            if ((i == 7) && (j == 7)) { numX = 310; numY = 610; }
            if ((i == 7) && (j == 8)) { numX = 380; numY = 610; }
            if ((i == 8) && (j == 6)) { numX = 470; numY = 610; }
            if ((i == 8) && (j == 7)) { numX = 540; numY = 610; }
            if ((i == 8) && (j == 8)) { numX = 610; numY = 610; }
            result.X = numX; result.Y = numY;
            return result;
        }//Переводит номер квадрата в координаты поля
        void showGame(Graphics g)
        {
            coord coord;
            drawField(g);
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    coord = transformFromNumber(i, j);
                    drawSign(g, coord.X, coord.Y, game.MicroGame[i, j]);
                }
                if (game.EndMicroGame(1, i)) drawMacroSign(g, i, 1);
                if (game.EndMicroGame(-1, i)) drawMacroSign(g, i, -1);
                if (game.userSquare != -1) drawLimitation(g, game.userSquare);
            }
        }
        private void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
        }
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        void clearField()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    game.MicroGame[i, j] = 0;
                }
                game.MacroGame[i] = 0;
            }
            game.userSquare = -1;
            field.Invalidate();
        }
       
        private void загрузитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader sr = new
                   StreamReader(openFileDialog1.FileName);
                string line=sr.ReadLine();
                string[] numbers = line.Split(' ');
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        game.MicroGame[i, j] = Convert.ToInt32(numbers[i * 9 + j]);
                    }
                    game.MacroGame[i] = Convert.ToInt32(numbers[81 + i]);
                }
                game.userSquare = Convert.ToInt32(numbers[90]);
                game.StepsToLeft = Convert.ToInt32(numbers[91]);
                field.Invalidate();

            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = "C:\\UTTT";
            saveFileDialog1.Filter = "utt files (*.utt)|*.utt|All files|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fileData="";
                    for (int i = 0; i < 9; i++)
			        {
                        for (int j = 0; j < 9; j++)
                        {
                            fileData += game.MicroGame[i, j].ToString() + " ";
                        }
			        }
                    for (int i = 0; i < 9; i++)
                    {
                        fileData += game.MacroGame[i].ToString() + " ";
                    }
                    fileData += game.userSquare + " ";
                    fileData += game.StepsToLeft;
                    string fileName = saveFileDialog1.FileName;
                    System.IO.File.WriteAllText(fileName, fileData);
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NewGame();
        }

        private void показатьПараметрыИгрыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Options option = new Options(game.maxDepth, game.StepsToFullView, game.MicroGame, game.MacroGame, game.userSquare); 
            option.ShowDialog();
            game.maxDepth = option.MaxDepth;
            game.StepsToFullView = option.StepsToFullView;
            option.Hide();
        }

        private void показатьГрафикРостаОценочнойФункцииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shedule shedule = new Shedule(game.Evaluated);
            shedule.ShowDialog();
        }

        private void сходитьЗаМеняToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (game.userSquare == -1)//Вызов Search с номером макро доски -1 вызовет ошибку. Искусственно заставим игру выбрать доску другим образом
                for (int i = 0; i < 9; i++)
                {
                    if (game.MacroGame[i] != 0)
                    {
                        game.Search(player, -9999, 9999, game.maxDepth, i);
                        game.Move(game.BestMicroMove, game.MicroGame);
                        game.Move(game.BestMacroMove, game.MacroGame);
                        game.Evaluated.AddFirst(game.evaluate(-player) - game.evaluate(player));
                        game.CountStepsToLeft(); break;
                    }
                }
            else
            {
                game.Search(player, -9999, 9999, game.maxDepth, game.userSquare);
                game.Move(game.BestMicroMove, game.MicroGame);
                game.Move(game.BestMacroMove, game.MacroGame);
                game.Evaluated.AddFirst(game.evaluate(-player) - game.evaluate(player));
                game.CountStepsToLeft();
            }
            if (game.userSquare == -1)//Вызов Search с номером макро доски -1 вызовет ошибку. Искусственно заставим игру выбрать доску другим образом
                for (int i = 0; i < 9; i++)
                {
                    if (game.MacroGame[i] != 0)
                    {
                        game.Search(-player, -9999, 9999, game.maxDepth, i);
                        game.Move(game.BestMicroMove, game.MicroGame);
                        game.Move(game.BestMacroMove, game.MacroGame);
                        game.Evaluated.AddFirst(game.evaluate(-player) - game.evaluate(player));
                        game.CountStepsToLeft(); break;
                    }
                }
            else
            {
                game.Search(-player, -9999, 9999, game.maxDepth, game.userSquare);
                game.Move(game.BestMicroMove, game.MicroGame);
                game.Move(game.BestMacroMove, game.MacroGame);
                game.Evaluated.AddFirst(game.evaluate(-player) - game.evaluate(player));
                game.CountStepsToLeft();
            }
            field.Invalidate();
        }
    }
}