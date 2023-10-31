using System.Net;
using HtmlAgilityPack;
using RestSharp;
using System.Text;

List<string> urls = new List<string>();

string fileName = "urls.txt";
int BufferSize = 128;

//Open txt file with  team IDs and parse it line by line
using (var fileStream = File.OpenRead(fileName))
using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
{
    String line;

    //Process line by line from URLs for Scraping
    while ((line = streamReader.ReadLine()) != null)
    {
        urls.Add(line);
    }
}


var options = new RestClientOptions() {
    Proxy = new WebProxy()
    {
        Address = new Uri("http://proxy.zenrows.com:8001"),
        Credentials = new NetworkCredential("", "")
    },
    RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
};

foreach(string url in urls){
    
    List<string> prices = new List<string>();

    var client = new RestClient(options);
    var request = new RestRequest(url);

    var response = client.Get(request);
        
    var html = (response.Content);


    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(html);


    HtmlNodeCollection divs = htmlDocument.DocumentNode.SelectNodes("//div[@class='s-item__wrapper clearfix']");


    //Iterate through the selected divs
    foreach (HtmlNode div in divs)
    {

        //Removing HTML Comments
        var nodes = div.SelectNodes("//comment()");
        if (nodes != null)
        {
            foreach (HtmlNode comment in nodes)
            {
                if (!comment.InnerText.StartsWith("DOCTYPE"))
                    comment.ParentNode.RemoveChild(comment);
            }
        }

        //Selecting Price Nodes
        HtmlNode span_price = div.SelectSingleNode(".//span[@class='s-item__price']");
        string price = span_price.InnerText;

        //Selecting Title Nodes
        HtmlNode div_title = div.SelectSingleNode(".//div[@class='s-item__title']");
        string title = div_title.InnerText;
        

        //Ignore Junk Listings, This can be refined
        if(title.ToLower().Contains("overlay") == true || title.ToLower().Contains("support") == true || title.ToLower().Contains("trim") == true || title.ToLower().Contains("insert") == true || title.ToLower().Contains("badge") || title.ToLower().Contains("shop on ebay")){

        }
        else{
            prices.Add(price);
        }

    }

    double avg_price = 0;

    foreach (string value in prices){
        if(value.Substring(0,1) == "$"){
            avg_price += double.Parse(value.Substring(1,value.Length-1));
        }
        else{
            Console.WriteLine(value);
        }
    }

    avg_price /= prices.Count();
 
    Console.WriteLine("Scraping: " + url);
    Console.WriteLine("Average Price of Item: $" + Math.Round(avg_price,2));
    Console.WriteLine("# of Items: " + prices.Count());
}






