(function () {
    'use strict';

    angular.module('print.module').service('bookService', ['$http', function ($http) {
        var vm = this;

        vm.getBooks = function () {
            return $http.get("/api/Books/GetBooks/2017");
        }

        vm.selectedBook = function (bookId) {
            return $http.put('api/books/' + bookId);
        }

    }]
)
})();
