using System;
using System.Collections.Generic;
using System.Linq;

namespace Covid19DB.Services
{
    /// <summary>
    /// Source https://www.50states.com/abbreviations.htm
    /// </summary>
    public class ProvinceLookupService : IProvinceLookupService
    {
        private readonly Dictionary<string, string> USStates = new Dictionary<string, string>
        {
            {"AL", "Alabama"},
            {"AK", "Alaska" },
            {"AZ", "Arizona"  },
            {"AR","Arkansas" },
            {"CA","California" },
            {"CO","Colorado" },
            {"CT", "Connecticut" },
            {"DE", "Delaware" },
            {"FL", "Florida" },
            {"GA","Georgia" },
            {"HI", "Hawaii" },
            {"ID", "Idaho" },
            {"IL", "Illinois" },
            {"IN", "Indiana" },
            {"IA", "Iowa" },
            {"KS","Kansas" },
            {"KY","Kentucky" },
            {"LA","Louisiana" },
            {"ME", "Maine" },
            {"MD", "Maryland" },
            {"MA", "Massachusetts" },
            {"MI", "Michigan" },
            {"MN", "Minnesota" },
            {"MS", "Mississippi" },
            {"MO", "Missouri" } ,
            {"MT", "Montana" },
            {"NE", "Nebraska" },
            {"NV", "Nevada" },
            {"NH", "New Hampshire" },
            {"NJ", "New Jersey" },
            {"NM", "New Mexico" },
            {"NY", "New York" },
            {"NC", "North Carolina" },
            {"ND","North Dakota" },
            {"OH","Ohio" },
            {"OK","Oklahoma" },
            {"OR", "Oregon" },
            {"PA", "Pennsylvania" },
            {"RI", "Rhode Island" } ,
            {"SC", "South Carolina" },
            {"SD", "South Dakota" },
            {"TN", "Tennessee" },
            {"TX","Texas" },
            {"UT", "Utah" },
            {"VT", "Vermont" },
            {"VA", "Virginia" },
            {"WA", "Washington" },
            {"WV", "West Virginia" },
            {"WI", "Wisconsin" },
            {"WY", "Wyoming" },
            {"DC", "District of Columbia" },
            {"D.C.", "District of Columbia" },
            {"MH", "Marshall Islands" },
            {"AA", "Armed Forces Americas" },
            //TODO: Is this correct?
            {"AE", "Armed Forces Africa / Canada / Europe / Middle East" },
            {"AP", "Armed Forces Pacific" }
        };

        private readonly Dictionary<string, string> CanadianStates = new Dictionary<string, string>
        {
            {"AB", "Alberta" },
            {"BC", "British Columbia" },
            {"MB", "Manitoba" },
            {"NB", "New Brunswick"},
            {"NL", "Newfoundland and Labrador" },
            {"NT", "Northwest Territories" },
            {"NS", "Nova Scotia" },
            {"NU", "Nunavut" },
            {"ON", "Ontario"},
            {"PE", "Prince Edward Island" },
            {"QC", "Quebec" },
            {"SK", "Saskatchewan" },
            {"YT", "Yukon" }
        };


        public string GetProvinceName(string country, string provinceCode)
        {
            if (country == "US")
                return USStates[provinceCode];

            if (country == "Canada")
            {
                if (CanadianStates.ContainsKey(provinceCode))
                {
                    return CanadianStates[provinceCode];
                }
                else
                {
                    //Some canadian data uses the full state name
                    if (CanadianStates.Values.Contains(provinceCode))
                    {
                        return provinceCode;
                    }
                }
            }

            throw new NotImplementedException();
        }
    }
}
