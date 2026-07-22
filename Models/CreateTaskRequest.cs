using System.ComponentModel.DataAnnotations;

namespace K5.Assessment.Starter.Models;

// With [ApiController], these attributes are evaluated before the action runs
// and a failure short-circuits into an automatic 400 response.
//
// On a positional record the attribute must stay on the CONSTRUCTOR PARAMETER
// (no `[property:]` target). MVC throws InvalidOperationException at request
// time if it finds validation metadata on the generated property instead.
public record CreateTaskRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
    string Title = "",

    [RegularExpression("^(Low|Normal|High)$", ErrorMessage = "Priority must be Low, Normal, or High.")]
    string Priority = "Normal");
