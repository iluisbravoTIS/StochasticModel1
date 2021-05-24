using Gurobi;
using ModelStochastic1.Models;
using ModelStochastic1.Models.Structs;
using System;
using System.Collections.Generic;

namespace ModelStochastic1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("READING DATA....");           

            //FOLDER FILES
            string inputFolder = "..//..//..//InputFiles//"; //Relative path
            string outputFolder = "..//..//..//OutputFiles//"; //Relative path

            ReadData readData = new ReadData();
            InputData inputData = readData.readData(inputFolder);

            //GLOBAL VARS
            int nCUT = 0;
            double Min_Stage2_Profit = double.MaxValue;
            double GAP = double.MaxValue;

            //let { p in PROD}
            //inv1[p] := 0;
            List<Inv0_param> inv1List = new List<Inv0_param>();
            inputData.ProdList.ForEach(pl =>
            {
                Inv0_param invProd = new Inv0_param()
                {
                    PROD = pl.PROD,
                    INV0 = 0
                };
                inv1List.Add(invProd);
            });

            SubModelParameters subModelParams = new SubModelParameters()
            {
                inv1= inv1List
            };
            SubModelOutputs subModelOutputs = new SubModelOutputs();

            MasterModelParameters masterModelParameters = new MasterModelParameters();
            MasterModelOutputs masterModelOutputs = new MasterModelOutputs();

            //FOR 50
            for (int x = 0; x <= 50; x++)
            {
                SubModel sb = new SubModel(subModelParams);
                subModelOutputs = sb.Build_Model(inputData, outputFolder);

                //if Stage2_Profit < Min_Stage2_Profit - 0.00001 
                if (subModelOutputs.stage2Profit < Min_Stage2_Profit - 0.00001)
                {
                    //let GAP := min(GAP, Min_Stage2_Profit - Stage2_Profit);
                    GAP = Math.Min(GAP, Min_Stage2_Profit - subModelOutputs.stage2Profit);

                    //option display_1col 0;
                    //display GAP, Make, Sell, Inv;

                    //let nCUT := nCUT + 1;
                    nCUT++;

                    //let { t in 2..T, s in SCEN} time_price[t, s, nCUT] := Time[t, s].dual;
                    List<TimePrice_param> timePriceList = new List<TimePrice_param>();
                    List<T_param> tlist2 = inputData.TList.FindAll(tl => tl.T >= 2);
                    tlist2.ForEach(tl =>
                    {
                        inputData.ScenList.ForEach(sl =>
                        {
                            string cn = "TIME_" + tl.T + "_" + sl.SCEN;
                            double dl = subModelOutputs.gModel.GetConstrByName(cn).Get(GRB.DoubleAttr.Pi);

                            TimePrice_param tpp = new TimePrice_param()
                            {
                                nCUT = nCUT,
                                T = tl,
                                SCEN = sl,
                                DUAL = dl
                            };

                            timePriceList.Add(tpp);
                        });
                    });

                    //let { p in PROD, s in SCEN} bal2_price[p, s, nCUT] := Balance2[p, s].dual;
                    List<Balance2Price_param> balance2PriceList = new List<Balance2Price_param>();
                    inputData.ProdList.ForEach(pl =>
                    {
                        inputData.ScenList.ForEach(sl =>
                        {
                            string cn = "BALANCE2_" + pl.PROD + "_" + sl.SCEN;
                            var dl = subModelOutputs.gModel.GetConstrByName(cn).Get(GRB.DoubleAttr.Pi);

                            Balance2Price_param b2p = new Balance2Price_param()
                            {
                                nCUT = nCUT,
                                PROD = pl,
                                SCEN = sl,
                                DUAL = dl
                            };

                            balance2PriceList.Add(b2p);
                        });
                    });

                    //let { p in PROD, t in 2..T, s in SCEN} sell_lim_price[p, t, s, nCUT] := Sell[p, t, s].urc;
                    List<SellLimPrice_param> sellLimPriceList = new List<SellLimPrice_param>();
                    IndexesVar ixVar = new IndexesVar();
                    inputData.ProdList.ForEach(pl =>
                    {
                        tlist2.ForEach(tl =>
                        {
                            inputData.ScenList.ForEach(sl =>
                            {
                                int ixP = inputData.ProdList.IndexOf(pl);
                                int ixT = tlist2.IndexOf(tl);
                                int ixS = inputData.ScenList.IndexOf(sl);
                                int ix = ixVar.getIx3(ixP, ixT, ixS, inputData.ProdList.Count, tlist2.Count, inputData.ScenList.Count);

                                SellLimPrice_param slp = new SellLimPrice_param()
                                {
                                    nCUT = nCUT,
                                    PROD = pl,
                                    SCEN = sl,
                                    T = tl,
                                    URC = subModelOutputs.sell[ix].RC
                                };
                                sellLimPriceList.Add(slp);
                            });
                        });
                    });
                }
                else break;

                //printf "\nRE-SOLVING MASTER PROBLEM\n\n";

                //solve Master;
                //printf "\n";
                //option display_1col 20;
                //display Make1, Inv1, Sell1;

                MasterModel masterModel = new MasterModel();
                masterModelOutputs = masterModel.buildModel(inputData, nCUT, masterModelParameters, outputFolder);

                //let { p in PROD}
                //inv1[p] := Inv1[p];
                inputData.ProdList.ForEach(pl =>
                {
                    int ixP = inputData.ProdList.IndexOf(pl);
                    //inv1List.Find(il => il.PROD.Equals(pl.PROD)).INV0 = masterModelOutputs.inv1[ixP].X;

                    Inv0_param invProdNew = new Inv0_param()
                    {
                        PROD = pl.PROD,
                        INV0 = masterModelOutputs.inv1[ixP].X
                    };

                    int ixPInv = inv1List.IndexOf(inv1List.Find(il => il.PROD.Equals(pl.PROD)));
                    inv1List[ixPInv] = invProdNew;

                });

            }
        }
    }
}
