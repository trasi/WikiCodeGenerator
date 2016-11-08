Af einhverjum ástæðum er ekki að smíða hefðbundið Service Reference fyrir Confluence vefþjónustu (https://wiki.rb.is/rpc/soap-axis/confluenceservice-v2?wsdl). 

Þess vegna þarf að fara eftirfarandi bakdyraleið (sjá: http://stackoverflow.com/questions/853896/implement-a-c-sharp-client-that-uses-webservices-over-ssl):

1. Smíða DLL-skrá úr VS Command Prompt: 

  > wsdl /l:CS /protocol:SOAP /namespace:RB.Utils.WikiCodeGenerator.ConfluenceService confluenceservice-v2.wsdl

  > csc /t:library /r:System.Web.Services.dll /r:System.Xml.dll ConfluenceSoapServiceService.cs

2. Bæta svo vísun í ConfluenceSoapServiceService.dll í viðkomandi VS project-i.