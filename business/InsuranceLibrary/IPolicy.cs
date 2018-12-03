using System;
using System.Collections.Generic;

namespace IFInsurance.Library
{
    public interface IPolicy
    {
        /// <summary>
        /// Name of insured object
        /// </summary>
        string NameOfInsuredObject { get; set; }

        /// <summary>
        /// Date when policy becomes active
        /// </summary>
        DateTime ValidFrom { get; set; }

        /// <summary>
        /// Date when policy becomes inactive
        /// </summary>
        DateTime ValidTill { get; set; }
        
        /// <summary>
        /// Total price of the policy. Calculate by summing up all insured risks.
        /// Take into account that risk price is given for 1 full year. Policy/risk period can be shorter.
        /// </summary>
        decimal Premium { get; }

        /// <summary>
        /// Initially included risks of risks at specific moment of time.
        /// </summary>
        IList<Risk> InsuredRisks { get; set; }
    }
}