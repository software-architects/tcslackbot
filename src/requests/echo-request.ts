import { Request, Response } from 'express';
import { BAD_REQUEST } from 'http-status-codes';

/**
 * Returns the body to the client.
 * @param request the client request
 * @param response the server response
 */
export const getEcho = (request: Request, response: Response) => {
    if (!request.params.message) {
        response.sendStatus(BAD_REQUEST);
    } else {
        response.send(request.params.message);
    }
};
