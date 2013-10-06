using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;


namespace ACO_TsaiHung
{
    class ACO
    {
        public double GBestFitness;
        //private double LBestFitness;
        private int antSize;//each cycle can use number of antSize ant;
        private int Dimension;//number of City;
        private double Upperbound;//城市數上限  
        private double Lowerbound;//城市數下限 // 此用於非對稱TSP OR 每個點不一定連接等等的問題  //在此可以不使用

        Random R = new Random(Guid.NewGuid().GetHashCode());//隨機數字用於輪盤法
        public static double[][] distance;//between every node and node length;
        public static double[][] Visibility;//eta is 1/distance
        //public static double[][] doubledistance;//用於取整數計算ACO後 的計算Fitness

        private double [][]pheromone;//費洛蒙
        private double[] antFitness;//each ant AllowNode length
        private int[][] antAtCity;//每隻螞蟻移動城市的順序

        private double[][] probability;//each ant probability from node i to node j ;
        //private int[] GbestTrail;//it can print best tour // on run end

        private double[] Pro_Roulette;//RouletteWheel;
        private double Alpha = 1, Beta = 5, rho = 0.9;//Parameter basics
        private double C=1/*0.0001*/, Q=1;//Constant setting;
        private int Wheelindex;//NextNode
        List<int> AllowNode = new List<int>();	//allowable node
        //private int[] GbestAntTrail;//it can print best tour // on run end
        public int[] GbestTrail;
        public void Init(int Population,int Dimension, int Lowerbound,int Upperbound)
        {
            this.antSize = Population;
            this.Dimension = Dimension;
            this.Upperbound = Upperbound;//dimesion
            this.Lowerbound = Lowerbound;//0
            GBestFitness = Double.MaxValue;

            antAtCity = new int[Population][];//產生螞蟻數量的禁忌清單
            antFitness = new double[Population];

            probability = new double[Dimension][];
            pheromone = new double[Dimension][];
            GbestTrail = new int[Dimension];
            for (int i = 0; i < Population; i++)
            {
                antAtCity[i] = new int[Dimension];//產生城市數維度的陣列
            }
            for (int i = 0; i < Dimension; i++)
            {
                    probability[i] = new double[Dimension];//產生選擇路徑的機率陣列
                    pheromone[i] = new double[Dimension];
                    for (int j = 0; j < Dimension; j++)
                    {
                        pheromone[i][j] = C;//Initial pheromone trail = C
                    }
            }         
        }

        public void Run(int Iteration)
        {
  
            for (int IterationCount = 0; IterationCount < Iteration; IterationCount++/*IterationCount += Population*/)
            { //number of Iteration            
                for (int ant = 0; ant < antSize; ant++)
                {
                    FirstAllowNode(ant);//設定 螞蟻走的第一個點和允許清單
                    for (int city = 0; city < Dimension; city++)//減去的那次已經在FirstAllowNode的時候做了
                    {//每次螞蟻走一個城市
                        if (city == Dimension - 1)
                        {//最後一個城市 回到第一個城市  // 因為FITNESS會算這個部分的距離 所以只需要更新費洛蒙
                            //LocalPheromoneUpdate(antAtCity[ant][city], antAtCity[ant][0]);//更新走過的該段費洛蒙
                            break;
                        }
                        UpdateAllowNode(ant,city);//刪除走過的城市
                        Probility_Ant(ant,city);//計算前往個城市的機率 //並透過輪盤法決定下一個城市
                        //LocalPheromoneUpdate(antAtCity[ant][city], antAtCity[ant][city + 1]);//更新走過的該段費洛蒙   
                        //LocalPheromoneUpdate(antAtCity[ant][city+1], antAtCity[ant][city]);//更新走過的該段費洛蒙(因為是對稱TSP 所以是否對稱的地方也要更新?)  
                    }//End of each ant
                    GetAntFitness(ant);//計算該隻螞蟻的總路徑長度
                }//End of each city
                GlobalPheromoneUpdate();//依據各螞蟻的總路徑長度更新費洛蒙
                evalution();//尋找GBestFitness 並使用菁英法 加重費洛蒙
            }//End of Iteration

            //GbestAntTrail = new int[Dimension];
            //for (int city = 0; city < Dimension; city++)//it can print best tour
            //    Console.Write(GbestTrail[city] + "  ");
        }
        private void GetAntFitness(int i)
        {//get each ant tour length
            int[] Solution = new int[Dimension];//放置前往各城市的先後順序
            for (int j = 0; j < Dimension; j++)
                Solution[j] = antAtCity[i][j];
            antFitness[i] = Fitness(Solution);//丟入Fitness陣列 等尋找GbestFitness
        }
        
        private void FirstAllowNode(int i)
        {//隨機選取該隻螞蟻的起始城市
            AllowNode.Clear();//先清空arraylist
            for (int j = 0; j < Dimension; j++)
                AllowNode.Add(j);//初始化城市
            antAtCity[i][0] =(int)R.Next(0,Dimension);//隨機螞蟻的起始city
        }
        private void UpdateAllowNode(int each_ant,int each_city)
        {
            int k = 0;
            while (k < AllowNode.Count)
            {//將走過的城市 從arraylist中刪除
                if (antAtCity[each_ant][each_city] == AllowNode[k])
                {
                    AllowNode.RemoveAt(k);
                    break;
                }
                k++;
            }
        }
        private void Probility_Ant(int each_Ant, int now_City)
        {
            double sum = 0;
            double[] numerator = new double[AllowNode.Count];
            int[] NextNode=new int[AllowNode.Count];

            for (int city = 0; city < AllowNode.Count; city++)
            {//計算走向下一個node城市的機率
                //updateallownode
                NextNode[city] = AllowNode[city];
                numerator[city] = Math.Pow(pheromone[antAtCity[each_Ant][now_City]][NextNode[city]], Alpha) * Math.Pow(Visibility[antAtCity[each_Ant][now_City]][NextNode[city]], Beta);
              
                sum = sum + numerator[city];
            }
            //double trans = R.NextDouble();  //較新的轉換法則 似乎影像不大 所以不用了
            //double q0 = 0.3;

            //if (trans <= q0)
            //{//直接取最好 不用輪盤 
            //    int tempCity = 0;
            //    double tempNumerator = 0.0;

            //    //for (int a = 0; a < Population; a++)
            //    for (int city = 0; city < AllowNode.Count; city++)
            //        if (numerator[city] >= tempNumerator)
            //        {
            //            tempNumerator = numerator[city];
            //            tempCity = NextNode[city];
            //        }
            //    antAtCity[each_Ant][now_City + 1] = tempCity;
            //}
            //else
            {//用輪盤法選下一個城市
                Pro_Roulette = new double[AllowNode.Count];
                for (int city = 0; city < AllowNode.Count; city++)
                    Pro_Roulette[city] = numerator[city] / sum;
                RouletteWheel(each_Ant, now_City);
            }
        }
        private void RouletteWheel(int each_ant, int now_City)
        {//利用輪盤法決定接下來要去哪個城市
            double WheelPool = Pro_Roulette[0];
            Wheelindex = 0;
            double ran = R.NextDouble();
            while (ran > WheelPool)
            {
                Wheelindex++;
                WheelPool += Pro_Roulette[Wheelindex];
            }

            antAtCity[each_ant][now_City + 1] = AllowNode[Wheelindex];
        }

        private void evalution()
        {//評估適合度 // 計算每隻螞蟻走的路徑總長 取最小的為GBestFitness   
            int GbestAnt = 0;
            // LBestFitness = Double.MaxValue;
            for (int ant = 0; ant < antSize; ant++)
            {

                //if (antFitness[ant] < LBestFitness)
                //{
                //    LBestFitness = antFitness[ant];
                //    LbestAnt = ant;
                //}
                if (antFitness[ant] < GBestFitness)
                {
                    GbestAnt = ant;
                    GBestFitness = antFitness[ant];
                    for (int i = 0; i < Dimension; i++)
                    {
                        GbestTrail[i] = antAtCity[ant][i];
                    }                 
                }
            }
            elitist();
        }//End of Evalution

        private void LocalPheromoneUpdate(int i,int j )
        {//區域更新似乎影響不大 無論是否用加入殘留率及蒸發率
            pheromone[i][j] = pheromone[i][j]/* * rho*/ + C;
        }
        private void GlobalPheromoneUpdate()
        {
            for (int x = 0; x < Dimension; x++)
                for (int y = 0; y < Dimension; y++)
                    pheromone[x][y] *= rho; //全部揮發一次

                    for (int ant = 0; ant < antSize; ant++)
                    {//針對每隻螞蟻走過的路徑逕行費洛蒙更新;
                        for (int city = 0; city < Dimension; city++)
                        {
                            if(city ==Dimension-1)
                                pheromone[antAtCity[ant][Dimension - 1]] [antAtCity[ant][0]] +=  Q / antFitness[ant];
                            else
                                pheromone[antAtCity[ant][city]][antAtCity[ant][city + 1]] +=  Q / antFitness[ant];//因為是循環問題CYCLE  所以終點->起始點也要更新
                        }
                    }   
        }
        private void elitist()
        {//菁英 的部分有先*=RHO 避免 太早收斂
            for (int city = 0; city < Dimension; city++)
            {
             if(city==Dimension-1)
                 pheromone[GbestTrail[city]][GbestTrail[0]] = pheromone[GbestTrail[city]][GbestTrail[0]] /* * rho */+ Q / GBestFitness;
             else//循環問題 CYCLE 
                 pheromone[GbestTrail[city]][GbestTrail[city + 1]] = pheromone[GbestTrail[city]][GbestTrail[city + 1]] /** rho */+ Q / GBestFitness;
            }
        }
        public virtual double Fitness(int[] f)
        {
            return -1;
        }
    }
}
