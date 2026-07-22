using System.ComponentModel.DataAnnotations;

namespace K5.Assessment.Starter.Models;

// See CreateTaskRequest: on positional records the validation attributes must
// target the constructor parameter, not the generated property.
public record UpdateTaskRequest(
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title must be 200 characters or fewer.")]
    string Title = "",

    [RegularExpression("^(Low|Normal|High)$", ErrorMessage = "Priority must be Low, Normal, or High.")]
    string Priority = "Normal");
