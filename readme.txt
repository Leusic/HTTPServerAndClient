folders:

location - client solution with working executable available in bin/Debug/netcoreapp3.1
client has working whois, HTTP0.9, HTTP1.0, HTTP1.1 requests available through commands -h9, -h0 and -h1 and no argument for whois
-h command for choosing host
-p command for choosing port
-t command for choosing timeout

locationserver - server solution with working executable available in bin/Debug/netcoreapp3.1
able to process all requests from the client and return responses (HTTP0.9,HTTP1.0,HTTP1.1,whois)
multiple threads for handling multiple connections at once

