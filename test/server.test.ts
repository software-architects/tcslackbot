import request from 'supertest';
import * as app from '../src/server';

describe('TestCase1', () => {
    it('simple 1+1', () => {
        expect((1 + 1) === 2);
    });
    it('server get', async () => {
        const result = await request(app).get('/api/echo/:message');
        expect(result.text).toEqual('test');
    });
});
