using System;
using System.Xml.XPath;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Common.Utils.XML;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Workers
{
    /// <summary>
    /// Summary description for AINWorker.
    /// </summary>
    public class AINWorker : LAAssessorCacheWorker
    {
        #region Properties
        private AddressValidatorWorker _AddressValidatorWorker;
        public AddressValidatorWorker AddressValidatorWorker
        {
            get { return _AddressValidatorWorker; }
            set { _AddressValidatorWorker = value; }
        }
        #endregion

        public AINWorker(LAAssessorCache laAssessorCache, LAAssessorAgent laAssessorAgent, AddressValidatorWorker addressValidatorWorker)
            : base(laAssessorCache, laAssessorAgent)
        {
            AddressValidatorWorker = addressValidatorWorker;
        }

        public string[] GetAIN(string numberStr, string preDirectional, string name, string suffix, string postDirectional, string city, string state, string zipStr)
        {
            StreetAddress address = new StreetAddress(numberStr, preDirectional, name, suffix, postDirectional, city, state, zipStr);
            return GetAIN(address);
        }
        public string[] GetAIN(StreetAddress address)
        {
            string[] ret = null;

            // query the LA Assessor with the address and street number
            XPathDocument xpathDocument = LAAssessorAgent.RunGetAINAgent(
                address.Number,
                address.PreDirectional,
                address.StreetName,
                address.Suffix,
                address.PostDirectional,
                address.City,
                address.State,
                address.ZIP);

            // get the rows
            XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
            XPathExpression xpathExpression = xpathNavigator.Compile("//Row");
            XPathNodeIterator rows = xpathNavigator.Select(xpathExpression);

            int count = 0;

            // loop through each of the addresses returned by the LA Assessor page
            while (rows.MoveNext())
            {


                XPathNavigator node = rows.Current;

                string currentAssessorId = XMLUtils.getGrandchildValue(node, "AssessorID", "Value");
                string completeAddress = XMLUtils.getGrandchildValue(node, "CompleteAddress", "Value");

                string primaryAddress = XMLUtils.getGrandchildValue(node, "PrimaryAddress", "Value");
                string noNumberPrimaryAddress = XMLUtils.getGrandchildValue(node, "NoNumberPrimaryAddress", "Value");
                string streetNumberString = XMLUtils.getGrandchildValue(node, "StreetNumber", "Value");

                string currentStreetNumber = XMLUtils.getGrandchildValue(node, "StreetNumber", "Value");

                if (currentStreetNumber != null)
                {
                    // for now, we do not insert vacant addresses in to the cache (VAC/VIC in the streetnumber)
                    if (StringUtils.StringIsInt(currentStreetNumber))
                    {

                        string currentSecondaryAddress = XMLUtils.getGrandchildValue(node, "SecondaryAddress", "Value");

                        string city = XMLUtils.getGrandchildValue(node, "City", "Value");
                        string state = XMLUtils.getGrandchildValue(node, "State", "Value");
                        string zip = XMLUtils.getGrandchildValue(node, "Zip", "Value");

                        string countString = XMLUtils.getGrandchildValue(node, "Count", "Value");
                        if (!StringUtils.IsEmpty(countString))
                        {
                            try
                            {
                                count = Convert.ToInt32(countString);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Problem casting CountString (" + countString + ") to int: " + e.Message);
                            }
                        }

                        bool hasSecondaryAddress = !StringUtils.IsEmpty(currentSecondaryAddress);


                        StreetAddress extractedAddress = TokenParseAddress(completeAddress);
                        extractedAddress.City = StringUtils.ValueOrNoBlank(city);
                        extractedAddress.State = "CA";
                        extractedAddress.ZIP = StringUtils.ValueOrNoBlank(zip);

                        // for now do not keep the 1/2 in a 1234 1/2 street address number
                        string[] currentStreetNumberParts = extractedAddress.Number.Split(' ');
                        if (currentStreetNumberParts.Length > 1)
                        {
                            extractedAddress.Number = currentStreetNumberParts[0];
                        }

                        if (address.Equals(extractedAddress))
                        {
                            if (ret == null)
                            {
                                ret = new string[0];
                            }

                            string[] temp = new string[ret.Length + 1];
                            ret.CopyTo(temp, 0);
                            temp[temp.Length - 1] = currentAssessorId;
                            ret = temp;
                        }
                    }
                }
            }
            return ret;
        }


        public bool ValidateAIN(string ain)
        {
            bool ret = false;

            // query the LA Assessor with the address and street number
            XPathDocument xpathDocument = LAAssessorAgent.RunValidateAINAgent(ain);

            // get the rows
            XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
            XPathExpression xpathExpression = xpathNavigator.Compile("//Row");
            XPathNodeIterator rows = xpathNavigator.Select(xpathExpression);


            // loop through each of the addresses returned by the LA Assessor page
            while (rows.MoveNext())
            {


                XPathNavigator node = rows.Current;

                string ainLink = XMLUtils.getGrandchildValue(node, "ainLink", "Value");
                string errorMsg = XMLUtils.getGrandchildValue(node, "errorMsg", "Value");

                if (ainLink != null)
                {
                    ret = true;
                }
                else
                {
                    ret = false;
                }

            }

            return ret;
        }

        public StreetAddress TokenParseAddress(string streetAddress)
        {
            StreetAddress ret = new StreetAddress();

            XPathDocument xpathDocument = LAAssessorAgent.RunTokenBasedParserAgent(streetAddress);
            XPathNavigator xpathNavigator = xpathDocument.CreateNavigator();
            XPathExpression xpathExpression = xpathNavigator.Compile("//Row");
            XPathNodeIterator rows = xpathNavigator.Select(xpathExpression);

            while (rows.MoveNext())
            {
                XPathNavigator node = rows.Current;
                ret.Number = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "number", "Value"));
                ret.PreDirectional = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "pre", "Value"));
                ret.StreetName = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "name", "Value"));
                ret.Suffix = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "suffix", "Value"));
                ret.PostDirectional = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "post", "Value"));
                ret.SuiteType = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "suite", "Value"));
                ret.SuiteNumber = StringUtils.ValueOrNoBlank(XMLUtils.getGrandchildValue(node, "suitenumber", "Value"));
            }

            return ret;
        }

    }
}
