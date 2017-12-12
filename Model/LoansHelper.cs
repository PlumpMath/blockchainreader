using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace blockchain_parser.Model
{
    public class LoansHelper : DataBaseHelper
    {

        public List<Loans> FindProjects(List<string> addresses) {
           return ExecuteDbTransaction(db => {
               return db.Loans
                .Include(l => l.InvestorPaymentReferences).ThenInclude(r => r.Investor).ThenInclude(i => i.User)
                .Include(l => l.Creator).ThenInclude(c => c.User)
                .Where(l => l.WalletAddress != null 
                    && addresses.Any(a => l.WalletAddress.Equals(a, StringComparison.CurrentCultureIgnoreCase) ))
                    .ToList();
           });
        }
    }
}