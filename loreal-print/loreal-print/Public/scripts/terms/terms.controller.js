(function () {
    'use strict';

    angular.module('print.module')
    .controller('termsCtrl', ['$http', 'toaster', 'modalService', 'termsService', function ($http, toaster, modalService, termsService) {
        var vm = this;

        //Stores Top Level Answers to Questions
        vm.termAnswers;

        // Controller Functions
        vm.getQuestionsList = function() {
            termsService.getTermsData().then(function (result) {
                vm.terms = result.data;
                           
            }, function (error) {
                modalService.saveErrorModal(error.data.Message);
            });
        }
        vm.getQuestionsList();

        vm.getAnswersList = function() {
            termsService.getTermsAnswers().then(function (result) {

                var getAnsIndex = result.data;
                vm.termYesNoAns = [];
                vm.termFreeFormAns = [];

                for (var i = 0; i < Object.keys(getAnsIndex).length; i++) {

                    vm.termYesNoAns[getAnsIndex[i].QuestionID] = getAnsIndex[i].AnswerYesNo;
                    vm.termFreeFormAns[getAnsIndex[i].QuestionID] = getAnsIndex[i].AnswerFreeForm;
                }
            }, function (error) {
                console.log("no answers");
            });
        }
        vm.getAnswersList();

        //method for saving answers      
        vm.saveAnswers = function() {
            //combines the index of YesNo and FreeForm Answers
            if (!vm.termYesNoAns) { vm.termYesNoAns = []; }
            if (!vm.termFreeFormAns) { vm.termFreeFormAns = []; }

            vm.isTimingOut = true;

            var AnsIndex = Object.keys(vm.termYesNoAns).concat(Object.keys(vm.termFreeFormAns));
            //creates a unique index AnsIndex
            var uniqueAnsIndex = AnsIndex.filter(function (item, i, ar) { return ar.indexOf(item) === i; })

            //termAnswers is where all the answer objects will be stored for QuestionID, AnswerFreeForm, AnswerYesNo
            vm.termAnswers = [];
            //loop that pushes all the answers into termAnswers
            for (var i = 0; i < uniqueAnsIndex.length; i++) {
                vm.termAnswers.push({
                    QuestionID: uniqueAnsIndex[i],
                    AnswerYesNo: vm.termYesNoAns[uniqueAnsIndex[i]],
                    AnswerFreeForm: vm.termFreeFormAns[uniqueAnsIndex[i]]
                });
            }
          
            termsService.saveTermsAnswers(vm.termAnswers).then(function (result) {
                vm.answers = result.data;
                modalService.saveSuccessModal();
                vm.isTimingOut = false;


            }, function (error) {
                handleException(error);
                modalService.saveErrorModal();
                vm.isTimingOut = false;
            });
        }
      
    }]
)})();
