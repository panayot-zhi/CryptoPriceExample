# CryptoPriceExample

Prerequisites:
- docker command
- nothing on port 3306
- nothing on port 5253

This project has been built with Microsoft Visual Studio Community 2022 (64-bit)  
Version 17.3.0

It should suffice to run `docker compose up` on the root directory.  
The application exposes endpoints on http://localhost:5253/swagger endpoint.  

You can test the console application afterwards by running a  
cmd/powershell from CryptoPriceExample.Console\publish  
and executing the commands  
`24h {symbol}`  
or  
`sma {symbol} {n} {p} {s}`  

You can also build and run the project yourself  from Visual Studio and it's configured profiles.