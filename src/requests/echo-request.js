"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const http_status_codes_1 = require("http-status-codes");
/**
 * Returns the body to the client.
 * @param request the client request
 * @param response the server response
 */
exports.getEcho = (request, response) => {
    if (!request.params.message) {
        response.sendStatus(http_status_codes_1.BAD_REQUEST);
    }
    else {
        response.send(request.params.message);
    }
};
