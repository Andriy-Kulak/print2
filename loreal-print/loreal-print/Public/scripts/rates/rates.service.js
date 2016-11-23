(function () {
    'use strict';

    angular.module('print.module').service('ratesService', ['$http', function ($http) {
        var vm = this;

         vm.getRatesDataService = function() {
            return $http.get("api/Rates/GetRates");
         }

         vm.saveRatesDataService = function (data) {
             return $http.post("/api/Rates/saveRates/RatesModel", data);
         }

         vm.openBleedFormula = function (netP4CBopenRate, netP4CopenRate, bleedValue) {
             if (netP4CBopenRate && netP4CopenRate) {
                 bleedValue = ((netP4CBopenRate - netP4CopenRate) / netP4CopenRate * 100).toFixed(2);
             }
         }

    }]
)
})();
