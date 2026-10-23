using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DSAS.Web.Models;

public class StaffDto
{
    [JsonPropertyName("staff_id")]
    public string StaffId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

// Embedded via PostgREST resource embedding (registrations -> students
// through the student_id foreign key). The ERD keeps full_name/
// student_number/programme on Student only, not duplicated onto
// Registration, so every screen that needs them reads through here.
public class StudentDto
{
    [JsonPropertyName("student_id")]
    public string StudentId { get; set; } = string.Empty;

    [JsonPropertyName("student_number")]
    public string StudentNumber { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("programme")]
    public string? Programme { get; set; }
}

public class RegistrationDto
{
    [JsonPropertyName("registration_id")]
    public string RegistrationId { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime? SubmittedDate { get; set; }

    [JsonPropertyName("student_id")]
    public string StudentId { get; set; } = string.Empty;

    [JsonPropertyName("staff_id")]
    public string? StaffId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Pending";

    [JsonPropertyName("upload_photo")]
    public string? UploadPhoto { get; set; }

    [JsonPropertyName("upload_proof_of_registration")]
    public string? UploadProofOfRegistration { get; set; }

    [JsonPropertyName("rejection_reason")]
    public string? RejectionReason { get; set; }

    [JsonPropertyName("students")]
    public StudentDto? Student { get; set; }
}

public class RegistrationUpdateDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("rejection_reason")]
    public string? RejectionReason { get; set; }

    [JsonPropertyName("staff_id")]
    public string? StaffId { get; set; }
}

public class DashboardViewModel
{
    public int Submitted { get; set; }
    public int Pending { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public List<RegistrationDto> Registrations { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
}

public class RegistrationDetailViewModel
{
    public RegistrationDto Registration { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class RejectViewModel
{
    [Required(ErrorMessage = "Please provide a reason for rejection.")]
    public string Reason { get; set; } = string.Empty;
}