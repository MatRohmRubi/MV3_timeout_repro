using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NativeMessagingHost.Pages
{
  public class IndexModel : PageModel
  {
    public class PostModel
    {
      [Required]
      public string Action { get; set; }
    }

    private readonly IMessagePublisher _messagePublisher;

    public IndexModel (IMessagePublisher messagePublisher)
    {
      _messagePublisher = messagePublisher;
    }

    public IActionResult OnGet()
    {
      return Page();
    }

    public IActionResult OnPost ([FromForm] PostModel model)
    {
      if (!ModelState.IsValid)
        return Page();

      var message = JsonDocument.Parse ("34");
      _messagePublisher.PublishMessage (message);

      return RedirectToPage ("./Index");
    }
  }
}
