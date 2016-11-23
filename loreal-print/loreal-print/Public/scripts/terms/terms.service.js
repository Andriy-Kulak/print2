(function () {
    'use strict';

    angular.module('print.module').service('termsService', ['$http', function ($http) {
        var vm = this;

        vm.getTermsData = function () {
            return $http.get("/api/Terms");
        }

        vm.getTermsAnswers = function () {
            return $http.get("api/Terms/GetAnswers");
        }

        vm.saveTermsAnswers = function (answers) {
            return $http.post("/api/terms/saveAnswers/answers", answers);
        }
    }]
)
})();
