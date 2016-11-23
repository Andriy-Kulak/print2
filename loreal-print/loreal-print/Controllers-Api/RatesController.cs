using loreal_print.MEC_Common.Security;
using loreal_print.Models;
using loreal_print.Models.Rates;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace loreal_print.Controllers_Api
{
    public class RatesController : ApiController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Route("api/Rates/GetRates")]
        [HttpGet]
        public IHttpActionResult GetRates()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lRatesContainer = new RatesModel();

            try
            {
                var ReposRates = new Repository.Rates();
                lRatesContainer = ReposRates.GetRates();

                if (lRatesContainer.RateParentAdTypes.Any())
                    lResult = Ok(lRatesContainer);
                else
                    lResult = Ok();

                return lResult;
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in RatesController GetRates.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}. INNER EXCEPTION: {1}  SOURCE: {2}. STACKTRACE: {3}.", ex.Message, ex.InnerException, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
                return lResult;
            }
        }

        // POST api/<controller>
        [Route("api/Rates/saveRates/RatesModel")]
        [Authorize]
        public IHttpActionResult saveRates(RatesModel RatesModel)
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;

            if (RatesModel != null)
            {
                if (ModelState.IsValid)
                {
                    // Get BookID, VersionID & Year from Cookie
                    var pContext = System.Web.HttpContext.Current;
                    var lCookieData = CookieManagement.GetCookie(pContext);
                    var lBookID = Convert.ToInt32(lCookieData.BookID);
                    var lParentAdTypes = new List<string>();
                    var lRateTypes = new List<string>();
                    var lRateBaseCircResult = int.MinValue;

                    try
                    {
                        foreach (var parentAdType in RatesModel.RateParentAdTypes)
                        {
                            var lRateTypeID = parentAdType.RateRateTypes[0].RateTypeID;

                            // Check ParentAdType Class level values.
                            var lOpenRateResult = 0;
                            // For ParentAdType 1(P4C) only
                            if (parentAdType.ParentAdTypeID == 1)
                            {
                                // Save/Update Premium Discount Rate values               
                                var lPDRResult = SavePremiumDiscountRate(parentAdType, lBookID, lCookieData.VersionID, lCookieData.Year);

                                // Save Open Rate Metrics
                                if (parentAdType.RateRateTypes[0].RateTypeName == "General")
                                {
                                    var lAdType = parentAdType.RateRateTypes[0].RateAdTypes[1];
                                    if (lAdType.AdTypeID == 17)
                                        lOpenRateResult = saveOpenRate(parentAdType.RateRateTypes[0].RateAdTypes[1], lRateTypeID, lAdType.AdTypeID, 1);

                                    if (parentAdType.RateRateTypes[0].RateAdTypes[2] != null)
                                    {
                                        lAdType = parentAdType.RateRateTypes[0].RateAdTypes[2];
                                        if (lAdType.AdTypeID == 18)
                                            lOpenRateResult = saveOpenRate(parentAdType.RateRateTypes[0].RateAdTypes[2], lRateTypeID, lAdType.AdTypeID, 1);
                                    }
                                }
                                if (parentAdType.RateRateTypes.Count > 1)
                                {
                                    if (parentAdType.RateRateTypes[1].RateTypeName == "Retail")
                                    {
                                        lRateTypeID = parentAdType.RateRateTypes[1].RateTypeID;
                                        var lAdType = parentAdType.RateRateTypes[1].RateAdTypes[1];
                                        if (lAdType.AdTypeID == 17)
                                            lOpenRateResult = saveOpenRate(lAdType, lRateTypeID, lAdType.AdTypeID, 1);
                                        //lOpenRateResult = saveOpenRate(parentAdType.RateRateTypes[1].RateAdTypes[1], lRateTypeID, lAdType.AdTypeID, 1);

                                        if (parentAdType.RateRateTypes[0].RateAdTypes[2] != null)
                                        {
                                            lAdType = parentAdType.RateRateTypes[1].RateAdTypes[2];
                                            if (lAdType.AdTypeID == 18)
                                                lOpenRateResult = saveOpenRate(lAdType, lRateTypeID, lAdType.AdTypeID, 1);
                                            //lOpenRateResult = saveOpenRate(parentAdType.RateRateTypes[1].RateAdTypes[2], lRateTypeID, lAdType.AdTypeID, 1);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                foreach (var rateAdType in parentAdType.RateRateTypes[0].RateAdTypes)
                                {
                                    foreach (var editionType in rateAdType.RateTiers[0].RateEditionTypes)
                                    {
                                        lOpenRateResult = saveOpenRate(rateAdType, lRateTypeID, rateAdType.AdTypeID, editionType.EditionTypeID);
                                    }
                                }
                            }


                            //if (parentAdType.ParentAdTypeID == 4)
                            //{
                            // Go over with Negmat on Monday.
                            //
                            //}

                            // Save Earned Rates
                            var lEarnedRatesResult = int.MinValue;
                            var lCount = 0;
                            var lRTypeID = parentAdType.RateRateTypes[lCount].RateTypeID; // General
                                                                                          //var lATypeID = int.MinValue;
                                                                                          //if (parentAdType.ParentAdTypeID == 1)
                                                                                          //    lATypeID = parentAdType.RateRateTypes[lCount].RateAdTypes[1].AdTypeID;
                                                                                          //else
                                                                                          //    lATypeID = parentAdType.RateRateTypes[lCount].RateAdTypes[0].AdTypeID;
                                                                                          //var adTypeIDArray = new List<int>();

                            lEarnedRatesResult = saveEarnedRates(parentAdType.RateRateTypes, parentAdType.ParentAdTypeID);

                            /* Save/Update RateBaseCirculationGuarantee & Rate
                             * We only do this for:
                             * ParentAdType == 1
                             *    We add CirculationGuarantee & AveragePrintRun for both Edition Types
                             * ParentAdType == 4
                             *     We take the CirculationGuarantee & AveragePrintRun values added for Scent Strips (ParentAdtype 4) and INSERT/Update these records for
                             *     1.  BRC
                             *     2.  INSERT */

                            var lRateEditionTypes = new List<RateEditionType>();
                            if (parentAdType.ParentAdTypeID == 1)
                            {
                                lRateEditionTypes = parentAdType.RateRateTypes[0].RateAdTypes[1].RateTiers[0].RateEditionTypes;

                                foreach (var rateEditionType in lRateEditionTypes)
                                {
                                    if (rateEditionType.RateBaseCirculationGuarantee != null)
                                    {
                                        lRateBaseCircResult = saveGuaranteedRateBase(parentAdType.ParentAdTypeID, rateEditionType);
                                    }

                                    // Save Average Print Run
                                    if (rateEditionType.AveragePrintRun != null)
                                    {
                                        lRateBaseCircResult = saveAveragePrintRun(parentAdType.ParentAdTypeID, rateEditionType);
                                    }
                                }
                            }
                            else if (parentAdType.ParentAdTypeID == 4)  // Scent Strips
                            {
                                lRateEditionTypes = parentAdType.RateRateTypes[0].RateAdTypes[0].RateTiers[0].RateEditionTypes;
                                var lGuaranteedRateSubAvgs = saveGuaranteedRateSubAvgs(lRateEditionTypes);
                            }

                            //lRateEditionTypes = parentAdType.RateRateTypes[0].RateAdTypes[0].RateTiers[0].RateEditionTypes;

                            // Save ProductionCostNEP Values
                            var lProductionCostNEP = saveProductionCostNEPValues(parentAdType.RateRateTypes[0], parentAdType.ParentAdTypeID);

                            foreach (var rateType in parentAdType.RateRateTypes)
                            {
                                lParentAdTypes.Add(rateType.RateTypeName);
                            }
                        }

                        var lParentAdTypesArray = lParentAdTypes.ToArray();

                        if (lParentAdTypesArray.Any())
                        {
                            lResult = Ok(lParentAdTypesArray);
                        }
                    }
                    catch (Exception ex)
                    {
                        var lReturnMessage = String.Format("Error in saveRates.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                        lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                        logger.Error(lLogMsg);
                        lResult = BadRequest(lReturnMessage);
                        return lResult;
                    }
                }
                else
                {
                    string errorMessages = string.Join("; ", ModelState.Values
                                                            .SelectMany(e => e.Errors)
                                                            .Select(e => e.ErrorMessage));

                    lResult = BadRequest(errorMessages);
                }
            }
            else
            {
                lResult = BadRequest("No Rates data was sent.  Please try again.");
            }
            return lResult;
        }

        [Route("api/Rates/getGuaranteedRateBase")]
        [Authorize]
        [HttpGet()]
        public IHttpActionResult getGuaranteedRateBase()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lParentAdTypeID = 1;
            var lEditionTypeID = 1;

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Enter the RateBaseCirculationGuarantee value.
                    var lRateBaseCirculationGuarantee = db.CirculationGuarantees
                        .Where(c => (c.Year == lCookieData.Year
                        && c.BookID == lCookieData.BookID
                        && c.VersionID == lCookieData.VersionID
                        && c.ParentAdTypeID == lParentAdTypeID
                        && c.EditionTypeID == lEditionTypeID))
                        .Select(r => r.RateBaseCirculationGuarantee)
                        .FirstOrDefault();

                    if (lRateBaseCirculationGuarantee > -1)
                    {
                        lResult = Ok(lRateBaseCirculationGuarantee);
                    }
                    else
                    {
                        lResult = BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in getGuaranteedRateBase.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
                return lResult;
            }
            return lResult;
        }

        private bool SavePremiumDiscountRate(RateParentAdType pRateParentAdType, int pBookID, int pVersionID, string pYear)
        {
            var lLogMsg = string.Empty;
            var lResult = false;
            var isUpdate = false;
            var lPremiumDiscountRateID = -1;

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Enter the RateBaseCirculationGuarantee value.
                    var lExistingRecord = db.PremiumDiscountRates
                        .Where(p => (p.Year == pYear
                        && p.BookID == pBookID
                        && p.VersionID == pVersionID)).FirstOrDefault();

                    if (lExistingRecord != null)
                        isUpdate = true;

                    var lPremDiscRateRecord = new PremiumDiscountRate();

                    // AdvertorialEarnedPercentDiscount
                    if (pRateParentAdType.AdvertorialEarnedPercentDiscount != null)
                    {
                        if (isUpdate)
                            lExistingRecord.AdvertorialEarnedPercentDiscount = Convert.ToDecimal(pRateParentAdType.AdvertorialEarnedPercentDiscount);
                        else
                            lPremDiscRateRecord.AdvertorialEarnedPercentDiscount = Convert.ToDecimal(pRateParentAdType.AdvertorialEarnedPercentDiscount);
                    }
                    // BleedOpenPercentPremium
                    if (pRateParentAdType.BleedOpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.BleedOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.BleedOpenPercentPremium);
                        else
                            lPremDiscRateRecord.BleedOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.BleedOpenPercentPremium);
                    }
                    // BleedEarnedPercentPremium
                    if (pRateParentAdType.BleedEarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.BleedEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.BleedEarnedPercentPremium);
                        else
                            lPremDiscRateRecord.BleedEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.BleedEarnedPercentPremium);
                    }
                    // Cover2OpenPercentPremium
                    if (pRateParentAdType.Cover2OpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.Cover2OpenPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover2OpenPercentPremium);
                        else
                            lPremDiscRateRecord.Cover2OpenPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover2OpenPercentPremium);
                    }
                    // Cover2EarnedPercentPremium
                    if (pRateParentAdType.Cover2EarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.Cover2EarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover2EarnedPercentPremium);
                        else
                            lPremDiscRateRecord.Cover2EarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover2EarnedPercentPremium);
                    }
                    // Cover3OpenPercentPremium
                    if (pRateParentAdType.Cover3OpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.Cover3OpenPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover3OpenPercentPremium);
                        else
                            lPremDiscRateRecord.Cover3OpenPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover3OpenPercentPremium);
                    }
                    // Cover3EarnedPercentPremium
                    if (pRateParentAdType.Cover3EarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.Cover3EarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover3EarnedPercentPremium);
                        else
                            lPremDiscRateRecord.Cover3EarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover3EarnedPercentPremium);
                    }
                    // Cover4OpenPercentPremium
                    if (pRateParentAdType.Cover4OpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.Cover4OpenPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover4OpenPercentPremium);
                        else
                            lPremDiscRateRecord.Cover4OpenPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover4OpenPercentPremium);
                    }
                    // Cover4EarnedPercentPremium
                    if (pRateParentAdType.Cover4EarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.Cover4EarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover4EarnedPercentPremium);
                        else
                            lPremDiscRateRecord.Cover4EarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.Cover4EarnedPercentPremium);
                    }
                    // FracHalfPageOpenPercentPremium
                    if (pRateParentAdType.FracHalfPageOpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FracHalfPageOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.FracHalfPageOpenPercentPremium);
                        else
                            lPremDiscRateRecord.FracHalfPageOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.FracHalfPageOpenPercentPremium);
                    }
                    // FracHalfPageEarnedPercentPremium
                    if (pRateParentAdType.FracHalfPageEarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FracHalfPageEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.FracHalfPageEarnedPercentPremium);
                        else
                            lPremDiscRateRecord.FracHalfPageEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.FracHalfPageEarnedPercentPremium);
                    }
                    // FracThirdPageOpenPercentPremium
                    if (pRateParentAdType.FracThirdPageOpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FracThirdPageOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdPageOpenPercentPremium);
                        else
                            lPremDiscRateRecord.FracThirdPageOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdPageOpenPercentPremium);
                    }
                    // FracThirdPageEarnedPercentPremium
                    if (pRateParentAdType.FracThirdPageEarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FracThirdPageEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdPageEarnedPercentPremium);
                        else
                            lPremDiscRateRecord.FracThirdPageEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdPageEarnedPercentPremium);
                    }
                    // FracThirdRunOppFBPOpenPercentPremium
                    if (pRateParentAdType.FracThirdRunOppFBPOpenPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FracThirdRunOppFBPOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdRunOppFBPOpenPercentPremium);
                        else
                            lPremDiscRateRecord.FracThirdRunOppFBPOpenPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdRunOppFBPOpenPercentPremium);
                    }
                    // FracThirdRunOppFBPEarnedPercentPremium
                    if (pRateParentAdType.FracThirdRunOppFBPEarnedPercentPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FracThirdRunOppFBPEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdRunOppFBPEarnedPercentPremium);
                        else
                            lPremDiscRateRecord.FracThirdRunOppFBPEarnedPercentPremium = Convert.ToDecimal(pRateParentAdType.FracThirdRunOppFBPEarnedPercentPremium);
                    }
                    // SpreadC2P1EarnedPercentDiscount
                    if (pRateParentAdType.SpreadC2P1EarnedPercentDiscount != null)
                    {
                        if (isUpdate)
                            lExistingRecord.SpreadC2P1EarnedPercentDiscount = Convert.ToDecimal(pRateParentAdType.SpreadC2P1EarnedPercentDiscount);
                        else
                            lPremDiscRateRecord.SpreadC2P1EarnedPercentDiscount = Convert.ToDecimal(pRateParentAdType.SpreadC2P1EarnedPercentDiscount);
                    }
                    // SpreadROBEarnedPercentDiscount
                    if (pRateParentAdType.SpreadROBEarnedPercentDiscount != null)
                    {
                        if (isUpdate)
                            lExistingRecord.SpreadROBEarnedPercentDiscount = Convert.ToDecimal(pRateParentAdType.SpreadROBEarnedPercentDiscount);
                        else
                            lPremDiscRateRecord.SpreadROBEarnedPercentDiscount = Convert.ToDecimal(pRateParentAdType.SpreadROBEarnedPercentDiscount);
                    }
                    // FifthColorMetallicOpenDollarPremium
                    if (pRateParentAdType.FifthColorMetallicOpenDollarPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FifthColorMetallicOpenDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMetallicOpenDollarPremium);
                        else
                            lPremDiscRateRecord.FifthColorMetallicOpenDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMetallicOpenDollarPremium);
                    }
                    // FifthColorMetallicEarnedDollarPremium
                    if (pRateParentAdType.FifthColorMetallicEarnedDollarPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FifthColorMetallicEarnedDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMetallicEarnedDollarPremium);
                        else
                            lPremDiscRateRecord.FifthColorMetallicEarnedDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMetallicEarnedDollarPremium);
                    }
                    // FifthColorMatchOpenDollarPremium
                    if (pRateParentAdType.FifthColorMatchOpenDollarPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FifthColorMatchOpenDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMatchOpenDollarPremium);
                        else
                            lPremDiscRateRecord.FifthColorMatchOpenDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMatchOpenDollarPremium);
                    }
                    // FifthColorMatchEarnedDollarPremium
                    if (pRateParentAdType.FifthColorMatchEarnedDollarPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FifthColorMatchEarnedDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMatchEarnedDollarPremium);
                        else
                            lPremDiscRateRecord.FifthColorMatchEarnedDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorMatchEarnedDollarPremium);
                    }
                    // FifthColorPMSOpenDollarPremium
                    if (pRateParentAdType.FifthColorPMSOpenDollarPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FifthColorPMSOpenDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorPMSOpenDollarPremium);
                        else
                            lPremDiscRateRecord.FifthColorPMSOpenDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorPMSOpenDollarPremium);
                    }
                    // FifthColorPMSEarnedDollarPremium
                    if (pRateParentAdType.FifthColorPMSEarnedDollarPremium != null)
                    {
                        if (isUpdate)
                            lExistingRecord.FifthColorPMSEarnedDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorPMSEarnedDollarPremium);
                        else
                            lPremDiscRateRecord.FifthColorPMSEarnedDollarPremium = Convert.ToInt32(pRateParentAdType.FifthColorPMSEarnedDollarPremium);
                    }

                    if (isUpdate == false)
                    {
                        // Create
                        lPremDiscRateRecord.Load_Date = DateTime.Now;
                        lPremDiscRateRecord.BookID = pBookID;
                        lPremDiscRateRecord.VersionID = pVersionID;
                        lPremDiscRateRecord.Year = pYear;
                        db.PremiumDiscountRates.Add(lPremDiscRateRecord);
                    }
                    else
                    {
                        // Update
                        lExistingRecord.VersionID = pVersionID;
                        db.Entry(lExistingRecord).State = EntityState.Modified;
                    }
                    lPremiumDiscountRateID = db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                lLogMsg = String.Format("Error in SavePremiumDiscountRate.  ParentAdType: {0}, BookID: {1}.  ERROR MESSAGE: {2}.  SOURCE: {3}. STACKTRACE: {4}.", pRateParentAdType, pBookID, ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            if (lPremiumDiscountRateID > int.MinValue)
            {
                lResult = true;
            }
            return lResult;
        }

        [Route("api/Rates/getGuaranteedRateSubAvgs")]
        [Authorize]
        [HttpGet]
        public IHttpActionResult getGuaranteedRateSubAvgs()
        {
            /* Return the six records associated with the BookID, Year & Version
             * for ParentAdTypes:
             * 1. P4C - 1
             * 2. Insert - 2
             * 2. BRC Card - 3
             * 3. Scent Strip - 4 */
            const string FULLRUN = "Full Run";
            const string SUBSONLY = "Subscriptions Only";
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            int[] lParents = new int[] { 2, 3, 4 };
            var lCircGuars = new List<RateSubAvg>();

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Retrieve records
                    var lResults = db.CirculationGuarantees
                        .Where(c => (c.Year == lCookieData.Year
                        && c.BookID == lCookieData.BookID
                        && c.VersionID == lCookieData.VersionID
                        && lParents.Contains(c.ParentAdTypeID)))
                        .OrderBy(c => c.ParentAdTypeID)
                        .ThenBy(c => c.EditionTypeID)
                        .Select(c => new { c.EditionTypeID, c.AveragePrintRun, c.RateBaseCirculationGuarantee })
                        .ToList();

                    if (lResults.Any())
                    {
                        foreach (var record in lResults)
                        {
                            var item = new RateSubAvg
                            {
                                EditionType = record.EditionTypeID == 1 ? FULLRUN : SUBSONLY,
                                AveragePrintRun = Convert.ToInt32(record.AveragePrintRun),
                                RateBaseCirculationGuarantee = Convert.ToInt32(record.RateBaseCirculationGuarantee)
                            };
                            lCircGuars.Add(item);
                        }
                        lResult = Ok(lCircGuars);
                    }
                    lResult = Ok();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in getGuaranteedRateSubAvgs.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            return lResult;
        }

        [Route("api/Rates/getOpenRate/{type}")]
        [Authorize]
        [HttpGet()]
        public IHttpActionResult getOpenRate(string type)
        {
            const string P4C = "P4C";
            var lAdTypeID = int.MinValue;
            var lOpenRate = new RateOpen();

            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;

            // Determine whether P4C or P4CB
            if (type == P4C)
                lAdTypeID = 17;
            else
                lAdTypeID = 18;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lEditionTypeID = 1;

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    var lRate = db.Rates
                        .Where(c => (c.Year == lCookieData.Year
                        && c.BookID == lCookieData.BookID
                        && c.VersionID == lCookieData.VersionID
                        && c.AdTypeID == lAdTypeID
                        && c.EditionTypeID == lEditionTypeID))
                        .FirstOrDefault();

                    if (lRate != null)
                    {
                        lOpenRate.CPM = Convert.ToDecimal(lRate.CPM);
                        lOpenRate.Rate = Convert.ToDecimal(lRate.Rate1);
                        lOpenRate.Type = type;
                        lResult = Ok(lOpenRate);
                    }
                    else
                    {
                        lResult = BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in getOpenRate. TYPE: {0} Publisher: {1}, Book: {2}, BookID: {3}.", type, lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }
            return lResult;
        }

        [HttpGet()]
        [Route("api/Rates/getDiscountStructure")]
        [Authorize]
        public IHttpActionResult getDiscountStructure()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var lDiscountStructure = new PremiumDiscountRate();
            var pContext = System.Web.HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    lDiscountStructure = db.PremiumDiscountRates
                        .Where(p => p.BookID == lCookieData.BookID && p.Year == lCookieData.Year && p.VersionID == lCookieData.VersionID)
                        .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in getDiscountStructure.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
            }

            if (lDiscountStructure != null)
            {
                lResult = Ok(lDiscountStructure);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        [HttpGet()]
        [Route("api/Rates/getScentStripAdTypesOpen")]
        [Authorize]
        public IHttpActionResult getScentStripAdTypesOpen()
        {
            IHttpActionResult lResult = null;
            var lLogMsg = string.Empty;
            var pContext = System.Web.HttpContext.Current;
            var lParentAdTypeID = 4; // Scent Strip
            var lAdTypes = new List<RateAdType>();

            // Get Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Retrieve records
                    var lResults = db.AdTypes
                        .Where(a => a.Year == lCookieData.Year && a.ParentAdTypeID == lParentAdTypeID)
                        .OrderBy(a => a.AdTypeID)
                        .Select(a => new { a.AdTypeID, a.AdType1 })
                        .ToList();

                    foreach (var item in lResults)
                    {
                        var lAdType = new RateAdType
                        {
                            AdTypeID = item.AdTypeID,
                            AdType = item.AdType1
                        };
                        lAdTypes.Add(lAdType);
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in getScentStripAdTypesOpen.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                lResult = BadRequest(lReturnMessage);
                return lResult;
            }
            if (lAdTypes.Any())
            {
                lResult = Ok(lAdTypes);
            }
            else
            {
                lResult = Ok();
            }
            return lResult;
        }

        /* *** PRIVATE RATE SAVE METHODS *** */
        private int saveGuaranteedRateSubAvgs(List<RateEditionType> pEditionTypes)
        {
            // TODO: Refactor and merge with saveGuaranteedRateBase.
            /* This is for the Page Rate Guarranteed Rate - for Subscription & Full Run - 
             * for ParentAdTypes:
             * 1. Insert - 2
             * 2. BRC Card - 3
             * 3. Scent Strip - 4 
             * These records/values go in lockstep - the user enters:
             * - RateBaseCirculationGuarantee
             * - AveragePrintRun
             * values for Edition Types (four values total):
             * - Full Run
             * - Subscriptions Only
             * for each of the ParentAdTypes.
             * This results two records for each of the ParentAdTypes:
             * - 6 records total
             * - 12 values (RateBaseCirculationGuarantee & AveragePrintRun) total */

            var lResult = int.MinValue;
            var lCirculationGuaranteeID = int.MinValue;
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            //var lParentAdTypeID = 2;
            //var lEditionTypeID = 1;
            var lRate = new RateSubAvg();

            try
            {
                // We only need to find one of the six records because they are always entered in lockstep


                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    foreach (var editionType in pEditionTypes)
                    {
                        if (editionType.AveragePrintRun != null || editionType.RateBaseCirculationGuarantee != null)
                        {
                            // Create three Records
                            for (int p = 2; p < 5; p++)
                            {
                                var lCircGuar = new CirculationGuarantee
                                {
                                    Year = lCookieData.Year,
                                    BookID = Convert.ToInt32(lCookieData.BookID),
                                    VersionID = lCookieData.VersionID,
                                    ParentAdTypeID = p,
                                    EditionTypeID = editionType.EditionTypeID,
                                    Load_Date = DateTime.Now
                            };

                                var lUpdate = db.CirculationGuarantees
                                    .Where(c => (c.Year == lCircGuar.Year
                                    && c.BookID == lCircGuar.BookID
                                    && c.VersionID == lCircGuar.VersionID
                                    && c.ParentAdTypeID == lCircGuar.ParentAdTypeID
                                    && c.EditionTypeID == lCircGuar.EditionTypeID)).FirstOrDefault();

                                if (editionType.AveragePrintRun != null)
                                    lCircGuar.AveragePrintRun = Convert.ToInt32(1000 * editionType.AveragePrintRun);
                                if (editionType.RateBaseCirculationGuarantee != null)
                                    lCircGuar.RateBaseCirculationGuarantee = Convert.ToInt32(1000 * editionType.RateBaseCirculationGuarantee);

                                if (lUpdate != null)
                                {
                                    //db.Entry(lCircGuar).State = EntityState.Modified;
                                    if (editionType.AveragePrintRun != null)
                                        lUpdate.AveragePrintRun = Convert.ToInt32(1000 * editionType.AveragePrintRun);
                                    if (editionType.RateBaseCirculationGuarantee != null)
                                        lUpdate.RateBaseCirculationGuarantee = Convert.ToInt32(1000 * editionType.RateBaseCirculationGuarantee);
                                }                                    
                                else
                                    db.CirculationGuarantees.Add(lCircGuar);
                            }
                        }
                    }
                    lCirculationGuaranteeID = db.SaveChanges();
                }
                // What do we want to return here?
                lResult = lCirculationGuaranteeID;
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveGuaranteedRateSubAvgs.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lResult;
        }

        private int saveGuaranteedRateBase(int pParentAdTypeID, RateEditionType pEditionMetrics)
        {
            // This is for the Page Rate Guarranteed Rate Base
            var lResult = int.MinValue;
            var lCirculationGuaranteeID = int.MinValue;
            var lRateBase = Convert.ToInt32(1000 * pEditionMetrics.RateBaseCirculationGuarantee);
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            var lCircGuar = new CirculationGuarantee
            {
                Year = lCookieData.Year,
                BookID = Convert.ToInt32(lCookieData.BookID),
                Load_Date = DateTime.Now,
                ParentAdTypeID = pParentAdTypeID,
                EditionTypeID = pEditionMetrics.EditionTypeID,
                RateBaseCirculationGuarantee = lRateBase,
                VersionID = lCookieData.VersionID
            };

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Enter the RateBaseCirculationGuarantee value.
                    var lCircUpdate = db.CirculationGuarantees
                        .Where(c => (c.Year == lCircGuar.Year
                        && c.BookID == lCircGuar.BookID
                        && c.VersionID == lCircGuar.VersionID
                        && c.ParentAdTypeID == lCircGuar.ParentAdTypeID
                        && c.EditionTypeID == lCircGuar.EditionTypeID)).FirstOrDefault();

                    if (lCircUpdate != null)
                    {
                        // Update
                        lCircUpdate.RateBaseCirculationGuarantee = lRateBase;
                        lCircUpdate.VersionID = lCookieData.VersionID;
                    }
                    else
                    {
                        // Create
                        db.CirculationGuarantees.Add(lCircGuar);
                    }
                    lCirculationGuaranteeID = db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveGuaranteedRateBase.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            lResult = lCirculationGuaranteeID;

            return lResult;
        }

        private int saveAveragePrintRun(int pParentAdTypeID, RateEditionType pEditionMetrics)
        {
            // This is for the Page Rate Guarranteed Rate Base
            var lResult = int.MinValue;
            var lAveragePrintRunID = int.MinValue;
            var lAveragePrintRun = Convert.ToInt32(1000 * pEditionMetrics.AveragePrintRun);
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                var lCircGuar = new CirculationGuarantee
                {
                    Year = lCookieData.Year,
                    BookID = Convert.ToInt32(lCookieData.BookID),
                    ParentAdTypeID = pParentAdTypeID,
                    EditionTypeID = pEditionMetrics.EditionTypeID,
                    AveragePrintRun = lAveragePrintRun,
                    VersionID = lCookieData.VersionID
                };

                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Enter the RateBaseCirculationGuarantee value.
                    var lCircUpdate = db.CirculationGuarantees
                        .Where(c => (c.Year == lCircGuar.Year
                        && c.BookID == lCircGuar.BookID
                        && c.VersionID == lCircGuar.VersionID
                        && c.ParentAdTypeID == lCircGuar.ParentAdTypeID
                        && c.EditionTypeID == lCircGuar.EditionTypeID)).FirstOrDefault();

                    if (lCircUpdate != null)
                    {
                        // Update
                        lCircUpdate.AveragePrintRun = lAveragePrintRun;
                        lCircUpdate.VersionID = lCookieData.VersionID;
                    }
                    else
                    {
                        // Create
                        db.CirculationGuarantees.Add(lCircGuar);
                    }
                    lAveragePrintRunID = db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveAveragePrintRun.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            lResult = lAveragePrintRunID;

            return lResult;
        }

        private int saveOpenRate(RateAdType pRateAdType, int pRateTypeID, int pAdTypeID, int pEditionTypeID)
        {
            var lLogMsg = string.Empty;
            var lResult = int.MinValue;
            var lRateID = int.MinValue;
            var lRate = decimal.MinValue;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            var lTierID = 0;

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    var lUpdateRate = db.Rates
                        .Where(r => (r.Year == lCookieData.Year
                        && r.BookID == lCookieData.BookID
                        && r.VersionID == lCookieData.VersionID
                        && r.RateTypeID == pRateTypeID
                        && r.AdTypeID == pAdTypeID
                        && r.TierID == lTierID
                        && r.EditionTypeID == pEditionTypeID)).FirstOrDefault();

                    if (pEditionTypeID == 1)
                    {
                        // Check for NULL
                        if (pRateAdType.RateTiers[0].RateEditionTypes[0].Rate == null && lUpdateRate == null)
                        {
                            // Negmat - TODO
                            // 1.  Record doesn't exist - user didn't enter Rate value
                            return lResult;

                            // 2.  Record exists - user wants to remove Rate altogether.
                            // TODO
                        }
                        else
                            lRate = Convert.ToDecimal(pRateAdType.RateTiers[0].RateEditionTypes[0].Rate);
                    }
                    else
                    {
                        if (pRateAdType.RateTiers[0].RateEditionTypes[1].Rate == null && lUpdateRate == null)
                        {
                            // 1.  Record doesn't exist - user didn't enter Rate value
                            return lResult;

                            // 2.  Record exists - user wants to remove Rate altogether.
                            // TODO
                        }
                        else
                            lRate = Convert.ToDecimal(pRateAdType.RateTiers[0].RateEditionTypes[1].Rate);
                    }

                    if (lUpdateRate != null)
                    {
                        // Update
                        lUpdateRate.Rate1 = lRate;
                        lUpdateRate.VersionID = lCookieData.VersionID;
                        //db.Entry(lUpdateRate).State = EntityState.Modified;
                    }
                    else
                    {
                        // Create
                        if (lRate >= 0)
                        {
                            var lRateOpen = new Rate
                            {
                                Year = lCookieData.Year,
                                BookID = Convert.ToInt32(lCookieData.BookID),
                                AdTypeID = pAdTypeID,
                                EditionTypeID = pEditionTypeID,
                                RateTypeID = pRateTypeID,
                                TierID = lTierID,
                                Load_Date = DateTime.Now,
                                IsOverriden = "No",
                                Rate1 = lRate,
                                VersionID = lCookieData.VersionID
                            };
                            db.Rates.Add(lRateOpen);
                        }
                    }
                    lRateID = db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveOpenRate.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }

            lResult = lRateID;
            return lResult;
        }

        private int saveProductionCostNEPValues(RateRateType pRateType, int pParentAdType)
        {
            var lResult = int.MinValue;
            var lProductCostNEPID = int.MinValue;
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    foreach (var adType in pRateType.RateAdTypes)
                    {
                        foreach (var editionType in adType.RateTiers[0].RateEditionTypes)
                        {
                            /* First - do any of the following fields have a value?
                             * 1.  EarnedProductionCost
                             * 2.  EarnedNEP
                             * 3.  OpenProductionCost */

                            // Does the record already exist?
                            var lUpdateNEP = db.ProductionCostNEPs
                                .Where(p => (p.Year == lCookieData.Year
                                && p.BookID == lCookieData.BookID
                                && p.VersionID == lCookieData.VersionID
                                && p.AdTypeID == adType.AdTypeID
                                && p.EditionTypeID == editionType.EditionTypeID)).FirstOrDefault();

                            if (lUpdateNEP != null)
                            {
                                // Update
                                if (editionType.EarnedProductionCost != null)
                                    lUpdateNEP.EarnedProductionCost = Convert.ToInt32(editionType.EarnedProductionCost);
                                if (editionType.EarnedNEP != null)
                                    lUpdateNEP.EarnedNEP = Convert.ToDecimal(editionType.EarnedNEP);
                                if (editionType.OpenProductionCost != null)
                                    lUpdateNEP.OpenProductionCost = Convert.ToInt32(editionType.OpenProductionCost);
                                lUpdateNEP.VersionID = lCookieData.VersionID;
                            }
                            else
                            {
                                if (editionType.EarnedProductionCost != null || editionType.EarnedNEP != null || editionType.OpenProductionCost != null)
                                {
                                    // Create
                                    var lCreateNEP = new ProductionCostNEP
                                    {
                                        Year = lCookieData.Year,
                                        BookID = Convert.ToInt32(lCookieData.BookID),
                                        AdTypeID = adType.AdTypeID,
                                        EditionTypeID = editionType.EditionTypeID,
                                        VersionID = lCookieData.VersionID,
                                        Load_Date = DateTime.Now
                                    };
                                    if (editionType.EarnedProductionCost != null)
                                        lCreateNEP.EarnedProductionCost = Convert.ToInt32(editionType.EarnedProductionCost);
                                    if (editionType.EarnedNEP != null)
                                        lCreateNEP.EarnedNEP = Convert.ToDecimal(editionType.EarnedNEP);
                                    if (editionType.OpenProductionCost != null)
                                        lCreateNEP.OpenProductionCost = Convert.ToInt32(editionType.OpenProductionCost);

                                    db.ProductionCostNEPs.Add(lCreateNEP);
                                }
                            }
                        }
                    }
                    lProductCostNEPID = db.SaveChanges();
                    lResult = lProductCostNEPID;
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveProductionCostNEPValues.  RateType: {0}, RateTypeID: {1}, ParentAdTypeID: {2}, Publisher: {3}, Book: {4}, BookID: {5}.", 
                    pRateType.RateTypeName, pRateType.RateTypeID, pParentAdType, lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lResult;
        }

        private int saveEarnedRates(List<RateRateType> pRateTypes, int pParentAdType)
        {
            var lResult = int.MinValue;
            var lRateID = int.MinValue;
            var lLogMsg = string.Empty;

            // Get BookID, VersionID & Year from Cookie
            var pContext = System.Web.HttpContext.Current;
            var lCookieData = CookieManagement.GetCookie(pContext);
            var lAdTypes = new List<RateAdType>();

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    // Check whether INSERT or UPDATE for each Tier
                    foreach (var rateType in pRateTypes)
                    {
                        if (pParentAdType != 1)
                            lAdTypes = rateType.RateAdTypes;
                        else
                            lAdTypes = rateType.RateAdTypes.Where(a => a.AdTypeID > 8).ToList();

                        foreach (var adType in lAdTypes)
                        {
                            foreach (var tier in adType.RateTiers)
                            {
                                foreach (var editionType in tier.RateEditionTypes)
                                {
                                    // Only EARNED Rates
                                    if (tier.TierID > 0)
                                    {
                                        // Does the record already exist?
                                        var lUpdateRate = db.Rates
                                            .Where(r => (r.Year == lCookieData.Year
                                            && r.BookID == lCookieData.BookID
                                            && r.VersionID == lCookieData.VersionID
                                            && r.RateTypeID == rateType.RateTypeID
                                            && r.AdTypeID == adType.AdTypeID
                                            && r.TierID == tier.TierID
                                            && r.EditionTypeID == editionType.EditionTypeID)).FirstOrDefault();

                                        if (lUpdateRate != null)
                                        {
                                            // Update
                                            lUpdateRate.Rate1 = Convert.ToDecimal(editionType.Rate);
                                            lUpdateRate.CPM = Convert.ToDecimal(editionType.CPM);
                                            lUpdateRate.VersionID = lCookieData.VersionID;
                                        }
                                        else
                                        {
                                            if (editionType.Rate != null || editionType.CPM != null)
                                            {
                                                // Create
                                                var lRateEarned = new Rate
                                                {
                                                    Year = lCookieData.Year,
                                                    BookID = Convert.ToInt32(lCookieData.BookID),
                                                    AdTypeID = adType.AdTypeID,
                                                    EditionTypeID = editionType.EditionTypeID,
                                                    RateTypeID = rateType.RateTypeID,
                                                    TierID = tier.TierID,
                                                    Load_Date = DateTime.Now,
                                                    IsOverriden = "No",
                                                    Rate1 = Convert.ToDecimal(editionType.Rate),
                                                    CPM = Convert.ToDecimal(editionType.CPM),
                                                    VersionID = lCookieData.VersionID
                                                };
                                                db.Rates.Add(lRateEarned);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    lRateID = db.SaveChanges();
                }
                lResult = lRateID;
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in saveEarnedRates. Publisher: {0}, ParentAdTypeID: {1}, Book: {2}, BookID: {3}.", lCookieData.Publisher, pParentAdType, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
            }
            return lResult;
        }
    }
}
