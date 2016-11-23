(function () {
    'use strict';

    angular.module('print.module')
    .controller('booksCtrl', ['$http', '$state', 'modalService', 'bookService',
        function booksCtrl($http, $state, modalService, bookService) {

        var vm = this;
        vm.books = []; //array of book objects

        vm.selectedBookOptions = function () {
            
            if (vm.booksForm.$valid) {
                bookService.selectedBook(vm.selectedBook.ID)
                .then(function (result) {                  
                    // Redirect to Terms tab
                     $state.go('print.terms');
                }, function (err) {
                    console.log(err);
                    modalService.saveErrorModal(err);
                }
                );
            }
        }        

        function getBooksList() {
            bookService.getBooks().then(function (result) {
                vm.books = result.data;
                console.log(vm.books);
            }, function (error) {
                handleException(error);
            });
        }
        getBooksList();
    }]
)})();
