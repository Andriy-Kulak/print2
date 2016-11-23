(function () {
    'use strict';

    angular.module('print.module').service('circService', ['$http', function ($http) {
        var vm = this;

        // retrive data
        vm.getCircData = function () {
            return $http.get("api/circulation/GetCirculation");
        }

        // save data
        vm.saveCircData = function (answers) {
            return $http.post('api/Circulation/saveCirculationRecords/CircContent', answers);
        }

    }]
)
})();
