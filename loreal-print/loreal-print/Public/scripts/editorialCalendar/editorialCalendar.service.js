(function () {
    'use strict';

    angular.module('print.module').service('editCalService', ['$http', function ($http) {
        var vm = this;

        // gets editorial calendar row
        vm.getCalData = function () {
            return $http.get("api/EditorialCalendar/GetCalendar");
        }

        // deletes the selected row
        vm.deleteCalData = function (id) {
            return $http.delete('api/EditorialCalendar/' + id);
        }

        // saved or updates editorial calendar data
        vm.saveCalData = function (data) {

            return $http.post('api/EditorialCalendar/saveEditorialCalendar/calendarRecord', data);
        }
       
    }]
)
})();
