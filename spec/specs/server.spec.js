"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (Object.hasOwnProperty.call(mod, k)) result[k] = mod[k];
    result["default"] = mod;
    return result;
};
Object.defineProperty(exports, "__esModule", { value: true });
const supertest_1 = __importDefault(require("supertest"));
const app = __importStar(require("../../src/server"));
describe('ServerTestSuite', () => {
    const a = true;
    it('Send a get and get a json back', () => {
        expect(a).toBe(true);
        supertest_1.default(app)
            .get('/api/echo/:message')
            .expect(200)
            .expect('Content-Type', 'application/json; charset=utf-8');
        // .end((error) => (error) ? done.fail(error) : done());
    });
});
