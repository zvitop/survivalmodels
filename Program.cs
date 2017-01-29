﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurvivalAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var appX = new CrashMetaData[] {new CrashMetaData{UserID = 0, CrashTime = 1, Crashed = false},
                                            new CrashMetaData{UserID = 1, CrashTime = 5, Crashed = true},  
                                            new CrashMetaData{UserID = 2, CrashTime = 5, Crashed = false}, 
                                            new CrashMetaData{UserID = 3, CrashTime = 8, Crashed = false}, 
                                            new CrashMetaData{UserID = 4, CrashTime = 10, Crashed = false},
                                            new CrashMetaData{UserID = 5, CrashTime = 12, Crashed = true},
                                            new CrashMetaData{UserID = 6, CrashTime = 15, Crashed = false}, 
                                            new CrashMetaData{UserID = 7, CrashTime = 18, Crashed = true}, 
                                            new CrashMetaData{UserID = 8, CrashTime = 21, Crashed = false},
                                            new CrashMetaData{UserID = 9, CrashTime = 22, Crashed = true}};
            Console.WriteLine("Begin Survival Analysis demo\n");

            Console.WriteLine("Data for mobile app X:\n");

            for (int i = 0; i < appX.Length; ++i)
                Console.WriteLine(appX[i].ToString());


            var appY = new CrashMetaData[] {new CrashMetaData{UserID = 0, CrashTime = 1, Crashed = true},
                                            new CrashMetaData{UserID = 1, CrashTime = 5, Crashed = true},  
                                            new CrashMetaData{UserID = 2, CrashTime = 6, Crashed = true}, 
                                            new CrashMetaData{UserID = 3, CrashTime = 10, Crashed = false}, 
                                            new CrashMetaData{UserID = 4, CrashTime = 10, Crashed = true},
                                            new CrashMetaData{UserID = 5, CrashTime = 11, Crashed = true},
                                            new CrashMetaData{UserID = 6, CrashTime = 15, Crashed = false}, 
                                            new CrashMetaData{UserID = 7, CrashTime = 20, Crashed = true}, 
                                            new CrashMetaData{UserID = 8, CrashTime = 21, Crashed = true},
                                            new CrashMetaData{UserID = 9, CrashTime = 22, Crashed = true}};



            Console.WriteLine("Data for mobile app Y:\n");

            for (int i = 0; i < appY.Length; ++i)
                Console.WriteLine(appY[i].ToString());



            Console.WriteLine("Computing KM survival probability estimates\n");
            SurvivalCurve appXKM = EstimateKaplanMeier(appX);
            SurvivalCurve appYKM = EstimateKaplanMeier(appY);
            Console.WriteLine("Kaplan Meier estimates computed");

            Console.WriteLine("\nKaplan Meier estimate for Application X");
            Console.WriteLine(appXKM);
            Console.WriteLine("\nKaplan Meier estimate for Application Y");
            Console.WriteLine(appYKM);
            Console.WriteLine("Median Survival Time for Application X");
            Console.WriteLine(GetSurvivalMedianTime(appXKM) + " days");
            Console.WriteLine("Median Survival Time for Application Y");
            Console.WriteLine(GetSurvivalMedianTime(appYKM) + " days");

            Console.WriteLine("End Survival Analysis demo\n");

            Console.ReadLine();

        }

        public static SurvivalCurve EstimateKaplanMeier(CrashMetaData[] crashData)
        {
            if (crashData == null) return null;

            var survivalCurve = new SurvivalCurve();

            var crashDataList = crashData.AsEnumerable<CrashMetaData>().ToList<CrashMetaData>();
            int lastCrashTime = 0;
            int n = crashData.Length;
            int atRisk = n;
            int censored = 0;
            int crashed = 0;
            double runningSurvivalEstimate = 1;
            survivalCurve.AddPoint(0, runningSurvivalEstimate);
            for (int i = 0; i < n; ++i)
            {
                if (i + 1 < n && crashData[i + 1].CrashTime > lastCrashTime && crashed > 0)
                {
                    runningSurvivalEstimate *= 1 - (double)crashed / (double)(atRisk - censored);
                    survivalCurve.AddPoint(lastCrashTime, runningSurvivalEstimate);
                    atRisk = atRisk - censored - crashed;
                    censored = 0;
                    crashed = 0;
                }
                if (!crashData[i].Crashed)
                {
                    ++censored;
                    continue;
                }
                if (crashData[i].CrashTime > lastCrashTime)
                {
                    lastCrashTime = crashData[i].CrashTime;
                    crashed = 1;
                    continue;
                }
                ++crashed;
            }
            if (crashed != 0)
            {
                runningSurvivalEstimate *= 1 - (double)crashed / (double)(atRisk - censored);
                survivalCurve.AddPoint(lastCrashTime, runningSurvivalEstimate);
            }

            return survivalCurve;
        }
        public static double GetSurvivalMedianTime(SurvivalCurve survivalCurve)
        {
            double medianSurvivalTime = 0;

            if (survivalCurve != null)
            {
                var survivalPoints = survivalCurve.GetSurvivalCurve();
                medianSurvivalTime = (from survivalPoint in survivalPoints
                                      where survivalPoint.Prob <= 0.5
                                      select survivalPoint.Time).Min();


            }
            return medianSurvivalTime;

        }
    }



    public class CrashMetaData
    {
        public int UserID { get; set; }
        public int CrashTime { get; set; }
        public bool Crashed { get; set; }

        public override string ToString()
        {
            string s = "";
            s += "user = " + UserID + ", ";
            s += "day = " + CrashTime.ToString().PadLeft(2) + ", ";
            if (Crashed == true)
                s += "event = app crash";
            else
                s += "event = app off";

            return s;
        }
    }
    public class SurvivalCurve
    {
        protected IList<SurvivalCurvePoint> survivalCurve = null;

        public void AddPoint(int time, double prob)
        {
            if (survivalCurve == null)
            {
                survivalCurve = new List<SurvivalCurvePoint>();
            }
            var survivalCurvePoint = new SurvivalCurvePoint { Time = time, Prob = prob };
            survivalCurve.Add(survivalCurvePoint);
        }

        public IList<SurvivalCurvePoint> GetSurvivalCurve()
        {
            return survivalCurve;
        }
        public override string ToString()
        {
            if (survivalCurve == null) return "";

            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine("Time \t Survival Probability");
            for (int i = 0; i < survivalCurve.Count; ++i)
            {
                strBuilder.AppendLine(survivalCurve[i].Time + " days\t " + Math.Round(survivalCurve[i].Prob, 3).ToString("F3"));
            }
            return strBuilder.ToString();
        }

    }
    public class SurvivalCurvePoint
    {
        public int Time { get; set; }
        public double Prob { get; set; }
    }
}