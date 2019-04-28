const app = require('../../src/server.ts');
const test = require('supertest');

describe("ServerTestSuite", function () {
    var a = true;
    it("Send a get and get a json back", function () {
        expect(a).toBe(true);
        test(app)
            .get('/api/echo/:message')
            .expect(200)
            .expect('Content-Type', 'application/json; charset=utf-8')
            .end((error) => (error) ? done.fail(error) : done());


    });
});