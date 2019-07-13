import * as keyvault from 'azure-keyvault'
import * as msRestAzure from 'ms-rest-azure'
import { ServiceClientCredentials } from 'ms-rest';

// MSI: https://azure.microsoft.com/en-in/resources/samples/app-service-msi-keyvault-node/

const clientId = "";
const secret = "";
const domain = "";
function getKeyVaultCredentials() {
    if (process.env.APPSETTING_WEBSITE_SITE_NAME) {
        return msRestAzure.loginWithAppServiceMSI({ resource: 'https://vault.azure.net' });
    } else {
        return msRestAzure.loginWithServicePrincipalSecret(clientId, secret, domain);
    }
}

const KEY_VAULT_URI = "";
function getKeyVaultSecret(credentials: ServiceClientCredentials) {
    let keyVaultClient = new keyvault.KeyVaultClient(credentials);
    return keyVaultClient.getSecret(KEY_VAULT_URI, 'secret', "");
}

getKeyVaultCredentials().then(
    getKeyVaultSecret
).then(function (secret) {
    console.log(`Your secret value is: ${secret.value}.`);
}).catch(function (err) {
    throw (err);
});