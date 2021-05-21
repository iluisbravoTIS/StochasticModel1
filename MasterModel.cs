using Gurobi;
using ModelStochastic1.Models;
using ModelStochastic1.Models.Structs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModelStochastic1
{
    class MasterModel
    {
        IndexesVar ixVar = new IndexesVar();
        public void buildModel(
             InputData inputData,
             int nCUT,
             MasterModelParameters masterModelParameters,
             string outputFolder
            )
        {
            try
            {
                // Create an empty environment, set options and start
                GRBEnv env = new GRBEnv(true);
                env.Set("LogFile", "LogFileGurobiModel.log");
                env.Start();

                int T = 4;
                List<T_param> Tlist2 = inputData.TList.FindAll(tl => tl.T >= 2);
                List<T_param> Tlist3 = inputData.TList.FindAll(tl => tl.T >= 3);

                // Create empty Gurobi model
                GRBModel gModel = new GRBModel(env);

                // Create an empty linear expression object. 
                //Parameters and variables will be added and then used to add model components
                //(Objective Function and Constraints).
                GRBLinExpr expr1 = 0.0;
                GRBLinExpr expr2 = 0.0;

                // Crear la variables              
                GRBVar[] Make1 = gModel.AddVars(inputData.ProdList.Count, GRB.CONTINUOUS);
                GRBVar[] Inv1 = gModel.AddVars(inputData.ProdList.Count, GRB.CONTINUOUS);
                GRBVar[] Sell1 = gModel.AddVars(inputData.ProdList.Count, GRB.CONTINUOUS);

                //MAKE1 VARNAME
                inputData.ProdList.ForEach(pl => {
                    int ixP = inputData.ProdList.IndexOf(pl);
                    Make1[ixP].VarName = "MAKE1_Prod: " + pl.PROD;                        
                });

                //INV1 VARNAME
                inputData.ProdList.ForEach(pl => {
                    int ixP = inputData.ProdList.IndexOf(pl);
                    Inv1[ixP].VarName = "INV1_Prod: " + pl.PROD;
                });

                //MAKE1 VARNAME
                inputData.ProdList.ForEach(pl => {
                    int ixP = inputData.ProdList.IndexOf(pl);
                    Sell1[ixP].VarName = "SELL1_Prod: " + pl.PROD;
                });

                //MAKE1 => 0 (constrain)
                //GRB.CONTINUOUS
                for (int a = 0; a < Make1.Length; a++)
                {
                    expr1.Clear();
                    expr1.AddTerm(1, Make1[a]);
                    gModel.AddConstr(expr1, GRB.GREATER_EQUAL, 0, ">=0");
                }

                //INV1 => 0 (constrain)
                for (int a = 0; a < Inv1.Length; a++)
                {
                    expr1.Clear();
                    expr1.AddTerm(1, Inv1[a]);
                    gModel.AddConstr(expr1, GRB.GREATER_EQUAL, 0, ">=0");
                }

                //SELL1 => 0 (constrain)
                for (int a = 0; a < Sell1.Length; a++)
                {
                    expr1.Clear();
                    expr1.AddTerm(1, Sell1[a]);
                    gModel.AddConstr(expr1, GRB.GREATER_EQUAL, 0, ">=0");
                }

                //SELL1 <= market[p,1]
                expr1.Clear();
                inputData.ProdList.ForEach(pl => {                    
                    double market = inputData
                    .MarketList
                    .Find(ml =>
                    ml.PROD.Equals(pl.PROD) &&
                    ml.T.Equals(1)).MARKET;

                    int ixP = inputData.ProdList.IndexOf(pl);
                           
                    expr1.Clear();
                    expr1.AddTerm(1, Sell1[ixP]);
                    gModel.AddConstr(expr1, GRB.LESS_EQUAL, market, "<=Market");
                });

                GRBVar Min_Stage2_Profit = gModel.AddVar(double.MinValue, double.MaxValue, masterModelParameters.Min_Stage2_Profit, GRB.CONTINUOUS, "Min_Stage2_Profit");

                //Funcion objetivo
                //maximize Expected_Profit:
                //sum { s in SCEN}
                //prob[s] *
                //sum { p in PROD} (revenue[p, 1, s] * Sell1[p] -
                //prodcost[p] * Make1[p] - invcost[p] * Inv1[p]) +
                //Min_Stage2_Profit;
                expr1.Clear();
                inputData.ScenList.ForEach(sl =>
                {
                    double prob = inputData.ProbList.Find(x => x.SCEN.Equals(sl.SCEN)).PROB;

                    inputData.ProdList.ForEach(pl => {
                        
                        double revenue = inputData
                        .RevenueList
                        .Find(rl => 
                        rl.PROD.Equals(pl.PROD) && 
                        rl.T.Equals(1) && 
                        rl.SCEN.Equals(sl.SCEN))
                        .REVENUE;

                        int ixP = inputData.ProdList.IndexOf(pl);
                        double invcost = inputData.InvCostList.Find(inv => inv.PROD.Equals(pl.PROD)).INVCOST;

                        double prodcost = inputData.ProdCostList.Find(inv => inv.PROD.Equals(pl.PROD)).PRODCOST;

                        expr1.AddTerm(prob * revenue, Sell1[ixP]);
                        expr1.AddTerm(-prob * prodcost, Make1[ixP]);
                        expr1.AddTerm(-prob * invcost, Inv1[ixP]);
                        expr1.AddTerm(1, Min_Stage2_Profit);
                    });

                });

                //Insertar funcion objetivo
                gModel.SetObjective(expr1, GRB.MAXIMIZE);

                //subj to Cut_Defn {k in 1..nCUT}:  KE ESTA PASANDO
                // Min_Stage2_Profit <=
                //    sum { t in 2..T, s in SCEN}
                //    time_price[t, s, k] * avail[t] +
                //    sum { p in PROD, s in SCEN}
                //    bal2_price[p, s, k] * (-Inv1[p]) +
                //    sum { p in PROD, t in 2..T, s in SCEN}
                //    sell_lim_price[p, t, s, k] * market[p, t];
                expr1.Clear();
                double sum1 = 0;
                double sum2 = 0;
                for (int k = 1; k <= nCUT; k++)
                {
                    Tlist2.ForEach(tl =>
                    {
                        inputData.ScenList.ForEach(sl =>
                        {
                            double timePrice = masterModelParameters.timePriceList.Find(x => x.T.Equals(tl.T)
                            && x.SCEN.Equals(sl.SCEN) 
                            && x.nCUT.Equals(k)).TIMEPRICE;

                            double avail = inputData.AvailList.Find(al => al.T.Equals(tl.T)).AVAIL;

                            sum1 += timePrice * avail;
                        });
                    });

                    inputData.ProdList.ForEach(pl =>{
                        Tlist2.ForEach(tl => {
                            inputData.ScenList.ForEach(sl =>
                            {
                                double sellLimPrice = masterModelParameters.sellLimPriceList.Find(slpl => slpl.PROD.Equals(pl.PROD)
                                && slpl.T.Equals(tl.T)
                                && slpl.SCEN.Equals(sl.SCEN)
                                && slpl.nCUT.Equals(k)).SELLLIMPRICE;

                                double market = inputData.MarketList.Find(ml => ml.PROD.Equals(pl.PROD)
                                && ml.T.Equals(tl.T)).MARKET;

                                sum2 += sellLimPrice * market;
                            });
                        });                        
                    });

                    inputData.ProdList.ForEach(pl =>
                    {
                        inputData.ScenList.ForEach(sl =>
                        {
                            double bal2Price = masterModelParameters.balance2PriceList.Find(bp => bp.PROD.Equals(pl.PROD)
                                && bp.SCEN.Equals(sl.SCEN) && bp.nCUT.Equals(k)).BALANCE2PRICE;


                        });
                    });
                }

                //Subject to Time1
                expr1.Clear();
                inputData.ProdList.ForEach(pl =>
                {
                    double rate = 1 / inputData.RateList.Find(rl => rl.PROD.Equals(pl.PROD)).RATE;
                    double avail = inputData.AvailList.Find(al => al.T.Equals(1)).AVAIL;
                    double rateAvail = avail / rate;
                    int ixP = inputData.ProdList.IndexOf(pl);

                    gModel.AddConstr(Make1[ixP], GRB.GREATER_EQUAL, rateAvail, "TIME");

                });


                //Subject to Balance1
                expr1.Clear();
                inputData.ProdList.ForEach(pl => {
                    expr1.Clear();

                    int ixP = inputData.ProdList.IndexOf(pl);
                    expr1.AddTerm(1, Sell1[ixP]);
                    expr1.AddTerm(1, Inv1[ixP]);
                    expr1.AddTerm(-1, Make1[ixP]);

                    double inv0 = inputData.Inv0List.Find(il => il.PROD.Equals(pl.PROD)).INV0;

                    gModel.AddConstr(inv0, GRB.EQUAL, expr1, "Balance 1");
                });
            }
            catch (GRBException ex)
            {
                Console.WriteLine("Error code: " + ex.ErrorCode + ". " + ex.Message);
            }

        }
    }
}
