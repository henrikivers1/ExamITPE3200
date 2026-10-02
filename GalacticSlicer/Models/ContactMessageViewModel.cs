using System.ComponentModel.DataAnnotations;

namespace GalacticSlicer.Models;

// Simple view-model for the Contact form.
// Messages are only logged for now (no database table yet)

public class ContactMessageViewModel
{
    [Required(ErrorMessage = "Please enter a title.")]
    [StringLength(100, ErrorMessage = "Title can be at most 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a description.")]
    [StringLength(1000, ErrorMessage = "Description can be at most 1000 characters.")]
    public string Description { get; set; } = string.Empty;
}