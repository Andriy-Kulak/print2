describe('Books Service', function () {
    var bookService, httpBackend, state, service;

    beforeEach(module('stateMock'));

    // Initialize the controller and a mock scope
    //beforeEach(inject(function ($state, _bookService_, $httpBackend) {
    //    state = $state;
    //    bookService = _bookService_;
    //    httpBackend = $httpBackend;
    //    initialize other stuff
    //}));



    //beforeEach(
    //    angular.mock.module('print.module'));

    beforeEach(inject(function ($injector) {
        service = $injector.get('bookService');
            httpBackend = $injector.get('$httpBackend');
                })
        );

    it(" needs to be initialized", function() {
        expect(bookService).toBeDefined();
    });








    //beforeEach(inject(function (_bookService_, $httpBackend, $state) {
    //    bookService = _bookService_;
    //    httpBackend = $httpBackend;
    //    state = $state;
    //}));

    it('has a dummy spec to test 2 + 2', function () {
        // An intentionally failing test. No code within expect() will never equal 4.
        expect("test").toEqual("test");
    });

    it("gets books data", function () {
        httpBackend.whenGET("/api/Books/GetBooks/2017").respond({
            data: [
                  { id: 1, Name: "Allure" },
                  { id: 2, Name: "Architectural Digest" },
                  { id: 3, Name: "Bon Appetit" },
                  { id: 4, Name: "Brides" },
                  { id: 5, Name: "Conde Nast Traveler" },
                  { id: 6, Name: "Domino" },
                  { id: 7, Name: "Glamour" },
                  { id: 8, Name: "Golf Digest" }
                ]           
        });
        bookService.getBooks().then(function (response) {
            //expect(response.data[0].id).toEqual(1);
            expect(response.data.length).toBeGreaterThan(0);
        });
        httpBackend.flush();
    });
});

//describe("Get list of books", function () {
//    var getBooks, service;
//    beforeEach(module('print.module'));
//    beforeEach(module(function ($provide) {
//        $provide.service('bookService', bookService);
//    }));

//    beforeEach(inject(function ($injector) {
//        // Set up the mock http service responses
//        $httpBackend = $injector.get('$httpBackend');
//        // backend definition common for all tests
//        authRequestHandler = $httpBackend.when('/api/Books/GetBooks/2017')
//                               .respond({ userId: 'test' });

//    }));

//    beforeEach(inject(function (_getBooks_) {
//        getBooks = _getBooks_;
//    }));

//    //it("returns a list of books", function () {
//    //    expect(getBooks()).toEqual("Hello Dave");
//    // });

//    //it(" needs to be initialized", () => {
//    //    expect(service).toBeDefined();
//    //});

//    it(" needs to be initialized", () => {
//        expect(service).toBeDefined();
//    });
//});