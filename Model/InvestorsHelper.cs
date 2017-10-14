using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace blockchain_parser.Model
{
    public class InvestorsHelper : DataBaseHelper
    {
        public List<Investors> FindBackers(List<string> addresses) {
            return Reads(db => db.Investors, condition => ValueIsThere(condition.Eth, addresses));
        }
    }
}