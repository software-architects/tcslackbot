import express = require('express');
import { getEcho } from './requests/echo-request';

//
// Variables
//
const port = 8080;
const app = express();

//
// Requests
//
app.get('/api/echo/:message', getEcho);

//
// Start the server
//
app.listen(port, () => console.log(`Server listening on port ${port}.`));
