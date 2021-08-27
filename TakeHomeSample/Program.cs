using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace TakeHomeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            AccountingFile accountingFile = new AccountingFile("C:\\Users\\ryan.t.defreitas\\Documents\\accounting.txt");
            foreach (ResolvedAccounting resolvedAccounting in accountingFile.ResolvedAccountingLines)
            {
                if (resolvedAccounting.AmountOwed > 0)
                {
                    Console.WriteLine($"{resolvedAccounting.Name} is owed ${resolvedAccounting.AmountOwed} " +
                        $"for order {resolvedAccounting.Order}");
                }
                else if (resolvedAccounting.AmountOwed < 0)
                {
                    Console.WriteLine($"{resolvedAccounting.Name} was overpaid by ${Math.Abs(resolvedAccounting.AmountOwed)} " +
                        $"for order {resolvedAccounting.Order}");
                }
            }
        }
    }

    public class AccountingFile
    {
        public List<AccountingLine> AccountingLines { get; set; }
        public List<ResolvedAccounting> ResolvedAccountingLines { get; set; }

        public AccountingFile(string filename)
        {
            this.AccountingLines = new List<AccountingLine>();
            this.ResolvedAccountingLines = new List<ResolvedAccounting>();

            if (!File.Exists(filename)) { throw new Exception("File not Found"); }
            this.PopulateAccountingLines(filename);
            this.ResolveAccounting();
        }

        private void PopulateAccountingLines(string filename)
        {
            // Parse file to a string array.
            string[] fileLines = File.ReadAllLines(filename);

            for (int i = 1; i < fileLines.Length; i++)
            {
                AccountingLine accountingLine = new AccountingLine();
                string[] lineValues = fileLines[i].Split(',');
                accountingLine.TransId = int.Parse(lineValues[0]);
                accountingLine.Order = lineValues[1];
                if (lineValues[2] == "Obligation")
                {
                    accountingLine.TransactionType = TransactionType.Obligation;
                }
                else
                {
                    accountingLine.TransactionType = TransactionType.Payment;
                }
                accountingLine.Amount = decimal.Parse(lineValues[3]);
                accountingLine.TransactionDate = DateTime.Parse(lineValues[4]);
                accountingLine.Name = lineValues[5];

                AccountingLines.Add(accountingLine);
            }
        }
        private void ResolveAccounting()
        {
            //Grab a uniqu list of orders
            List<string> uniqueOrders = (from order in AccountingLines
                                         select order.Order)
                                        .Distinct().ToList();
            //Loop through Each Order Number
            foreach (string order in uniqueOrders)
            {
                // Grab total Obligated
                decimal totalObligated = (from line in AccountingLines
                                          where line.Order == order
                                          && line.TransactionType == TransactionType.Obligation
                                          select line.Amount).Sum();
                // Grab total Paid
                decimal totalPaid = (from line in AccountingLines
                                          where line.Order == order
                                          && line.TransactionType == TransactionType.Payment
                                          select line.Amount).Sum();
                //Grab info 
                AccountingLine sampleLine = (from sample in AccountingLines
                                             where sample.Order == order
                                             select sample)
                                            .FirstOrDefault();

                ResolvedAccounting resolvedAccounting = new ResolvedAccounting();
                resolvedAccounting.AmountObligated = totalObligated;
                resolvedAccounting.AmountPaid = totalPaid;
                resolvedAccounting.Name = sampleLine.Name;
                resolvedAccounting.Order = order;

                ResolvedAccountingLines.Add(resolvedAccounting);
            }                                     
        }
    }

    public class ResolvedAccounting
    { 
        public string Order { get; set; }
        public string Name { get; set; }
        public decimal AmountObligated { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountOwed 
        {
            get 
            {
                return (this.AmountObligated - this.AmountPaid);
            }
        }
    }


    public class AccountingLine
    {
        public int TransId {get; set;}
        public string Order { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Name { get; set; }
    }

    public enum TransactionType
    { 
        Obligation,
        Payment
    }
}
