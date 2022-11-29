using System.Text;
using Bot.Models;
using Bot.Services;

namespace Web.Services;

public class TemplateGenerator
{
    private readonly QuoteService _quoteService;

    public TemplateGenerator(QuoteService quoteService) =>
        _quoteService = quoteService;
    
    public async Task<string> GetHTMLString()
    {
        var quotes = await _quoteService.GetAsync();

        var sb = new StringBuilder();
        sb.Append(@"
                    <html>
                        <head>
                        </head>
                        <body>
                            <div class='header'><h1>This is the generated PDF report!!!</h1></div>
                            <table align='center'>
                                <tr>
                                    <th>Author</th>
                                    <th>Quote</th>
                                </tr>");
        
        foreach (Quote quote in quotes)
        {
            sb.AppendFormat(@"  <tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                </tr>", quote.author, string.Join(" ", quote.quote));
        }

        sb.Append(@"
                            </table>
                        </body>
                    </html>");

        return sb.ToString();
    }
}