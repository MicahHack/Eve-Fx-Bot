# EVE-FX-BOT
Eve is meant to be a Forex Trading Bot that runs on a linux server.
It uses bollinger bands that sell when the market reaches out the top of the bands and buys when the market reaches outside the bottom.


## Getting Started

- CONFIGURE FX ACCOUNT TO BUY/SELL
1. Visit "https://metaapi.cloud/" and create an account.
2. After creating an account remain on the "MT Accounts" page and select "Add New Account" (You will need to have a metatrader 4 or 5 account with the ID and password to it).
3. Once the new account has been added, generate an API key for the account and enter the API key into the local HttpService.cs file in the "token" variable.
4. Take the account ID and add it to the ForexService file under the "accountId" variable.

- RUNNING AS A SERVICE
Eve was built to run automatically and make buy/sells in the background as a Linux service.
Simply create a service for it on your linux device which points towards the build files.

## Reporting
If any errors are encountered, please reach out to Micah.Hack.18@gmail.com or whatsapp me on +27722490648