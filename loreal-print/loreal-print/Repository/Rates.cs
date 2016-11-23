using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using System.Web;
using loreal_print.Models.Rates;
using loreal_print.MEC_Common.Security;
using loreal_print.Models;

namespace loreal_print.Repository
{
    public class Rates
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        

        public RatesModel GetRates()
        {
            var lResult = new RatesModel();
            var lLogMsg = string.Empty;
            var lRatesContainer = new RatesModel();
            var pContext = System.Web.HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            var newParentADType = new RateParentAdType();
            var lParentAdTypeID = 0;
            var lRateTypeID = 0;
            var lAdTypeID = 0;
            var lTierID = -1;

            var isTierOpen = false;
            var isAdOpen = false;
            var isRateOpen = false;
            var isParentAdOpen = false;

            var newRateRateType = new RateRateType();
            var newRateAdType = new RateAdType();
            var newRateTier = new RateTier();
            var newRateEditionType = new RateEditionType();
            var recordCount = 0;

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    //var lRatesData = db.GetRates("2017", 8, 1)
                    var lRatesData = db.GetRates(lCookieData.Year, lCookieData.BookID, lCookieData.VersionID)
                    .ToList();

                    var recordTotal = lRatesData.Count;

                    lRatesContainer.PublisherName = lCookieData.Publisher;
                    lRatesContainer.BookName = lCookieData.Book;
                    lRatesContainer.Year = lCookieData.Year;
                    lRatesContainer.VersionID = lCookieData.VersionID;

                    foreach (var rateItem in lRatesData)
                    {
                        recordCount++;

                        // Add class member values for new record.

                        // ParentAdType
                        if (lParentAdTypeID != rateItem.ParentAdTypeID)
                        {
                            lParentAdTypeID = Convert.ToInt32(rateItem.ParentAdTypeID);

                            /* Ready to add the populated ParentAdType to RatesModel RateParentAdTypes collection or is this the first one?
                             * Reset vars for new new ParentADType */
                            lRateTypeID = 0;
                            lAdTypeID = 0;
                            lTierID = -1;

                            if (isTierOpen)
                            {
                                newRateAdType.RateTiers.Add(newRateTier);
                                isTierOpen = false;
                            }

                            if (isAdOpen)
                            {
                                newRateRateType.RateAdTypes.Add(newRateAdType);
                                isAdOpen = false;
                            }
                            if (isRateOpen)
                            {
                                newParentADType.RateRateTypes.Add(newRateRateType);
                                isRateOpen = false;
                            }

                            // Make this specific to the parent
                            if (isParentAdOpen)
                            {
                                lRatesContainer.RateParentAdTypes.Add(newParentADType);
                                isParentAdOpen = false;
                            }

                            // Now add the new populated ParentADType to the Rates container
                            if (lParentAdTypeID != 0)
                            {
                                newParentADType = new RateParentAdType
                                {
                                    ParentAdTypeID = lParentAdTypeID,
                                    ParentAdType = rateItem.ParentAdType
                                };
                                if (rateItem.AdvertorialEarnedPercentDiscount != null)
                                    newParentADType.AdvertorialEarnedPercentDiscount = rateItem.AdvertorialEarnedPercentDiscount;
                                if (rateItem.BleedOpenPercentPremium != null)
                                    newParentADType.BleedOpenPercentPremium = rateItem.BleedOpenPercentPremium;
                                if (rateItem.BleedEarnedPercentPremium != null)
                                    newParentADType.BleedEarnedPercentPremium = rateItem.BleedEarnedPercentPremium;
                                if (rateItem.Cover2OpenPercentPremium != null)
                                    newParentADType.Cover2OpenPercentPremium = rateItem.Cover2OpenPercentPremium;
                                if (rateItem.Cover2EarnedPercentPremium != null)
                                    newParentADType.Cover2EarnedPercentPremium = rateItem.Cover2EarnedPercentPremium;
                                if (rateItem.Cover3OpenPercentPremium != null)
                                    newParentADType.Cover3OpenPercentPremium = rateItem.Cover3OpenPercentPremium;
                                if (rateItem.Cover3EarnedPercentPremium != null)
                                    newParentADType.Cover3EarnedPercentPremium = rateItem.Cover3EarnedPercentPremium;
                                if (rateItem.Cover4OpenPercentPremium != null)
                                    newParentADType.Cover4OpenPercentPremium = rateItem.Cover4OpenPercentPremium;
                                if (rateItem.Cover4EarnedPercentPremium != null)
                                    newParentADType.Cover4EarnedPercentPremium = rateItem.Cover4EarnedPercentPremium;
                                if (rateItem.FracHalfPageOpenPercentPremium != null)
                                    newParentADType.FracHalfPageOpenPercentPremium = rateItem.FracHalfPageOpenPercentPremium;
                                if (rateItem.FracHalfPageEarnedPercentPremium != null)
                                    newParentADType.FracHalfPageEarnedPercentPremium = rateItem.FracHalfPageEarnedPercentPremium;
                                if (rateItem.FracThirdPageOpenPercentPremium != null)
                                    newParentADType.FracThirdPageOpenPercentPremium = rateItem.FracThirdPageOpenPercentPremium;
                                if (rateItem.FracThirdPageEarnedPercentPremium != null)
                                    newParentADType.FracThirdPageEarnedPercentPremium = rateItem.FracThirdPageEarnedPercentPremium;
                                if (rateItem.FracThirdRunOppFBPOpenPercentPremium != null)
                                    newParentADType.FracThirdRunOppFBPOpenPercentPremium = rateItem.FracThirdRunOppFBPOpenPercentPremium;
                                if (rateItem.FracThirdRunOppFBPEarnedPercentPremium != null)
                                    newParentADType.FracThirdRunOppFBPEarnedPercentPremium = rateItem.FracThirdRunOppFBPEarnedPercentPremium;
                                if (rateItem.SpreadC2P1EarnedPercentDiscount != null)
                                    newParentADType.SpreadC2P1EarnedPercentDiscount = rateItem.SpreadC2P1EarnedPercentDiscount;
                                if (rateItem.SpreadROBEarnedPercentDiscount != null)
                                    newParentADType.SpreadROBEarnedPercentDiscount = rateItem.SpreadROBEarnedPercentDiscount;
                                if (rateItem.FifthColorMetallicOpenDollarPremium != null)
                                    newParentADType.FifthColorMetallicOpenDollarPremium = rateItem.FifthColorMetallicOpenDollarPremium;
                                if (rateItem.FifthColorMetallicEarnedDollarPremium != null)
                                    newParentADType.FifthColorMetallicEarnedDollarPremium = rateItem.FifthColorMetallicEarnedDollarPremium;
                                if (rateItem.FifthColorMatchOpenDollarPremium != null)
                                    newParentADType.FifthColorMatchOpenDollarPremium = rateItem.FifthColorMatchOpenDollarPremium;
                                if (rateItem.FifthColorMatchEarnedDollarPremium != null)
                                    newParentADType.FifthColorMatchEarnedDollarPremium = rateItem.FifthColorMatchEarnedDollarPremium;
                                if (rateItem.FifthColorPMSOpenDollarPremium != null)
                                    newParentADType.FifthColorPMSOpenDollarPremium = rateItem.FifthColorPMSOpenDollarPremium;
                                if (rateItem.FifthColorPMSEarnedDollarPremium != null)
                                    newParentADType.FifthColorPMSEarnedDollarPremium = rateItem.FifthColorPMSEarnedDollarPremium;
                                isParentAdOpen = true;
                            }
                        }
                        // RateType
                        if (lRateTypeID != rateItem.RateTypeID)
                        {
                            lRateTypeID = Convert.ToInt32(rateItem.RateTypeID);

                            if (isTierOpen)
                            {
                                newRateAdType.RateTiers.Add(newRateTier);
                                isTierOpen = false;
                            }

                            if (isAdOpen)
                            {
                                newRateRateType.RateAdTypes.Add(newRateAdType);
                                isAdOpen = false;
                            }

                            if (isRateOpen)
                            {
                                newParentADType.RateRateTypes.Add(newRateRateType);
                                isRateOpen = false;
                            }

                            lAdTypeID = 0;
                            lTierID = -1;
                            if (lRateTypeID != 0)
                            {
                                newRateRateType = new RateRateType
                                {
                                    RateTypeName = rateItem.RateType,
                                    RateTypeID = lRateTypeID
                                };
                                isRateOpen = true;
                            }
                        }

                        // AdType
                        if (lAdTypeID != rateItem.AdTypeID)
                        {
                            lAdTypeID = Convert.ToInt32(rateItem.AdTypeID);

                            if (isTierOpen)
                            {
                                newRateAdType.RateTiers.Add(newRateTier);
                                isTierOpen = false;
                            }
                            lTierID = -1;

                            if (isAdOpen)
                            {
                                newRateRateType.RateAdTypes.Add(newRateAdType);
                                isAdOpen = false;
                            }

                            if (lAdTypeID != 0)
                            {
                                newRateAdType = new RateAdType
                                {
                                    AdType = rateItem.AdType,
                                    AdTypeID = lAdTypeID
                                };
                                if (rateItem.PublisherAgreement != null)
                                    newRateAdType.PublisherAgreement = rateItem.PublisherAgreement;
                                isAdOpen = true;
                            }
                        }

                        // Tier
                        if (lTierID != rateItem.TierID)
                        {
                            lTierID = Convert.ToInt32(rateItem.TierID);

                            if (isTierOpen)
                            {
                                newRateAdType.RateTiers.Add(newRateTier);
                                isTierOpen = false;
                            }

                            newRateTier = new RateTier
                            {
                                Tier = rateItem.Tier,
                                TierRange = rateItem.TierRange,
                                TierID = lTierID
                            };
                            isTierOpen = true;
                        }

                        // EditionType
                        newRateEditionType = new RateEditionType();
                        // Only add a class member if the record has a value.  Otherwise, leave out to preserve NULL.
                        if (rateItem.EditionTypeID != null)
                            newRateEditionType.EditionTypeID = Convert.ToInt32(rateItem.EditionTypeID);
                        if (!String.IsNullOrEmpty(rateItem.EditionType))
                            newRateEditionType.EditionTypeName = rateItem.EditionType;
                        if (rateItem.Rate != null)
                            newRateEditionType.Rate = Convert.ToDecimal(rateItem.Rate);
                        if (rateItem.CPM != null)
                            newRateEditionType.CPM = Convert.ToDecimal(rateItem.CPM);
                        if (rateItem.RateBaseCirculationGuarantee != null)
                            newRateEditionType.RateBaseCirculationGuarantee = Convert.ToDecimal(rateItem.RateBaseCirculationGuarantee / 1000.00); // For display
                        if (rateItem.AveragePrintRun != null)
                            newRateEditionType.AveragePrintRun = Convert.ToDecimal(rateItem.AveragePrintRun / 1000.00); // For display
                        if (rateItem.OpenProductionCost != null)
                            newRateEditionType.OpenProductionCost = Convert.ToInt32(rateItem.OpenProductionCost);
                        if (rateItem.EarnedProductionCost != null)
                            newRateEditionType.EarnedProductionCost = Convert.ToInt32(rateItem.EarnedProductionCost);
                        if (rateItem.EarnedNEP != null)
                            newRateEditionType.EarnedNEP = Convert.ToDecimal(rateItem.EarnedNEP);

                        newRateTier.RateEditionTypes.Add(newRateEditionType);

                        if (recordCount == recordTotal)
                        {
                            newRateAdType.RateTiers.Add(newRateTier);
                            newRateRateType.RateAdTypes.Add(newRateAdType);
                            newParentADType.RateRateTypes.Add(newRateRateType);
                            lRatesContainer.RateParentAdTypes.Add(newParentADType);
                        }
                    }

                    if (lRatesContainer.RateParentAdTypes.Any())
                    {
                        lResult = lRatesContainer;
                    }

                    return lResult;
                }
            }
            catch (Exception ex)
            {

                var lReturnMessage = String.Format("Error in Repository GetRates method.  BookID: {0}.", lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}. INNER EXCEPTION: {1}  SOURCE: {2}. STACKTRACE: {3}.", ex.Message, ex.InnerException, ex.Source, ex.StackTrace);

                if (ex.InnerException != null)
                {
                    if (((System.Data.SqlClient.SqlException)ex.InnerException).Procedure != null)
                    {
                        var lSQLError = String.Format("SQL ERROR INFORMATION: PROCEDURE: {0}.  LINE NUMBER: {1}.", ((System.Data.SqlClient.SqlException)ex.InnerException).Procedure, ((System.Data.SqlClient.SqlException)ex.InnerException).LineNumber);
                        lLogMsg = lSQLError + lLogMsg;
                    }
                }
                logger.Error(lLogMsg);
                throw new Exception(lReturnMessage);
            }
        }
    }
}