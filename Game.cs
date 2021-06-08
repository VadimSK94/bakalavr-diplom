using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UTTTWithGUI
{
    class Game
    {
        public Game(int player)//Конструктор, инициализирует поля
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    MicroGame[i, j] = BestMicroMove[i, j] = 0;
                }
                MacroGame[i] = 0;
                BestMacroMove[i] = 0;
            }
            userSquare = -1;
            StepsToFullView = 12;
            StepsToLeft = 81;
            maxDepth = 7;
            this.player = player;  
        }
        
        public int unfact(Int64 n)//Обратный факториал
        {
            Int64 count = 2;
            while (n != 1) n = n / count++;
            return Convert.ToInt32(count-1);
        }
        public int[] MacroGame = new int[9];//Большая доска, 0 - в игре, -1 - крестик, 1 - нолик, 2 - ничья
        public int[,] MicroGame = new int[9, 9];//Микро доска, 0 - свободна, -1 - крестик, 1 - нолик
        public int[,] BestMicroMove = new int[9,9];//Запоминание лучшего хода на микро доске
        public int[] BestMacroMove = new int[9];//Запоминание лучшего хода на макро доске
        public LinkedList<int> Evaluated = new LinkedList<int>();
        public int maxDepth;//Максимальная глубина просчета
        public int player;//Игрок, -1 - крестик, 1 - нолик
        public int StepsToLeft;//Ходов до окончания игры
        public int userSquare;//Номер поля, на которое следует сходить следующему игроку. В случае неограниченного хода принимает значение -1
        public int StepsToFullView;//Ходов до включения полного перебора
        public void Move(int[,] a, int[,] b)//Присваивает значения второго массива первому
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    b[i, j] = a[i, j];
                }
        }
        public void Move(int[] a, int[] b)
        {
            for (int j = 0; j < 9; j++)
            {
                b[j] = a[j];
            }
        }//Присваивает значения второго массива первому
        public void CountStepsToLeft()
        {
            StepsToLeft = 81;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (MacroGame[i] != 0)
                    {
                        StepsToLeft -= 9;
                        break;
                    }
                    else if (MicroGame[i, j] != 0) StepsToLeft -= 1;
                }
            }
        }//Высчитывает поле StepsToLeft
        private bool full(int nMG)
        {
            for (int i = 0; i < 9; i++)
			{
			    if(MicroGame[nMG, i]==0) return false;
			}
            return true;
        }//Проверяет доску на ничью
        public bool EndMicroGame(int Player,int nMG)
        {
            if ((MicroGame[nMG, 0] == Player) && (MicroGame[nMG, 1] == Player) && (MicroGame[nMG, 2] == Player)) return true;
            if ((MicroGame[nMG, 3] == Player) && (MicroGame[nMG, 4] == Player) && (MicroGame[nMG, 5] == Player)) return true;
            if ((MicroGame[nMG, 6] == Player) && (MicroGame[nMG, 7] == Player) && (MicroGame[nMG, 8] == Player)) return true;
            if ((MicroGame[nMG, 0] == Player) && (MicroGame[nMG, 3] == Player) && (MicroGame[nMG, 6] == Player)) return true;
            if ((MicroGame[nMG, 1] == Player) && (MicroGame[nMG, 4] == Player) && (MicroGame[nMG, 7] == Player)) return true;
            if ((MicroGame[nMG, 2] == Player) && (MicroGame[nMG, 5] == Player) && (MicroGame[nMG, 8] == Player)) return true;
            if ((MicroGame[nMG, 0] == Player) && (MicroGame[nMG, 4] == Player) && (MicroGame[nMG, 8] == Player)) return true;
            if ((MicroGame[nMG, 2] == Player) && (MicroGame[nMG, 4] == Player) && (MicroGame[nMG, 6] == Player)) return true;
            return false;
        }//Проверка на окончание игры на микро доске в пользу Player
        public bool EndMacroGame(int Player)
        {
            if ((MacroGame[0] == Player) & (MacroGame[1] == Player) & (MacroGame[2] == Player)) return true;
            if ((MacroGame[3] == Player) & (MacroGame[4] == Player) & (MacroGame[5] == Player)) return true;
            if ((MacroGame[6] == Player) & (MacroGame[7] == Player) & (MacroGame[8] == Player)) return true;
            if ((MacroGame[0] == Player) & (MacroGame[3] == Player) & (MacroGame[6] == Player)) return true;
            if ((MacroGame[1] == Player) & (MacroGame[4] == Player) & (MacroGame[7] == Player)) return true;
            if ((MacroGame[2] == Player) & (MacroGame[5] == Player) & (MacroGame[8] == Player)) return true;
            if ((MacroGame[0] == Player) & (MacroGame[4] == Player) & (MacroGame[8] == Player)) return true;
            if ((MacroGame[2] == Player) & (MacroGame[4] == Player) & (MacroGame[6] == Player)) return true;
            return false;
        }//Проверка на окончание игры на макро доске в пользу Player
        public int Search(int player, int alpha, int beta, int depth, int nMG)//Поиск хода
        {
            int tmp;//Временная переменная, сохранение оценки
            bool findMove = false;//Найден ли ход
            if (EndMacroGame(-player)) return -9990;//Проверяется, проиграли ли мы. Если да, то возвращается худшая оценка
            if (depth == 0) return evaluate(player) - evaluate(-player);//Когда добрались до дна дерева перебора, возвращаем оценку позиции
            if (nMG != -1) 
                if (MacroGame[nMG] == 0)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (MicroGame[nMG, i] == 0)
                        {
                            findMove = true;//Нашли ход
                            MicroGame[nMG, i] = player;//Сходили
                            if (EndMicroGame(player, nMG)) MacroGame[nMG] = player; //Если выиграли на микро доске, то отмечаем это
                            if (full(nMG)) MacroGame[nMG] = 2;//Если ничья на микро доске, то отмечаем это
                            tmp = -Search(-player, -beta, -alpha, depth - 1, i);//Вызвали рекурсивно то же самое, то бишь сходили противником из текущей позиции
                            if (tmp > alpha)
                            {
                                alpha = tmp;
                                if (depth == maxDepth)//Когда рекурсия свернулась, отмечаем лучшие ходы и поле, на которое следует ходить сопернику
                                {
                                    Move(MicroGame, BestMicroMove);
                                    Move(MacroGame, BestMacroMove);
                                    userSquare = MacroGame[i] == 0 ? i : -1;
                                }
                            }
                            MacroGame[nMG] = 0;
                            MicroGame[nMG, i] = 0;
                            if (alpha >= beta) break;
                        }
                    }
                }
            else
            {
                //Если компьютеру дать широкий выбор хода, то количество вариантов резко возрастает и приходится искусственно снижать глубино перебора
                for (int i = 0; i < 9; i++)
                {
                    if(MacroGame[i]==0)
                    for (int j = 0; j < 9; j++)
                    {
                        if (MicroGame[i, j] == 0)
                        {
                            findMove = true;//Нашли ход
                            MicroGame[i, j] = player;//Сходили
                            if (EndMicroGame(player, i)) MacroGame[i] = player; //Если выиграли на микро доске, то отмечаем это
                            if (full(i)) MacroGame[i] = 2;//Если ничья на микро доске, то отмечаем это
                            tmp = -Search(-player, -beta, -alpha, depth - 1, j);//Вызвали рекурсивно то же самое, то бишь сходили противником из текущей позиции
                            if (tmp > alpha)
                            {
                                alpha = tmp;
                                if (depth == maxDepth)//Когда рекурсия свернулась, отмечаем лучшие ходы и поле, на которое следует ходить сопернику
                                {
                                    Move(MicroGame, BestMicroMove);
                                    Move(MacroGame, BestMacroMove);
                                    userSquare = MacroGame[j] == 0 ? j : -1;
                                }
                            }
                            MacroGame[i] = 0;
                            MicroGame[i, j] = 0;
                            if (alpha >= beta) break;
                        }
                    }
                }   
			}
            if (findMove)
                return alpha;
            else return 0;
        }
        private int evaluateMicroBoard(int player, int nMG)
        {
            int score = 0;
            for (int i = 0; i < 9; i++)
            {
                if (MicroGame[nMG, i] == player)
                switch (i)
                {
                    case 0:
                        if ((MicroGame[nMG, 1] != -player) && (MicroGame[nMG, 2] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 3] != -player) && (MicroGame[nMG, 6] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 4] != -player) && (MicroGame[nMG, 8] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 1] == player) || (MicroGame[nMG, 2] == player))
                                score += 1;
                        if ((MicroGame[nMG, 3] == player) || (MicroGame[nMG, 6] == player))
                                score += 1;
                        if ((MicroGame[nMG, 4] == player) || (MicroGame[nMG, 8] == player))
                                score += 1;
                        break;
                    case 1:
                        if ((MicroGame[nMG, 0] != -player) && (MicroGame[nMG, 2] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 4] != -player) && (MicroGame[nMG, 7] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 0] == player) || (MicroGame[nMG, 2] == player))
                            score += 1;
                        if ((MicroGame[nMG, 4] == player) || (MicroGame[nMG, 7] == player))
                            score += 1;
                        break;
                    case 2:
                        if ((MicroGame[nMG, 0] != -player) && (MicroGame[nMG, 1] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 5] != -player) && (MicroGame[nMG, 8] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 4] != -player) && (MicroGame[nMG, 6] != -player))
                                score += 1;
                        
                        if ((MicroGame[nMG, 0] == player) || (MicroGame[nMG, 1] == player))
                                score += 1;
                        if ((MicroGame[nMG, 5] == player) || (MicroGame[nMG, 8] == player))
                                score += 1;
                        if ((MicroGame[nMG, 4] == player) || (MicroGame[nMG, 6] == player))
                                score += 1;
                        break;
                    case 3:
                        if ((MicroGame[nMG, 0] != -player) && (MicroGame[nMG, 6] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 4] != -player) && (MicroGame[nMG, 5] != -player))
                            score += 1;
                        
                        if ((MicroGame[nMG, 0] == player) || (MicroGame[nMG, 6] == player))
                            score += 1;
                        if ((MicroGame[nMG, 4] == player) || (MicroGame[nMG, 5] == player))
                            score += 1;
                        break;
                    case 4:
                        if ((MicroGame[nMG, 0] != -player) && (MicroGame[nMG, 8] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 1] != -player) && (MicroGame[nMG, 7] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 2] != -player) && (MicroGame[nMG, 6] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 3] != -player) && (MicroGame[nMG, 5] != -player))
                            score += 1;
                        
                        if ((MicroGame[nMG, 0] == player) || (MicroGame[nMG, 8] == player))
                            score += 1;
                        if ((MicroGame[nMG, 1] == player) || (MicroGame[nMG, 7] == player))
                            score += 1;
                        if ((MicroGame[nMG, 2] == player) || (MicroGame[nMG, 6] == player))
                            score += 1;
                        if ((MicroGame[nMG, 3] == player) || (MicroGame[nMG, 5] == player))
                            score += 1;
                        break;
                    case 5:
                        if ((MicroGame[nMG, 2] != -player) && (MicroGame[nMG, 8] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 4] != -player) && (MicroGame[nMG, 3] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 2] == player) || (MicroGame[nMG, 8] == player))
                            score += 1;
                        if ((MicroGame[nMG, 4] == player) || (MicroGame[nMG, 3] == player))
                            score += 1;
                        break;
                    case 6:
                        if ((MicroGame[nMG, 0] != -player) && (MicroGame[nMG, 3] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 4] != -player) && (MicroGame[nMG, 2] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 7] != -player) && (MicroGame[nMG, 8] != -player))
                                score += 1;
                        
                        if ((MicroGame[nMG, 0] == player) || (MicroGame[nMG, 3] == player))
                                score += 1;
                        if ((MicroGame[nMG, 4] == player) || (MicroGame[nMG, 2] == player))
                                score += 1;
                        if ((MicroGame[nMG, 7] == player) || (MicroGame[nMG, 8] == player))
                                score += 1;

                        break;
                    case 7:
                        if ((MicroGame[nMG, 1] != -player) && (MicroGame[nMG, 4] != -player))
                            score += 1;
                        if ((MicroGame[nMG, 6] != -player) && (MicroGame[nMG, 8] != -player))
                            score += 1;
                        
                        if ((MicroGame[nMG, 1] == player) || (MicroGame[nMG, 4] == player))
                            score += 1;
                        if ((MicroGame[nMG, 6] == player) || (MicroGame[nMG, 8] == player))
                            score += 1;
                        break;
                    case 8:
                        if ((MicroGame[nMG, 0] != -player) && (MicroGame[nMG, 4] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 5] != -player) && (MicroGame[nMG, 2] != -player))
                                score += 1;
                        if ((MicroGame[nMG, 7] != -player) && (MicroGame[nMG, 6] != -player))
                                score += 1;
                        
                        if ((MicroGame[nMG, 0] == player) || (MicroGame[nMG, 4] == player))
                                score += 1;
                        if ((MicroGame[nMG, 5] == player) || (MicroGame[nMG, 2] == player))
                                score += 1;
                        if ((MicroGame[nMG, 7] == player) || (MicroGame[nMG, 6] == player))
                                score += 1;
                        break;
                }
            }
                return score;
        }//Оценка мини доски для игрока player
        private int evaluateMacroBoard(int player, int nMG)//Оценка макро доски для игрока player
        {
            int score = 0;
                    switch (nMG)
                    {
                        case 0:
                            if ((MacroGame[ 1] != -player) && (MacroGame[ 2] != -player))
                                score += 1;
                            if ((MacroGame[ 3] != -player) && (MacroGame[ 6] != -player))
                                score += 1;
                            if ((MacroGame[ 4] != -player) && (MacroGame[ 8] != -player))
                                score += 1;
                            if ((MacroGame[ 1] == player) || (MacroGame[ 2] == player))
                                score += 1;
                            if ((MacroGame[ 3] == player) || (MacroGame[ 6] == player))
                                score += 1;
                            if ((MacroGame[ 4] == player) || (MacroGame[ 8] == player))
                                score += 1;
                            break;
                        case 1:
                            if ((MacroGame[ 0] != -player) && (MacroGame[ 2] != -player))
                                score += 1;
                            if ((MacroGame[ 4] != -player) && (MacroGame[ 7] != -player))
                                score += 1;
                            if ((MacroGame[ 0] == player) || (MacroGame[ 2] == player))
                                score += 1;
                            if ((MacroGame[ 4] == player) || (MacroGame[ 7] == player))
                                score += 1;
                            break;
                        case 2:
                            if ((MacroGame[ 0] != -player) && (MacroGame[ 1] != -player))
                                score += 1;
                            if ((MacroGame[ 5] != -player) && (MacroGame[ 8] != -player))
                                score += 1;
                            if ((MacroGame[ 4] != -player) && (MacroGame[ 6] != -player))
                                score += 1;
                             if ((MacroGame[ 0] == player) || (MacroGame[ 1] == player))
                                score += 1;
                            if ((MacroGame[ 5] == player) || (MacroGame[ 8] == player))
                                score += 1;
                            if ((MacroGame[ 4] == player) || (MacroGame[ 6] == player))
                                score += 1;
                            break;
                        case 3:
                            if ((MacroGame[ 0] != -player) && (MacroGame[ 6] != -player))
                                score += 1;
                            if ((MacroGame[ 4] != -player) && (MacroGame[ 5] != -player))
                                score += 1;
                            if ((MacroGame[ 0] == player) || (MacroGame[ 6] == player))
                                score += 1;
                            if ((MacroGame[ 4] == player) || (MacroGame[ 5] == player))
                                score += 1;
                            break;
                        case 4:
                            if ((MacroGame[ 0] != -player) && (MacroGame[ 8] != -player))
                                score += 1;
                            if ((MacroGame[ 1] != -player) && (MacroGame[ 7] != -player))
                                score += 1;
                            if ((MacroGame[ 2] != -player) && (MacroGame[ 6] != -player))
                                score += 1;
                            if ((MacroGame[ 3] != -player) && (MacroGame[ 5] != -player))
                                score += 1;
                            if ((MacroGame[ 0] == player) || (MacroGame[ 8] == player))
                                score += 1;
                            if ((MacroGame[ 1] == player) || (MacroGame[ 7] == player))
                                score += 1;
                            if ((MacroGame[ 2] == player) || (MacroGame[ 6] == player))
                                score += 1;
                            if ((MacroGame[ 3] == player) || (MacroGame[ 5] == player))
                                score += 1;
                            break;
                        case 5:
                            if ((MacroGame[ 2] != -player) && (MacroGame[ 8] != -player))
                                score += 1;
                            if ((MacroGame[ 4] != -player) && (MacroGame[ 3] != -player))
                                score += 1;
                            if ((MacroGame[ 2] == player) || (MacroGame[ 8] == player))
                                score += 1;
                            if ((MacroGame[ 4] == player) || (MacroGame[ 3] == player))
                                score += 1;
                            break;
                        case 6:
                            if ((MacroGame[ 0] != -player) && (MacroGame[ 3] != -player))
                                score += 1;
                            if ((MacroGame[ 4] != -player) && (MacroGame[ 2] != -player))
                                score += 1;
                            if ((MacroGame[ 7] != -player) && (MacroGame[ 8] != -player))
                                score += 1;
                            if ((MacroGame[ 0] == player) || (MacroGame[ 3] == player))
                                score += 1;
                            if ((MacroGame[ 4] == player) || (MacroGame[ 2] == player))
                                score += 1;
                            if ((MacroGame[ 7] == player) || (MacroGame[ 8] == player))
                                score += 1;
                            break;
                        case 7:
                            if ((MacroGame[ 1] != -player) && (MacroGame[ 4] != -player))
                                score += 1;
                            if ((MacroGame[ 6] != -player) && (MacroGame[ 8] != -player))
                                score += 1;
                            
                            if ((MacroGame[ 1] == player) || (MacroGame[ 4] == player))
                                score += 1;
                            if ((MacroGame[ 6] == player) || (MacroGame[ 8] == player))
                                score += 1;
                            break;
                        case 8:
                            if ((MacroGame[ 0] != -player) && (MacroGame[ 4] != -player))
                                score += 1;
                            if ((MacroGame[ 5] != -player) && (MacroGame[ 2] != -player))
                                score += 1;
                            if ((MacroGame[ 7] != -player) && (MacroGame[ 6] != -player))
                                score += 1;
                            if ((MacroGame[ 0] == player) || (MacroGame[ 4] == player))
                                score += 1;
                            if ((MacroGame[ 5] == player) || (MacroGame[ 2] == player))
                                score += 1;
                            if ((MacroGame[ 7] == player) || (MacroGame[ 6] == player))
                                score += 1;
                            break;
            }
            return score;
        }
        public int evaluate(int player)//Оценочная функция
        {
            int score=0;
            for (int i = 0; i < 9; i++)
            {
                if (MacroGame[i] == 0)
                    score += evaluateMicroBoard(player, i) * evaluateMacroBoard(player, i);
                if (MacroGame[i] == player)
                    score += evaluateMacroBoard(player, i) * 50;
            }
            return score;
        }
        public bool GameOver()
        {
            if (!(EndMacroGame(1) || EndMacroGame(-1)))
            {
                for (int i = 0; i < 9; i++)
                {
                    if (MacroGame[i] == 0) return false;
                }
            }
            return true;
        }//Функция проверки окончания игры
    }
}