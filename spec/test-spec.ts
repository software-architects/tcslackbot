import 'jasmine';

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
    public getEcho(message: string): string {
        return message;
    }
}

describe('Testing server', () => {
    let service: MockedExpressService;
    let echoRequest: MockedEchoRequest;

    beforeEach(() => {
        service = new MockedExpressService();
        echoRequest = new MockedEchoRequest();
    });

    it('should send a mock get request', () => {
        const newRequest: string = service.get('/api/echo/', echoRequest.getEcho('test'));
        expect(newRequest).toBe('/api/echo/test');
    });

    it('should start listening to port 8080', () => {
        const expressListen = service.listen(8080);
        expect(expressListen).toBe('Listening on port 8080');
    });

});

// @ts-ignore
// describe('Failing test', () => {
//     it('should fail', () => {
//         const fal: boolean = false;
//         expect(fal).toBeTruthy();
//     });
// });
