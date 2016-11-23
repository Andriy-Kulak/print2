using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using System.Web;
using loreal_print.Models.RatesReport;
using loreal_print.MEC_Common.Security;
using loreal_print.Models;

namespace loreal_print.Repository
{
    public class RatesReport
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public RatesReportModel GetRatesReport()
        {
            var lResult = new RatesReportModel();
            var lLogMsg = string.Empty;
            var lRatesReportOutput = new List<RatesReportModel>();
            var newRatesReportOutput = new RatesReportModel();
            var newRatesReportRateType = new RatesReportRateType();
            var newRatesReportAdType = new RatesReportAdType();
            var lRatesReportAdTypeList = new List<RatesReportAdType>();
            var newRatesReportTier = new RatesReportTier();
            var lRatesReportTierList = new List<RatesReportTier>();
            var pContext = System.Web.HttpContext.Current;
            var isRateOpen = false;
            var isAdTypeOpen = false;
            var isTierOpen = false;
            var lRateTypeID = 0;
            var lAdTypeID = 0;
            var lTierID = -1;
            var recordCount = 0;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    var lRatesReportRecords = db.GetRatesEndReport(lCookieData.Year, lCookieData.BookID, lCookieData.VersionID)
                    .ToList();

                    var recordTotal = lRatesReportRecords.Count;

                    newRatesReportOutput.BookID = Convert.ToInt32(lCookieData.BookID);
                    newRatesReportOutput.VersionID = lCookieData.VersionID;
                    newRatesReportOutput.Year = lCookieData.Year;

                    foreach (var reportItem in lRatesReportRecords)
                    {
                        recordCount++;

                        if (lRateTypeID != reportItem.RateTypeID)
                        {
                            lRateTypeID = Convert.ToInt32(reportItem.RateTypeID);

                            lAdTypeID = 0;
                            lTierID = -1;

                            if (isTierOpen)
                            {
                                newRatesReportAdType.Tiers.Add(newRatesReportTier);
                                isTierOpen = false;
                            }

                            if (isAdTypeOpen)
                            {
                                newRatesReportRateType.AdTypes.Add(newRatesReportAdType);
                                isAdTypeOpen = false;
                            }

                            if (isRateOpen)
                            {
                                newRatesReportOutput.RateTypes.Add(newRatesReportRateType);
                                isRateOpen = false;
                            }

                            newRatesReportRateType = new RatesReportRateType
                            {
                                RateType = reportItem.RateType,
                                RateTypeID = Convert.ToInt32(reportItem.RateTypeID)
                            };
                            isRateOpen = true;
                        }

                        // AdTypes
                        if (lAdTypeID != reportItem.AdTypeID)
                        {
                            lAdTypeID = Convert.ToInt32(reportItem.AdTypeID);

                            if (isTierOpen)
                            {
                                newRatesReportAdType.Tiers.Add(newRatesReportTier);
                                isTierOpen = false;
                            }
                            lTierID = -1;

                            if (isAdTypeOpen)
                            {
                                newRatesReportRateType.AdTypes.Add(newRatesReportAdType);
                                isAdTypeOpen = false;
                            }

                            newRatesReportAdType = new RatesReportAdType
                            {
                                ParentAdType = reportItem.ParentAdType,
                                ParentAdTypeID = Convert.ToInt32(reportItem.ParentAdTypeID),
                                EditionType = reportItem.EditionType,
                                EditionTypeID = Convert.ToInt32(reportItem.EditionTypeID),
                                AdType = reportItem.AdType,
                                AdTypeID = Convert.ToInt32(reportItem.AdTypeID),
                            };

                            if (!String.IsNullOrEmpty(reportItem.PublisherAgreement))
                            {
                                newRatesReportAdType.PublisherAgreement = reportItem.PublisherAgreement;
                            }
                            isAdTypeOpen = true;
                        }

                        //Tiers
                        if (lTierID != reportItem.TierID)
                        {
                            lTierID = Convert.ToInt32(reportItem.TierID);

                            if (isTierOpen)
                            {
                                newRatesReportAdType.Tiers.Add(newRatesReportTier);
                                isTierOpen = false;
                            }

                            newRatesReportTier = new RatesReportTier
                            {
                                TierID = Convert.ToInt32(reportItem.TierID),
                                TierRange = reportItem.TierRange
                            };

                            if (reportItem.Rate != null)
                            {
                                newRatesReportTier.Rate = Convert.ToDecimal(reportItem.Rate);
                            }
                            isTierOpen = true;
                        }

                        if (recordCount == recordTotal)
                        {
                            newRatesReportAdType.Tiers.Add(newRatesReportTier);
                            newRatesReportRateType.AdTypes.Add(newRatesReportAdType);
                            newRatesReportOutput.RateTypes.Add(newRatesReportRateType);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = string.Format("Error in Repository GetRatesReport method.  Publisher: {0}, Book: {1}, BookID: {2}.", lCookieData.Publisher, lCookieData.Book, lCookieData.BookID);
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                throw new Exception(lReturnMessage);
            }

            if (newRatesReportOutput.RateTypes.Any())
            {
                lResult = newRatesReportOutput;
            }

            return lResult;
        }
    }
}