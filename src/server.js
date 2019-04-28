"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const express = require("express");
const echo_request_1 = require("./requests/echo-request");
//
// Variables
//
const port = 8080;
const app = express();
//
// Requests
//
app.get('/api/echo/:message', echo_request_1.getEcho);
//
// Start the server
//
app.listen(port, () => console.log(`Server listening on port ${port}.`));
