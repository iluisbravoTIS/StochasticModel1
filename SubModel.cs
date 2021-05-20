using Gurobi;
using ModelStochastic1.Models;
using ModelStochastic1.Models.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelStochastic1
{
    class SubModel
    {
        SubModelParameters subModelParams = new SubModelParameters();

        public SubModel(SubModelParameters subModelParameters) {
            subModelParams = subModelParameters;
        }

        IndexesVar ixVar = new IndexesVar();
        public SubModelOutputs Build_Model(
          InputData inputData,
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
                GRBVar[] Make = gModel.AddVars(inputData.ProdList.Count * Tlist2.Count * inputData.ScenList.Count, GRB.CONTINUOUS);
                GRBVar[] Inv = gModel.AddVars(inputData.ProdList.Count * Tlist2.Count * inputData.ScenList.Count, GRB.CONTINUOUS);
                GRBVar[] Sell = gModel.AddVars(inputData.ProdList.Count * Tlist2.Count * inputData.ScenList.Count, GRB.CONTINUOUS);

                //MAKE VARNAME
                inputData.ProdList.ForEach(pl => {
                    Tlist2.ForEach(tl => {
                        inputData.ScenList.ForEach(sl =>
                        {
                            int ixP = inputData.ProdList.IndexOf(pl);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(sl);
                            int ix = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);
                            Make[ix].VarName = "MAKE_PROD: " + pl.PROD + " T: " + tl.T + " SCEN: " + sl.SCEN;
                        });
                    });                   
                });

                //INV VARNAME
                inputData.ProdList.ForEach(pl => {
                    Tlist2.ForEach(tl => {
                        inputData.ScenList.ForEach(sl =>
                        {
                            int ixP = inputData.ProdList.IndexOf(pl);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(sl);
                            int ix = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);
                            Inv[ix].VarName = "INV_PROD: " + pl.PROD + " T: " + tl.T + " SCEN: " + sl.SCEN;
                        });
                    });
                });

                //SELL VARNAME
                inputData.ProdList.ForEach(pl => {
                    Tlist2.ForEach(tl => {
                        inputData.ScenList.ForEach(sl =>
                        {
                            int ixP = inputData.ProdList.IndexOf(pl);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(sl);
                            int ix = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);
                            Sell[ix].VarName = "SELL_PROD: " + pl.PROD + " T: " + tl.T + " SCEN: " + sl.SCEN;
                        });
                    });
                });

                //MAKE => 0 (constrain)
                //GRB.CONTINUOUS
                for (int a = 0; a < Make.Length; a++)
                {
                    expr1.Clear();
                    expr1.AddTerm(1, Make[a]);
                    gModel.AddConstr(expr1, GRB.GREATER_EQUAL, 0, ">=0");
                }

                //INV => 0 (constrain)
                for (int a = 0; a < Inv.Length; a++)
                {
                    expr1.Clear();
                    expr1.AddTerm(1, Inv[a]);
                    gModel.AddConstr(expr1, GRB.GREATER_EQUAL, 0, ">=0");
                }

                //SELL => 0 (constrain)
                for (int a = 0; a < Sell.Length; a++)
                {
                    expr1.Clear();
                    expr1.AddTerm(1, Sell[a]);
                    gModel.AddConstr(expr1, GRB.GREATER_EQUAL, 0, ">=0");
                }

                //SELL <= market[p,t]
                expr1.Clear();
                inputData.ProdList.ForEach(pl => {
                    Tlist2.ForEach(tl => {
                        inputData.ScenList.ForEach(sl => {
                            double market = inputData
                            .MarketList
                            .Find(ml =>
                            ml.PROD.Equals(pl.PROD) &&
                            ml.T.Equals(tl.T)).MARKET;

                            int ixP = inputData.ProdList.IndexOf(pl);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(sl);

                            int index = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);
                            expr1.Clear();
                            expr1.AddTerm(1, Sell[index]);
                            gModel.AddConstr(expr1, GRB.LESS_EQUAL, market, "<=Market");

                        });
                    });
                });

                //Construir funcion objetivo
                //maximize Stage2_Profit:
                //   sum {s in SCEN} prob[s] *  
                //      sum {p in PROD, t in 2..T} (revenue[p,t,s]*Sell[p,t,s] -
                //         prodcost[p]*Make[p,t,s] - invcost[p]*Inv[p,t,s]);
                expr1.Clear();
                inputData.ScenList.ForEach(s =>
                {
                    double prob = inputData.ProbList.Find(x => x.SCEN.Equals(s.SCEN)).PROB;

                    inputData.ProdList.ForEach(p =>
                    {
                        Tlist2.ForEach(tl =>
                        {
                            //SELL
                            double revenue = inputData
                            .RevenueList
                            .Find(r =>
                            r.PROD.Equals(p.PROD) &&
                            r.T.Equals(tl.T) &&
                            r.SCEN.Equals(s.SCEN)
                            ).REVENUE;

                            double prodCost = inputData
                            .ProdCostList
                            .Find(pc =>
                            pc.PROD.Equals(p.PROD)
                            ).PRODCOST;

                            double invCost = inputData
                            .InvCostList
                            .Find(ic =>
                            ic.PROD.Equals(p.PROD)
                            ).INVCOST;

                            int ixP = inputData.ProdList.IndexOf(p);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(s);

                            int indexVar = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);

                            expr1.AddTerm(prob * revenue, Sell[indexVar]);
                            expr1.AddTerm(-1 * prob * prodCost, Make[indexVar]);
                            expr1.AddTerm(-1 * prob * invCost, Inv[indexVar]);
                        });
                    });

                });

                //Insertar funcion objetivo
                gModel.SetObjective(expr1, GRB.MAXIMIZE);

                //Insertar Restricciones

               //subject to Time {t in 2..T, s in SCEN}:
               //sum {p in PROD} (1/rate[p]) * Make[p,t,s] <= avail[t];
                expr1.Clear();
                Tlist2.ForEach(tl => {
                    inputData.ProdList.ForEach(pl =>
                    {
                        inputData.ScenList.ForEach(sl =>
                        {
                            double rate = 1 / inputData.RateList.Find(rl => rl.PROD.Equals(pl.PROD)).RATE;

                            int ixP = inputData.ProdList.IndexOf(pl);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(sl);

                            int ixMake = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);

                            expr1.AddTerm(rate, Make[ixMake]);

                            double avail = inputData.AvailList.Find(al => al.T.Equals(tl.T)).AVAIL;

                            gModel.AddConstr(expr1, GRB.LESS_EQUAL, avail, "TIME_" + tl.T + "_" + sl.SCEN);
                        });
                    });
                });

                //subject to Balance2 { p in PROD, s in SCEN}:
                //Make[p, 2, s] + inv1[p] = Sell[p, 2, s] + Inv[p, 2, s];
                //2 => index T => 0
                expr1.Clear();
                inputData.ProdList.ForEach(pl =>
                {
                    inputData.ScenList.ForEach(sl =>
                    {    
                        int ixP = inputData.ProdList.IndexOf(pl);
                        int ixT = 0;
                        int ixS = inputData.ScenList.IndexOf(sl);
                        int ix = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);

                        double inv1 = subModelParams.inv1.Find(il => il.PROD.Equals(pl.PROD)).INV0;

                        expr1.Clear();
                        expr2.Clear();

                        //como sumar un param ????
                        expr1.AddTerm(1, Make[ix]);
                        //expr1.AddTerm(inv1, null); ???? como??? // Se despeja vars & params 
                        expr1.AddTerm(-1, Sell[ix]);
                        expr1.AddTerm(-1,Inv[ix]);
                        
                        gModel.AddConstr(expr1, GRB.EQUAL, inv1, "BALANCE2_"+pl.PROD+"_"+sl.SCEN);

                    });
                });

                //subject to Balance { p in PROD, t in 3..T, s in SCEN}:
                //Make[p, t, s] + Inv[p, t - 1, s] = Sell[p, t, s] + Inv[p, t, s];

                inputData.ProdList.ForEach(pl =>
                {
                    Tlist3.ForEach(tl =>
                    {
                        inputData.ScenList.ForEach(sl =>
                        {
                            int ixP = inputData.ProdList.IndexOf(pl);
                            int ixT = Tlist2.IndexOf(tl);
                            int ixS = inputData.ScenList.IndexOf(sl);
                            int ix = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);
                            int ix2 = ixVar.getIx3(ixP, ixT - 1, ixS, inputData.ProdList.Count, Tlist2.Count, inputData.ScenList.Count);

                            expr1.Clear();
                            expr2.Clear();

                            expr1.AddTerm(1, Make[ix]);
                            expr1.AddTerm(1, Inv[ix2]);

                            expr2.AddTerm(1, Sell[ix]);
                            expr2.AddTerm(1, Inv[ix]);

                            gModel.AddConstr(expr1, GRB.EQUAL, expr2, "SUBJECT TO BALANCE");
                        });
                    });
                });

                //carpeta donde se expande el modelo 
                gModel.Write(outputFolder + "Submodel_Model.lp");
                
                // RESOLVER EL MODELO
                try
                {
                    Console.WriteLine("Solving the sub model with gurobi..");
                    gModel.Optimize();

                    if (gModel.Status == 2) {                        
                        for (int m = 0; m < Make.Length; m++)
                        {
                            Console.WriteLine("Make Var: " + Make[m].VarName + " = " + Make[m].X);
                        }

                        for (int m = 0; m < Inv.Length; m++)
                        {
                            Console.WriteLine("Inv Var: " + Inv[m].VarName + " = " + Inv[m].X);
                        }

                        for (int m = 0; m < Sell.Length; m++)
                        {
                            Console.WriteLine("Sell Var: " + Sell[m].VarName + " = " + Sell[m].X);
                        }
                    }

                    gModel.Dispose();
                    env.Dispose();

                    return new SubModelOutputs()
                    {
                        make = Make,
                        sell = Sell,
                        inv = Inv,
                        stage2_Profit = gModel.ObjVal,
                        gModel = gModel
                    };

                }
                catch { Console.WriteLine("ERROR SOLVING THE MODEL"); }

                gModel.Dispose();
                env.Dispose();
            }

            catch (GRBException ex)
            {

                Console.WriteLine("Error code: " + ex.ErrorCode + ". " + ex.Message);
            }
         
         
            return new SubModelOutputs();


        }
    }
}
