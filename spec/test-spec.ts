import { Handler } from 'express';
import 'jasmine';
// import { getEcho } from './../src/requests/echo-request';

class MockedExpressService {
    public get(s: string, h: any) {
        return s + h;
    }
    public listen(port: number): string {
        return 'Listening on port ' + port;
    }
}

// tslint:disable-next-line:max-classes-per-file
class MockedEchoRequest {
    public getEcho(): string {
        const s = 'test';
        return s;
    }
}

describe('Testing server', () => {
    let service: MockedExpressService;
    let echoRequest: MockedEchoRequest;

    beforeEach(() => {
        service = new MockedExpressService();
        echoRequest = new MockedEchoRequest();
    });

    it('should call a mock get request', () => {
        const newRequest: string = service.get('/api/echo/', echoRequest.getEcho());
        expect(newRequest).toBe('/api/echo/test');
    });
    it('should start listen to a port', () => {
        const expressListen = service.listen(8080);
        expect(expressListen).toBe('Listening on port 8080');
    });

});
