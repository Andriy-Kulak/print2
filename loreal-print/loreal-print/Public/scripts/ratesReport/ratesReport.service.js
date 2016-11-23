(function () {
    'use strict';

    angular.module('print.module').service('ratesReportService', ['$http', function ($http) {
        var vm = this;

        vm.getRatesRepData = function () {
            return $http.get("api/RatesReport/GetRatesReport");
        }

        vm.savePubAgreementData = function (data) {
            return $http.post("api/RatesReport/savePublisherAgreements/answers", data);
        }

    }]
)
})();
