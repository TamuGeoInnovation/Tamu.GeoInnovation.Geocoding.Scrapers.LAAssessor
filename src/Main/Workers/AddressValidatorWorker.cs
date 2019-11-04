using System;
using System.Xml.XPath;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Common.Utils.XML;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Validating.AddressValidators;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Workers
{
    public class AddressValidatorWorker : LAAssessorCacheWorker
    {
        public AddressValidatorWorker(LAAssessorCache laAssessorCache, LAAssessorAgent laAssessorAgent)
            : base(laAssessorCache, laAssessorAgent) { }



        public AddressValidatorResult ValidateAddress(ValidateableStreetAddress address, string streetNumber)
        {
            AddressValidatorResult addressValidatorResult = new AddressValidatorResult();



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

                        addressValidatorResult.count = count;

                        bool hasSecondaryAddress = !StringUtils.IsEmpty(currentSecondaryAddress);


                        // if the page that came back did not contain any addresses then,
                        //   the address (method parameter streetNumber) passed to the assessor agent does not exist,
                        //   so cache it as invalid

                        if (currentStreetNumber == null)
                        {

                        }

                        // the page that came back had some addresses on it so,
                        //    add each of them into the cache
                        //    they might already be in the cache, so we have to check for their existence first
                        else
                        {
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
                        }
                    }
                }
            }

            return addressValidatorResult;
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
