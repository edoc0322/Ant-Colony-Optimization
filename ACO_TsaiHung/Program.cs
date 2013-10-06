using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using Metaheuristic;

namespace ACO_TsaiHung
{
    class Program
    {
        static void Main(string[] args)
        {
            int runLength = 10, FEC = 1000;
          
            //宣告要將結果存入 tsp.txt 檔案裡

            TSP ACOtsp = new TSP();

            //寫入檔案
            StreamWriter sw = new StreamWriter("test.csv");
            sw.WriteLine("case, best Value, Average, STD, average time");
            //馬錶
            System.Diagnostics.Stopwatch swt = new System.Diagnostics.Stopwatch();

            //ACObest 最好的值(越小越好);best 十次的值; 平均十次的值(/10); 標準差(best) ;平均時間(time / 10)
            double[] best = new double[10];

            for (int i = 0; i < ACOtsp.problem.Length; i++)
            {
                double bestsum = 0.0;

                double ACObest = double.MaxValue;
                


                    for (int k = 0; k < runLength; k++)
                    {
                        swt.Reset();
                        swt.Start();

                        ACOtsp.initDistance(i);                      //問題初始

                        ACOtsp.fitness=0;

                        //方法初始化
                        ACOtsp.Init(40, ACOtsp.distanceLength, 0, ACOtsp.distanceLength - 1/*,
                        ACOOption.RepeatableOption.Nonrepeatable, ACOOption.CycleOption.Cycle*/);
                        ACOtsp.Run(FEC);
                        // Console.WriteLine(ACOtsp.GbestFitness);

                        best[k] = ACOtsp.GBestFitness;
                        bestsum += best[k];

                        if (best[k] < ACObest)
                        {
                            ACObest = best[k];

                        }
                        //Console.WriteLine("fitness=" + ACOtsp.fitness);
                        //Console.WriteLine("fitness=" + fitness);
                        Console.WriteLine("ACObest = " + ACOtsp.GBestFitness);
                    }
                //Console.Read();
                swt.Stop();
                //碼錶出來的時間是毫秒，要轉成秒，除以1000。
                double time = (double)swt.Elapsed.TotalMilliseconds / 1000;
                Console.WriteLine(ACOtsp.problem[i] + ", " + ACObest + "," + bestsum / 10 + "," + std(best) + "," + time / 10);
                sw.WriteLine(ACOtsp.problem[i] + ", " + ACObest + "," + bestsum / 10 + "," + std(best) + "," + time / 10);

            }
            sw.Close();

        }

        public static double std(double[] fit)
        {
            double sum = 0.0, average;
            for (int i = 0; i < fit.Length; i++)
                sum += fit[i];
            average = sum / fit.Length;
            sum = 0.0;
            for (int i = 0; i < fit.Length; i++)
                sum += (Math.Pow(fit[i] - average, 2));
            return Math.Pow(sum / fit.Length, 0.5);
        }
    }

    class TSP : ACO
    {
        public int fitness;
        public string[] problem = new string[] { "Bays29", "Berlin52", "Eil51", "Eil76", "Pr76", "St70", "Oliver30" };
        public int distanceLength;

        public void initDistance(int i)
        {
   
            if (i == 0)
            {//只有 Bays29 的問題 已經算好了各點之間的距離 所以直接使用
                distance = new double[29][];
                Visibility = new double[29][];
                //doubledistance = new double[29][];
                for (int k = 0; k < distance.Length; k++)
                {
                    distance[k] = new double[29];
                    Visibility[k] = new double[29];
                    //doubledistance[k] = new double[29];
                }
                readStreetDistance("TSP\\"   +problem[i] + ".txt");
            }
            else
            {
                readGeographicalDistance(problem[i] + ".txt");
            }
                distanceLength = distance[0].Length;
        }
        public static void readGeographicalDistance(string file)
        {
            StreamReader str = new StreamReader(file);
            string all = str.ReadToEnd();
            string[] c = all.Split(new char[] { ' ', '\n' });
            distance = new double[int.Parse(c[0])][];
            Visibility = new double[int.Parse(c[0])][];

            //doubledistance = new double[int.Parse(c[0])][];

            for (int i = 0; i < int.Parse(c[0]); i++)
            {
                distance[i] = new double[int.Parse(c[0])];
                Visibility[i] = new double[int.Parse(c[0])];
            }
            for (int i = 0; i < int.Parse(c[0]); i++)
            {
                for (int j = 0; j < int.Parse(c[0]); j++)
                {
                    int t1 = 3 * i + 1, t2 = 3 * j + 1;
                    double x1 = Double.Parse(c[t1 + 1]), y1 = Double.Parse(c[t1 + 2]);
                    double x2 = Double.Parse(c[t2 + 1]), y2 = Double.Parse(c[t2 + 2]);
                    distance[i][j] = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
                    //doubledistance[i][j] = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)); //在判斷Fitness的時候仍然用計算的實際距離計算   透過兩項 可以使算出來的解更好
                    //distance[i][j] = Math.Truncate(doubledistance[i][j]);                          //在判斷螞蟻行動時用無條件捨去的整數進行運算
                    Visibility[i][j] = 1 / distance[i][j];
                }
            }
             //Eil51 best
             //{15,49,8,48,37,4,36,16,3,17,46,11,45,50,26,47,5,22,6,42,23,1,3,24,12,40,18,39,41,43,14,44,32,38,9,29,33,20,28,19,34,35,2,27,30,7,25,21,0,31,10,1 };

        }
        public static void readStreetDistance(string file)
        {
            StreamReader str = new StreamReader(file);
            string all = str.ReadToEnd();
            string[] c = all.Split(new char[] { ' ', '\n' });
            int index = 0;
            for (int i = 0; i < 29; i++)
            {
                for (int j = 0; j < 29; j++)
                {
                    for (int k = index; k < c.Length; k++)
                    {
                        if (c[k] != "")
                        {
                            index = k + 1;
                            break;
                        }
                    }
                    distance[i][j] = Double.Parse(c[index - 1]);
                    //doubledistance[i][j] = distance[i][j];
                    Visibility[i][j] = 1 / distance[i][j];
                }
            }
         
        }
        public override double Fitness(int[] solution)
        {
            double sum = 0;
            for (int j = 0; j < solution.Length - 1; j++)
                sum = sum + distance[solution[j]][solution[j + 1]];
            sum = sum + distance[solution[solution.Length - 1]][solution[0]];
            //fitness++;
            return sum;
        }
    }

}
