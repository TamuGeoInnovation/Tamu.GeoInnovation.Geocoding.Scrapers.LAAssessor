using System;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.GeographicFeatures.Streets;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Agents;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Caches;
using USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Validating.AddressValidators;

namespace USC.GISResearchLab.Geocoding.Scrapers.LAAssessor.Workers
{

    public class NumberOfLotsWorker : LAAssessorCacheWorker
    {

        #region Properties
        private AddressValidatorWorker _AddressValidatorWorker;
        public AddressValidatorWorker AddressValidatorWorker
        {
            get { return _AddressValidatorWorker; }
            set { _AddressValidatorWorker = value; }
        }
        #endregion

        public NumberOfLotsWorker(LAAssessorCache laAssessorCache, LAAssessorAgent laAssessorAgent, AddressValidatorWorker addressValidatorWorker)
            :base(laAssessorCache, laAssessorAgent)
        {
            AddressValidatorWorker = addressValidatorWorker;
        }

        public int GetNumberOfLots(ValidateableStreetAddress address, Street street)
        {
            int ret = -1;

            int evenOddAddress = Convert.ToInt32(address.Number) % 2;

            int[] evenOdd = LAAssessorCache.NumberOfLotsCache.GetNumberOfLots(street.Source, street.Id);

            AddressRangeDoubleSided addressRange = street.GetAddressRangeDoubleSided();
            AddressRange rangeEven = addressRange.getEvenSide();
            AddressRange rangeOdd = addressRange.getOddSide();

            if (evenOdd == null || evenOdd[0] < 0 || evenOdd[1] < 0)
            {
                if (evenOdd== null)
                {
                    evenOdd = new int[2];
                }

                if (evenOdd[0] < 0)
                {
                    ValidateableStreetAddress firstEven = address.Clone();
                    firstEven.Number = rangeEven.getAddressAt(0).ToString();

                    CacheNumberOfLotsFromAssessor(firstEven, rangeEven);
                    int numEven = LAAssessorCache.ParcelCache.CalculateNumberOfLots(firstEven, rangeEven.FromAddress, rangeEven.ToAddress);

                    LAAssessorCache.SegmentCache.CacheSegment(street.Id, street.Source);
                    if (numEven > 0)
                    {
                        evenOdd[0] = numEven;
                    }
                    else
                    {
                        evenOdd[0] = 0;
                    }

                    LAAssessorCache.NumberOfLotsCache.CacheNumberOfLotsEven(numEven, street.Source, addressRange.Id);
                }

                if (evenOdd[1] < 0)
                {
                    ValidateableStreetAddress firstOdd = address.Clone();
                    firstOdd.Number = rangeOdd.getAddressAt(0).ToString();

                    CacheNumberOfLotsFromAssessor(firstOdd, rangeOdd);
                    int numOdd = LAAssessorCache.ParcelCache.CalculateNumberOfLots(firstOdd, rangeOdd.FromAddress, rangeOdd.ToAddress);

                    LAAssessorCache.SegmentCache.CacheSegment(street.Id, street.Source);
                    if (numOdd > 0)
                    {
                        evenOdd[1] = numOdd;
                    }
                    else
                    {
                        evenOdd[1] = 0;
                    }
                    LAAssessorCache.NumberOfLotsCache.CacheNumberOfLotsOdd(numOdd, street.Source, addressRange.Id);
                }
            }

            if (evenOddAddress == 0)
            {
                ret = evenOdd[0];
            }
            else
            {
                ret = evenOdd[1];
            }

            return ret;
        }

        public void CacheNumberOfLotsFromAssessor(ValidateableStreetAddress address, AddressRange addressRange)
        {
            try
            {
                // Handles the case where an address is not in the cache
                // We need to test all the addresses on this segement to see if they exist
                //
                // Procedure:
                // 
                // 1)   Generate all addresses on that street
                // 2)   Use addressValidator source (assessor) to verify each
                //         -- we can be smart and only call the assessor as many times as needed by
                //          a) calling it with substrings to get as many results as possible per page
                //          b) stop calling it when it not going to give us any more information
                //             i) if it returns less that 500 results, we don't need furter calls, we got all it is going to give

                addressRange.generateAddresses(true);

                for (int i = 0; i < addressRange.getNumberOfAddresses(); i++)
                {
                    int currentAddress = addressRange.getAddressAt(i);

                    string currentAddressString = currentAddress.ToString();
                    CachedParcel currentParcelCached = LAAssessorCache.ParcelCache.checkExtractedParcelCache(address, currentAddressString);

                    // // if the address is not in the cache, query the assessor source to validate it
                    if (currentParcelCached == null)
                    {
                        // loop through all substring portions of the current address
                        // this is to get the most addresses possible on the page returning from the assessor


                        for (int j = 0; j <= currentAddressString.Length; j++)
                        {
                            // check if the substring of the current address has been queried
                            string currentAddressSubstring = currentAddressString.Substring(0, j);
                            CachedQuery cachedQuery = LAAssessorCache.QueryCache.checkQueriesCache(address, currentAddressSubstring);


                            // if this substring is in the query cache, 
                            //  if it returned less than 500 results the first time
                            //   increment the iterator so its children are not done
                            //   since they don't need to be
                            if (cachedQuery != null)
                            {
                                if (cachedQuery.resultCount < 500)
                                {
                                    // move to the next address in the address range
                                    // the i++ in the loop wil increment it by 1, so we need to subtract 1

                                    // if we have the full string, jump 2 for Odd/Even
                                    // else jump 1
                                    if (j == currentAddressString.Length)
                                    {
                                        i = addressRange.jumpIterator(i, j, true) - 1;
                                    }
                                    else
                                    {
                                        i = addressRange.jumpIterator(i, j, false) - 1;
                                    }

                                    // get out of the substring loop
                                    j = currentAddressString.Length + 1;
                                }
                            }
                            // if this substring is not in the cache, check it against the assessor
                            else
                            {
                                AddressValidatorResult addressValidatorResult = AddressValidatorWorker.ValidateAddress(address, currentAddressSubstring);
                                if (addressValidatorResult != null)
                                {
                                    if (addressValidatorResult.count < 1)
                                    {
                                        // find and uncomment
                                        //AddressCacheUtils.cacheInvalidAddress(address, currentAddressSubstring, statistics);
                                    }

                                    // check the count of results returned from the assessor
                                    // if under 500, 
                                    //   we do not need to requery the assessor on addresses with more-complete substrings, 
                                    //   because we have already gotten all that we are going to get
                                    //      more queries would return either: 
                                    //         results that we already have which are valid
                                    //         results we dont have which are all going to be invalid
                                    //      no matter what, the numberOfLots will not be increasing

                                    if (addressValidatorResult.count < 500)
                                    {
                                        // the i++ in the loop wil increment it by 1, so we need to subtract 1

                                        // if we have the full string, jump 2 for Odd/Even
                                        // else jump 1
                                        if (j == currentAddressString.Length)
                                        {
                                            i = addressRange.jumpIterator(i, j, true) - 1;
                                        }
                                        else
                                        {
                                            i = addressRange.jumpIterator(i, j, false) - 1;
                                        }

                                        // now exit the substring loop
                                        j = currentAddressString.Length + 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("An error occured while querying the assessor: " + e.Message);
            }
        }

    }
}
