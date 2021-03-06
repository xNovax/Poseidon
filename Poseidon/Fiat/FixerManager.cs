#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using Newtonsoft.Json;
using Poseidon.Misc;
using Poseidon.Models.FiatCurrency.Fixer;

#endregion

namespace Poseidon.Fiat
{
    /// <summary>
    /// Manager for Fixer API Interactions
    /// </summary>
    public class FixerManager
    {
        private FixerResponse _response;
        private FixerEntry _entry;
        private bool fail;


        /// <summary>
        /// Public function for getting data from Fixer, manipulating it, and storing it
        /// </summary>
        public void GetFiatRates()
        {
            GetData();

            if (fail) return;
            _response.rebasedRates = Rebase(_response.rates, Settings.GetCurrency());
            _entry = MakeEntry();
            AddToDatabase();
        }

        /// <summary>
        /// Gets a response from the FixerAPI
        /// </summary>
        private void GetData()
        {
            fail = false;
            var client = new WebClient();
            try
            {
                //Rates are all in base EURO for the free api
                var address = "http://data.fixer.io/api/latest?access_key=" + Settings.GetFixer_Key();
                var data = client.OpenRead(address);
                var reader = new StreamReader(data);
                var jsonText = reader.ReadToEnd();

                _response = JsonConvert.DeserializeObject<FixerResponse>(jsonText);

                if (_response.success == false)
                {
                    Logger.WriteLine("Fixer Error " + _response.error.code + " : " + _response.error.info);
                    fail = true;
                }

                Logger.WriteLine("Updated Fiat Rates from Fixer");
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Creates an entry from the Fixer API response
        /// </summary>
        private FixerEntry MakeEntry()
        {
            FixerResponse response = _response;
            FixerEntry entry = new FixerEntry();
            entry.SetTimeStamp(Utilities.UnixTimestampToString(response.timeStamp));
            foreach (var valuation in response.rebasedRates)
            {
                entry.AddValuation(valuation.Key, valuation.Value);
            }

            return entry;
        }

        /// <summary>
        /// Adds the data collected to the database
        /// </summary>
        private void AddToDatabase()
        {
            Database.CreateFixerEntry(_entry);
        }


        /// <summary>
        /// Rebases the currency to the base currency defined in settings
        /// </summary>
        private Dictionary<string, double> Rebase(Dictionary<string, double> original, string conversionCurrency)
        {
            Dictionary<string, double> rebasedRates = new Dictionary<string, double>();

            foreach (var valuation in original)
            {
                var currencyName = valuation.Key;
                var newValue = valuation.Value / original[conversionCurrency];
                rebasedRates.Add(currencyName, newValue);
            }

            return rebasedRates;
        }
    }
}