using loreal_print.MEC_Common.Security;
using loreal_print.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using loreal_print.Models.Tablet;
using NLog;
using System.Web;

namespace loreal_print.Repository
{
    public class Tablet
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<TabletModel> GetTabletRates()
        {
            var lResult = new List<TabletModel>();
            var lLogMsg = string.Empty;
            var lTableRatesOutput = new List<TabletModel>();
            var pContext = System.Web.HttpContext.Current;

            // Get BookID, VersionID & Year from Cookie
            var lCookieData = CookieManagement.GetCookie(pContext);

            try
            {
                using (Loreal_DEVEntities6 db = new Loreal_DEVEntities6())
                {
                    var lTabletRates = db.GetTabletRates(lCookieData.Year, lCookieData.BookID, lCookieData.VersionID)
                    .ToList();

                    if (lTabletRates.Any())
                    {
                        foreach (var tabletRate in lTabletRates)
                        {
                            //recordCount++;

                            var newTableRateRecord = new TabletModel
                            {
                                TabletFunctionalityID = tabletRate.TabletFunctionalityID,
                                TabletParentFunctionality = tabletRate.TabletParentFunctionality,
                                TabletFunctionality = tabletRate.TabletFunctionality
                            };
                            if (tabletRate.EarnedRate != null)
                                newTableRateRecord.EarnedRate = Convert.ToInt32(tabletRate.EarnedRate);
                            if (tabletRate.OpenRate != null)
                                newTableRateRecord.OpenRate = Convert.ToInt32(tabletRate.OpenRate);

                            lTableRatesOutput.Add(newTableRateRecord);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var lReturnMessage = String.Format("Error in Repository GetTabletRates method.");
                lLogMsg = String.Format(lReturnMessage + "ERROR MESSAGE: {0}.  SOURCE: {1}. STACKTRACE: {2}.", ex.Message, ex.Source, ex.StackTrace);
                logger.Error(lLogMsg);
                throw new Exception(lReturnMessage);
            }
            if (lTableRatesOutput.Any())
            {
                lResult = lTableRatesOutput;
            }

            return lResult;
        }
    }
}