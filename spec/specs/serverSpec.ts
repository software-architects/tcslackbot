import test from 'supertest';
import * as app from '../../src/server';

describe('ServerTestSuite', () => {
    const a = true;
    it('Send a get and get a json back', () => {
        expect(a).toBe(true);
        test(app)
            .get('/api/echo/:message')
            .expect(200)
            .expect('Content-Type', 'application/json; charset=utf-8');
        // .end((error) => (error) ? done.fail(error) : done());

    });
});
